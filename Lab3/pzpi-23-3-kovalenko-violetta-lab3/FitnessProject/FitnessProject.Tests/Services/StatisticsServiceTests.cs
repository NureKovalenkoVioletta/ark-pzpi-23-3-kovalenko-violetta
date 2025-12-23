using FitnessProject.BLL.Services;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;
using FitnessProject.Enums;
using Moq;
using Xunit;

namespace FitnessProject.Tests.Services;

public class StatisticsServiceTests
{
    private readonly DateTime _day = new(2025, 12, 18, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task GetDailyStatistics_NoDevices_ReturnsEmpty()
    {
        var service = CreateService(
            devices: Array.Empty<Device>(),
            telemetry: Array.Empty<TelemetrySample>(),
            sleep: Array.Empty<SleepRecord>(),
            trainings: Array.Empty<TrainingSession>());

        var result = await service.GetDailyStatisticsAsync(1, _day);

        Assert.Equal(0, result.Steps);
        Assert.Null(result.HeartRateAvg);
        Assert.Equal(0, result.HeartRateSamples);
        Assert.Equal(0, result.TotalSleepMinutes);
        Assert.Equal(0, result.TrainingCount);
    }

    [Fact]
    public async Task GetDailyStatistics_HeartRateAggregates()
    {
        var devices = new[] { new Device { DeviceId = 1, UserId = 1 } };
        var telemetry = new[]
        {
            Hr(1, _day.AddHours(1), 60),
            Hr(1, _day.AddHours(2), 70),
            Hr(1, _day.AddHours(3), 80)
        };

        var service = CreateService(devices, telemetry, Array.Empty<SleepRecord>(), Array.Empty<TrainingSession>());

        var result = await service.GetDailyStatisticsAsync(1, _day);

        Assert.Equal(70, result.HeartRateAvg);
        Assert.Equal(60, result.HeartRateMin);
        Assert.Equal(80, result.HeartRateMax);
        Assert.Equal(3, result.HeartRateSamples);
    }

    [Fact]
    public async Task GetDailyStatistics_StepsSum()
    {
        var devices = new[] { new Device { DeviceId = 1, UserId = 1 } };
        var telemetry = new[]
        {
            Steps(1, _day.AddHours(1), 1000),
            Steps(1, _day.AddHours(2), 2500),
            Steps(1, _day.AddHours(3), 3000)
        };

        var service = CreateService(devices, telemetry, Array.Empty<SleepRecord>(), Array.Empty<TrainingSession>());

        var result = await service.GetDailyStatisticsAsync(1, _day);

        Assert.Equal(6500, result.Steps);
    }

    [Fact]
    public async Task GetDailyStatistics_SleepAggregation()
    {
        var devices = new[] { new Device { DeviceId = 1, UserId = 1 } };
        var sleep = new[]
        {
            new SleepRecord
            {
                DeviceId = 1, Date = _day, TotalSleepMinutes = 300, DeepSleepMinutes = 100,
                LightSleepMinutes = 180, AwakeMinutes = 20, SleepQuality = 0.6m
            },
            new SleepRecord
            {
                DeviceId = 1, Date = _day, TotalSleepMinutes = 200, DeepSleepMinutes = 50,
                LightSleepMinutes = 120, AwakeMinutes = 10, SleepQuality = null
            },
            new SleepRecord
            {
                DeviceId = 1, Date = _day, TotalSleepMinutes = 0, DeepSleepMinutes = 0,
                LightSleepMinutes = 0, AwakeMinutes = 0, SleepQuality = 0.8m
            }
        };

        var service = CreateService(devices, Array.Empty<TelemetrySample>(), sleep, Array.Empty<TrainingSession>());

        var result = await service.GetDailyStatisticsAsync(1, _day);

        Assert.Equal(500, result.TotalSleepMinutes);
        Assert.Equal(150, result.DeepSleepMinutes);
        Assert.Equal(300, result.LightSleepMinutes);
        Assert.Equal(30, result.AwakeMinutes);
        Assert.Equal(0.7m, result.SleepQualityAvg);
    }

    [Fact]
    public async Task GetDailyStatistics_TrainingAggregation()
    {
        var devices = new[] { new Device { DeviceId = 1, UserId = 1 } };
        var trainings = new[]
        {
            new TrainingSession
            {
                DeviceId = 1, StartTime = _day.AddHours(9), DurationInMin = 30,
                Intensity = Intensity.Moderate, CaloriesEstimated = null
            },
            new TrainingSession
            {
                DeviceId = 1, StartTime = _day.AddHours(18), DurationInMin = 60,
                Intensity = Intensity.High, CaloriesEstimated = 500
            }
        };

        var service = CreateService(devices, Array.Empty<TelemetrySample>(), Array.Empty<SleepRecord>(), trainings);

        var result = await service.GetDailyStatisticsAsync(1, _day);

        Assert.Equal(2, result.TrainingCount);
        Assert.Equal(90, result.TrainingDurationMinutes);
        Assert.Equal(1.5m, result.TrainingIntensityAvg);
        Assert.Equal(500, result.TrainingCalories);
    }

    [Fact]
    public async Task GetDailyStatistics_FiltersByDate()
    {
        var devices = new[] { new Device { DeviceId = 1, UserId = 1 } };
        var telemetry = new[]
        {
            Hr(1, _day.AddDays(-1), 50),
            Hr(1, _day.AddHours(1), 70),
            Steps(1, _day.AddHours(2), 1000),
            Steps(1, _day.AddDays(1), 5000)
        };
        var sleep = new[]
        {
            new SleepRecord { DeviceId = 1, Date = _day.AddDays(-1), TotalSleepMinutes = 300 },
            new SleepRecord { DeviceId = 1, Date = _day, TotalSleepMinutes = 200 }
        };
        var trainings = new[]
        {
            new TrainingSession { DeviceId = 1, StartTime = _day.AddDays(-1), DurationInMin = 30, Intensity = Intensity.Low },
            new TrainingSession { DeviceId = 1, StartTime = _day.AddHours(10), DurationInMin = 60, Intensity = Intensity.High, CaloriesEstimated = 400 }
        };

        var service = CreateService(devices, telemetry, sleep, trainings);

        var result = await service.GetDailyStatisticsAsync(1, _day);

        Assert.Equal(70, result.HeartRateAvg);
        Assert.Equal(1000, result.Steps);
        Assert.Equal(200, result.TotalSleepMinutes);
        Assert.Equal(1, result.TrainingCount);
        Assert.Equal(60, result.TrainingDurationMinutes);
        Assert.Equal(400, result.TrainingCalories);
    }

    private StatisticsService CreateService(
        IEnumerable<Device> devices,
        IEnumerable<TelemetrySample> telemetry,
        IEnumerable<SleepRecord> sleep,
        IEnumerable<TrainingSession> trainings)
    {
        var deviceRepo = new Moq.Mock<IDeviceRepository>();
        deviceRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Device, bool>>>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<Device, bool>> p) => devices.Where(p.Compile()).ToList());

