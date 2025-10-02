using Microsoft.AspNetCore.Mvc;
using UserService.API.Filters.ExceptionFilters;
using UserService.BusinessLogic.Models.CrossService;
using UserService.BusinessLogic.Services.CrossServiceServices.QueryServices;

namespace UserService.API.Controllers.BaseControllers
{
    [Route("api/accounts")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class AccountController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        public AccountController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
        }

    }
}
