using Npgsql;

var connectionString = Environment.GetEnvironmentVariable("SOLARSCOPE_DB_CONNECTION");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("Environment variable SOLARSCOPE_DB_CONNECTION belum diisi.");
    return 1;
}

try
{
    await using var dataSource = NpgsqlDataSource.Create(connectionString);
    await using var command = dataSource.CreateCommand("""
        SELECT
            (SELECT COUNT(*) FROM public.simulations),
            (SELECT COUNT(*) FROM public.monthly_productions);
        """);

    await using var reader = await command.ExecuteReaderAsync();
    await reader.ReadAsync();

    Console.WriteLine("Koneksi Supabase berhasil.");
    Console.WriteLine($"simulations: {reader.GetInt64(0)} baris");
    Console.WriteLine($"monthly_productions: {reader.GetInt64(1)} baris");

    return 0;
}
catch (PostgresException ex)
{
    Console.Error.WriteLine($"Database menolak query. SQLSTATE: {ex.SqlState}");
    return 1;
}
catch (NpgsqlException ex)
{
    for (Exception? error = ex; error != null; error = error.InnerException)
    {
        Console.Error.WriteLine($"{error.GetType().Name}: {error.Message}");
    }

    return 1;
}