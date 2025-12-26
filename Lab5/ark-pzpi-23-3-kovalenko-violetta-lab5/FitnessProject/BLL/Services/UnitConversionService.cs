using System.Globalization;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.Enums;

namespace FitnessProject.BLL.Services;

public class UnitConversionService : IUnitConversionService
{
    private const decimal GramsPerOunce = 28.3495m;
    private const decimal MillilitersPerFluidOunce = 29.5735m;

    public PreferredUnits DeterminePreferredUnits(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return PreferredUnits.Metric;
        }

        var lower = locale.ToLowerInvariant();
        if (lower.StartsWith("en"))
        {
            return PreferredUnits.Imperial;
        }

        return PreferredUnits.Metric;
    }

    public decimal ConvertWeight(decimal value, PreferredUnits from, PreferredUnits to, int precision = 1)
    {
        if (from == to)
        {
            return Math.Round(value, precision);
        }

        return to switch
        {
            PreferredUnits.Imperial => Math.Round(value / GramsPerOunce, precision),
            PreferredUnits.Metric => Math.Round(value * GramsPerOunce, precision),
            _ => Math.Round(value, precision)
        };
    }

    public decimal ConvertVolume(decimal value, PreferredUnits from, PreferredUnits to, int precision = 1)
    {
        if (from == to)
        {
            return Math.Round(value, precision);
        }

        return to switch
        {
            PreferredUnits.Imperial => Math.Round(value / MillilitersPerFluidOunce, precision),
            PreferredUnits.Metric => Math.Round(value * MillilitersPerFluidOunce, precision),
            _ => Math.Round(value, precision)
        };
    }
}

