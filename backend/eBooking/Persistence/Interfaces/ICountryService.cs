using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface ICountryService : IBaseService<CountryDto, CreateCountryDto, UpdateCountryDto>
    {
    }
}
