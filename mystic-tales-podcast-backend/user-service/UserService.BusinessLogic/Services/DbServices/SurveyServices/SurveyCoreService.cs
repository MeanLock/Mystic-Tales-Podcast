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
using UserService.Common.AppConfigurations.BusinessSetting.interfaces;
using UserService.BusinessLogic.DTOs.Survey.ListItems;
using UserService.BusinessLogic.DTOs.Survey.Filters;
using UserService.BusinessLogic.DTOs.Survey;
using UserService.BusinessLogic.DTOs.Survey.Details;
using UserService.BusinessLogic.DTOs.Survey.JsonConfigs;
using UserService.BusinessLogic.DTOs.FilterTag;
using UserService.BusinessLogic.Services.EmbeddingVectorServices;
using UserService.BusinessLogic.DTOs.Survey.Publishment;
using UserService.BusinessLogic.Exceptions;
using UserService.BusinessLogic.Services.OpenAIServices;
using UserService.BusinessLogic.Helpers.AuthHelpers;
using UserService.BusinessLogic.Helpers.FileHelpers;
using UserService.BusinessLogic.Helpers.DateHelpers;

namespace UserService.BusinessLogic.Services.DbServices.SurveyServices
{
    public class SurveyCoreService
    {
        // LOGGER
        private readonly ILogger<SurveyCoreService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly ISurveyConfig _surveyConfig;
        private readonly IEmbeddingVectorModelConfig _embeddingVectorModelConfig;

        // DB CONTEXT
        private readonly AppDbContext _appDbContext;
        private readonly PostgresDbContext _postgresDbContext;

        // Helper
        private readonly BcryptHelper _bcryptHelper;
        private readonly JwtHelper _jwtHelper;
        private readonly FileIOHelper _fileIOHelper;
        private readonly DateHelper _dateHelper;

        // UNIT OF WORK
        private readonly IUnitOfWork _unitOfWork;

        // REPOSITORIES
        private readonly IGenericRepository<Account> _accountGenericRepository;
        private readonly IGenericRepository<Survey> _surveyGenericRepository;
        private readonly IGenericRepository<SurveyStatus> _surveyStatusGenericRepository;
        private readonly IGenericRepository<SurveyStatusTracking> _surveyStatusTrackingGenericRepository;
        private readonly IGenericRepository<SurveyQuestion> _surveyQuestionGenericRepository;
        private readonly IGenericRepository<SurveyOption> _surveyOptionGenericRepository;
        private readonly IGenericRepository<SurveyTakenResult> _surveyTakenResultGenericRepository;
        private readonly IGenericRepository<SurveyResponse> _surveyResponseGenericRepository;
        private readonly IGenericRepository<FilterTag> _filterTagGenericRepository;
        private readonly IGenericRepository<SurveyTakenResultTagFilter> _surveyTakenResultTagFilterGenericRepository;
        private readonly IGenericRepository<SurveyDefaultBackgroundTheme> _surveyDefaultBackgroundThemeGenericRepository;
        private readonly IGenericRepository<SurveyTopic> _surveyTopicGenericRepository;
        private readonly IGenericRepository<SurveySpecificTopic> _surveySpecificTopicGenericRepository;
        private readonly IGenericRepository<SurveySecurityMode> _surveySecurityModeGenericRepository;
        private readonly IGenericRepository<SurveyQuestionType> _surveyQuestionTypeGenericRepository;
        private readonly IGenericRepository<SurveyFieldInputType> _surveyFieldInputTypeGenericRepository;
        private readonly IGenericRepository<SurveyRewardTracking> _surveyRewardTrackingGenericRepository;


        // OPENAI SERVICE
        private readonly SurveyOpenAIService _surveyOpenAIService;

        // EMBEDDING VECTOR SERVICE
        private readonly SurveyEmbeddingVectorService _surveyEmbeddingVectorService;



        public SurveyCoreService(
            ILogger<SurveyCoreService> logger,
            AppDbContext appDbContext,
            PostgresDbContext postgresDbContext,
            BcryptHelper bcryptHelper,
            JwtHelper jwtHelper,
            IUnitOfWork unitOfWork,

            IGenericRepository<Account> accountGenericRepository,
            IGenericRepository<Survey> surveyGenericRepository,
            IGenericRepository<SurveyStatus> surveyStatusGenericRepository,
            IGenericRepository<SurveyStatusTracking> surveyStatusTrackingGenericRepository,
            IGenericRepository<SurveyQuestion> surveyQuestionGenericRepository,
            IGenericRepository<SurveyOption> surveyOptionGenericRepository,
            IGenericRepository<SurveyTakenResult> surveyTakenResultGenericRepository,
            IGenericRepository<SurveyResponse> surveyResponseGenericRepository,
            IGenericRepository<FilterTag> filterTagGenericRepository,
            IGenericRepository<SurveyTakenResultTagFilter> surveyTakenResultTagFilterGenericRepository,
            IGenericRepository<SurveyDefaultBackgroundTheme> surveyDefaultBackgroundThemeGenericRepository,
            IGenericRepository<SurveyTopic> surveyTopicGenericRepository,
            IGenericRepository<SurveySpecificTopic> surveySpecificTopicGenericRepository,
            IGenericRepository<SurveySecurityMode> surveySecurityModeGenericRepository,
            IGenericRepository<SurveyQuestionType> surveyQuestionTypeGenericRepository,
            IGenericRepository<SurveyFieldInputType> surveyFieldInputTypeGenericRepository,
            IGenericRepository<SurveyRewardTracking> surveyRewardTrackingGenericRepository,

            FileIOHelper fileIOHelper,
            DateHelper dateHelper,
            IFilePathConfig filePathConfig,
            SurveyOpenAIService surveyOpenAIService,
            SurveyEmbeddingVectorService surveyEmbeddingVectorService,
            IAppConfig appConfig,
            ISurveyConfig surveyConfig,
            IEmbeddingVectorModelConfig embeddingVectorModelConfig
            )
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _postgresDbContext = postgresDbContext;
            _bcryptHelper = bcryptHelper;
            _jwtHelper = jwtHelper;
            _fileIOHelper = fileIOHelper;
            _dateHelper = dateHelper;
            _unitOfWork = unitOfWork;

            _accountGenericRepository = accountGenericRepository;
            _surveyGenericRepository = surveyGenericRepository;
            _surveyStatusGenericRepository = surveyStatusGenericRepository;
            _surveyStatusTrackingGenericRepository = surveyStatusTrackingGenericRepository;
            _surveyQuestionGenericRepository = surveyQuestionGenericRepository;
            _surveyOptionGenericRepository = surveyOptionGenericRepository;
            _surveyTakenResultGenericRepository = surveyTakenResultGenericRepository;
            _surveyResponseGenericRepository = surveyResponseGenericRepository;
            _filterTagGenericRepository = filterTagGenericRepository;
            _surveyTakenResultTagFilterGenericRepository = surveyTakenResultTagFilterGenericRepository;
            _surveyDefaultBackgroundThemeGenericRepository = surveyDefaultBackgroundThemeGenericRepository;
            _surveyTopicGenericRepository = surveyTopicGenericRepository;
            _surveySpecificTopicGenericRepository = surveySpecificTopicGenericRepository;
            _surveySecurityModeGenericRepository = surveySecurityModeGenericRepository;
            _surveyQuestionTypeGenericRepository = surveyQuestionTypeGenericRepository;
            _surveyFieldInputTypeGenericRepository = surveyFieldInputTypeGenericRepository;
            _surveyRewardTrackingGenericRepository = surveyRewardTrackingGenericRepository;

            _fileIOHelper = fileIOHelper;
            _filePathConfig = filePathConfig;
            _surveyConfig = surveyConfig;
            _appConfig = appConfig;
            _embeddingVectorModelConfig = embeddingVectorModelConfig;

            _surveyOpenAIService = surveyOpenAIService;
            _surveyEmbeddingVectorService = surveyEmbeddingVectorService;


        }

