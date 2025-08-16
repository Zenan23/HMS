using API.DTOs;

namespace API.Interfaces
{
    public interface IBaseService<TDto, TCreateDto, TUpdateDto>
        where TDto : BaseEntityDto
        where TCreateDto : CreateBaseEntityDto
        where TUpdateDto : UpdateBaseEntityDto
    {
        Task<TDto?> GetByIdAsync(int id);
        Task<IEnumerable<TDto>> GetAllAsync();
        Task<IEnumerable<TDto>> GetAllAsync(int pageNumber, int pageSize);
        Task<TDto> CreateAsync(TCreateDto createDto);
        Task<bool> UpdateAsync(int id, TUpdateDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync();
    }
}
