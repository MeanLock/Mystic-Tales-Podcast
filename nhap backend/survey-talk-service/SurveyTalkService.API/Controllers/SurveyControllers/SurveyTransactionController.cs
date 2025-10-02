using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SurveyTalkService.API.Controllers.UserControllers;
using SurveyTalkService.API.Filters.ExceptionFilters;
using SurveyTalkService.BusinessLogic.DTOs.Survey.Publishment;
using SurveyTalkService.BusinessLogic.Services.DbServices.SurveyServices;
using SurveyTalkService.BusinessLogic.Services.DbServices.UserServices;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.API.Controllers.SurveyControllers
{
    [Route("api/Survey/transaction")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class SurveyTransactionController : ControllerBase
    {
        private ILogger<SurveyTransactionController> _logger;
        private readonly SurveyTransactionService _surveyTransactionService;

        public SurveyTransactionController(
            ILogger<SurveyTransactionController> logger,
            SurveyTransactionService surveyTransactionService)
        {
            _logger = logger;
            _surveyTransactionService = surveyTransactionService;
        }

        // POST /api/Survey/transaction/community/surveys/{SurveyId}/publish-price-calcular
        [HttpPost("community/surveys/{surveyId}/publish-price-calcular")]
        [Authorize(Policy = "CustomerRequiredOnly")]
        public async Task<IActionResult> CalculatePublishPrice(int surveyId, [FromBody] PublishPriceCalculationRequestDTO publishPriceCalculationRequest)
        {
            int userId = int.Parse(User.FindFirst("id")?.Value);
            decimal theoryPrice = await _surveyTransactionService.CalculateTheoryPrice(surveyId, userId, publishPriceCalculationRequest);
            return Ok(new
            {
                Message = "Tính toán giá đăng thành công",
                TheoryPrice = theoryPrice
            });
        }

        // POST /api/Survey/transaction/community/surveys/{SurveyId}/publish
        [HttpPost("community/surveys/{surveyId}/publish")]
        [Authorize(Policy = "CustomerRequiredOnly")]
        public async Task<IActionResult> PublishCommunitySurvey(int surveyId, [FromBody] CommunitySurveyPublishRequestDTO communitySurveyPublishRequestDTO)
        {
            int userId = int.Parse(User.FindFirst("id")?.Value);
            await _surveyTransactionService.PublishCommunitySurvey(surveyId, userId, communitySurveyPublishRequestDTO);
            return Ok(new
            {
                Message = "Đăng community survey thành công",
            });
        }

        // GET /api/Survey/transaction/community/survey-taken-earn/history
        [HttpGet("community/survey-taken-earn/history")]
        [Authorize(Policy = "UserTransactionReportAccess")]
        public async Task<IActionResult> GetSurveyTakenEarnHistory([FromQuery] int? page = 1, [FromQuery] int? pageSize = 10)
        {
            Account account = HttpContext.Items["LoggedInAccount"] as Account;
            var history = await _surveyTransactionService.GetSurveyTakenEarnHistory(account);
            return Ok(new
            {
                Message = "Lấy lịch sử kiếm tiền từ survey thành công",
                TransactionHistory = history
            });
        }
            

        // // POST /api/Survey/core/filter/surveys
        // [HttpPost("filter/surveys")]
        // [Authorize(Policy = "AdminOrManagerRequired")]
        // public async Task<IActionResult> CreateFilterSurveys()
        // {
        //     int userId = int.Parse(User.FindFirst("id")?.Value);
        //     var newSurveyId = await _surveyCoreService.CreateFilterSurvey(userId);
        //     return Ok(new
        //     {
        //         Message = "Tạo filter survey thành công",
        //         NewSurveyId = newSurveyId
        //     }
        //     );
        // }

        // // GET /api/Survey/core/filter/surveys
        // [HttpGet("filter/surveys")]
        // [Authorize(Policy = "AdminOrManagerRequired")]
        // public async Task<IActionResult> GetFilterSurveys()
        // {
        //     int roleId = int.Parse(User.FindFirst("role_id")?.Value);
        //     var surveys = await _surveyCoreService.GetFilterSurveys(roleId);
        //     return Ok(new
        //     {
        //         Surveys = surveys
        //     });
        // }

        // // GET /api/Survey/core/available-filter-survey
        // [HttpGet("available-filter-survey")]
        // [Authorize(Policy = "LoginRequired")]
        // public async Task<IActionResult> GetAvailableFilterSurvey()
        // {
        //     int roleId = int.Parse(User.FindFirst("role_id")?.Value);
        //     var surveys = await _surveyCoreService.GetFilterSurveys(roleId);
        //     return Ok(new
        //     {
        //         Survey = surveys.FirstOrDefault(),
        //     });
        // }

        // // GET /api/Survey/core/filter/surveys/{SurveyId}  
        // [HttpGet("filter/surveys/{surveyId}")]
        // [Authorize(Policy = "AdminOrManagerRequired")]
        // public async Task<IActionResult> GetFilterSurvey(int surveyId)
        // {
        //     var surveys = await _surveyCoreService.GetFilterSurveyDetail(surveyId);
        //     return Ok(new
        //     {
        //         Surveys = surveys
        //     });
        // }

        // // PUT /api/Survey/core/filter/surveys/{SurveyId}/publish
        // [HttpPut("filter/surveys/{surveyId}/publish")]
        // [Authorize(Policy = "AdminOrManagerRequired")]
        // public async Task<IActionResult> PublishFilterSurvey(int surveyId)
        // {
        //     await _surveyCoreService.PublishFilterSurvey(surveyId);
        //     return Ok(new
        //     {
        //         Message = "Đăng filter survey thành công",
        //     });
        // }


        // // GET /api/Survey/core/community/surveys
        // [HttpGet("community/surveys")]
        // [Authorize(Policy = "LoginRequired")]
        // public async Task<IActionResult> GetCommunitySurveys(
        //     [FromQuery] string? Keyword,
        //     [FromQuery] string? Additional,
        //     [FromQuery] string? Deadline
        // )
        // {
        //     int roleId = int.Parse(User.FindFirst("role_id")?.Value);
        //     var surveys = await _surveyCoreService.GetCommunitySurveys(roleId);
        //     return Ok(new
        //     {
        //         Surveys = surveys
        //     });
        // }

        // // POST /api/Survey/core/community/surveys
        // [HttpPost("community/surveys")]
        // [Authorize(Policy = "CustomerRequiredOnly")]
        // public async Task<IActionResult> CreateCommunitySurvey()
        // {
        //     int userId = int.Parse(User.FindFirst("id")?.Value);
        //     var newSurveyId = await _surveyCoreService.CreateCommunitySurvey(userId);
        //     return Ok(new
        //     {
        //         Message = "Tạo community survey thành công",
        //         NewSurveyId = newSurveyId
        //     });
        // }

        // // GET /api/Survey/core/community/surveys/me
        // [HttpGet("community/surveys/me")]
        // [Authorize(Policy = "CustomerRequiredOnly")]
        // public async Task<IActionResult> GetMyCommunitySurveys()
        // {
        //     int userId = int.Parse(User.FindFirst("id")?.Value);
        //     var surveys = await _surveyCoreService.GetOwnCommunitySurveys(userId);
        //     return Ok(new
        //     {
        //         Surveys = surveys
        //     });
        // }

        // // PUT /api/Survey/core/community/surveys/{SurveyId}
        // [HttpPut("community/surveys/{surveyId}")]
        // [Authorize(Policy = "AdminOrManagerRequired")]
        // public async Task<IActionResult> UpdateCommunitySurvey(int surveyId, [FromBody] JObject data)
        // {
        //     dynamic updateData = data.ToObject<dynamic>();
        //     await _surveyCoreService.UpdateCommunitySurvey(surveyId, updateData);
        //     return Ok(new
        //     {
        //         Message = "Cập nhật community survey thành công",
        //     });
        // }

        // // GET /api/Survey/core/community/surveys/{SurveyId}
        // [HttpGet("community/surveys/{surveyId}")]
        // [Authorize(Policy = "LoginRequired")]
        // public async Task<IActionResult> GetCommunitySurvey(int surveyId)
        // {
        //     int userId = int.Parse(User.FindFirst("id")?.Value);
        //     var survey = await _surveyCoreService.GetCommunitySurveyDetail(surveyId, userId);
        //     return Ok(new
        //     {
        //         Survey = survey
        //     });
        // }

        // // PUT /api/Survey/core/community/surveys/{SurveyId}/unpublished
        // [HttpPut("community/surveys/{surveyId}/unpublished")]
        // [Authorize(Policy = "CustomerRequiredOnly")]
        // public async Task<IActionResult> UnpublishedCommunitySurvey(int surveyId)
        // {
        //     int userId = int.Parse(User.FindFirst("id")?.Value);
        //     await _surveyCoreService.UnpublishedCommunitySurvey(surveyId, userId);
        //     return Ok(new
        //     {
        //         Message = "Hủy đăng community survey thành công",
        //     });
        // }

        // // GET /api/Survey/core/survey-default-background-themes 
        // [HttpGet("survey-default-background-themes")]
        // [Authorize(Policy = "LoginRequired")]
        // public async Task<IActionResult> GetSurveyDefaultBackgroundThemes()
        // {
        //     var themes = await _surveyCoreService.GetSurveyDefaultBackgroundThemes();
        //     return Ok(new
        //     {
        //         SurveyDefaultBackgroundThemes = themes
        //     });
        // }

        // // POST /api/Survey/core/community/surveys/{SurveyId}/summary-filter-tag
        // [HttpPost("community/surveys/{surveyId}/summary-filter-tag")]
        // [Authorize(Policy = "CustomerRequiredOnly")]
        // public async Task<IActionResult> CreateSummaryFilterTag(int surveyId, [FromBody] JObject data)
        // {
        //     int userId = int.Parse(User.FindFirst("id")?.Value);
        //     SurveyTakerSegmentDTO  surveyTakerSegment = data["SurveyTakerSegment"].ToObject<SurveyTakerSegmentDTO>();
        //     SurveyTakerSegmentSummarizedFilterTagDTO summarizedFilterTags = await _surveyCoreService.GetSurveyTakerSegmentSummarizedFilterTagAndSurveyTakingFrequencyRate(surveyId, userId, surveyTakerSegment);
        //     return Ok(new
        //     {
        //         Message = "Tạo filter tag thành công",
        //         FilterTags = summarizedFilterTags.FilterTags,
        //         MaxKpi = summarizedFilterTags.MaxKpi,
        //         R = summarizedFilterTags.R
        //     });
        // }

        ///////////////////////////////////////////////////////////////

    }
}
