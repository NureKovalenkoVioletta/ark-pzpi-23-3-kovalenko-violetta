using System;
using System.Threading.Tasks;
using FitnessProject.BLL.Configuration;
using FitnessProject.BLL.DTO.Services;
using FitnessProject.BLL.Services;
using FitnessProject.Data;
using FitnessProject.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitnessProject.Tests.Services;

public class SleepAnalysisServiceTests
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
        var svc = new SleepAnalysisService(ctx);

        var result = await svc.AnalyzeSleepQualityAsync(1, DateTime.UtcNow.Date);

        Assert.Null(result.AverageDeepSleepPercent);
        Assert.Null(result.AverageQuality);
        Assert.Null(result.AverageSleepHours);
        Assert.False(result.IsSleepDeprived);
    }

    [Fact]
    public async Task NoRecords_ReturnsDefaults()
    {
        await using var ctx = CreateContext();
        ctx.Devices.Add(new Device { DeviceId = 1, UserId = 1 });
        await ctx.SaveChangesAsync();

        var svc = new SleepAnalysisService(ctx);
        var result = await svc.AnalyzeSleepQualityAsync(1, DateTime.UtcNow.Date);

        Assert.False(result.IsSleepDeprived);
        Assert.Null(result.AverageSleepHours);
    }

    [Fact]
    public async Task Analyze_AggregatesAndFlagsDeprivation()
    {
        await using var ctx = CreateContext();
        ctx.Devices.Add(new Device { DeviceId = 1, UserId = 1 });

        var today = DateTime.UtcNow.Date;
        ctx.SleepRecords.Add(new SleepRecord
        {
            DeviceId = 1,
            Date = today,
            TotalSleepMinutes = 300, // < 420 critical
            DeepSleepMinutes = 30,
            LightSleepMinutes = 200,
            AwakeMinutes = 70,
            SleepQuality = 30 // < 50 critical
        });
        ctx.SleepRecords.Add(new SleepRecord
        {
            DeviceId = 1,
            Date = today.AddDays(-1),
            TotalSleepMinutes = 500,
            DeepSleepMinutes = 70,
            LightSleepMinutes = 350,
            AwakeMinutes = 80,
            SleepQuality = 70
        });
        await ctx.SaveChangesAsync();

        var svc = new SleepAnalysisService(ctx);
        var result = await svc.AnalyzeSleepQualityAsync(1, today);

        Assert.True(result.IsSleepDeprived); // because one record is under thresholds
        Assert.True(result.AverageDeepSleepPercent.HasValue);
        Assert.True(result.AverageQuality.HasValue);
        Assert.InRange(result.AverageSleepHours ?? 0m, 6m, 7m); // (300+500)/2 = 400 min ≈ 6.67h
    }

    [Fact]
    public void IsSleepDeprived_ByThresholds()
    {
        var svc = new SleepAnalysisService(CreateContext());

        var recLowTotal = new SleepRecord { TotalSleepMinutes = (int)SleepThresholds.TotalSleepMinutesCritical - 10, DeepSleepMinutes = 60, LightSleepMinutes = 200, AwakeMinutes = 20, SleepQuality = 80 };
        Assert.True(svc.IsSleepDeprived(recLowTotal));

        var recLowDeep = new SleepRecord { TotalSleepMinutes = 480, DeepSleepMinutes = (int)(0.05m * 480), LightSleepMinutes = 400, AwakeMinutes = 20, SleepQuality = 80 };
        Assert.True(svc.IsSleepDeprived(recLowDeep));

        var recLowQuality = new SleepRecord { TotalSleepMinutes = 480, DeepSleepMinutes = 100, LightSleepMinutes = 320, AwakeMinutes = 60, SleepQuality = (int)SleepThresholds.SleepQualityCritical - 1 };
        Assert.True(svc.IsSleepDeprived(recLowQuality));

        var recNormal = new SleepRecord { TotalSleepMinutes = 480, DeepSleepMinutes = 120, LightSleepMinutes = 300, AwakeMinutes = 60, SleepQuality = 80 };
        Assert.False(svc.IsSleepDeprived(recNormal));
    }

    [Fact]
    public void ShouldAdjustForSleepDeprivation_UsesFlagsAndQuality()
    {
        var svc = new SleepAnalysisService(CreateContext());

        var deprived = new SleepQualityAnalysis { IsSleepDeprived = true };
        Assert.True(svc.ShouldAdjustForSleepDeprivation(deprived));

        var lowQuality = new SleepQualityAnalysis { IsSleepDeprived = false, AverageQuality = SleepThresholds.SleepQualityCritical - 1 };
        Assert.True(svc.ShouldAdjustForSleepDeprivation(lowQuality));

        var normal = new SleepQualityAnalysis { IsSleepDeprived = false, AverageQuality = SleepThresholds.SleepQualityCritical + 10 };
        Assert.False(svc.ShouldAdjustForSleepDeprivation(normal));
    }
}

