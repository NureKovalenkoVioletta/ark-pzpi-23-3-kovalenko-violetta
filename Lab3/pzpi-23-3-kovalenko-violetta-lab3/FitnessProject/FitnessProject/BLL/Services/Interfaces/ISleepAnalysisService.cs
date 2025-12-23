using FitnessProject.BLL.DTO.Services;

namespace FitnessProject.BLL.Services.Interfaces;

public interface ISleepAnalysisService
{
    Task<SleepQualityAnalysis> AnalyzeSleepQualityAsync(int userId, DateTime date, int days = 3);
    bool IsSleepDeprived(FitnessProject.Entities.SleepRecord record);
    bool ShouldAdjustForSleepDeprivation(SleepQualityAnalysis analysis);
}

