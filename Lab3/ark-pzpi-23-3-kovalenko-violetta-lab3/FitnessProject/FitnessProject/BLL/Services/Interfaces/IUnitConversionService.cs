using FitnessProject.Enums;

namespace FitnessProject.BLL.Services.Interfaces;

public interface IUnitConversionService
{
    PreferredUnits DeterminePreferredUnits(string? locale);

    decimal ConvertWeight(decimal value, PreferredUnits from, PreferredUnits to, int precision = 1);

    decimal ConvertVolume(decimal value, PreferredUnits from, PreferredUnits to, int precision = 1);
}

