using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SurveyTalkService.API.Filters.ExceptionFilters;
using SurveyTalkService.BusinessLogic.DTOs.Survey.Session.V2;
using SurveyTalkService.BusinessLogic.Enums;
using SurveyTalkService.BusinessLogic.Services.DbServices.SurveyServices;
using SurveyTalkService.BusinessLogic.Services.DbServices.UserServices;

namespace SurveyTalkService.API.Controllers.SurveyControllers
{
    [Route("api/Survey/session")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class SurveySessionController : ControllerBase
    {
        private ILogger<SurveySessionController> _logger;
        private readonly AuthService _authService;
        private readonly SurveySessionService _surveySessionService;

        public SurveySessionController(
            ILogger<SurveySessionController> logger,
            AuthService authService,
            AccountService accountService,
            SurveySessionService surveySessionService)
        {
            _logger = logger;
            _authService = authService;
            _surveySessionService = surveySessionService;
        }

        // GET /api/Survey/session/surveys/{SurveyId}/editing-session
        [HttpGet("surveys/{surveyId}/editing-session")]
        [Authorize(Policy = "LoginRequired")]
        public async Task<IActionResult> GetSurveyEditingSession(int surveyId, [FromQuery] int? version = null)
        {
            int userId = int.Parse(User.FindFirst("id")?.Value);
            var editingSession = await _surveySessionService.GetSurveyEditingSession(surveyId, userId, version);
            return Ok(new
            {
                Message = "Lấy thông tin phiên chỉnh sửa thành công",
                EditingAllow = true,
                EditingSession = editingSession
            });
        }

        // PUT /api/Survey/session/surveys/{SurveyId}/editing-auto-trigger
        [HttpPut("surveys/{surveyId}/editing-auto-trigger")]
        [Authorize(Policy = "LoginRequired")]
        public async Task<IActionResult> GetSurveyEditingSessionAutoTrigger(int surveyId, [FromBody] JToken data, [FromQuery] int? version = null)
        {
            int userId = int.Parse(User.FindFirst("id")?.Value);

            SurveyEditingSessionDTO surveyEditingSessionDTO = data["EditingSession"].ToObject<SurveyEditingSessionDTO>();

            var editingSession = await _surveySessionService.UpdateSurveyEditingSessionAutoTrigger(surveyId, surveyEditingSessionDTO, userId);
            return Ok(editingSession);
        }

        // PUT /api/Survey/session/surveys/{SurveyId}/editing-manual-trigger
        [HttpPut("surveys/{surveyId}/editing-manual-trigger")]
        [Authorize(Policy = "LoginRequired")]
        public async Task<IActionResult> GetSurveyEditingSessionManualTrigger(int surveyId, [FromBody] JToken data, [FromQuery] int? version = null)
        {
            int userId = int.Parse(User.FindFirst("id")?.Value);

            SurveyEditingSessionDTO surveyEditingSessionDTO = data["EditingSession"].ToObject<SurveyEditingSessionDTO>();

            var editingSession = await _surveySessionService.UpdateSurveyEditingSessionAutoTrigger(surveyId, surveyEditingSessionDTO, userId);
            return Ok(editingSession);
        }

        // GET /api/Survey/session/surveys/{SurveyId}/taking-session
        [HttpGet("surveys/{surveyId}/taking-session")]
        [Authorize(Policy = "LoginRequired")]
        public async Task<IActionResult> GetSurveyTakingSession(int surveyId, [FromQuery] int? Version = null, [FromQuery] SurveyTakingSubjectEnum? taking_subject = null)
        {
            if (!taking_subject.HasValue)
            {
                return BadRequest("TakingSubject is required.");
            }

            int userId = int.Parse(User.FindFirst("id")?.Value);
            var takingSession = await _surveySessionService.GetSurveyTakingSession(surveyId, userId, taking_subject.Value, Version);
            return Ok(new
            {
                TakingAllow = true,
                Message = "Lấy thông tin phiên trả lời thành công",
                TakingSession = takingSession
            });
        }

    }
}
