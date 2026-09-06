namespace SolarScope.Core.Models;

public class MonthlyProduction
{
    public int MonthIndex { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public double SolarRadiationDailyAverage { get; set; }
    public double EstimatedEnergyKwh { get; set; }
}