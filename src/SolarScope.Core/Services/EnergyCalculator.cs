using System.Globalization;
using SolarScope.Core.Models;

namespace SolarScope.Core.Services;

public abstract class EnergyCalculator
{
    private static readonly CultureInfo IndonesianCulture = CultureInfo.GetCultureInfo("id-ID");

    public ServiceResult<SimulationSummary> Calculate(SystemParameter parameter, IReadOnlyList<double> dailyRadiationByMonth)
    {
        var parameterCheck = parameter.Validate();
        if (!parameterCheck.IsValid)
        {
            return ServiceResult<SimulationSummary>.Fail(parameterCheck.err!);
        }
        if (dailyRadiationByMonth is not { Count: 12 })
        {
            return ServiceResult<SimulationSummary>.Fail("Data radiasi harus berisi 12 nilai bulanan");
        }
        if (dailyRadiationByMonth.Any(radiation => !double.IsFinite(radiation) || radiation < 0))
        {
            return ServiceResult<SimulationSummary>.Fail("Data radiasi tidak valid atau bernilai negatif");
        }

        var months = new List<MonthlyProduction>(12);
        for (int month = 1; month <= 12; month++)
        {
            double radiation = dailyRadiationByMonth[month - 1];
            months.Add(new MonthlyProduction
            {
                MonthIndex = month,
                MonthName = IndonesianCulture.DateTimeFormat.GetMonthName(month),
                SolarRadiationDailyAverage = radiation,
                EstimatedEnergyKwh = Math.Round(EstimateMonthKwh(parameter, radiation, GetDaysInMonth(month)), 2)
            });
        }

        return ServiceResult<SimulationSummary>.Ok(new SimulationSummary
        {
            TotalAnnualEnergyKwh = Math.Round(months.Sum(m => m.EstimatedEnergyKwh), 2),
            PeakProductionMonth = months.MaxBy(m => m.EstimatedEnergyKwh)!.MonthName,
            LowestProductionMonth = months.MinBy(m => m.EstimatedEnergyKwh)!.MonthName,
            MonthlyProductions = months
        });
    }

    protected abstract int GetDaysInMonth(int month);

    private static double EstimateMonthKwh(SystemParameter parameter, double dailyRadiation, int days) =>
        parameter.SystemCapacityKwp * dailyRadiation * days * parameter.PerformanceRatio;
}
