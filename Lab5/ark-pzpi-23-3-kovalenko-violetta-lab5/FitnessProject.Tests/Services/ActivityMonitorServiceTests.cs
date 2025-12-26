using System;
using System.Linq;
using System.Threading.Tasks;
using FitnessProject.BLL.Configuration;
using FitnessProject.BLL.Services;
using FitnessProject.Data;
using FitnessProject.Entities;
using FitnessProject.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Xunit;

namespace FitnessProject.Tests.Services;

public class ActivityMonitorServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task NoDevices_ReturnsDefaults()
    {
        await using var ctx = CreateContext();
        var svc = new ActivityMonitorService(ctx);

        var result = await svc.CheckActivityChangesAsync(1, DateTime.UtcNow.Date);

        Assert.Equal(0m, result.StepsToday);
        Assert.Null(result.HeartRateToday);
        Assert.Null(result.TrainingIntensityToday);
        Assert.False(result.StepsSpike);
        Assert.False(result.TrainingIntensityChange);
        Assert.False(result.HeartRateAnomaly);
        Assert.Null(result.StepsChangePercent);
    }

    [Fact]
    public async Task StepsSpike_Detected_WhenAboveThreshold()
    {
        await using var ctx = CreateContext();
        SeedDevice(ctx, 1, 1);
        var today = DateTime.UtcNow.Date;

        // Weekly average steps: 1000, today steps: 3000 => change +200% > threshold
        AddSteps(ctx, 1, today.AddDays(-1), 1000m);
        AddSteps(ctx, 1, today.AddDays(-2), 1000m);
        AddSteps(ctx, 1, today.AddDays(-3), 1000m);
        AddSteps(ctx, 1, today, 3000m);
        await ctx.SaveChangesAsync();

        var svc = new ActivityMonitorService(ctx);
        var result = await svc.CheckActivityChangesAsync(1, today);

        Assert.True(result.StepsSpike);
        Assert.Equal(3000m, result.StepsToday);
        Assert.True(result.StepsChangePercent.HasValue && result.StepsChangePercent.Value > ActivityThresholds.StepsSpikeThreshold);
    }

    [Fact]
    public async Task HeartRateAnomaly_Low_Detected()
    {
        await using var ctx = CreateContext();
        SeedDevice(ctx, 1, 1);
        var today = DateTime.UtcNow.Date;

        AddHeartRate(ctx, 1, today, 35m);
        await ctx.SaveChangesAsync();

        var svc = new ActivityMonitorService(ctx);
        var result = await svc.CheckActivityChangesAsync(1, today);

        Assert.True(result.HeartRateAnomaly);
        Assert.Equal(35m, result.HeartRateToday);
    }

    [Fact]
    public async Task TrainingIntensityChange_Detected()
    {
        await using var ctx = CreateContext();
        SeedDevice(ctx, 1, 1);
        var today = DateTime.UtcNow.Date;

        AddTraining(ctx, 1, today.AddDays(-1), Intensity.Moderate);
        AddTraining(ctx, 1, today.AddDays(-2), Intensity.Moderate);
        AddTraining(ctx, 1, today, Intensity.VeryHigh);
        await ctx.SaveChangesAsync();

        var svc = new ActivityMonitorService(ctx);
        var result = await svc.CheckActivityChangesAsync(1, today);

        Assert.True(result.TrainingIntensityChange);
        Assert.True(result.TrainingIntensityChangePercent.HasValue);
        Assert.True(Math.Abs(result.TrainingIntensityChangePercent!.Value) > ActivityThresholds.TrainingIntensityChangeThreshold);
    }

    private static void SeedDevice(ApplicationDbContext ctx, int userId, int deviceId)
    {
        ctx.Devices.Add(new Device { DeviceId = deviceId, UserId = userId });
    }

    private static void AddSteps(ApplicationDbContext ctx, int deviceId, DateTime day, decimal value)
    {
        ctx.TelemetrySamples.Add(new TelemetrySample
        {
            DeviceId = deviceId,
            TelemetryType = TelemetryType.Steps,
            TelemetryValue = value,
            Timestamp = day.AddHours(8)
        });
    }

    private static void AddHeartRate(ApplicationDbContext ctx, int deviceId, DateTime day, decimal value)
    {
        ctx.TelemetrySamples.Add(new TelemetrySample
        {
            DeviceId = deviceId,
            TelemetryType = TelemetryType.HeartRate,
            TelemetryValue = value,
            Timestamp = day.AddHours(8)
        });
    }

    private static void AddTraining(ApplicationDbContext ctx, int deviceId, DateTime day, Intensity intensity)
    {
        ctx.TrainingSessions.Add(new TrainingSession
        {
            DeviceId = deviceId,
            StartTime = day.AddHours(10),
            EndTime = day.AddHours(11),
            Intensity = intensity,
            DurationInMin = 60
        });
    }
}
