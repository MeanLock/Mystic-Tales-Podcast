using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.API.Filters.ExceptionFilters;
using UserService.BusinessLogic.Models.CrossService;
using UserService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using UserService.DataAccess.Data;

namespace UserService.API.Controllers.BaseControllers
{
    [Route("api/accounts")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class AccountController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly AppDbContext _appDbContext;

        public AccountController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient, AppDbContext appDbContext)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _appDbContext = appDbContext;
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _appDbContext.Roles.ToListAsync();
            return Ok(roles);
        }
    }
}
