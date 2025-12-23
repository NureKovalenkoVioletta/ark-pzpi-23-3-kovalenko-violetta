using FitnessProject.BLL.DTO.TemplateDietPlan;

namespace FitnessProject.BLL.Services.Interfaces;

public interface ITemplateDietPlanService : IService<Entities.TemplateDietPlan, TemplateDietPlanCreateDto, TemplateDietPlanUpdateDto, TemplateDietPlanResponseDto>
{
    Task<TemplateDietPlanDetailsDto?> GetTemplateDietPlanDetailsByIdAsync(int id);
}

