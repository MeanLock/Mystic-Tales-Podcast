using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;
using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.UOW;
using PodcastService.DataAccess.Repositories.interfaces;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.Infrastructure.Services.Google.Email;
using PodcastService.Infrastructure.Configurations.Google.interfaces;
using PodcastService.BusinessLogic.Helpers.AuthHelpers;
using PodcastService.BusinessLogic.Helpers.FileHelpers;
using PodcastService.BusinessLogic.Helpers.DateHelpers;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using PodcastService.BusinessLogic.Models.CrossService;
using Newtonsoft.Json.Linq;
using PodcastService.Infrastructure.Services.Kafka;
using PodcastService.BusinessLogic.Enums.Kafka;
using PodcastService.Infrastructure.Models.Kafka;
using PodcastService.BusinessLogic.Services.MessagingServices.interfaces;
using PodcastService.Infrastructure.Services.Redis;
using PodcastService.DataAccess.Entities.SqlServer;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateChannel;
using PodcastService.BusinessLogic.Enums.Podcast;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.Cache;
using PodcastService.BusinessLogic.DTOs.Hashtag;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.Services.DbServices.MiscServices;
using PodcastService.BusinessLogic.DTOs.Show;
using PodcastService.BusinessLogic.DTOs.Episode;
using PodcastService.BusinessLogic.DTOs.Episode.Details;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateEpisode;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubtractEpisodeTotalSaved;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PlusEpisodeTotalSaved;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateEpisode;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UploadEpisodeLicenses;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteEpisodeLicenses;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubmitEpisodeAudioFile;
using PodcastService.Infrastructure.Services.Audio.Transcription;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.ProcessingEpisodeDraftAudio;
using PodcastService.BusinessLogic.Services.AudioServices;
using PodcastService.Infrastructure.Services.Audio.AcoustID;
using PodcastService.Infrastructure.Models.Audio.AcoustID;
using Newtonsoft.Json;
using PodcastService.BusinessLogic.Enums.Account;

namespace PodcastService.BusinessLogic.Services.DbServices.PodcastServices
{
    public class PodcastEpisodeService
    {
        // LOGGER
        private readonly ILogger<PodcastEpisodeService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly IPodcastPublishReviewSessionConfig _podcastPublishReviewSessionConfig;

        // DB CONTEXT
        private readonly AppDbContext _appDbContext;

        // HELPERS
        private readonly BcryptHelper _bcryptHelper;
        private readonly JwtHelper _jwtHelper;
        private readonly FileIOHelper _fileIOHelper;
        private readonly DateHelper _dateHelper;

        // UNIT OF WORK
        private readonly IUnitOfWork _unitOfWork;

        // REPOSITORIES
        private readonly IGenericRepository<PodcastChannel> _podcastChannelGenericRepository;
        private readonly IGenericRepository<PodcastChannelStatusTracking> _podcastChannelStatusTrackingGenericRepository;
        private readonly IGenericRepository<PodcastChannelHashtag> _podcastChannelHashtagGenericRepository;
        private readonly IGenericRepository<Hashtag> _hashtagGenericRepository;
        private readonly IGenericRepository<PodcastShow> _podcastShowGenericRepository;
        private readonly IGenericRepository<PodcastShowStatusTracking> _podcastShowStatusTrackingGenericRepository;
        private readonly IGenericRepository<PodcastShowHashtag> _podcastShowHashtagGenericRepository;
        private readonly IGenericRepository<PodcastEpisode> _podcastEpisodeGenericRepository;
        private readonly IGenericRepository<PodcastShowReview> _podcastShowReviewGenericRepository;
        private readonly IGenericRepository<PodcastEpisodeStatusTracking> _podcastEpisodeStatusTrackingGenericRepository;
        private readonly IGenericRepository<PodcastEpisodeHashtag> _podcastEpisodeHashtagGenericRepository;
        private readonly IGenericRepository<PodcastEpisodeLicense> _podcastEpisodeLicenseGenericRepository;
        private readonly IGenericRepository<PodcastEpisodeLicenseType> _podcastEpisodeLicenseTypeGenericRepository;
        private readonly IGenericRepository<PodcastEpisodePublishReviewSession> _podcastEpisodePublishReviewSessionGenericRepository;
        private readonly IGenericRepository<PodcastEpisodePublishDuplicateDetection> _podcastEpisodePublishDuplicateDetectionGenericRepository;

        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        // CACHING SERVICE
        private readonly AccountCachingService _accountCachingService;

        // GOOGLE SERVICE
        private readonly FluentEmailService _fluentEmailService;

        // KAFKA SERVICE
        private readonly IMessagingService _messagingService;
        private readonly KafkaProducerService _kafkaProducerService;

        // REDIS SERVICE
        private readonly RedisSharedCacheService _redisSharedCacheService;

        // AUDIO SERVICE
        private readonly AudioTranscriptionService _audioTranscriptionService;
        private readonly AcoustIDAudioFingerprintGenerator _acoustIDAudioFingerprintGenerator;
        private readonly AcoustIDAudioFingerprintComparator _acoustIDAudioFingerprintComparator;

        public PodcastEpisodeService(
            ILogger<PodcastEpisodeService> logger,
            AppDbContext appDbContext,
            BcryptHelper bcryptHelper,
            FluentEmailService fluentEmailService,
            JwtHelper jwtHelper,
            IUnitOfWork unitOfWork,

            IServiceProvider serviceProvider,
            IGenericRepository<PodcastChannel> podcastChannelGenericRepository,
            IGenericRepository<PodcastChannelStatusTracking> podcastChannelStatusTrackingGenericRepository,
            IGenericRepository<PodcastChannelHashtag> podcastChannelHashtagGenericRepository,
            IGenericRepository<Hashtag> hashtagGenericRepository,
            IGenericRepository<PodcastShow> podcastShowGenericRepository,
            IGenericRepository<PodcastShowStatusTracking> podcastShowStatusTrackingGenericRepository,
            IGenericRepository<PodcastShowHashtag> podcastShowHashtagGenericRepository,
            IGenericRepository<PodcastEpisode> podcastEpisodeGenericRepository,
            IGenericRepository<PodcastShowReview> podcastShowReviewGenericRepository,
            IGenericRepository<PodcastEpisodeStatusTracking> podcastEpisodeStatusTrackingGenericRepository,
            IGenericRepository<PodcastEpisodeHashtag> podcastEpisodeHashtagGenericRepository,
            IGenericRepository<PodcastEpisodeLicense> podcastEpisodeLicenseGenericRepository,
            IGenericRepository<PodcastEpisodeLicenseType> podcastEpisodeLicenseTypeGenericRepository,
            IGenericRepository<PodcastEpisodePublishReviewSession> podcastEpisodePublishReviewSessionGenericRepository,

            FileIOHelper fileIOHelper,
            DateHelper dateHelper,

            IFilePathConfig filePathConfig,
            IAppConfig appConfig,
            IPodcastPublishReviewSessionConfig podcastPublishReviewSessionConfig,

            HttpServiceQueryClient httpServiceQueryClient,

            AccountCachingService accountCachingService,

            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,

            RedisSharedCacheService redisSharedCacheService,

            AudioTranscriptionService audioTranscriptionService,
            AcoustIDAudioFingerprintGenerator audioFingerprintService,
            AcoustIDAudioFingerprintComparator audioFingerprintComparator
            )
        {
            _logger = logger;

            _appDbContext = appDbContext;
            _unitOfWork = unitOfWork;

            _podcastChannelGenericRepository = podcastChannelGenericRepository;
            _podcastChannelStatusTrackingGenericRepository = podcastChannelStatusTrackingGenericRepository;
            _podcastChannelHashtagGenericRepository = podcastChannelHashtagGenericRepository;
            _hashtagGenericRepository = hashtagGenericRepository;
            _podcastShowGenericRepository = podcastShowGenericRepository;
            _podcastShowStatusTrackingGenericRepository = podcastShowStatusTrackingGenericRepository;
            _podcastShowHashtagGenericRepository = podcastShowHashtagGenericRepository;
            _podcastEpisodeGenericRepository = podcastEpisodeGenericRepository;
            _podcastShowReviewGenericRepository = podcastShowReviewGenericRepository;
            _podcastEpisodeStatusTrackingGenericRepository = podcastEpisodeStatusTrackingGenericRepository;
            _podcastEpisodeHashtagGenericRepository = podcastEpisodeHashtagGenericRepository;
            _podcastEpisodeLicenseGenericRepository = podcastEpisodeLicenseGenericRepository;
            _podcastEpisodeLicenseTypeGenericRepository = podcastEpisodeLicenseTypeGenericRepository;

            _fileIOHelper = fileIOHelper;
            _jwtHelper = jwtHelper;
            _bcryptHelper = bcryptHelper;
            _dateHelper = dateHelper;

            _fluentEmailService = fluentEmailService;

            _filePathConfig = filePathConfig;
            _podcastPublishReviewSessionConfig = podcastPublishReviewSessionConfig;
            _appConfig = appConfig;

            _httpServiceQueryClient = httpServiceQueryClient;

            _accountCachingService = accountCachingService;

            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;

            _redisSharedCacheService = redisSharedCacheService;

            _audioTranscriptionService = audioTranscriptionService;
            _acoustIDAudioFingerprintGenerator = audioFingerprintService;
            _acoustIDAudioFingerprintComparator = audioFingerprintComparator;
        }



