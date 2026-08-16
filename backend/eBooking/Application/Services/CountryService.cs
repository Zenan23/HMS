using AutoMapper;
using Contracts.DTOs;
using Microsoft.Extensions.Logging;
using Persistence.Interfaces;
using Persistence.Models;

namespace Application.Services
{
    public class CountryService : BaseDtoService<Country, CountryDto, CreateCountryDto, UpdateCountryDto>, ICountryService
    {
        public CountryService(
            IRepository<Country> repository,
            IMapper mapper,
            ILogger<CountryService> logger)
            : base(repository, mapper, logger)
        {
        }
    }
}
