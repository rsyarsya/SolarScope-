namespace SolarScope.Core.Services;

public sealed class ClimatologyEnergyCalculator : EnergyCalculator
{
    private static readonly int[] DaysPerMonth = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

    protected override int GetDaysInMonth(int month) => DaysPerMonth[month - 1];
}
