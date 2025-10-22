using Microsoft.Extensions.Logging;
using TransactionService.Common.AppConfigurations.App.interfaces;
using TransactionService.Common.AppConfigurations.FilePath.interfaces;
using TransactionService.DataAccess.Data;
using TransactionService.DataAccess.UOW;
using TransactionService.DataAccess.Entities;
using TransactionService.DataAccess.Repositories.interfaces;
using TransactionService.BusinessLogic.Enums;
using Microsoft.EntityFrameworkCore;
using TransactionService.Common.AppConfigurations.BusinessSetting.interfaces;
using TransactionService.BusinessLogic.DTOs.Report;
using TransactionService.BusinessLogic.DTOs.Feedback;
using TransactionService.BusinessLogic.Helpers.AuthHelpers;
using TransactionService.BusinessLogic.Helpers.FileHelpers;
using TransactionService.BusinessLogic.Helpers.DateHelpers;

namespace TransactionService.BusinessLogic.Services.DbServices.ReportServices
{
    public class UserStatisticsService
    {
        // LOGGER
        private readonly ILogger<UserStatisticsService> _logger;

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
        private readonly IGenericRepository<PlatformFeedback> _platformFeedbackGenericRepository;


        public UserStatisticsService(
            ILogger<UserStatisticsService> logger,
            AppDbContext appDbContext,
            PostgresDbContext postgresDbContext,

            BcryptHelper bcryptHelper,
            JwtHelper jwtHelper,
            DateHelper dateHelper,
            IUnitOfWork unitOfWork,

            IGenericRepository<PlatformFeedback> platformFeedbackGenericRepository,


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

            _platformFeedbackGenericRepository = platformFeedbackGenericRepository;

            _fileIOHelper = fileIOHelper;
            _filePathConfig = filePathConfig;


            _appConfig = appConfig;
            _surveyConfig = surveyConfig;
        }


        /////////////////////////////////////////////////////////////

        public async Task<AccountRegistrationSummaryCountDTO> GetAccountRegistrationSummaryReport(StatisticsReportPeriodEnum reportPeriod)
        {
            try
            {

                if (reportPeriod == StatisticsReportPeriodEnum.Daily)
                {
                    DateOnly today = DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone());
                    DateOnly previousDay = today.AddDays(-1);
                    var registrationCount = await _unitOfWork.AccountRepository.CountAccountRegistrationByPeriodAsync(today, today);
                    var previousRegistrationCount = await _unitOfWork.AccountRepository.CountAccountRegistrationByPeriodAsync(previousDay, previousDay);
                    return new AccountRegistrationSummaryCountDTO
                    {
                        NewRegistrationCount = registrationCount,
                        PercentChange = previousRegistrationCount == 0 ? 100 : Math.Round(((double)(registrationCount - previousRegistrationCount) / (double)previousRegistrationCount * 100), 2, MidpointRounding.AwayFromZero),
                    };

                }
                else if (reportPeriod == StatisticsReportPeriodEnum.Weekly)
                {
                    DateOnly today = DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone());
                    DateOnly startOfWeek = DateOnly.FromDateTime(_dateHelper.GetDayOfWeek(_dateHelper.GetNowByAppTimeZone(), DayOfWeek.Monday));
                    DateOnly endOfWeek = startOfWeek.AddDays(6);
                    var registrationCount = await _unitOfWork.AccountRepository.CountAccountRegistrationByPeriodAsync(startOfWeek, endOfWeek);
                    var previousStartOfWeek = startOfWeek.AddDays(-7);
                    var previousEndOfWeek = endOfWeek.AddDays(-7);
                    var previousRegistrationCount = await _unitOfWork.AccountRepository.CountAccountRegistrationByPeriodAsync(previousStartOfWeek, previousEndOfWeek);
                    return new AccountRegistrationSummaryCountDTO
                    {
                        NewRegistrationCount = registrationCount,
                        PercentChange = previousRegistrationCount == 0 ? 100 : Math.Round(((double)(registrationCount - previousRegistrationCount) / (double)previousRegistrationCount * 100), 2, MidpointRounding.AwayFromZero),
                    };

                }
                else if (reportPeriod == StatisticsReportPeriodEnum.Monthly)
                {
                    DateOnly today = DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone());
                    DateOnly startDateOfMonth = DateOnly.FromDateTime(_dateHelper.GetFirstDayOfMonthByDate(_dateHelper.GetNowByAppTimeZone()));
                    DateOnly endDateOfMonth = DateOnly.FromDateTime(_dateHelper.GetLastDayOfMonthByDate(_dateHelper.GetNowByAppTimeZone()));

                    var previousStartDateOfMonth = startDateOfMonth.AddMonths(-1);
                    var previousEndDateOfMonth = endDateOfMonth.AddMonths(-1);

                    var registrationCount = await _unitOfWork.AccountRepository.CountAccountRegistrationByPeriodAsync(startDateOfMonth, endDateOfMonth);
                    var previousRegistrationCount = await _unitOfWork.AccountRepository.CountAccountRegistrationByPeriodAsync(previousStartDateOfMonth, previousEndDateOfMonth);
                    return new AccountRegistrationSummaryCountDTO
                    {
                        NewRegistrationCount = registrationCount,
                        PercentChange = previousRegistrationCount == 0 ? 100 : Math.Round(((double)(registrationCount - previousRegistrationCount) / (double)previousRegistrationCount * 100), 2, MidpointRounding.AwayFromZero),
                    };

                }
                else if (reportPeriod == StatisticsReportPeriodEnum.Yearly)
                {
                    DateOnly today = DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone());
                    DateOnly startDateOfYear = DateOnly.FromDateTime(_dateHelper.GetFirstDayOfYearByDate(_dateHelper.GetNowByAppTimeZone()));
                    DateOnly endDateOfYear = DateOnly.FromDateTime(_dateHelper.GetLastDayOfYearByDate(_dateHelper.GetNowByAppTimeZone()));

                    var previousStartDateOfYear = startDateOfYear.AddYears(-1);
                    var previousEndDateOfYear = endDateOfYear.AddYears(-1);

                    var registrationCount = await _unitOfWork.AccountRepository.CountAccountRegistrationByPeriodAsync(startDateOfYear, endDateOfYear);
                    var previousRegistrationCount = await _unitOfWork.AccountRepository.CountAccountRegistrationByPeriodAsync(previousStartDateOfYear, previousEndDateOfYear);
                    return new AccountRegistrationSummaryCountDTO
                    {
                        NewRegistrationCount = registrationCount,
                        PercentChange = previousRegistrationCount == 0 ? 100 : Math.Round(((double)(registrationCount - previousRegistrationCount) / (double)previousRegistrationCount * 100), 2, MidpointRounding.AwayFromZero),
                    };
                }
                else
                {
                    throw new HttpRequestException("không hỗ trợ thống kê theo thời gian này.");
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lấy báo cáo đăng ký tài khoản thất bại, lí do: " + ex.Message);
            }
        }

        public async Task<List<PlatformFeedbackDTO>> GetPlatformFeedbackReport()
        {
            try
            {
                var feedbacks = await _platformFeedbackGenericRepository.FindAll().ToListAsync();
                return feedbacks.Select(feedback => new PlatformFeedbackDTO
                {
                    AccountId = feedback.AccountId,
                    RatingScore = feedback.RatingScore,
                    Comment = feedback.Comment,
                    CreatedAt = feedback.CreatedAt,
                    UpdatedAt = feedback.UpdatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lấy báo cáo phản hồi nền tảng thất bại, lí do: " + ex.Message);
            }
        }

        


        // ///////////////////////////////////////////////////////////



    }
}
