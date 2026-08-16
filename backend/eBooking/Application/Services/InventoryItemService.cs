using AutoMapper;
using Contracts.DTOs;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class InventoryItemService : BaseDtoService<InventoryItem, InventoryItemDto, CreateInventoryItemDto, UpdateInventoryItemDto>, IInventoryItemService
    {
        public InventoryItemService(
            IRepository<InventoryItem> repository,
            IMapper mapper,
            ILogger<InventoryItemService> logger)
            : base(repository, mapper, logger)
        {
        }
    }
}
