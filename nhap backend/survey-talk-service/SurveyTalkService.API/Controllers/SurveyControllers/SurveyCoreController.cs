using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SurveyTalkService.API.Controllers.UserControllers;
using SurveyTalkService.API.Filters.ExceptionFilters;
using SurveyTalkService.BusinessLogic.DTOs.Survey;
using SurveyTalkService.BusinessLogic.DTOs.Survey.Publishment;
using SurveyTalkService.BusinessLogic.Enums;
using SurveyTalkService.BusinessLogic.Helpers.DateHelpers;
using SurveyTalkService.BusinessLogic.Services.DbServices.SurveyServices;
using SurveyTalkService.BusinessLogic.Services.DbServices.UserServices;
using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.API.Controllers.SurveyControllers
{
    [Route("api/Survey/core")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class SurveyCoreController : ControllerBase
    {
        // LOGGER
        private ILogger<SurveyCoreController> _logger;

        // SERVICES
        private readonly SurveyCoreService _surveyCoreService;

        // HELPERS
        private readonly DateHelper _dateHelper;

        public SurveyCoreController(
            ILogger<SurveyCoreController> logger,
            SurveyCoreService surveyCoreService,
            DateHelper dateHelper
        )
        {
            _logger = logger;
            _surveyCoreService = surveyCoreService;
            _dateHelper = dateHelper;
        }

        // GET /api/Survey/core/feature/surveys
        [HttpGet("feature/surveys")]
        [Authorize(Policy = "LoginRequired")]
        public async Task<IActionResult> GetFeatureSurveys(
            [FromQuery] int? limit = null
        )
        {
            var account = HttpContext.Items["LoggedInAccount"] as Account;
            int userId = int.Parse(User.FindFirst("id")?.Value);
            int roleId = int.Parse(User.FindFirst("role_id")?.Value);
            var surveys = await _surveyCoreService.GetCommunitySurveys(account);

            return Ok(new
            {
                BestmatchSurveys = surveys.Take(limit ?? 5).ToList(),
                BigbonusSurveys = surveys.Where(s => s.EndDate.HasValue && s.EndDate.Value >= DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone()))
                                     .OrderByDescending(s => s.CurrentSurveyRewardTracking?.RewardPrice ?? 0)
                                     .Take(limit ?? 5)
                                     .ToList(),
                FavoriteSurveys = surveys
                                .Where(s => s.SurveyTopicId != null
                                    && account?.SurveyTopicFavorites != null
                                    && account.SurveyTopicFavorites.Any(f => f.SurveyTopicId == s.SurveyTopicId))
                                .OrderByDescending(s => account.SurveyTopicFavorites
                                    .FirstOrDefault(f => f.SurveyTopicId == s.SurveyTopicId)?.FavoriteScore ?? 0)
                                .ToList()
                                .GroupBy(s => account.SurveyTopicFavorites
                                .FirstOrDefault(f => f.SurveyTopicId == s.SurveyTopicId)?.FavoriteScore ?? 0)
                                .OrderByDescending(g => g.Key)
                                .SelectMany(g => g.OrderBy(x => Guid.NewGuid()))
                                .Take(limit ?? 10)
                                .ToList()

            });

        }

        // POST /api/Survey/core/filter/surveys
        [HttpPost("filter/surveys")]
        [Authorize(Policy = "AdminOrManagerRequired")]
        public async Task<IActionResult> CreateFilterSurveys()
        {
            int userId = int.Parse(User.FindFirst("id")?.Value);
            var newSurveyId = await _surveyCoreService.CreateFilterSurvey(userId);
            return Ok(new
            {
                Message = "Tạo filter survey thành công",
                NewSurveyId = newSurveyId
            }
            );
        }

        // GET /api/Survey/core/filter/surveys
        [HttpGet("filter/surveys")]
        [Authorize(Policy = "AdminOrManagerRequired")]
        public async Task<IActionResult> GetFilterSurveys()
        {
            int roleId = int.Parse(User.FindFirst("role_id")?.Value);
            var surveys = await _surveyCoreService.GetFilterSurveys(roleId);
            return Ok(new
            {
                Surveys = surveys
            });
        }

        // GET /api/Survey/core/available-filter-survey
        [HttpGet("available-filter-survey")]
        [Authorize(Policy = "LoginRequired")]
        public async Task<IActionResult> GetAvailableFilterSurvey()
        {
            int roleId = int.Parse(User.FindFirst("role_id")?.Value);
            var surveys = await _surveyCoreService.GetFilterSurveys(roleId);
            return Ok(new
            {
                Survey = surveys.FirstOrDefault(),
            });
        }

        // GET /api/Survey/core/filter/surveys/{SurveyId}  
        [HttpGet("filter/surveys/{surveyId}")]
        [Authorize(Policy = "AdminOrManagerRequired")]
        public async Task<IActionResult> GetFilterSurvey(int surveyId)
        {
            var surveys = await _surveyCoreService.GetFilterSurveyDetail(surveyId);
            return Ok(new
            {
                Surveys = surveys
            });
        }

        // PUT /api/Survey/core/filter/surveys/{SurveyId}/publish
        [HttpPut("filter/surveys/{surveyId}/publish")]
        [Authorize(Policy = "AdminOrManagerRequired")]
        public async Task<IActionResult> PublishFilterSurvey(int surveyId)
        {
            await _surveyCoreService.PublishFilterSurvey(surveyId);
            return Ok(new
            {
                Message = "Đăng filter survey thành công",
            });
        }


        // GET /api/Survey/core/community/surveys
        [HttpGet("community/surveys")]
        [Authorize(Policy = "LoginRequired")]
        public async Task<IActionResult> GetCommunitySurveys(
            [FromQuery] SurveyAdditionalQueryEnum? additional = null,
            [FromQuery] SurveyDeadlineQueryEnum? deadline = null,
            [FromQuery] string? keywords = null,
            [FromQuery] int? limit = null
        )
        {
            var account = HttpContext.Items["LoggedInAccount"] as Account;
            int userId = int.Parse(User.FindFirst("id")?.Value);
            int roleId = int.Parse(User.FindFirst("role_id")?.Value);
            var surveys = await _surveyCoreService.GetCommunitySurveys(account);
            if (!string.IsNullOrEmpty(keywords))
            {
                surveys = surveys.Where(s => s.Title.Contains(keywords, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (deadline.HasValue)
            {
                if (deadline == SurveyDeadlineQueryEnum.OnDeadline)
                {
                    surveys = surveys.Where(s => s.AvailableTakenResultSlot != 0 && s.SurveyStatusId == 2 && s.EndDate.HasValue && s.EndDate.Value == DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone())).OrderByDescending(s => s.EndDate).Take(limit ?? 10).ToList();
                }
                else if (deadline == SurveyDeadlineQueryEnum.NearDeadline)
                {
                    surveys = surveys.Where(s => s.AvailableTakenResultSlot != 0 && s.SurveyStatusId == 2 && s.EndDate.HasValue && s.EndDate.Value > DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone())).OrderByDescending(s => s.EndDate).Take(limit ?? 10).ToList();
                }
                else if (deadline == SurveyDeadlineQueryEnum.LateForDeadline)
                {
                    surveys = surveys.Where(s => s.AvailableTakenResultSlot != 0 && s.SurveyStatusId == 3 && s.EndDate.HasValue && s.EndDate.Value < DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone())).OrderByDescending(s => s.EndDate).Take(limit ?? 10).ToList();
                }
                else
                {
                    return BadRequest("Invalid DeadlineQueryType.");
                }
            }

            if (additional.HasValue)
            {
                if (additional == SurveyAdditionalQueryEnum.BigBonus)
                {
                    surveys = surveys.Where(s => s.EndDate.HasValue && s.EndDate.Value >= DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone()))
                                     .OrderByDescending(s => s.CurrentSurveyRewardTracking?.RewardPrice ?? 0)
                                     .Take(limit ?? 10)
                                     .ToList();
                }
                else if (additional == SurveyAdditionalQueryEnum.SuitYouBest)
                {
                    surveys = surveys.Take(limit ?? 10).ToList();
                }
                else if (additional == SurveyAdditionalQueryEnum.SuitYourFavorite)
                {
                    surveys = surveys
                                .Where(s => s.SurveyTopicId != null
                                    && account?.SurveyTopicFavorites != null
                                    && account.SurveyTopicFavorites.Any(f => f.SurveyTopicId == s.SurveyTopicId))
                                .OrderByDescending(s => account.SurveyTopicFavorites
                                    .FirstOrDefault(f => f.SurveyTopicId == s.SurveyTopicId)?.FavoriteScore ?? 0)
                                .ToList()
                                .GroupBy(s => account.SurveyTopicFavorites
                                .FirstOrDefault(f => f.SurveyTopicId == s.SurveyTopicId)?.FavoriteScore ?? 0)
                                .OrderByDescending(g => g.Key)
                                .SelectMany(g => g.OrderBy(x => Guid.NewGuid()))
                                .Take(limit ?? 10)
                                .ToList();

                }
                else
                {
                    return BadRequest("Invalid AdditionalQueryType.");
                }

            }

            // nếu cả 2 đều không có trong query thì random lại danh sách survey trên
            if (!additional.HasValue && !deadline.HasValue)
            {
                Random random = new Random();
                surveys = surveys.OrderBy(s => random.Next()).Take(limit ?? 100).ToList();
            }

            return Ok(new
            {
                Surveys = surveys
            });
        }

        // POST /api/Survey/core/community/surveys
        [HttpPost("community/surveys")]
        [Authorize(Policy = "CustomerRequiredOnly")]
        public async Task<IActionResult> CreateCommunitySurvey()
        {
            int userId = int.Parse(User.FindFirst("id")?.Value);
            var newSurveyId = await _surveyCoreService.CreateCommunitySurvey(userId);
            return Ok(new
            {
                Message = "Tạo community survey thành công",
                NewSurveyId = newSurveyId
            });
        }

        // GET /api/Survey/core/level-update-community-survey
        [HttpGet("level-update-community-survey")]
        [Authorize(Policy = "CustomerRequiredOnly")]
        public async Task<IActionResult> GetLevelUpdateCommunitySurvey()
        {
            Account account = HttpContext.Items["LoggedInAccount"] as Account;
            if (account.ProgressionSurveyCount <= 0)
            {
                return BadRequest("Level hiện tại không cho phép bạn làm level update survey");
            }
            var survey = await _surveyCoreService.GetLevelUpdateCommunitySurvey(account);
            
            return Ok(new
            {
                Survey = survey
            });
        }

        // GET /api/Survey/core/community/surveys/me
        [HttpGet("community/surveys/me")]
        [Authorize(Policy = "CustomerRequiredOnly")]
        public async Task<IActionResult> GetMyCommunitySurveys(
            [FromQuery] SurveyDeadlineQueryEnum? deadline = null,
            [FromQuery] string? keywords = null,
            [FromQuery] SurveyStatusQueryEnum? status = null,
            [FromQuery] int? limit = null
        )
        {
            int userId = int.Parse(User.FindFirst("id")?.Value);
            var surveys = await _surveyCoreService.GetOwnCommunitySurveys(userId);
            if (!string.IsNullOrEmpty(keywords))
            {
                surveys = surveys.Where(s => s.Title.Contains(keywords, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (deadline.HasValue)
            {
                if (deadline == SurveyDeadlineQueryEnum.OnDeadline)
                {
                    surveys = surveys.Where(s => s.AvailableTakenResultSlot != 0 && s.SurveyStatusId == 2 && s.EndDate.HasValue && s.EndDate.Value == DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone())).OrderByDescending(s => s.EndDate).Take(limit ?? 100).ToList();
                }
                else if (deadline == SurveyDeadlineQueryEnum.NearDeadline)
                {
                    surveys = surveys.Where(s => s.AvailableTakenResultSlot != 0 && s.SurveyStatusId == 2 && s.EndDate.HasValue && s.EndDate.Value > DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone())).OrderByDescending(s => s.EndDate).Take(limit ?? 100).ToList();
                }
                else if (deadline == SurveyDeadlineQueryEnum.LateForDeadline)
                {
                    surveys = surveys.Where(s => s.AvailableTakenResultSlot != 0 && s.SurveyStatusId == 3 && s.EndDate.HasValue && s.EndDate.Value < DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone())).OrderByDescending(s => s.EndDate).Take(limit ?? 100).ToList();
                }
                else
                {
                    return BadRequest("Invalid DeadlineQueryType.");
                }
            }
            
            if (status.HasValue)
            {
                if (status == SurveyStatusQueryEnum.Editing)
                {
                    surveys = surveys.Where(s => s.SurveyStatusId == 1).OrderByDescending(s => s.SurveyPrivateData.UpdatedAt).Take(limit ?? 100).ToList();
                }
                else if (status == SurveyStatusQueryEnum.Published)
                {
                    surveys = surveys.Where(s => s.SurveyStatusId == 2).OrderByDescending(s => s.SurveyPrivateData.UpdatedAt).Take(limit ?? 100).ToList();
                }
                else if (status == SurveyStatusQueryEnum.Completed)
                {
                    surveys = surveys.Where(s => s.SurveyStatusId == 3).OrderByDescending(s => s.SurveyPrivateData.UpdatedAt).Take(limit ?? 100).ToList();
                }
                else if (status == SurveyStatusQueryEnum.Deactivated)
                {
                    surveys = surveys.Where(s => s.SurveyStatusId == 5).OrderByDescending(s => s.SurveyPrivateData.UpdatedAt).Take(limit ?? 100).ToList();
                }
                else
                {
                    return BadRequest("Invalid StatusQueryType.");
                }
            }

            return Ok(new
            {
                Surveys = surveys
            });
        }

        // PUT /api/Survey/core/community/surveys/{SurveyId}
        [HttpPut("community/surveys/{surveyId}")]
        [Authorize(Policy = "AdminOrManagerRequired")]
        public async Task<IActionResult> UpdateCommunitySurvey(int surveyId, [FromBody] JObject data)
        {
            dynamic updateData = data.ToObject<dynamic>();
            await _surveyCoreService.UpdateCommunitySurvey(surveyId, updateData);
            return Ok(new
            {
                Message = "Cập nhật community survey thành công",
            });
        }

        // GET /api/Survey/core/community/surveys/{SurveyId}
        [HttpGet("community/surveys/{surveyId}")]
        [Authorize(Policy = "LoginRequired")]
        public async Task<IActionResult> GetCommunitySurvey(int surveyId)
        {
            int userId = int.Parse(User.FindFirst("id")?.Value);
            var survey = await _surveyCoreService.GetCommunitySurveyDetail(surveyId, userId);
            return Ok(new
            {
                Survey = survey
            });
        }

        // PUT /api/Survey/core/community/surveys/{SurveyId}/unpublished
        [HttpPut("community/surveys/{surveyId}/unpublished")]
        [Authorize(Policy = "CustomerRequiredOnly")]
        public async Task<IActionResult> UnpublishedCommunitySurvey(int surveyId)
        {
            int userId = int.Parse(User.FindFirst("id")?.Value);
            await _surveyCoreService.UnpublishedCommunitySurvey(surveyId, userId);
            return Ok(new
            {
                Message = "Hủy đăng community survey thành công",
            });
        }

        // GET /api/Survey/core/survey-default-background-themes 
        [HttpGet("survey-default-background-themes")]
        [Authorize(Policy = "LoginRequired")]
        public async Task<IActionResult> GetSurveyDefaultBackgroundThemes()
        {
            var themes = await _surveyCoreService.GetSurveyDefaultBackgroundThemes();
            return Ok(new
            {
                SurveyDefaultBackgroundThemes = themes
            });
        }

        // POST /api/Survey/core/community/surveys/{SurveyId}/summary-filter-tag
        [HttpPost("community/surveys/{surveyId}/summary-filter-tag")]
        [Authorize(Policy = "CustomerRequiredOnly")]
        public async Task<IActionResult> CreateSummaryFilterTag(int surveyId, [FromBody] JObject data)
        {
            int userId = int.Parse(User.FindFirst("id")?.Value);
            SurveyTakerSegmentDTO surveyTakerSegment = data["SurveyTakerSegment"].ToObject<SurveyTakerSegmentDTO>();
            SurveyTakerSegmentSummarizedFilterTagDTO summarizedFilterTags = await _surveyCoreService.GetSurveyTakerSegmentSummarizedFilterTagAndSurveyTakingFrequencyRate(surveyId, userId, surveyTakerSegment);
            return Ok(new
            {
                Message = "Tổng hợp filter tag thành công",
                FilterTags = summarizedFilterTags.FilterTags,
                MaxKpi = summarizedFilterTags.MaxKpi,
                R = summarizedFilterTags.R
            });
        }

        // DELETE /api/Survey/core/surveys/{SurveyId}
        [HttpDelete("surveys/{surveyId}")]
        [Authorize(Policy = "LoginRequired")]
        public async Task<IActionResult> DeleteSurvey(int surveyId)
        {
            int userId = int.Parse(User.FindFirst("id")?.Value);
            Account account = HttpContext.Items["LoggedInAccount"] as Account;
            await _surveyCoreService.DeleteSurvey(surveyId, account);
            return Ok(new
            {
                Message = "Xóa survey thành công",
            });
        }

        // GET /api/Survey/core/topics
        [HttpGet("topics")]
        [Authorize(Policy = "LoginRequired")]
        public async Task<IActionResult> GetTopics()
        {
            var topics = await _surveyCoreService.GetSurveyTopics();
            return Ok(topics);
        }

        // GET /api/Survey/core/specific-topics
        [HttpGet("specific-topics")]
        [Authorize(Policy = "LoginRequired")]
        public async Task<IActionResult> GetSpecificTopics()
        {
            var specificTopics = await _surveyCoreService.GetSurveySpecificTopics();
            return Ok(specificTopics);
        }

        // GET /api/Survey/core/security-modes
        [HttpGet("security-modes")]
        [Authorize(Policy = "LoginRequired")]
        public async Task<IActionResult> GetSecurityModes()
        {
            var securityModes = await _surveyCoreService.GetSurveySecurityModes();
            return Ok(securityModes);
        }

        // GET /api/Survey/core/survey-question-types
        [HttpGet("survey-question-types")]
        [Authorize(Policy = "LoginRequired")]
        public async Task<IActionResult> GetSurveyQuestionTypes()
        {
            var questionTypes = await _surveyCoreService.GetSurveyQuestionTypes();
            return Ok(questionTypes);
        }

        // GET /api/Survey/core/survey-field-input-types
        [HttpGet("survey-field-input-types")]
        [Authorize(Policy = "LoginRequired")]
        public async Task<IActionResult> GetSurveyFieldInputTypes()
        {
            var fieldInputTypes = await _surveyCoreService.GetSurveyFieldInputTypes();
            return Ok(fieldInputTypes);
        }




    }
}
