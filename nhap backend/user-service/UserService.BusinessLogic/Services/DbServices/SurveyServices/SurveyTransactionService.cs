using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserService.Common.AppConfigurations.App.interfaces;
using UserService.Common.AppConfigurations.FilePath.interfaces;
using UserService.DataAccess.Data;
using UserService.DataAccess.UOW;
using UserService.DataAccess.Repositories.interfaces;
using UserService.DataAccess.Entities;
using UserService.BusinessLogic.DTOs.Survey.Publishment;
using UserService.BusinessLogic.DTOs.Survey.Filters;
using System.Security.Cryptography;
using Pgvector;
using UserService.DataAccess.Entities.postgres;
using UserService.BusinessLogic.DTOs.Transaction;
using System.Linq.Expressions;
using UserService.BusinessLogic.Helpers.AuthHelpers;
using UserService.BusinessLogic.Helpers.FileHelpers;
using UserService.BusinessLogic.Helpers.DateHelpers;

namespace UserService.BusinessLogic.Services.DbServices.SurveyServices
{
    public class SurveyTransactionService
    {
        // LOGGER
        private readonly ILogger<SurveyTransactionService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        private readonly IFilePathConfig _filePathConfig;

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
        private readonly IGenericRepository<Account> _accountGenericRepository;
        private readonly IGenericRepository<Survey> _surveyGenericRepository;
        private readonly IGenericRepository<SurveyTimeRateConfig> _surveyTimeRateConfigGenericRepository;
        private readonly IGenericRepository<SurveyTakerSegment> _surveyTakerSegmentGenericRepository;
        private readonly IGenericRepository<SurveyTagFilter> _surveyTagFilterGenericRepository;
        private readonly IGenericRepository<SurveyStatusTracking> _surveyStatusTrackingGenericRepository;
        private readonly IGenericRepository<SurveyRewardTracking> _surveyRewardTrackingGenericRepository;
        private readonly IGenericRepository<SurveyCommunityTransaction> _surveyCommunityTransactionGenericRepository;

        public SurveyTransactionService(
            ILogger<SurveyTransactionService> logger,
            AppDbContext appDbContext,
            PostgresDbContext postgresDbContext,
            BcryptHelper bcryptHelper,
            JwtHelper jwtHelper,
            IUnitOfWork unitOfWork,

            IGenericRepository<Account> accountGenericRepository,
            IGenericRepository<Survey> surveyGenericRepository,
            IGenericRepository<SurveyTimeRateConfig> surveyTimeRateConfigGenericRepository,
            IGenericRepository<SurveyTakerSegment> surveyTakerSegmentGenericRepository,
            IGenericRepository<SurveyTagFilter> surveyTagFilterGenericRepository,
            IGenericRepository<SurveyStatusTracking> surveyStatusTrackingGenericRepository,
            IGenericRepository<SurveyRewardTracking> surveyRewardTrackingGenericRepository,
            IGenericRepository<SurveyCommunityTransaction> surveyCommunityTransactionGenericRepository,

            FileIOHelper fileIOHelper,
            DateHelper dateHelper,
            IFilePathConfig filePathConfig,
            IAppConfig appConfig
            )
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _postgresDbContext = postgresDbContext;
            _bcryptHelper = bcryptHelper;
            _jwtHelper = jwtHelper;
            _unitOfWork = unitOfWork;

            _accountGenericRepository = accountGenericRepository;
            _surveyGenericRepository = surveyGenericRepository;
            _surveyTimeRateConfigGenericRepository = surveyTimeRateConfigGenericRepository;
            _surveyTakerSegmentGenericRepository = surveyTakerSegmentGenericRepository;
            _surveyTagFilterGenericRepository = surveyTagFilterGenericRepository;
            _surveyStatusTrackingGenericRepository = surveyStatusTrackingGenericRepository;
            _surveyRewardTrackingGenericRepository = surveyRewardTrackingGenericRepository;
            _surveyCommunityTransactionGenericRepository = surveyCommunityTransactionGenericRepository;

