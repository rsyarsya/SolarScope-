using SolarScope.Core.Models;
using SolarScope.Core.Services;

internal static class Program
{
    private static int checks;

    private static void Check(bool value, string message)
    {
        if (!value) throw new Exception("FAIL " + message);
        Console.WriteLine("PASS " + message);
        checks++;
    }

    private static async Task<int> Main()
    {
        var parameter = new SystemParameter { SystemCapacityKwp = 2, PerformanceRatio = 0.75 };
        var flatRadiation = Enumerable.Repeat(5.0, 12).ToArray();

        var climatology = new ClimatologyEnergyCalculator().Calculate(parameter, flatRadiation);
        Check(climatology.IsSuccess && climatology.Data!.MonthlyProductions.Count == 12, "Climatology returns 12 months");
        Check(climatology.Data!.MonthlyProductions[0].EstimatedEnergyKwh == 232.5, "January = 2 kWp x 5 x 31 x 0.75 = 232.5 kWh");
        Check(climatology.Data.MonthlyProductions[1].EstimatedEnergyKwh == 210, "February uses 28 days in climatology");
        Check(climatology.Data.TotalAnnualEnergyKwh == 2737.5, "Annual total = 2 x 5 x 365 x 0.75 = 2737.5 kWh");
        Check(climatology.Data.MonthlyProductions[0].MonthName == "Januari", "Month name is Indonesian");

        // Polymorphism: same call through base type, behavior depends on runtime subclass.
        EnergyCalculator[] calculators = { new ClimatologyEnergyCalculator(), new YearlyEnergyCalculator(2024) };
        var februaries = calculators.Select(c => c.Calculate(parameter, flatRadiation).Data!.MonthlyProductions[1].EstimatedEnergyKwh).ToArray();
        Check(februaries[0] == 210 && februaries[1] == 217.5, "February differs by subclass (28 vs 29 days in 2024)");
        Check(new YearlyEnergyCalculator(2024).Calculate(parameter, flatRadiation).Data!.TotalAnnualEnergyKwh == 2745, "Leap year total uses 366 days");

        double[] radiation = { 4.8, 4.9, 5.1, 5.0, 4.7, 4.5, 4.9, 5.4, 5.8, 5.6, 5.0, 4.6 };
        var varied = new ClimatologyEnergyCalculator().Calculate(parameter, radiation).Data!;
        Check(varied.PeakProductionMonth == "September" && varied.LowestProductionMonth == "Juni", "Peak and lowest months follow estimated energy");

        Check(!new ClimatologyEnergyCalculator().Calculate(parameter, new double[11]).IsSuccess, "Rejects radiation data without 12 months");
        Check(!new ClimatologyEnergyCalculator().Calculate(parameter, flatRadiation.Select((v, i) => i == 3 ? -999 : v).ToArray()).IsSuccess, "Rejects negative (NASA fill value -999) radiation");
        Check(!new ClimatologyEnergyCalculator().Calculate(new SystemParameter { SystemCapacityKwp = 0 }, flatRadiation).IsSuccess, "Rejects zero capacity");

        bool threw = false;
        try { _ = new YearlyEnergyCalculator(0); } catch (ArgumentOutOfRangeException) { threw = true; }
        Check(threw, "Rejects invalid year");

        await VerifyDatabaseAsync(parameter, radiation);

        Console.WriteLine($"{checks} checks passed");
        return 0;
    }

    // Round trip ke Supabase: simpan, baca daftar, baca detail, hapus. Data uji dihapus kembali.
    private static async Task VerifyDatabaseAsync(SystemParameter parameter, double[] radiation)
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(PostgresSimulationRepository.ConnectionEnvironmentVariable)))
        {
            Console.WriteLine($"SKIP database checks ({PostgresSimulationRepository.ConnectionEnvironmentVariable} belum diisi)");
            return;
        }

        await using var repository = PostgresSimulationRepository.FromEnvironment();
        var simulation = new Simulation
        {
            ScenarioName = "Verifikasi Otomatis",
            Location = new Location { Latitude = -7.7956, Longitude = 110.3695, LocationName = "Yogyakarta" },
            SystemParameter = parameter,
            Summary = new ClimatologyEnergyCalculator().Calculate(parameter, radiation).Data
        };

        var insert = await repository.InsertSimulationAsync(simulation);
        Check(insert.IsSuccess && insert.Data > 0, insert.IsSuccess ? $"Insert simulation to PostgreSQL (id {insert.Data})" : "Insert failed: " + insert.ErrorMessage);
        int id = insert.Data;
        try
        {
            var all = await repository.GetAllSimulationsAsync();
            Check(all.IsSuccess && all.Data!.Any(s => s.SimulationId == id), "Saved simulation appears in history list");

            var detail = await repository.GetSimulationByIdAsync(id);
            Check(detail.IsSuccess && detail.Data!.Summary!.MonthlyProductions.Count == 12
                && detail.Data.Summary.TotalAnnualEnergyKwh == simulation.Summary!.TotalAnnualEnergyKwh
                && detail.Data.Location.LocationName == "Yogyakarta", "Detail reloads 12 months and matching totals");
        }
        finally
        {
            var delete = await repository.DeleteSimulationAsync(id);
            Check(delete.IsSuccess, "Delete simulation (monthly rows cascade)");
        }
        Check(!(await repository.GetSimulationByIdAsync(id)).IsSuccess, "Deleted simulation is no longer found");
        Check(!(await repository.InsertSimulationAsync(new Simulation { ScenarioName = "Tanpa Hasil", SystemParameter = parameter })).IsSuccess, "Rejects saving simulation without results");
    }
}
