using FitnessProject.BLL.DTO.Services;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitnessProject.BLL.Services;

public class StatisticsService : IStatisticsService
{
    private readonly ITelemetrySampleRepository _telemetrySampleRepository;
    private readonly ISleepRecordRepository _sleepRecordRepository;
    private readonly ITrainingSessionRepository _trainingSessionRepository;
    private readonly IDeviceRepository _deviceRepository;

    public StatisticsService(
        ITelemetrySampleRepository telemetrySampleRepository,
        ISleepRecordRepository sleepRecordRepository,
        ITrainingSessionRepository trainingSessionRepository,
        IDeviceRepository deviceRepository)
    {
        _telemetrySampleRepository = telemetrySampleRepository;
        _sleepRecordRepository = sleepRecordRepository;
        _trainingSessionRepository = trainingSessionRepository;
        _deviceRepository = deviceRepository;
    }

    public async Task<DailyStatisticsDto> GetDailyStatisticsAsync(int userId, DateTime date)
    {
        var day = date.Date;
        var devices = await _deviceRepository.FindAsync(d => d.UserId == userId);
        var deviceIds = devices.Select(d => d.DeviceId).ToList();
        if (!deviceIds.Any())
        {
            return new DailyStatisticsDto();
        }

        var telemetry = await AggregateTelemetryForDay(deviceIds, day);
        var sleep = await AggregateSleepForDay(deviceIds, day);
        var training = await AggregateTrainingsForDay(deviceIds, day);

        return new DailyStatisticsDto
        {
            Steps = telemetry.Steps,
            HeartRateAvg = telemetry.HeartRateAvg,
            HeartRateMin = telemetry.HeartRateMin,
            HeartRateMax = telemetry.HeartRateMax,
            HeartRateSamples = telemetry.HeartRateSamples,

            TotalSleepMinutes = sleep.TotalSleepMinutes,
            DeepSleepMinutes = sleep.DeepSleepMinutes,
            LightSleepMinutes = sleep.LightSleepMinutes,
            AwakeMinutes = sleep.AwakeMinutes,
            SleepQualityAvg = sleep.SleepQualityAvg,

            TrainingCount = training.TrainingCount,
            TrainingDurationMinutes = training.TrainingDurationMinutes,
            TrainingIntensityAvg = training.TrainingIntensityAvg,
            TrainingCalories = training.TrainingCalories
        };
    }

    public async Task<WeeklyStatisticsDto> GetWeeklyStatisticsAsync(int userId, DateTime startDate)
    {
        var start = startDate.Date;
        var end = start.AddDays(7);

        var days = new List<DailyStatisticsDto>();
        for (var day = start; day < end; day = day.AddDays(1))
        {
            days.Add(await GetDailyStatisticsAsync(userId, day));
        }

        var weekly = new WeeklyStatisticsDto
        {
            StartDate = start,
            EndDate = end.AddDays(-1),
            Days = days
        };

        weekly.TotalSteps = days.Sum(d => d.Steps);
        weekly.HeartRateAvg = AverageNullable(days.Select(d => d.HeartRateAvg));
        weekly.HeartRateMin = MinNullable(days.Select(d => d.HeartRateMin));
        weekly.HeartRateMax = MaxNullable(days.Select(d => d.HeartRateMax));

        weekly.TotalSleepMinutes = days.Sum(d => d.TotalSleepMinutes);
        weekly.DeepSleepMinutes = days.Sum(d => d.DeepSleepMinutes);
        weekly.LightSleepMinutes = days.Sum(d => d.LightSleepMinutes);
        weekly.AwakeMinutes = days.Sum(d => d.AwakeMinutes);
        weekly.SleepQualityAvg = AverageNullable(days.Select(d => d.SleepQualityAvg));

        weekly.TrainingCount = days.Sum(d => d.TrainingCount);
        weekly.TrainingDurationMinutes = days.Sum(d => d.TrainingDurationMinutes);
        weekly.TrainingIntensityAvg = AverageNullable(days.Select(d => d.TrainingIntensityAvg));
        weekly.TrainingCalories = days.Sum(d => d.TrainingCalories);

        weekly.StepsTrendPercent = CalculateTrend(days, d => d.Steps);
        weekly.HeartRateAvgTrendPercent = CalculateTrend(days, d => d.HeartRateAvg ?? 0);
        weekly.SleepMinutesTrendPercent = CalculateTrend(days, d => (decimal)d.TotalSleepMinutes);
        weekly.TrainingDurationTrendPercent = CalculateTrend(days, d => (decimal)d.TrainingDurationMinutes);
        weekly.TrainingCaloriesTrendPercent = CalculateTrend(days, d => d.TrainingCalories);

        return weekly;
    }