            _fileIOHelper = fileIOHelper;
            _filePathConfig = filePathConfig;
            _dateHelper = dateHelper;

            _appConfig = appConfig;
        }



        // ///////////////////////////////////////////////////////////

        public async Task<decimal> CalculateTheoryPrice(int surveyId, int userId, PublishPriceCalculationRequestDTO publishPriceCalculationRequest)
        {
            // Validate the request
            if (publishPriceCalculationRequest.Kpi <= 0 || publishPriceCalculationRequest.RS <= 0)
            {
                throw new HttpRequestException("Invalid MaxKpi or RS value.");
            }
            try
            {
                SurveyFilterObject surveyFilterObject = new SurveyFilterObject
                {
                    RequesterId = userId,
                    SurveyTypeId = 2, // Community survey
                    IsDeletedContain = false,
                    // IsInvalidTakenResultContain = false,
                    IsAvailable = false, // Only get unpublished surveys
                    SurveyStatusIds = new List<int> { 1 } // Only get surveys with status "Draft"
                };
                var survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                if (survey == null)
                {
                    throw new HttpRequestException("survey với id " + surveyId.ToString() + " không tồn tại hoặc không thể thể tiếp nhận yêu cầu này ở thời điểm hiện tại");
                }
                var systemConfigProfile = await _unitOfWork.SystemConfigProfileRepository.FindActiveProfileAsync();

                decimal kpi = (decimal)publishPriceCalculationRequest.Kpi;
                decimal questionPriceSum = survey.SurveyQuestions
                    // .Where(q => q.QuestionTypeId != 1) // Exclude "Text" question type
                    .Sum(q => q.QuestionType.Price);
                decimal TimeRate = publishPriceCalculationRequest.RS == 1 ? 1 : (decimal)systemConfigProfile.SurveyTimeRateConfigs
                    .Where(config => publishPriceCalculationRequest.RS >= config.MinDurationRate && publishPriceCalculationRequest.RS <= config.MaxDurationRate) // Community survey
                    .Select(config => config.Rate)
                    .FirstOrDefault();
                decimal securityRate = (decimal)systemConfigProfile.SurveySecurityModeConfigs
                    .Where(config => config.SurveySecurityModeId == survey.SecurityModeId)
                    .Select(config => config.Rate)
                    .FirstOrDefault();

                // in tất cả các thành phần 
                Console.WriteLine($"KPI: {kpi}, Question Price Sum: {questionPriceSum}, Time Rate: {TimeRate}, Security Rate: {securityRate}");

                decimal theoryPrice = kpi * questionPriceSum * TimeRate * securityRate;


                return theoryPrice;

            }
            catch (Exception ex)
            {

                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Tính toán giá lý thuyết thất bại, lỗi: " + ex.Message);
            }


        }


        public async Task PublishCommunitySurvey(int surveyId, int userId, CommunitySurveyPublishRequestDTO communitySurveyPublishRequestDTO)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    SurveyFilterObject surveyFilterObject = new SurveyFilterObject
                    {
                        RequesterId = userId,
                        SurveyTypeId = 2, // Community survey
                        IsDeletedContain = false,
                        // IsInvalidTakenResultContain = false,
                        IsAvailable = false, // Only get unpublished surveys
                        SurveyStatusIds = new List<int> { 1 } // Only get surveys with status "Draft"
                    };
                    var survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                    if (survey == null)
                    {
                        throw new HttpRequestException("survey với id " + surveyId.ToString() + " không tồn tại hoặc không thể thể tiếp nhận yêu cầu này ở thời điểm hiện tại");
                    }
                    var systemConfigProfile = await _unitOfWork.SystemConfigProfileRepository.FindActiveProfileAsync();


