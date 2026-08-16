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
    public class CountriesController : BaseController<CountryDto, CreateCountryDto, UpdateCountryDto>
    {
        public CountriesController(
            ICountryService countryService,
            ILogger<CountriesController> logger)
            : base(countryService, logger)
        {
        }

        [HttpPost]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<CountryDto>>> Create([FromBody] CreateCountryDto createDto)
            => base.Create(createDto);

        [HttpPut("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<CountryDto>>> Update([FromRoute] int id, [FromBody] UpdateCountryDto updateDto)
            => base.Update(id, updateDto);

        [HttpDelete("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] int id)
            => base.Delete(id);
    }
}
