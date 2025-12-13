using AutoMapper;
using FitnessProject.BLL.DTO.Recommendation;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.BLL.Services;

public class RecommendationService : IRecommendationService
{
    private readonly IRecommendationRepository _repository;
    private readonly IMapper _mapper;

    public RecommendationService(IRecommendationRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<RecommendationResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<RecommendationResponseDto>(entity);
    }

    public async Task<IEnumerable<RecommendationResponseDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<RecommendationResponseDto>>(entities);
    }

    public async Task<RecommendationResponseDto> CreateAsync(RecommendationCreateDto createDto)
    {
        var entity = _mapper.Map<Recommendation>(createDto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<RecommendationResponseDto>(created);
    }

    public async Task<RecommendationResponseDto> UpdateAsync(RecommendationUpdateDto updateDto)
    {
        var entity = await _repository.GetByIdAsync(updateDto.RecommendationId);
        if (entity == null)
            throw new KeyNotFoundException($"Recommendation with ID {updateDto.RecommendationId} not found.");

        _mapper.Map(updateDto, entity);
        await _repository.UpdateAsync(entity);
        return _mapper.Map<RecommendationResponseDto>(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (!await _repository.ExistsAsync(id))
            return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _repository.ExistsAsync(id);
    }
}