                    // 1.các loại price(có cả extra price)
                    // 2.update start date, end date, kpi, theory price, extra price, publish at (giống start date) , is available
                    // 3.lưu SurveyTakerSegment
                    // 4.lưu SurveyTagFilter + SurveyEmbeddingVectorTagFilter
                    // 5.thêm survey reward tracking đầu tiên(có cả extra price)
                    // 5.thêm survey reward tracking đầu tiên(có cả extra price)

                    // Tính toán tiền
                    // profitPrice : decimal (18,2) Theory * ProfitRate (SurveyGeneralConfig) 
                    // allocBaseAmount : decimal (18,2) (Theory + extraPrice - profitPrice ) * basePriceAllocationRate (SurveyGeneralConfig)
                    // allocTimeAmount  : decimal (18,2) (Theory + extraPrice - profitPrice ) * timePriceAllocationRate (SurveyGeneralConfig)
                    // allocLevelAmount : decimal (18,2) (Theory + extraPrice - profitPrice ) * levelPriceAllocationRate (SurveyGeneralConfig)
                    // maxXp : int số câu hỏi của survey * xpPerQuestion (SurveyGeneralConfig)
                    // takerBaseRewardPrice : allocBaseAmount / kpi


                    Console.WriteLine($"1111111111111111111111111: {systemConfigProfile.SurveyGeneralConfig.PublishProfitRate}");
                    decimal profitPrice = communitySurveyPublishRequestDTO.TheoryPrice * (decimal)systemConfigProfile.SurveyGeneralConfig.PublishProfitRate;
                    decimal allocBaseAmount = (communitySurveyPublishRequestDTO.TheoryPrice + communitySurveyPublishRequestDTO.ExtraPrice - profitPrice) * (decimal)systemConfigProfile.SurveyGeneralConfig.BasePriceAllocationRate;
                    decimal allocTimeAmount = (communitySurveyPublishRequestDTO.TheoryPrice + communitySurveyPublishRequestDTO.ExtraPrice - profitPrice) * (decimal)systemConfigProfile.SurveyGeneralConfig.TimePriceAllocationRate;
                    decimal allocLevelAmount = (communitySurveyPublishRequestDTO.TheoryPrice + communitySurveyPublishRequestDTO.ExtraPrice - profitPrice) * (decimal)systemConfigProfile.SurveyGeneralConfig.LevelPriceAllocationRate;
                    int maxXp = survey.SurveyQuestions.Count * systemConfigProfile.SurveyGeneralConfig.XpPerQuestion;
                    decimal takerBaseRewardPrice = allocBaseAmount / communitySurveyPublishRequestDTO.Kpi;
                    decimal totalPrice = communitySurveyPublishRequestDTO.TheoryPrice + communitySurveyPublishRequestDTO.ExtraPrice;
                    // cập nhật balance của account

                    var account = await _unitOfWork.AccountRepository.FindByIdAsync(userId);
                    if (account.Balance < totalPrice)
                    {
                        throw new HttpRequestException("Tài khoản không đủ tiền để đăng tải khảo sát cộng đồng, balance: " + account.Balance + ", total price: " + totalPrice);
                    }
                    account.Balance -= totalPrice;
                    await _accountGenericRepository.UpdateAsync(account.Id, account);

                    // Lưu PaymentHistory
                    var surveyCommunityTransaction = new SurveyCommunityTransaction
                    {
                        AccountId = userId,
                        Amount = totalPrice,
                        SurveyId = surveyId,
                        Profit = profitPrice,
                        TransactionStatusId = 2,
                        TransactionTypeId = 1,

                    };
                    await _surveyCommunityTransactionGenericRepository.CreateAsync(surveyCommunityTransaction);


