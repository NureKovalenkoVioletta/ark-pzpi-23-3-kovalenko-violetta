using FitnessProject.BLL.Configuration;
using FitnessProject.BLL.DTO.Services;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.Data;
using FitnessProject.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitnessProject.BLL.Services;

public class ActivityMonitorService : IActivityMonitorService
{
    private readonly ApplicationDbContext _context;

    public ActivityMonitorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ActivityChangeResult> CheckActivityChangesAsync(int userId, DateTime date)
    {
        var day = date.Date;
        var dayStart = day;
        var dayEnd = day.AddDays(1);
        var weekStart = day.AddDays(-7);

        var deviceIds = await _context.Devices
            .Where(d => d.UserId == userId)
            .Select(d => d.DeviceId)
            .ToListAsync();

        if (!deviceIds.Any())
        {
            return new ActivityChangeResult();
        }

        var stepsToday = await _context.TelemetrySamples
            .Where(ts => deviceIds.Contains(ts.DeviceId)
                         && ts.TelemetryType == TelemetryType.Steps
                         && ts.Timestamp >= dayStart
                         && ts.Timestamp < dayEnd)
            .SumAsync(ts => (decimal?)ts.TelemetryValue) ?? 0m;

        var heartRateTodayList = await _context.TelemetrySamples
            .Where(ts => deviceIds.Contains(ts.DeviceId)
                         && ts.TelemetryType == TelemetryType.HeartRate
                         && ts.Timestamp >= dayStart
                         && ts.Timestamp < dayEnd)
            .Select(ts => ts.TelemetryValue)
            .ToListAsync();
        var heartRateToday = heartRateTodayList.Any() ? heartRateTodayList.Average() : (decimal?)null;

        var intensityTodayList = await _context.TrainingSessions
            .Where(ts => deviceIds.Contains(ts.DeviceId)
                         && ts.StartTime >= dayStart
                         && ts.StartTime < dayEnd)
            .Select(ts => (decimal?)ts.Intensity)
            .ToListAsync();
        var intensityToday = intensityTodayList.Any() ? intensityTodayList.Average() : (decimal?)null;

        var weeklyStepsDaily = await _context.TelemetrySamples
            .Where(ts => deviceIds.Contains(ts.DeviceId)
                         && ts.TelemetryType == TelemetryType.Steps
                         && ts.Timestamp >= weekStart
                         && ts.Timestamp < dayStart)
            .GroupBy(ts => ts.Timestamp.Date)
            .Select(g => g.Sum(x => x.TelemetryValue))
            .ToListAsync();

        var weeklyHeartDaily = await _context.TelemetrySamples
            .Where(ts => deviceIds.Contains(ts.DeviceId)
                         && ts.TelemetryType == TelemetryType.HeartRate
                         && ts.Timestamp >= weekStart
                         && ts.Timestamp < dayStart)
            .GroupBy(ts => ts.Timestamp.Date)
            .Select(g => g.Average(x => x.TelemetryValue))
            .ToListAsync();

        var weeklyIntensityDaily = await _context.TrainingSessions
            .Where(ts => deviceIds.Contains(ts.DeviceId)
                         && ts.StartTime >= weekStart
                         && ts.StartTime < dayStart)
            .GroupBy(ts => ts.StartTime.Date)
            .Select(g => g.Average(x => (decimal)x.Intensity))
            .ToListAsync();

        var weeklyAverage = new WeeklyActivityAverage
        {
            AverageSteps = weeklyStepsDaily.Any() ? weeklyStepsDaily.Average() : 0m,
            AverageHeartRate = weeklyHeartDaily.Any() ? weeklyHeartDaily.Average() : null,
            AverageTrainingIntensity = weeklyIntensityDaily.Any() ? weeklyIntensityDaily.Average() : null
        };

        var stepsChange = ComputeChangePercent(stepsToday, weeklyAverage.AverageSteps);
        var hrChange = ComputeChangePercent(heartRateToday, weeklyAverage.AverageHeartRate);
        var intensityChange = ComputeChangePercent(intensityToday, weeklyAverage.AverageTrainingIntensity);

        var result = new ActivityChangeResult
        {
            WeeklyAverage = weeklyAverage,
            StepsToday = stepsToday,
            HeartRateToday = heartRateToday,
            TrainingIntensityToday = intensityToday,
            StepsChangePercent = stepsChange,
            HeartRateChangePercent = hrChange,
            TrainingIntensityChangePercent = intensityChange,
            StepsSpike = stepsChange.HasValue && stepsChange.Value > ActivityThresholds.StepsSpikeThreshold,
            TrainingIntensityChange = intensityChange.HasValue && Math.Abs(intensityChange.Value) > ActivityThresholds.TrainingIntensityChangeThreshold,
            HeartRateAnomaly = heartRateToday.HasValue &&
                               (heartRateToday < ActivityThresholds.RestHeartRateLow ||
                                heartRateToday > ActivityThresholds.RestHeartRateHigh)
        };

        return result;
    }

    private static decimal? ComputeChangePercent(decimal? current, decimal? average)
    {
        if (!current.HasValue || !average.HasValue || average == 0)
        {
            return null;
        }

        return (current.Value - average.Value) / average.Value;
    }
}

