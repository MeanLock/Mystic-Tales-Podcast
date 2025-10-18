using Microsoft.Extensions.Logging;
using SubscriptionService.Common.AppConfigurations.App.interfaces;
using SubscriptionService.Common.AppConfigurations.FilePath.interfaces;
using SubscriptionService.DataAccess.Data;
using SubscriptionService.DataAccess.UOW;
using SubscriptionService.Common.AppConfigurations.BusinessSetting.interfaces;
using SubscriptionService.BusinessLogic.DTOs.Report;
using SubscriptionService.BusinessLogic.Enums;
using SubscriptionService.BusinessLogic.DTOs.Survey.Filters;
using SubscriptionService.BusinessLogic.Helpers.FileHelpers;
using SubscriptionService.BusinessLogic.Helpers.DateHelpers;
using SubscriptionService.BusinessLogic.Helpers.AuthHelpers;

namespace SubscriptionService.BusinessLogic.Services.DbServices.ReportServices
{
    public class SurveyStatisticsService
    {
        // LOGGER
        private readonly ILogger<SurveyStatisticsService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly ISurveyConfig _surveyConfig;

        // DB CONTEXT
        private readonly AppDbContext _appDbContext;
        private readonly PostgresDbContext _postgresDbContext;

        // HELPERS
        private readonly BcryptHelper _bcryptHelper;
        private readonly JwtHelper _jwtHelper;
        private readonly FileIOHelper _fileIOHelper;
        private readonly DateHelper _dateHelper;


        // UNIT OF WORK
        private readonly IUnitOfWork _unitOfWork;

        // REPOSITORIES


        public SurveyStatisticsService(
            ILogger<SurveyStatisticsService> logger,
            AppDbContext appDbContext,
            PostgresDbContext postgresDbContext,

            BcryptHelper bcryptHelper,
            JwtHelper jwtHelper,
            DateHelper dateHelper,
            IUnitOfWork unitOfWork,



            FileIOHelper fileIOHelper,
            IFilePathConfig filePathConfig,
            IAppConfig appConfig,
            ISurveyConfig surveyConfig
            )
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _postgresDbContext = postgresDbContext;

            _bcryptHelper = bcryptHelper;
            _jwtHelper = jwtHelper;
            _dateHelper = dateHelper;
            _unitOfWork = unitOfWork;



            _fileIOHelper = fileIOHelper;
            _filePathConfig = filePathConfig;

            _appConfig = appConfig;
            _surveyConfig = surveyConfig;
        }



        /////////////////////////////////////////////////////////////