                    // Cập nhật survey
                    survey.ProfitPrice = profitPrice;
                    survey.AllocBaseAmount = allocBaseAmount;
                    survey.AllocTimeAmount = allocTimeAmount;
                    survey.AllocLevelAmount = allocLevelAmount;
                    survey.MaxXp = maxXp;
                    survey.TakerBaseRewardPrice = takerBaseRewardPrice;
                    survey.IsAvailable = true;
                    survey.StartDate = DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone());
                    survey.EndDate = DateOnly.FromDateTime(communitySurveyPublishRequestDTO.EndDate);
                    survey.Kpi = communitySurveyPublishRequestDTO.Kpi;
                    survey.TheoryPrice = communitySurveyPublishRequestDTO.TheoryPrice;
                    survey.ExtraPrice = communitySurveyPublishRequestDTO.ExtraPrice;
                    survey.PublishedAt = _dateHelper.GetNowByAppTimeZone();
                    await _surveyGenericRepository.UpdateAsync(survey.Id, survey);
                    Console.WriteLine($"\n\nCập nhật survey thành công: {survey.Id}, Profit Price: {profitPrice}, Alloc Base Amount: {allocBaseAmount}, Alloc Time Amount: {allocTimeAmount}, Alloc Level Amount: {allocLevelAmount}");

                    // Lưu SurveyTakerSegment
                    if (survey.SurveyTakerSegment == null)
                    {
                        var surveyTakerSegment = communitySurveyPublishRequestDTO.SurveyTakerSegment;
                        var newSurveyTakerSegment = new SurveyTakerSegment
                        {
                            SurveyId = survey.Id,
                            CountryRegion = surveyTakerSegment.CountryRegion,
                            MaritalStatus = surveyTakerSegment.MaritalStatus,
                            AverageIncome = surveyTakerSegment.AverageIncome,
                            EducationLevel = surveyTakerSegment.EducationLevel,
                            JobField = surveyTakerSegment.JobField,
                            Prompt = surveyTakerSegment.Prompt,
                            TagFilterAccuracyRate = surveyTakerSegment.TagFilterAccuracyRate
                        };

                        await _surveyTakerSegmentGenericRepository.CreateAsync(newSurveyTakerSegment);
                    }
                    else
                    {
                        await _unitOfWork.SurveyTakerSegmentRepository.UpdateAsync(survey.Id, survey.SurveyTakerSegment);
                    }

                    var existingSurveyTimeRateConfig = await _unitOfWork.SurveyTagFilterRepository.FindBySurveyIdAsync(survey.Id);
                    // lưu SurveyTagFilter + SurveyEmbeddingVectorTagFilter
                    foreach (var tagFilter in communitySurveyPublishRequestDTO.FilterTags)
                    {
                        // lặp kiểm tra nếu có thì update, không có thì create
                        // var existingSummarizedSurveyTagFilter = survey.SurveyTagFilters.FirstOrDefault(tf => tf.FilterTagId == tagFilter.Id);
                        var existingSummarizedSurveyTagFilter = existingSurveyTimeRateConfig.FirstOrDefault(tf => tf.FilterTagId == tagFilter.Id);
                        if (existingSummarizedSurveyTagFilter != null)
                        {
                            existingSummarizedSurveyTagFilter.Summary = tagFilter.Summary;
                            existingSummarizedSurveyTagFilter.FilterTag = null; // Để tránh vòng lặp vô hạn khi serialize
                            existingSummarizedSurveyTagFilter.Survey = null; // Để tránh vòng lặp vô hạn khi serialize
                            await _unitOfWork.SurveyTagFilterRepository.UpdateAsync(existingSummarizedSurveyTagFilter);
                        }
                        else
                        {
                            var newSurveyTagFilter = new SurveyTagFilter
                            {
                                SurveyId = survey.Id,
                                FilterTagId = tagFilter.Id,
                                Summary = tagFilter.Summary
                            };
                            await _surveyTagFilterGenericRepository.CreateAsync(newSurveyTagFilter);
                        }

                        // var existingEmbeddingVectorSurveyTagFilter = await _postgresDbContext.SurveyEmbeddingVectorTagFilters
                        //     .FirstOrDefaultAsync(t => t.SurveyId == surveyId && t.FilterTagId == tagFilter.Id);
                        // if (existingEmbeddingVectorSurveyTagFilter != null)
                        // {
                        //     existingEmbeddingVectorSurveyTagFilter.EmbeddingVector = tagFilter.EmbeddingVector == null || tagFilter.EmbeddingVector.Count() == 0
                        //         ? null
                        //         : new Vector(tagFilter.EmbeddingVector.ToArray());
                        //     _postgresDbContext.SurveyEmbeddingVectorTagFilters.Update(existingEmbeddingVectorSurveyTagFilter);
                        // }
                        // else
                        // {
                        //     var newSurveyTagFilter = new SurveyEmbeddingVectorTagFilter
                        //     {
                        //         SurveyId = surveyId,
                        //         FilterTagId = tagFilter.Id,
                        //         EmbeddingVector = tagFilter.EmbeddingVector == null || tagFilter.EmbeddingVector.Count() == 0
                        //             ? null
                        //             : new Vector(tagFilter.EmbeddingVector.ToArray())
                        //     };
                        //     await _postgresDbContext.SurveyEmbeddingVectorTagFilters.AddAsync(newSurveyTagFilter);
                        // }


                    }

                    // Lưu SurveyStatusTracking
                    var surveyStatusTracking = new SurveyStatusTracking
                    {
                        SurveyId = survey.Id,
                        SurveyStatusId = 2,
                    };
                    await _surveyStatusTrackingGenericRepository.CreateAsync(surveyStatusTracking);

                    // Lưu SurveyRewardTracking
                    // RewardPrice: (takerBaseRewardPrice +  allocTimeAmount * [(currentDate - startDate)/ (endDate - startDate)]
                    // RewardXp: MaxXp * [1 -  [(currentDate - startDate)/ (endDate - startDate)]]
                    var currentDate = DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone());
                    var startDate = survey.StartDate;
                    var endDate = survey.EndDate;
                    var daysPassed = currentDate.DayNumber - startDate?.DayNumber;
                    var totalDays = endDate?.DayNumber - startDate?.DayNumber;
                    Console.WriteLine($"Tiền cộng đồng: {communitySurveyPublishRequestDTO.TheoryPrice}, Extra Price: {communitySurveyPublishRequestDTO.ExtraPrice}");
                    Console.WriteLine($"Profit Price: {profitPrice}, Alloc Base Amount: {allocBaseAmount}, Alloc Time Amount: {allocTimeAmount}, Alloc Level Amount: {allocLevelAmount}");
                    Console.WriteLine($"Max XP: {maxXp}, Taker Base Reward Price: {takerBaseRewardPrice}");

                    Console.WriteLine($"Current Date: {currentDate}, Start Date: {startDate}, End Date: {endDate}, Days Passed: {daysPassed}, Total Days: {totalDays}");

                    //var rewardPrice = takerBaseRewardPrice + allocTimeAmount * (decimal)daysPassed / totalDays;
                    var rewardPrice = takerBaseRewardPrice + allocTimeAmount * (decimal)daysPassed / (totalDays ?? 1);
                    Console.WriteLine(totalDays.HasValue && totalDays.Value != 0
                        ? "ngu"
                        : "không ngu");
                    var rewardXp = totalDays.HasValue && totalDays.Value != 0
                        ? (int)Math.Floor(maxXp * (1 - ((decimal)(daysPassed) / totalDays ?? 1)))
                        : 0;

                    Console.WriteLine($"Reward Price: {rewardPrice}, Reward XP: {rewardXp}");
                    await _surveyRewardTrackingGenericRepository.CreateAsync(new SurveyRewardTracking
                    {
                        SurveyId = survey.Id,
                        RewardPrice = rewardPrice,
                        RewardXp = rewardXp,
                    });

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Đăng tải khảo sát cộng đồng thất bại, lỗi: " + ex.Message);
                }
            }

            using (var transaction = await _postgresDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var tagFilter in communitySurveyPublishRequestDTO.FilterTags)
                    {
                        var existingEmbeddingVectorSurveyTagFilter = await _postgresDbContext.SurveyEmbeddingVectorTagFilters
                            .FirstOrDefaultAsync(t => t.SurveyId == surveyId && t.FilterTagId == tagFilter.Id);
                        if (existingEmbeddingVectorSurveyTagFilter != null)
                        {
                            existingEmbeddingVectorSurveyTagFilter.EmbeddingVector = tagFilter.EmbeddingVector == null || tagFilter.EmbeddingVector.Count() == 0
                                ? null
                                : new Vector(tagFilter.EmbeddingVector.ToArray());
                            _postgresDbContext.SurveyEmbeddingVectorTagFilters.Update(existingEmbeddingVectorSurveyTagFilter);
                        }
                        else
                        {
                            var newSurveyTagFilter = new SurveyEmbeddingVectorTagFilter
                            {
                                SurveyId = surveyId,
                                FilterTagId = tagFilter.Id,
                                EmbeddingVector = tagFilter.EmbeddingVector == null || tagFilter.EmbeddingVector.Count() == 0
                                    ? null
                                    : new Vector(tagFilter.EmbeddingVector.ToArray())
                            };
                            await _postgresDbContext.SurveyEmbeddingVectorTagFilters.AddAsync(newSurveyTagFilter);
                        }


                    }


                    await _postgresDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Đăng tải khảo sát cộng đồng thành công, lỗi filter tag: " + ex.Message);
                }
            }
        }

        public async Task<List<SurveyCommunityTransactionDTO>> GetSurveyTakenEarnHistory(Account account)
        {
            try
            {
                // Retrieve the withdraw history for the account
                var transactions = await _surveyCommunityTransactionGenericRepository.FindAll(
                    predicate: transaction => transaction.TransactionTypeId == 2 && transaction.TransactionStatusId == 2
                    &&
                    // nếu là manager thì lấy hết account, nếu là customer thì chỉ lấy account của mình
                    (account.RoleId == 2 || account.RoleId == 4 && transaction.AccountId == account.Id)
                    ,
                    includeProperties: new Expression<Func<SurveyCommunityTransaction, object>>[] {
                        t => t.Account,
                        t => t.Survey,
                        t => t.TransactionType,
                        t => t.TransactionStatus
                    }
                ).ToListAsync();
                var history = new List<SurveyCommunityTransactionDTO>();
                foreach (var t in transactions)
                {
                    history.Add(new SurveyCommunityTransactionDTO
                    {
                        Id = t.Id,
                        Account = new SurveyCommunityTransactionAccountDTO
                        {
                            Id = t.Account.Id,
                            Email = t.Account.Email,
                            FullName = t.Account.FullName,
                            Phone = t.Account.Phone,
                            MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.ACCOUNt_IMAGE_PATH, t.Account.Id.ToString()))
                        },
                        Survey = new SurveyCommunityTransactionSurveyDTO
                        {
                            Id = t.Survey.Id,
                            Title = t.Survey.Title,
                            MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, t.Survey.Id.ToString())),
                        },
                        Amount = t.Amount,
                        Profit = t.Profit,
                        CreatedAt = t.CreatedAt,
                        TransactionStatus = new TransactionStatusDTO
                        {
                            Id = t.TransactionStatus.Id,
                            Name = t.TransactionStatus.Name
                        },
                        TransactionType = new TransactionTypeDTO
                        {
                            Id = t.TransactionType.Id,
                            Name = t.TransactionType.Name,
                            OperationType = t.TransactionType.OperationType
                        }
                    });
                }
                return history;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lấy lịch sử rút tiền không thành công, lỗi: " + ex.Message);
            }
        }

    }
}
