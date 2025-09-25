using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SurveyTalkService.API.Controllers.UserControllers;
using SurveyTalkService.API.Filters.ExceptionFilters;
using SurveyTalkService.BusinessLogic.Services.DbServices.UserServices;

namespace SurveyTalkService.API.Controllers.ConfigurationControllers
{
    [Route("api/Configuration/system")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class SystemConfigController : ControllerBase
    {
        private ILogger<SystemConfigController> _logger;
        private readonly AuthService _authService;
        private readonly AccountService _accountService;

        public SystemConfigController(
            ILogger<SystemConfigController> logger,
            AuthService authService, 
            AccountService accountService)
        {
            _logger = logger;
            _authService = authService;
            _accountService = accountService;
        }

        

        ///////////////////////////////////////////////////////////////
        [HttpGet("")]
        public async Task<IActionResult> GetAccounts()
        {
            //var accounts = await _accountService.GetAccounts();

            return Ok(new
            {
                Accounts = ""
            });
        }
    }
}