        public async Task<List<T>> GenerateSurveyListIntoDTO<T>(IEnumerable<Survey> surveys, bool isSurveyPrivateDataTaken = false) where T : class
        {
            try
            {
                if (surveys == null || !surveys.Any())
                {
                    return new List<T>();
                }
                var result = new List<T>();
                if (typeof(T) == typeof(FilterSurveyListItemDTO))
                {
                    foreach (var survey in surveys)
                    {
                        var surveyQuestions = await _unitOfWork.SurveyQuestionRepository.FindBySurveyIdAndIsDeletedContainAsync(survey.Id, false);
                        SurveyConfigJsonDTO surveyConfigJson = (JObject.Parse(survey.ConfigJsonString)).ToObject<SurveyConfigJsonDTO>();
                        string backgroundImageUrl = "";
                        if (surveyConfigJson.IsUseBackgroundImageBase64 == true)
                        {
                            backgroundImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString()));
                        }
                        else
                        {
                            backgroundImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_DEFAULT_BACKGROUND_IMAGE_PATH, surveyConfigJson.DefaultBackgroundImageId.ToString()));
                        }
                        var filterSurvey = new FilterSurveyListItemDTO
                        {
                            Id = survey.Id,
                            Title = survey.Title,
                            Description = survey.Description,
                            SurveyTopicId = survey.SurveyTopicId,
                            SurveySpecificTopicId = survey.SurveySpecificTopicId,
                            StartDate = survey.StartDate ?? null,
                            EndDate = survey.EndDate ?? null,
                            SecurityModeId = survey.SecurityModeId,
                            IsAvailable = survey.IsAvailable,
                            PublishedAt = survey.PublishedAt ?? null,
                            SurveyStatusId = survey.SurveyStatusTrackings
                                .OrderByDescending(sst => sst.CreatedAt)
                                .FirstOrDefault()?.SurveyStatusId ?? 1,
                            MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString())),
                            BackgroundImageUrl = backgroundImageUrl,
                            QuestionCount = surveyQuestions.Count(),
                            TakerBaseRewardPrice = (double)(survey.TakerBaseRewardPrice ?? 0),
                            ConfigJsonString = survey.ConfigJsonString,
                            SurveyPrivateData = isSurveyPrivateDataTaken == false ? null :
                            new SurveyPrivateDataDTO
                            {
                                RequesterId = survey.RequesterId,
                                SurveyTypeId = survey.SurveyTypeId,
                                Kpi = survey.Kpi ?? 0,
                                TheoryPrice = (double)(survey.TheoryPrice ?? 0),
                                ExtraPrice = (double)(survey.ExtraPrice ?? 0),

                                ProfitPrice = (double)(survey.ProfitPrice ?? 0),
                                AllocBaseAmount = (double)(survey.AllocBaseAmount ?? 0),
                                AllocTimeAmount = (double)(survey.AllocTimeAmount ?? 0),
                                AllocLevelAmount = (double)(survey.AllocLevelAmount ?? 0),
                                MaxXp = survey.MaxXp ?? 0,

                                DeletedAt = survey.DeletedAt ?? null,
                                CreatedAt = survey.CreatedAt,
                                UpdatedAt = survey.UpdatedAt
                            },
                            CurrentTakenResultCount = await _unitOfWork.SurveyTakenResultRepository.CountBySurveyIdAsync(survey.Id, false)
                        };
                        result.Add((T)(object)filterSurvey);
                    }
                }
                else if (typeof(T) == typeof(CommunitySurveyListItemDTO))
                {
                    foreach (var survey in surveys)
                    {
                        var surveyQuestions = await _unitOfWork.SurveyQuestionRepository.FindBySurveyIdAndIsDeletedContainAsync(survey.Id, false);
                        SurveyConfigJsonDTO surveyConfigJson = (JObject.Parse(survey.ConfigJsonString)).ToObject<SurveyConfigJsonDTO>();
                        string backgroundImageUrl = "";
                        if (surveyConfigJson.IsUseBackgroundImageBase64 == true)
                        {
                            backgroundImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(),"background"));
                        }
                        else
                        {
                            backgroundImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_DEFAULT_BACKGROUND_IMAGE_PATH, surveyConfigJson.DefaultBackgroundImageId.ToString(),"main"));
                        }
                        int currentTakenResultCount = await _unitOfWork.SurveyTakenResultRepository.CountBySurveyIdAsync(survey.Id, false);
                        int surveyStatusId = survey.SurveyStatusTrackings
                            .OrderByDescending(sst => sst.CreatedAt)
                            .FirstOrDefault()?.SurveyStatusId ?? 1;
                        var communitySurvey = new CommunitySurveyListItemDTO
                        {
                            Id = survey.Id,
                            Title = survey.Title,
                            Description = survey.Description,
                            SurveyTopicId = survey.SurveyTopicId,
                            SurveySpecificTopicId = survey.SurveySpecificTopicId,
                            StartDate = survey.StartDate ?? null,
                            EndDate = survey.EndDate ?? null,
                            SecurityModeId = survey.SecurityModeId,
                            IsAvailable = survey.IsAvailable,
                            PublishedAt = survey.PublishedAt ?? null,
                            SurveyStatusId = surveyStatusId,
                            MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "main")),
                            BackgroundImageUrl = backgroundImageUrl,
                            QuestionCount = surveyQuestions.Count(),
                            TakerBaseRewardPrice = (double)(survey.TakerBaseRewardPrice ?? 0),
                            ConfigJsonString = survey.ConfigJsonString,
                            SurveyPrivateData = isSurveyPrivateDataTaken == false ? null :
                            new SurveyPrivateDataDTO
                            {
                                RequesterId = survey.RequesterId,
                                SurveyTypeId = survey.SurveyTypeId,
                                Kpi = survey.Kpi ?? 0,
                                TheoryPrice = (double)(survey.TheoryPrice ?? 0),
                                ExtraPrice = (double)(survey.ExtraPrice ?? 0),

                                ProfitPrice = (double)(survey.ProfitPrice ?? 0),
                                AllocBaseAmount = (double)(survey.AllocBaseAmount ?? 0),
                                AllocTimeAmount = (double)(survey.AllocTimeAmount ?? 0),
                                AllocLevelAmount = (double)(survey.AllocLevelAmount ?? 0),
                                MaxXp = survey.MaxXp ?? 0,

                                DeletedAt = survey.DeletedAt ?? null,
                                CreatedAt = survey.CreatedAt,
                                UpdatedAt = survey.UpdatedAt
                            },
                            CurrentTakenResultCount = currentTakenResultCount,
                            AvailableTakenResultSlot = surveyStatusId != 1 ? ((survey.Kpi ?? 0) - currentTakenResultCount) : 0,
                            CurrentSurveyRewardTracking = survey.SurveyRewardTrackings.Select(srt => new SurveyRewardTracking
                            {
                                Id = srt.Id,
                                SurveyId = srt.SurveyId,
                                RewardPrice = srt.RewardPrice,
                                RewardXp = srt.RewardXp,
                                CreatedAt = srt.CreatedAt
                            }).OrderByDescending(sst => sst.CreatedAt)
                                .FirstOrDefault(),

                        };
                        result.Add((T)(object)communitySurvey);
                    }
                }
                // else if (typeof(T) == typeof(OtherDTO)) { ... }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new Exception("không xác định");
            }
        }

        public async Task<T?> GenerateSurveyListItemIntoDTO<T>(Survey survey, bool isSurveyPrivateDataTaken = false) where T : class
        {
            try
            {
                if (survey == null)
                {
                    return null;
                }
                if (typeof(T) == typeof(FilterSurveyListItemDTO))
                {
                    var surveyQuestions = await _unitOfWork.SurveyQuestionRepository.FindBySurveyIdAndIsDeletedContainAsync(survey.Id, false);
                    SurveyConfigJsonDTO surveyConfigJson = (JObject.Parse(survey.ConfigJsonString)).ToObject<SurveyConfigJsonDTO>();
                    string backgroundImageUrl = "";
                    if (surveyConfigJson.IsUseBackgroundImageBase64 == true)
                    {
                        backgroundImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "background")));
                    }
                    else
                    {
                        backgroundImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_DEFAULT_BACKGROUND_IMAGE_PATH, surveyConfigJson.DefaultBackgroundImageId.ToString(), "main")));
                    }
                    var filterSurvey = new FilterSurveyListItemDTO
                    {
                        Id = survey.Id,
                        Title = survey.Title,
                        Description = survey.Description,
                        SurveyTopicId = survey.SurveyTopicId,
                        SurveySpecificTopicId = survey.SurveySpecificTopicId,
                        StartDate = survey.StartDate ?? null,
                        EndDate = survey.EndDate ?? null,
                        SecurityModeId = survey.SecurityModeId,
                        IsAvailable = survey.IsAvailable,
                        PublishedAt = survey.PublishedAt ?? null,
                        SurveyStatusId = survey.SurveyStatusTrackings
                            .OrderByDescending(sst => sst.CreatedAt)
                            .FirstOrDefault()?.SurveyStatusId ?? 1,
                        MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "main")),
                        BackgroundImageUrl = backgroundImageUrl,
                        QuestionCount = surveyQuestions.Count(),
                        TakerBaseRewardPrice = (double)(survey.TakerBaseRewardPrice ?? 0),
                        ConfigJsonString = survey.ConfigJsonString,
                        SurveyPrivateData = isSurveyPrivateDataTaken == false ? null :
                        new SurveyPrivateDataDTO
                        {
                            RequesterId = survey.RequesterId,
                            SurveyTypeId = survey.SurveyTypeId,
                            Kpi = survey.Kpi ?? 0,
                            TheoryPrice = (double)(survey.TheoryPrice ?? 0),
                            ExtraPrice = (double)(survey.ExtraPrice ?? 0),
                            ProfitPrice = (double)(survey.ProfitPrice ?? 0),
                            AllocBaseAmount = (double)(survey.AllocBaseAmount ?? 0),
                            AllocTimeAmount = (double)(survey.AllocTimeAmount ?? 0),
                            AllocLevelAmount = (double)(survey.AllocLevelAmount ?? 0),
                            MaxXp = survey.MaxXp ?? 0,
                            DeletedAt = survey.DeletedAt ?? null,
                            CreatedAt = survey.CreatedAt,
                            UpdatedAt = survey.UpdatedAt
                        },
                        CurrentTakenResultCount = await _unitOfWork.SurveyTakenResultRepository.CountBySurveyIdAsync(survey.Id, false)
                    };
                    return filterSurvey as T;
                }
                else if (typeof(T) == typeof(CommunitySurveyListItemDTO))
                {
                    var surveyQuestions = await _unitOfWork.SurveyQuestionRepository.FindBySurveyIdAndIsDeletedContainAsync(survey.Id, false);
                    SurveyConfigJsonDTO surveyConfigJson = (JObject.Parse(survey.ConfigJsonString)).ToObject<SurveyConfigJsonDTO>();
                    string backgroundImageUrl = "";
                    if (surveyConfigJson.IsUseBackgroundImageBase64 == true)
                    {
                        backgroundImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "background")));
                    }
                    else
                    {
                        backgroundImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_DEFAULT_BACKGROUND_IMAGE_PATH, surveyConfigJson.DefaultBackgroundImageId.ToString(), "main")));
                    }
                    int currentTakenResultCount = await _unitOfWork.SurveyTakenResultRepository.CountBySurveyIdAsync(survey.Id, false);
                    int surveyStatusId = survey.SurveyStatusTrackings
                        .OrderByDescending(sst => sst.CreatedAt)
                        .FirstOrDefault()?.SurveyStatusId ?? 1;
                    var communitySurvey = new CommunitySurveyListItemDTO
                    {
                        Id = survey.Id,
                        Title = survey.Title,
                        Description = survey.Description,
                        SurveyTopicId = survey.SurveyTopicId,
                        SurveySpecificTopicId = survey.SurveySpecificTopicId,
                        StartDate = survey.StartDate ?? null,
                        EndDate = survey.EndDate ?? null,
                        SecurityModeId = survey.SecurityModeId,
                        IsAvailable = survey.IsAvailable,
                        PublishedAt = survey.PublishedAt ?? null,
                        SurveyStatusId = surveyStatusId,
                        MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "main")),
                        BackgroundImageUrl = backgroundImageUrl,
                        QuestionCount = surveyQuestions.Count(),
                        TakerBaseRewardPrice = (double)(survey.TakerBaseRewardPrice ?? 0),
                        ConfigJsonString = survey.ConfigJsonString,
                        SurveyPrivateData = isSurveyPrivateDataTaken == false ? null :
                        new SurveyPrivateDataDTO
                        {
                            RequesterId = survey.RequesterId,
                            SurveyTypeId = survey.SurveyTypeId,
                            Kpi = survey.Kpi ?? 0,
                            TheoryPrice = (double)(survey.TheoryPrice ?? 0),
                            ExtraPrice = (double)(survey.ExtraPrice ?? 0),
                            ProfitPrice = (double)(survey.ProfitPrice ?? 0),
                            AllocBaseAmount = (double)(survey.AllocBaseAmount ?? 0),
                            AllocTimeAmount = (double)(survey.AllocTimeAmount ?? 0),
                            AllocLevelAmount = (double)(survey.AllocLevelAmount ?? 0),
                            MaxXp = survey.MaxXp ?? 0,
                            DeletedAt = survey.DeletedAt ?? null,
                            CreatedAt = survey.CreatedAt,
                            UpdatedAt = survey.UpdatedAt
                        },
                        CurrentTakenResultCount = currentTakenResultCount,
                        AvailableTakenResultSlot = surveyStatusId != 1 ? ((survey.Kpi ?? 0) - currentTakenResultCount) : 0,
                        CurrentSurveyRewardTracking = survey.SurveyRewardTrackings.Select(srt => new SurveyRewardTracking
                        {
                            Id = srt.Id,
                            SurveyId = srt.SurveyId,
                            RewardPrice = srt.RewardPrice,
                            RewardXp = srt.RewardXp,
                            CreatedAt = srt.CreatedAt
                        }).OrderByDescending(sst => sst.CreatedAt)
                            .FirstOrDefault(),
                    };
                    return communitySurvey as T;
                }
                // else if (typeof(T) == typeof(OtherDTO)) { ... }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new Exception("không xác định");
            }
        }


        public async Task<T> GenerateSurveyDetailIntoDTO<T>(Survey survey, bool isSurveyPrivateDataTaken = false) where T : class
        {
            try
            {
                if (typeof(T) == typeof(FilterSurveyDetailDTO))
                {
                    var surveyQuestions = await _unitOfWork.SurveyQuestionRepository.FindBySurveyIdAndIsDeletedContainAsync(survey.Id, false);
                    SurveyConfigJsonDTO surveyConfigJson = (JObject.Parse(survey.ConfigJsonString)).ToObject<SurveyConfigJsonDTO>();
                    // Generate background image URL based on the survey configuration
                    string backgroundImageUrl = "";
                    if (surveyConfigJson.IsUseBackgroundImageBase64 == true)
                    {
                        backgroundImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "background"));
                    }
                    else
                    {
                        backgroundImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_DEFAULT_BACKGROUND_IMAGE_PATH, surveyConfigJson.DefaultBackgroundImageId.ToString(), "main"));
                    }
                    var filterSurveyDetail = new FilterSurveyDetailDTO
                    {
                        Id = survey.Id,
                        Title = survey.Title,
                        Description = survey.Description,
                        SurveyTopicId = survey.SurveyTopicId,
                        SurveySpecificTopicId = survey.SurveySpecificTopicId,
                        StartDate = survey.StartDate?.ToString("yyyy-MM-dd") ?? "",
                        EndDate = survey.EndDate?.ToString("yyyy-MM-dd") ?? "",
                        SecurityModeId = survey.SecurityModeId,
                        IsAvailable = survey.IsAvailable,
                        PublishedAt = survey.PublishedAt?.ToString("yyyy-MM-dd") ?? "",
                        SurveyStatusId = survey.SurveyStatusTrackings
                            .OrderByDescending(sst => sst.CreatedAt)
                            .FirstOrDefault()?.SurveyStatusId ?? 1,
                        MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "main")),
                        BackgroundImageUrl = backgroundImageUrl,
                        QuestionCount = surveyQuestions.Count(),
                        TakerBaseRewardPrice = (double)(survey.TakerBaseRewardPrice ?? 0),
                        ConfigJsonString = survey.ConfigJsonString,
                        CurrentTakenResultCount = await _unitOfWork.SurveyTakenResultRepository.CountBySurveyIdAsync(survey.Id, false),
                        Questions = JArray.FromObject(
                            await Task.WhenAll(surveyQuestions.Select(async sq =>
                            {
                                return new
                                {
                                    sq.Id,
                                    sq.SurveyId,
                                    sq.QuestionTypeId,
                                    sq.Content,
                                    sq.Description,
                                    sq.TimeLimit,
                                    sq.IsVoiced,
                                    sq.Order,
                                    sq.ConfigJsonString,
                                    sq.DeletedAt,
                                    MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString() + "/question_" + sq.Id, "main")),
                                    Options = JArray.FromObject(
                                    await Task.WhenAll(sq.SurveyOptions.Select(async so => new
                                    {
                                        so.Id,
                                        so.SurveyQuestionId,
                                        so.Content,
                                        so.Order,
                                        MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString() + "/question_" + sq.Id + "/option_" + so.Id, "main"))
                                    })
                                    )
                                )
                                };
                            }

                            ))
                        ),
                        SurveyPrivateData = isSurveyPrivateDataTaken == false ? null : new SurveyPrivateDataDTO
                        {
                            RequesterId = survey.RequesterId,
                            SurveyTypeId = survey.SurveyTypeId,
                            Kpi = survey.Kpi ?? 0,
                            TheoryPrice = (double)(survey.TheoryPrice ?? 0),
                            ExtraPrice = (double)(survey.ExtraPrice ?? 0),

                            ProfitPrice = (double)(survey.ProfitPrice ?? 0),
                            AllocBaseAmount = (double)(survey.AllocBaseAmount ?? 0),
                            AllocTimeAmount = (double)(survey.AllocTimeAmount ?? 0),
                            AllocLevelAmount = (double)(survey.AllocLevelAmount ?? 0),
                            MaxXp = survey.MaxXp ?? 0,
                            DeletedAt = survey.DeletedAt ?? null,
                            CreatedAt = survey.CreatedAt,
                            UpdatedAt = survey.UpdatedAt
                        }
                    };
                    return filterSurveyDetail as T;
                }
                else if (typeof(T) == typeof(CommunitySurveyDetailDTO))
                {
                    var surveyTakenResults = await _unitOfWork.SurveyTakenResultRepository.FindBySurveyIdAsync(survey.Id, false);
                    var surveyQuestions = await _unitOfWork.SurveyQuestionRepository.FindBySurveyIdAndIsDeletedContainAsync(survey.Id, false);
                    SurveyConfigJsonDTO surveyConfigJson = (JObject.Parse(survey.ConfigJsonString)).ToObject<SurveyConfigJsonDTO>();
                    // Generate background image URL based on the survey configuration
                    string backgroundImageUrl = "";
                    if (surveyConfigJson.IsUseBackgroundImageBase64 == true)
                    {
                        backgroundImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "background"));
                    }
                    else
                    {
                        backgroundImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_DEFAULT_BACKGROUND_IMAGE_PATH, surveyConfigJson.DefaultBackgroundImageId.ToString(), "main"));
                    }
                    int currentTakenResultCount = await _unitOfWork.SurveyTakenResultRepository.CountBySurveyIdAsync(survey.Id, false);
                    int surveyStatusId = survey.SurveyStatusTrackings
                            .OrderByDescending(sst => sst.CreatedAt)
                            .FirstOrDefault()?.SurveyStatusId ?? 1;
                    var communitySurveyDetail = new CommunitySurveyDetailDTO
                    {
                        Id = survey.Id,
                        Title = survey.Title,
                        Description = survey.Description,
                        SurveyTopicId = survey.SurveyTopicId,
                        SurveySpecificTopicId = survey.SurveySpecificTopicId,
                        StartDate = survey.StartDate?.ToString("yyyy-MM-dd") ?? "",
                        EndDate = survey.EndDate?.ToString("yyyy-MM-dd") ?? "",
                        SecurityModeId = survey.SecurityModeId,
                        IsAvailable = survey.IsAvailable,
                        PublishedAt = survey.PublishedAt?.ToString("yyyy-MM-dd") ?? "",
                        SurveyStatusId = survey.SurveyStatusTrackings
                            .OrderByDescending(sst => sst.CreatedAt)
                            .FirstOrDefault()?.SurveyStatusId ?? 1,
                        MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "main")),
                        BackgroundImageUrl = backgroundImageUrl,
                        QuestionCount = surveyQuestions.Count(),
                        TakerBaseRewardPrice = (double)(survey.TakerBaseRewardPrice ?? 0),
                        ConfigJsonString = survey.ConfigJsonString,
                        CurrentTakenResultCount = currentTakenResultCount,
                        AvailableTakenResultSlot = surveyStatusId != 1 ? ((survey.Kpi ?? 0) - currentTakenResultCount) : 0,
                        Questions = JArray.FromObject(
                            await Task.WhenAll(surveyQuestions.Select(async sq =>
                            {
                                return new
                                {
                                    sq.Id,
                                    sq.SurveyId,
                                    sq.QuestionTypeId,
                                    sq.Content,
                                    sq.Description,
                                    sq.TimeLimit,
                                    sq.IsVoiced,
                                    sq.Order,
                                    sq.ConfigJsonString,
                                    sq.DeletedAt,
                                    MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "question_" + sq.Id, "main")),
                                    Options = JArray.FromObject(
                                    await Task.WhenAll(sq.SurveyOptions.Select(async so => new
                                    {
                                        so.Id,
                                        so.SurveyQuestionId,
                                        so.Content,
                                        so.Order,
                                        MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_ORIGINAL_IMAGE_PATH, survey.Id.ToString(), "question_" + sq.Id, "option_" + so.Id, "main"))
                                    })
                                    )
                                )
                                };
                            }

                            ))
                        ),
                        SurveyPrivateData = isSurveyPrivateDataTaken == false ? null : new SurveyPrivateDataDTO
                        {
                            RequesterId = survey.RequesterId,
                            SurveyTypeId = survey.SurveyTypeId,
                            Kpi = survey.Kpi ?? 0,
                            TheoryPrice = (double)(survey.TheoryPrice ?? 0),
                            ExtraPrice = (double)(survey.ExtraPrice ?? 0),

                            ProfitPrice = (double)(survey.ProfitPrice ?? 0),
                            AllocBaseAmount = (double)(survey.AllocBaseAmount ?? 0),
                            AllocTimeAmount = (double)(survey.AllocTimeAmount ?? 0),
                            AllocLevelAmount = (double)(survey.AllocLevelAmount ?? 0),
                            MaxXp = survey.MaxXp ?? 0,
                            DeletedAt = survey.DeletedAt ?? null,
                            CreatedAt = survey.CreatedAt,
                            UpdatedAt = survey.UpdatedAt
                        },
                        SurveyTakenResults = (await Task.WhenAll(surveyTakenResults.Select(async str => new SurveyTakenResultListItemDTO
                        {
                            Id = str.Id,
                            IsValid = str.IsValid,
                            CompletedAt = str.CompletedAt,
                            InvalidReason = str.InvalidReason,
                            MoneyEarned = str.MoneyEarned,
                            XpEarned = str.XpEarned,
                            Taker = new TakerListItemDTO
                            {
                                Id = str.Taker.Id,
                                FullName = str.Taker.FullName,
                                Dob = str.Taker.Dob,
                                Gender = str.Taker.Gender,
                                MainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.ACCOUNt_IMAGE_PATH, str.Taker.Id.ToString(), "main"))
                            },
                        }))).ToList(),
                        SurveyRewardTrackings = survey.SurveyRewardTrackings.Select(srt => new SurveyRewardTracking
                        {
                            Id = srt.Id,
                            SurveyId = srt.SurveyId,
                            RewardPrice = srt.RewardPrice,
                            RewardXp = srt.RewardXp,
                            CreatedAt = srt.CreatedAt
                        }).OrderByDescending(sst => sst.CreatedAt)
                            .ToList(),
                    };
                    return communitySurveyDetail as T;
                }

                // else if (typeof(T) == typeof(OtherDetailDTO)) { ... }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.Message + "\n");
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new Exception("không xác định");
            }
        }

        public async Task<Survey> GetExistSurveyByIdAndFilterObject(int surveyId, SurveyFilterObject surveyFilterObject = null)
        {
            // Console.WriteLine("surveyId: " + surveyId.ToString());
            // Console.WriteLine("surveyFilterObject: " + surveyFilterObject.ToString());
            var survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
            // Console.WriteLine("survey: " + survey.Title);
            if (survey == null)
            {
                throw new Exception("survey theo điều kiện và có id " + surveyId.ToString() + " không tồn tại");
            }
            return survey;
        }

        public async Task<string> GetDefaultSurveyConfigJsonString()
        {
            var firstSurveyDefaultBackgroundTheme = await _surveyDefaultBackgroundThemeGenericRepository.FindByIdAsync(1);
            SurveyDefaultBackgroundThemeConfigJsonDTO firstSurveyDefaultBackgroundThemeConfigJson = (JObject.Parse(firstSurveyDefaultBackgroundTheme.ConfigJsonString)).ToObject<SurveyDefaultBackgroundThemeConfigJsonDTO>();
            SurveyConfigJsonDTO surveyDefaultSurveyConfigJson = _surveyConfig.DefaultSurveyConfigJson.ToObject<SurveyConfigJsonDTO>();
            var newJsonConfigString = JObject.FromObject(new SurveyConfigJsonDTO
            {
                Background = surveyDefaultSurveyConfigJson.Background,
                IsUseBackgroundImageBase64 = surveyDefaultSurveyConfigJson.IsUseBackgroundImageBase64,
                IsPause = surveyDefaultSurveyConfigJson.IsPause,
                BackgroundGradient1Color = surveyDefaultSurveyConfigJson.BackgroundGradient1Color,
                BackgroundGradient2Color = surveyDefaultSurveyConfigJson.BackgroundGradient2Color,
                TitleColor = firstSurveyDefaultBackgroundThemeConfigJson.TitleColor,
                ContentColor = firstSurveyDefaultBackgroundThemeConfigJson.ContentColor,
                ButtonBackgroundColor = firstSurveyDefaultBackgroundThemeConfigJson.ButtonBackgroundColor,
                ButtonContentColor = firstSurveyDefaultBackgroundThemeConfigJson.ButtonContentColor,
                Password = surveyDefaultSurveyConfigJson.Password,
                Brightness = surveyDefaultSurveyConfigJson.Brightness,
                DefaultBackgroundImageId = firstSurveyDefaultBackgroundTheme.Id,
                SkipStartPage = surveyDefaultSurveyConfigJson.SkipStartPage
            }).ToString(Formatting.None);
            return newJsonConfigString;
        }

        /// <summary>
        /// Tính tần suất hoạt động survey (R) cho danh sách accountIds trong 30 ngày gần nhất.
        /// </summary>
        /// <param name="accountIds">Danh sách accountId cần tính</param>
        /// <returns>Giá trị R = DAC * TR</returns>
        public async Task<float> CalculateSurveyTakingFrequencyRateByAccountIds(List<int> accountIds, int? surveyTopicId)
        {
            var systemConfigProfile = await _unitOfWork.SystemConfigProfileRepository.FindActiveProfileAsync();
            int dailyActiveCountPeriod = systemConfigProfile.AccountGeneralConfig.DailyActiveCountPeriod;
            var now = _dateHelper.GetNowByAppTimeZone().Date;
            var dacList = new List<int>(); // Số người DAC mỗi ngày
            var trList = new List<float>(); // Tỉ lệ TR mỗi ngày
            // var allDACn = new HashSet<int>(); // Tập hợp người DAC trong 30 ngày
            // dateonly của ngày thứ 30 về quá khứ của hôm nay
            DateOnly startDate = DateOnly.FromDateTime(now.AddDays(-dailyActiveCountPeriod));

            // for (int i = 0; i < 30; i++)
            // {
            //     var day = now.AddDays(-i);
            //     // Lấy tracking online của các account trong ngày này
            //     var dacAccounts = await _unitOfWork.AccountOnlineTrackingRepository.FindByAccountIdsAndDateAsync(accountIds, DateOnly.FromDateTime(day));
            //     dacList.Add(dacAccounts.Count());
            //     // allDACn.UnionWith(dacAccounts.Select(x => x.AccountId).ToList());

            //     if (dacAccounts.Count() == 0)
            //     {
            //         trList.Add(0);
            //         Console.WriteLine($"Ngày: {day.ToString("yyyy-MM-dd")}, DAC: 0, DAT: 0");
            //         continue;
            //     }

            //     // Lấy những người DAC có ít nhất 1 taken result trong ngày đó
            //     var takenResults = await _unitOfWork.SurveyTakenResultRepository.FindByAccountIdsAndDateAsync(
            //         dacAccounts.Select(x => x.AccountId).ToList(),
            //         DateOnly.FromDateTime(day),
            //         false
            //     );
            //     // var datAccounts = takenResults.Select(x => x.TakerId).Distinct().ToList();
            //     var datAccounts = takenResults
            //         // .Where(x => x.Survey.SurveyTopicId == surveyTopicId) // Lọc theo survey topic
            //         .Where(x => surveyTopicId == null || x.Survey.SurveyTopicId == surveyTopicId)
            //         .Select(x => x.TakerId)
            //         .Distinct()
            //         .ToList();
            //     float trn = dacAccounts.Count() > 0 ? (float)datAccounts.Count() / dacAccounts.Count() : 0;
            //     trList.Add(trn);

            //     // in kết quả tổng hết ngày
            //     Console.WriteLine($"Ngày: {day.ToString("yyyy-MM-dd")}, DAC: {dacAccounts.Count()}, DAT: {datAccounts.Count()}");
            // }


            var surveyTakenResults = await _unitOfWork.SurveyTakenResultRepository.FindByAccountIdsAndDatePeriodAsync(
                accountIds,
                startDate,
                DateOnly.FromDateTime(now),
                false
            );
            for (int i = 0; i < dailyActiveCountPeriod; i++)
            {
                var day = now.AddDays(-i);
                // DAC: số người có ít nhất 1 taken result trong ngày này
                var dacAccounts = surveyTakenResults
                    .Where(str => str.CompletedAt.Date == day)
                    .Select(str => str.TakerId)
                    .Distinct()
                    .ToList();
                dacList.Add(dacAccounts.Count);

                if (dacAccounts.Count == 0)
                {
                    trList.Add(0);
                    // Console.WriteLine($"Ngày: {day:yyyy-MM-dd}, DAC: 0, TR: 0");
                    continue;
                }

                // DAT: số người DAC có taken result đúng surveyTopicId (nếu có)
                var datAccounts = surveyTakenResults
                    .Where(str => str.CompletedAt.Date == day && (surveyTopicId == null || str.Survey.SurveyTopicId == surveyTopicId) && dacAccounts.Contains(str.TakerId))
                    .Select(str => str.TakerId)
                    .Distinct()
                    .ToList();
                // float trn = dacAccounts.Count > 0 ? (float)datAccounts.Count / dacAccounts.Count : 0;
                float trn = dacAccounts.Count > 0 ? (float)datAccounts.Count / (float)dacAccounts.Count : 0;


                trList.Add(trn);

            }

            // Tính trung bình DAC và TR
            float avgDAC = dacList.Count > 0 ? (float)dacList.Average() : 0;
            float avgTR = trList.Count > 0 ? trList.Average() : 0;

            float R = avgDAC * avgTR;
            return R == 0 ? 0.07f : R;
        }


        /////////////////////////////////////////////////////////////

        public async Task<int> CreateFilterSurvey(int creatorId)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    Survey newSurvey = new Survey
                    {
                        RequesterId = creatorId,
                        SurveyTypeId = 1,
                        Title = "",
                        Description = "",
                        IsAvailable = false,
                        // ConfigJsonString = _surveyConfig.DefaultSurveyConfigJson.ToString(),
                        ConfigJsonString = await GetDefaultSurveyConfigJsonString(),
                    };

                    var survey = await _surveyGenericRepository.CreateAsync(newSurvey);

                    SurveyStatusTracking newSurveyStatusTracking = new SurveyStatusTracking
                    {
                        SurveyId = survey.Id,
                        SurveyStatusId = 1
                    };
                    await _surveyStatusTrackingGenericRepository.CreateAsync(newSurveyStatusTracking);

                    await transaction.CommitAsync();
                    return survey.Id;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Tạo filter survey thất bại, lỗi: " + ex.Message);
                }
            }



        }

        public async Task<List<FilterSurveyListItemDTO>> GetFilterSurveys(int roleId)
        {
            try
            {
                SurveyFilterObject surveyFilterObject = new SurveyFilterObject
                {
                    SurveyTypeId = 1, // Filter survey type id
                    SurveyStatusIds = new List<int> { 1, 2, 3, 4, 5 }, // Only get surveys with status "Draft"
                    IsDeletedContain = true,
                    // IsAvailable = true,
                };
                bool isSurveyPrivateDataTaken = true;

                if (roleId == 4)
                {
                    surveyFilterObject = new SurveyFilterObject
                    {
                        IsAvailable = true,
                        SurveyTypeId = 1, // Filter survey type id
                        SurveyStatusIds = new List<int> { 2 }, // Only get surveys with status "Draft"
                        IsDeletedContain = false,
                    };
                    isSurveyPrivateDataTaken = false;
                }

                var surveys = await _unitOfWork.SurveyRepository.FindByFilterObjectAsync(surveyFilterObject);
                var filterSurveyList = await GenerateSurveyListIntoDTO<FilterSurveyListItemDTO>(surveys, isSurveyPrivateDataTaken);
                return filterSurveyList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.Message + "\n");
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lấy danh sách filter surveys thất bại, lỗi: " + ex.Message);
            }
        }

        public async Task<FilterSurveyDetailDTO> GetFilterSurveyDetail(int surveyId)
        {
            try
            {
                SurveyFilterObject surveyFilterObject = new SurveyFilterObject
                {
                    SurveyTypeId = 1,
                    SurveyStatusIds = new List<int> { 1, 2, 3, 4, 5 }, // Only get surveys with status "Draft"
                    IsDeletedContain = true,
                };
                // var survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                var survey = await GetExistSurveyByIdAndFilterObject(surveyId, surveyFilterObject);
                var surveyQuestions = await _unitOfWork.SurveyQuestionRepository.FindBySurveyIdAndIsDeletedContainAsync(survey.Id, false);

                var filterSurveyDetail = await GenerateSurveyDetailIntoDTO<FilterSurveyDetailDTO>(survey, true);

                return filterSurveyDetail;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lấy chi tiết filter survey thất bại, lỗi: " + ex.Message);
            }
        }

        public async Task PublishFilterSurvey(int surveyId)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    SurveyFilterObject existSurveyDetailFilterObject = new SurveyFilterObject
                    {
                        SurveyTypeId = 1, // Filter survey type id
                        IsDeletedContain = false,
                        IsAvailable = false,
                    };
                    SurveyFilterObject existSurveyListFilterObject = new SurveyFilterObject
                    {
                        SurveyTypeId = 1, // Filter survey type id
                        IsDeletedContain = false,
                        IsAvailable = true,
                        SurveyStatusIds = new List<int> { 2 },
                    };

                    var survey = await this.GetExistSurveyByIdAndFilterObject(surveyId, existSurveyDetailFilterObject);
                    var existingSurvey = await _unitOfWork.SurveyRepository.FindByFilterObjectAsync(existSurveyListFilterObject);
                    // chuyển surveyStatusTracking của các existingSurvey thành 5  và isAvailable = false
                    foreach (var existing in existingSurvey)
                    {
                        existing.IsAvailable = false;
                        existing.PublishedAt = null;
                        await _surveyGenericRepository.UpdateAsync(existing.Id, existing);

                        await _surveyStatusTrackingGenericRepository.CreateAsync(new SurveyStatusTracking
                        {
                            SurveyId = existing.Id,
                            SurveyStatusId = 5
                        });

                        var customerAccounts = await _unitOfWork.AccountRepository.FindByRoleIdAsync(4);
                        // Console.WriteLine("customerAccounts: " + customerAccounts.Count());
                        foreach (var customer in customerAccounts)
                        {
                            customer.IsFilterSurveyRequired = true;
                            await _accountGenericRepository.UpdateAsync(customer.Id, customer);
                        }
                    }
                    survey.IsAvailable = true;
                    survey.PublishedAt = _dateHelper.GetNowByAppTimeZone();
                    await _surveyGenericRepository.UpdateAsync(survey.Id, survey);

                    await _surveyStatusTrackingGenericRepository.CreateAsync(new SurveyStatusTracking
                    {
                        SurveyId = survey.Id,
                        SurveyStatusId = 2
                    });

                    await transaction.CommitAsync();

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Đăng filter survey thất bại, lỗi: " + ex.Message);
                }

            }
        }

        public async Task<List<CommunitySurveyListItemDTO>> GetCommunitySurveys(Account account)
        {
            try
            {
                SurveyFilterObject surveyFilterObject = new SurveyFilterObject
                {
                    SurveyTypeId = 2,
                    IsDeletedContain = false,
                    SurveyStatusIds = new List<int> { 2, 3 }
                };
                bool isSurveyPrivateDataTaken = true;

                if (account.RoleId == 4)
                {
                    surveyFilterObject = new SurveyFilterObject
                    {
                        IsAvailable = true,
                        SurveyTypeId = 2,
                        IsDeletedContain = false,
                        SurveyStatusIds = new List<int> { 2 },
                        IsEndDateExceededContain = false,
                    };
                    isSurveyPrivateDataTaken = false;
                }

                var surveys = await _unitOfWork.SurveyRepository.FindByFilterObjectAsync(surveyFilterObject);


                if (account.RoleId == 4)
                {
                    List<SurveyTakenResult> surveyTakenResults = await _surveyTakenResultGenericRepository.FindAll(str => str.TakerId == account.Id && str.IsValid == true).ToListAsync();
                    surveys = surveys
                        .Where(s => !surveyTakenResults.Any(str => str.SurveyId == s.Id) && s.RequesterId != account.Id)
                        .ToList();

                    var accountTagFilters = await _postgresDbContext.TakerEmbeddingVectorTagFilters
                        .Where(x => x.TakerId == account.Id)
                        .Select(x => new EmbeddingVectorFilterTagDTO
                        {
                            FilterTagId = x.FilterTagId,
                            EmbeddingVector = x.EmbeddingVector != null ? x.EmbeddingVector.ToArray() : null
                        })
                        .ToListAsync();

                    // Lấy danh sách SurveyEmbeddingVectorTagFilters theo surveyId từ danh sách surveys và rồi group lại theo CandidateEmbeddingVectorFilterTagsDTO
                    var surveyTagFilters = new List<CandidateEmbeddingVectorFilterTagsDTO>();
                    foreach (var survey in surveys)
                    {
                        var compareAccountProfile = new AccountProfile
                        {
                            CountryRegion = survey.SurveyTakerSegment.CountryRegion,
                            AverageIncome = survey.SurveyTakerSegment.AverageIncome,
                            EducationLevel = survey.SurveyTakerSegment.EducationLevel,
                            JobField = survey.SurveyTakerSegment.JobField,
                            MaritalStatus = survey.SurveyTakerSegment.MaritalStatus
                        };

                        if (_unitOfWork.AccountRepository.CompareAccountProfile(account.AccountProfile, compareAccountProfile) == false)
                        {
                            continue;
                        }
                        Console.WriteLine("survey.Id: " + survey.Id);

                        var embeddingSurveyTagFilters = await _postgresDbContext.SurveyEmbeddingVectorTagFilters
                            .Where(x => x.SurveyId == survey.Id)
                            .Select(x => new EmbeddingVectorFilterTagDTO
                            {
                                FilterTagId = x.FilterTagId,
                                EmbeddingVector = x.EmbeddingVector != null ? x.EmbeddingVector.ToArray() : null
                            })
                            .ToListAsync();
                        surveyTagFilters.Add(new CandidateEmbeddingVectorFilterTagsDTO
                        {
                            CandidateId = survey.Id,
                            EmbeddingVectorFilterTags = embeddingSurveyTagFilters,
                            CandidateTagFilterAccuracyRate = survey.SurveyTakerSegment.TagFilterAccuracyRate
                        });
                    }

                    // Lọc surveys theo accountTagFilters
                    // [CÁCH 1]
                    // List<FilterTagSimilarityComparisonResultDTO> matchedSurvey = await _surveyEmbeddingVectorService.GetSimilarCandidateIdsByEmbeddingVectorFilterTagsWithCandidateAccuracyAsync(
                    // new FilterTagSimilarityComparisonRequestDTO
                    // {
                    //     TargetEmbeddingVectorFilterTags = accountTagFilters,
                    //     CandidateEmbeddingVectorFilterTags = surveyTagFilters,
                    //     MinScore = _embeddingVectorModelConfig.MinScore,
                    //     MaxScore = _embeddingVectorModelConfig.MaxScore
                    // });

                    // [CÁCH 2]
                    List<FilterTagSimilarityComparisonResultDTO> matchedSurvey = _surveyEmbeddingVectorService.FilterCandidatesByTagSimilarityByCandidateAccuracy(
                        new FilterTagSimilarityComparisonRequestDTO
                        {
                            TargetEmbeddingVectorFilterTags = accountTagFilters,
                            CandidateEmbeddingVectorFilterTags = surveyTagFilters,
                            MinScore = _embeddingVectorModelConfig.MinScore,
                            MaxScore = _embeddingVectorModelConfig.MaxScore
                        });

                    List<int> matchedSurveyIds = matchedSurvey.Select(x => x.CandidateId).ToList();
                    surveys = surveys.Where(s => matchedSurveyIds.Contains(s.Id))
                                            .OrderByDescending(s => matchedSurvey.First(x => x.CandidateId == s.Id).SimilarityScore)
                                            .ToList();
                }

                var communitySurveyList = await GenerateSurveyListIntoDTO<CommunitySurveyListItemDTO>(surveys, isSurveyPrivateDataTaken);


                return communitySurveyList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lấy danh community surveys thất bại, lỗi: " + ex.Message);
            }
        }


        public async Task<int> CreateCommunitySurvey(int creatorId)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    Survey newSurvey = new Survey
                    {
                        RequesterId = creatorId,
                        SurveyTypeId = 2,
                        Title = "",
                        Description = "",
                        IsAvailable = false,
                        // ConfigJsonString = _surveyConfig.DefaultSurveyConfigJson.ToString(),
                        SecurityModeId = 1, // Default security mode
                        ConfigJsonString = await GetDefaultSurveyConfigJsonString(),
                    };

                    var survey = await _surveyGenericRepository.CreateAsync(newSurvey);

                    SurveyStatusTracking newSurveyStatusTracking = new SurveyStatusTracking
                    {
                        SurveyId = survey.Id,
                        SurveyStatusId = 1, // Draft status
                    };
                    await _surveyStatusTrackingGenericRepository.CreateAsync(newSurveyStatusTracking);

                    await transaction.CommitAsync();
                    return survey.Id;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Tạo community survey thất bại, lỗi: " + ex.Message);
                }
            }
        }

        public async Task<CommunitySurveyListItemDTO> GetLevelUpdateCommunitySurvey(Account account)
        {
            try
            {
                SurveyFilterObject surveyFilterObject = new SurveyFilterObject
                {
                    SurveyTypeId = 2, // Filter survey type id
                    SurveyStatusIds = new List<int> { 2 }, // Only get surveys with status "Draft"
                    IsDeletedContain = false,
                    IsAvailable = true, // Only get available surveys
                    IsEndDateExceededContain = false, // Only get surveys that have not ended
                };
                bool isSurveyPrivateDataTaken = false;

                var surveys = await _unitOfWork.SurveyRepository.FindByFilterObjectAsync(surveyFilterObject);
                // Console.WriteLine("surveys.Counttttttttttttttttttttt: " + surveys.Count());
                List<SurveyTakenResult> surveyTakenResults = await _surveyTakenResultGenericRepository.FindAll(str => str.TakerId == account.Id && str.IsValid == true).ToListAsync();
                surveys = surveys
                    .Where(s => !surveyTakenResults.Any(str => str.SurveyId == s.Id) && s.RequesterId != account.Id)
                    .ToList();

                var accountTagFilters = await _postgresDbContext.TakerEmbeddingVectorTagFilters
                    .Where(x => x.TakerId == account.Id)
                    .Select(x => new EmbeddingVectorFilterTagDTO
                    {
                        FilterTagId = x.FilterTagId,
                        EmbeddingVector = x.EmbeddingVector != null ? x.EmbeddingVector.ToArray() : null
                    })
                    .ToListAsync();

                var surveyTagFilters = new List<CandidateEmbeddingVectorFilterTagsDTO>();
                foreach (var survey in surveys)
                {
                    var compareAccountProfile = new AccountProfile
                    {
                        CountryRegion = survey.SurveyTakerSegment.CountryRegion,
                        AverageIncome = survey.SurveyTakerSegment.AverageIncome,
                        EducationLevel = survey.SurveyTakerSegment.EducationLevel,
                        JobField = survey.SurveyTakerSegment.JobField,
                        MaritalStatus = survey.SurveyTakerSegment.MaritalStatus
                    };

                    if (_unitOfWork.AccountRepository.CompareAccountProfile(account.AccountProfile, compareAccountProfile) == false)
                    {
                        continue;
                    }
                    Console.WriteLine("survey.Id: " + survey.Id);

                    var embeddingSurveyTagFilters = await _postgresDbContext.SurveyEmbeddingVectorTagFilters
                        .Where(x => x.SurveyId == survey.Id)
                        .Select(x => new EmbeddingVectorFilterTagDTO
                        {
                            FilterTagId = x.FilterTagId,
                            EmbeddingVector = x.EmbeddingVector != null ? x.EmbeddingVector.ToArray() : null
                        })
                        .ToListAsync();
                    surveyTagFilters.Add(new CandidateEmbeddingVectorFilterTagsDTO
                    {
                        CandidateId = survey.Id,
                        EmbeddingVectorFilterTags = embeddingSurveyTagFilters,
                        CandidateTagFilterAccuracyRate = survey.SurveyTakerSegment.TagFilterAccuracyRate
                    });
                }
                List<FilterTagSimilarityComparisonResultDTO> matchedSurvey = _surveyEmbeddingVectorService.FilterCandidatesByTagSimilarityByCandidateAccuracy(
                    new FilterTagSimilarityComparisonRequestDTO
                    {
                        TargetEmbeddingVectorFilterTags = accountTagFilters,
                        CandidateEmbeddingVectorFilterTags = surveyTagFilters,
                        MinScore = _embeddingVectorModelConfig.MinScore,
                        MaxScore = _embeddingVectorModelConfig.MaxScore
                    });

                List<int> matchedSurveyIds = matchedSurvey.Select(x => x.CandidateId).ToList();
                surveys = surveys.Where(s => matchedSurveyIds.Contains(s.Id))
                                        // .OrderByDescending(s => matchedSurvey.First(x => x.CandidateId == s.Id).SimilarityScore)
                                        .OrderBy(s => s.EndDate)
                                        .ToList();
                // GenerateSurveyListItemIntoDTO


                var levelUpdatesurvey = await GenerateSurveyListItemIntoDTO<CommunitySurveyListItemDTO>(surveys.FirstOrDefault(), isSurveyPrivateDataTaken);
                return levelUpdatesurvey;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.Message + "\n");
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lấy danh sách filter surveys thất bại, lỗi: " + ex.Message);
            }
        }

        public async Task<List<CommunitySurveyListItemDTO>> GetOwnCommunitySurveys(int accountId)
        {
            try
            {
                SurveyFilterObject surveyFilterObject = new SurveyFilterObject
                {
                    RequesterId = accountId,
                    SurveyTypeId = 2,
                    IsDeletedContain = false,
                    SurveyStatusIds = new List<int> { 1, 2, 3, 5 }
                };

                var surveys = await _unitOfWork.SurveyRepository.FindByFilterObjectAsync(surveyFilterObject);
                var communitySurveyList = await GenerateSurveyListIntoDTO<CommunitySurveyListItemDTO>(surveys, true);
                foreach (var survey in communitySurveyList)
                {
                    var surveyPrivateData = survey.SurveyPrivateData;
                    survey.SurveyPrivateData = new SurveyPrivateDataDTO
                    {
                        Kpi = surveyPrivateData.Kpi,
                        ExtraPrice = surveyPrivateData.ExtraPrice,
                        CreatedAt = surveyPrivateData.CreatedAt,
                        UpdatedAt = surveyPrivateData.UpdatedAt,
                    };
                }
                return communitySurveyList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lấy danh sách community surveys thất bại, lỗi: " + ex.Message);
            }
        }

        public async Task UpdateCommunitySurvey(int surveyId, dynamic surveyUpdateData)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (surveyUpdateData == null)
                    {
                        throw new HttpRequestException("Dữ liệu cập nhật survey không hợp lệ");
                    }
                    else if (surveyUpdateData.MaxXp == null || surveyUpdateData.MaxXp < 0)
                    {
                        throw new HttpRequestException("Dữ liệu cập nhật survey không hợp lệ, MaxXp là bắt buộc");
                    }
                    SurveyFilterObject existSurveyDetailFilterObject = new SurveyFilterObject
                    {
                        SurveyTypeId = 2, // Community survey type id
                        IsDeletedContain = false,
                        IsAvailable = true,
                        SurveyStatusIds = new List<int> { 2 }
                    };
                    var survey = await this.GetExistSurveyByIdAndFilterObject(surveyId, existSurveyDetailFilterObject);

                    // Update survey properties
                    survey.MaxXp = surveyUpdateData.MaxXp != null ? surveyUpdateData.MaxXp : survey.MaxXp;


                    // thêm SurveyRewardTracking
                    // RewardPrice: (takerBaseRewardPrice +  allocTimeAmount * [(currentDate - startDate)/ (endDate - startDate)]
                    // RewardXp: MaxXp * [1 -  [(currentDate - startDate)/ (endDate - startDate)]]
                    var currentDate = DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone());
                    var startDate = survey.StartDate;
                    var endDate = survey.EndDate;
                    var daysPassed = currentDate.DayNumber - startDate?.DayNumber;
                    var totalDays = endDate?.DayNumber - startDate?.DayNumber;
                    decimal rewardPrice =
                        (survey.TakerBaseRewardPrice ?? 0) +
                        (survey.AllocTimeAmount ?? 0) * (decimal)daysPassed / (totalDays ?? 1);
                    var rewardXp = totalDays.HasValue && totalDays.Value != 0
                        ? (int)Math.Floor((decimal)surveyUpdateData.MaxXp * ((decimal)((1 - daysPassed) / totalDays ?? 1)))
                        : 0;
                    var newSurveyRewardTracking = new SurveyRewardTracking
                    {
                        SurveyId = survey.Id,
                        RewardPrice = rewardPrice,
                        RewardXp = rewardXp,
                    };
                    await _surveyRewardTrackingGenericRepository.CreateAsync(newSurveyRewardTracking);


                    await _surveyGenericRepository.UpdateAsync(survey.Id, survey);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Cập nhật community survey thất bại, lỗi: " + ex.Message);
                }
            }
        }

        public async Task<CommunitySurveyDetailDTO> GetCommunitySurveyDetail(int surveyId, int accountId)
        {
            try
            {

                Account account = await _unitOfWork.AccountRepository.FindByIdAsync(accountId);


                SurveyFilterObject surveyFilterObject = new SurveyFilterObject
                {
                    RequesterId = accountId,
                    SurveyTypeId = 2,
                    IsDeletedContain = false,
                    // IsInvalidTakenResultContain = false,
                    SurveyStatusIds = new List<int> { 1, 2, 3, 5 } // Only get surveys with status "Draft"

                };

                if (account.RoleId != 4) // Customer role
                {
                    surveyFilterObject = new SurveyFilterObject
                    {
                        SurveyTypeId = 2,
                        IsDeletedContain = false,
                        // IsInvalidTakenResultContain = false,
                        SurveyStatusIds = new List<int> { 2 } // Only get surveys with status "Draft"
                    };
                }

                var survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);


                if (account.RoleId == 4 && survey == null)
                {
                    throw new HttpRequestException("bạn không có quyền truy cập vào survey này");
                }
                else if (survey == null)
                {
                    throw new HttpRequestException("survey theo điều kiện và có id " + surveyId.ToString() + " không tồn tại");
                }

                var communitySurveyDetail = await GenerateSurveyDetailIntoDTO<CommunitySurveyDetailDTO>(survey, true);
                if (account.RoleId == 4)
                {
                    var surveyPrivateData = communitySurveyDetail.SurveyPrivateData;
                    communitySurveyDetail.SurveyPrivateData = new SurveyPrivateDataDTO
                    {
                        Kpi = surveyPrivateData.Kpi,
                        ExtraPrice = surveyPrivateData.ExtraPrice,
                        TheoryPrice = surveyPrivateData.TheoryPrice,          
                        CreatedAt = surveyPrivateData.CreatedAt,
                        UpdatedAt = surveyPrivateData.UpdatedAt,
                    };
                    communitySurveyDetail.SurveyRewardTrackings = [];
                }

                return communitySurveyDetail;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lấy chi tiết community survey thất bại, lỗi: " + ex.Message);
            }
        }

        public async Task UnpublishedCommunitySurvey(int surveyId, int accountId)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    Account account = await _unitOfWork.AccountRepository.FindByIdAsync(accountId);
                    SurveyFilterObject surveyFilterObject = new SurveyFilterObject
                    {
                        RequesterId = accountId,
                        SurveyTypeId = 2,
                        IsDeletedContain = false,
                        SurveyStatusIds = new List<int> { 2 },
                        IsAvailable = true,
                        // IsInvalidTakenResultContain = false
                    };
                    var survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                    if (survey == null)
                    {
                        throw new Exception("survey theo điều kiện và có id " + surveyId.ToString() + " không tồn tại");
                    }
                    survey.IsAvailable = false;
                    survey.Kpi = null;
                    survey.TheoryPrice = null;
                    survey.ExtraPrice = null;
                    survey.ProfitPrice = null;
                    survey.AllocBaseAmount = null;
                    survey.AllocTimeAmount = null;
                    survey.AllocLevelAmount = null;
                    survey.TakerBaseRewardPrice = null;
                    survey.MaxXp = null;
                    survey.PublishedAt = null;
                    survey.StartDate = null;
                    survey.EndDate = null;

                    await _surveyGenericRepository.UpdateAsync(survey.Id, survey);


                    SurveyStatusTracking newSurveyStatusTracking = new SurveyStatusTracking
                    {
                        SurveyId = survey.Id,
                        SurveyStatusId = 1
                    };
                    await _surveyStatusTrackingGenericRepository.CreateAsync(newSurveyStatusTracking);
                    await transaction.CommitAsync();

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Hủy xuất bản community survey thất bại, lỗi: " + ex.Message);
                }
            }
        }


        public async Task<List<SurveyDefaultBackgroundThemeListItemDTO>> GetSurveyDefaultBackgroundThemes()
        {
            try
            {
                var themes = await _surveyDefaultBackgroundThemeGenericRepository.FindAll().ToListAsync();
                List<SurveyDefaultBackgroundThemeListItemDTO> surveyDefaultBackgroundThemeListItems = new List<SurveyDefaultBackgroundThemeListItemDTO>();
                foreach (var theme in themes)
                {
                    var configJson = JObject.Parse(theme.ConfigJsonString).ToObject<SurveyDefaultBackgroundThemeConfigJsonDTO>();
                    var mainImageUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(_filePathConfig.SURVEY_DEFAULT_BACKGROUND_IMAGE_PATH, theme.Id.ToString(), "main"));

                    surveyDefaultBackgroundThemeListItems.Add(new SurveyDefaultBackgroundThemeListItemDTO
                    {
                        Id = theme.Id,
                        ConfigJson = configJson,
                        MainImageUrl = mainImageUrl
                    });
                }

                return surveyDefaultBackgroundThemeListItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lấy danh sách chủ đề nền mặc định thất bại, lỗi: " + ex.Message);
            }
        }

        public async Task<SurveyTakerSegmentSummarizedFilterTagDTO> GetSurveyTakerSegmentSummarizedFilterTagAndSurveyTakingFrequencyRate(int surveyId, int userId, SurveyTakerSegmentDTO surveyTakerSegment)
        {
            try
            {
                var systemConfigProfile = await _unitOfWork.SystemConfigProfileRepository.FindActiveProfileAsync();
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

                var filterTags = await _filterTagGenericRepository.FindAll(null, a => a.FilterTagType).ToListAsync();

                var summarizedFilterTags = await _surveyOpenAIService.GetSurveyTakerSegmentSummarizedFilterTagAsync(filterTags, surveyTakerSegment, survey.SurveyTopic, survey.SurveySpecificTopic);
                foreach (var summarizedFilterTag in summarizedFilterTags)
                {
                    Console.WriteLine("summarizedFilterTag: " + summarizedFilterTag.FilterTagId + " : " + summarizedFilterTag.Summary);
                }

                var surveyTakerSegmentEmbeddingVectorFilterTags = await _surveyEmbeddingVectorService.GetVectorEncodedFilterTagsAsync(summarizedFilterTags); ;

                var matchedAccountsBySurveyTakerSegment = await _unitOfWork.AccountRepository.FindByAccountProfileAsync(new AccountProfile
                {
                    CountryRegion = surveyTakerSegment.CountryRegion,
                    MaritalStatus = surveyTakerSegment.MaritalStatus,
                    AverageIncome = surveyTakerSegment.AverageIncome,
                    JobField = surveyTakerSegment.JobField,
                    EducationLevel = surveyTakerSegment.EducationLevel,
                });
                if (matchedAccountsBySurveyTakerSegment == null || matchedAccountsBySurveyTakerSegment.Count() == 0)
                {
                    throw new Exception("Không tìm thấy tài khoản nào phù hợp với phân khúc người tham gia khảo sát");
                }

                List<EmbeddingVectorFilterTagDTO> targetEmbeddingVectorFilterTags = surveyTakerSegmentEmbeddingVectorFilterTags.Select(x => new EmbeddingVectorFilterTagDTO
                {
                    FilterTagId = x.FilterTagId,
                    EmbeddingVector = x.EmbeddingVector?.ToArray(),
                }).ToList();

                List<CandidateEmbeddingVectorFilterTagsDTO> candidateEmbeddingVectorFilterTags = new List<CandidateEmbeddingVectorFilterTagsDTO>();
                foreach (var account in matchedAccountsBySurveyTakerSegment)
                {
                    var accountFilterTags = await _postgresDbContext.TakerEmbeddingVectorTagFilters
                                                        .Where(x => x.TakerId == account.Id).ToListAsync();

                    candidateEmbeddingVectorFilterTags.Add(new CandidateEmbeddingVectorFilterTagsDTO
                    {
                        CandidateId = account.Id,
                        EmbeddingVectorFilterTags = accountFilterTags.Select(x => new EmbeddingVectorFilterTagDTO
                        {
                            FilterTagId = x.FilterTagId,
                            EmbeddingVector = x.EmbeddingVector?.ToArray(),
                        }).ToList()
                    });
                }

                // [CÁCH 1]
                // List<int> matchedAccountIds = (await _surveyEmbeddingVectorService.GetSimilarCandidateIdsByEmbeddingVectorFilterTagsWithTargetAccuracyAsync(
                //     new FilterTagSimilarityRequestDTO
                //     {
                //         TargetEmbeddingVectorFilterTags = targetEmbeddingVectorFilterTags,
                //         CandidateEmbeddingVectorFilterTags = candidateEmbeddingVectorFilterTags,
                //         MinScore = _embeddingVectorModelConfig.MinScore,
                //         MaxScore = _embeddingVectorModelConfig.MaxScore,
                //         TargetTagFilterAccuracyRate = surveyTakerSegment.TagFilterAccuracyRate
                //     })).Select(x => x.CandidateId).ToList();

                // [CÁCH 2]
                List<int> matchedAccountIds = _surveyEmbeddingVectorService.FilterCandidatesByTagSimilarityByTargetAccuracy(
                    new FilterTagSimilarityComparisonRequestDTO
                    {
                        TargetEmbeddingVectorFilterTags = targetEmbeddingVectorFilterTags,
                        CandidateEmbeddingVectorFilterTags = candidateEmbeddingVectorFilterTags,
                        MinScore = _embeddingVectorModelConfig.MinScore,
                        MaxScore = _embeddingVectorModelConfig.MaxScore,
                        TargetTagFilterAccuracyRate = surveyTakerSegment.TagFilterAccuracyRate
                    }).Select(x => x.CandidateId).ToList();

                // Console.WriteLine("\n\n\nmatchedAccountsBySurveyTakerSegment: " + matchedAccountsBySurveyTakerSegment.Count());
                // Console.WriteLine("matchedAccountIds: " + matchedAccountIds.Count());
                // foreach (var matchedAccount in matchedAccountIds)
                // {
                //     Console.WriteLine("matchedAccountId: " + matchedAccount);
                // }

                return new SurveyTakerSegmentSummarizedFilterTagDTO
                {
                    FilterTags = summarizedFilterTags.Select(x => new SummarizedAndEmbeddingVectorFilterTagDTO
                    {
                        Id = x.FilterTagId,
                        Name = filterTags.FirstOrDefault(ft => ft.Id == x.FilterTagId)?.Name,
                        TagColor = filterTags.FirstOrDefault(ft => ft.Id == x.FilterTagId)?.TagColor,
                        Summary = x.Summary,
                        FilterTagTypeId = filterTags.FirstOrDefault(ft => ft.Id == x.FilterTagId).FilterTagTypeId,
                        EmbeddingVector = surveyTakerSegmentEmbeddingVectorFilterTags.FirstOrDefault(ft => ft.FilterTagId == x.FilterTagId)?.EmbeddingVector?.ToArray(),
                    }).ToList(),
                    MaxKpi = (int)Math.Floor(matchedAccountIds.Count() * (1 - systemConfigProfile.AccountGeneralConfig.SafetyFilterRate)),
                    R = await CalculateSurveyTakingFrequencyRateByAccountIds(matchedAccountIds, survey.SurveyTopicId)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Lấy danh sách tag tóm tắt của người tham gia khảo sát thất bại, lỗi: " + ex.Message);
            }
        }

        public async Task DeleteSurvey(int surveyId, Account account)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    SurveyFilterObject surveyFilterObject = new SurveyFilterObject { };

                    Survey survey = await _unitOfWork.SurveyRepository.FindByIdAndFilterObjectAsync(surveyId, surveyFilterObject);
                    if (survey == null)
                    {
                        // return new SurveySessionUpdateTriggerResponseDTO("survey không tồn tại", false);
                        throw new ForbiddenException("survey không tồn tại");
                    }
                    var currentSurveyStatus = await _unitOfWork.SurveyRepository.GetLatestSurveyStatusTrackingBySurveyIdAsync(survey.Id);
                    if (currentSurveyStatus == null)
                    {
                        // return new SurveySessionUpdateTriggerResponseDTO("không tìm thấy trạng thái survey", false);
                        throw new ForbiddenException("không tìm thấy trạng thái survey");
                    }


                    // kiêm tra quyền
                    if (survey.SurveyTypeId == 1 && account.RoleId != 2 && account.RoleId != 3)
                    {
                        throw new ForbiddenException("không có quyền xoá survey này");
                    }
                    else if (survey.SurveyTypeId == 2 && account.RoleId != 4)
                    {
                        throw new ForbiddenException("không có quyền xoá survey này");
                    }
                    else if (survey.SurveyTypeId == 3 && account.RoleId != 2 && account.RoleId != 3)
                    {
                        throw new ForbiddenException("không có quyền xoá survey này");
                    }




                    if (survey.SurveyTypeId == 1)
                    {
                        // kiểm tra survey status
                        if (currentSurveyStatus?.SurveyStatusId == 2)
                        {
                            throw new ForbiddenException("không được phép xoá survey ở thời điểm hiện tại vì nó đã được đăng tải");
                        }

                        try
                        {
                            survey.DeletedAt = _dateHelper.GetNowByAppTimeZone();
                            SurveyStatusTracking surveyStatusTracking = new SurveyStatusTracking
                            {
                                SurveyId = survey.Id,
                                SurveyStatusId = 4, // Deleted status
                            };
                            await _surveyGenericRepository.UpdateAsync(survey.Id, survey);
                            await _surveyStatusTrackingGenericRepository.CreateAsync(surveyStatusTracking);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("\n" + ex.Message + "\n");
                            Console.WriteLine("\n" + ex.StackTrace + "\n");
                            throw new Exception(ex.Message);
                        }

                    }
                    else if (survey.SurveyTypeId == 2)
                    {
                        // kiểm tra requester id
                        if (survey.RequesterId != account.Id || survey.RequesterId != account.Id || survey.RequesterId != survey.RequesterId)
                        {
                            throw new ForbiddenException("chỉ có người tạo survey này mới có quyền xoá");
                        }


                        try
                        {
                            survey.DeletedAt = _dateHelper.GetNowByAppTimeZone();
                            SurveyStatusTracking surveyStatusTracking = new SurveyStatusTracking
                            {
                                SurveyId = survey.Id,
                                SurveyStatusId = 4, // Deleted status
                            };
                            await _surveyGenericRepository.UpdateAsync(survey.Id, survey);
                            await _surveyStatusTrackingGenericRepository.CreateAsync(surveyStatusTracking);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("\n" + ex.Message + "\n");
                            Console.WriteLine("\n" + ex.StackTrace + "\n");
                            throw new Exception(ex.Message);
                        }

                    }
                    else
                    {
                        throw new Exception("không xác định");
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("Xoá survey thất bại, lỗi: " + ex.Message);
                }
            }
        }

        public async Task<List<SurveyTopicDTO>> GetSurveyTopics()
        {
            return _surveyTopicGenericRepository.FindAll().Select(x => new SurveyTopicDTO
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
        }


        public async Task<List<SurveySpecificTopicDTO>> GetSurveySpecificTopics()
        {
            return _surveySpecificTopicGenericRepository.FindAll().Select(x => new SurveySpecificTopicDTO
            {
                Id = x.Id,
                Name = x.Name,
                SurveyTopicId = x.SurveyTopicId
            }).ToList();
        }

        public async Task<List<SurveySecurityModeDTO>> GetSurveySecurityModes()
        {
            return _surveySecurityModeGenericRepository.FindAll().Select(x => new SurveySecurityModeDTO
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            }).ToList();
        }

        public async Task<List<SurveyQuestionTypeDTO>> GetSurveyQuestionTypes()
        {
            return _surveyQuestionTypeGenericRepository.FindAll().Select(x => new SurveyQuestionTypeDTO
            {
                Id = x.Id,
                Name = x.Name,
                Price = x.Price,
                DeactivatedAt = x.DeactivatedAt
            }).ToList();
        }

        public async Task<List<SurveyFieldInputTypeDTO>> GetSurveyFieldInputTypes()
        {
            return _surveyFieldInputTypeGenericRepository.FindAll().Select(x => new SurveyFieldInputTypeDTO
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
        }






        /////////////////////////////////////////////////////////////

        

    }
}
