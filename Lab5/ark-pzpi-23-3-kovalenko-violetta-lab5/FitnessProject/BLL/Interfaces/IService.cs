namespace FitnessProject.BLL.Services.Interfaces;

public interface IService<TEntity, TCreateDto, TUpdateDto, TResponseDto>
{
    Task<TResponseDto?> GetByIdAsync(int id);
    Task<IEnumerable<TResponseDto>> GetAllAsync();
    Task<TResponseDto> CreateAsync(TCreateDto createDto);
    Task<TResponseDto> UpdateAsync(TUpdateDto updateDto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

