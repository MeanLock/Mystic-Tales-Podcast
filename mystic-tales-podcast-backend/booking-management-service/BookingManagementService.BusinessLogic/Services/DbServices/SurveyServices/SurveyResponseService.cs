using Microsoft.Extensions.Logging;
using BookingManagementService.Common.AppConfigurations.App.interfaces;
using BookingManagementService.Common.AppConfigurations.FilePath.interfaces;
using BookingManagementService.DataAccess.Data;
using BookingManagementService.DataAccess.UOW;
using BookingManagementService.DataAccess.Entities;
using BookingManagementService.BusinessLogic.DTOs.Survey.Filters;
using Newtonsoft.Json.Linq;
using BookingManagementService.BusinessLogic.Exceptions;
using Newtonsoft.Json;
using BookingManagementService.DataAccess.Repositories.interfaces;
using BookingManagementService.BusinessLogic.DTOs.Survey.Session;
using BookingManagementService.BusinessLogic.Enums;
using BookingManagementService.BusinessLogic.DTOs.Survey.JsonConfigs;
using BookingManagementService.BusinessLogic.DTOs.Survey.TakenResult.V1;
using BookingManagementService.BusinessLogic.DTOs.Survey.TakenResult;
using BookingManagementService.BusinessLogic.Services.EmbeddingVectorServices;
using BookingManagementService.DataAccess.Entities.postgres;
using Pgvector;
using BookingManagementService.BusinessLogic.DTOs.FilterTag;
using Microsoft.EntityFrameworkCore;
using BookingManagementService.Common.AppConfigurations.BusinessSetting.interfaces;
using Duende.IdentityServer.Extensions;
using BookingManagementService.BusinessLogic.Services.OpenAIServices;
using BookingManagementService.BusinessLogic.Helpers.AuthHelpers;
using BookingManagementService.BusinessLogic.Helpers.FileHelpers;
using BookingManagementService.BusinessLogic.Helpers.DateHelpers;

namespace BookingManagementService.BusinessLogic.Services.DbServices.SurveyServices
{
    public class SurveyResponseService
    {
        // LOGGER
        private readonly ILogger<SurveySessionService> _logger;

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
        private readonly IGenericRepository<Account> _accountGenericRepository;
        private readonly IGenericRepository<AccountOnlineTracking> _accountOnlineTrackingGenericRepository;
        private readonly IGenericRepository<TakerTagFilter> _takerTagFilterGenericRepository;
        private readonly IGenericRepository<FilterTag> _filterTagGenericRepository;
        private readonly IGenericRepository<AccountProfile> _accountProfileGenericRepository;
        private readonly IGenericRepository<Survey> _surveyGenericRepository;
        private readonly IGenericRepository<SurveyQuestion> _surveyQuestionGenericRepository;
        private readonly IGenericRepository<SurveyOption> _surveyOptionGenericRepository;
        private readonly IGenericRepository<SurveyTakenResult> _surveyTakenResultGenericRepository;
        private readonly IGenericRepository<SurveyResponse> _surveyResponseGenericRepository;
        private readonly IGenericRepository<SurveyTagFilter> _surveyTagFilterGenericRepository;
        private readonly IGenericRepository<SurveyTakenResultTagFilter> _surveyTakenResultTagFilterGenericRepository;
        private readonly IGenericRepository<SurveyStatusTracking> _surveyStatusTrackingGenericRepository;
        private readonly IGenericRepository<AccountBalanceTransaction> _accountBalanceTransactionGenericRepository;
        private readonly IGenericRepository<SurveyCommunityTransaction> _surveyCommunityTransactionGenericRepository;


        // OPENAI SERVICE
        private readonly SurveyOpenAIService _surveyOpenAIService;

        // EMBEDDING VECTOR SERVICE
        private readonly SurveyEmbeddingVectorService _surveyEmbeddingVectorService;