        public async Task<CommunitySurveySummaryCountDTO> GetSurveyCommunitySummaryReport(StatisticsReportPeriodEnum reportPeriod)
        {
            try
            {
                SurveyFilterObject surveyFilterObject = new SurveyFilterObject
                {
                    SurveyTypeId = 2,
                    IsDeletedContain = false,
                    SurveyStatusIds = new List<int> { 2, 3 },
                    // IsInvalidTakenResultContain = false
                };
                var surveys = await _unitOfWork.SurveyRepository.FindByFilterObjectAsync(surveyFilterObject);

                var communitySurveySummaryCountDTO = new CommunitySurveySummaryCountDTO();

                if (reportPeriod == StatisticsReportPeriodEnum.Daily)
                {
                    DateOnly today = DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone());
                    // surveys = surveys.Where(s => s.EndDate.HasValue && s.EndDate.Value == today).ToList();
                    communitySurveySummaryCountDTO.Published = surveys.Count(s => (s.SurveyStatusTrackings
                                .OrderByDescending(sst => sst.CreatedAt)
                                .FirstOrDefault()?.SurveyStatusId ?? 1) == 2 && s.PublishedAt.HasValue && DateOnly.FromDateTime(s.PublishedAt.Value) == today);

                }
                else if (reportPeriod == StatisticsReportPeriodEnum.Weekly)
                {
                    DateOnly startOfWeek = DateOnly.FromDateTime(_dateHelper.GetDayOfWeek(_dateHelper.GetNowByAppTimeZone(), DayOfWeek.Monday));
                    DateOnly endOfWeek = startOfWeek.AddDays(6);

                    communitySurveySummaryCountDTO.Published = surveys.Count(s => (s.SurveyStatusTrackings
                                .OrderByDescending(sst => sst.CreatedAt)
                                .FirstOrDefault()?.SurveyStatusId ?? 1) == 2 && s.PublishedAt.HasValue && DateOnly.FromDateTime(s.PublishedAt.Value) >= startOfWeek && DateOnly.FromDateTime(s.PublishedAt.Value.Date) <= endOfWeek);
                }
                else if (reportPeriod == StatisticsReportPeriodEnum.Monthly)
                {
                    DateOnly startDateOfMonth = DateOnly.FromDateTime(_dateHelper.GetFirstDayOfMonthByDate(_dateHelper.GetNowByAppTimeZone()));
                    DateOnly endDateOfMonth = DateOnly.FromDateTime(_dateHelper.GetLastDayOfMonthByDate(_dateHelper.GetNowByAppTimeZone()));

                    communitySurveySummaryCountDTO.Published = surveys.Count(s => (s.SurveyStatusTrackings
                                .OrderByDescending(sst => sst.CreatedAt)
                                .FirstOrDefault()?.SurveyStatusId ?? 1) == 2 && s.PublishedAt.HasValue && DateOnly.FromDateTime(s.PublishedAt.Value) >= startDateOfMonth && DateOnly.FromDateTime(s.PublishedAt.Value.Date) <= endDateOfMonth);
                }
                else if (reportPeriod == StatisticsReportPeriodEnum.Yearly)
                {
                    DateOnly startDateOfYear = DateOnly.FromDateTime(_dateHelper.GetFirstDayOfYearByDate(_dateHelper.GetNowByAppTimeZone()));
                    DateOnly endDateOfYear = DateOnly.FromDateTime(_dateHelper.GetLastDayOfYearByDate(_dateHelper.GetNowByAppTimeZone()));

                    communitySurveySummaryCountDTO.Published = surveys.Count(s => (s.SurveyStatusTrackings
                                .OrderByDescending(sst => sst.CreatedAt)
                                .FirstOrDefault()?.SurveyStatusId ?? 1) == 2 && s.PublishedAt.HasValue && DateOnly.FromDateTime(s.PublishedAt.Value) >= startDateOfYear && DateOnly.FromDateTime(s.PublishedAt.Value.Date) <= endDateOfYear);
                }
                else
                {
                    throw new HttpRequestException("Không hỗ trợ thống kê theo thời gian này.");
                }

                communitySurveySummaryCountDTO.OnDeadline = surveys.Count(s => (s.SurveyStatusTrackings
                            .OrderByDescending(sst => sst.CreatedAt)
                            .FirstOrDefault()?.SurveyStatusId ?? 1) == 2 && s.EndDate.HasValue && s.EndDate.Value == DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone()));
                communitySurveySummaryCountDTO.NearDeadline = surveys.Count(s => (s.SurveyStatusTrackings
                            .OrderByDescending(sst => sst.CreatedAt)
                            .FirstOrDefault()?.SurveyStatusId ?? 1) == 2 && s.EndDate.HasValue && s.EndDate.Value > DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone()));
                communitySurveySummaryCountDTO.LateForDeadline = surveys.Count(s => (s.SurveyStatusTrackings
                            .OrderByDescending(sst => sst.CreatedAt)
                            .FirstOrDefault()?.SurveyStatusId ?? 1) == 3 && s.EndDate.HasValue && s.EndDate.Value < DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone()));


                foreach (var survey in surveys)
                {
                    int currentTakenResultCount = await _unitOfWork.SurveyTakenResultRepository.CountBySurveyIdAsync(survey.Id, false);
                    int surveyStatusId = survey.SurveyStatusTrackings
                            .OrderByDescending(sst => sst.CreatedAt)
                            .FirstOrDefault()?.SurveyStatusId ?? 1;
                    Console.WriteLine($"Survey ID: {survey.Id}, Current Taken Result Count: {surveyStatusId}");

                    if (surveyStatusId == 3)
                    {
                        int availableTakenResultSlot = (survey.Kpi ?? 0) - currentTakenResultCount;
                        communitySurveySummaryCountDTO.Achieved += 1;
                    }
                }

                return communitySurveySummaryCountDTO;

            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lấy báo cáo thống kê cộng đồng khảo sát thất bại, lí do: " + ex.Message);
            }


        }

        


        // ///////////////////////////////////////////////////////////



    }
}
