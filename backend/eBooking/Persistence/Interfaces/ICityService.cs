using Contracts.DTOs;

namespace Persistence.Interfaces
{
    public interface ICityService : IBaseService<CityDto, CreateCityDto, UpdateCityDto>
    {
        Task<IEnumerable<CityDto>> GetByCountryIdAsync(int countryId);
    }
}
