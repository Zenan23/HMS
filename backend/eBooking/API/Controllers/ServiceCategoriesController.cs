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
    public class ServiceCategoriesController : BaseController<ServiceCategoryDto, CreateServiceCategoryDto, UpdateServiceCategoryDto>
    {
        public ServiceCategoriesController(
            IServiceCategoryService serviceCategoryService,
            ILogger<ServiceCategoriesController> logger)
            : base(serviceCategoryService, logger)
        {
        }

        [HttpPost]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<ServiceCategoryDto>>> Create([FromBody] CreateServiceCategoryDto createDto)
            => base.Create(createDto);

        [HttpPut("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<ServiceCategoryDto>>> Update([FromRoute] int id, [FromBody] UpdateServiceCategoryDto updateDto)
            => base.Update(id, updateDto);

        [HttpDelete("{id}")]
        [AuthorizeRole(UserRole.Employee)]
        public override Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] int id)
            => base.Delete(id);
    }
}
