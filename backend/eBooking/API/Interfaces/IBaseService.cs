using API.DTOs;
using API.Models;

namespace API.Interfaces
{
    public interface IBaseService<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(int pageNumber, int pageSize);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync();
    }

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
