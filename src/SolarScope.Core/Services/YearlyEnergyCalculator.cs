namespace SolarScope.Core.Services;

public sealed class YearlyEnergyCalculator : EnergyCalculator
{
    private readonly int _year;

    public YearlyEnergyCalculator(int year)
    {
        if (year < 1 || year > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Tahun harus bernilai antara 1 dan 9999");
        }
        _year = year;
    }

    public int Year => _year;

    protected override int GetDaysInMonth(int month) => DateTime.DaysInMonth(_year, month);
}
