using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SurveyTalkService.API.Controllers.UserControllers;
using SurveyTalkService.API.Filters.ExceptionFilters;
using SurveyTalkService.BusinessLogic.DTOs.Survey.TakenResult;
using SurveyTalkService.BusinessLogic.DTOs.Survey.TakenResult.V1;
using SurveyTalkService.BusinessLogic.Enums;
using SurveyTalkService.BusinessLogic.Services.DbServices.SurveyServices;
using SurveyTalkService.BusinessLogic.Services.DbServices.UserServices;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.API.Controllers.SurveyControllers
{
    [Route("api/Survey/response")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class SurveyResponseController : ControllerBase
    {
        private ILogger<SurveyResponseController> _logger;
        private readonly SurveyResponseService _surveyResponseService;

        public SurveyResponseController(
            ILogger<SurveyResponseController> logger,
            AuthService authService,
            SurveyResponseService surveyResponseService)
        {
            _logger = logger;
            _surveyResponseService = surveyResponseService;
        }

        // GET /api/Survey/response/filter/surveys/{SurveyId}
        [HttpGet("filter/surveys/{surveyId}")]
        [Authorize(Policy = "AdminOrManagerRequired")]
        public async Task<IActionResult> GetFilterSurveyResponseSummary(int surveyId)
        {
            var questionResponseSummaryLists = await _surveyResponseService.GetFilterSurveyResponseSummary(surveyId);
            return Ok(new
            {
                Message = "Lấy kêt quả khảo sát thành công",
                QuestionResponseSummaryLists = questionResponseSummaryLists
            });
        }

        // POST /api/Survey/response/filter/surveys/{SurveyId}
        [HttpPost("filter/surveys/{surveyId}")]
        [Authorize(Policy = "CustomerRequiredOnly")]
        public async Task<IActionResult> CreateFilterSurveyResponse(int surveyId, [FromBody] SurveyTakingResponseRequestDTO surveyResponseRequestDTO,
            [FromQuery] SurveyTakenSubjectEnum? taken_subject = null)
        {
            if (!taken_subject.HasValue)
            {
                return BadRequest("TakenSubject is required.");
            }

            int userId = int.Parse(User.FindFirst("id")?.Value);
            await _surveyResponseService.TakeFilterSurveyResponse(surveyId, userId, surveyResponseRequestDTO, taken_subject.Value);
            return Ok(new
            {
                Message = "Ghi nhận kêt quả khảo sát thành công"
            });
        }

        // POST /api/Survey/response/community/surveys/{SurveyId}
        [HttpPost("community/surveys/{surveyId}")]
        [Authorize(Policy = "CustomerRequiredOnly")]
        public async Task<IActionResult> CreateCommunitySurveyResponse(int surveyId, [FromBody] SurveyTakingResponseRequestDTO surveyResponseRequestDTO, [FromQuery] SurveyTakenSubjectEnum? taken_subject = null)
        {
            if (!taken_subject.HasValue)
            {
                return BadRequest("TakenSubject is required.");
            }

            int userId = int.Parse(User.FindFirst("id")?.Value);
            var communitySurveyTakenResultResponseDTO = await _surveyResponseService.TakeCommunitySurveyResponse(surveyId, userId, surveyResponseRequestDTO, taken_subject.Value);
            return Ok(new
            {
                Message = "Ghi nhận kêt quả khảo sát thành công",
                Result = communitySurveyTakenResultResponseDTO
            });
        }

        // GET /api/Survey/response/community/surveys/{SurveyId}
        [HttpGet("community/surveys/{surveyId}")]
        [Authorize(Policy = "CustomerRequiredOnly")]
        public async Task<IActionResult> GetCommunitySurveyResponse(int surveyId)
        {
            Account account = HttpContext.Items["LoggedInAccount"] as Account;
            var questionResponseSummaryLists = await _surveyResponseService.GetCommunitySurveyResponse(surveyId, account);
            return Ok(new
            {
                Message = "Lấy kêt quả khảo sát thành công",
                QuestionResponseSummaryLists = questionResponseSummaryLists
            });
        }

        

        ///////////////////////////////////////////////////////////////

    }
}