        var telemetryRepo = new Moq.Mock<ITelemetrySampleRepository>();
        telemetryRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TelemetrySample, bool>>>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<TelemetrySample, bool>> p) => telemetry.Where(p.Compile()).ToList());

        var sleepRepo = new Moq.Mock<ISleepRecordRepository>();
        sleepRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SleepRecord, bool>>>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<SleepRecord, bool>> p) => sleep.Where(p.Compile()).ToList());

        var trainingRepo = new Moq.Mock<ITrainingSessionRepository>();
        trainingRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TrainingSession, bool>>>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<TrainingSession, bool>> p) => trainings.Where(p.Compile()).ToList());

        return new StatisticsService(telemetryRepo.Object, sleepRepo.Object, trainingRepo.Object, deviceRepo.Object);
    }

    private static TelemetrySample Hr(int deviceId, DateTime ts, decimal value) =>
        new()
        {
            DeviceId = deviceId,
            Timestamp = ts,
            TelemetryType = TelemetryType.HeartRate,
            TelemetryValue = value
        };

    private static TelemetrySample Steps(int deviceId, DateTime ts, decimal value) =>
        new()
        {
            DeviceId = deviceId,
            Timestamp = ts,
            TelemetryType = TelemetryType.Steps,
            TelemetryValue = value
        };
}