    public async Task<WeekComparisonDto> CompareWithPreviousWeek(int userId, DateTime currentWeekStart)
    {
        var current = await GetWeeklyStatisticsAsync(userId, currentWeekStart);
        var previousStart = currentWeekStart.Date.AddDays(-7);
        var previous = await GetWeeklyStatisticsAsync(userId, previousStart);

        return new WeekComparisonDto
        {
            CurrentWeek = current,
            PreviousWeek = previous,
            StepsChangePercent = ChangePercent(previous.TotalSteps, current.TotalSteps),
            HeartRateAvgChangePercent = ChangePercent(previous.HeartRateAvg, current.HeartRateAvg),
            TotalSleepChangePercent = ChangePercent(previous.TotalSleepMinutes, current.TotalSleepMinutes),
            TrainingDurationChangePercent = ChangePercent(previous.TrainingDurationMinutes, current.TrainingDurationMinutes),
            TrainingCaloriesChangePercent = ChangePercent(previous.TrainingCalories, current.TrainingCalories)
        };
    }

    private async Task<TelemetryAggregateDto> AggregateTelemetryForDay(List<int> deviceIds, DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        var heartRates = await _telemetrySampleRepository
            .FindAsync(ts => deviceIds.Contains(ts.DeviceId)
                             && ts.TelemetryType == TelemetryType.HeartRate
                             && ts.Timestamp >= start
                             && ts.Timestamp < end);

        var stepsSamples = await _telemetrySampleRepository
            .FindAsync(ts => deviceIds.Contains(ts.DeviceId)
                             && ts.TelemetryType == TelemetryType.Steps
                             && ts.Timestamp >= start
                             && ts.Timestamp < end);

        var heartRateValues = heartRates.Select(hr => hr.TelemetryValue).ToList();

        return new TelemetryAggregateDto
        {
            Steps = stepsSamples.Sum(s => s.TelemetryValue),
            HeartRateAvg = heartRateValues.Any() ? heartRateValues.Average() : null,
            HeartRateMin = heartRateValues.Any() ? heartRateValues.Min() : null,
            HeartRateMax = heartRateValues.Any() ? heartRateValues.Max() : null,
            HeartRateSamples = heartRateValues.Count
        };
    }

    private async Task<SleepAggregateDto> AggregateSleepForDay(List<int> deviceIds, DateTime date)
    {
        var day = date.Date;
        var records = await _sleepRecordRepository
            .FindAsync(sr => deviceIds.Contains(sr.DeviceId) && sr.Date.Date == day);

        var qualityList = records.Where(r => r.SleepQuality.HasValue).Select(r => r.SleepQuality!.Value).ToList();

        return new SleepAggregateDto
        {
            TotalSleepMinutes = records.Sum(r => r.TotalSleepMinutes),
            DeepSleepMinutes = records.Sum(r => r.DeepSleepMinutes),
            LightSleepMinutes = records.Sum(r => r.LightSleepMinutes),
            AwakeMinutes = records.Sum(r => r.AwakeMinutes),
            SleepQualityAvg = qualityList.Any() ? qualityList.Average() : null
        };
    }

    private async Task<TrainingAggregateDto> AggregateTrainingsForDay(List<int> deviceIds, DateTime date)
    {
        var day = date.Date;
        var trainings = await _trainingSessionRepository
            .FindAsync(t => deviceIds.Contains(t.DeviceId) && t.StartTime.Date == day);

        var intensityValues = trainings.Select(t => (decimal)t.Intensity).ToList();

        return new TrainingAggregateDto
        {
            TrainingCount = trainings.Count(),
            TrainingDurationMinutes = trainings.Sum(t => t.DurationInMin),
            TrainingIntensityAvg = intensityValues.Any() ? intensityValues.Average() : null,
            TrainingCalories = trainings.Sum(t => t.CaloriesEstimated ?? 0)
        };
    }

    private static decimal? AverageNullable(IEnumerable<decimal?> values)
    {
        var list = values.Where(v => v.HasValue).Select(v => v.Value).ToList();
        return list.Any() ? list.Average() : null;
    }

    private static decimal? MinNullable(IEnumerable<decimal?> values)
    {
        var list = values.Where(v => v.HasValue).Select(v => v.Value).ToList();
        return list.Any() ? list.Min() : null;
    }

    private static decimal? MaxNullable(IEnumerable<decimal?> values)
    {
        var list = values.Where(v => v.HasValue).Select(v => v.Value).ToList();
        return list.Any() ? list.Max() : null;
    }

    private static decimal? CalculateTrend(List<DailyStatisticsDto> days, Func<DailyStatisticsDto, decimal> selector)
    {
        var values = days.Select(selector).ToList();
        var nonZeroCount = values.Count(v => v != 0);
        if (nonZeroCount < 6) return null;

        var first3 = values.Take(3).Average();
        var last3 = values.Skip(Math.Max(0, values.Count - 3)).Average();

        if (first3 == 0) return null;
        return (last3 - first3) / first3;
    }

    private static decimal? ChangePercent(decimal? oldValue, decimal? newValue)
    {
        if (!oldValue.HasValue || !newValue.HasValue) return null;
        if (oldValue.Value == 0) return null;
        return (newValue.Value - oldValue.Value) / oldValue.Value;
    }

    private static decimal? ChangePercent(decimal oldValue, decimal newValue)
    {
        if (oldValue == 0) return null;
        return (newValue - oldValue) / oldValue;
    }
}