        public static string GenerateRandomVerifyCode(int length)
        {
            var random = new Random();
            var digits = new char[length];

            for (int i = 0; i < length; i++)
            {
                digits[i] = (char)('0' + random.Next(0, 10));
            }

            return new string(digits);
        }

        public async Task<JObject> GetActiveSystemConfigProfile()
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "activeSystemConfigProfile",
                            QueryType = "findall",
                            EntityType = "SystemConfigProfile",
                                Parameters = JObject.FromObject(new
                                {
                                    where = new
                                    {
                                        IsActive = true
                                    },
                                    include = "AccountConfig,AccountViolationLevelConfigs, BookingConfig, PodcastSubscriptionConfigs, PodcastSuggestionConfig, ReviewSessionConfig",

                                }),
                            Fields = new[] { "Id", "Name", "IsActive", "AccountConfig", "AccountViolationLevelConfigs", "BookingConfig", "PodcastSubscriptionConfigs", "PodcastSuggestionConfig", "ReviewSessionConfig" }
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("SystemConfigurationService", batchRequest);

            return ((JArray)result.Results["activeSystemConfigProfile"]).First as JObject;
        }

        public async Task<List<string>> GetAllPodcastRestrictedTerms()
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastRestrictedTerms",
                            QueryType = "findall",
                            EntityType = "PodcastRestrictedTerm",
                                Parameters = JObject.FromObject(new
                                {

                                }),
                            Fields = new[] { "Id", "Term" }
                        }
                    }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("SystemConfigurationService", batchRequest);

            // return Ok((JArray)result.Results["podcastRestrictedTerms"]);
            List<string> terms = ((JArray)result.Results["podcastRestrictedTerms"]).Select(terms => terms["Term"].ToString()).ToList();
            return terms;
        }
        public async Task<Guid> SendChangeAccountStatusMessage(int id)
        {
            var requestData = JObject.FromObject(new
            {
                Id = id
            });
            var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "account-status-change-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            return startSagaTriggerMessage.SagaInstanceId;
        }

        public int CalculateViolationLevel(int violationPoint, JArray accountViolationLevelConfigs)
        {
            int violationLevel = 0;
            accountViolationLevelConfigs = new JArray(accountViolationLevelConfigs.OrderBy(c => c.Value<int>("ViolationPointThreshold")));
            int maxLevelPointThreshold = accountViolationLevelConfigs.Max(c => c.Value<int>("ViolationPointThreshold"));
            if (violationPoint > maxLevelPointThreshold)
            {
                violationLevel = accountViolationLevelConfigs.Max(c => c.Value<int>("ViolationLevel"));
                return violationLevel;
            }
            foreach (var config in accountViolationLevelConfigs)
            {
                int level = config.Value<int>("ViolationLevel");
                int pointThreshold = config.Value<int>("ViolationPointThreshold");
                // Console.WriteLine($"Checking level {level} with threshold {pointThreshold} against violation point {violationPoint}");
                if (violationPoint <= pointThreshold)
                {
                    violationLevel = level;
                    break;
                }
            }

            return violationLevel;
        }

        public async Task<AccountStatusCache> QueryAccountStatusCacheById(int accountId)
        {
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
                {
                    new BatchQueryItem
                    {
                        Key = "account",
                        QueryType = "findbyid",
                        EntityType = "Account",
                        Parameters = JObject.FromObject(new
                        {
                            id = accountId,
                            include = "PodcasterProfile"
                        }),
                        Fields = new[] {
                            "Id",
                            "Email",
                            "Password",
                            "RoleId",
                            "FullName",
                            "Dob",
                            "Gender",
                            "Address",
                            "Phone",
                            "Balance",
                            "MainImageFileKey",
                            "IsVerified",
                            "GoogleId",
                            "VerifyCode",
                            "PodcastListenSlot",
                            "ViolationPoint",
                            "ViolationLevel",
                            "LastViolationPointChanged",
                            "LastViolationLevelChanged",
                            "LastPodcastListenSlotChanged",
                            "DeactivatedAt",
                            "CreatedAt",
                            "UpdatedAt",
                            "PodcasterProfile",
                        }
                    }
                }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
            if (result.Results["account"] == null) return null;


            var accountStatusCache = (result.Results["account"] as JObject).ToObject<AccountStatusCache>();
            var podcasterProfile = result.Results["account"]["PodcasterProfile"] as JObject;
            accountStatusCache.HasVerifiedPodcasterProfile = accountStatusCache.RoleId == 1 && podcasterProfile != null && podcasterProfile["IsVerified"]?.ToObject<bool>() == true ? true : false;
            Console.WriteLine($"Queried Account: Id={accountStatusCache.Id}, RoleId={accountStatusCache.RoleId}, IsVerified={accountStatusCache.IsVerified}, DeactivatedAt={accountStatusCache.DeactivatedAt}, HasVerifiedPodcasterProfile={accountStatusCache.HasVerifiedPodcasterProfile}");
            return accountStatusCache;
        }

        public List<string> ScanTranscriptionForRestrictedTerms(string transcriptionText, List<string> restrictedTerms)
        {
            var detectedTerms = new List<string>();
            if (string.IsNullOrWhiteSpace(transcriptionText) || restrictedTerms == null || !restrictedTerms.Any())
            {
                return new List<string>();
            }

            // Normalize transcription to lowercase for case-insensitive matching
            var normalizedTranscription = transcriptionText.ToLowerInvariant();

            foreach (var term in restrictedTerms)
            {
                if (string.IsNullOrWhiteSpace(term)) continue;

                var normalizedTerm = term.ToLowerInvariant();

                if (normalizedTranscription.Contains(normalizedTerm))
                {
                    detectedTerms.Add(term);
                }
            }

            return detectedTerms;
        }

        public async Task<List<AccountDTO>> GetAllAvailableStaffs()
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                {
                    new BatchQueryItem
                    {
                        Key = "account",
                        QueryType = "findbyid",
                        EntityType = "Account",

                        Parameters = JObject.FromObject(new
                        {
                            where = new
                            {
                                RoleId = (int) RoleEnum.Staff
                            },
                        }),
                    }
                }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
                var accounts = ((JArray)result.Results["account"]).ToObject<List<AccountDTO>>();
                return accounts;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get all available staffs failed, error: " + ex.Message);
            }
        }


        /////////////////////////////////////////////////////////////

                #region Sample coding format must be followed
                #endregion

        public async Task<EpisodeDetailResponseDTO> GetEpisodeByIdAsync(Guid episodeId, int? role)
        {
            try
            {
                var episodeQuery = _podcastEpisodeGenericRepository.FindAll(
                    predicate: pe => pe.DeletedAt == null && pe.Id == episodeId,
                    includeFunc: q => q
                        .Include(pe => pe.PodcastShow)
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                        .ThenInclude(pet => pet.PodcastEpisodeStatus)
                        .Include(pe => pe.PodcastEpisodeHashtags)
                        .ThenInclude(peh => peh.Hashtag)
                        .Include(pe => pe.PodcastEpisodeSubscriptionType)
                );

                if (role == null || role == 1)
                {
                    episodeQuery = episodeQuery.Where(pe => pe.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published && pe.IsReleased != null);
                }

                var episode = await episodeQuery.FirstOrDefaultAsync();

                if (episode == null)
                {
                    throw new Exception("Podcast episode with id " + episodeId + " does not exist");
                }

                var podcaster = await _accountCachingService.GetAccountStatusCacheById(episode.PodcastShow.PodcasterId);
                if (podcaster == null || podcaster.Id != episode.PodcastShow.PodcasterId || podcaster.IsVerified == false || podcaster.HasVerifiedPodcasterProfile == false)
                {
                    throw new Exception("Podcaster with id " + episode.PodcastShow.PodcasterId + " does not exist");
                }

                var episodeDetail = new EpisodeDetailResponseDTO
                {
                    Id = episode.Id,
                    Name = episode.Name,
                    Description = episode.Description,
                    AudioFileKey = episode.AudioFileKey,
                    AudioLength = episode.AudioLength,
                    ReleaseDate = episode.ReleaseDate,
                    IsReleased = episode.IsReleased,
                    AudioFileSize = episode.AudioFileSize,
                    EpisodeOrder = episode.EpisodeOrder,
                    ExplicitContent = episode.ExplicitContent,
                    IsAudioPublishable = episode.IsAudioPublishable,
                    ListenCount = episode.ListenCount,
                    MainImageFileKey = episode.MainImageFileKey,
                    SeasonNumber = episode.SeasonNumber,
                    TakenDownReason = role == null || role == 1 ? null : episode.TakenDownReason,
                    TotalSave = episode.TotalSave,
                    Hashtags = episode.PodcastEpisodeHashtags.Select(peh => new HashtagDTO
                    {
                        Id = peh.Hashtag.Id,
                        Name = peh.Hashtag.Name
                    }).ToList(),
                    PodcastShow = new PodcastShowSnippetResponseDTO
                    {
                        Id = episode.PodcastShow.Id,
                        Name = episode.PodcastShow.Name,
                        MainImageFileKey = episode.PodcastShow.MainImageFileKey
                    },
                    Podcaster = new AccountSnippetResponseDTO
                    {
                        Id = podcaster.Id,
                        Email = podcaster.Email,
                        FullName = podcaster.FullName,
                        MainImageFileKey = podcaster.MainImageFileKey
                    },
                    PodcastEpisodeSubscriptionType = episode.PodcastEpisodeSubscriptionType != null ? new PodcastEpisodeSubscriptionTypeDTO
                    {
                        Id = episode.PodcastEpisodeSubscriptionType.Id,
                        Name = episode.PodcastEpisodeSubscriptionType.Name
                    } : null,
                    CreatedAt = episode.CreatedAt,
                    UpdatedAt = episode.UpdatedAt,
                    CurrentStatus = episode.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).Select(pet => new PodcastEpisodeStatusDTO
                    {
                        Id = pet.PodcastEpisodeStatus.Id,
                        Name = pet.PodcastEpisodeStatus.Name
                    }).FirstOrDefault()!,
                };
                return episodeDetail;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get episode by id failed, error: " + ex.Message);
            }

        }

        public async Task<EpisodeDetailResponseDTO> GetEpisodeByIdForPodcasterAsync(Guid episodeId, int podcasterId)
        {
            try
            {
                var episodeQuery = _podcastEpisodeGenericRepository.FindAll(
                    predicate: pe => pe.DeletedAt == null && pe.Id == episodeId,
                    includeFunc: q => q
                        .Include(pe => pe.PodcastShow)
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                        .ThenInclude(pet => pet.PodcastEpisodeStatus)
                        .Include(pe => pe.PodcastEpisodeHashtags)
                        .ThenInclude(peh => peh.Hashtag)
                        .Include(pe => pe.PodcastEpisodeSubscriptionType)
                );

                var episode = await episodeQuery.FirstOrDefaultAsync();

                if (episode == null)
                {
                    throw new Exception("Podcast episode with id " + episodeId + " does not exist");
                }
                else if (episode.PodcastShow.DeletedAt != null)
                {
                    throw new Exception("Podcast show with id " + episode.PodcastShow.Id + " has been deleted");
                }
                else if (episode.PodcastShow.PodcasterId != podcasterId)
                {
                    throw new Exception("You do not have permission to access this podcast episode");
                }

                var podcaster = await _accountCachingService.GetAccountStatusCacheById(episode.PodcastShow.PodcasterId);
                if (podcaster == null || podcaster.Id != episode.PodcastShow.PodcasterId || podcaster.IsVerified == false || podcaster.HasVerifiedPodcasterProfile == false)
                {
                    throw new Exception("Podcaster with id " + episode.PodcastShow.PodcasterId + " does not exist");
                }

                var episodeDetail = new EpisodeDetailResponseDTO
                {
                    Id = episode.Id,
                    Name = episode.Name,
                    Description = episode.Description,
                    AudioFileKey = episode.AudioFileKey,
                    AudioLength = episode.AudioLength,
                    ReleaseDate = episode.ReleaseDate,
                    IsReleased = episode.IsReleased,
                    AudioFileSize = episode.AudioFileSize,
                    EpisodeOrder = episode.EpisodeOrder,
                    ExplicitContent = episode.ExplicitContent,
                    IsAudioPublishable = episode.IsAudioPublishable,
                    ListenCount = episode.ListenCount,
                    MainImageFileKey = episode.MainImageFileKey,
                    SeasonNumber = episode.SeasonNumber,
                    TakenDownReason = episode.TakenDownReason,
                    TotalSave = episode.TotalSave,
                    Hashtags = episode.PodcastEpisodeHashtags.Select(peh => new HashtagDTO
                    {
                        Id = peh.Hashtag.Id,
                        Name = peh.Hashtag.Name
                    }).ToList(),
                    PodcastShow = new PodcastShowSnippetResponseDTO
                    {
                        Id = episode.PodcastShow.Id,
                        Name = episode.PodcastShow.Name,
                        MainImageFileKey = episode.PodcastShow.MainImageFileKey
                    },
                    Podcaster = new AccountSnippetResponseDTO
                    {
                        Id = podcaster.Id,
                        Email = podcaster.Email,
                        FullName = podcaster.FullName,
                        MainImageFileKey = podcaster.MainImageFileKey
                    },
                    PodcastEpisodeSubscriptionType = episode.PodcastEpisodeSubscriptionType != null ? new PodcastEpisodeSubscriptionTypeDTO
                    {
                        Id = episode.PodcastEpisodeSubscriptionType.Id,
                        Name = episode.PodcastEpisodeSubscriptionType.Name
                    } : null,
                    CreatedAt = episode.CreatedAt,
                    UpdatedAt = episode.UpdatedAt,
                    CurrentStatus = episode.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).Select(pet => new PodcastEpisodeStatusDTO
                    {
                        Id = pet.PodcastEpisodeStatus.Id,
                        Name = pet.PodcastEpisodeStatus.Name
                    }).FirstOrDefault()!,
                };
                return episodeDetail;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get episode by id for podcaster failed, error: " + ex.Message);
            }

        }

        public async Task PlusPodcastEpisodeTotalSaved(PlusEpisodeTotalSavedParameterDTO plusEpisodeTotalSavedParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var podcastEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(plusEpisodeTotalSavedParameterDTO.PodcastEpisodeId);
                    if (podcastEpisode == null)
                    {
                        throw new Exception("Podcast episode with id " + plusEpisodeTotalSavedParameterDTO.PodcastEpisodeId + " does not exist");
                    }
                    else if (podcastEpisode.DeletedAt != null)
                    {
                        throw new Exception("Podcast episode with id " + plusEpisodeTotalSavedParameterDTO.PodcastEpisodeId + " has been deleted");
                    }

                    podcastEpisode.TotalSave += 1;
                    await _podcastEpisodeGenericRepository.UpdateAsync(podcastEpisode.Id, podcastEpisode);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = podcastEpisode.Id;
                    messageNextRequestData["AccountId"] = command.RequestData["AccountId"];
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = podcastEpisode.Id,
                        AccountId = command.RequestData["AccountId"],
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "plus-episode-total-saved.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Plus podcast episode total saved failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "plus-episode-total-saved.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task SubtractPodcastEpisodeTotalSaved(SubtractEpisodeTotalSavedParameterDTO subtractEpisodeTotalSavedParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var podcastEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(subtractEpisodeTotalSavedParameterDTO.PodcastEpisodeId);
                    if (podcastEpisode == null)
                    {
                        throw new Exception("Podcast episode with id " + subtractEpisodeTotalSavedParameterDTO.PodcastEpisodeId + " does not exist");
                    }
                    else if (podcastEpisode.DeletedAt != null)
                    {
                        throw new Exception("Podcast episode with id " + subtractEpisodeTotalSavedParameterDTO.PodcastEpisodeId + " has been deleted");
                    }

                    podcastEpisode.TotalSave = Math.Max(0, podcastEpisode.TotalSave - subtractEpisodeTotalSavedParameterDTO.AffectedAccountIds.Count);
                    await _podcastEpisodeGenericRepository.UpdateAsync(podcastEpisode.Id, podcastEpisode);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = podcastEpisode.Id;
                    messageNextRequestData["AccountId"] = command.RequestData["AccountId"];
                    messageNextRequestData["AffectedAccountIds"] = JArray.FromObject(subtractEpisodeTotalSavedParameterDTO.AffectedAccountIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = podcastEpisode.Id,
                        AccountId = command.RequestData["AccountId"],
                        AffectedAccountIds = subtractEpisodeTotalSavedParameterDTO.AffectedAccountIds,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "subtract-episode-total-saved.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Subtract podcast episode total saved failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "subtract-episode-total-saved.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }


        public async Task CreatePodcastEpisode(CreateEpisodeParameterDTO createEpisodeParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingPodcastShow = await (_podcastShowGenericRepository.FindAll(
                        predicate: ps => ps.Id == createEpisodeParameterDTO.PodcastShowId && ps.DeletedAt == null,
                        includeFunc: ps => ps.Include(ps => ps.PodcastShowStatusTrackings)
                    )).FirstOrDefaultAsync();
                    if (existingPodcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + createEpisodeParameterDTO.PodcastShowId + " does not exist");
                    }
                    else if (existingPodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + createEpisodeParameterDTO.PodcastShowId + " has been deleted");
                    }
                    else if (existingPodcastShow.PodcasterId != createEpisodeParameterDTO.PodcasterId)
                    {
                        throw new Exception("Podcast show with id " + createEpisodeParameterDTO.PodcastShowId + " does not belong to podcaster with id " + createEpisodeParameterDTO.PodcasterId);
                    }
                    else if (existingPodcastShow.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnum.Removed)
                    {
                        throw new Exception("Podcast show with id " + createEpisodeParameterDTO.PodcastShowId + " has been removed");
                    }


                    var podcastEpisode = new PodcastEpisode
                    {
                        Name = createEpisodeParameterDTO.Name,
                        Description = createEpisodeParameterDTO.Description,
                        ExplicitContent = createEpisodeParameterDTO.ExplicitContent,
                        PodcastShowId = createEpisodeParameterDTO.PodcastShowId,
                        PodcastEpisodeSubscriptionTypeId = createEpisodeParameterDTO.PodcastEpisodeSubscriptionTypeId,
                        SeasonNumber = createEpisodeParameterDTO.SeasonNumber,
                        EpisodeOrder = createEpisodeParameterDTO.EpisodeOrder


                    };

                    var existingPodcaster = await _accountCachingService.GetAccountStatusCacheById(createEpisodeParameterDTO.PodcasterId);
                    if (existingPodcaster == null || existingPodcaster.Id != existingPodcastShow.PodcasterId || existingPodcaster.IsVerified == false || existingPodcaster.DeactivatedAt != null || existingPodcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + existingPodcastShow.PodcasterId + " does not exist");
                    }

                    await _podcastEpisodeGenericRepository.CreateAsync(podcastEpisode);

                    var newPodcastEpisodeStatusTracking = new PodcastEpisodeStatusTracking
                    {
                        PodcastEpisodeId = podcastEpisode.Id,
                        PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Draft
                    };
                    await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newPodcastEpisodeStatusTracking);


                    var folderPath = _filePathConfig.PODCAST_EPISODE_FILE_PATH + "\\" + podcastEpisode.Id;
                    if (createEpisodeParameterDTO.MainImageFileKey != null && createEpisodeParameterDTO.MainImageFileKey != "")
                    {
                        var MainImageFileKey = FilePathHelper.CombinePaths(folderPath, $"main_image{FilePathHelper.GetExtension(createEpisodeParameterDTO.MainImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(createEpisodeParameterDTO.MainImageFileKey, MainImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(createEpisodeParameterDTO.MainImageFileKey);
                        podcastEpisode.MainImageFileKey = MainImageFileKey;
                    }


                    await _podcastEpisodeGenericRepository.UpdateAsync(podcastEpisode.Id, podcastEpisode);

                    foreach (var hashtagId in createEpisodeParameterDTO.HashtagIds)
                    {
                        var existingHashtag = await _hashtagGenericRepository.FindByIdAsync(hashtagId);
                        if (existingHashtag == null)
                        {
                            throw new Exception("Hashtag with id " + hashtagId + " does not exist");
                        }
                        var podcastEpisodeHashtag = new PodcastEpisodeHashtag
                        {
                            PodcastEpisodeId = podcastEpisode.Id,
                            HashtagId = hashtagId
                        };
                        await _podcastEpisodeHashtagGenericRepository.CreateAsync(podcastEpisodeHashtag);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["Name"] = podcastEpisode.Name;
                    messageNextRequestData["Description"] = podcastEpisode.Description;
                    messageNextRequestData["ExplicitContent"] = podcastEpisode.ExplicitContent;
                    messageNextRequestData["MainImageFileKey"] = podcastEpisode.MainImageFileKey;
                    messageNextRequestData["PodcastEpisodeSubscriptionTypeId"] = podcastEpisode.PodcastEpisodeSubscriptionTypeId;
                    messageNextRequestData["PodcastShowId"] = podcastEpisode.PodcastShowId;
                    messageNextRequestData["SeasonNumber"] = podcastEpisode.SeasonNumber;
                    messageNextRequestData["EpisodeOrder"] = podcastEpisode.EpisodeOrder;
                    messageNextRequestData["HashtagIds"] = JArray.FromObject(createEpisodeParameterDTO.HashtagIds);
                    messageNextRequestData["PodcasterId"] = createEpisodeParameterDTO.PodcasterId;


                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = podcastEpisode.Id,
                        Name = podcastEpisode.Name,
                        Description = podcastEpisode.Description,
                        ExplicitContent = podcastEpisode.ExplicitContent,
                        MainImageFileKey = podcastEpisode.MainImageFileKey,
                        PodcastEpisodeSubscriptionTypeId = podcastEpisode.PodcastEpisodeSubscriptionTypeId,
                        PodcastShowId = podcastEpisode.PodcastShowId,
                        SeasonNumber = podcastEpisode.SeasonNumber,
                        EpisodeOrder = podcastEpisode.EpisodeOrder,
                        HashtagIds = createEpisodeParameterDTO.HashtagIds,
                        PodcasterId = createEpisodeParameterDTO.PodcasterId
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-episode.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Create podcast episode failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-episode.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }


        public async Task UpdatePodcastEpisode(UpdateEpisodeParameterDTO updateEpisodeParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingPodcastEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(updateEpisodeParameterDTO.PodcastEpisodeId,
                        includeFunc: q => q.Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                    );
                    if (existingPodcastEpisode == null)
                    {
                        throw new Exception("Podcast episode with id " + updateEpisodeParameterDTO.PodcastEpisodeId + " does not exist");
                    }
                    else if (existingPodcastEpisode.DeletedAt != null)
                    {
                        throw new Exception("Podcast episode with id " + updateEpisodeParameterDTO.PodcastEpisodeId + " has been deleted");
                    }
                    else if (existingPodcastEpisode.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Removed)
                    {
                        throw new Exception("Podcast episode with id " + updateEpisodeParameterDTO.PodcastEpisodeId + " has been removed");
                    }

                    if (existingPodcastEpisode.PodcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " does not exist");
                    }
                    else if (existingPodcastEpisode.PodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been deleted");
                    }
                    else if (existingPodcastEpisode.PodcastShow.PodcasterId != updateEpisodeParameterDTO.PodcasterId)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " does not belong to podcaster with id " + updateEpisodeParameterDTO.PodcasterId);
                    }
                    else if (existingPodcastEpisode.PodcastShow.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnum.Removed)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been removed");
                    }


                    var existingPodcaster = await _accountCachingService.GetAccountStatusCacheById(existingPodcastEpisode.PodcastShow.PodcasterId);
                    if (existingPodcaster == null || existingPodcaster.Id != existingPodcastEpisode.PodcastShow.PodcasterId || existingPodcaster.IsVerified == false || existingPodcaster.DeactivatedAt != null || existingPodcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + existingPodcastEpisode.PodcastShow.PodcasterId + " does not exist");
                    }

                    existingPodcastEpisode.Name = updateEpisodeParameterDTO.Name;
                    existingPodcastEpisode.Description = updateEpisodeParameterDTO.Description;
                    existingPodcastEpisode.ExplicitContent = updateEpisodeParameterDTO.ExplicitContent;
                    existingPodcastEpisode.PodcastEpisodeSubscriptionTypeId = updateEpisodeParameterDTO.PodcastEpisodeSubscriptionTypeId;
                    existingPodcastEpisode.SeasonNumber = updateEpisodeParameterDTO.SeasonNumber;
                    existingPodcastEpisode.EpisodeOrder = updateEpisodeParameterDTO.EpisodeOrder;

                    if (updateEpisodeParameterDTO.MainImageFileKey != null && updateEpisodeParameterDTO.MainImageFileKey != "")
                    {
                        if (existingPodcastEpisode.MainImageFileKey != null && existingPodcastEpisode.MainImageFileKey != "")
                        {
                            await _fileIOHelper.DeleteFileAsync(existingPodcastEpisode.MainImageFileKey);
                        }
                        var folderPath = _filePathConfig.PODCAST_EPISODE_FILE_PATH + "\\" + existingPodcastEpisode.Id;
                        var MainImageFileKey = FilePathHelper.CombinePaths(folderPath, $"main_image{FilePathHelper.GetExtension(updateEpisodeParameterDTO.MainImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(updateEpisodeParameterDTO.MainImageFileKey, MainImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(updateEpisodeParameterDTO.MainImageFileKey);
                        existingPodcastEpisode.MainImageFileKey = MainImageFileKey;
                    }
                    await _podcastEpisodeGenericRepository.UpdateAsync(existingPodcastEpisode.Id, existingPodcastEpisode);
                    // Update hashtags
                    await _unitOfWork.PodcastEpisodeHashtagRepository.DeleteByPodcastEpisodeIdAsync(existingPodcastEpisode.Id);

                    foreach (var hashtagId in updateEpisodeParameterDTO.HashtagIds)
                    {
                        var existingHashtag = await _hashtagGenericRepository.FindByIdAsync(hashtagId);
                        if (existingHashtag == null)
                        {
                            throw new Exception("Hashtag with id " + hashtagId + " does not exist");
                        }
                        var podcastEpisodeHashtag = new PodcastEpisodeHashtag
                        {
                            PodcastEpisodeId = existingPodcastEpisode.Id,
                            HashtagId = hashtagId
                        };
                        await _podcastEpisodeHashtagGenericRepository.CreateAsync(podcastEpisodeHashtag);
                    }
                    await transaction.CommitAsync();
                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = existingPodcastEpisode.Id;
                    messageNextRequestData["PodcasterId"] = updateEpisodeParameterDTO.PodcasterId;
                    messageNextRequestData["Name"] = existingPodcastEpisode.Name;
                    messageNextRequestData["Description"] = existingPodcastEpisode.Description;
                    messageNextRequestData["ExplicitContent"] = existingPodcastEpisode.ExplicitContent;
                    messageNextRequestData["MainImageFileKey"] = existingPodcastEpisode.MainImageFileKey;
                    messageNextRequestData["PodcastEpisodeSubscriptionTypeId"] = existingPodcastEpisode.PodcastEpisodeSubscriptionTypeId;
                    messageNextRequestData["SeasonNumber"] = existingPodcastEpisode.SeasonNumber;
                    messageNextRequestData["EpisodeOrder"] = existingPodcastEpisode.EpisodeOrder;
                    messageNextRequestData["HashtagIds"] = JArray.FromObject(updateEpisodeParameterDTO.HashtagIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = existingPodcastEpisode.Id,
                        PodcasterId = updateEpisodeParameterDTO.PodcasterId,
                        Name = existingPodcastEpisode.Name,
                        Description = existingPodcastEpisode.Description,
                        ExplicitContent = existingPodcastEpisode.ExplicitContent,
                        MainImageFileKey = existingPodcastEpisode.MainImageFileKey,
                        PodcastEpisodeSubscriptionTypeId = existingPodcastEpisode.PodcastEpisodeSubscriptionTypeId,
                        SeasonNumber = existingPodcastEpisode.SeasonNumber,
                        EpisodeOrder = existingPodcastEpisode.EpisodeOrder,
                        HashtagIds = updateEpisodeParameterDTO.HashtagIds,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-episode.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Update podcast episode failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-episode.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task UploadPodcastEpisodeLicenseFiles(UploadEpisodeLicensesParameterDTO uploadEpisodeLicenseFilesParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // No database changes needed, just commit transaction
                    var podcastEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(uploadEpisodeLicenseFilesParameterDTO.PodcastEpisodeId,
                        includeFunc: q => q.Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                    );
                    if (podcastEpisode == null)
                    {
                        throw new Exception("Podcast episode with id " + uploadEpisodeLicenseFilesParameterDTO.PodcastEpisodeId + " does not exist");
                    }
                    else if (podcastEpisode.DeletedAt != null)
                    {
                        throw new Exception("Podcast episode with id " + uploadEpisodeLicenseFilesParameterDTO.PodcastEpisodeId + " has been deleted");
                    }
                    else if (podcastEpisode.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Removed)
                    {
                        throw new Exception("Podcast episode with id " + uploadEpisodeLicenseFilesParameterDTO.PodcastEpisodeId + " has been removed");
                    }

                    if (podcastEpisode.PodcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + podcastEpisode.PodcastShow.Id + " does not exist");
                    }
                    else if (podcastEpisode.PodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + podcastEpisode.PodcastShow.Id + " has been deleted");
                    }
                    else if (podcastEpisode.PodcastShow.PodcasterId != uploadEpisodeLicenseFilesParameterDTO.PodcasterId)
                    {
                        throw new Exception("Podcast show with id " + podcastEpisode.PodcastShow.Id + " does not belong to podcaster with id " + uploadEpisodeLicenseFilesParameterDTO.PodcasterId);
                    }
                    else if (podcastEpisode.PodcastShow.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnum.Removed)
                    {
                        throw new Exception("Podcast show with id " + podcastEpisode.PodcastShow.Id + " has been removed");
                    }

                    // var existingPodcastEpisodeLicense = await _podcastEpisodeLicenseGenericRepository.FindAll(
                    //     predicate: pel => pel.PodcastEpisodeId == uploadEpisodeLicenseFilesParameterDTO.PodcastEpisodeId
                    // ).ToListAsync();

                    // foreach (var pel in existingPodcastEpisodeLicense)
                    // {
                    //     await _podcastEpisodeLicenseGenericRepository.DeleteAsync(pel.Id);
                    //     if (pel.LicenseDocumentFileKey != null && pel.LicenseDocumentFileKey != "")
                    //     {
                    //         await _fileIOHelper.DeleteFileAsync(pel.LicenseDocumentFileKey);
                    //     }
                    // }

                    var LicenseDocumentFileKeys = uploadEpisodeLicenseFilesParameterDTO.LicenseDocumentFileKeys;
                    Dictionary<string, string> podcastEpisodeLicensesDict = new Dictionary<string, string>();
                    var folderPath = _filePathConfig.PODCAST_EPISODE_FILE_PATH + "\\" + podcastEpisode.Id;
                    for (int i = 0; i < LicenseDocumentFileKeys.Count; i++)
                    {
                        var licenseDocumentFileKey = LicenseDocumentFileKeys[i];
                        if (licenseDocumentFileKey != null && licenseDocumentFileKey != "")
                        {
                            string fileNameWithoutExtension = FilePathHelper.GetFileNameWithoutExtension(licenseDocumentFileKey);
                            // kiểm tra tên file có phải là con số nguyên không để dùng nó làm PodcastEpisodeLicenseTypeId
                            fileNameWithoutExtension = fileNameWithoutExtension.Trim().Split('_')[1];
                            if (int.TryParse(fileNameWithoutExtension, out int licenseTypeId))
                            {
                                var existingLicenseType = await _podcastEpisodeLicenseTypeGenericRepository.FindByIdAsync(licenseTypeId);
                                if (existingLicenseType == null)
                                {
                                    throw new Exception("Podcast episode license type with id " + licenseTypeId + " does not exist");
                                }
                                var newPodcastLicense = new PodcastEpisodeLicense
                                {
                                    PodcastEpisodeId = uploadEpisodeLicenseFilesParameterDTO.PodcastEpisodeId,
                                    LicenseDocumentFileKey = "",
                                    PodcastEpisodeLicenseTypeId = licenseTypeId
                                };

                                await _podcastEpisodeLicenseGenericRepository.CreateAsync(newPodcastLicense);

                                var newLicenseDocumentFileKey = FilePathHelper.CombinePaths(folderPath, $"{newPodcastLicense.Id}_license_document{FilePathHelper.GetExtension(licenseDocumentFileKey)}");
                                // await _fileIOHelper.CopyFileToFileAsync(licenseDocumentFileKey, newLicenseDocumentFileKey);
                                // await _fileIOHelper.DeleteFileAsync(licenseDocumentFileKey);
                                // LicenseDocumentFileKeys[i] = newLicenseDocumentFileKey;

                                podcastEpisodeLicensesDict[licenseDocumentFileKey] = newLicenseDocumentFileKey;

                                newPodcastLicense.LicenseDocumentFileKey = newLicenseDocumentFileKey;
                                await _podcastEpisodeLicenseGenericRepository.UpdateAsync(newPodcastLicense.Id, newPodcastLicense);
                            }
                            else
                            {
                                throw new Exception("Invalid license document file name: " + fileNameWithoutExtension + ". It should be a valid PodcastEpisodeLicenseTypeId.");
                            }
                        }
                    }
                    // Move files after all validations and database operations are done
                    foreach (var kvp in podcastEpisodeLicensesDict)
                    {
                        await _fileIOHelper.CopyFileToFileAsync(kvp.Key, kvp.Value);
                        await _fileIOHelper.DeleteFileAsync(kvp.Key);
                    }


                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = uploadEpisodeLicenseFilesParameterDTO.PodcastEpisodeId;
                    messageNextRequestData["LicenseDocumentFileKeys"] = JArray.FromObject(LicenseDocumentFileKeys);
                    messageNextRequestData["PodcasterId"] = uploadEpisodeLicenseFilesParameterDTO.PodcasterId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = uploadEpisodeLicenseFilesParameterDTO.PodcastEpisodeId,
                        LicenseDocumentFileKeys = LicenseDocumentFileKeys,
                        PodcasterId = uploadEpisodeLicenseFilesParameterDTO.PodcasterId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "upload-episode-licenses.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Upload podcast episode license files failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "upload-episode-licenses.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeletePodcastEpisodeLicenseFiles(DeleteEpisodeLicensesParameterDTO deleteEpisodeLicensesParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    List<string> licenseFileKeysToDelete = new List<string>();
                    foreach (var podcastEpisodeLicenseId in deleteEpisodeLicensesParameterDTO.PodcastEpisodeLicenseIds)
                    {
                        var existingPodcastEpisodeLicense = await _podcastEpisodeLicenseGenericRepository.FindByIdAsync(podcastEpisodeLicenseId);
                        if (existingPodcastEpisodeLicense == null)
                        {
                            throw new Exception("Podcast episode license with id " + podcastEpisodeLicenseId + " does not exist");
                        }
                        if (existingPodcastEpisodeLicense.LicenseDocumentFileKey != null && existingPodcastEpisodeLicense.LicenseDocumentFileKey != "")
                        {
                            licenseFileKeysToDelete.Add(existingPodcastEpisodeLicense.LicenseDocumentFileKey);
                        }
                        await _podcastEpisodeLicenseGenericRepository.DeleteAsync(podcastEpisodeLicenseId);
                    }
                    // Delete files after all database operations are done
                    foreach (var fileKey in licenseFileKeysToDelete)
                    {
                        await _fileIOHelper.DeleteFileAsync(fileKey);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = deleteEpisodeLicensesParameterDTO.PodcastEpisodeId;
                    messageNextRequestData["PodcastEpisodeLicenseIds"] = JArray.FromObject(deleteEpisodeLicensesParameterDTO.PodcastEpisodeLicenseIds);
                    messageNextRequestData["PodcasterId"] = deleteEpisodeLicensesParameterDTO.PodcasterId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = deleteEpisodeLicensesParameterDTO.PodcastEpisodeId,
                        PodcastEpisodeLicenseIds = deleteEpisodeLicensesParameterDTO.PodcastEpisodeLicenseIds,
                        PodcasterId = deleteEpisodeLicensesParameterDTO.PodcasterId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-episode-licenses.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete podcast episode licenses failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-episode-licenses.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }


        public async Task SubmitPodcastEpisodeAudioFile(SubmitEpisodeAudioFileParameterDTO submitEpisodeAudioFileParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // * quá trình upload audio:
                    // 	+ DRAFT:
                    // 		+ xoá audio cũ (bao gồm cả playlist nếu có)
                    // 		+ Update: AudioFileKey, AudioFileSize, AudioLength
                    // 		+ Lưu audio mới
                    // 		+ Reset: AudioFingerPrint = null, AudioTranscript = null
                    // 	+ Audio Processing:
                    // 		+ không thể upload audio trong trường hợp này (ở tất cả các hàng động khác cũng không được phép làm gì hết, ngoài trừ xoá)
                    // 	+ PENDING REVIEW:
                    // 		+ không thể upload audio trong trường hợp này (phải discard  trước)
                    // 	+ PENDING EDIT REQUIRED:
                    //  		+ xoá audio cũ (bao gồm cả playlist nếu có)
                    //  		+ Update: AudioFileKey, AudioFileSize, AudioLength
                    //  		+ Lưu audio mới
                    //  		+ Reset: AudioFingerPrint = null, AudioTranscript = null
                    // 		+ Chuyển episode status → Audio Processing (processing để cập nhật (AudioFingerPrint/AudioTranscript) xong sẽ chuyển qua Pending Review)
                    // 		+ Update review session status → Pending Review (Tăng reReviewCount += 1)
                    // 	+ READY TO RELEASE:
                    // 		+ xoá audio cũ (bao gồm cả playlist nếu có)
                    //  		+ Update: AudioFileKey, AudioFileSize, AudioLength
                    // 		+ Lưu audio mới
                    // 		+ Reset: AudioFingerPrint = null, AudioTranscript = null, IsAudioPublishable = null
                    // 		+ Xóa tất cả PodcastEpisodeIllegalContentTypeMarking
                    // 		+ Xóa tất cả PodcastEpisodeLicense (và file trên cloud)
                    // 		+ Chuyển episode status → Draft
                    // 	+ PUBLISHED
                    // 		+ không thể upload audio trong trường hợp này
                    // 	+ TAKEN DOWN
                    // 		+ không thể upload audio trong trường hợp này
                    // 	+ REMOVED
                    // 		+ không thể upload audio trong trường hợp này
                    var existingPodcastEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(submitEpisodeAudioFileParameterDTO.PodcastEpisodeId,
                        includeFunc: q => q.Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastChannel)
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                    );
                    if (existingPodcastEpisode == null)
                    {
                        throw new Exception("Podcast episode with id " + submitEpisodeAudioFileParameterDTO.PodcastEpisodeId + " does not exist");
                    }
                    else if (existingPodcastEpisode.DeletedAt != null)
                    {
                        throw new Exception("Podcast episode with id " + submitEpisodeAudioFileParameterDTO.PodcastEpisodeId + " has been deleted");
                    }
                    else if (existingPodcastEpisode.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.AudioProcessing)
                    {
                        throw new Exception("Podcast episode with id " + submitEpisodeAudioFileParameterDTO.PodcastEpisodeId + " is in audio processing and cannot upload new audio");
                    }
                    else if (existingPodcastEpisode.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.PendingReview)
                    {
                        throw new Exception("Podcast episode with id " + submitEpisodeAudioFileParameterDTO.PodcastEpisodeId + " is pending review and cannot upload new audio");
                    }
                    else if (existingPodcastEpisode.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published)
                    {
                        throw new Exception("Podcast episode with id " + submitEpisodeAudioFileParameterDTO.PodcastEpisodeId + " is published and cannot upload new audio");
                    }
                    else if (existingPodcastEpisode.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.TakenDown)
                    {
                        throw new Exception("Podcast episode with id " + submitEpisodeAudioFileParameterDTO.PodcastEpisodeId + " has been taken down and cannot upload new audio");
                    }
                    else if (existingPodcastEpisode.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Removed)
                    {
                        throw new Exception("Podcast episode with id " + submitEpisodeAudioFileParameterDTO.PodcastEpisodeId + " has been removed");
                    }

                    if (existingPodcastEpisode.PodcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " does not exist");
                    }
                    else if (existingPodcastEpisode.PodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been deleted");
                    }
                    else if (existingPodcastEpisode.PodcastShow.PodcasterId != submitEpisodeAudioFileParameterDTO.PodcasterId)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " does not belong to podcaster with id " + submitEpisodeAudioFileParameterDTO.PodcasterId);
                    }
                    else if (existingPodcastEpisode.PodcastShow.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnum.Removed)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been removed");
                    }

                    if (existingPodcastEpisode.PodcastShow.PodcastChannel != null)
                    {
                        if (existingPodcastEpisode.PodcastShow.PodcastChannel.DeletedAt != null)
                        {
                            throw new Exception("Podcast channel with id " + existingPodcastEpisode.PodcastShow.PodcastChannel.Id + " has been deleted");
                        }
                        else if (existingPodcastEpisode.PodcastShow.PodcastChannel.PodcasterId != submitEpisodeAudioFileParameterDTO.PodcasterId)
                        {
                            throw new Exception("Podcast channel with id " + existingPodcastEpisode.PodcastShow.PodcastChannel.Id + " does not belong to podcaster with id " + submitEpisodeAudioFileParameterDTO.PodcasterId);
                        }
                    }

                    var existingPodcaster = await _accountCachingService.GetAccountStatusCacheById(existingPodcastEpisode.PodcastShow.PodcasterId);
                    if (existingPodcaster == null || existingPodcaster.Id != existingPodcastEpisode.PodcastShow.PodcasterId || existingPodcaster.IsVerified == false || existingPodcaster.DeactivatedAt != null || existingPodcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + existingPodcastEpisode.PodcastShow.PodcasterId + " does not exist");
                    }



                    // 	+ DRAFT:
                    // 		+ xoá audio cũ (bao gồm cả playlist nếu có)
                    // 		+ Update: AudioFileKey, AudioFileSize, AudioLength
                    // 		+ Lưu audio mới
                    // 		+ Reset: AudioFingerPrint = null, AudioTranscript = null
                    // 	+ PENDING EDIT REQUIRED:
                    //  		+ xoá audio cũ (bao gồm cả playlist nếu có)
                    //  		+ Update: AudioFileKey, AudioFileSize, AudioLength
                    //  		+ Lưu audio mới
                    //  		+ Reset: AudioFingerPrint = null, AudioTranscript = null
                    // 		+ Chuyển episode status → Audio Processing (processing để cập nhật (AudioFingerPrint/AudioTranscript) xong sẽ chuyển qua Pending Review)
                    // 		+ Update review session status → Pending Review (Tăng reReviewCount += 1)
                    // 	+ READY TO RELEASE:
                    // 		+ xoá audio cũ (bao gồm cả playlist nếu có)
                    //  		+ Update: AudioFileKey, AudioFileSize, AudioLength
                    // 		+ Lưu audio mới
                    // 		+ Reset: AudioFingerPrint = null, AudioTranscript = null, IsAudioPublishable = null
                    // 		+ Xóa tất cả PodcastEpisodeIllegalContentTypeMarking
                    // 		+ Xóa tất cả PodcastEpisodeLicense (và file trên cloud)
                    // 		+ Chuyển episode status → Draft

                    var episodeCurrentStatus = existingPodcastEpisode.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId;

                    if (episodeCurrentStatus == (int)PodcastEpisodeStatusEnum.Draft)
                    {
                        var folderPath = _filePathConfig.PODCAST_EPISODE_FILE_PATH + "\\" + existingPodcastEpisode.Id;

                        if (existingPodcastEpisode.AudioFileKey != null && existingPodcastEpisode.AudioFileKey != "")
                        {
                            await _fileIOHelper.DeleteFileAsync(existingPodcastEpisode.AudioFileKey);
                            var existingPlaylistFolderKey = FilePathHelper.CombinePaths(
                                folderPath,
                                "playlist"
                            );
                            await _fileIOHelper.DeleteFolderAsync(existingPlaylistFolderKey);
                        }
                        // Upload the new audio file
                        var newAudioFileKey = FilePathHelper.CombinePaths(folderPath, $"audio{FilePathHelper.GetExtension(submitEpisodeAudioFileParameterDTO.AudioFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(submitEpisodeAudioFileParameterDTO.AudioFileKey, newAudioFileKey);
                        await _fileIOHelper.DeleteFileAsync(submitEpisodeAudioFileParameterDTO.AudioFileKey);

                        existingPodcastEpisode.AudioFileKey = newAudioFileKey;
                        existingPodcastEpisode.AudioFileSize = submitEpisodeAudioFileParameterDTO.AudioFileSize;
                        existingPodcastEpisode.AudioLength = submitEpisodeAudioFileParameterDTO.AudioLength;
                        existingPodcastEpisode.AudioFingerPrint = null;
                        existingPodcastEpisode.AudioTranscript = null;
                    }
                    else if (episodeCurrentStatus == (int)PodcastEpisodeStatusEnum.PendingEditRequired)
                    {
                        var folderPath = _filePathConfig.PODCAST_EPISODE_FILE_PATH + "\\" + existingPodcastEpisode.Id;

                        if (existingPodcastEpisode.AudioFileKey != null && existingPodcastEpisode.AudioFileKey != "")
                        {
                            await _fileIOHelper.DeleteFileAsync(existingPodcastEpisode.AudioFileKey);
                            var existingPlaylistFolderKey = FilePathHelper.CombinePaths(
                                folderPath,
                                "playlist"
                            );
                            await _fileIOHelper.DeleteFolderAsync(existingPlaylistFolderKey);
                        }
                        // Upload the new audio file
                        var newAudioFileKey = FilePathHelper.CombinePaths(folderPath, $"audio{FilePathHelper.GetExtension(submitEpisodeAudioFileParameterDTO.AudioFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(submitEpisodeAudioFileParameterDTO.AudioFileKey, newAudioFileKey);
                        Console.WriteLine("After copy file to new audio file key: " + submitEpisodeAudioFileParameterDTO.AudioFileKey + " to " + newAudioFileKey);
                        await _fileIOHelper.DeleteFileAsync(submitEpisodeAudioFileParameterDTO.AudioFileKey);

                        existingPodcastEpisode.AudioFileKey = newAudioFileKey;
                        existingPodcastEpisode.AudioFileSize = submitEpisodeAudioFileParameterDTO.AudioFileSize;
                        existingPodcastEpisode.AudioLength = submitEpisodeAudioFileParameterDTO.AudioLength;
                        existingPodcastEpisode.AudioFingerPrint = null;
                        existingPodcastEpisode.AudioTranscript = null;

                        // Change episode status to Audio Processing
                        var newStatusTracking = new PodcastEpisodeStatusTracking
                        {
                            PodcastEpisodeId = existingPodcastEpisode.Id,
                            PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.AudioProcessing
                        };
                        await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);

                        // Update review session status to Pending Review (increase reReviewCount by 1)
                        JObject requestData = JObject.FromObject(
                            new
                            {
                                PodcastEpisodeId = existingPodcastEpisode.Id,
                                PodcasterId = existingPodcaster.Id,
                            }
                        );

                        var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "episode-draft-audio-processing-flow");
                        await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
                    }
                    else if (episodeCurrentStatus == (int)PodcastEpisodeStatusEnum.ReadyToRelease)
                    {
                        var folderPath = _filePathConfig.PODCAST_EPISODE_FILE_PATH + "\\" + existingPodcastEpisode.Id;

                        if (existingPodcastEpisode.AudioFileKey != null && existingPodcastEpisode.AudioFileKey != "")
                        {
                            await _fileIOHelper.DeleteFileAsync(existingPodcastEpisode.AudioFileKey);
                            var existingPlaylistFolderKey = FilePathHelper.CombinePaths(
                                folderPath,
                                "playlist"
                            );
                            await _fileIOHelper.DeleteFolderAsync(existingPlaylistFolderKey);
                        }
                        // Upload the new audio file
                        var newAudioFileKey = FilePathHelper.CombinePaths(folderPath, $"audio{FilePathHelper.GetExtension(submitEpisodeAudioFileParameterDTO.AudioFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(submitEpisodeAudioFileParameterDTO.AudioFileKey, newAudioFileKey);
                        await _fileIOHelper.DeleteFileAsync(submitEpisodeAudioFileParameterDTO.AudioFileKey);

                        existingPodcastEpisode.AudioFileKey = newAudioFileKey;
                        existingPodcastEpisode.AudioFileSize = submitEpisodeAudioFileParameterDTO.AudioFileSize;
                        existingPodcastEpisode.AudioLength = submitEpisodeAudioFileParameterDTO.AudioLength;
                        existingPodcastEpisode.AudioFingerPrint = null;
                        existingPodcastEpisode.AudioTranscript = null;
                        existingPodcastEpisode.IsAudioPublishable = null;

                        // Delete all PodcastEpisodeIllegalContentTypeMarking
                        await _unitOfWork.PodcastEpisodeIllegalContentTypeMarkingRepository.DeleteByPodcastEpisodeIdAsync(existingPodcastEpisode.Id);
                        // Delete all PodcastEpisodeLicense and files on cloud
                        var existingPodcastEpisodeLicenses = await _podcastEpisodeLicenseGenericRepository.FindAll(
                            predicate: pel => pel.PodcastEpisodeId == existingPodcastEpisode.Id
                        ).ToListAsync();
                        foreach (var pel in existingPodcastEpisodeLicenses)
                        {
                            if (pel.LicenseDocumentFileKey != null && pel.LicenseDocumentFileKey != "")
                            {
                                await _fileIOHelper.DeleteFileAsync(pel.LicenseDocumentFileKey);
                            }
                            await _podcastEpisodeLicenseGenericRepository.DeleteAsync(pel.Id);
                        }
                        // Change episode status to Draft
                        var newStatusTracking = new PodcastEpisodeStatusTracking
                        {
                            PodcastEpisodeId = existingPodcastEpisode.Id,
                            PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Draft
                        };
                        await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);
                    }


                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = existingPodcastEpisode.Id;
                    messageNextRequestData["PodcasterId"] = existingPodcaster.Id;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = existingPodcastEpisode.Id,
                        PodcasterId = existingPodcaster.Id,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "submit-episode-audio-file.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Submit podcast episode audio file failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "submit-episode-audio-file.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task ProcessPodcastEpisodeDraftAudio(ProcessingEpisodeDraftAudioParameterDTO processingEpisodeDraftAudioParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //                     Draft AUDIO PROCESSING: chạy khi yêu cầu kiểm duyệt / upload lại audio khi pending edit required
                    // 		+ XỬ LÍ TRANSTRIPT
                    // 		+ XỬ LÍ FINGERFRINT
                    // 		+ Cập nhật fingerprint
                    // 		+ cập nhật transcript
                    // 		+ KIỂM TRA PUBLISH REVIEW SESSION ĐANG PENDING REVIEW:
                    // 			+ CÓ: 
                    // 				+ CỘNG 1 VÀO PRENDING REVIEW, 
                    // 				+ CẬP NHẬT LẠI SỐ AUDIO BỊ TRÙNG
                    // 				+ CẬP NHẬT CÁC TỪ RESTRICT QUÉT ĐƯỢC
                    // 				+ chuyển trạng thái của episode sang pending review
                    // 			+ KHÔNG CÓ && THOẢ 1 TRONG 2 ĐIỀU KIỀN VI PHẠM (RESTRICT TERM / DUPLICATION):
                    // 				+ tạo 1 publish review session
                    // 				+ CẬP NHẬT LẠI SỐ AUDIO BỊ TRÙNG
                    //  				+ CẬP NHẬT CÁC TỪ RESTRICT QUÉT ĐƯỢC
                    // 				+ chuyển trạng thái của episode sang pending review
                    // 			+ Không có && không thoả điều kiện vi phạm nào:
                    // 				+ chuyển trạng thái của episode sang Ready to release
                    var existingPodcastEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(processingEpisodeDraftAudioParameterDTO.PodcastEpisodeId,
                        includeFunc: q => q
                        .Include(pe => pe.PodcastShow)
                    );
                    if (existingPodcastEpisode == null)
                    {
                        throw new Exception("Podcast episode with id " + processingEpisodeDraftAudioParameterDTO.PodcastEpisodeId + " does not exist");
                    }
                    else if (existingPodcastEpisode.DeletedAt != null)
                    {
                        throw new Exception("Podcast episode with id " + processingEpisodeDraftAudioParameterDTO.PodcastEpisodeId + " has been deleted");
                    }

                    var existingPodcaster = await _accountCachingService.GetAccountStatusCacheById(existingPodcastEpisode.PodcastShow.PodcasterId);
                    if (existingPodcaster == null || existingPodcaster.Id != existingPodcastEpisode.PodcastShow.PodcasterId || existingPodcaster.IsVerified == false || existingPodcaster.DeactivatedAt != null || existingPodcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + existingPodcastEpisode.PodcastShow.PodcasterId + " does not exist");
                    }

                    // var activeSystemConfigProfile = await GetActiveSystemConfigProfile();
                    var restrictedTerms = await GetAllPodcastRestrictedTerms();

                    using (Stream audioFileStream = await _fileIOHelper.GetFileStreamAsync(existingPodcastEpisode.AudioFileKey))
                    {
                        Console.WriteLine("Audio file key: " + existingPodcastEpisode.AudioFileKey);
                        Console.WriteLine("Audio file is null: " + (audioFileStream == null));
                        // Tạo 2 copy riêng biệt cho 2 operations
                        using var transcriptionStreamCopy = new MemoryStream();
                        using var fingerprintStreamCopy = new MemoryStream();

                        // Đọc toàn bộ stream vào buffer trước
                        var buffer = new byte[81920]; // 80KB buffer
                        int bytesRead;

                        // Copy vào transcription stream
                        while ((bytesRead = await audioFileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await transcriptionStreamCopy.WriteAsync(buffer, 0, bytesRead);
                        }

                        // Reset audioFileStream nếu có thể, nếu không thì tạo lại từ file
                        if (audioFileStream.CanSeek)
                        {
                            audioFileStream.Position = 0;
                        }
                        else
                        {
                            // Nếu không thể seek, đọc lại từ file
                            using var audioFileStreamForFingerprint = await _fileIOHelper.GetFileStreamAsync(existingPodcastEpisode.AudioFileKey);
                            while ((bytesRead = await audioFileStreamForFingerprint.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fingerprintStreamCopy.WriteAsync(buffer, 0, bytesRead);
                            }
                        }

                        // Reset position cho cả 2 stream
                        transcriptionStreamCopy.Position = 0;
                        fingerprintStreamCopy.Position = 0;

                        // Chạy đồng thời với Task.WhenAll
                        var transcriptionTask = _audioTranscriptionService.TranscribeAudioAsync(transcriptionStreamCopy);
                        var fingerprintTask = _acoustIDAudioFingerprintGenerator.GenerateFingerprintAsync(fingerprintStreamCopy);

                        var startsw = System.Diagnostics.Stopwatch.StartNew();

                        await Task.WhenAll(transcriptionTask, fingerprintTask);

                        // Lấy kết quả từ từng task
                        string transcript = await transcriptionTask;
                        AcoustIDAudioFingerprintGeneratedResult audioFingerPrint = await fingerprintTask;

                        Console.WriteLine("Generated Transcript: " + transcript);
                        Console.WriteLine("Generated Fingerprint: " + audioFingerPrint.FingerprintData);
                        startsw.Stop();
                        Console.WriteLine($"Audio processing completed in {startsw.ElapsedMilliseconds} ms");
                        List<string> detectedRestrictTerms = ScanTranscriptionForRestrictedTerms(transcript, restrictedTerms);
                        Console.WriteLine($"Scan restrict terms: {string.Join(", ", detectedRestrictTerms)}");

                        // Chuẩn bị so sánh fingerprint
                        var publishedEpisode = await _podcastEpisodeGenericRepository.FindAll(
                            predicate: pe => pe.Id != existingPodcastEpisode.Id &&
                                pe.AudioFingerPrint != null &&
                                pe.DeletedAt == null &&
                                pe.PodcastEpisodeStatusTrackings
                                    .OrderByDescending(pet => pet.CreatedAt)
                                    .FirstOrDefault()
                                    .PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published,
                            includeFunc: q => q.Include(pe => pe.PodcastEpisodeStatusTrackings)
                        ).ToListAsync();
                        AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparison comparison = null;
                        AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparisonPercentageResult comparisonResult = new AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparisonPercentageResult
                        {
                            results = new List<AcoustIDAudioFingerprintSimilarityPercentageResult>()
                        };
                        if (publishedEpisode.Count > 0)
                        {
                            comparison.Target = new AcoustIDAudioFingerprintComparisonObject
                            {
                                AudioFingerPrint = audioFingerPrint.FingerprintData,
                                Id = existingPodcastEpisode.Id.ToString(),
                            };
                            comparison.Candidates = publishedEpisode.Select(pe => new AcoustIDAudioFingerprintComparisonObject
                            {
                                AudioFingerPrint = System.Text.Encoding.UTF8.GetString(pe.AudioFingerPrint),
                                Id = pe.Id.ToString(),
                            }).ToList();
                            comparisonResult = _acoustIDAudioFingerprintComparator.CompareTargetToCandidates(comparison);
                        }
                        // Lọc ra các episode bị trùng dựa trên ngưỡng similarity
                        List<Guid> duplicateEpisodeIds = comparisonResult.results
                            .Where(res => res.SimilarityPercentage >= _podcastPublishReviewSessionConfig.MinDuplicateSimilarityRate)
                            .Select(res => Guid.Parse(res.Id.ToString()))
                            .ToList();

                        // Đếm số term bị vi phạm so với TranscriptionMinRestrictedTermCount
                        bool violatedTermCount = detectedRestrictTerms.Count >= _podcastPublishReviewSessionConfig.TranscriptionMinRestrictedTermCount;

                        existingPodcastEpisode.AudioTranscript = transcript;
                        existingPodcastEpisode.AudioFingerPrint = audioFingerPrint.FingerprintData != null
                            ? System.Text.Encoding.UTF8.GetBytes(audioFingerPrint.FingerprintData)
                            : null;

                        await _podcastEpisodeGenericRepository.UpdateAsync(existingPodcastEpisode.Id, existingPodcastEpisode);

                        // KIỂM TRA PUBLISH REVIEW SESSION ĐANG PENDING REVIEW:
                        // 			+ CÓ: [DANH SÁCH CÁC SESSION ĐANG PENDING]
                        // 				+ CỘNG 1 VÀO PRENDING REVIEW, 
                        // 				+ CẬP NHẬT LẠI SỐ AUDIO BỊ TRÙNG
                        // 				+ CẬP NHẬT CÁC TỪ RESTRICT QUÉT ĐƯỢC
                        // 				+ chuyển trạng thái của episode sang pending review

                        var pendingReviewSession = await _podcastEpisodePublishReviewSessionGenericRepository.FindAll(
                            predicate: pers => pers.PodcastEpisodeId == existingPodcastEpisode.Id &&
                                pers.PodcastEpisodePublishReviewSessionStatusTrackings
                                    .OrderByDescending(persst => persst.CreatedAt)
                                    .FirstOrDefault()
                                    .PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview,
                            includeFunc: q => q.Include(pers => pers.PodcastEpisodePublishReviewSessionStatusTrackings)
                        ).FirstOrDefaultAsync();

                        if (pendingReviewSession != null)
                        {
                            // CỘNG 1 VÀO PRENDING REVIEW
                            pendingReviewSession.ReReviewCount += 1;

                            // xoá toàn bộ các PodcastEpisodePublishDuplicateDetection cũ để thêm mới
                            await _unitOfWork.PodcastEpisodePublishDuplicateDetectionRepository.DeleteByPublishReviewSessionIdAsync(pendingReviewSession.Id);
                            if (duplicateEpisodeIds.Count > 0)
                            {
                                foreach (var duplicateEpisodeId in duplicateEpisodeIds)
                                {
                                    var newDuplicateDetection = new PodcastEpisodePublishDuplicateDetection
                                    {
                                        PodcastEpisodePublishReviewSessionId = pendingReviewSession.Id,
                                        DuplicatePodcastEpisodeId = duplicateEpisodeId
                                    };
                                    await _podcastEpisodePublishDuplicateDetectionGenericRepository.CreateAsync(newDuplicateDetection);
                                }
                            }

                            var episodeNewStatusTracking = new PodcastEpisodeStatusTracking
                            {
                                PodcastEpisodeId = existingPodcastEpisode.Id,
                                PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.PendingReview
                            };
                            await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(episodeNewStatusTracking);


                            await _podcastEpisodePublishReviewSessionGenericRepository.UpdateAsync(pendingReviewSession.Id, pendingReviewSession);
                        }
                        else if (pendingReviewSession == null && (violatedTermCount || duplicateEpisodeIds.Count > 0))
                        {
                            // 			+ KHÔNG CÓ && THOẢ 1 TRONG 2 ĐIỀU KIỀN VI PHẠM (RESTRICT TERM / DUPLICATION):
                            // 				+ tạo 1 publish review session

                            // Lọc ra các staff id đã được assign vào các phiên review trước đó trong hệ thống
                            // từ đó so với danh sách available staff truy vấn được từ Userservice để group lại các staff ít được assign nhất, nếu danh sách > 1 thì random chọn
                            List<int> assignedStaffIds = await _podcastEpisodePublishReviewSessionGenericRepository.FindAll(
                                predicate: null,
                                includeFunc: null
                            ).Select(pprs => pprs.AssignedStaff).ToListAsync();

                            List<AccountDTO> availableStaff = await GetAllAvailableStaffs();

                            foreach (var staff in availableStaff)
                            {
                                if (!assignedStaffIds.Contains(staff.Id))
                                {
                                    assignedStaffIds.Add(staff.Id);
                                }
                            }
                            
                            var newPublishReviewSession = new PodcastEpisodePublishReviewSession
                            {
                                PodcastEpisodeId = existingPodcastEpisode.Id,
                                AssignedStaffIds = AssignedStaff,    
                                ReReviewCount = 0,
                            };
                            await _podcastEpisodePublishReviewSessionGenericRepository.CreateAsync(newPublishReviewSession);

                            // 				+ CẬP NHẬT LẠI SỐ AUDIO BỊ TRÙNG
                            if (duplicateEpisodeIds.Count > 0)
                            {
                                foreach (var duplicateEpisodeId in duplicateEpisodeIds)
                                {
                                    var newDuplicateDetection = new PodcastEpisodePublishDuplicateDetection
                                    {
                                        PodcastEpisodePublishReviewSessionId = newPublishReviewSession.Id,
                                        DuplicatePodcastEpisodeId = duplicateEpisodeId
                                    };
                                    await _podcastEpisodePublishDuplicateDetectionGenericRepository.CreateAsync(newDuplicateDetection);
                                }
                            }

                            var episodeNewStatusTracking = new PodcastEpisodeStatusTracking
                            {
                                PodcastEpisodeId = existingPodcastEpisode.Id,
                                PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.PendingReview
                            };
                            await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(episodeNewStatusTracking);

                        }
                        else
                        {
                            // 			+ Không có && không thoả điều kiện vi phạm nào:
                            // 				+ chuyển trạng thái của episode sang Ready to release
                            var episodeNewStatusTracking = new PodcastEpisodeStatusTracking
                            {
                                PodcastEpisodeId = existingPodcastEpisode.Id,
                                PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.ReadyToRelease
                            };
                            await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(episodeNewStatusTracking);
                        }

                        // chuyển trạng thái của episode sang pending review

                    }




                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = existingPodcastEpisode.Id;
                    messageNextRequestData["PodcasterId"] = existingPodcaster.Id;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = existingPodcastEpisode.Id,
                        PodcasterId = existingPodcaster.Id,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "processing-episode-draft-audio.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Submit podcast episode audio file failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "processing-episode-draft-audio.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

    }
}