        public SurveyResponseService(
            ILogger<SurveySessionService> logger,
            AppDbContext appDbContext,
            PostgresDbContext postgresDbContext,

            BcryptHelper bcryptHelper,
            JwtHelper jwtHelper,
            DateHelper dateHelper,
            IUnitOfWork unitOfWork,

            IGenericRepository<Account> accountGenericRepository,
            IGenericRepository<AccountOnlineTracking> accountOnlineTrackingGenericRepository,
            IGenericRepository<TakerTagFilter> takerTagFilterGenericRepository,
            IGenericRepository<FilterTag> filterTagGenericRepository,
            IGenericRepository<AccountProfile> accountProfileGenericRepository,
            IGenericRepository<Survey> surveyGenericRepository,
            IGenericRepository<SurveyQuestion> surveyQuestionGenericRepository,
            IGenericRepository<SurveyOption> surveyOptionGenericRepository,
            IGenericRepository<SurveyTakenResult> surveyTakenResultGenericRepository,
            IGenericRepository<SurveyResponse> surveyResponseGenericRepository,
            IGenericRepository<SurveyTagFilter> surveyTagFilterGenericRepository,
            IGenericRepository<SurveyTakenResultTagFilter> surveyTakenResultTagFilterGenericRepository,
            IGenericRepository<SurveyStatusTracking> surveyStatusTrackingGenericRepository,
            IGenericRepository<AccountBalanceTransaction> paymentHistoryGenericRepository,
            IGenericRepository<SurveyCommunityTransaction> surveyCommunityTransactionGenericRepository,


            FileIOHelper fileIOHelper,
            IFilePathConfig filePathConfig,
            SurveyOpenAIService surveyOpenAIService,
            SurveyEmbeddingVectorService surveyEmbeddingVectorService,
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

            _accountGenericRepository = accountGenericRepository;
            _accountOnlineTrackingGenericRepository = accountOnlineTrackingGenericRepository;
            _takerTagFilterGenericRepository = takerTagFilterGenericRepository;
            _filterTagGenericRepository = filterTagGenericRepository;
            _accountProfileGenericRepository = accountProfileGenericRepository;
            _surveyGenericRepository = surveyGenericRepository;
            _surveyQuestionGenericRepository = surveyQuestionGenericRepository;
            _surveyOptionGenericRepository = surveyOptionGenericRepository;
            _surveyTakenResultGenericRepository = surveyTakenResultGenericRepository;
            _surveyResponseGenericRepository = surveyResponseGenericRepository;
            _surveyTagFilterGenericRepository = surveyTagFilterGenericRepository;
            _surveyTakenResultTagFilterGenericRepository = surveyTakenResultTagFilterGenericRepository;
            _surveyStatusTrackingGenericRepository = surveyStatusTrackingGenericRepository;
            _accountBalanceTransactionGenericRepository = paymentHistoryGenericRepository;
            _surveyCommunityTransactionGenericRepository = surveyCommunityTransactionGenericRepository;

            _fileIOHelper = fileIOHelper;
            _filePathConfig = filePathConfig;


            _surveyOpenAIService = surveyOpenAIService;
            _surveyEmbeddingVectorService = surveyEmbeddingVectorService;
            _appConfig = appConfig;
            _surveyConfig = surveyConfig;
        }

        public async Task TrackingOnline(int accountId)
        {
            // kiểm tra hôm nay đã có dòng nào chưa, nếu chưa thì câp nhật surveyTakenCount += 1
            var today = DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone());
            var existingTracking = await _unitOfWork.AccountOnlineTrackingRepository.FindLatestByAccountIdAsync(accountId);
            if (existingTracking != null && existingTracking.OnlineDate == today)
            {
                existingTracking.SurveyTakenCount += 1;
                await _accountOnlineTrackingGenericRepository.UpdateAsync(existingTracking.Id, existingTracking);
                return;
            }
            else
            {
                var newTracking = new AccountOnlineTracking
                {
                    AccountId = accountId
                };
                await _accountOnlineTrackingGenericRepository.CreateAsync(newTracking);
            }
        }

        /////////////////////////////////////////////////////////////

