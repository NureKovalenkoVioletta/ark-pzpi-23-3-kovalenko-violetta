using FitnessProject.BLL.DTO.Services;

namespace FitnessProject.BLL.Services.Interfaces;

public interface IStatisticsService
{
    Task<DailyStatisticsDto> GetDailyStatisticsAsync(int userId, DateTime date);
    Task<WeeklyStatisticsDto> GetWeeklyStatisticsAsync(int userId, DateTime startDate);
    Task<WeekComparisonDto> CompareWithPreviousWeek(int userId, DateTime currentWeekStart);
}

