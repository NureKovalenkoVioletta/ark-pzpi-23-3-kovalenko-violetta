using FitnessProject.BLL.Services;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;
using FitnessProject.Enums;
using Moq;
using Xunit;

namespace FitnessProject.Tests.Services;

public class StatisticsComparisonTests
{
    [Fact]
    public async Task CompareWithPreviousWeek_ComputesPercentChanges()
    {
        var devices = new[] { new Device { DeviceId = 1, UserId = 1 } };
        var startCurrent = new DateTime(2025, 12, 8, 0, 0, 0, DateTimeKind.Utc);
        var startPrev = startCurrent.AddDays(-7);

        var telemetryPrev = Enumerable.Range(0, 7)
            .Select(i => Steps(1, startPrev.AddDays(i).AddHours(1), 1000)).ToList();
        var telemetryCurr = Enumerable.Range(0, 7)
            .Select(i => Steps(1, startCurrent.AddDays(i).AddHours(1), 2000)).ToList();

        var service = CreateService(
            devices,
            telemetryPrev.Concat(telemetryCurr),
            Array.Empty<SleepRecord>(),
            Array.Empty<TrainingSession>());

        var comparison = await service.CompareWithPreviousWeek(1, startCurrent);

        Assert.NotNull(comparison.StepsChangePercent);
        Assert.Equal(1.0m, comparison.StepsChangePercent!.Value); // +100%
    }

    [Fact]
    public async Task CompareWithPreviousWeek_DivideByZeroSafe()
    {
        var devices = new[] { new Device { DeviceId = 1, UserId = 1 } };
        var startCurrent = new DateTime(2025, 12, 8, 0, 0, 0, DateTimeKind.Utc);
        var startPrev = startCurrent.AddDays(-7);

        var telemetryPrev = new List<TelemetrySample>();
        var telemetryCurr = Enumerable.Range(0, 7)
            .Select(i => Steps(1, startCurrent.AddDays(i).AddHours(1), 2000)).ToList();

        var service = CreateService(
            devices,
            telemetryPrev.Concat(telemetryCurr),
            Array.Empty<SleepRecord>(),
            Array.Empty<TrainingSession>());

        var comparison = await service.CompareWithPreviousWeek(1, startCurrent);

        Assert.Null(comparison.StepsChangePercent);
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

    private static TelemetrySample Steps(int deviceId, DateTime ts, decimal value) =>
        new()
        {
            DeviceId = deviceId,
            Timestamp = ts,
            TelemetryType = TelemetryType.Steps,
            TelemetryValue = value
        };
}