        public async Task TakeFilterSurveyResponse(int surveyId, int userId, SurveyTakingResponseRequestDTO surveyResponseRequestDTO, SurveyTakenSubjectEnum TakenSubject)
        {
            var summarizedFilterTags = new List<SummarizedFilterTagDTO>();
            var vectorEncodedFilterTags = new List<EmbeddingVectorFilterTagDTO>();
            // var isValid = true;

            var defaultFilterTags = (await _unitOfWork.FilterTagRepository.FindByTagTypeIdAsync(1)).ToList();
            var additionalFilterTags = (await _unitOfWork.FilterTagRepository.FindByTagTypeIdAsync(2)).Select(tag => new FilterTag
            {
                Id = tag.Id,
                FilterTagTypeId = tag.FilterTagTypeId,
                Name = tag.Name,
                FilterTagType = new FilterTagType
                {
                    Id = tag.FilterTagType.Id,
                    Name = tag.FilterTagType.Name
                }
            }).ToList();
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    SurveyFilterObject surveyFilterObject = new SurveyFilterObject
                    {
                        IsDeletedContain = false,
                        // IsInvalidTakenResultContain = false,
                        IsAvailable = true,
                        SurveyTypeId = 1,
                        SurveyStatusIds = new List<int> { 2 }
                    };

                    Survey survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                    if (survey == null)
                    {
                        throw new Exception("survey không tồn tại");
                    }

                    var newSurveyTakenResult = new SurveyTakenResult
                    {
                        SurveyId = surveyId,
                        TakerId = userId,
                        IsValid = surveyResponseRequestDTO.SurveyResponses.Any(r => r.IsValid == false) ? false : true,
                        CompletedAt = _dateHelper.GetNowByAppTimeZone(),
                        InvalidReason = surveyResponseRequestDTO.InvalidReason,
                    };

                    // Lưu kết quả khảo sát
                    await _surveyTakenResultGenericRepository.CreateAsync(newSurveyTakenResult);

                    foreach (var response in surveyResponseRequestDTO.SurveyResponses)
                    {
                        var jsonStringValue = JsonConvert.SerializeObject(response.ValueJson);

                        var newSurveyResponse = new SurveyResponse
                        {
                            SurveyTakenResultId = newSurveyTakenResult.Id,
                            IsValid = response.IsValid,
                            ValueJsonString = jsonStringValue,
                            SurveyQuestionId = response.ValueJson.QuestionContent.Id
                        };
                        // Lưu phản hồi câu hỏi
                        await _surveyResponseGenericRepository.CreateAsync(newSurveyResponse);
                    }

                    Account account = await _unitOfWork.AccountRepository.FindByIdAsync(userId);
                    account.IsFilterSurveyRequired = false;
                    account.LastFilterSurveyTakenAt = _dateHelper.GetNowByAppTimeZone();

                    // Console.WriteLine("1111111111111111111111111111: " + (account.AccountProfile == null ? "null" : account.AccountProfile.AccountId));
                    // // Console.WriteLine("account.IsFilterSurveyRequired: " + account.IsFilterSurveyRequired + " - account.LastFilterSurveyTakenAt: " + account.LastFilterSurveyTakenAt);
                    // var entityType = _appDbContext.Model.FindEntityType(typeof(AccountProfile));
                    // var primaryKey = entityType.FindPrimaryKey();
                    // var keyNames = primaryKey.Properties.Select(p => p.Name).ToList();
                    // Console.WriteLine("Primary Key Names: " + string.Join(", ", keyNames));

                    // Cập nhật trạng thái tài khoản
                    await _accountGenericRepository.UpdateAsync(userId, account);
                    await this.TrackingOnline(userId);

                    // lấy SurveyTopicFavorite/AccountProfile/filterSurveyTakenResult để đưa vào 
                    var accountProfile = account.AccountProfile;
                    accountProfile.Account = null;
                    var surveyTopicFavorites = (await _unitOfWork.SurveyTopicFavoriteRepository.FindByAccountIdAsync(userId)).Select(f => new SurveyTopicFavorite
                    {
                        SurveyTopicId = f.SurveyTopicId,
                        AccountId = f.AccountId,
                        FavoriteScore = f.FavoriteScore,
                        SurveyTopic = new SurveyTopic
                        {
                            Id = f.SurveyTopic.Id,
                            Name = f.SurveyTopic.Name,
                        },
                    }).ToList();
                    var filterSurveyResponseObjects = new List<SurveyTakingResponseValueJsonDTO>();
                    foreach (var response in surveyResponseRequestDTO.SurveyResponses)
                    {
                        filterSurveyResponseObjects.Add(response.ValueJson);
                    }
                    var filterSurveyResponses = JArray.FromObject(filterSurveyResponseObjects);

                    summarizedFilterTags = await _surveyOpenAIService.GetSummarizedDefaultFilterTagAsync(defaultFilterTags, filterSurveyResponses, account, accountProfile, surveyTopicFavorites);
                    vectorEncodedFilterTags = await _surveyEmbeddingVectorService.GetVectorEncodedFilterTagsAsync(summarizedFilterTags);

                    // Lưu các thẻ lọc đã tóm tắt và mã hóa vector (nếu chưa tồn tại thì thêm , nếu đã tồn tại thì cập nhật)
                    foreach (var summarizedFilterTag in summarizedFilterTags)
                    {
                        var existingTakerTagFilter = account.TakerTagFilters.FirstOrDefault(t => t.FilterTagId == summarizedFilterTag.FilterTagId);
                        if (existingTakerTagFilter != null)
                        {
                            existingTakerTagFilter.Summary = summarizedFilterTag.Summary;
                            await _unitOfWork.TakerTagFilterRepository.UpdateAsync(existingTakerTagFilter);
                        }
                        else
                        {
                            var newTakerTagFilter = new TakerTagFilter
                            {
                                TakerId = userId,
                                FilterTagId = summarizedFilterTag.FilterTagId,
                                Summary = summarizedFilterTag.Summary,
                            };
                            await _takerTagFilterGenericRepository.CreateAsync(newTakerTagFilter);
                        }
                    }
                    foreach (var additionalFilterTag in additionalFilterTags)
                    {
                        var existingTakerTagFilter = account.TakerTagFilters.FirstOrDefault(t => t.FilterTagId == additionalFilterTag.Id);
                        if (existingTakerTagFilter == null)
                        {
                            var newTakerTagFilter = new TakerTagFilter
                            {
                                TakerId = userId,
                                FilterTagId = additionalFilterTag.Id,
                                Summary = null,
                            };
                            await _takerTagFilterGenericRepository.CreateAsync(newTakerTagFilter);
                        }
                    }



                    await transaction.CommitAsync();
                    // isValid = newSurveyTakenResult.IsValid;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Ghi nhận kết quả thất bại, lí do: " + ex.Message);
                }
            }

            using (var transaction = await _postgresDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Lưu các thẻ lọc đã tóm tắt và mã hóa vector vào cơ sở dữ liệu PostgreSQL (nếu chưa tồn tại thì thêm , nếu đã tồn tại thì cập nhật)
                    foreach (var vectorEncodedFilterTag in vectorEncodedFilterTags)
                    {
                        var existingTakerTagFilter = await _postgresDbContext.TakerEmbeddingVectorTagFilters
                            .FirstOrDefaultAsync(t => t.TakerId == userId && t.FilterTagId == vectorEncodedFilterTag.FilterTagId);
                        if (existingTakerTagFilter != null)
                        {
                            existingTakerTagFilter.EmbeddingVector = vectorEncodedFilterTag.EmbeddingVector == null || vectorEncodedFilterTag.EmbeddingVector.Count() == 0
                                ? null
                                : new Vector(vectorEncodedFilterTag.EmbeddingVector.ToArray());
                            _postgresDbContext.TakerEmbeddingVectorTagFilters.Update(existingTakerTagFilter);
                        }
                        else
                        {
                            var newTakerTagFilter = new TakerEmbeddingVectorTagFilter
                            {
                                TakerId = userId,
                                FilterTagId = vectorEncodedFilterTag.FilterTagId,
                                EmbeddingVector = vectorEncodedFilterTag.EmbeddingVector == null || vectorEncodedFilterTag.EmbeddingVector.Count() == 0
                                    ? null
                                    : new Vector(vectorEncodedFilterTag.EmbeddingVector.ToArray())
                            };
                            await _postgresDbContext.TakerEmbeddingVectorTagFilters.AddAsync(newTakerTagFilter);
                        }
                    }
                    foreach (var additionalFilterTag in additionalFilterTags)
                    {
                        var existingTakerTagFilter = await _postgresDbContext.TakerEmbeddingVectorTagFilters
                            .FirstOrDefaultAsync(t => t.TakerId == userId && t.FilterTagId == additionalFilterTag.Id);
                        if (existingTakerTagFilter == null)
                        {
                            var newTakerTagFilter = new TakerEmbeddingVectorTagFilter
                            {
                                TakerId = userId,
                                FilterTagId = additionalFilterTag.Id,
                                EmbeddingVector = null
                            };
                            await _postgresDbContext.TakerEmbeddingVectorTagFilters.AddAsync(newTakerTagFilter);
                        }
                    }

                    await _postgresDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Ghi nhận kết quả thất bại, lí do: " + ex.Message);
                }
            }
            // return isValid;
        }


        public async Task<CommunitySurveyTakenResultResponseDTO> TakeCommunitySurveyResponse(int surveyId, int userId, SurveyTakingResponseRequestDTO surveyResponseRequestDTO, SurveyTakenSubjectEnum TakenSubject)
        {
            var summarizedSurveyTakenResultFilterTags = new List<SummarizedFilterTagDTO>();
            var summarizedTakerAdditionalFilterTags = new List<SummarizedFilterTagDTO>();
            var vectorEncodedSurveyTakenResultFilterTags = new List<EmbeddingVectorFilterTagDTO>();
            var vectorEncodedTakerAdditionalFilterTags = new List<EmbeddingVectorFilterTagDTO>();
            int newSurveyTakenResultId = 0;
            var isValid = true;
            decimal moneyEarned = 0;
            int xpEarned = 0;

            bool isKpiExceeded = false;

            var systemConfigProfile = await _unitOfWork.SystemConfigProfileRepository.FindActiveProfileAsync();
            SurveyFilterObject surveyFilterObject = new SurveyFilterObject
            {
                IsDeletedContain = false,
                // IsInvalidTakenResultContain = false,
                // IsAvailable = true,
                SurveyTypeId = 2,
                SurveyStatusIds = new List<int> { 2, 3 }
            };
            Survey survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
            var surveyTakenResults = await _unitOfWork.SurveyTakenResultRepository.FindBySurveyIdAsync(surveyId, false);

            var additionalFilterTags = (await _unitOfWork.FilterTagRepository.FindByTagTypeIdAsync(2)).ToList();

            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (survey == null)
                    {
                        throw new Exception("survey không tồn tại");
                    }
                    // [CHỈNH LẠI]  sau này sẽ mở ra nếu cầnhhhh
                    // if (survey.Kpi <= surveyTakenResults.Count())
                    // {
                    //     throw new Exception("survey đã đủ số lượng người làm");
                    // }
                    // if (survey.EndDate < DateOnly.FromDateTime(_dateHelpers.GetNowByAppTimeZone()))
                    // {
                    //     throw new Exception("survey đã kết thúc và ngừng tiếp nhận kết quả");
                    // }
                    if (survey.RequesterId == userId)
                    {
                        throw new Exception("không được phép làm survey của chính mình");
                    }
                    if (surveyTakenResults.Any(s => s.TakerId == userId))
                    {
                        throw new Exception("bạn đã làm survey này rồi");
                    }

                    var newSurveyTakenResult = new SurveyTakenResult
                    {
                        SurveyId = surveyId,
                        TakerId = userId,
                        IsValid = surveyResponseRequestDTO.SurveyResponses.Any(r => r.IsValid == false) ? false : true,
                        CompletedAt = _dateHelper.GetNowByAppTimeZone(),
                        InvalidReason = surveyResponseRequestDTO.InvalidReason.IsNullOrEmpty() ? null : surveyResponseRequestDTO.InvalidReason,
                    };

                    // Lưu kết quả khảo sát
                    await _surveyTakenResultGenericRepository.CreateAsync(newSurveyTakenResult);
                    List<SurveyResponse> addedSurveyResponses = new List<SurveyResponse>();

                    foreach (var response in surveyResponseRequestDTO.SurveyResponses)
                    {
                        var jsonStringValue = JsonConvert.SerializeObject(response.ValueJson);

                        var newSurveyResponse = new SurveyResponse
                        {
                            SurveyTakenResultId = newSurveyTakenResult.Id,
                            IsValid = response.IsValid,
                            ValueJsonString = jsonStringValue,
                            SurveyQuestionId = response.ValueJson.QuestionContent.Id
                        };

                        // Lưu phản hồi câu hỏi
                        await _surveyResponseGenericRepository.CreateAsync(newSurveyResponse);
                        addedSurveyResponses.Add(newSurveyResponse);
                    }

                    survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                    var newSurveyTakenResults = await _unitOfWork.SurveyTakenResultRepository.FindBySurveyIdAsync(surveyId, false);

                    if (newSurveyTakenResults.Count() >= survey.Kpi || survey.EndDate < DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone()))
                    {

                        survey.IsAvailable = false;
                        await _surveyGenericRepository.UpdateAsync(survey.Id, survey);
                        var newSurveyStatusTracking = new SurveyStatusTracking
                        {
                            SurveyId = survey.Id,
                            SurveyStatusId = 3
                        };
                        await _surveyStatusTrackingGenericRepository.CreateAsync(newSurveyStatusTracking);


                        if (newSurveyTakenResults.Count() > survey.Kpi)
                        {

                            moneyEarned = 0;
                            xpEarned = (int)Math.Round((float)survey.MaxXp * _surveyConfig.KpiExceedXpEarnRate);
                            Console.WriteLine(survey.MaxXp + " " + _surveyConfig.KpiExceedXpEarnRate);
                            Console.WriteLine("moneyEarned: " + (int)Math.Round((float)survey.MaxXp * _surveyConfig.KpiExceedXpEarnRate));

                        }
                        else if (newSurveyTakenResults.Count() == survey.Kpi)
                        {
                            moneyEarned = survey.SurveyRewardTrackings.OrderByDescending(r => r.CreatedAt).FirstOrDefault().RewardPrice;
                            xpEarned = survey.SurveyRewardTrackings.OrderByDescending(r => r.CreatedAt).FirstOrDefault().RewardXp;
                        }

                    }
                    else
                    {
                        moneyEarned = survey.SurveyRewardTrackings.OrderByDescending(r => r.CreatedAt).FirstOrDefault().RewardPrice;
                        xpEarned = survey.SurveyRewardTrackings.OrderByDescending(r => r.CreatedAt).FirstOrDefault().RewardXp;
                    }

                    newSurveyTakenResult.MoneyEarned = moneyEarned;
                    newSurveyTakenResult.XpEarned = xpEarned;
                    await _surveyTakenResultGenericRepository.UpdateAsync(newSurveyTakenResult.Id, newSurveyTakenResult);


                    await transaction.CommitAsync();

                    // await transaction.CommitAsync();
                    newSurveyTakenResultId = newSurveyTakenResult.Id;
                    isValid = newSurveyTakenResult.IsValid;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Ghi nhận kết quả thất bại, lí do: " + ex.Message);
                }
            }

            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    Account account = await _unitOfWork.AccountRepository.FindByIdAsync(userId);
                    int currentLevel = account.Level;
                    if (isValid == true)
                    {
                        await this.TrackingOnline(account.Id);
                        decimal levelBonusRate = (decimal)systemConfigProfile.AccountLevelSettingConfigs.Where(c => c.Level == account.Level).Select(c => c.BonusRate).FirstOrDefault();
                        account.Balance += moneyEarned + (decimal)survey.AllocLevelAmount * levelBonusRate;
                        account.Xp += xpEarned;

                        // tính toán lại level
                        if (TakenSubject == SurveyTakenSubjectEnum.Verified)
                        {
                            int currentXpLevelThreshold = currentLevel * systemConfigProfile.AccountGeneralConfig.XpLevelThreshold;
                            if (account.Xp > currentXpLevelThreshold && account.ProgressionSurveyCount == 0)
                            {
                                account.ProgressionSurveyCount = systemConfigProfile.AccountLevelSettingConfigs.Where(c => c.Level == currentLevel + 1).Select(c => c.ProgressionSurveyCount).FirstOrDefault();
                            }
                        }
                        else if (TakenSubject == SurveyTakenSubjectEnum.LevelUpdate)
                        {
                            account.ProgressionSurveyCount -= 1;
                            if (account.ProgressionSurveyCount == 0)
                            {
                                account.Level += 1;
                                int currentXpLevelThreshold = account.Level * systemConfigProfile.AccountGeneralConfig.XpLevelThreshold;
                                if (account.Xp > currentXpLevelThreshold)
                                {
                                    account.ProgressionSurveyCount = systemConfigProfile.AccountLevelSettingConfigs.Where(al => al.Level == account.Level).Select(c => c.ProgressionSurveyCount).FirstOrDefault();
                                }
                            }
                        }

                        await _accountGenericRepository.UpdateAsync(userId, account);

                        var newSurveyCommunityTransaction = new SurveyCommunityTransaction
                        {
                            AccountId = userId,
                            Amount = moneyEarned + (decimal)survey.AllocLevelAmount * levelBonusRate,
                            TransactionStatusId = 2,
                            TransactionTypeId = 2,
                            SurveyId = surveyId
                        };
                        await _surveyCommunityTransactionGenericRepository.CreateAsync(newSurveyCommunityTransaction);


                    }



                    // lấy communitySurveyTakenResult để đưa vào 
                    var communitySurveyResponseObjects = new List<SurveyTakingResponseValueJsonDTO>();
                    foreach (var response in surveyResponseRequestDTO.SurveyResponses)
                    {
                        communitySurveyResponseObjects.Add(response.ValueJson);
                    }
                    var communitySurveyResponses = JArray.FromObject(communitySurveyResponseObjects);

                    summarizedSurveyTakenResultFilterTags = await _surveyOpenAIService.GetSummarizedAddtionalFilterTagBySurveyTakenResponsesAsync(additionalFilterTags, communitySurveyResponses);
                    vectorEncodedSurveyTakenResultFilterTags = await _surveyEmbeddingVectorService.GetVectorEncodedFilterTagsAsync(summarizedSurveyTakenResultFilterTags);

                    // Lưu các thẻ lọc đã tóm tắt và mã hóa vector 
                    foreach (var summarizedFilterTag in summarizedSurveyTakenResultFilterTags)
                    {
                        var newSurveyTakenResultTagFilter = new SurveyTakenResultTagFilter
                        {
                            SurveyTakenResultId = newSurveyTakenResultId,
                            AdditionalFilterTagId = summarizedFilterTag.FilterTagId,
                            Summary = summarizedFilterTag.Summary,
                        };
                        await _surveyTakenResultTagFilterGenericRepository.CreateAsync(newSurveyTakenResultTagFilter);
                    }

                    // lấy các danh sách SurveyTakenResultTagFilters thuộc limit 20 lần taken gần nhất bao gồm cả lần hiện tại 
                    var recentSurveyTakenResults = await _unitOfWork.SurveyTakenResultRepository.FindByTakerIdAsync(userId, _surveyConfig.RecentTakenThreshold);
                    List<List<SurveyTakenResultTagFilter>> recentSurveyTakenResultTagFilters = recentSurveyTakenResults.Select(s =>
                    {
                        return s.SurveyTakenResultTagFilters.Select(t => new SurveyTakenResultTagFilter
                        {
                            AdditionalFilterTagId = t.AdditionalFilterTagId,
                            Summary = t.Summary
                        }).ToList();
                    }).ToList();
                    List<TakerTagFilter> recentTakerAdditionalFilterTags = account.TakerTagFilters
                        .Where(t => t.FilterTag.FilterTagTypeId == 2)
                        .Select(t => new TakerTagFilter
                        {
                            FilterTagId = t.FilterTagId,
                            Summary = t.Summary
                        })
                        .ToList();



                    summarizedTakerAdditionalFilterTags = await _surveyOpenAIService.GetMergedAdditionalFilterTagSummariesAsync(
                        additionalFilterTags,
                        recentSurveyTakenResultTagFilters,
                        recentTakerAdditionalFilterTags);
                    vectorEncodedTakerAdditionalFilterTags = await _surveyEmbeddingVectorService.GetVectorEncodedFilterTagsAsync(
                        summarizedTakerAdditionalFilterTags);



                    // Lưu các summarizedTakerAdditionalFilterTags mới vào TakerTagFilter theo id tương ứng (nếu chưa tồn tại thì thêm , nếu đã tồn tại thì cập nhật)
                    foreach (var summarizedFilterTag in summarizedTakerAdditionalFilterTags)
                    {
                        var existingTakerTagFilter = account.TakerTagFilters.FirstOrDefault(t => t.FilterTagId == summarizedFilterTag.FilterTagId);
                        if (existingTakerTagFilter != null)
                        {
                            existingTakerTagFilter.Summary = summarizedFilterTag.Summary;
                            await _unitOfWork.TakerTagFilterRepository.UpdateAsync(existingTakerTagFilter);
                        }
                        else
                        {
                            var newTakerTagFilter = new TakerTagFilter
                            {
                                TakerId = userId,
                                FilterTagId = summarizedFilterTag.FilterTagId,
                                Summary = summarizedFilterTag.Summary,
                            };
                            await _takerTagFilterGenericRepository.CreateAsync(newTakerTagFilter);
                        }
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Ghi nhận kết quả thành công, lỗi filter tag: " + ex.Message);
                }
            }

            using (var transaction = await _postgresDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Lưu các thẻ lọc đã tóm tắt và mã hóa vector vào cơ sở dữ liệu PostgreSQL 
                    foreach (var vectorEncodedFilterTag in vectorEncodedSurveyTakenResultFilterTags)
                    {
                        var newSurveyEmbeddingVectorTagFilter = new SurveyEmbeddingVectorTakenResultTagFilter
                        {
                            SurveyTakenResultId = newSurveyTakenResultId,
                            AdditionalFilterTagId = vectorEncodedFilterTag.FilterTagId,
                            EmbeddingVector = vectorEncodedFilterTag.EmbeddingVector == null || vectorEncodedFilterTag.EmbeddingVector.Count() == 0
                                ? null
                                : new Vector(vectorEncodedFilterTag.EmbeddingVector.ToArray())
                        };
                        await _postgresDbContext.SurveyEmbeddingVectorTakenResultTagFilters.AddAsync(newSurveyEmbeddingVectorTagFilter);
                    }

                    foreach (var vectorEncodedFilterTag in vectorEncodedTakerAdditionalFilterTags)
                    {
                        var existingTakerTagFilter = await _postgresDbContext.TakerEmbeddingVectorTagFilters
                            .FirstOrDefaultAsync(t => t.TakerId == userId && t.FilterTagId == vectorEncodedFilterTag.FilterTagId);
                        if (existingTakerTagFilter != null)
                        {
                            existingTakerTagFilter.EmbeddingVector = vectorEncodedFilterTag.EmbeddingVector == null || vectorEncodedFilterTag.EmbeddingVector.Count() == 0
                                ? null
                                : new Vector(vectorEncodedFilterTag.EmbeddingVector.ToArray());
                            _postgresDbContext.TakerEmbeddingVectorTagFilters.Update(existingTakerTagFilter);
                        }
                        else
                        {
                            var newTakerTagFilter = new TakerEmbeddingVectorTagFilter
                            {
                                TakerId = userId,
                                FilterTagId = vectorEncodedFilterTag.FilterTagId,
                                EmbeddingVector = vectorEncodedFilterTag.EmbeddingVector == null || vectorEncodedFilterTag.EmbeddingVector.Count() == 0
                                    ? null
                                    : new Vector(vectorEncodedFilterTag.EmbeddingVector.ToArray())
                            };
                            await _postgresDbContext.TakerEmbeddingVectorTagFilters.AddAsync(newTakerTagFilter);
                        }
                    }

                    await _postgresDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Ghi nhận kết quả thành công, lỗi filter tag: " + ex.Message);
                }
            }

            return new CommunitySurveyTakenResultResponseDTO
            {
                IsValid = isValid,
                MoneyEarned = moneyEarned,
                XpEarned = xpEarned,
            };
        }



        public async Task<List<CommunityResponseSummaryListItemDTO>> GetCommunitySurveyResponse(int surveyId, Account account)
        {
            try
            {
                var surveyFilterObject = new SurveyFilterObject
                {
                    RequesterId = account.Id,
                    IsDeletedContain = false,
                    SurveyTypeId = 2,
                    SurveyStatusIds = new List<int> { 2, 3, 4, 5 },
                    IsEndDateExceededContain = true
                };

                var survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                if (survey == null)
                {
                    throw new Exception("survey không tồn tại hoặc không thể lấy dữ liệu khảo sát ở thời điểm hiện tại");
                }

                var communitySurveyTakenResults = await _unitOfWork.SurveyTakenResultRepository.FindBySurveyIdAsync(surveyId, false);

                var questionResponseSummaryLists = new List<CommunityResponseSummaryListItemDTO>();

                foreach (var takenResult in communitySurveyTakenResults)
                {
                    var questionResponseSummaryList = new CommunityResponseSummaryListItemDTO
                    {
                        Questions = (await Task.WhenAll(takenResult.SurveyResponses.Select(async r => new CommunityResponseSummarySurveyQuestionDTO
                        {
                            Id = r.SurveyQuestion.Id,
                            SurveyId = r.SurveyQuestion.SurveyId,
                            QuestionTypeId = r.SurveyQuestion.QuestionTypeId ?? 0,
                            Content = r.SurveyQuestion.Content,
                            Description = r.SurveyQuestion.Description,
                            TimeLimit = r.SurveyQuestion.TimeLimit,
                            IsVoiced = r.SurveyQuestion.IsVoiced,
                            Order = r.SurveyQuestion.Order,
                            ConfigJsonString = r.SurveyQuestion.ConfigJsonString,
                            DeletedAt = r.SurveyQuestion.DeletedAt,
                            MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "question_" + r.SurveyQuestion.Id, "main")),
                            Options = (await Task.WhenAll(r.SurveyQuestion.SurveyOptions.Select(async o => new CommunityResponseSummarySurveyOptionDTO
                            {
                                Id = o.Id,
                                Content = o.Content,
                                Order = o.Order,
                                SurveyQuestionId = r.SurveyQuestion.Id,
                                MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "question_" + r.SurveyQuestion.Id, "option_" + o.Id, "main"))
                            }))).ToList()
                        }))).ToList(),
                        Responses = takenResult.SurveyResponses.Select(r => new CommunityResponseSummarySurveyResponseDTO
                        {
                            Id = r.Id,
                            ValueJsonString = r.ValueJsonString,
                            IsValid = r.IsValid,
                            SurveyQuestionId = r.SurveyQuestionId,
                            SurveyTakenResultId = r.SurveyTakenResultId,
                        }).ToList()
                    };
                    questionResponseSummaryLists.Add(questionResponseSummaryList);
                }

                return questionResponseSummaryLists;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lấy kết quả khảo sát thất bại, lí do: " + ex.Message);
            }

        }

        public async Task<List<FilterResponseSummaryListItemDTO>> GetFilterSurveyResponseSummary(int surveyId)
        {
            try
            {
                SurveyFilterObject surveyFilterObject = new SurveyFilterObject
                {
                    SurveyTypeId = 1,
                    SurveyStatusIds = new List<int> { 1, 2, 3, 4, 5 }, // Only get surveys with status "Draft"
                    IsDeletedContain = true,
                };

                var survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                if (survey == null)
                {
                    throw new Exception("survey không tồn tại hoặc không thể lấy dữ liệu khảo sát ở thời điểm hiện tại");
                }

                var filterSurveyTakenResults = await _unitOfWork.SurveyTakenResultRepository.FindBySurveyIdAsync(surveyId, false);

                var questionResponseSummaryLists = new List<FilterResponseSummaryListItemDTO>();

                foreach (var takenResult in filterSurveyTakenResults)
                {
                    var questionResponseSummaryList = new FilterResponseSummaryListItemDTO
                    {
                        Questions = (await Task.WhenAll(takenResult.SurveyResponses.Select(async r => new FilterResponseSummarySurveyQuestionDTO
                        {
                            Id = r.SurveyQuestion.Id,
                            SurveyId = r.SurveyQuestion.SurveyId,
                            QuestionTypeId = r.SurveyQuestion.QuestionTypeId ?? 0,
                            Content = r.SurveyQuestion.Content,
                            Description = r.SurveyQuestion.Description,
                            Order = r.SurveyQuestion.Order,
                            ConfigJsonString = r.SurveyQuestion.ConfigJsonString,
                            DeletedAt = r.SurveyQuestion.DeletedAt,
                            MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "question_" + r.SurveyQuestion.Id, "main")),
                            Options = (await Task.WhenAll(r.SurveyQuestion.SurveyOptions.Select(async o => new FilterResponseSummarySurveyOptionDTO
                            {
                                Id = o.Id,
                                Content = o.Content,
                                Order = o.Order,
                                SurveyQuestionId = r.SurveyQuestion.Id,
                                MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "question_" + r.SurveyQuestion.Id, "option_" + o.Id, "main"))
                            }))).ToList()
                        }))).ToList(),
                        Responses = takenResult.SurveyResponses.Select(r => new FilterResponseSummarySurveyResponseDTO
                        {
                            Id = r.Id,
                            ValueJsonString = r.ValueJsonString,
                            IsValid = r.IsValid,
                            SurveyQuestionId = r.SurveyQuestionId,
                            SurveyTakenResultId = r.SurveyTakenResultId,
                        }).ToList()
                    };
                    questionResponseSummaryLists.Add(questionResponseSummaryList);
                }

                return questionResponseSummaryLists;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lấy kết quả khảo sát thất bại, lí do: " + ex.Message);
            }

        }



        // ///////////////////////////////////////////////////////////



    }
}
