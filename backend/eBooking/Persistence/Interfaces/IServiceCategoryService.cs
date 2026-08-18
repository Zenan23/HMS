using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface IServiceCategoryService : IBaseService<ServiceCategoryDto, CreateServiceCategoryDto, UpdateServiceCategoryDto>
    {
    }
}
