using FitnessProject.Enums;

namespace FitnessProject.BLL.Services.Helpers;

public static class TelemetryNormalizer
{
    private const decimal MinHeartRate = 40;
    private const decimal MaxHeartRate = 220;
    private const decimal MinSteps = 0;
    private const decimal MinSleepMinutes = 0;
    private const decimal MaxSleepMinutes = 1440;

    public static (bool isValid, decimal normalizedValue, string? errorMessage) NormalizeAndValidate(
        TelemetryType telemetryType, 
        decimal value)
    {
        if (value < 0 && telemetryType != TelemetryType.BloodPressure)
        {
            return (false, value, $"Value cannot be negative for {telemetryType}. Received: {value}");
        }

        return telemetryType switch
        {
            TelemetryType.HeartRate => NormalizeHeartRate(value),
            TelemetryType.Steps => NormalizeSteps(value),
            _ => (true, Math.Round(value, 2), null)
        };
    }

    private static (bool isValid, decimal normalizedValue, string? errorMessage) NormalizeHeartRate(decimal value)
    {
        if (value < MinHeartRate || value > MaxHeartRate)
        {
            return (false, value, 
                $"Invalid heart rate value: {value} bpm. Expected range: {MinHeartRate}-{MaxHeartRate} bpm. " +
                $"This value appears to be unrealistic and was rejected. Please check the device.");
        }

        return (true, Math.Round(value, 0), null);
    }

    private static (bool isValid, decimal normalizedValue, string? errorMessage) NormalizeSteps(decimal value)
    {
        if (value < MinSteps)
        {
            return (false, value, 
                $"Invalid steps value: {value}. Steps cannot be negative. " +
                $"This value appears to be an error and was rejected. Please check the device.");
        }

        return (true, Math.Round(value, 0), null);
    }

    public static (bool isValid, string? errorMessage) ValidateSleepData(
        Dictionary<string, object>? metadata)
    {
        if (metadata == null || metadata.Count == 0)
        {
            return (false, "Sleep data requires metadata with sleep information");
        }

        if (!metadata.ContainsKey("TotalSleepMinutes"))
        {
            return (false, "Sleep data must contain TotalSleepMinutes");
        }

        if (metadata.TryGetValue("TotalSleepMinutes", out var totalSleepObj) &&
            totalSleepObj != null)
        {
            if (decimal.TryParse(totalSleepObj.ToString(), out var totalSleep))
            {
                if (totalSleep < MinSleepMinutes || totalSleep > MaxSleepMinutes)
                {
                    return (false, 
                        $"Invalid sleep duration: {totalSleep} minutes. Expected range: {MinSleepMinutes}-{MaxSleepMinutes} minutes (0-24 hours). " +
                        $"This value appears to be unrealistic and was rejected. Please check the device.");
                }
            }
        }

        return (true, null);
    }

    public static int NormalizeSleepMinutes(object? value)
    {
        if (value == null)
            return 0;

        if (int.TryParse(value.ToString(), out var minutes))
        {
            return Math.Max(0, Math.Min(minutes, (int)MaxSleepMinutes));
        }

        if (decimal.TryParse(value.ToString(), out var decimalMinutes))
        {
            return Math.Max(0, Math.Min((int)Math.Round(decimalMinutes), (int)MaxSleepMinutes));
        }

        return 0;
    }

    public static decimal? NormalizeSleepQuality(object? value)
    {
        if (value == null)
            return null;

        if (decimal.TryParse(value.ToString(), out var quality))
        {
            return Math.Max(0, Math.Min(100, Math.Round(quality, 2)));
        }

        return null;
    }
}

