namespace SolarScope.Core.Models;

public class SimulationSummary
{
    public double TotalAnnualEnergyKwh { get; set; }
    public string PeakProductionMonth { get; set; } = string.Empty;
    public string LowestProductionMonth { get; set; } = string.Empty;
    public List<MonthlyProduction> MonthlyProductions { get; set; } = new();
}