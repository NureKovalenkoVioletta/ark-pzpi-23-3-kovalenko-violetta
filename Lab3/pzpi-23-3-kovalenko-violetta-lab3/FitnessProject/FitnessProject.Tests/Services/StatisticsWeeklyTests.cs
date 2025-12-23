using FitnessProject.BLL.Services;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;
using FitnessProject.Enums;
using Moq;
using Xunit;

namespace FitnessProject.Tests.Services;

public class StatisticsWeeklyTests
{
    [Fact]
    public async Task GetWeeklyStatistics_AggregatesAndAverages()
    {
        var devices = new[] { new Device { DeviceId = 1, UserId = 1 } };
        var start = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc);

        var telemetry = new List<TelemetrySample>();
        var sleep = new List<SleepRecord>();
        var trainings = new List<TrainingSession>();

        for (int i = 0; i < 7; i++)
        {
            var day = start.AddDays(i);
            telemetry.Add(Steps(1, day.AddHours(1), 1000 + i * 100));
            telemetry.Add(Hr(1, day.AddHours(2), 60 + i));

            sleep.Add(new SleepRecord
            {
                DeviceId = 1,
                Date = day,
                TotalSleepMinutes = 400 + i * 10,
                DeepSleepMinutes = 80 + i * 2,
                LightSleepMinutes = 280 + i * 8,
                AwakeMinutes = 40,
                SleepQuality = 0.6m + i * 0.01m
            });

            trainings.Add(new TrainingSession
            {
                DeviceId = 1,
                StartTime = day.AddHours(9),
                DurationInMin = 30 + i,
                Intensity = i % 2 == 0 ? Intensity.Moderate : Intensity.High,
                CaloriesEstimated = 300 + i * 10
            });
        }

        var service = CreateService(devices, telemetry, sleep, trainings);

        var weekly = await service.GetWeeklyStatisticsAsync(1, start);

        Assert.Equal(7, weekly.Days.Count);
        Assert.Equal(7 * (1000 + 1600) / 2, weekly.TotalSteps);
        Assert.Equal(7 * (400 + 460) / 2, weekly.TotalSleepMinutes);
        Assert.Equal(7 * (30 + 36) / 2, weekly.TrainingDurationMinutes);
        Assert.Equal(7 * (300 + 360) / 2, weekly.TrainingCalories);

        Assert.Equal(63, weekly.HeartRateAvg);
        Assert.Equal(0.63m, weekly.SleepQualityAvg);

        Assert.NotNull(weekly.StepsTrendPercent);
        Assert.True(weekly.StepsTrendPercent!.Value > 0);
        Assert.True(weekly.TrainingDurationTrendPercent!.Value > 0);
        Assert.True(weekly.TrainingCaloriesTrendPercent!.Value > 0);
    }

    [Fact]
    public async Task GetWeeklyStatistics_TrendsNullIfNotEnoughDays()
    {
        var devices = new[] { new Device { DeviceId = 1, UserId = 1 } };
        var start = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc);
        var telemetry = new List<TelemetrySample>
        {
            Steps(1, start.AddHours(1), 1000),
            Steps(1, start.AddDays(1).AddHours(1), 1100),
            Steps(1, start.AddDays(2).AddHours(1), 1200),
        };

        var service = CreateService(devices, telemetry, Array.Empty<SleepRecord>(), Array.Empty<TrainingSession>());
        var weekly = await service.GetWeeklyStatisticsAsync(1, start);

        Assert.Null(weekly.StepsTrendPercent);
    }

    private StatisticsService CreateService(
        IEnumerable<Device> devices,
        IEnumerable<TelemetrySample> telemetry,
        IEnumerable<SleepRecord> sleep,
        IEnumerable<TrainingSession> trainings)
    {
        var deviceRepo = new Mock<IDeviceRepository>();
        deviceRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Device, bool>>>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<Device, bool>> p) => devices.Where(p.Compile()).ToList());

        var telemetryRepo = new Mock<ITelemetrySampleRepository>();
        telemetryRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TelemetrySample, bool>>>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<TelemetrySample, bool>> p) => telemetry.Where(p.Compile()).ToList());

        var sleepRepo = new Mock<ISleepRecordRepository>();
        sleepRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SleepRecord, bool>>>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<SleepRecord, bool>> p) => sleep.Where(p.Compile()).ToList());

        var trainingRepo = new Mock<ITrainingSessionRepository>();
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

