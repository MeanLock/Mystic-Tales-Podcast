using Microsoft.AspNetCore.Mvc;
using UserService.API.Filters.ExceptionFilters;
using UserService.BusinessLogic.Helpers.FileHelpers;
using UserService.BusinessLogic.Models.CrossService;
using UserService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using UserService.Infrastructure.Services.Audio.AcoustID;

namespace UserService.API.Controllers.BaseControllers
{
    [Route("api/auth")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class AuthController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        

        public AuthController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
        }


        
    }
}
