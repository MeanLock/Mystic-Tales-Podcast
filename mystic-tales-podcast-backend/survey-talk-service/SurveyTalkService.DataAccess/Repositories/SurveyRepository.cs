using SurveyTalkService.DataAccess.Data;
using SurveyTalkService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using SurveyTalkService.DataAccess.Entities;
using SurveyTalkService.BusinessLogic.DTOs.Survey.Filters;
using SurveyTalkService.Common.AppConfigurations.App;
using SurveyTalkService.Common.AppConfigurations.App.interfaces;

namespace SurveyTalkService.DataAccess.Repositories
{
    public class SurveyRepository : ISurveyRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAppConfig _appConfig;


        public SurveyRepository(AppDbContext appDbContext, IAppConfig appConfig)
        {
            _appConfig = appConfig;
            _appDbContext = appDbContext;
        }

        public async Task<SurveyStatusTracking> GetLatestSurveyStatusTrackingBySurveyIdAsync(int surveyId)
        {
            return await _appDbContext.SurveyStatusTrackings
                .Where(sst => sst.SurveyId == surveyId)
                .OrderByDescending(sst => sst.CreatedAt)
                .FirstOrDefaultAsync();
        }

        // truy vấn nhanh
        public async Task<Survey> FindByIdAsync(int id)
        {
            var survey = await _appDbContext.Surveys
                .Include(s => s.Requester)
                .Include(s => s.SurveyType)
                .Include(s => s.SecurityMode)
                .Include(s => s.SurveyMarkets) // có
                .FirstOrDefaultAsync(s => s.Id == id);
            return survey;
        }

        public async Task<Survey> FindByIdAndFilterObjectAsync(int id, SurveyFilterObject surveyFilterObject)
        {
            // Console.WriteLine($"Finding survey by ID: {id} with filter: {surveyFilterObject.ToString()}");
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(_appConfig.TIME_ZONE));
            var today = DateOnly.FromDateTime(now);
            var survey = await _appDbContext.Surveys
                .Include(s => s.Requester)
                .Include(s => s.SurveyType)
                .Include(s => s.SurveyTakerSegment)
                .Include(s => s.SurveyTopic)
                .Include(s => s.SurveySpecificTopic)
                .ThenInclude(s => s.SurveyTopic)
                .Include(s => s.SecurityMode)
                .Include(s => s.SurveyStatusTrackings)
                .Include(s => s.SurveyRewardTrackings)
                // .Include(s => s.SurveyTakenResults)
                // .ThenInclude(srt => srt.Taker)
                // .Include(s => s.SurveyTakenResults)
                // .ThenInclude(srt => srt.SurveyResponses)
                // .ThenInclude(sr => sr.SurveyMarketResponseVersions)
                .Include(s => s.SurveyMarkets) // có
                .Include(sm => sm.SurveyMarketVersionStatusTrackings) // có
                .Include(s => s.SurveyQuestions) // có
                .ThenInclude(sqr => sqr.SurveyOptions) // có
                .Include(s => s.SurveyQuestions) // có
                .ThenInclude(sqr => sqr.QuestionType)
                // .Include(s => s.SurveyTagFilters)
                .FirstOrDefaultAsync(s =>
                    s.Id == id
                    &&
                    (surveyFilterObject.RequesterId == null || s.RequesterId == surveyFilterObject.RequesterId)
                    &&
                    (surveyFilterObject.SurveyTypeId == null || s.SurveyTypeId == surveyFilterObject.SurveyTypeId)
                    &&
                    (surveyFilterObject.IsAvailable == null || s.IsAvailable == surveyFilterObject.IsAvailable)
                    &&
                    (surveyFilterObject.IsDeletedContain == null ||
                        (surveyFilterObject.IsDeletedContain == true) ||
                        (surveyFilterObject.IsDeletedContain == false && s.DeletedAt == null))
                    &&
                    (surveyFilterObject.IsEndDateExceededContain == null ||
                        (surveyFilterObject.IsEndDateExceededContain == true) ||
                        (surveyFilterObject.IsEndDateExceededContain == false && s.EndDate >= today))
                );
            if (survey == null)
            {
                return null;
            }

            if (surveyFilterObject.Version != null)
            {
                survey.SurveyMarkets = survey.SurveyMarkets
                    .Where(sm => sm.Version == surveyFilterObject.Version)
                    .ToList();

                if (!survey.SurveyMarkets.Any())
                {
                    return null;
                }

                survey.SurveyMarketVersionStatusTrackings = survey.SurveyMarketVersionStatusTrackings
                    .Where(smvst => smvst.Version == surveyFilterObject.Version)
                    .ToList();

                if (!survey.SurveyMarketVersionStatusTrackings.Any())
                {
                    return null;
                }

                // Bổ sung kiểm tra SurveyMarketVersionStatusIds giống concept SurveyStatusIds
                if (surveyFilterObject.SurveyMarketVersionStatusIds != null && surveyFilterObject.SurveyMarketVersionStatusIds.Count > 0)
                {
                    var latestMarketStatus = survey.SurveyMarketVersionStatusTrackings
                        .OrderByDescending(smvst => smvst.CreatedAt)
                        .FirstOrDefault();
                    bool marketStatusMatch = latestMarketStatus != null && surveyFilterObject.SurveyMarketVersionStatusIds.Contains(latestMarketStatus.SurveyStatusId);
                    if (!marketStatusMatch)
                    {
                        return null;
                    }
                }

                survey.SurveyQuestions = survey.SurveyQuestions
                    .Where(q => q.Version == surveyFilterObject.Version)
                    .ToList();
            }

