using API.Attributes;
using Contracts.DTOs;
using Contracts.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Persistence.Interfaces;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CitiesController : BaseController<CityDto, CreateCityDto, UpdateCityDto>
    {
        private readonly ICityService _cityService;

        public CitiesController(
            ICityService cityService,
            ILogger<CitiesController> logger)
            : base(cityService, logger)
        {
            _cityService = cityService;
        }

        [HttpGet("country/{countryId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CityDto>>>> GetByCountryId([FromRoute] int countryId)
        {
            if (countryId <= 0)
            {
                return BadRequest(ApiResponse<IEnumerable<CityDto>>.ErrorResult("Neispravan ID države."));
            }

            var cities = await _cityService.GetByCountryIdAsync(countryId);
            return Ok(ApiResponse<IEnumerable<CityDto>>.SuccessResult(cities, "Gradovi su uspješno učitani."));
        }

        [HttpPost]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<CityDto>>> Create([FromBody] CreateCityDto createDto)
            => base.Create(createDto);

        [HttpPut("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<CityDto>>> Update([FromRoute] int id, [FromBody] UpdateCityDto updateDto)
            => base.Update(id, updateDto);

        [HttpDelete("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] int id)
            => base.Delete(id);
    }
}
