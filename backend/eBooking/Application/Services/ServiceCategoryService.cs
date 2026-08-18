using AutoMapper;
using Contracts.DTOs;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class ServiceCategoryService : BaseDtoService<ServiceCategory, ServiceCategoryDto, CreateServiceCategoryDto, UpdateServiceCategoryDto>, IServiceCategoryService
    {
        public ServiceCategoryService(
            IRepository<ServiceCategory> repository,
            IMapper mapper,
            ILogger<ServiceCategoryService> logger)
            : base(repository, mapper, logger)
        {
        }
    }
}