            // if (surveyFilterObject.IsInvalidTakenResultContain == false && survey.SurveyTakenResults != null)
            // {
            //     survey.SurveyTakenResults = survey.SurveyTakenResults.Where(tr => tr.IsValid == true).ToList();
            // }
            var latestStatus = await GetLatestSurveyStatusTrackingBySurveyIdAsync(survey.Id);
            bool statusMatch = surveyFilterObject.SurveyStatusIds == null ||
                               surveyFilterObject.SurveyStatusIds.Count == 0 ||
                               (latestStatus != null && surveyFilterObject.SurveyStatusIds.Contains(latestStatus.SurveyStatusId));


            return statusMatch == true ? survey : null;
        }

        public async Task<IEnumerable<Survey>> FindByFilterObjectAsync(SurveyFilterObject surveyFilterObject)
        {
            // Console.WriteLine("Finding surveys by filter object..., "+ surveyFilterObject.ToString());
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(_appConfig.TIME_ZONE));
            var today = DateOnly.FromDateTime(now);
            var surveys = await _appDbContext.Surveys
                .Include(s => s.Requester)
                .Include(s => s.SurveyTakerSegment)
                .Include(s => s.SurveyType)
                .Include(s => s.SurveyTopic)
                .Include(s => s.SurveySpecificTopic)
                .Include(s => s.SecurityMode)
                .Include(s => s.SurveyStatusTrackings)
                .Include(s => s.SurveyRewardTrackings)
                // .Include(s => s.SurveyTakenResults)
                // .ThenInclude(srt => srt.Taker)
                .Include(s => s.SurveyMarkets)
                .Include(sm => sm.SurveyMarketVersionStatusTrackings)
                .Include(s => s.SurveyQuestions)
                .ToListAsync();
            var result = new List<Survey>();
            foreach (var survey in surveys)
            {
                if (!((surveyFilterObject.RequesterId == null || survey.RequesterId == surveyFilterObject.RequesterId)
                    && (surveyFilterObject.SurveyTypeId == null || survey.SurveyTypeId == surveyFilterObject.SurveyTypeId)
                    && (surveyFilterObject.IsAvailable == null || survey.IsAvailable == surveyFilterObject.IsAvailable)
                    && (surveyFilterObject.IsDeletedContain == null ||
                        (surveyFilterObject.IsDeletedContain == true) ||
                        (surveyFilterObject.IsDeletedContain == false && survey.DeletedAt == null))
                    && (surveyFilterObject.IsEndDateExceededContain == null ||
                        (surveyFilterObject.IsEndDateExceededContain == true) ||
                        (surveyFilterObject.IsEndDateExceededContain == false && survey.EndDate >= today))
                ))
                {
                    continue;
                }

                if (surveyFilterObject.Version != null)
                {
                    survey.SurveyMarkets = survey.SurveyMarkets
                        .Where(sm => sm.Version == surveyFilterObject.Version)
                        .ToList();

                    if (!survey.SurveyMarkets.Any())
                    {
                        return null;
                    }

                    survey.SurveyMarketVersionStatusTrackings = survey.SurveyMarketVersionStatusTrackings
                        .Where(smvst => smvst.Version == surveyFilterObject.Version)
                        .ToList();

                    if (!survey.SurveyMarketVersionStatusTrackings.Any())
                    {
                        return null;
                    }
                    survey.SurveyQuestions = survey.SurveyQuestions
                        .Where(q => q.Version == surveyFilterObject.Version)
                        .ToList();
                }

                // if (surveyFilterObject.IsInvalidTakenResultContain == false && survey.SurveyTakenResults != null)
                // {
                //     survey.SurveyTakenResults = survey.SurveyTakenResults.Where(tr => tr.IsValid == true).ToList();
                // }
                var latestStatus = await GetLatestSurveyStatusTrackingBySurveyIdAsync(survey.Id);
                bool statusMatch = surveyFilterObject.SurveyStatusIds == null ||
                                   surveyFilterObject.SurveyStatusIds.Count == 0 ||
                                   (latestStatus != null && surveyFilterObject.SurveyStatusIds.Contains(latestStatus.SurveyStatusId));
                if (statusMatch == true)
                {
                    result.Add(survey);
                }
            }
            return result;
        }

        // public async Task<decimal> GetProfitByTypeAndPeriodAsync(int surveyTypeId, DateOnly startDate, DateOnly endDate)
        // {
        //     var surveys = await _appDbContext.Surveys
        //         .Where(s => s.StartDate >= startDate && s.EndDate <= endDate && s.ProfitPrice != null && s.SurveyTypeId == surveyTypeId)
        //         .ToListAsync();

        //     decimal totalProfit = 0;

        //     foreach (var survey in surveys)
        //     {
        //         if (survey.ProfitPrice.HasValue)
        //         {
        //             totalProfit += survey.ProfitPrice.Value;
        //         }
        //     }
        //     return totalProfit;
        // }
    }
}
