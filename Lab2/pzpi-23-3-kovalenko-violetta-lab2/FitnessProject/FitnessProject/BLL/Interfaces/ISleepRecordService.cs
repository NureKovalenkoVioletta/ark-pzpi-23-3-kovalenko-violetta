using FitnessProject.BLL.DTO.SleepRecord;

namespace FitnessProject.BLL.Services.Interfaces;

public interface ISleepRecordService : IService<Entities.SleepRecord, SleepRecordCreateDto, SleepRecordUpdateDto, SleepRecordResponseDto>
{
}

