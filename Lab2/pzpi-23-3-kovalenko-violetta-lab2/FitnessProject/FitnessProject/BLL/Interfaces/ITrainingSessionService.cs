using FitnessProject.BLL.DTO.TrainingSession;

namespace FitnessProject.BLL.Services.Interfaces;

public interface ITrainingSessionService : IService<Entities.TrainingSession, TrainingSessionCreateDto, TrainingSessionUpdateDto, TrainingSessionResponseDto>
{
}

