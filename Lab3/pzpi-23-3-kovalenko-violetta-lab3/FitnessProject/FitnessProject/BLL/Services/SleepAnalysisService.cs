using FitnessProject.BLL.Configuration;
using FitnessProject.BLL.DTO.Services;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessProject.BLL.Services;

public class SleepAnalysisService : ISleepAnalysisService
{
    private readonly ApplicationDbContext _context;

    public SleepAnalysisService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SleepQualityAnalysis> AnalyzeSleepQualityAsync(int userId, DateTime date, int days = 3)
    {
        var endDate = date.Date;
        var startDate = endDate.AddDays(-(days - 1));

        var deviceIds = await _context.Devices
            .Where(d => d.UserId == userId)
            .Select(d => d.DeviceId)
            .ToListAsync();

        if (!deviceIds.Any())
        {
            return new SleepQualityAnalysis();
        }

        var records = await _context.SleepRecords
            .Where(sr => deviceIds.Contains(sr.DeviceId)
                         && sr.Date.Date >= startDate
                         && sr.Date.Date <= endDate)
            .ToListAsync();

        if (!records.Any())
        {
            return new SleepQualityAnalysis();
        }

        var averageSleepHours = records
            .Select(r => r.TotalSleepMinutes)
            .DefaultIfEmpty(0)
            .Average() / 60d;

        var deepPercents = records
            .Where(r => r.TotalSleepMinutes > 0)
            .Select(r => (decimal)r.DeepSleepMinutes / r.TotalSleepMinutes)
            .ToList();

        decimal? averageDeepPercent = deepPercents.Any() ? deepPercents.Average() : null;

        var qualityValues = records
            .Where(r => r.SleepQuality.HasValue)
            .Select(r => r.SleepQuality!.Value)
            .ToList();

        decimal? averageQuality = qualityValues.Any() ? qualityValues.Average() : null;

        var isSleepDeprived = records.Any(IsSleepDeprived);

        return new SleepQualityAnalysis
        {
            AverageSleepHours = (Decimal)averageSleepHours,
            AverageDeepSleepPercent = averageDeepPercent,
            AverageQuality = averageQuality,
            IsSleepDeprived = isSleepDeprived
        };
    }

    public bool IsSleepDeprived(FitnessProject.Entities.SleepRecord record)
    {
        var totalMinutes = record.TotalSleepMinutes;
        var deepPercent = totalMinutes > 0
            ? (decimal)record.DeepSleepMinutes / totalMinutes
            : (decimal?)null;

        var quality = record.SleepQuality;

        var totalBad = totalMinutes > 0 && totalMinutes < SleepThresholds.TotalSleepMinutesCritical;
        var deepBad = deepPercent.HasValue && deepPercent.Value < SleepThresholds.DeepSleepPercentCritical;
        var qualityBad = quality.HasValue && quality.Value < SleepThresholds.SleepQualityCritical;

        return totalBad || deepBad || qualityBad;
    }

    public bool ShouldAdjustForSleepDeprivation(SleepQualityAnalysis analysis)
    {
        if (analysis.IsSleepDeprived)
        {
            return true;
        }

        if (analysis.AverageQuality.HasValue && analysis.AverageQuality.Value < SleepThresholds.SleepQualityCritical)
        {
            return true;
        }

        return false;
    }
}

