using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SurveyTalkService.API.Controllers.UserControllers;
using SurveyTalkService.API.Filters.ExceptionFilters;
using SurveyTalkService.BusinessLogic.DTOs.Auth;
using SurveyTalkService.BusinessLogic.DTOs.Feedback;
using SurveyTalkService.BusinessLogic.Services.DbServices.MiscServices;
using SurveyTalkService.BusinessLogic.Services.DbServices.UserServices;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.API.Controllers.MiscControllers
{
    [Route("api/Misc/platform-feedback")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class PlatformFeedbackController : ControllerBase
    {
        private ILogger<PlatformFeedbackController> _logger;
        private readonly PlatformFeedbackService _platformFeedbackService;

        public PlatformFeedbackController(
            ILogger<PlatformFeedbackController> logger,
            PlatformFeedbackService platformFeedbackService)
        {
            _logger = logger;
            _platformFeedbackService = platformFeedbackService;
        }



        ///////////////////////////////////////////////////////////////
        [HttpPost("")]
        [Authorize(Policy = "CustomerRequiredOnly")]
        public async Task<IActionResult> CreatePlatformFeedback([FromBody] PlatformFeedbackRequestDTO platformFeedback)
        {
            Account account = HttpContext.Items["LoggedInAccount"] as Account;
            await _platformFeedbackService.CreatePlatformFeedback(platformFeedback, account);
            return Ok(new
            {
                Message = "Tải lên phản hồi thành công",
            });
        }

    }
}
