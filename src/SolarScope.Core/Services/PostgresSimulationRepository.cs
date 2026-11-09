using Npgsql;
using SolarScope.Core.Models;

namespace SolarScope.Core.Services;

/// <summary>
/// Menyimpan dan membaca riwayat simulasi di PostgreSQL (Supabase) melalui Npgsql.
/// Connection string diterima dari luar (environment variable SOLARSCOPE_DB_CONNECTION), tidak ditulis di kode.
/// </summary>
public sealed class PostgresSimulationRepository : IAsyncDisposable
{
    public const string ConnectionEnvironmentVariable = "SOLARSCOPE_DB_CONNECTION";

    private readonly NpgsqlDataSource _dataSource;

    public PostgresSimulationRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string database belum diisi", nameof(connectionString));
        }
        _dataSource = NpgsqlDataSource.Create(connectionString);
    }

    public static PostgresSimulationRepository FromEnvironment() =>
        new(Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable) ?? string.Empty);

    public async Task<ServiceResult<int>> InsertSimulationAsync(Simulation simulation)
    {
        var check = simulation.Validate();
        if (!check.IsValid) return ServiceResult<int>.Fail(check.err!);
        if (simulation.Summary is not { MonthlyProductions.Count: 12 } summary)
        {
            return ServiceResult<int>.Fail("Simulasi harus memiliki hasil estimasi 12 bulan sebelum disimpan");
        }

        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            await using var insertSimulation = new NpgsqlCommand("""
                INSERT INTO simulations (scenario_name, latitude, longitude, location_name, system_capacity_kwp,
                    performance_ratio, annual_average_radiation, total_annual_energy_kwh, peak_production_month, lowest_production_month)
                VALUES (@name, @lat, @lon, @location, @kwp, @pr, @radiation, @total, @peak, @lowest)
                RETURNING simulation_id, created_at
                """, connection, transaction);
            insertSimulation.Parameters.AddWithValue("name", simulation.ScenarioName.Trim());
            insertSimulation.Parameters.AddWithValue("lat", simulation.Location.Latitude);
            insertSimulation.Parameters.AddWithValue("lon", simulation.Location.Longitude);
            insertSimulation.Parameters.AddWithValue("location", string.IsNullOrWhiteSpace(simulation.Location.LocationName) ? DBNull.Value : simulation.Location.LocationName);
            insertSimulation.Parameters.AddWithValue("kwp", simulation.SystemParameter.SystemCapacityKwp);
            insertSimulation.Parameters.AddWithValue("pr", simulation.SystemParameter.PerformanceRatio);
            insertSimulation.Parameters.AddWithValue("radiation", summary.MonthlyProductions.Average(m => m.SolarRadiationDailyAverage));
            insertSimulation.Parameters.AddWithValue("total", summary.TotalAnnualEnergyKwh);
            insertSimulation.Parameters.AddWithValue("peak", summary.PeakProductionMonth);
            insertSimulation.Parameters.AddWithValue("lowest", summary.LowestProductionMonth);

            int id;
            DateTime createdAt;
            await using (var reader = await insertSimulation.ExecuteReaderAsync())
            {
                await reader.ReadAsync();
                id = reader.GetInt32(0);
                createdAt = reader.GetDateTime(1);
            }

            var batch = new NpgsqlBatch(connection, transaction);
            foreach (var month in summary.MonthlyProductions)
            {
                var command = new NpgsqlBatchCommand("""
                    INSERT INTO monthly_productions (simulation_id, month_index, month_name, solar_radiation_daily_average, estimated_energy_kwh)
                    VALUES ($1, $2, $3, $4, $5)
                    """);
                command.Parameters.Add(new() { Value = id });
                command.Parameters.Add(new() { Value = month.MonthIndex });
                command.Parameters.Add(new() { Value = month.MonthName });
                command.Parameters.Add(new() { Value = month.SolarRadiationDailyAverage });
                command.Parameters.Add(new() { Value = month.EstimatedEnergyKwh });
                batch.BatchCommands.Add(command);
            }
            await using (batch) await batch.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            simulation.SimulationId = id;
            simulation.CreatedAt = createdAt;
            return ServiceResult<int>.Ok(id);
        }
        catch (NpgsqlException ex)
        {
            return ServiceResult<int>.Fail(DatabaseError(ex));
        }
    }

    /// <summary>Daftar riwayat (terbaru dulu) tanpa rincian bulanan, untuk tabel Riwayat.</summary>
    public async Task<ServiceResult<List<Simulation>>> GetAllSimulationsAsync()
    {
        try
        {
            await using var command = _dataSource.CreateCommand(SelectSimulation + " ORDER BY created_at DESC, simulation_id DESC");
            await using var reader = await command.ExecuteReaderAsync();
            var simulations = new List<Simulation>();
            while (await reader.ReadAsync()) simulations.Add(ReadSimulation(reader));
            return ServiceResult<List<Simulation>>.Ok(simulations);
        }
        catch (NpgsqlException ex)
        {
            return ServiceResult<List<Simulation>>.Fail(DatabaseError(ex));
        }
    }

    /// <summary>Detail satu simulasi beserta 12 rincian bulanan.</summary>
    public async Task<ServiceResult<Simulation>> GetSimulationByIdAsync(int id)
    {
        try
        {
            Simulation simulation;
            await using (var command = _dataSource.CreateCommand(SelectSimulation + " WHERE simulation_id = $1"))
            {
                command.Parameters.Add(new() { Value = id });
                await using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync()) return ServiceResult<Simulation>.Fail($"Simulasi dengan ID {id} tidak ditemukan");
                simulation = ReadSimulation(reader);
            }

            await using (var command = _dataSource.CreateCommand("""
                SELECT month_index, month_name, solar_radiation_daily_average, estimated_energy_kwh
                FROM monthly_productions WHERE simulation_id = $1 ORDER BY month_index
                """))
            {
                command.Parameters.Add(new() { Value = id });
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    simulation.Summary!.MonthlyProductions.Add(new MonthlyProduction
                    {
                        MonthIndex = reader.GetInt32(0),
                        MonthName = reader.GetString(1),
                        SolarRadiationDailyAverage = reader.GetDouble(2),
                        EstimatedEnergyKwh = reader.GetDouble(3)
                    });
                }
            }
            return ServiceResult<Simulation>.Ok(simulation);
        }
        catch (NpgsqlException ex)
        {
            return ServiceResult<Simulation>.Fail(DatabaseError(ex));
        }
    }

    /// <summary>Menghapus simulasi; rincian bulanan ikut terhapus lewat ON DELETE CASCADE.</summary>
    public async Task<ServiceResult<bool>> DeleteSimulationAsync(int id)
    {
        try
        {
            await using var command = _dataSource.CreateCommand("DELETE FROM simulations WHERE simulation_id = $1");
            command.Parameters.Add(new() { Value = id });
            return await command.ExecuteNonQueryAsync() == 1
                ? ServiceResult<bool>.Ok(true)
                : ServiceResult<bool>.Fail($"Simulasi dengan ID {id} tidak ditemukan");
        }
        catch (NpgsqlException ex)
        {
            return ServiceResult<bool>.Fail(DatabaseError(ex));
        }
    }

    public ValueTask DisposeAsync() => _dataSource.DisposeAsync();

    private const string SelectSimulation = """
        SELECT simulation_id, scenario_name, created_at, latitude, longitude, location_name, system_capacity_kwp,
            performance_ratio, total_annual_energy_kwh, peak_production_month, lowest_production_month
        FROM simulations
        """;

    private static Simulation ReadSimulation(NpgsqlDataReader reader) => new()
    {
        SimulationId = reader.GetInt32(0),
        ScenarioName = reader.GetString(1),
        CreatedAt = reader.GetDateTime(2),
        Location = new Location
        {
            Latitude = reader.GetDouble(3),
            Longitude = reader.GetDouble(4),
            LocationName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
        },
        SystemParameter = new SystemParameter
        {
            SystemCapacityKwp = reader.GetDouble(6),
            PerformanceRatio = reader.GetDouble(7)
        },
        Summary = new SimulationSummary
        {
            TotalAnnualEnergyKwh = reader.GetDouble(8),
            PeakProductionMonth = reader.GetString(9),
            LowestProductionMonth = reader.GetString(10)
        }
    };

    private static string DatabaseError(NpgsqlException ex) => ex is PostgresException pg
        ? $"Database menolak operasi ({pg.SqlState}): {pg.MessageText}"
        : "Tidak dapat terhubung ke database. Periksa koneksi internet dan status project Supabase.";
}
