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
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.RequestEpisodeAudioExamination;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.DiscardEpisodePublishReviewSession;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PublishEpisode;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.ProcessingEpisodePublishAudio;
using PodcastService.Infrastructure.Models.Audio.Hls;
using PodcastService.Infrastructure.Services.Audio.Hls;
using PodcastService.BusinessLogic.DTOs.Episode.ListItems;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.RequireEpisodePublishReviewSessionEdit;
using System.Security.Claims;
using PodcastService.BusinessLogic.DTOs.PodcastSubscription;
using PodcastService.Infrastructure.Configurations.Audio.Hls.interfaces;
using PodcastService.Infrastructure.Models.Audio.Tuning;
using Microsoft.AspNetCore.Http;
using PodcastService.BusinessLogic.DTOs.AudioTuning;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.DiscardEpisodePublishReviewDmcaRemoveEpisodeForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveEpisodeDmcaRemoveEpisodeForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedEpisodeDmcaUnpublishEpisodeForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UnpublishEpisodeUnpublishEpisodeForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.DiscardShowEpisodesPublishReviewDmcaRemoveShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveShowEpisodesDmcaRemoveShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedShowEpisodesDmcaUnpublishShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.DiscardEpisodePublishReviewEpisodeDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedEpisodeDmcaEpisodeDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteEpisodeEpisodeDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.DiscardShowEpisodesPublishReviewShowDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedShowEpisodesDmcaShowDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteShowEpisodesShowDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveChannelDismissedEpisodesDmcaUnpublishChannelForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.DiscardChannelEpisodesPublishReviewChannelDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveChannelDismissedEpisodesDmcaChannelDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteChannelEpisodesChannelDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UnpublishPodcasterEpisodesTerminatePodcasterForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedEpisodeDmcaTerminatePodcasterForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.DiscardPodcasterEpisodesPublishReviewTerminatePodcasterForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateEpisodeListenSessionDuration;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveEpisodeListenSessionContentDmcaRemoveEpisodeForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveEpisodeListenSessionContentUnpublishEpisodeForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveShowEpisodesListenSessionContentDmcaRemoveShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveShowEpisodesListenSessionContentUnpublishShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveChannelEpisodesListenSessionContentUnpublishChannelForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveEpisodeListenSessionContentEpisodeDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveShowEpisodesListenSessionContentShowDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveChannelEpisodesListenSessionContentChannelDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemovePodcasterEpisodesListenSessionContentTerminatePodcasterForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.TakedownContentDmca;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RestoreContentDmca;
using PodcastService.BusinessLogic.Services.DbServices.CachingServices;
using PodcastService.BusinessLogic.DTOs.Auth;
using PodcastService.BusinessLogic.Enums.App;
using PodcastService.BusinessLogic.DTOs.SystemConfiguration;
using Microsoft.IdentityModel.Tokens;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CompleteAllUserEpisodeListenSessions;
using PodcastService.BusinessLogic.DTOs.Cache.ListesnSessionProcedure;
using PodcastService.BusinessLogic.Enums.ListenSessionProcedure;
using Microsoft.AspNetCore.SignalR;
using PodcastService.BusinessLogic.SignalRHubs;
using Duende.IdentityServer.Models;

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
        private readonly IPodcastListenSessionConfig _podcastListenSessionConfig;
        private readonly IHlsConfig _hlsConfig;
        private readonly ICustomerListenSessionProcedureConfig _customerListenSessionProcedureConfig;

        // DB CONTEXT
        private readonly AppDbContext _appDbContext;

        // SIGNALR HUB CONTEXT
        private readonly IHubContext<PodcastContentNotificationHub> _podcastContentNotificationHubContext;

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
        private readonly IGenericRepository<PodcastEpisodeIllegalContentTypeMarking> _podcastEpisodeIllegalContentTypeMarkingGenericRepository;
        private readonly IGenericRepository<PodcastEpisodePublishReviewSessionStatusTracking> _podcastEpisodePublishReviewSessionStatusTrackingGenericRepository;
        private readonly IGenericRepository<PodcastEpisodeListenSession> _podcastEpisodeListenSessionGenericRepository;
        private readonly IGenericRepository<PodcastEpisodeListenSessionHlsEnckeyRequestToken> _podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository;

        private readonly HttpServiceQueryClient _httpServiceQueryClient;

        // CACHING SERVICE
        private readonly AccountCachingService _accountCachingService;
        private readonly CustomerListenSessionProcedureCachingService _customerListenSessionProcedureCachingService;

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
        private readonly FFMpegCoreHlsService _ffMpegCoreHlsService;
        private readonly AudioTuningService _audioTuningService;

        // HTTP CLIENT
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CrossServiceHttpService _crossServiceHttpService;

        public PodcastEpisodeService(
            ILogger<PodcastEpisodeService> logger,
            AppDbContext appDbContext,

            IHubContext<PodcastContentNotificationHub> podcastContentNotificationHubContext,

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
            IGenericRepository<PodcastEpisodePublishDuplicateDetection> podcastEpisodePublishDuplicateDetectionGenericRepository,
            IGenericRepository<PodcastEpisodeIllegalContentTypeMarking> podcastEpisodeIllegalContentTypeMarkingGenericRepository,
            IGenericRepository<PodcastEpisodePublishReviewSessionStatusTracking> podcastEpisodePublishReviewSessionStatusTrackingGenericRepository,
            IGenericRepository<PodcastEpisodeListenSession> podcastEpisodeListenSessionGenericRepository,
            IGenericRepository<PodcastEpisodeListenSessionHlsEnckeyRequestToken> podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository,

            FileIOHelper fileIOHelper,
            DateHelper dateHelper,

            IFilePathConfig filePathConfig,
            IAppConfig appConfig,
            IPodcastPublishReviewSessionConfig podcastPublishReviewSessionConfig,
            IPodcastListenSessionConfig podcastListenSessionConfig,
            IHlsConfig hlsConfig,
            ICustomerListenSessionProcedureConfig customerListenSessionProcedureConfig,

            HttpServiceQueryClient httpServiceQueryClient,

            AccountCachingService accountCachingService,
            CustomerListenSessionProcedureCachingService customerListenSessionProcedureCachingService,

            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,

            RedisSharedCacheService redisSharedCacheService,

            AudioTranscriptionService audioTranscriptionService,
            AcoustIDAudioFingerprintGenerator audioFingerprintService,
            AcoustIDAudioFingerprintComparator audioFingerprintComparator,
            FFMpegCoreHlsService ffMpegCoreHlsService,
            AudioTuningService audioTuningService,

            IHttpClientFactory httpClientFactory,
            CrossServiceHttpService crossServiceHttpService
            )
        {
            _logger = logger;

            _appDbContext = appDbContext;

            _podcastContentNotificationHubContext = podcastContentNotificationHubContext;

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
            _podcastEpisodePublishReviewSessionGenericRepository = podcastEpisodePublishReviewSessionGenericRepository;
            _podcastEpisodePublishDuplicateDetectionGenericRepository = podcastEpisodePublishDuplicateDetectionGenericRepository;
            _podcastEpisodeIllegalContentTypeMarkingGenericRepository = podcastEpisodeIllegalContentTypeMarkingGenericRepository;
            _podcastEpisodePublishReviewSessionStatusTrackingGenericRepository = podcastEpisodePublishReviewSessionStatusTrackingGenericRepository;
            _podcastEpisodeListenSessionGenericRepository = podcastEpisodeListenSessionGenericRepository;
            _podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository = podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository;

            _fileIOHelper = fileIOHelper;
            _jwtHelper = jwtHelper;
            _bcryptHelper = bcryptHelper;
            _dateHelper = dateHelper;

            _fluentEmailService = fluentEmailService;

            _filePathConfig = filePathConfig;
            _podcastPublishReviewSessionConfig = podcastPublishReviewSessionConfig;
            _appConfig = appConfig;
            _podcastListenSessionConfig = podcastListenSessionConfig;
            _hlsConfig = hlsConfig;
            _customerListenSessionProcedureConfig = customerListenSessionProcedureConfig;

            _httpServiceQueryClient = httpServiceQueryClient;

            _accountCachingService = accountCachingService;
            _customerListenSessionProcedureCachingService = customerListenSessionProcedureCachingService;

            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;

            _redisSharedCacheService = redisSharedCacheService;

            _audioTranscriptionService = audioTranscriptionService;
            _acoustIDAudioFingerprintGenerator = audioFingerprintService;
            _acoustIDAudioFingerprintComparator = audioFingerprintComparator;
            _ffMpegCoreHlsService = ffMpegCoreHlsService;
            _audioTuningService = audioTuningService;

            _httpClientFactory = httpClientFactory;
            _crossServiceHttpService = crossServiceHttpService;
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

        public async Task<SystemConfigProfileDTO> GetActiveSystemConfigProfile()
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

            // return ((JArray)result.Results["activeSystemConfigProfile"]).First as JObject;
            var config = (result.Results["activeSystemConfigProfile"].First as JObject).ToObject<SystemConfigProfileDTO>();
            return config;
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
                        QueryType = "findall",
                        EntityType = "Account",

                        Parameters = JObject.FromObject(new
                        {
                            where = new
                            {
                                RoleId = (int) RoleEnum.Staff,
                                DeactivatedAt = (DateTime?) null
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

        public async Task<AccountDTO> GetAccountById(int accountId)
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
                            id = accountId
                        }),
                    }
                }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
                var account = (result.Results["account"]).ToObject<AccountDTO>();
                return account;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get account failed, error: " + ex.Message);
            }
        }

        public async Task<PodcastSubscriptionRegistrationDTO> GetAccountSubscriptionRegistrationByAccountIdAndSubscriptionId(int accountId, int podcastSubscriptionId)
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                {
                    new BatchQueryItem
                    {
                        Key = "podcastSubscriptionRegistration",
                        QueryType = "findall",
                        EntityType = "PodcastSubscriptionRegistration",

                        Parameters = JObject.FromObject(new
                        {
                            where = new
                            {
                                AccountId = accountId,
                                PodcastSubscriptionId = podcastSubscriptionId,
                                CancelledAt = (DateTime?) null
                            },
                            include = "PodcastSubscription, PodcastSubscription.PodcastSubscriptionBenefitMappings",
                        }),
                    }
                }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("SubscriptionService", batchRequest);
                var podcastSubscriptionRegistration = (result.Results["podcastSubscriptionRegistration"] as JObject)?.ToObject<PodcastSubscriptionRegistrationDTO>();
                return podcastSubscriptionRegistration;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get podcast subscription registration by account id and subscription id failed, error: " + ex.Message);
            }
        }

        // public async Task<List<int>> GetAccountSubscriptionRegistrationBenefitsByPodcastSubscriptionIdAndVersion(int podcastSubscriptionId, int version)
        // {
        //     try
        //     {
        //         var batchRequest = new BatchQueryRequest
        //         {
        //             Queries = new List<BatchQueryItem>
        //         {
        //             new BatchQueryItem
        //             {
        //                 Key = "podcastSubscriptionBenefits",
        //                 QueryType = "findall",
        //                 EntityType = "PodcastSubscriptionBenefitMapping",

        //                 Parameters = JObject.FromObject(new
        //                 {
        //                     where = new
        //                     {
        //                         PodcastSubscriptionId = podcastSubscriptionId,
        //                         Version = version
        //                     },
        //                 }),
        //             }
        //         }
        //         };
        //         var result = await _httpServiceQueryClient.ExecuteBatchAsync("SubscriptionService", batchRequest);
        //         // var podcastSubscription = (result.Results["podcastSubscription"])?.ToObject<PodcastSubscriptionDTO>();
        //         var podcastSubscription = ((JArray)result.Results["podcastSubscriptionBenefits"]).Select(b => b["PodcastSubscriptionBenefitId"].ToObject<int>()).ToList();
        //         return podcastSubscription;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         throw new HttpRequestException("Get podcast subscription registration benefits by podcast subscription id and version failed, error: " + ex.Message);
        //     }
        // }

        public async Task<PodcastSubscriptionDTO> GetActivePodcastSubscriptionByShowId(Guid podcastShowId)
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                {
                    new BatchQueryItem
                    {
                        Key = "podcastSubscription",
                        QueryType = "findall",
                        EntityType = "PodcastSubscription",

                        Parameters = JObject.FromObject(new
                        {
                            where = new
                            {
                                PodcastShowId = podcastShowId,
                                IsActive = true
                            },
                        }),

                    }
                }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("SubscriptionService", batchRequest);
                var podcastSubscription = (result.Results["podcastSubscription"] as JObject)?.ToObject<PodcastSubscriptionDTO>();
                return podcastSubscription;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get podcast subscription by id failed, error: " + ex.Message);
            }
        }

        public async Task<PodcastSubscriptionDTO> GetActivePodcastSubscriptionByChannelId(Guid? podcastChannelId)
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                {
                    new BatchQueryItem
                    {
                        Key = "podcastSubscription",
                        QueryType = "findall",
                        EntityType = "PodcastSubscription",

                        Parameters = JObject.FromObject(new
                        {
                            where = new
                            {
                                PodcastChannelId = podcastChannelId,
                                IsActive = true
                            },
                            include = "PodcastShow"
                        }),
                    }
                }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("SubscriptionService", batchRequest);
                var podcastSubscription = (result.Results["podcastSubscription"] as JObject)?.ToObject<PodcastSubscriptionDTO>();
                return podcastSubscription;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get podcast subscription by id failed, error: " + ex.Message);
            }
        }


        public async Task<Stream> GetEpisodeAudioGeneralTuningSettingsAsync(GeneralTuningProfileRequestInfo generalTuningProfileRequestInfo, Stream audioFile, Guid podcastEpisodeId, int podcasterId)
        {
            try
            {
                var existingPodcastEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(podcastEpisodeId,
                        includeFunc: q => q
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastChannel)
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                    );
                if (existingPodcastEpisode == null)
                {
                    throw new Exception("Podcast episode with id " + podcastEpisodeId + " does not exist");
                }
                else if (existingPodcastEpisode.DeletedAt != null)
                {
                    throw new Exception("Podcast episode with id " + podcastEpisodeId + " has been deleted");
                }
                else if (existingPodcastEpisode.PodcastEpisodeStatusTrackings
                    .OrderByDescending(pet => pet.CreatedAt)
                    .FirstOrDefault()
                    .PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Removed)
                {
                    throw new Exception("Podcast episode with id " + podcastEpisodeId + " has been removed");
                }

                if (existingPodcastEpisode.PodcastShow == null)
                {
                    throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " does not exist");
                }
                else if (existingPodcastEpisode.PodcastShow.DeletedAt != null)
                {
                    throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been deleted");
                }
                else if (existingPodcastEpisode.PodcastShow.PodcasterId != podcasterId)
                {
                    throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " does not belong to podcaster with id " + podcasterId);
                }
                else if (existingPodcastEpisode.PodcastShow.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnum.Removed)
                {
                    throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been removed");
                }

                if (existingPodcastEpisode.PodcastShow.PodcastChannel != null)
                {
                    if (existingPodcastEpisode.PodcastShow.PodcastChannel.DeletedAt != null)
                    {
                        throw new Exception("Podcast channel with id " + existingPodcastEpisode.PodcastShow.PodcastChannelId + " has been deleted");
                    }
                }

                var existingPodcaster = await _accountCachingService.GetAccountStatusCacheById(existingPodcastEpisode.PodcastShow.PodcasterId);
                if (existingPodcaster == null || existingPodcaster.Id != existingPodcastEpisode.PodcastShow.PodcasterId || existingPodcaster.IsVerified == false || existingPodcaster.DeactivatedAt != null || existingPodcaster.HasVerifiedPodcasterProfile == false)
                {
                    throw new Exception("Podcaster with id " + existingPodcastEpisode.PodcastShow.PodcasterId + " does not exist");
                }

                // in tất cả thông tin của generalTuningProfileRequestInfo
                var abc = JsonConvert.SerializeObject(generalTuningProfileRequestInfo);
                Console.WriteLine("General Tuning Profile Request Info: " + abc);
                // { "EqualizerProfile":{ "ExpandEqualizer":{ "Mood":1},"BaseEqualizer":{ "SubBass":0,"Bass":0,"Low":0,"LowMid":0,"Mid":10,"Presence":0,"HighMid":0,"Treble":0,"Air":0} },"BackgroundMergeProfile":null,"AITuningProfile":null}

                var generalTuningProfile = new GeneralTuningProfile
                {
                    EqualizerProfile = generalTuningProfileRequestInfo.EqualizerProfile != null ? new EqualizerProfile
                    {
                        BaseEqualizer = generalTuningProfileRequestInfo.EqualizerProfile.BaseEqualizer != null ? new BaseEqualizerProfile
                        {
                            Bass = generalTuningProfileRequestInfo.EqualizerProfile.BaseEqualizer?.Bass,
                            Air = generalTuningProfileRequestInfo.EqualizerProfile.BaseEqualizer?.Air,
                            HighMid = generalTuningProfileRequestInfo.EqualizerProfile.BaseEqualizer?.HighMid,
                            LowMid = generalTuningProfileRequestInfo.EqualizerProfile.BaseEqualizer?.LowMid,
                            Presence = generalTuningProfileRequestInfo.EqualizerProfile.BaseEqualizer?.Presence,
                            Treble = generalTuningProfileRequestInfo.EqualizerProfile.BaseEqualizer?.Treble,
                            Low = generalTuningProfileRequestInfo.EqualizerProfile.BaseEqualizer?.Low,
                            Mid = generalTuningProfileRequestInfo.EqualizerProfile.BaseEqualizer?.Mid,
                            SubBass = generalTuningProfileRequestInfo.EqualizerProfile.BaseEqualizer?.SubBass

                        } : null,
                        ExpandEqualizer = generalTuningProfileRequestInfo.EqualizerProfile.ExpandEqualizer != null ? new ExpandEqualizerProfile
                        {
                            Mood = generalTuningProfileRequestInfo.EqualizerProfile.ExpandEqualizer?.Mood
                        } : null,
                    } : null,
                    AITuningProfile = generalTuningProfileRequestInfo.AITuningProfile != null ? new AITuningProfile
                    {
                        UvrMdxNetMainProfile = generalTuningProfileRequestInfo.AITuningProfile.UvrMdxNetMainProfile != null ? new UvrMdxNetMainProfile
                        {
                            BackgroundSoundGainDb = generalTuningProfileRequestInfo.AITuningProfile.UvrMdxNetMainProfile?.BackgroundSoundGainDb,
                            VoiceGainDb = generalTuningProfileRequestInfo.AITuningProfile.UvrMdxNetMainProfile?.VoiceGainDb
                        } : null
                    } : null,
                    BackgroundMergeProfile = generalTuningProfileRequestInfo.BackgroundMergeProfile != null ? new BackgroundMergeProfile
                    {
                        VolumeGainDb = generalTuningProfileRequestInfo.BackgroundMergeProfile?.VolumeGainDb,
                        FileStream = generalTuningProfileRequestInfo.BackgroundMergeProfile?.BackgroundSoundTrackFileKey != null ? await _fileIOHelper.GetFileStreamAsync(generalTuningProfileRequestInfo.BackgroundMergeProfile?.BackgroundSoundTrackFileKey) : null
                    } : null,
                    MultipleTimeRangeBackgroundMergeProfile = generalTuningProfileRequestInfo.MultipleTimeRangeBackgroundMergeProfile != null ? new MultipleTimeRangeBackgroundMergeProfile
                    {
                        TimeRangeMergeBackgrounds = generalTuningProfileRequestInfo.MultipleTimeRangeBackgroundMergeProfile?.TimeRangeMergeBackgrounds != null
                            ? (await Task.WhenAll(
                                generalTuningProfileRequestInfo.MultipleTimeRangeBackgroundMergeProfile.TimeRangeMergeBackgrounds.Select(async trbm => new TimeRangeMergeBackground
                                {
                                    BackgroundCutEndSecond = trbm.BackgroundCutEndSecond,
                                    BackgroundCutStartSecond = trbm.BackgroundCutStartSecond,
                                    OriginalMergeEndSecond = trbm.OriginalMergeEndSecond,
                                    OriginalMergeStartSecond = trbm.OriginalMergeStartSecond,
                                    VolumeGainDb = trbm.VolumeGainDb,
                                    FileStream = trbm.BackgroundSoundTrackFileKey != null
                                        ? await _fileIOHelper.GetFileStreamAsync(trbm.BackgroundSoundTrackFileKey)
                                        : null
                                })
                            )).ToList()
                            : null
                    } : null
                };

                Stream tunedAudioStream = await _audioTuningService.ProcessTuningAsync(audioFile, generalTuningProfile);

                if (tunedAudioStream != null && tunedAudioStream.CanSeek)
                {
                    tunedAudioStream.Position = 0;
                }

                return tunedAudioStream;


            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.Message + "\n");
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get episode audio general tuning settings failed, error: " + ex.Message);
            }
        }



        /////////////////////////////////////////////////////////////
        public async Task<List<PodcastEpisodeLicenseTypeDTO>> GetPodcastEpisodeLicenseTypesAsync()
        {
            try
            {
                var episodeLicenseTypes = await _podcastEpisodeLicenseTypeGenericRepository.FindAll().ToListAsync();
                var episodeLicenseTypeList = episodeLicenseTypes.Select(pelt => new PodcastEpisodeLicenseTypeDTO
                {
                    Id = pelt.Id,
                    Name = pelt.Name,
                }).ToList();

                return episodeLicenseTypeList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get podcast episode license types failed, error: " + ex.Message);
            }
        }
        public async Task<List<PodcastEpisodeLicenseListItemResponseDTO>> GetEpisodeLicensesByIdAsync(Guid episodeId, AccountStatusCache requestingAccount)
        {
            try
            {
                var existingEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(
                    id: episodeId,
                    includeFunc: q => q
                        .Include(pe => pe.PodcastShow)
                );
                if (existingEpisode == null)
                {
                    throw new Exception("Podcast episode with id " + episodeId + " does not exist");
                }
                else if (existingEpisode.DeletedAt != null)
                {
                    throw new Exception("Podcast episode with id " + episodeId + " has been deleted");
                }

                if (existingEpisode.PodcastShow == null)
                {
                    throw new Exception("Podcast show with id " + existingEpisode.PodcastShowId + " does not exist");
                }
                else if (existingEpisode.PodcastShow.DeletedAt != null)
                {
                    throw new Exception("Podcast show with id " + existingEpisode.PodcastShowId + " has been deleted");
                }
                else if (requestingAccount.RoleId == (int)RoleEnum.Customer && existingEpisode.PodcastShow.PodcasterId != requestingAccount.Id)
                {
                    throw new Exception("Podcast show with id " + existingEpisode.PodcastShowId + " does not belong to podcaster with id " + requestingAccount.Id);
                }

                var episodeLicenses = await _podcastEpisodeLicenseGenericRepository.FindAll(
                    predicate: pel => pel.PodcastEpisodeId == episodeId,
                    includeFunc: q => q
                        .Include(pel => pel.PodcastEpisodeLicenseType)
                ).ToListAsync();

                var episodeLicenseList = episodeLicenses.Select(pel => new PodcastEpisodeLicenseListItemResponseDTO
                {
                    Id = pel.Id,
                    PodcastEpisodeId = pel.PodcastEpisodeId,
                    PodcastEpisodeLicenseType = new PodcastEpisodeLicenseTypeDTO
                    {
                        Id = pel.PodcastEpisodeLicenseType.Id,
                        Name = pel.PodcastEpisodeLicenseType.Name,
                    },
                    LicenseDocumentFileKey = pel.LicenseDocumentFileKey,
                    CreatedAt = pel.CreatedAt
                }).ToList();

                return episodeLicenseList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get episode licenses by id failed, error: " + ex.Message);
            }
        }

        public async Task<List<EpisodeListItemResponseDTO>> GetMySavedEpisodesAsync(int accountId)
        {
            try
            {
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
            {
                new BatchQueryItem
                {
                    Key = "savedEpisodes",
                    QueryType = "findall",
                    EntityType = "AccountSavedPodcastEpisode",
                    Parameters = JObject.FromObject(new
                    {
                        where = new
                        {
                            AccountId = accountId
                        },
                        orderBy = "CreatedAt"
                    }),
                    Fields = new[] { "AccountId", "PodcastEpisodeId", "CreatedAt" }
                }
            }
                };

                var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
                var savedEpisodes = ((JArray)result.Results["savedEpisodes"])
                    .ToObject<List<AccountSavedPodcastEpisodeDTO>>();
                var savedEpisodeIds = savedEpisodes.Select(se => se.PodcastEpisodeId).ToList();
                // truy vấn các episode và trả về 
                var episodeList = await _podcastEpisodeGenericRepository.FindAll(
                    predicate: pe => pe.DeletedAt == null
                    && pe.PodcastShow.DeletedAt == null
                    && (pe.PodcastShow.PodcastChannel == null || pe.PodcastShow.PodcastChannel.DeletedAt == null)
                    && savedEpisodeIds.Contains(pe.Id),
                    includeFunc: q => q
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                        .ThenInclude(pet => pet.PodcastEpisodeStatus)
                        .Include(pe => pe.PodcastEpisodeHashtags)
                        .ThenInclude(peh => peh.Hashtag)
                        .Include(pe => pe.PodcastEpisodeSubscriptionType)
                        .Include(pe => pe.PodcastShow)
                ).ToListAsync();

                // var shows = (await Task.WhenAll(showList.Select(async ps =>
                // {
                //     var podcaster = await _accountCachingService.GetAccountStatusCacheById(ps.PodcasterId);
                //     if (podcaster == null || podcaster.Id != ps.PodcasterId || podcaster.IsVerified == false || podcaster.HasVerifiedPodcasterProfile == false)
                //     {
                //         throw new Exception("Podcaster with id " + ps.PodcasterId + " does not exist");
                //     }
                //     return new ShowListItemResponseDTO
                //     {
                //         Id = ps.Id,
                //         Name = ps.Name,
                //         Description = ps.Description,
                //         MainImageFileKey = ps.MainImageFileKey,
                //         TrailerAudioFileKey = ps.TrailerAudioFileKey,
                //         TotalFollow = ps.TotalFollow,
                //         ListenCount = ps.ListenCount,
                //         AverageRating = ps.AverageRating,
                //         RatingCount = ps.RatingCount,
                //         Copyright = ps.Copyright,
                //         IsReleased = ps.IsReleased,
                //         Language = ps.Language,
                //         UploadFrequency = ps.UploadFrequency,
                //         ReleaseDate = ps.ReleaseDate,
                //         TakenDownReason = null,
                //         EpisodeCount = ps.PodcastEpisodes.Count(pe =>
                //         {
                //             return pe.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published && pe.DeletedAt == null;

                //         }),
                //         PodcastCategory = ps.PodcastCategory != null ? new PodcastCategoryDTO
                //         {
                //             Id = ps.PodcastCategory.Id,
                //             Name = ps.PodcastCategory.Name,
                //             MainImageFileKey = ps.PodcastCategory.MainImageFileKey
                //         } : null,
                //         PodcastSubCategory = ps.PodcastSubCategory != null ? new PodcastSubCategoryDTO
                //         {
                //             Id = ps.PodcastSubCategory.Id,
                //             Name = ps.PodcastSubCategory.Name,
                //             PodcastCategoryId = ps.PodcastSubCategory.PodcastCategoryId
                //         } : null,
                //         PodcastChannel = ps.PodcastChannel != null ? new PodcastChannelSnippetResponseDTO
                //         {
                //             Id = ps.PodcastChannel.Id,
                //             Name = ps.PodcastChannel.Name,
                //             Description = ps.PodcastChannel.Description,
                //             MainImageFileKey = ps.PodcastChannel.MainImageFileKey
                //         } : null,
                //         Podcaster = new AccountSnippetResponseDTO
                //         {
                //             Id = podcaster.Id,
                //             Email = podcaster.Email,
                //             FullName = podcaster.PodcasterProfileName,
                //             MainImageFileKey = podcaster.MainImageFileKey
                //         },
                //         PodcastShowSubscriptionType = ps.PodcastShowSubscriptionType != null ? new PodcastShowSubscriptionTypeDTO
                //         {
                //             Id = ps.PodcastShowSubscriptionType.Id,
                //             Name = ps.PodcastShowSubscriptionType.Name
                //         } : null,
                //         Hashtags = ps.PodcastShowHashtags.Select(psh => new HashtagDTO
                //         {
                //             Id = psh.Hashtag.Id,
                //             Name = psh.Hashtag.Name
                //         }).ToList(),
                //         CreatedAt = ps.CreatedAt,
                //         UpdatedAt = ps.UpdatedAt,
                //         CurrentStatus = ps.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).Select(pst => new PodcastShowStatusDTO
                //         {
                //             Id = pst.PodcastShowStatus.Id,
                //             Name = pst.PodcastShowStatus.Name
                //         }).FirstOrDefault()!,
                //     };
                // }))).ToList();

                var episodes = episodeList.Select(pe => new EpisodeListItemResponseDTO
                {
                    Id = pe.Id,
                    Name = pe.Name,
                    Description = pe.Description,
                    AudioFileKey = pe.AudioFileKey,
                    AudioLength = pe.AudioLength,
                    ReleaseDate = pe.ReleaseDate,
                    IsReleased = pe.IsReleased,
                    AudioFileSize = pe.AudioFileSize,
                    EpisodeOrder = pe.EpisodeOrder,
                    ExplicitContent = pe.ExplicitContent,
                    IsAudioPublishable = pe.IsAudioPublishable,
                    ListenCount = pe.ListenCount,
                    MainImageFileKey = pe.MainImageFileKey,
                    SeasonNumber = pe.SeasonNumber,
                    TakenDownReason = null,
                    TotalSave = pe.TotalSave,
                    Hashtags = pe.PodcastEpisodeHashtags.Select(peh => new HashtagDTO
                    {
                        Id = peh.Hashtag.Id,
                        Name = peh.Hashtag.Name
                    }).ToList(),
                    PodcastShow = new PodcastShowSnippetResponseDTO
                    {
                        Id = pe.PodcastShow.Id,
                        Name = pe.PodcastShow.Name,
                        Description = pe.PodcastShow.Description,
                        MainImageFileKey = pe.PodcastShow.MainImageFileKey,
                        IsReleased = pe.PodcastShow.IsReleased,
                        ReleaseDate = pe.PodcastShow.ReleaseDate
                    },
                    PodcastEpisodeSubscriptionType = pe.PodcastEpisodeSubscriptionType != null ? new PodcastEpisodeSubscriptionTypeDTO
                    {
                        Id = pe.PodcastEpisodeSubscriptionType.Id,
                        Name = pe.PodcastEpisodeSubscriptionType.Name
                    } : null,
                    CreatedAt = pe.CreatedAt,
                    UpdatedAt = pe.UpdatedAt,
                    CurrentStatus = pe.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).Select(pet => new PodcastEpisodeStatusDTO
                    {
                        Id = pet.PodcastEpisodeStatus.Id,
                        Name = pet.PodcastEpisodeStatus.Name
                    }).FirstOrDefault()!,
                }).ToList();

                return episodes;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get my saved episodes failed, error: " + ex.Message);
            }
        }

        public async Task<List<PodcastEpisodeSnippetResponseDTO>> GetDmcaAssignableEpisodesAsync()
        {
            try
            {
                var episodes = await _podcastEpisodeGenericRepository.FindAll(
                    predicate: pe => pe.DeletedAt == null
                    && pe.PodcastShow.DeletedAt == null
                    && (pe.PodcastShow.PodcastChannel == null || pe.PodcastShow.PodcastChannel.DeletedAt == null),
                    includeFunc: q => q
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastChannel)
                        .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                ).ToListAsync();

                // lấy ra các episode phải thuộc published/takendown, và show phải thuộc publish/takendown , và channel phải published nếu có
                episodes = episodes.Where(pe =>
                {
                    var episodeStatusId = pe.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId;
                    if (episodeStatusId != (int)PodcastEpisodeStatusEnum.Published && episodeStatusId != (int)PodcastEpisodeStatusEnum.TakenDown)
                    {
                        return false;
                    }

                    var showStatusId = pe.PodcastShow.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId;
                    if (showStatusId != (int)PodcastShowStatusEnum.Published && showStatusId != (int)PodcastShowStatusEnum.TakenDown)
                    {
                        return false;
                    }

                    if (pe.PodcastShow.PodcastChannel != null)
                    {
                        var channelStatusId = pe.PodcastShow.PodcastChannel.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).FirstOrDefault().PodcastChannelStatusId;
                        if (channelStatusId != (int)PodcastChannelStatusEnum.Published)
                        {
                            return false;
                        }
                    }

                    return true;
                }).ToList();

                var episodeSnippets = episodes.Select(pe => new PodcastEpisodeSnippetResponseDTO
                {
                    Id = pe.Id,
                    Name = pe.Name,
                    AudioLength = pe.AudioLength,
                    Description = pe.Description,
                    IsReleased = pe.IsReleased,
                    ReleaseDate = pe.ReleaseDate,
                    MainImageFileKey = pe.MainImageFileKey
                }).ToList();

                return episodeSnippets;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get DMCA assignable episodes failed, error: " + ex.Message);
            }
        }

        public async Task<EpisodeDetailResponseDTO> GetEpisodeByIdAsync(Guid episodeId, AccountStatusCache? requestedAccount)
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

                if (requestedAccount == null || requestedAccount.RoleId == null || requestedAccount.RoleId == (int)RoleEnum.Customer)
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

                bool? IsSavedByCurrentUser = null;
                if (requestedAccount != null && requestedAccount.RoleId != null && requestedAccount.RoleId == (int)RoleEnum.Customer)
                {
                    var batchRequest = new BatchQueryRequest
                    {
                        Queries = new List<BatchQueryItem>
                        {
                            new BatchQueryItem
                            {
                                Key = "savedEpisode",
                                QueryType = "findall",
                                EntityType = "AccountSavedPodcastEpisode",

                                Parameters = JObject.FromObject(new
                                {
                                    where = new
                                    {
                                        AccountId = requestedAccount.Id,
                                        PodcastEpisodeId = episode.Id
                                    }
                                }),
                            }
                        }
                    };
                    var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
                    var savedEpisode = ((JArray)result.Results["savedEpisode"])
                        .ToObject<List<AccountSavedPodcastEpisodeDTO>>()
                        .FirstOrDefault();
                    IsSavedByCurrentUser = savedEpisode != null;
                }

                var episodeDetail = new EpisodeDetailResponseDTO
                {
                    Id = episode.Id,
                    Name = episode.Name,
                    Description = episode.Description,
                    IsSavedByCurrentUser = IsSavedByCurrentUser,
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
                    TakenDownReason = requestedAccount == null || requestedAccount.RoleId == null || requestedAccount.RoleId == (int)RoleEnum.Customer ? null : episode.TakenDownReason,
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
                        Description = episode.PodcastShow.Description,
                        MainImageFileKey = episode.PodcastShow.MainImageFileKey,
                        IsReleased = episode.PodcastShow.IsReleased,
                        ReleaseDate = episode.PodcastShow.ReleaseDate
                    },
                    Podcaster = new AccountSnippetResponseDTO
                    {
                        Id = podcaster.Id,
                        Email = podcaster.Email,
                        FullName = podcaster.PodcasterProfileName,
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
                    predicate: pe => pe.DeletedAt == null && pe.Id == episodeId && pe.PodcastShow.DeletedAt == null && pe.PodcastShow.PodcasterId == podcasterId && (pe.PodcastShow.PodcastChannel == null || pe.PodcastShow.PodcastChannel.DeletedAt == null),
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
                    IsSavedByCurrentUser = null,
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
                        Description = episode.PodcastShow.Description,
                        MainImageFileKey = episode.PodcastShow.MainImageFileKey,
                        IsReleased = episode.PodcastShow.IsReleased,
                        ReleaseDate = episode.PodcastShow.ReleaseDate
                    },
                    Podcaster = new AccountSnippetResponseDTO
                    {
                        Id = podcaster.Id,
                        Email = podcaster.Email,
                        FullName = podcaster.PodcasterProfileName,
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
                            fileNameWithoutExtension = fileNameWithoutExtension.Trim().Split("_")[2];
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
                    //      + IsAudioPublishable = null
                    // 		+ xoá audio cũ (bao gồm cả playlist nếu có)
                    // 		+ Update: AudioFileKey, AudioFileSize, AudioLength
                    // 		+ Lưu audio mới
                    // 		+ Reset: AudioFingerPrint = null, AudioTranscript = null
                    // 	+ PENDING EDIT REQUIRED:
                    //      + IsAudioPublishable = null
                    //  	+ xoá audio cũ (bao gồm cả playlist nếu có)
                    //  	+ Update: AudioFileKey, AudioFileSize, AudioLength
                    //  	+ Lưu audio mới
                    //  	+ Reset: AudioFingerPrint = null, AudioTranscript = null
                    // 		+ Chuyển episode status → Audio Processing (processing để cập nhật (AudioFingerPrint/AudioTranscript) xong sẽ chuyển qua Pending Review)
                    // 		+ Update review session status → Pending Review (Tăng reReviewCount += 1)
                    // 	+ READY TO RELEASE:
                    // 		+ xoá audio cũ (bao gồm cả playlist nếu có)
                    //  	+ Update: AudioFileKey, AudioFileSize, AudioLength
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
                        existingPodcastEpisode.IsAudioPublishable = null;
                        await _podcastEpisodeGenericRepository.UpdateAsync(existingPodcastEpisode.Id, existingPodcastEpisode);
                        await transaction.CommitAsync();

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
                        existingPodcastEpisode.IsAudioPublishable = null;

                        // Change episode status to Audio Processing
                        var newStatusTracking = new PodcastEpisodeStatusTracking
                        {
                            PodcastEpisodeId = existingPodcastEpisode.Id,
                            PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.AudioProcessing
                        };
                        await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);
                        await _podcastEpisodeGenericRepository.UpdateAsync(existingPodcastEpisode.Id, existingPodcastEpisode);

                        JObject requestData = JObject.FromObject(
                            new
                            {
                                PodcastEpisodeId = existingPodcastEpisode.Id,
                                PodcasterId = existingPodcaster.Id,
                            }
                        );

                        await transaction.CommitAsync();

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
                        await _podcastEpisodeGenericRepository.UpdateAsync(existingPodcastEpisode.Id, existingPodcastEpisode);
                        await transaction.CommitAsync();

                    }



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

                    Console.WriteLine("Start processing audio for episode id: " + existingPodcastEpisode.Id + ", audio file key: " + existingPodcastEpisode.AudioFileKey);

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
                        var transcriptionTask = _audioTranscriptionService.TranscribeAudioAsync(transcriptionStreamCopy,
                        // lấy filename = "audio" + extension từ AudioFileKey
                        // FilePathHelper.GetFileName(existingPodcastEpisode.AudioFileKey)
                        "audio" + FilePathHelper.GetExtension(existingPodcastEpisode.AudioFileKey)
                        );
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
                        // lọc các episode đã published hoặc taken down, khác episode hiện tại, khác podcaster
                        var publishedEpisode = await _podcastEpisodeGenericRepository.FindAll(
                            predicate: pe => pe.Id != existingPodcastEpisode.Id &&
                                pe.AudioFingerPrint != null &&
                                pe.DeletedAt == null &&
                                pe.PodcastShow.PodcasterId != existingPodcastEpisode.PodcastShow.PodcasterId &&
                                pe.PodcastShow.DeletedAt == null &&
                                (pe.PodcastShow.PodcastChannel == null || pe.PodcastShow.PodcastChannel.DeletedAt == null)
                                , // không so sánh với episode của cùng podcaster
                                  // (pe.PodcastEpisodeStatusTrackings
                                  //     .OrderByDescending(pet => pet.CreatedAt)
                                  //     .FirstOrDefault()
                                  //     .PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published ||
                                  // pe.PodcastEpisodeStatusTrackings
                                  //     .OrderByDescending(pet => pet.CreatedAt)
                                  //     .FirstOrDefault()
                                  //     .PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.TakenDown),

                            includeFunc: q => q.Include(pe => pe.PodcastEpisodeStatusTrackings)
                            .Include(pe => pe.PodcastShow)
                            .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                            .Include(pe => pe.PodcastShow)
                            .ThenInclude(ps => ps.PodcastChannel)
                            .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                        ).ToListAsync();

                        publishedEpisode = publishedEpisode.Where(pe =>
                        {
                            // var latestEpisodeStatusId = pe.PodcastEpisodeStatusTrackings
                            //     .OrderByDescending(pet => pet.CreatedAt)
                            //     .FirstOrDefault()
                            //     .PodcastEpisodeStatusId;
                            // return latestStatusId == (int)PodcastEpisodeStatusEnum.Published ||
                            //        latestStatusId == (int)PodcastEpisodeStatusEnum.TakenDown;
                            var episodeCurrentStatus = pe.PodcastEpisodeStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastEpisodeStatusId;

                            var showCurrentStatus = pe.PodcastShow.PodcastShowStatusTrackings
                                .OrderByDescending(t => t.CreatedAt)
                                .FirstOrDefault()?.PodcastShowStatusId;

                            var channelCurrentStatus = pe.PodcastShow.PodcastChannel == null ? null : pe.PodcastShow.PodcastChannel.PodcastChannelStatusTrackings
                                .OrderByDescending(t => t.CreatedAt)
                                .FirstOrDefault()?.PodcastChannelStatusId;

                            return episodeCurrentStatus == (int)PodcastEpisodeStatusEnum.Published &&
                                   showCurrentStatus == (int)PodcastShowStatusEnum.Published &&
                                    (channelCurrentStatus == null || channelCurrentStatus == (int)PodcastChannelStatusEnum.Published);
                        }).ToList();

                        AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparison comparison = new AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparison
                        {
                            Target = null,
                            Candidates = new List<AcoustIDAudioFingerprintComparisonObject>()
                        };
                        AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparisonPercentageResult comparisonResult = new AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparisonPercentageResult
                        {
                            results = new List<AcoustIDAudioFingerprintSimilarityPercentageResult>()
                        };
                        if (publishedEpisode.Count > 0)
                        {
                            foreach (var pe in publishedEpisode)
                            {
                                Console.WriteLine("Comparing with published episode id: " + pe.Id);
                            }
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
                            .Where(res => res.SimilarityPercentage / 100 >= _podcastPublishReviewSessionConfig.MinDuplicateSimilarityRate)
                            .Select(res => Guid.Parse(res.Id.ToString()))
                            .ToList();

                        foreach (var dupId in duplicateEpisodeIds)
                        {
                            Console.WriteLine("Detected duplicate episode id: " + dupId);
                        }

                        // Đếm số term bị vi phạm so với HighTranscriptionMinRestrictedTermCount
                        bool isViolatedTermExceeded = detectedRestrictTerms.Count >= _podcastPublishReviewSessionConfig.HighTranscriptionMinRestrictedTermCount || (detectedRestrictTerms.Count >= _podcastPublishReviewSessionConfig.MediumTranscriptionMinRestrictedTermCount && existingPodcastEpisode.ExplicitContent == false);

                        existingPodcastEpisode.AudioTranscript = transcript != null ? transcript : "";
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
                            predicate: pers => pers.PodcastEpisodeId == existingPodcastEpisode.Id,
                            includeFunc: q => q.Include(pers => pers.PodcastEpisodePublishReviewSessionStatusTrackings)
                        ).FirstOrDefaultAsync();

                        var latestPendingReviewSessionStatusTracking = pendingReviewSession != null
                            ? pendingReviewSession.PodcastEpisodePublishReviewSessionStatusTrackings
                                .OrderByDescending(persst => persst.CreatedAt)
                                .FirstOrDefault()
                            : null;

                        pendingReviewSession = (latestPendingReviewSessionStatusTracking != null && latestPendingReviewSessionStatusTracking.PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview)
                            ? pendingReviewSession
                            : null;

                        if (pendingReviewSession != null)
                        {
                            // CỘNG 1 VÀO PRENDING REVIEW
                            pendingReviewSession.ReReviewCount += 1;
                            pendingReviewSession.Deadline = null; // reset deadline khi có re-review

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
                        else if (pendingReviewSession == null && (isViolatedTermExceeded || duplicateEpisodeIds.Count > 0))
                        {
                            // 			+ KHÔNG CÓ && THOẢ 1 TRONG 2 ĐIỀU KIỀN VI PHẠM (RESTRICT TERM / DUPLICATION):
                            // 				+ tạo 1 publish review session

                            // Lọc ra các staff id đã được assign vào các phiên review chưa hoàn thành
                            // từ đó so với danh sách available staff truy vấn được từ Userservice để group lại các staff ít được assign nhất, nếu danh sách > 1 thì random chọn
                            var podcastEpisodePublishReviewSessions = await _podcastEpisodePublishReviewSessionGenericRepository.FindAll(
                                predicate: null,
                                includeFunc: q => q.Include(pprs => pprs.PodcastEpisodePublishReviewSessionStatusTrackings)
                            ).ToListAsync();
                            var assignedStaffIds = podcastEpisodePublishReviewSessions
                                .Where(pprs =>
                                {
                                    var latestStatusTracking = pprs.PodcastEpisodePublishReviewSessionStatusTrackings
                                        .OrderByDescending(persst => persst.CreatedAt)
                                        .FirstOrDefault();
                                    return latestStatusTracking != null && latestStatusTracking.PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview;
                                })
                                .Select(pprs => pprs.AssignedStaff)
                                .ToList();


                            List<AccountDTO> availableStaff = await GetAllAvailableStaffs();
                            Dictionary<int, int> staffAssignmentCount = new Dictionary<int, int>();
                            foreach (var staff in availableStaff)
                            {
                                int count = assignedStaffIds.Count(id => id == staff.Id);
                                staffAssignmentCount[staff.Id] = count;
                            }

                            int minAssignmentCount = staffAssignmentCount.Values.Min();
                            List<int> leastAssignedStaffIds = staffAssignmentCount
                                .Where(kvp => kvp.Value == minAssignmentCount)
                                .Select(kvp => kvp.Key)
                                .ToList();
                            Random rand = new Random();
                            int randomIndex = rand.Next(leastAssignedStaffIds.Count);
                            int chosenStaffId = leastAssignedStaffIds[randomIndex];




                            var newPublishReviewSession = new PodcastEpisodePublishReviewSession
                            {
                                PodcastEpisodeId = existingPodcastEpisode.Id,
                                AssignedStaff = chosenStaffId,
                                ReReviewCount = 0,
                            };
                            await _podcastEpisodePublishReviewSessionGenericRepository.CreateAsync(newPublishReviewSession);
                            var newPublishReviewSessionStatusTracking = new PodcastEpisodePublishReviewSessionStatusTracking
                            {
                                PodcastEpisodePublishReviewSessionId = newPublishReviewSession.Id,
                                PodcastEpisodePublishReviewSessionStatusId = (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview
                            };
                            await _podcastEpisodePublishReviewSessionStatusTrackingGenericRepository.CreateAsync(newPublishReviewSessionStatusTracking);

                            if (isViolatedTermExceeded == true)
                            {
                                var illegalContentTypeMarking = new PodcastEpisodeIllegalContentTypeMarking
                                {
                                    PodcastEpisodeId = existingPodcastEpisode.Id,
                                    PodcastIllegalContentTypeId = (int)PodcastIllegalContentTypeEnum.ExcessiveRestrictedTerms
                                };
                                await _podcastEpisodeIllegalContentTypeMarkingGenericRepository.CreateAsync(illegalContentTypeMarking);
                            }

                            // 

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
                                var illegalContentTypeMarking = new PodcastEpisodeIllegalContentTypeMarking
                                {
                                    PodcastEpisodeId = existingPodcastEpisode.Id,
                                    PodcastIllegalContentTypeId = (int)PodcastIllegalContentTypeEnum.DuplicateContent
                                };
                                await _podcastEpisodeIllegalContentTypeMarkingGenericRepository.CreateAsync(illegalContentTypeMarking);
                            }

                            var episodeNewStatusTracking = new PodcastEpisodeStatusTracking
                            {
                                PodcastEpisodeId = existingPodcastEpisode.Id,
                                PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.PendingReview
                            };
                            await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(episodeNewStatusTracking);

                        }
                        else if (pendingReviewSession == null && !isViolatedTermExceeded && duplicateEpisodeIds.Count == 0) // phục vụ tình huống gọi request publish
                        {
                            // 			+ Không có && không thoả điều kiện vi phạm nào:
                            // 				+ chuyển trạng thái của episode sang Ready to release và set isAudioPublishable = true
                            existingPodcastEpisode.IsAudioPublishable = true;
                            await _podcastEpisodeGenericRepository.UpdateAsync(existingPodcastEpisode.Id, existingPodcastEpisode);



                            var episodeNewStatusTracking = new PodcastEpisodeStatusTracking
                            {
                                PodcastEpisodeId = existingPodcastEpisode.Id,
                                PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.ReadyToRelease
                            };
                            await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(episodeNewStatusTracking);

                        }
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

                    await _podcastContentNotificationHubContext.Clients.User(processingEpisodeDraftAudioParameterDTO.PodcasterId.ToString()).SendAsync(
                        "PodcastEpisodeAudioProcessingCompletedNotification",
                        JObject.FromObject(new
                        {
                            IsSuccess = true,
                            ErrorMessage = (string?)null,
                        })
                    );
                    Console.WriteLine("\n\n\nPodcast episode audio processing completed successfully for episode id: " + existingPodcastEpisode.Id + "\n\n\n");
                    // in người nhận
                    Console.WriteLine("Notification sent to podcaster id: " + processingEpisodeDraftAudioParameterDTO.PodcasterId);
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
                    await _podcastContentNotificationHubContext.Clients.User(processingEpisodeDraftAudioParameterDTO.PodcasterId.ToString()).SendAsync(
                        "PodcastEpisodeAudioProcessingCompletedNotification",
                        JObject.FromObject(new
                        {
                            IsSuccess = false,
                            ErrorMessage = ex.Message,
                        })
                    );
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RequestPodcastEpisodeAudioExamination(RequestEpisodeAudioExaminationParameterDTO requestEpisodeAudioExaminationParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Gửi yêu cầu kiểm duyệt audio cho staff
                    var existingPodcastEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(requestEpisodeAudioExaminationParameterDTO.PodcastEpisodeId,
                        includeFunc: q => q
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastChannel)
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                    );
                    if (existingPodcastEpisode == null)
                    {
                        throw new Exception("Podcast episode with id " + requestEpisodeAudioExaminationParameterDTO.PodcastEpisodeId + " does not exist");
                    }
                    else if (existingPodcastEpisode.DeletedAt != null)
                    {
                        throw new Exception("Podcast episode with id " + requestEpisodeAudioExaminationParameterDTO.PodcastEpisodeId + " has been deleted");
                    }
                    else if (existingPodcastEpisode.PodcastEpisodeStatusTrackings
                        .OrderByDescending(pet => pet.CreatedAt)
                        .FirstOrDefault()
                        .PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Draft)
                    {
                        throw new Exception("Podcast episode with id " + requestEpisodeAudioExaminationParameterDTO.PodcastEpisodeId + " is not in Draft status");
                    }
                    else if (existingPodcastEpisode.IsAudioPublishable == false)
                    {
                        throw new Exception("Podcast episode with id " + requestEpisodeAudioExaminationParameterDTO.PodcastEpisodeId + " is marked as non-publishable due to audio content issues, please update a new audio file");
                    }



                    if (existingPodcastEpisode.PodcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " does not exist");
                    }
                    else if (existingPodcastEpisode.PodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been deleted");
                    }
                    else if (existingPodcastEpisode.PodcastShow.PodcasterId != requestEpisodeAudioExaminationParameterDTO.PodcasterId)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " does not belong to podcaster with id " + requestEpisodeAudioExaminationParameterDTO.PodcasterId);
                    }
                    else if (existingPodcastEpisode.PodcastShow.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnum.Removed)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been removed");
                    }

                    if (existingPodcastEpisode.PodcastShow.PodcastChannel != null)
                    {
                        if (existingPodcastEpisode.PodcastShow.PodcastChannel.DeletedAt != null)
                        {
                            throw new Exception("Podcast channel with id " + existingPodcastEpisode.PodcastShow.PodcastChannelId + " has been deleted");
                        }
                    }

                    var existingPodcaster = await _accountCachingService.GetAccountStatusCacheById(existingPodcastEpisode.PodcastShow.PodcasterId);
                    if (existingPodcaster == null || existingPodcaster.Id != existingPodcastEpisode.PodcastShow.PodcasterId || existingPodcaster.IsVerified == false || existingPodcaster.DeactivatedAt != null || existingPodcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + existingPodcastEpisode.PodcastShow.PodcasterId + " does not exist");
                    }

                    // Change episode status to Audio Processing
                    var newStatusTracking = new PodcastEpisodeStatusTracking
                    {
                        PodcastEpisodeId = existingPodcastEpisode.Id,
                        PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.AudioProcessing
                    };
                    await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);

                    // xoá toàn bộ các đánh dấu vi phạm nội dung cũ
                    await _unitOfWork.PodcastEpisodeIllegalContentTypeMarkingRepository.DeleteByPodcastEpisodeIdAsync(existingPodcastEpisode.Id);

                    JObject requestData = JObject.FromObject(
                            new
                            {
                                PodcastEpisodeId = existingPodcastEpisode.Id,
                                PodcasterId = existingPodcaster.Id,
                            }
                        );

                    var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "episode-draft-audio-processing-flow");
                    await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);

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
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "request-episode-audio-examination.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);


                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Request podcast episode audio examination failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "request-episode-audio-examination.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }

        }

        public async Task DiscardPodcastEpisodePublishReviewSession(DiscardEpisodePublishReviewSessionParameterDTO discardEpisodePublishReviewSessionParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Huỷ bỏ phiên kiểm duyệt publish episode
                    var existingPodcastEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(discardEpisodePublishReviewSessionParameterDTO.PodcastEpisodeId,
                        includeFunc: q => q
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastChannel)
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                    );
                    if (existingPodcastEpisode == null)
                    {
                        throw new Exception("Podcast episode with id " + discardEpisodePublishReviewSessionParameterDTO.PodcastEpisodeId + " does not exist");
                    }
                    else if (existingPodcastEpisode.DeletedAt != null)
                    {
                        throw new Exception("Podcast episode with id " + discardEpisodePublishReviewSessionParameterDTO.PodcastEpisodeId + " has been deleted");
                    }
                    else if (existingPodcastEpisode.PodcastEpisodeStatusTrackings
                        .OrderByDescending(pet => pet.CreatedAt)
                        .FirstOrDefault()
                        .PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.PendingReview)
                    {
                        throw new Exception("Podcast episode with id " + discardEpisodePublishReviewSessionParameterDTO.PodcastEpisodeId + " is not in Pending Review status");
                    }


                    if (existingPodcastEpisode.PodcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " does not exist");
                    }
                    else if (existingPodcastEpisode.PodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been deleted");
                    }
                    else if (existingPodcastEpisode.PodcastShow.PodcasterId != discardEpisodePublishReviewSessionParameterDTO.PodcasterId)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " does not belong to podcaster with id " + discardEpisodePublishReviewSessionParameterDTO.PodcasterId);
                    }
                    else if (existingPodcastEpisode.PodcastShow.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnum.Removed)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been removed");
                    }

                    if (existingPodcastEpisode.PodcastShow.PodcastChannel != null)
                    {
                        if (existingPodcastEpisode.PodcastShow.PodcastChannel.DeletedAt != null)
                        {
                            throw new Exception("Podcast channel with id " + existingPodcastEpisode.PodcastShow.PodcastChannelId + " has been deleted");
                        }
                    }

                    var existingPodcaster = await _accountCachingService.GetAccountStatusCacheById(existingPodcastEpisode.PodcastShow.PodcasterId);
                    if (existingPodcaster == null || existingPodcaster.Id != existingPodcastEpisode.PodcastShow.PodcasterId || existingPodcaster.IsVerified == false || existingPodcaster.DeactivatedAt != null || existingPodcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + existingPodcastEpisode.PodcastShow.PodcasterId + " does not exist");
                    }
                    var pendingReviewSession = await _podcastEpisodePublishReviewSessionGenericRepository.FindAll(
                        predicate: pers => pers.PodcastEpisodeId == existingPodcastEpisode.Id,
                        // pers.PodcastEpisodePublishReviewSessionStatusTrackings
                        //     .OrderByDescending(persst => persst.CreatedAt)
                        //     .FirstOrDefault()
                        //     .PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview,
                        includeFunc: q => q.Include(pers => pers.PodcastEpisodePublishReviewSessionStatusTrackings)
                    ).FirstOrDefaultAsync();

                    var latestPendingReviewSessionStatusTracking = pendingReviewSession != null
                        ? pendingReviewSession.PodcastEpisodePublishReviewSessionStatusTrackings
                            .OrderByDescending(persst => persst.CreatedAt)
                            .FirstOrDefault()
                        : null;

                    pendingReviewSession = (latestPendingReviewSessionStatusTracking != null && latestPendingReviewSessionStatusTracking.PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview)
                        ? pendingReviewSession
                        : null;


                    if (pendingReviewSession == null)
                    {
                        throw new Exception("No pending review session found for podcast episode with id " + discardEpisodePublishReviewSessionParameterDTO.PodcastEpisodeId);
                    }
                    // Chuyển trạng thái phiên review sang Discarded
                    var episodeNewStatusTracking = new PodcastEpisodePublishReviewSessionStatusTracking
                    {
                        PodcastEpisodePublishReviewSessionId = pendingReviewSession.Id,
                        PodcastEpisodePublishReviewSessionStatusId = (int)PodcastEpisodePublishReviewSessionStatusEnum.Discard
                    };
                    await _podcastEpisodePublishReviewSessionStatusTrackingGenericRepository.CreateAsync(episodeNewStatusTracking);

                    // Chuyển trạng thái episode về Draft
                    var episodeStatusTracking = new PodcastEpisodeStatusTracking
                    {
                        PodcastEpisodeId = existingPodcastEpisode.Id,
                        PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Draft
                    };
                    await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(episodeStatusTracking);

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
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "discard-episode-publish-review-session.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Discard podcast episode publish review session failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "discard-episode-publish-review-session.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");

                }
            }
        }

        public async Task PublishPodcastEpisode(PublishEpisodeParameterDTO publishEpisodeParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Publish podcast episode
                    var existingPodcastEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(publishEpisodeParameterDTO.PodcastEpisodeId,
                        includeFunc: q => q
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastChannel)
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                    );
                    if (existingPodcastEpisode == null)
                    {
                        throw new Exception("Podcast episode with id " + publishEpisodeParameterDTO.PodcastEpisodeId + " does not exist");
                    }
                    else if (existingPodcastEpisode.DeletedAt != null)
                    {
                        throw new Exception("Podcast episode with id " + publishEpisodeParameterDTO.PodcastEpisodeId + " has been deleted");
                    }
                    else if (existingPodcastEpisode.IsAudioPublishable != true)
                    {
                        throw new Exception("Podcast episode with id " + publishEpisodeParameterDTO.PodcastEpisodeId + " is marked as non-publishable due to audio content issues, please update a new audio file");
                    }
                    else if (existingPodcastEpisode.PodcastEpisodeStatusTrackings
                        .OrderByDescending(pet => pet.CreatedAt)
                        .FirstOrDefault()
                        .PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.ReadyToRelease)
                    {
                        throw new Exception("Podcast episode with id " + publishEpisodeParameterDTO.PodcastEpisodeId + " is not in Ready to Release status");
                    }

                    if (existingPodcastEpisode.PodcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " does not exist");
                    }
                    else if (existingPodcastEpisode.PodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been deleted");
                    }
                    else if (existingPodcastEpisode.PodcastShow.PodcasterId != publishEpisodeParameterDTO.PodcasterId)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " does not belong to podcaster with id " + publishEpisodeParameterDTO.PodcasterId);
                    }
                    else if (existingPodcastEpisode.PodcastShow.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnum.Removed)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been removed");
                    }

                    if (existingPodcastEpisode.PodcastShow.PodcastChannel != null)
                    {
                        if (existingPodcastEpisode.PodcastShow.PodcastChannel.DeletedAt != null)
                        {
                            throw new Exception("Podcast channel with id " + existingPodcastEpisode.PodcastShow.PodcastChannelId + " has been deleted");
                        }
                    }

                    var existingPodcaster = await _accountCachingService.GetAccountStatusCacheById(existingPodcastEpisode.PodcastShow.PodcasterId);
                    if (existingPodcaster == null || existingPodcaster.Id != existingPodcastEpisode.PodcastShow.PodcasterId || existingPodcaster.IsVerified == false || existingPodcaster.DeactivatedAt != null || existingPodcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + existingPodcastEpisode.PodcastShow.PodcasterId + " does not exist");
                    }

                    existingPodcastEpisode.ReleaseDate = publishEpisodeParameterDTO.ReleaseDate;


                    if (publishEpisodeParameterDTO.ReleaseDate == null || publishEpisodeParameterDTO.ReleaseDate <= DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone()))
                    {
                        existingPodcastEpisode.IsReleased = true;
                    }
                    else
                    {
                        existingPodcastEpisode.IsReleased = false;
                    }
                    await _podcastEpisodeGenericRepository.UpdateAsync(existingPodcastEpisode.Id, existingPodcastEpisode);

                    // Change episode status to Published
                    var newStatusTracking = new PodcastEpisodeStatusTracking
                    {
                        PodcastEpisodeId = existingPodcastEpisode.Id,
                        PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.AudioProcessing
                    };
                    await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);

                    JObject requestData = JObject.FromObject(
                            new
                            {
                                PodcastEpisodeId = existingPodcastEpisode.Id,
                                PodcasterId = existingPodcaster.Id,
                            }
                        );

                    var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "episode-publish-audio-processing-flow");
                    await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);

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
                        messageName: "publish-episode.success"
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
                            ErrorMessage = $"Publish podcast episode failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "publish-episode.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task ProcessPodcastEpisodePublishAudio(ProcessingEpisodePublishAudioParameterDTO processingEpisodePublishAudioParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Xử lý audio publish podcast episode
                    var existingPodcastEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(processingEpisodePublishAudioParameterDTO.PodcastEpisodeId,
                        includeFunc: q => q
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastChannel)
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                    );
                    if (existingPodcastEpisode == null)
                    {
                        throw new Exception("Podcast episode with id " + processingEpisodePublishAudioParameterDTO.PodcastEpisodeId + " does not exist");
                    }
                    else if (existingPodcastEpisode.DeletedAt != null)
                    {
                        throw new Exception("Podcast episode with id " + processingEpisodePublishAudioParameterDTO.PodcastEpisodeId + " has been deleted");
                    }
                    else if (existingPodcastEpisode.PodcastEpisodeStatusTrackings
                        .OrderByDescending(pet => pet.CreatedAt)
                        .FirstOrDefault()
                        .PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.AudioProcessing)
                    {
                        throw new Exception("Podcast episode with id " + processingEpisodePublishAudioParameterDTO.PodcastEpisodeId + " is not in Audio Processing status");
                    }
                    else if (existingPodcastEpisode.AudioFileKey == null || existingPodcastEpisode.AudioFingerPrint == null || existingPodcastEpisode.AudioTranscript == null)
                    {
                        throw new Exception("Podcast episode with id " + processingEpisodePublishAudioParameterDTO.PodcastEpisodeId + " is missing audio data");
                    }

                    if (existingPodcastEpisode.PodcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " does not exist");
                    }
                    else if (existingPodcastEpisode.PodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been deleted");
                    }
                    else if (existingPodcastEpisode.PodcastShow.PodcasterId != processingEpisodePublishAudioParameterDTO.PodcasterId)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " does not belong to podcaster with id " + processingEpisodePublishAudioParameterDTO.PodcasterId);
                    }
                    else if (existingPodcastEpisode.PodcastShow.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnum.Removed)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been removed");
                    }
                    if (existingPodcastEpisode.PodcastShow.PodcastChannel != null)
                    {
                        if (existingPodcastEpisode.PodcastShow.PodcastChannel.DeletedAt != null)
                        {
                            throw new Exception("Podcast channel with id " + existingPodcastEpisode.PodcastShow.PodcastChannelId + " has been deleted");
                        }
                    }
                    var existingPodcaster = await _accountCachingService.GetAccountStatusCacheById(existingPodcastEpisode.PodcastShow.PodcasterId);
                    if (existingPodcaster == null || existingPodcaster.Id != existingPodcastEpisode.PodcastShow.PodcasterId || existingPodcaster.IsVerified == false || existingPodcaster.DeactivatedAt != null || existingPodcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + existingPodcastEpisode.PodcastShow.PodcasterId + " does not exist");
                    }


                    var stream = await _fileIOHelper.GetFileStreamAsync(existingPodcastEpisode.AudioFileKey);
                    if (stream == null)
                    {
                        throw new Exception("Could not retrieve uploaded file from storage");
                    }

                    HlsProcessingResult hlsResult = await _ffMpegCoreHlsService.ProcessAudioToHlsAsync(stream);
                    if (hlsResult.Success == false)
                    {
                        throw new Exception("HLS processing failed: " + hlsResult.ErrorMessage);
                    }

                    // xoá hết các file cũ trong thư mục playlist (nếu có)
                    var folderPath = _filePathConfig.PODCAST_EPISODE_FILE_PATH + "\\" + existingPodcastEpisode.Id;
                    var existingPlaylistFolderKey = FilePathHelper.CombinePaths(
                                folderPath,
                                "playlist"
                            );
                    await _fileIOHelper.DeleteFolderAsync(existingPlaylistFolderKey);

                    foreach (var segment in hlsResult.GeneratedFiles)
                    {
                        var segmentData = segment.FileContent;
                        await _fileIOHelper.UploadBinaryFileAsync(segmentData, existingPlaylistFolderKey, segment.FileName);
                    }

                    await _fileIOHelper.UploadBinaryFileAsync(
                        hlsResult.EncryptionKeyFile.FileContent,
                        existingPlaylistFolderKey,
                        hlsResult.EncryptionKeyFile.FileName
                    );

                    existingPodcastEpisode.AudioEncryptionKeyId = hlsResult.EncryptionKeyId;
                    existingPodcastEpisode.AudioEncryptionKeyFileKey = FilePathHelper.CombinePaths(
                        existingPlaylistFolderKey,
                        hlsResult.EncryptionKeyFile.FileName
                    );
                    await _podcastEpisodeGenericRepository.UpdateAsync(existingPodcastEpisode.Id, existingPodcastEpisode);

                    // Change episode status to Published
                    var newStatusTracking = new PodcastEpisodeStatusTracking
                    {
                        PodcastEpisodeId = existingPodcastEpisode.Id,
                        PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Published
                    };
                    await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);

                    // kiểm tra nếu status của show hiện tại là Draft thì chuyển nó thành ready to release
                    var podcastShowCurrentStatusId = existingPodcastEpisode.PodcastShow.PodcastShowStatusTrackings
                        .OrderByDescending(pst => pst.CreatedAt)
                        .Select(pst => pst.PodcastShowStatusId)
                        .FirstOrDefault();

                    if (podcastShowCurrentStatusId == (int)PodcastShowStatusEnum.Draft)
                    {
                        var podcastShowNewStatusTracking = new PodcastShowStatusTracking
                        {
                            PodcastShowId = existingPodcastEpisode.PodcastShow.Id,
                            PodcastShowStatusId = (int)PodcastShowStatusEnum.ReadyToRelease
                        };
                        await _podcastShowStatusTrackingGenericRepository.CreateAsync(podcastShowNewStatusTracking);
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
                        messageName: "processing-episode-publish-audio.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    await _podcastContentNotificationHubContext.Clients.User(processingEpisodePublishAudioParameterDTO.PodcasterId.ToString()).SendAsync(
                        "PodcastEpisodeAudioProcessingCompletedNotification",
                        JObject.FromObject(new
                        {
                            IsSuccess = true,
                            ErrorMessage = (string?)null
                        })
                    );

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Process podcast episode publish audio failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "processing-episode-publish-audio.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    await _podcastContentNotificationHubContext.Clients.User(processingEpisodePublishAudioParameterDTO.PodcasterId.ToString()).SendAsync(
                        "PodcastEpisodeAudioProcessingCompletedNotification",
                        JObject.FromObject(new
                        {
                            IsSuccess = false,
                            ErrorMessage = ex.Message
                        })
                    );
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task<PodcastEpisode> GetValidEpisodeListenPermission(Guid podcastEpisodeId)
        {
            var podcastEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(podcastEpisodeId,
                        includeFunc: q => q
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastCategory)
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastSubCategory)
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastChannel)
                        .ThenInclude(pe => pe.PodcastChannelStatusTrackings)
                    );

            if (podcastEpisode == null)
            {
                throw new Exception("Podcast episode with id " + podcastEpisodeId + " does not exist");
            }
            else if (podcastEpisode.DeletedAt != null)
            {
                throw new Exception("Podcast episode with id " + podcastEpisodeId + " has been deleted");
            }
            else if (podcastEpisode.PodcastEpisodeStatusTrackings
                .OrderByDescending(pet => pet.CreatedAt)
                .FirstOrDefault()
                .PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Published)
            {
                throw new Exception("Podcast episode with id " + podcastEpisodeId + " is not in Published status");
            }

            var podcastShow = podcastEpisode.PodcastShow;
            var podcastChannel = podcastShow.PodcastChannel != null ? podcastShow.PodcastChannel : null;

            var episodeCurrentStatusId = podcastEpisode.PodcastEpisodeStatusTrackings
                .OrderByDescending(pet => pet.CreatedAt)
                .Select(pet => pet.PodcastEpisodeStatusId)
                .FirstOrDefault();
            var showCurrentStatusId = podcastShow.PodcastShowStatusTrackings
                .OrderByDescending(pst => pst.CreatedAt)
                .Select(pst => pst.PodcastShowStatusId)
                .FirstOrDefault();
            var channelCurrentStatusId = podcastChannel != null ? podcastChannel.PodcastChannelStatusTrackings
                .OrderByDescending(pct => pct.CreatedAt)
                .Select(pct => pct.PodcastChannelStatusId)
                .FirstOrDefault() : (int?)null;

            // trạng thái của show
            if (podcastShow == null)
            {
                throw new Exception("Podcast show with id " + podcastShow.Id + " does not exist");
            }
            else if (podcastShow.DeletedAt != null)
            {
                throw new Exception("Podcast show with id " + podcastShow.Id + " has been deleted");
            }
            else if (showCurrentStatusId != (int)PodcastShowStatusEnum.Published)
            {
                throw new Exception("Podcast show with id " + podcastShow.Id + " is not in Published status");
            }

            // trạng thái của channel
            if (podcastChannel != null)
            {
                if (podcastChannel.DeletedAt != null)
                {
                    throw new Exception("Podcast channel with id " + podcastChannel.Id + " has been deleted");
                }
                else if (channelCurrentStatusId != (int)PodcastChannelStatusEnum.Published)
                {
                    throw new Exception("Podcast channel with id " + podcastChannel.Id + " is not in Published status");
                }
            }

            return podcastEpisode;
        }

        public async Task<HashSet<PodcastSubscriptionBenefitEnum>> GetEpisodeListenPermissionConditionsAsync(PodcastEpisode podcastEpisode, AccountDTO listenerAccount)
        {
            var conditions = new HashSet<PodcastSubscriptionBenefitEnum>();

            // isreleased = false (Show) : PodcastSubscriptionBenefitEnum.ShowsEpisodesEarlyAccess
            if (podcastEpisode.IsReleased == false || podcastEpisode.PodcastShow.IsReleased == false)
            {
                conditions.Add(PodcastSubscriptionBenefitEnum.ShowsEpisodesEarlyAccess);
            }

            // PodcastListenSlot == 0 : PodcastSubscriptionBenefitEnum.NonQuotaListening
            if (listenerAccount.PodcastListenSlot == 0)
            {
                conditions.Add(PodcastSubscriptionBenefitEnum.NonQuotaListening);
            }

            // Subscriber only (PodcastShowSubscriptionType) : PodcastSubscriptionBenefitEnum.SubscriberOnlyShows
            if (podcastEpisode.PodcastShow.PodcastShowSubscriptionTypeId == (int)PodcastShowSubscriptionTypeEnum.SubscriberOnly)
            {
                conditions.Add(PodcastSubscriptionBenefitEnum.SubscriberOnlyShows);
            }

            // Subscriber only (PodcastEpisodeSubscriptionType) : PodcastSubscriptionBenefitEnum.SubscriberOnlyEpisodes
            if (podcastEpisode.PodcastEpisodeSubscriptionTypeId == (int)PodcastEpisodeSubscriptionTypeEnum.SubscriberOnly)
            {
                conditions.Add(PodcastSubscriptionBenefitEnum.SubscriberOnlyEpisodes);
            }

            // Bonus (PodcastEpisodeSubscriptionType) : PodcastSubscriptionBenefitEnum.BonusEpisodes
            if (podcastEpisode.PodcastEpisodeSubscriptionTypeId == (int)PodcastEpisodeSubscriptionTypeEnum.Bonus)
            {
                conditions.Add(PodcastSubscriptionBenefitEnum.BonusEpisodes);
            }

            // Archive (PodcastEpisodeSubscriptionType) : PodcastSubscriptionBenefitEnum.ArchiveEpisodesAccess
            if (podcastEpisode.PodcastEpisodeSubscriptionTypeId == (int)PodcastEpisodeSubscriptionTypeEnum.Archive)
            {
                conditions.Add(PodcastSubscriptionBenefitEnum.ArchiveEpisodesAccess);
            }

            return conditions;
        }

        // public string GenerateEpisodeListenToken(Guid sessionId, bool isUsed)
        // {
        //     var claims = new Dictionary<string, object>
        //     {
        //         { "SessionId", sessionId },
        //         { "IsUsed", isUsed }
        //     };

        //     var token = _jwtHelper.GenerateJWT_OneSecretKey(claims, _podcastListenSessionConfig.TokenEncryptionKeyRequestExpirationMinutes, _podcastListenSessionConfig.TokenSecretKey);

        //     return token;
        // }
        public string GenerateEpisodeListenHlsEnckeyRequestToken(Guid sessionId)
        {
            var claims = new Dictionary<string, object>
            {
                { "SessionId", sessionId },
                { "jti", Guid.NewGuid().ToString() }
            };

            var token = _jwtHelper.GenerateJWT_OneSecretKey(claims, _podcastListenSessionConfig.TokenEncryptionKeyRequestExpirationMinutes, _podcastListenSessionConfig.TokenSecretKey);

            return token;
        }

        public async Task UpdateListenCountAsync(PodcastEpisode validEpisode, AccountDTO listenerAccount, AccountStatusCache podcaster, List<int> listenerBenefits)
        {
            validEpisode.ListenCount += 1;
            await _podcastEpisodeGenericRepository.UpdateAsync(validEpisode.Id, validEpisode);

            validEpisode.PodcastShow.ListenCount += 1;
            await _podcastShowGenericRepository.UpdateAsync(validEpisode.PodcastShow.Id, validEpisode.PodcastShow);

            if (validEpisode.PodcastShow.PodcastChannel != null)
            {
                validEpisode.PodcastShow.PodcastChannel.ListenCount += 1;
                await _podcastChannelGenericRepository.UpdateAsync(validEpisode.PodcastShow.PodcastChannel.Id, validEpisode.PodcastShow.PodcastChannel);
            }

            // chạy flow + listenCount cho Podcaster, - lượt nghe còn lại của account nếu danh sách benefits không có PodcastSubscriptionBenefitEnum.NonQuotaListening
            JObject requestData;
            StartSagaTriggerMessage startSagaTriggerMessage;
            if (!listenerBenefits.Contains((int)PodcastSubscriptionBenefitEnum.NonQuotaListening))
            {
                requestData = new JObject
                {
                    ["AccountId"] = listenerAccount.Id,
                    ["PodcastListenSlotAmount"] = 1,
                };

                startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "account-podcast-listen-slot-subtraction-flow");
                await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
            }


            requestData = new JObject
            {
                ["PodcasterId"] = podcaster.Id,
                ["ListenCountAmount"] = 1,
            };
            startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("user-management-domain", requestData, null, "podcaster-listen-count-add-flow");
            await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);
        }

        // public async Task<EpisodeListenResponseDTO> GetEpisodeListenAsync(Guid podcastEpisodeId, int listenerAccountId, List<PodcastSubscriptionBenefitDTO> currentPodcastSubscriptionRegistrationBenefitList, DeviceInfoDTO deviceInfo, string? token)
        // {
        //     using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //     {
        //         bool transactionCompleted = false;
        //         try
        //         {
        //             // nếu có token:
        //             // + kiểm tra token phải hợp lệ với secret key và có trường ExpiredAt chưa hết hạn
        //             // + lấy trường SessionId từ token để kiểm tra session tồn tại và IsCompleted = false 
        //             // + Check điều kiện cần đề nghe (alway cho mọi trường hợp): 
        //             //      + check channel nếu có thì deleted == null và phải đang publish
        //             //      + check show deleted == null và phải đang publish
        //             //      + check episode deleted == null và phải đang publish
        //             // + Check điều kiện liên quan đến subscription type của show và episode đang yêu cầu nghe, để append vào danh sách điều kiện (kiểu Set PodcastSubscriptionBenefitEnum điều kiện cần):
        //             //      + isreleased = false (Show) : PodcastSubscriptionBenefitEnum.ShowsEpisodesEarlyAccess
        //             //      + PodcastListenSlot == 0 : PodcastSubscriptionBenefitEnum.NonQuotaListening
        //             //      + Subscriber only (PodcastShowSubscriptionType) : PodcastSubscriptionBenefitEnum.SubscriberOnlyShows
        //             //      + Subscriber only (PodcastEpisodeSubscriptionType) : PodcastSubscriptionBenefitEnum.SubscriberOnlyEpisodes
        //             //      + Bonus (PodcastEpisodeSubscriptionType) : PodcastSubscriptionBenefitEnum.BonusEpisodes
        //             //      + Archive (PodcastEpisodeSubscriptionType) : PodcastSubscriptionBenefitEnum.ArchiveEpisodesAccess
        //             //  * nếu không tồn tại điều kiện nào trong số trên thì cho nghe bình thường
        //             //  * nếu tồn tại điều kiện và Show có ít nhất 1 gói subscription đang active hoặc channel (nếu channel != null) có ít nhất 1 gói subscription đang active, thì query vào subscription service để kiểm tra listenerAccountId đang đăng kí 1 trong 2 gói subscription đó hay không:
        //             //      + nếu không thì từ chối nghe + set IsCompleted = true cho session
        //             //      + nếu có thì lấy ra danh sách beneifit của gói đó để kiểm tra với danh sách điều kiện cần:
        //             //            + nếu bao gồm tất cả các điều kiện cần thì cho nghe
        //             //            + nếu không bao gồm tất cả các điều kiện cần thì từ chối nghe + set IsCompleted = true cho session
        //             // + tạo token mới với (SessionId, IsUsed = false, ExpiredAt = now + PodcastListenSessionConfig.TokenEncryptionKeyRequestExpirationMinutes)
        //             // + update token vào session
        //             // + gọi hàm cập nhật listenCount ở các đối tượng liên quan
        //             // + trả về token mới  + playlist file key
        //             // nếu không có token:
        //             // + Check điều kiện cần đề nghe như trên:
        //             //      + nếu không tồn tại điều kiện nào trong số trên thì cho nghe bình thường
        //             //      + nếu tồn tại điều kiện và Show có ít nhất 1 gói subscription đang active hoặc channel (nếu channel != null) có ít nhất 1 gói subscription đang active, thì query vào subscription service để kiểm tra listenerAccountId đang đăng kí 1 trong 2 gói subscription đó hay không:
        //             //          + nếu không thì từ chối nghe
        //             //          + nếu có thì lấy ra danh sách beneifit của gói đó để kiểm tra với danh sách điều kiện cần:
        //             //              + nếu bao gồm tất cả các điều kiện cần thì cho nghe
        //             //              + nếu không bao gồm tất cả các điều kiện cần thì từ chối nghe
        //             // + tạo mới session với IsCompleted = false và ExpiredAt = now + PodcastListenSessionConfig.SessionExpirationMinutes
        //             // + đánh isCompleted = true ở tất cả các session cũ chưa completed của episode này và account này
        //             // + tạo token mới với (SessionId, IsUsed = false, ExpiredAt = now + PodcastListenSessionConfig.TokenEncryptionKeyRequestExpirationMinutes)
        //             // + update token vào session
        //             // + gọi hàm cập nhật listenCount ở các đối tượng liên quan
        //             // + trả về token mới  + playlist file key

        //             // kiểm tra token sơ bộ
        //             ClaimsPrincipal? principal = null;
        //             if (token != null)
        //             {
        //                 principal = _jwtHelper.DecodeToken_OneSecretKey(token, _podcastListenSessionConfig.TokenSecretKey);
        //                 // kiểm tra session
        //                 var sessionId = principal?.FindFirst("SessionId")?.Value;
        //                 var existingSession = await (_podcastEpisodeListenSessionGenericRepository.FindAll(
        //                     predicate: pes => pes.Id.ToString() == sessionId && pes.AccountId == listenerAccountId && pes.IsCompleted == false,
        //                     includeFunc: null
        //                 )).FirstOrDefaultAsync();
        //                 if (existingSession == null)
        //                 {
        //                     throw new Exception("Invalid or already used token, session does not exist or already completed");
        //                 }


        //                 var validEpisode = await GetValidEpisodeListenPermission(podcastEpisodeId);
        //                 var podcaster = await _accountCachingService.GetAccountStatusCacheById(validEpisode.PodcastShow.PodcasterId);
        //                 var playlistFileKey = FilePathHelper.CombinePaths(
        //                                 _filePathConfig.PODCAST_EPISODE_FILE_PATH,
        //                                 validEpisode.Id.ToString(),
        //                                 "playlist",
        //                                 _hlsConfig.PlaylistFileName
        //                             );
        //                 string sessionToken = null;
        //                 var account = await GetAccountById(listenerAccountId);
        //                 if (account == null)
        //                 {
        //                     throw new Exception("Listener with id " + listenerAccountId + " does not exist");
        //                 }
        //                 HashSet<PodcastSubscriptionBenefitEnum> listenPermissionConditions = await GetEpisodeListenPermissionConditionsAsync(validEpisode, account);
        //                 if (listenPermissionConditions.Count == 0)
        //                 {
        //                     // sessionToken = GenerateEpisodeListenToken(existingSession.Id, false);
        //                     // existingSession.Token = sessionToken;
        //                     // await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(existingSession.Id, existingSession);
        //                     sessionToken = GenerateEpisodeListenHlsEnckeyRequestToken(existingSession.Id);
        //                     await _podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository.CreateAsync(new PodcastEpisodeListenSessionHlsEnckeyRequestToken
        //                     {
        //                         PodcastEpisodeListenSessionId = existingSession.Id,
        //                         Token = sessionToken,
        //                         IsUsed = false,
        //                     });
        //                 }
        //                 else
        //                 {
        //                     PodcastSubscriptionDTO channelSubscription = null;
        //                     PodcastSubscriptionDTO showSubscription = null;
        //                     // kiểm tra điều kiện subscription
        //                     if (validEpisode.PodcastShow.PodcastChannelId != null)
        //                     {
        //                         channelSubscription = await GetActivePodcastSubscriptionByChannelId(validEpisode.PodcastShow.PodcastChannelId);
        //                     }
        //                     showSubscription = await GetActivePodcastSubscriptionByShowId(validEpisode.PodcastShow.Id);

        //                     if (channelSubscription == null && showSubscription == null)
        //                     {
        //                         // không có gói subscription active nào => từ chối nghe
        //                         existingSession.IsCompleted = true;
        //                         await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(existingSession.Id, existingSession);
        //                         await transaction.CommitAsync();
        //                         transactionCompleted = true;
        //                         throw new Exception("Listener does not have permission to listen to this episode, reason: no active subscription");
        //                     }
        //                     else
        //                     {
        //                         // nếu có gói channel subscription active thì chỉ cần 1 query vào channel subscription , nếu không thì query vào show subscription
        //                         PodcastSubscriptionRegistrationDTO listenerSubscriptionRegistration = channelSubscription != null ?
        //                             await GetAccountSubscriptionRegistrationByAccountIdAndSubscriptionId(listenerAccountId, channelSubscription.Id) :
        //                             await GetAccountSubscriptionRegistrationByAccountIdAndSubscriptionId(listenerAccountId, showSubscription.Id);

        //                         if (listenerSubscriptionRegistration == null)
        //                         {
        //                             // không đăng kí gói subscription active nào => từ chối nghe
        //                             existingSession.IsCompleted = true;
        //                             await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(existingSession.Id, existingSession);
        //                             await transaction.CommitAsync();
        //                             transactionCompleted = true;
        //                             throw new Exception("Listener does not have permission to listen to this episode, reason: no subscription registration");
        //                         }
        //                         else
        //                         {
        //                             // kiểm tra benefit đang có 
        //                             List<int> listenerBenefits = listenerSubscriptionRegistration.PodcastSubscription.PodcastSubscriptionBenefitMappings
        //                                 .Where(psbm => psbm.Version == listenerSubscriptionRegistration.CurrentVersion)
        //                                 .Select(psbm => psbm.PodcastSubscriptionBenefitId)
        //                                 .ToList();

        //                             bool hasAllConditions = true;
        //                             HashSet<PodcastSubscriptionBenefitEnum> missingConditions = new HashSet<PodcastSubscriptionBenefitEnum>();
        //                             foreach (var condition in listenPermissionConditions)
        //                             {
        //                                 if (!listenerBenefits.Contains((int)condition))
        //                                 {
        //                                     hasAllConditions = false;
        //                                     // break;
        //                                     missingConditions.Add(condition);
        //                                 }
        //                             }

        //                             if (hasAllConditions == false)
        //                             {
        //                                 // không có đủ benefit để nghe => từ chối nghe
        //                                 existingSession.IsCompleted = true;
        //                                 await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(existingSession.Id, existingSession);
        //                                 await transaction.CommitAsync();
        //                                 transactionCompleted = true;
        //                                 throw new Exception("Listener does not have permission to listen to this episode, reason: insufficient benefits - missing conditions: " + string.Join(", ", missingConditions));
        //                             }
        //                             else
        //                             {
        //                                 // sessionToken = GenerateEpisodeListenToken(existingSession.Id, false);
        //                                 // existingSession.Token = sessionToken;
        //                                 // await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(existingSession.Id, existingSession);
        //                                 sessionToken = GenerateEpisodeListenHlsEnckeyRequestToken(existingSession.Id);
        //                                 await _podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository.CreateAsync(new PodcastEpisodeListenSessionHlsEnckeyRequestToken
        //                                 {
        //                                     PodcastEpisodeListenSessionId = existingSession.Id,
        //                                     Token = sessionToken,
        //                                     IsUsed = false,
        //                                 });
        //                             }
        //                         }

        //                     }

        //                 }

        //                 // Cập nhật listenCount ở các đối tượng liên quan
        //                 // await UpdateListenCountAsync(validEpisode, account, podcaster);
        //                 await transaction.CommitAsync();
        //                 transactionCompleted = true;
        //                 return new EpisodeListenResponseDTO
        //                 {
        //                     // Token = existingSession.Token,
        //                     Token = sessionToken,
        //                     PlaylistFileKey = playlistFileKey,
        //                     PodcastEpisodeListenSession = new PodcastEpisodeListenSessionSnippetResponseDTO
        //                     {
        //                         Id = existingSession.Id,
        //                         LastListenDurationSeconds = existingSession.LastListenDurationSeconds
        //                     },
        //                     PodcastEpisode = new PodcastEpisodeSnippetResponseDTO
        //                     {
        //                         Id = validEpisode.Id,
        //                         Name = validEpisode.Name,
        //                         Description = validEpisode.Description,
        //                         MainImageFileKey = validEpisode.MainImageFileKey,
        //                         IsReleased = validEpisode.IsReleased,
        //                         ReleaseDate = validEpisode.ReleaseDate,
        //                     },
        //                     Podcaster = new AccountSnippetResponseDTO
        //                     {
        //                         Id = podcaster.Id,
        //                         Email = podcaster.Email,
        //                         FullName = podcaster.PodcasterProfileName,
        //                         MainImageFileKey = podcaster.MainImageFileKey
        //                     },
        //                     AudioFileUrl = deviceInfo.Platform == DevicePlatform.ios.ToString() || deviceInfo.Platform == DevicePlatform.android.ToString()
        //                         ? await _fileIOHelper.GeneratePresignedUrlAsync(
        //                             validEpisode.AudioFileKey
        //                         )
        //                         : null
        //                 };
        //             }
        //             else // token == null
        //             {
        //                 var validEpisode = await GetValidEpisodeListenPermission(podcastEpisodeId);
        //                 var podcaster = await _accountCachingService.GetAccountStatusCacheById(validEpisode.PodcastShow.PodcasterId);
        //                 var playlistFileKey = FilePathHelper.CombinePaths(
        //                                 _filePathConfig.PODCAST_EPISODE_FILE_PATH,
        //                                 validEpisode.Id.ToString(),
        //                                 "playlist",
        //                                 _hlsConfig.PlaylistFileName
        //                             );

        //                 var account = await GetAccountById(listenerAccountId);
        //                 if (account == null)
        //                 {
        //                     throw new Exception("Listener with id " + listenerAccountId + " does not exist");
        //                 }

        //                 HashSet<PodcastSubscriptionBenefitEnum> listenPermissionConditions = await GetEpisodeListenPermissionConditionsAsync(validEpisode, account);

        //                 PodcastEpisodeListenSession newSession = null;
        //                 string sessionToken = null;

        //                 if (listenPermissionConditions.Count == 0)
        //                 {
        //                     // đánh iscopleted = true ở tất cả các session cũ chưa completed của episode này và account này
        //                     var oldSessionIds = await _podcastEpisodeListenSessionGenericRepository.FindAll(
        //                         predicate: pes => pes.AccountId == listenerAccountId && pes.IsCompleted == false,
        //                         includeFunc: null
        //                     ).Select(pes => pes.Id).ToListAsync();
        //                     foreach (var oldSessionId in oldSessionIds)
        //                     {
        //                         var oldSession = await _podcastEpisodeListenSessionGenericRepository.FindByIdAsync(oldSessionId);
        //                         oldSession.IsCompleted = true;
        //                         await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(oldSession.Id, oldSession);
        //                     }

        //                     // không có điều kiện đặc biệt => cho nghe bình thường


        //                     newSession = new PodcastEpisodeListenSession
        //                     {
        //                         Id = Guid.NewGuid(),
        //                         AccountId = listenerAccountId,
        //                         PodcastEpisodeId = validEpisode.Id,
        //                         PodcastCategoryId = validEpisode.PodcastShow.PodcastCategoryId,
        //                         PodcastSubCategoryId = validEpisode.PodcastShow.PodcastSubCategoryId,
        //                         IsContentRemoved = false,
        //                         ExpiredAt = _dateHelper.GetNowByAppTimeZone().AddMinutes(_podcastListenSessionConfig.SessionExpirationMinutes)
        //                     };

        //                     // sessionToken = GenerateEpisodeListenToken(newSession.Id, false);
        //                     // newSession.Token = sessionToken;
        //                     await _podcastEpisodeListenSessionGenericRepository.CreateAsync(newSession);
        //                     sessionToken = GenerateEpisodeListenHlsEnckeyRequestToken(newSession.Id);
        //                     await _podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository.CreateAsync(new PodcastEpisodeListenSessionHlsEnckeyRequestToken
        //                     {
        //                         PodcastEpisodeListenSessionId = newSession.Id,
        //                         Token = sessionToken,
        //                         IsUsed = false,
        //                     });

        //                     await UpdateListenCountAsync(validEpisode, account, podcaster, new List<int>());


        //                 }
        //                 else
        //                 {
        //                     PodcastSubscriptionDTO channelSubscription = null;
        //                     PodcastSubscriptionDTO showSubscription = null;

        //                     // kiểm tra điều kiện subscription
        //                     if (validEpisode.PodcastShow.PodcastChannelId != null)
        //                     {
        //                         channelSubscription = await GetActivePodcastSubscriptionByChannelId(validEpisode.PodcastShow.PodcastChannelId);
        //                     }
        //                     showSubscription = await GetActivePodcastSubscriptionByShowId(validEpisode.PodcastShow.Id);

        //                     if (channelSubscription == null && showSubscription == null)
        //                     {
        //                         // không có gói subscription active nào => từ chối nghe
        //                         await transaction.CommitAsync();
        //                         transactionCompleted = true;
        //                         throw new Exception("Listener does not have permission to listen to this episode, reason: no active subscription");
        //                     }
        //                     else
        //                     {
        //                         // // nếu có gói channel subscription active thì chỉ cần 1 query vào channel subscription , nếu không thì query vào show subscription
        //                         // PodcastSubscriptionRegistrationDTO listenerSubscriptionRegistration = channelSubscription != null ? 
        //                         //     await GetAccountSubscriptionRegistrationByAccountIdAndSubscriptionId(listenerAccountId, channelSubscription.Id) :
        //                         //     await GetAccountSubscriptionRegistrationByAccountIdAndSubscriptionId(listenerAccountId, showSubscription.Id);

        //                         // kiểm tra việc đăng kí gói subscription của người dùng , người dùng sẽ đăng kí 1 trong 2 level subscription (channel level hoặc show level) nhưng 1 lần chỉ được đăng kí 1 level chứ không được đk cả 2, dù có đăng kí ở level nào thì benefit cũng được áp dụng
        //                         // kiểm tra channel subscription != null thì kiểm tra đăng kí , nếu đăng kí bằng null thì kiểm tra show subscription != null thì kiểm tra đăng kí 
        //                         PodcastSubscriptionRegistrationDTO listenerSubscriptionRegistration = null;
        //                         if (channelSubscription != null)
        //                         {
        //                             listenerSubscriptionRegistration = await GetAccountSubscriptionRegistrationByAccountIdAndSubscriptionId(listenerAccountId, channelSubscription.Id);
        //                         }

        //                         if (showSubscription != null && listenerSubscriptionRegistration == null)
        //                         {
        //                             listenerSubscriptionRegistration = await GetAccountSubscriptionRegistrationByAccountIdAndSubscriptionId(listenerAccountId, showSubscription.Id);
        //                         }

        //                         if (listenerSubscriptionRegistration == null)
        //                         {
        //                             // không đăng kí gói subscription active nào => từ chối nghe
        //                             await transaction.CommitAsync();
        //                             transactionCompleted = true;
        //                             throw new Exception("Listener does not have permission to listen to this episode, reason: no subscription registration");
        //                         }
        //                         else
        //                         {
        //                             // kiểm tra benefit đang có 
        //                             List<int> listenerBenefits = listenerSubscriptionRegistration.PodcastSubscription.PodcastSubscriptionBenefitMappings
        //                                 .Where(psbm => psbm.Version == listenerSubscriptionRegistration.CurrentVersion)
        //                                 .Select(psbm => psbm.PodcastSubscriptionBenefitId)
        //                                 .ToList();

        //                             bool hasAllConditions = true;
        //                             HashSet<PodcastSubscriptionBenefitEnum> missingConditions = new HashSet<PodcastSubscriptionBenefitEnum>();
        //                             foreach (var condition in listenPermissionConditions)
        //                             {
        //                                 if (!listenerBenefits.Contains((int)condition))
        //                                 {
        //                                     hasAllConditions = false;
        //                                     // break;
        //                                     missingConditions.Add(condition);
        //                                 }
        //                             }

        //                             if (hasAllConditions == false)
        //                             {
        //                                 // không có đủ benefit để nghe => từ chối nghe
        //                                 await transaction.CommitAsync();
        //                                 transactionCompleted = true;
        //                                 throw new Exception("Listener does not have permission to listen to this episode, reason: insufficient benefits - missing conditions: " + string.Join(", ", missingConditions));
        //                             }
        //                             else
        //                             {
        //                                 // Đánh dấu các session cũ là đã hoàn thành
        //                                 var oldSessionIds = await _podcastEpisodeListenSessionGenericRepository.FindAll(
        //                                     predicate: pes => pes.AccountId == listenerAccountId && pes.IsCompleted == false,
        //                                     includeFunc: null
        //                                 ).Select(pes => pes.Id).ToListAsync();
        //                                 foreach (var oldSessionId in oldSessionIds)
        //                                 {
        //                                     var oldSession = await _podcastEpisodeListenSessionGenericRepository.FindByIdAsync(oldSessionId);
        //                                     oldSession.IsCompleted = true;
        //                                     await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(oldSession.Id, oldSession);
        //                                 }

        //                                 // có đủ benefit => tạo session mới
        //                                 newSession = new PodcastEpisodeListenSession
        //                                 {
        //                                     Id = Guid.NewGuid(),
        //                                     AccountId = listenerAccountId,
        //                                     PodcastEpisodeId = validEpisode.Id,
        //                                     PodcastCategoryId = validEpisode.PodcastShow.PodcastCategoryId,
        //                                     PodcastSubCategoryId = validEpisode.PodcastShow.PodcastSubCategoryId,
        //                                     IsContentRemoved = false,
        //                                     ExpiredAt = _dateHelper.GetNowByAppTimeZone().AddMinutes(_podcastListenSessionConfig.SessionExpirationMinutes)
        //                                 };

        //                                 // sessionToken = GenerateEpisodeListenToken(newSession.Id, false);
        //                                 // newSession.Token = sessionToken;
        //                                 await _podcastEpisodeListenSessionGenericRepository.CreateAsync(newSession);
        //                                 sessionToken = GenerateEpisodeListenHlsEnckeyRequestToken(newSession.Id);
        //                                 await _podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository.CreateAsync(new PodcastEpisodeListenSessionHlsEnckeyRequestToken
        //                                 {
        //                                     PodcastEpisodeListenSessionId = newSession.Id,
        //                                     Token = sessionToken,
        //                                     IsUsed = false,
        //                                 });

        //                                 await UpdateListenCountAsync(validEpisode, account, podcaster, listenerBenefits);

        //                             }
        //                         }
        //                     }
        //                 }

        //                 // Cập nhật listenCount ở các đối tượng liên quan
        //                 await transaction.CommitAsync();
        //                 transactionCompleted = true;

        //                 return new EpisodeListenResponseDTO
        //                 {
        //                     // Token = newSession.Token,
        //                     Token = sessionToken,
        //                     PlaylistFileKey = playlistFileKey,
        //                     // LastListenDurationSeconds = 0,
        //                     PodcastEpisodeListenSession = new PodcastEpisodeListenSessionSnippetResponseDTO
        //                     {
        //                         Id = newSession.Id,
        //                         LastListenDurationSeconds = newSession.LastListenDurationSeconds
        //                     },
        //                     PodcastEpisode = new PodcastEpisodeSnippetResponseDTO
        //                     {
        //                         Id = validEpisode.Id,
        //                         Name = validEpisode.Name,
        //                         Description = validEpisode.Description,
        //                         MainImageFileKey = validEpisode.MainImageFileKey,
        //                         IsReleased = validEpisode.IsReleased,
        //                         ReleaseDate = validEpisode.ReleaseDate,
        //                     },
        //                     Podcaster = new AccountSnippetResponseDTO
        //                     {
        //                         Id = podcaster.Id,
        //                         Email = podcaster.Email,
        //                         FullName = podcaster.PodcasterProfileName,
        //                         MainImageFileKey = podcaster.MainImageFileKey
        //                     },
        //                     AudioFileUrl = deviceInfo.Platform == DevicePlatform.ios.ToString() || deviceInfo.Platform == DevicePlatform.android.ToString()
        //                         ? await _fileIOHelper.GeneratePresignedUrlAsync(
        //                             validEpisode.AudioFileKey
        //                         )
        //                         : null
        //                 };
        //             }
        //         }


        //         catch (Exception ex)
        //         {
        //             if (!transactionCompleted)
        //             {
        //                 await transaction.RollbackAsync();
        //             }
        //             Console.WriteLine("\n" + ex.StackTrace + "\n");
        //             throw new HttpRequestException("An error occurred while processing your request, error: " + ex.Message);
        //         }
        //     }
        // }


        public async Task<EpisodeListenResponseDTO> GetEpisodeListenAsync(Guid podcastEpisodeId, int listenerAccountId, EpisodeListenRequestDTO episodeListenRequest, DeviceInfoDTO deviceInfo, Guid? continueListenSessionId, string? token)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                bool transactionCompleted = false;
                try
                {
                    PodcastEpisodeListenSession? continueListenSession = null;
                    if (continueListenSessionId != null)
                    {
                        continueListenSession = await _podcastEpisodeListenSessionGenericRepository.FindByIdAsync(
                            id: continueListenSessionId,
                            includeFunc: null
                        );
                        if (continueListenSession == null || continueListenSession.AccountId != listenerAccountId || continueListenSession.PodcastEpisodeId != podcastEpisodeId)
                        {
                            // throw new Exception("Invalid continue listen session id or session already completed");
                            throw new Exception("Invalid continue listen session id " + continueListenSessionId);
                        }
                    }

                    var validEpisode = await GetValidEpisodeListenPermission(podcastEpisodeId);
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(validEpisode.PodcastShow.PodcasterId);
                    var playlistFileKey = FilePathHelper.CombinePaths(
                                    _filePathConfig.PODCAST_EPISODE_FILE_PATH,
                                    validEpisode.Id.ToString(),
                                    "playlist",
                                    _hlsConfig.PlaylistFileName
                                );

                    var account = await GetAccountById(listenerAccountId);
                    if (account == null)
                    {
                        throw new Exception("Listener with id " + listenerAccountId + " does not exist");
                    }

                    HashSet<PodcastSubscriptionBenefitEnum> listenPermissionConditions = await GetEpisodeListenPermissionConditionsAsync(validEpisode, account);

                    PodcastEpisodeListenSession newSession = null;
                    string sessionToken = null;

                    if (listenPermissionConditions.Count == 0)
                    {
                        List<int> listenerBenefits = episodeListenRequest.CurrentPodcastSubscriptionRegistrationBenefitList
                                    .Select(psb => psb.Id)
                                    .ToList();
                        // đánh iscopleted = true ở tất cả các session cũ chưa completed của episode này và account này
                        var oldSessionIds = await _podcastEpisodeListenSessionGenericRepository.FindAll(
                            predicate: pes => pes.AccountId == listenerAccountId && pes.IsCompleted == false,
                            includeFunc: null
                        ).Select(pes => pes.Id).ToListAsync();
                        foreach (var oldSessionId in oldSessionIds)
                        {
                            var oldSession = await _podcastEpisodeListenSessionGenericRepository.FindByIdAsync(oldSessionId);
                            oldSession.IsCompleted = true;
                            await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(oldSession.Id, oldSession);
                        }

                        // không có điều kiện đặc biệt => cho nghe bình thường


                        newSession = new PodcastEpisodeListenSession
                        {
                            Id = Guid.NewGuid(),
                            AccountId = listenerAccountId,
                            PodcastEpisodeId = validEpisode.Id,
                            PodcastCategoryId = validEpisode.PodcastShow.PodcastCategoryId,
                            PodcastSubCategoryId = validEpisode.PodcastShow.PodcastSubCategoryId,
                            LastListenDurationSeconds = continueListenSession != null ? continueListenSession.LastListenDurationSeconds : 0,
                            IsContentRemoved = false,
                            ExpiredAt = _dateHelper.GetNowByAppTimeZone().AddMinutes(_podcastListenSessionConfig.SessionExpirationMinutes)
                        };


                        await _podcastEpisodeListenSessionGenericRepository.CreateAsync(newSession);
                        sessionToken = GenerateEpisodeListenHlsEnckeyRequestToken(newSession.Id);
                        await _podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository.CreateAsync(new PodcastEpisodeListenSessionHlsEnckeyRequestToken
                        {
                            PodcastEpisodeListenSessionId = newSession.Id,
                            Token = sessionToken,
                            IsUsed = false,
                        });

                        await UpdateListenCountAsync(validEpisode, account, podcaster, listenerBenefits);


                    }
                    else
                    {
                        // PodcastSubscriptionDTO channelSubscription = null;
                        // PodcastSubscriptionDTO showSubscription = null;

                        // // kiểm tra điều kiện subscription
                        // if (validEpisode.PodcastShow.PodcastChannelId != null)
                        // {
                        //     channelSubscription = await GetActivePodcastSubscriptionByChannelId(validEpisode.PodcastShow.PodcastChannelId);
                        // }
                        // showSubscription = await GetActivePodcastSubscriptionByShowId(validEpisode.PodcastShow.Id);

                        // if (channelSubscription == null && showSubscription == null)
                        // {
                        //     // không có gói subscription active nào => từ chối nghe
                        //     await transaction.CommitAsync();
                        //     transactionCompleted = true;
                        //     throw new Exception("Listener does not have permission to listen to this episode, reason: no active subscription");
                        // }
                        // else
                        // {
                        // kiểm tra các benefit người dùng gửi vào 
                        var currentPodcastSubscriptionRegistrationBenefitList = episodeListenRequest.CurrentPodcastSubscriptionRegistrationBenefitList;

                        if (currentPodcastSubscriptionRegistrationBenefitList == null || currentPodcastSubscriptionRegistrationBenefitList.Count == 0)
                        {
                            // không đăng kí gói subscription active nào => từ chối nghe
                            await transaction.CommitAsync();
                            transactionCompleted = true;
                            throw new Exception("Listener does not have permission to listen to this episode, reason: no subscription registration");
                        }
                        else
                        {
                            // kiểm tra benefit đang có theo danh sách người dùng gửi vào 
                            List<int> listenerBenefits = currentPodcastSubscriptionRegistrationBenefitList
                                .Select(psb => psb.Id)
                                .ToList();

                            bool hasAllConditions = true;
                            HashSet<PodcastSubscriptionBenefitEnum> missingConditions = new HashSet<PodcastSubscriptionBenefitEnum>();
                            foreach (var condition in listenPermissionConditions)
                            {
                                if (!listenerBenefits.Contains((int)condition))
                                {
                                    hasAllConditions = false;
                                    // break;
                                    missingConditions.Add(condition);
                                }
                            }

                            if (hasAllConditions == false)
                            {
                                // không có đủ benefit để nghe => từ chối nghe
                                await transaction.CommitAsync();
                                transactionCompleted = true;
                                throw new Exception("Listener does not have permission to listen to this episode, reason: insufficient benefits - missing conditions: " + string.Join(", ", missingConditions));
                            }
                            else
                            {
                                // Đánh dấu các session cũ là đã hoàn thành
                                var oldSessionIds = await _podcastEpisodeListenSessionGenericRepository.FindAll(
                                    predicate: pes => pes.AccountId == listenerAccountId && pes.IsCompleted == false,
                                    includeFunc: null
                                ).Select(pes => pes.Id).ToListAsync();
                                foreach (var oldSessionId in oldSessionIds)
                                {
                                    var oldSession = await _podcastEpisodeListenSessionGenericRepository.FindByIdAsync(oldSessionId);
                                    oldSession.IsCompleted = true;
                                    await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(oldSession.Id, oldSession);
                                }

                                // có đủ benefit => tạo session mới
                                newSession = new PodcastEpisodeListenSession
                                {
                                    Id = Guid.NewGuid(),
                                    AccountId = listenerAccountId,
                                    PodcastEpisodeId = validEpisode.Id,
                                    PodcastCategoryId = validEpisode.PodcastShow.PodcastCategoryId,
                                    PodcastSubCategoryId = validEpisode.PodcastShow.PodcastSubCategoryId,
                                    IsContentRemoved = false,
                                    LastListenDurationSeconds = continueListenSession != null ? continueListenSession.LastListenDurationSeconds : 0,
                                    ExpiredAt = _dateHelper.GetNowByAppTimeZone().AddMinutes(_podcastListenSessionConfig.SessionExpirationMinutes)
                                };

                                await _podcastEpisodeListenSessionGenericRepository.CreateAsync(newSession);
                                sessionToken = GenerateEpisodeListenHlsEnckeyRequestToken(newSession.Id);
                                await _podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository.CreateAsync(new PodcastEpisodeListenSessionHlsEnckeyRequestToken
                                {
                                    PodcastEpisodeListenSessionId = newSession.Id,
                                    Token = sessionToken,
                                    IsUsed = false,
                                });

                                await UpdateListenCountAsync(validEpisode, account, podcaster, listenerBenefits);

                            }
                        }
                        // }
                    }

                    // Cập nhật listenCount ở các đối tượng liên quan
                    await transaction.CommitAsync();
                    transactionCompleted = true;

                    // đánh completed mọi prcedure 
                    await _customerListenSessionProcedureCachingService.MarkAllProceduresCompletedAsync(listenerAccountId);
                    // tạo procedure mới
                    Guid newProcedureId = Guid.NewGuid();
                    Console.WriteLine("Generating new procedure id...,");
                    CustomerListenSessionProcedure newProcedure = new CustomerListenSessionProcedure
                    {
                        Id = newProcedureId,
                        PlayOrderMode = _customerListenSessionProcedureConfig.DefaultPlayOrderMode,
                        IsAutoPlay = _customerListenSessionProcedureConfig.DefaultIsAutoPlay,
                        ListenObjectsRandomOrder = new List<ListenSessionProcedureListenObjectQueueItem>(),
                        ListenObjectsSequentialOrder = new List<ListenSessionProcedureListenObjectQueueItem>(),
                        SourceDetail = new ListenSessionProcedureSourceDetail
                        {
                            Type = episodeListenRequest.SourceType.ToString(),
                            Booking = null,
                            PodcastShow = episodeListenRequest.SourceType == CustomerListenSessionProcedureSourceDetailTypeEnum.SpecifyShowEpisodes
                                ? new PodcastShowInfo
                                {
                                    Id = validEpisode.PodcastShowId,
                                    Name = validEpisode.PodcastShow.Name
                                }
                                : null
                        },
                        IsCompleted = false,
                        CreatedAt = _dateHelper.GetNowByAppTimeZone()
                    };
                    // refresh order 
                    if (episodeListenRequest.SourceType == CustomerListenSessionProcedureSourceDetailTypeEnum.SavedEpisodes)
                    {
                        newProcedure = await RefreshEpisodeOrderForSavedEpisodesSourceAsync(
                            newProcedure,
                            listenerAccountId,
                            null,
                            // episodeListenRequest.CurrentPodcastSubscriptionRegistrationBenefitList
                            null
                        );
                    }
                    else if (episodeListenRequest.SourceType == CustomerListenSessionProcedureSourceDetailTypeEnum.SpecifyShowEpisodes)
                    {
                        newProcedure = await RefreshEpisodeOrderForSpecifyShowEpisodesSourceAsync(
                            newProcedure,
                            listenerAccountId,
                            null,
                            episodeListenRequest.CurrentPodcastSubscriptionRegistrationBenefitList
                        );
                    }
                    await _customerListenSessionProcedureCachingService.CreateProcedureAsync(listenerAccountId, newProcedureId, newProcedure);

                    // chạy flow complete-all-user-episode-listen-sessions
                    JObject requestData = new JObject
                    {
                        ["AccountId"] = listenerAccountId,
                        ["IsEpisodeListenSessionCompleted"] = false,
                        ["IsBookingProducingListenSessionCompleted"] = true,
                    };

                    var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "all-user-listen-session-completion-flow");
                    await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);

                    return new EpisodeListenResponseDTO
                    {
                        ListenSession = new EpisodeListenSessionResponseDTO
                        {
                            // Token = newSession.Token,
                            Token = sessionToken,
                            PlaylistFileKey = playlistFileKey,
                            // LastListenDurationSeconds = 0,
                            PodcastEpisodeListenSession = new PodcastEpisodeListenSessionSnippetResponseDTO
                            {
                                Id = newSession.Id,
                                LastListenDurationSeconds = newSession.LastListenDurationSeconds
                            },
                            PodcastEpisode = new PodcastEpisodeSnippetResponseDTO
                            {
                                Id = validEpisode.Id,
                                Name = validEpisode.Name,
                                Description = validEpisode.Description,
                                MainImageFileKey = validEpisode.MainImageFileKey,
                                IsReleased = validEpisode.IsReleased,
                                ReleaseDate = validEpisode.ReleaseDate,
                                AudioLength = validEpisode.AudioLength
                            },
                            Podcaster = new AccountSnippetResponseDTO
                            {
                                Id = podcaster.Id,
                                Email = podcaster.Email,
                                FullName = podcaster.PodcasterProfileName,
                                MainImageFileKey = podcaster.MainImageFileKey
                            },
                            AudioFileUrl = deviceInfo.Platform == DevicePlatform.ios.ToString() || deviceInfo.Platform == DevicePlatform.android.ToString()
                            ? await _fileIOHelper.GeneratePresignedUrlAsync(
                                validEpisode.AudioFileKey, _podcastListenSessionConfig.SessionAudioUrlExpirationSeconds
                            )
                            : null
                        },
                        ListenSessionProcedure = newProcedure
                    };

                }
                catch (Exception ex)
                {
                    if (!transactionCompleted)
                    {
                        await transaction.RollbackAsync();
                    }
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("An error occurred while processing your request, error: " + ex.Message);
                }
            }
        }

        public async Task<EpisodeListenResponseDTO> NavigateEpisodeListenSessionAsync(int listenerAccountId, ListenSessionNavigateTypeEnum listenSessionNavigateType, EpisodeListenSessionNavigateRequestDTO episodeListenSessionNavigateRequestDTO, DeviceInfoDTO deviceInfo)
        {
            // /navigate : refresh lại procedure hiện tại
            // đánh completed = false cho procedure hiện tại và đánh completed mọi procedure cũ (ngoài trừ trường hợp còn lại duy nhất 1 item islistenable = true trong order và item đó là item hiện tại, thì không làm gì cả)
            // tạo listensession mới từ id chọn ra từ next hoặc previous theo EpisodeId của currentListensession là ListenObject trong procedure, sao khi có được navigate listenObjectId từ việc chọn sẽ tạo listensession với listenObjectId này (sẽ không tạo listensession tức là null khi không còn item nào đang IsListenable = true hoặc chỉ còn duy nhất 1 item và item đó là current item)
            // đánh completed mọi listensession cũ (nếu có listen session mới được tạo)
            // chạy all-user-listen-session-completion-flow (nếu có listen session mới được tạo)
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                bool transactionCompleted = false;
                try
                {
                    var account = await GetAccountById(listenerAccountId);

                    if (account == null)
                    {
                        throw new Exception("Listener with id " + listenerAccountId + " does not exist");
                    }

                    // STEP 1: Get current procedure
                    var currentProcedure = await _customerListenSessionProcedureCachingService.GetProcedureAsync(
                        listenerAccountId,
                        episodeListenSessionNavigateRequestDTO.CurrentListenSession.ListenSessionProcedureId
                    );

                    if (currentProcedure == null)
                    {
                        Console.WriteLine("[Navigate] Current procedure not found");
                        return new EpisodeListenResponseDTO
                        {
                            ListenSession = null,
                            ListenSessionProcedure = null
                        };
                    }

                    Console.WriteLine($"[Navigate] Current procedure mode: {currentProcedure.PlayOrderMode}, Direction: {listenSessionNavigateType.ToString()}");

                    // STEP 2: Validate current listen session
                    var currentListenSession = await _podcastEpisodeListenSessionGenericRepository.FindByIdAsync(
                        episodeListenSessionNavigateRequestDTO.CurrentListenSession.ListenSessionId
                    );

                    if (currentListenSession == null)
                    {
                        Console.WriteLine("[Navigate] Current listen session not found");
                        return new EpisodeListenResponseDTO
                        {
                            ListenSession = null,
                            ListenSessionProcedure = null
                        };
                    }
                    Console.WriteLine($"[Navigate] Current listen session episode id: {currentListenSession.PodcastEpisodeId}, listen object id: {currentListenSession.PodcastEpisodeId}");

                    // STEP 3: Xác định listenerBenefits để check NonQuota
                    List<int> listenerBenefits = new List<int>();

                    // Chỉ check benefit khi account.PodcastListenSlot == 0
                    if (account.PodcastListenSlot == 0 && currentProcedure.SourceDetail.Type == CustomerListenSessionProcedureSourceDetailTypeEnum.SpecifyShowEpisodes.ToString())
                    {
                        var currentBenefitList = episodeListenSessionNavigateRequestDTO.CurrentPodcastSubscriptionRegistrationBenefitList;

                        if (currentBenefitList != null && currentBenefitList.Count > 0)
                        {
                            listenerBenefits = currentBenefitList.Select(psb => psb.Id).ToList();

                            // Kiểm tra xem có benefit NonQuotaListening không
                            if (!listenerBenefits.Contains((int)PodcastSubscriptionBenefitEnum.NonQuotaListening))
                            {
                                throw new Exception("Listener does not have permission to listen to this episode, reason: PodcastListenSlot is 0 and no NonQuotaListening benefit");
                            }
                        }
                        else
                        {
                            throw new Exception("Listener does not have permission to listen to this episode, reason: PodcastListenSlot is 0 and no subscription benefits");
                        }
                    }




                    // STEP 4: Refresh procedure với current position
                    if (currentProcedure.SourceDetail.Type == CustomerListenSessionProcedureSourceDetailTypeEnum.SavedEpisodes.ToString())
                    {
                        currentProcedure = await RefreshEpisodeOrderForSavedEpisodesSourceAsync(
                            currentProcedure,
                            listenerAccountId,
                            currentListenSession.PodcastEpisodeId,
                            // episodeListenSessionNavigateRequestDTO.CurrentPodcastSubscriptionRegistrationBenefitList
                            null
                        );
                    }
                    else if (currentProcedure.SourceDetail.Type == CustomerListenSessionProcedureSourceDetailTypeEnum.SpecifyShowEpisodes.ToString())
                    {
                        currentProcedure = await RefreshEpisodeOrderForSpecifyShowEpisodesSourceAsync(
                            currentProcedure,
                            listenerAccountId,
                            currentListenSession.PodcastEpisodeId,
                            episodeListenSessionNavigateRequestDTO.CurrentPodcastSubscriptionRegistrationBenefitList
                        );
                    }

                    // STEP 5: Get order list theo mode
                    var orderList = currentProcedure.PlayOrderMode == CustomerListenSessionProcedurePlayOrderModeEnum.Sequential.ToString()
                        ? currentProcedure.ListenObjectsSequentialOrder
                        : currentProcedure.ListenObjectsRandomOrder;

                    if (orderList == null || orderList.Count == 0)
                    {
                        Console.WriteLine("[Navigate] Order list is empty");
                        return new EpisodeListenResponseDTO
                        {
                            ListenSession = null,
                            ListenSessionProcedure = currentProcedure
                        };
                    }

                    // foreach (var item in orderList)
                    // {
                    //     Console.WriteLine($"[Navigate] Order List Item - ListenObjectId: {item.ListenObjectId}, IsListenable: {item.IsListenable}");
                    // }

                    // STEP 6: Find current position
                    var currentIndex = orderList.FindIndex(item => item.ListenObjectId == currentListenSession.PodcastEpisodeId);

                    if (currentIndex == -1)
                    {
                        Console.WriteLine("[Navigate] Current episode not found in order");
                        return new EpisodeListenResponseDTO
                        {
                            ListenSession = null,
                            ListenSessionProcedure = currentProcedure
                        };
                    }

                    // STEP 7: Check if only one listenable item left
                    var listenableItems = orderList.Where(item => item.IsListenable).ToList();
                    // foreach (var item in listenableItems)
                    // {
                    //     Console.WriteLine($"[Navigate] Listenable Item - ListenObjectId: {item.ListenObjectId}");
                    // }

                    // Console.WriteLine($"[Navigate] Total listenable items count: {listenableItems.Count}, Current ListenObjectId: {currentListenSession.PodcastEpisodeId}, first listenable item id: {(listenableItems.Count > 0 ? listenableItems[0].ListenObjectId.ToString() : "N/A")}");

                    if (listenableItems.Count == 1 && listenableItems[0].ListenObjectId == currentListenSession.PodcastEpisodeId)
                    {
                        Console.WriteLine("[Navigate] Only one listenable item left (current item), no navigation possible");
                        // in ra thông tin vị trí hiện tại và item duy nhất
                        Console.WriteLine($"[Navigate] Current Index: {currentIndex}, ListenObjectId: {listenableItems[0].ListenObjectId}");

                        await _customerListenSessionProcedureCachingService.MarkAllProceduresCompletedAsync(listenerAccountId);
                        currentProcedure.IsCompleted = false;
                        await _customerListenSessionProcedureCachingService.UpdateProcedureAsync(
                            listenerAccountId,
                            currentProcedure.Id,
                            currentProcedure
                        );

                        return new EpisodeListenResponseDTO
                        {
                            ListenSession = null,
                            ListenSessionProcedure = currentProcedure
                        };
                    }

                    // STEP 8: Navigate to next/previous listenable item
                    Guid? nextListenObjectId = null;

                    if (listenSessionNavigateType == ListenSessionNavigateTypeEnum.Next)
                    {
                        Console.WriteLine("[Navigate] Searching for next listenable item (circular)...");

                        // Tìm từ vị trí hiện tại + 1 đến cuối list
                        for (int i = currentIndex + 1; i < orderList.Count; i++)
                        {
                            Console.WriteLine($"[Navigate] Checking item at index {i}, ListenObjectId: {orderList[i].ListenObjectId}, IsListenable: {orderList[i].IsListenable}");
                            if (orderList[i].IsListenable)
                            {
                                nextListenObjectId = orderList[i].ListenObjectId;
                                break;
                            }
                        }

                        // Nếu không tìm thấy, wrap around về đầu list (từ 0 đến currentIndex - 1)
                        if (nextListenObjectId == null)
                        {
                            Console.WriteLine("[Navigate] No item found after current, wrapping to start...");
                            for (int i = 0; i < currentIndex; i++)
                            {
                                Console.WriteLine($"[Navigate] Checking item at index {i}, ListenObjectId: {orderList[i].ListenObjectId}, IsListenable: {orderList[i].IsListenable}");
                                if (orderList[i].IsListenable)
                                {
                                    nextListenObjectId = orderList[i].ListenObjectId;
                                    break;
                                }
                            }
                        }
                    }
                    else if (listenSessionNavigateType == ListenSessionNavigateTypeEnum.Previous)
                    {
                        Console.WriteLine("[Navigate] Searching for previous listenable item (circular)...");

                        // Tìm từ vị trí hiện tại - 1 về đầu list
                        for (int i = currentIndex - 1; i >= 0; i--)
                        {
                            Console.WriteLine($"[Navigate] Checking item at index {i}, ListenObjectId: {orderList[i].ListenObjectId}, IsListenable: {orderList[i].IsListenable}");
                            if (orderList[i].IsListenable)
                            {
                                nextListenObjectId = orderList[i].ListenObjectId;
                                break;
                            }
                        }

                        // Nếu không tìm thấy, wrap around về cuối list (từ cuối về currentIndex + 1)
                        if (nextListenObjectId == null)
                        {
                            Console.WriteLine("[Navigate] No item found before current, wrapping to end...");
                            for (int i = orderList.Count - 1; i > currentIndex; i--)
                            {
                                Console.WriteLine($"[Navigate] Checking item at index {i}, ListenObjectId: {orderList[i].ListenObjectId}, IsListenable: {orderList[i].IsListenable}");
                                if (orderList[i].IsListenable)
                                {
                                    nextListenObjectId = orderList[i].ListenObjectId;
                                    break;
                                }
                            }
                        }
                    }

                    // STEP 9: Nếu không tìm thấy item listenable nào
                    if (nextListenObjectId == null)
                    {
                        Console.WriteLine($"[Navigate] No listenable item found in {listenSessionNavigateType.ToString()} direction");

                        await _customerListenSessionProcedureCachingService.MarkAllProceduresCompletedAsync(listenerAccountId);
                        currentProcedure.IsCompleted = false;
                        await _customerListenSessionProcedureCachingService.UpdateProcedureAsync(
                            listenerAccountId,
                            currentProcedure.Id,
                            currentProcedure
                        );

                        return new EpisodeListenResponseDTO
                        {
                            ListenSession = null,
                            ListenSessionProcedure = currentProcedure
                        };
                    }

                    Console.WriteLine($"[Navigate] Found next episode: {nextListenObjectId}");

                    // STEP 10: Get next episode info (KHÔNG validate permission vì refresh đã làm rồi)
                    var nextEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(nextListenObjectId.Value,
                        includeFunc: q => q
                            .Include(pe => pe.PodcastShow)
                                .ThenInclude(ps => ps.PodcastCategory)
                            .Include(pe => pe.PodcastShow)
                                .ThenInclude(ps => ps.PodcastSubCategory)

                    );

                    if (nextEpisode == null)
                    {
                        throw new Exception("Next episode not found");
                    }

                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(nextEpisode.PodcastShow.PodcasterId);


                    // STEP 11: Complete old sessions
                    var oldSessions = await _podcastEpisodeListenSessionGenericRepository.FindAll(
                        predicate: pes => pes.AccountId == listenerAccountId && pes.IsCompleted == false,
                        includeFunc: null
                    ).ToListAsync();

                    foreach (var oldSession in oldSessions)
                    {
                        oldSession.IsCompleted = true;
                        await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(oldSession.Id, oldSession);
                    }

                    // STEP 12: Create new listen session
                    var newSession = new PodcastEpisodeListenSession
                    {
                        Id = Guid.NewGuid(),
                        AccountId = listenerAccountId,
                        PodcastEpisodeId = nextEpisode.Id,
                        PodcastCategoryId = nextEpisode.PodcastShow.PodcastCategoryId,
                        PodcastSubCategoryId = nextEpisode.PodcastShow.PodcastSubCategoryId,
                        IsContentRemoved = false,
                        ExpiredAt = _dateHelper.GetNowByAppTimeZone().AddMinutes(_podcastListenSessionConfig.SessionExpirationMinutes)
                    };

                    await _podcastEpisodeListenSessionGenericRepository.CreateAsync(newSession);

                    var sessionToken = GenerateEpisodeListenHlsEnckeyRequestToken(newSession.Id);
                    await _podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository.CreateAsync(new PodcastEpisodeListenSessionHlsEnckeyRequestToken
                    {
                        PodcastEpisodeListenSessionId = newSession.Id,
                        Token = sessionToken,
                        IsUsed = false,
                    });

                    // STEP 13: Update listen count
                    await UpdateListenCountAsync(nextEpisode, account, podcaster, listenerBenefits);

                    // STEP 14: Mark tất cả procedures khác là completed
                    await _customerListenSessionProcedureCachingService.MarkAllProceduresCompletedAsync(listenerAccountId);

                    // STEP 15: Update procedure hiện tại về IsCompleted = false (active)
                    currentProcedure.IsCompleted = false;
                    await _customerListenSessionProcedureCachingService.UpdateProcedureAsync(
                        listenerAccountId,
                        currentProcedure.Id,
                        currentProcedure
                    );

                    await transaction.CommitAsync();
                    transactionCompleted = true;

                    // STEP 16: Run completion flow
                    JObject requestData = new JObject
                    {
                        ["AccountId"] = listenerAccountId,
                        ["IsEpisodeListenSessionCompleted"] = false,
                        ["IsBookingProducingListenSessionCompleted"] = true,
                    };

                    var startSagaTriggerMessage = _kafkaProducerService.PrepareStartSagaTriggerMessage("content-management-domain", requestData, null, "all-user-listen-session-completion-flow");
                    await _messagingService.SendSagaMessageAsync(startSagaTriggerMessage);

                    // STEP 17: Return response
                    var playlistFileKey = FilePathHelper.CombinePaths(
                        _filePathConfig.PODCAST_EPISODE_FILE_PATH,
                        nextEpisode.Id.ToString(),
                        "playlist",
                        _hlsConfig.PlaylistFileName
                    );

                    return new EpisodeListenResponseDTO
                    {
                        ListenSession = new EpisodeListenSessionResponseDTO
                        {
                            Token = sessionToken,
                            PlaylistFileKey = playlistFileKey,
                            PodcastEpisodeListenSession = new PodcastEpisodeListenSessionSnippetResponseDTO
                            {
                                Id = newSession.Id,
                                LastListenDurationSeconds = newSession.LastListenDurationSeconds
                            },
                            PodcastEpisode = new PodcastEpisodeSnippetResponseDTO
                            {
                                Id = nextEpisode.Id,
                                Name = nextEpisode.Name,
                                Description = nextEpisode.Description,
                                MainImageFileKey = nextEpisode.MainImageFileKey,
                                IsReleased = nextEpisode.IsReleased,
                                ReleaseDate = nextEpisode.ReleaseDate,
                                AudioLength = nextEpisode.AudioLength
                            },
                            Podcaster = new AccountSnippetResponseDTO
                            {
                                Id = podcaster.Id,
                                Email = podcaster.Email,
                                FullName = podcaster.PodcasterProfileName,
                                MainImageFileKey = podcaster.MainImageFileKey
                            },
                            AudioFileUrl = deviceInfo.Platform == DevicePlatform.ios.ToString() || deviceInfo.Platform == DevicePlatform.android.ToString()
                                ? await _fileIOHelper.GeneratePresignedUrlAsync(nextEpisode.AudioFileKey, _podcastListenSessionConfig.SessionAudioUrlExpirationSeconds)
                                : null
                        },
                        ListenSessionProcedure = currentProcedure
                    };
                }
                catch (Exception ex)
                {
                    if (!transactionCompleted)
                    {
                        await transaction.RollbackAsync();
                    }
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("An error occurred while navigating episode listen session, error: " + ex.Message);
                }
            }
        }

        // hàm refresh order của episode đối với nguồn SavedEpisodes (order by createdAt trong danh sách AccountSavedPodcastEpisode) 
        // Mode là CustomerListenSessionProcedurePlayOrderModeEnum.Sequencial sẽ refreh trên ListenObjectsSequentialOrder và Random sẽ refresh trên ListenObjectsRandomOrder
        // truyền vào List<PodcastSubscriptionBenefitDTO> listenerCurrentPodcastSubscriptionRegistrationBenefitList để làm điều kiện cho field IsListenable ở các item trong order
        // truyền vào CustomerListenSessionProcedure (lấy ra PlayOrderMode để xác định mục đích order, và SourceDetail để xác định nguồn là SavedEpisodes hay SpecifyShowEpisodes để làm cơ sở truy vấn, và order hiện tại để làm cơ sở modify lại (chỉ thêm và không xoá bớt, nếu có rồi thì cập nhật lại IsListenable))
        // truy vấn các episode từ nguồn (đảm bảo cả 3 level đều Publish và không cái nào bị xoá), sau đó kiểu tra điều kiện nghe của từng cái bằng hàm CheckListenerCanListenToEpisodeAsync(không check NonQuota tại hàm này vì lí do business rule), sau đó refresh lại order đang có , nếu có rồi thì chỉ cần đánh dấu lại IsListenable tuỳ theo điều kiện nghe của episode, nếu chưa có thể append theo đúng logic của PlayOrderMode
        // truyền vào vị trí order đang đứng hiện tại (truyền vào ListenObjectId đang đứng ở hiện ) (tham số này chỉ dùng để làm việc với mode random, nếu có vị trí đứng thì các order item mới không có trong mảng order cũ sẽ phải được random bố trí vào 1 vị trí nào đó ở đằng sau vị trí đứng , nếu vị trí đứng là ListenObject order cuối cùng thì sẽ random vào 1 vị trí nào đó ở đằng trước, nếu không có vị trí đứng thì random toàn bộ. nếu mode là sequencial thì không cần dùng tham số này. nếu trường hợp phải bố trí các item mới vào đằng trước/sau thì phải bố trí theo kiểu đẩy vào 1 order bất kì chứ không phải suffle lại toàn bộ đằng trước/sau với item mới thêm)
        // Đối với Sequencial thì xếp theo (season number asc, episode order asc, created at asc)
        // ĐỐi với random sẽ chỉ random toà bộ khi không có vị trí đứng hiện, ngược lại sẽ random bố trí vào trước/sau tuy vào việc vị trí đứng có phải là cuối cùng hay không
        // hàm này lấy ra tất cả các episode đang publish ở cả 3 level (channel level, show level, episode level) trong danh sách episode của soure type và đặt IsListenable theo điều kiện nghe của listener
        public async Task<CustomerListenSessionProcedure> RefreshEpisodeOrderForSavedEpisodesSourceAsync(
    CustomerListenSessionProcedure customerListenSessionProcedure,
    int listenerAccountId,
    Guid? currentListenObjectId,
    List<PodcastSubscriptionBenefitDTO>? listenerCurrentPodcastSubscriptionRegistrationBenefitList = null)
        {
            try
            {
                Console.WriteLine($"[RefreshSavedEpisodes] Start for listener {listenerAccountId}, mode: {customerListenSessionProcedure.PlayOrderMode}");

                // STEP 1: Validate source type
                if (customerListenSessionProcedure.SourceDetail.Type != CustomerListenSessionProcedureSourceDetailTypeEnum.SavedEpisodes.ToString())
                {
                    throw new Exception($"Invalid source type: {customerListenSessionProcedure.SourceDetail.Type}. Expected SavedEpisodes.");
                }

                // STEP 2: Query saved episodes từ UserService
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
            {
                new BatchQueryItem
                {
                    Key = "savedEpisodes",
                    QueryType = "findall",
                    EntityType = "AccountSavedPodcastEpisode",
                    Parameters = JObject.FromObject(new
                    {
                        where = new
                        {
                            AccountId = listenerAccountId
                        },
                        orderBy = "CreatedAt"
                    }),
                    Fields = new[] { "AccountId", "PodcastEpisodeId", "CreatedAt" }
                }
            }
                };

                var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
                var savedEpisodes = ((JArray)result.Results["savedEpisodes"])
                    .ToObject<List<AccountSavedPodcastEpisodeDTO>>();

                if (savedEpisodes == null || savedEpisodes.Count == 0)
                {
                    Console.WriteLine("[RefreshSavedEpisodes] No saved episodes found");
                    return customerListenSessionProcedure;
                }

                var episodeIds = savedEpisodes.Select(se => se.PodcastEpisodeId).ToList();
                Console.WriteLine($"[RefreshSavedEpisodes] Found {episodeIds.Count} saved episodes");

                // STEP 3: Query episodes với 3-level validation
                var episodes = await _podcastEpisodeGenericRepository.FindAll(
                    predicate: pe => episodeIds.Contains(pe.Id)
                        && pe.DeletedAt == null
                        && pe.PodcastShow.DeletedAt == null
                        && (pe.PodcastShow.PodcastChannel == null || pe.PodcastShow.PodcastChannel.DeletedAt == null),
                    includeFunc: q => q
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                            .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                            .ThenInclude(ps => ps.PodcastChannel)
                                .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                ).ToListAsync();

                // STEP 4: Filter Published status
                var publishedEpisodes = episodes.Where(pe =>
                {
                    var episodeStatus = pe.PodcastEpisodeStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastEpisodeStatusId;

                    var showStatus = pe.PodcastShow.PodcastShowStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastShowStatusId;

                    var channelValid = pe.PodcastShow.PodcastChannel == null ||
                        (pe.PodcastShow.PodcastChannel.DeletedAt == null &&
                         pe.PodcastShow.PodcastChannel.PodcastChannelStatusTrackings
                            .OrderByDescending(t => t.CreatedAt)
                            .FirstOrDefault()?.PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published);

                    return episodeStatus == (int)PodcastEpisodeStatusEnum.Published
                        && showStatus == (int)PodcastShowStatusEnum.Published
                        && channelValid;
                }).ToList();

                Console.WriteLine($"[RefreshSavedEpisodes] {publishedEpisodes.Count} published episodes after 3-level validation");

                // STEP 5: Check IsListenable cho từng episode
                var episodesWithListenability = new List<(PodcastEpisode episode, bool isListenable, DateTime createdAt)>();

                // nếu listenerCurrentPodcastSubscriptionRegistrationBenefitList = null nghĩa là người dùng đang gọi từ navigate trong soure SavedEpisodes
                // lúc này cần phải query các gói đăng kí của người dùng ở mọi episode khác vì source saved episodes không làm việc trên 1 specify show nên không thể có gói cố định gửi vào
                UserPodcastSubscriptionRegistrationEpisodeBaseQueryResponseDTO userPodcastSubscriptionRegistrationEpisodeBaseQueryResponseDTO = null;
                if (listenerCurrentPodcastSubscriptionRegistrationBenefitList == null)
                {
                    UserPodcastSubscriptionRegistrationEpisodeBaseQueryRequestDTO userPodcastSubscriptionRegistrationEpisodeBaseQueryRequestDTO = new UserPodcastSubscriptionRegistrationEpisodeBaseQueryRequestDTO
                    {
                        EpisodeBaseSourceInfoList = publishedEpisodes.Select(pe => new EpisodeBaseSourceInfoDTO
                        {
                            EpisodeId = pe.Id,
                            ShowId = pe.PodcastShowId,
                            ChannelId = pe.PodcastShow.PodcastChannel != null ? pe.PodcastShow.PodcastChannel.Id : (Guid?)null
                        }).ToList(),
                    };

                    userPodcastSubscriptionRegistrationEpisodeBaseQueryResponseDTO =
                           await _crossServiceHttpService.PostManualAsync<UserPodcastSubscriptionRegistrationEpisodeBaseQueryResponseDTO>(
                               serviceName: "SubscriptionService",
                               body: userPodcastSubscriptionRegistrationEpisodeBaseQueryRequestDTO,
                               relativePath: $"api/podcast-subscriptions/service-query/{listenerAccountId}"
                           );
                }

                // List<List<PodcastSubscriptionBenefitDTO>> mappedEpisodeBaseListenerBenefit = userPodcastSubscriptionRegistrationEpisodeBaseQueryResponseDTO.EpisodeBaseBenefitList.Select(e => e.PodcastSubscriptionBenefitIds.Select(id => new PodcastSubscriptionBenefitDTO { Id = id }).ToList()).ToList();

                foreach (var episode in publishedEpisodes)
                {
                    var savedEpisode = savedEpisodes.First(se => se.PodcastEpisodeId == episode.Id);

                    var canListen = await CheckListenerCanListenToEpisodeAsync(
                        listenerId: listenerAccountId,
                        validEpisode: episode,
                        isNonQuotaListeningCheck: true, // true vì lí do FE và BE đầu vào không thể chặng trước , nên sẽ chặng từ bước này , ví do là vì nguồn này không thuộc vào show cụ thể
                                                        // listenerCurrentPodcastSubscriptionRegistrationBenefitList: listenerCurrentPodcastSubscriptionRegistrationBenefitList
                                                        // listenerCurrentPodcastSubscriptionRegistrationBenefitList: userPodcastSubscriptionRegistrationEpisodeBaseQueryResponseDTO.EpisodeBaseBenefitList.First(e => e.EpisodeId == episode.Id).PodcastSubscriptionBenefitIds.Select(id => new PodcastSubscriptionBenefitDTO { Id = id }).ToList()
                        listenerCurrentPodcastSubscriptionRegistrationBenefitList: listenerCurrentPodcastSubscriptionRegistrationBenefitList != null ? listenerCurrentPodcastSubscriptionRegistrationBenefitList
                            : userPodcastSubscriptionRegistrationEpisodeBaseQueryResponseDTO.EpisodeBaseBenefitList
                                .First(e => e.EpisodeId == episode.Id)
                                .PodcastSubscriptionBenefitIds
                                .Select(id => new PodcastSubscriptionBenefitDTO { Id = id })
                                .ToList()
                    );

                    episodesWithListenability.Add((episode, canListen.CanListen, savedEpisode.CreatedAt));
                }

                // STEP 6: Get current order
                var currentOrder = customerListenSessionProcedure.PlayOrderMode == CustomerListenSessionProcedurePlayOrderModeEnum.Sequential.ToString()
                    ? customerListenSessionProcedure.ListenObjectsSequentialOrder ?? new List<ListenSessionProcedureListenObjectQueueItem>()
                    : customerListenSessionProcedure.ListenObjectsRandomOrder ?? new List<ListenSessionProcedureListenObjectQueueItem>();

                var existingIds = currentOrder.Select(o => o.ListenObjectId).ToHashSet();

                // Console.WriteLine($"[RefreshSavedEpisodes] CCCCCCCCurrent order count: {currentOrder.Count}, Existing IDs count: {existingIds.Count}");

                // STEP 7: Refresh order theo mode
                List<ListenSessionProcedureListenObjectQueueItem> refreshedOrder;

                if (customerListenSessionProcedure.PlayOrderMode == CustomerListenSessionProcedurePlayOrderModeEnum.Sequential.ToString())
                {
                    refreshedOrder = await RefreshSequentialOrderWithOrderForSavedEpisodesSource(
                        episodesWithListenability,
                        currentOrder,
                        existingIds
                    );
                    customerListenSessionProcedure.ListenObjectsSequentialOrder = refreshedOrder;
                    Console.WriteLine($"[RefreshSavedEpisodes] Final order count: {refreshedOrder.Count}");

                }
                else // Random
                {
                    refreshedOrder = RefreshRandomOrderWithOrderForSavedEpisodesSource(
                        episodesWithListenability,
                        currentOrder,
                        existingIds,
                        currentListenObjectId
                    );
                    customerListenSessionProcedure.ListenObjectsRandomOrder = refreshedOrder;
                    Console.WriteLine($"[RefreshSavedEpisodes] Final order count: {refreshedOrder.Count}");
                }

                return customerListenSessionProcedure;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RefreshSavedEpisodes] ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        #region Helper Methods for Order Refresh WITH Order field for SavedEpisodes Source

        private async Task<List<ListenSessionProcedureListenObjectQueueItem>> RefreshSequentialOrderWithOrderForSavedEpisodesSource(
    List<(PodcastEpisode episode, bool isListenable, DateTime createdAt)> episodesWithListenability,
    List<ListenSessionProcedureListenObjectQueueItem> currentOrder,
    HashSet<Guid> existingIds)
        {
            // STEP 1: Sort episodes còn tồn tại theo CreatedAt (từ AccountSavedPodcastEpisode)
            var sortedEpisodes = episodesWithListenability
                .OrderBy(e => e.createdAt)
                .ToList();

            // STEP 2: Tìm các items đã bị unsave (không còn trong episodesWithListenability)
            var sortedEpisodeIds = episodesWithListenability.Select(e => e.episode.Id).ToHashSet();
            var removedItems = currentOrder
                .Where(item => !sortedEpisodeIds.Contains(item.ListenObjectId))
                .OrderBy(item => item.Order) // Giữ nguyên thứ tự cũ của removed items
                .ToList();

            // STEP 3: Tạo order mới
            var newOrder = new List<ListenSessionProcedureListenObjectQueueItem>();

            // Thêm episodes còn tồn tại (sorted theo createdAt)
            foreach (var (episode, isListenable, createdAt) in sortedEpisodes)
            {
                newOrder.Add(new ListenSessionProcedureListenObjectQueueItem
                {
                    ListenObjectId = episode.Id,
                    Order = 0, // Temporary
                    IsListenable = isListenable
                });
            }

            // STEP 4: Append removed items vào cuối với IsListenable = false
            foreach (var removedItem in removedItems)
            {
                newOrder.Add(new ListenSessionProcedureListenObjectQueueItem
                {
                    ListenObjectId = removedItem.ListenObjectId,
                    Order = 0, // Temporary
                    IsListenable = false // Đánh dấu không thể nghe
                });
            }

            // STEP 5: Re-index Order liên tục (1, 2, 3, 4...)
            return newOrder.Select((item, index) => new ListenSessionProcedureListenObjectQueueItem
            {
                ListenObjectId = item.ListenObjectId,
                Order = index + 1,
                IsListenable = item.IsListenable
            }).ToList();
        }
        private List<ListenSessionProcedureListenObjectQueueItem> RefreshRandomOrderWithOrderForSavedEpisodesSource(
            List<(PodcastEpisode episode, bool isListenable, DateTime createdAt)> episodesWithListenability,
            List<ListenSessionProcedureListenObjectQueueItem> currentOrder,
            HashSet<Guid> existingIds,
            Guid? currentListenObjectId)
        {
            var random = new Random();
            var newOrder = new List<ListenSessionProcedureListenObjectQueueItem>(currentOrder);

            // STEP 1: Tìm valid episode IDs
            var validEpisodeIds = episodesWithListenability.Select(e => e.episode.Id).ToHashSet();

            // STEP 2: Update IsListenable cho tất cả items trong currentOrder
            foreach (var item in newOrder)
            {
                if (validEpisodeIds.Contains(item.ListenObjectId))
                {
                    // Episode còn tồn tại → Update IsListenable
                    var episode = episodesWithListenability.First(e => e.episode.Id == item.ListenObjectId);
                    item.IsListenable = episode.isListenable;
                }
                else
                {
                    // Episode đã bị removed (unsaved) → Mark IsListenable = false, GIỮ NGUYÊN VỊ TRÍ
                    item.IsListenable = false;
                }
            }

            // STEP 3: Tìm new episodes (chưa có trong currentOrder)
            var newEpisodes = episodesWithListenability
                .Where(e => !existingIds.Contains(e.episode.Id))
                .ToList();

            if (newEpisodes.Count == 0)
            {
                // Không có episodes mới → chỉ re-index
                return newOrder.Select((item, index) => new ListenSessionProcedureListenObjectQueueItem
                {
                    ListenObjectId = item.ListenObjectId,
                    Order = index + 1,
                    IsListenable = item.IsListenable
                }).ToList();
            }

            Console.WriteLine($"[RefreshRandomOrder] {newEpisodes.Count} new episodes to insert");

            // STEP 4: Xử lý insert logic
            // CASE 1: Procedure mới (currentOrder rỗng) → Full shuffle
            if (currentOrder.Count == 0)
            {
                Console.WriteLine("[RefreshRandomOrder] New procedure (empty order) → Full shuffle");

                foreach (var (episode, isListenable, _) in newEpisodes)
                {
                    newOrder.Add(new ListenSessionProcedureListenObjectQueueItem
                    {
                        ListenObjectId = episode.Id,
                        Order = 0, // Temporary
                        IsListenable = isListenable
                    });
                }

                // Shuffle toàn bộ
                var shuffled = newOrder.OrderBy(x => random.Next()).ToList();
                return shuffled.Select((item, index) => new ListenSessionProcedureListenObjectQueueItem
                {
                    ListenObjectId = item.ListenObjectId,
                    Order = index + 1,
                    IsListenable = item.IsListenable
                }).ToList();
            }

            // CASE 2: Procedure cũ → LUÔN có currentListenObjectId
            if (currentListenObjectId == null || !existingIds.Contains(currentListenObjectId.Value))
            {
                throw new Exception("Invalid state: Existing procedure must have valid currentListenObjectId");
            }

            var currentIndex = newOrder.FindIndex(o => o.ListenObjectId == currentListenObjectId.Value);
            Console.WriteLine($"[RefreshRandomOrder] Current index of listenObjectId {currentListenObjectId.Value}: {currentIndex}");

            if (currentIndex == -1)
            {
                throw new Exception("Current position not found in order");
            }

            bool isLastPosition = currentIndex == newOrder.Count - 1;
            Console.WriteLine($"[RefreshRandomOrder] Current position index: {currentIndex}/{newOrder.Count}, isLast: {isLastPosition}");

            if (isLastPosition)
            {
                // ✅ Last position → INSERT vào TRƯỚC
                Console.WriteLine("[RefreshRandomOrder] Last position → Insert BEFORE current");

                foreach (var (episode, isListenable, _) in newEpisodes)
                {
                    var insertIndex = currentIndex > 0
                        ? random.Next(0, currentIndex)
                        : 0;

                    newOrder.Insert(insertIndex, new ListenSessionProcedureListenObjectQueueItem
                    {
                        ListenObjectId = episode.Id,
                        Order = 0,
                        IsListenable = isListenable
                    });

                    currentIndex++; // Update vị trí current vì insert vào trước
                }
            }
            else
            {
                // ✅ Not last → INSERT vào SAU
                Console.WriteLine("[RefreshRandomOrder] Not last position → Insert AFTER current");

                foreach (var (episode, isListenable, _) in newEpisodes)
                {
                    var insertIndex = random.Next(currentIndex + 1, newOrder.Count + 1);

                    newOrder.Insert(insertIndex, new ListenSessionProcedureListenObjectQueueItem
                    {
                        ListenObjectId = episode.Id,
                        Order = 0,
                        IsListenable = isListenable
                    });
                }
            }

            // Re-index Order liên tục (1, 2, 3, 4...)
            return newOrder.Select((item, index) => new ListenSessionProcedureListenObjectQueueItem
            {
                ListenObjectId = item.ListenObjectId,
                Order = index + 1,
                IsListenable = item.IsListenable
            }).ToList();
        }
        #endregion


        // hàm refresh order của episode đối với nguồn SpecifyShowEpisodes (order by episode season number tới episode order tới created at , vì có thể có sự trùng nhau về seansonnumer hoặc episode order giữa những episode) (truyền vào chế độ là CustomerListenSessionProcedureSPlayOrderModeEnum.Sequencial hoặc Random)
        public async Task<CustomerListenSessionProcedure> RefreshEpisodeOrderForSpecifyShowEpisodesSourceAsync(
            CustomerListenSessionProcedure customerListenSessionProcedure,
            int listenerAccountId,
            Guid? currentListenObjectId,
            List<PodcastSubscriptionBenefitDTO> listenerCurrentPodcastSubscriptionRegistrationBenefitList)
        {
            try
            {
                Console.WriteLine($"[RefreshSpecifyShowEpisodes] Start for listener {listenerAccountId}, mode: {customerListenSessionProcedure.PlayOrderMode}");

                // STEP 1: Validate source type
                if (customerListenSessionProcedure.SourceDetail.Type != CustomerListenSessionProcedureSourceDetailTypeEnum.SpecifyShowEpisodes.ToString())
                {
                    throw new Exception($"Invalid source type: {customerListenSessionProcedure.SourceDetail.Type}. Expected SpecifyShowEpisodes.");
                }

                // STEP 2: Validate PodcastShow info in SourceDetail
                if (customerListenSessionProcedure.SourceDetail.PodcastShow == null)
                {
                    throw new Exception("PodcastShow info is missing in SourceDetail");
                }

                var podcastShowId = customerListenSessionProcedure.SourceDetail.PodcastShow.Id;

                // STEP 3: Query episodes từ PodcastShow với 3-level validation
                var episodes = await _podcastEpisodeGenericRepository.FindAll(
                    predicate: pe => pe.PodcastShowId == podcastShowId
                        && pe.DeletedAt == null
                        && pe.PodcastShow.DeletedAt == null
                        && (pe.PodcastShow.PodcastChannel == null || pe.PodcastShow.PodcastChannel.DeletedAt == null),
                    includeFunc: q => q
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                            .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                            .ThenInclude(ps => ps.PodcastChannel)
                                .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                ).ToListAsync();

                // STEP 4: Filter Published status
                var publishedEpisodes = episodes.Where(pe =>
                {
                    var episodeStatus = pe.PodcastEpisodeStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastEpisodeStatusId;

                    var showStatus = pe.PodcastShow.PodcastShowStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastShowStatusId;

                    var channelValid = pe.PodcastShow.PodcastChannel == null ||
                        (pe.PodcastShow.PodcastChannel.DeletedAt == null &&
                         pe.PodcastShow.PodcastChannel.PodcastChannelStatusTrackings
                            .OrderByDescending(t => t.CreatedAt)
                            .FirstOrDefault()?.PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published);

                    return episodeStatus == (int)PodcastEpisodeStatusEnum.Published
                        && showStatus == (int)PodcastShowStatusEnum.Published
                        && channelValid;
                }).ToList();

                Console.WriteLine($"[RefreshSpecifyShowEpisodes] {publishedEpisodes.Count} published episodes after 3-level validation");

                // STEP 5: Check IsListenable cho từng episode
                var episodesWithListenability = new List<(PodcastEpisode episode, bool isListenable, DateTime createdAt)>();

                foreach (var episode in publishedEpisodes)
                {
                    var canListen = await CheckListenerCanListenToEpisodeAsync(
                        listenerId: listenerAccountId,
                        validEpisode: episode,
                        isNonQuotaListeningCheck: false,
                        listenerCurrentPodcastSubscriptionRegistrationBenefitList: listenerCurrentPodcastSubscriptionRegistrationBenefitList
                    );

                    episodesWithListenability.Add((episode, canListen.CanListen, episode.CreatedAt));
                }

                // STEP 6: Get current order
                var currentOrder = customerListenSessionProcedure.PlayOrderMode == CustomerListenSessionProcedurePlayOrderModeEnum.Sequential.ToString()
                    ? customerListenSessionProcedure.ListenObjectsSequentialOrder ?? new List<ListenSessionProcedureListenObjectQueueItem>()
                    : customerListenSessionProcedure.ListenObjectsRandomOrder ?? new List<ListenSessionProcedureListenObjectQueueItem>();

                var existingIds = currentOrder.Select(o => o.ListenObjectId).ToHashSet();

                // STEP 7: Refresh order theo mode
                List<ListenSessionProcedureListenObjectQueueItem> refreshedOrder;

                if (customerListenSessionProcedure.PlayOrderMode == CustomerListenSessionProcedurePlayOrderModeEnum.Sequential.ToString())
                {
                    refreshedOrder = await RefreshSequentialOrderWithOrderForSpecifyShowEpisodesSource(
                        episodesWithListenability,
                        currentOrder,
                        existingIds
                    );
                    customerListenSessionProcedure.ListenObjectsSequentialOrder = refreshedOrder;
                    Console.WriteLine($"[RefreshSpecifyShowEpisodes] Final order count: {refreshedOrder.Count}");

                }
                else if (customerListenSessionProcedure.PlayOrderMode == CustomerListenSessionProcedurePlayOrderModeEnum.Random.ToString())
                {
                    refreshedOrder = RefreshRandomOrderWithOrderForSpecifyShowEpisodesSource(
                        episodesWithListenability,
                        currentOrder,
                        existingIds,
                        currentListenObjectId
                    );
                    customerListenSessionProcedure.ListenObjectsRandomOrder = refreshedOrder;
                    Console.WriteLine($"[RefreshSpecifyShowEpisodes] Final order count: {refreshedOrder.Count}");

                }

                return customerListenSessionProcedure;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RefreshSpecifyShowEpisodes] ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        #region Helper Methods for Order Refresh WITH Order field for SpecifyShowEpisodes Source

        private async Task<List<ListenSessionProcedureListenObjectQueueItem>> RefreshSequentialOrderWithOrderForSpecifyShowEpisodesSource(
            List<(PodcastEpisode episode, bool isListenable, DateTime createdAt)> episodesWithListenability,
            List<ListenSessionProcedureListenObjectQueueItem> currentOrder,
            HashSet<Guid> existingIds)
        {
            // STEP 1: Tìm các ID CŨ KHÔNG CÓ trong sortedEpisodes
            var sortedEpisodeIds = episodesWithListenability.Select(e => e.episode.Id).ToHashSet();
            var removedItemIds = currentOrder
                .Where(item => !sortedEpisodeIds.Contains(item.ListenObjectId))
                .Select(item => item.ListenObjectId)
                .ToList();

            // STEP 2: Query episodes bị removed để lấy sort info
            var removedEpisodesInfo = new List<(Guid id, int seasonNumber, int episodeOrder, DateTime createdAt)>();

            if (removedItemIds.Count > 0)
            {
                var removedEpisodes = await _podcastEpisodeGenericRepository.FindAll(
                    predicate: pe => removedItemIds.Contains(pe.Id),
                    includeFunc: null
                ).ToListAsync();

                // Map sang tuple để sort
                removedEpisodesInfo = removedEpisodes.Select(pe =>
                    (pe.Id, pe.SeasonNumber, pe.EpisodeOrder, pe.CreatedAt)
                ).ToList();
            }

            // STEP 3: Merge sortedEpisodes + removedEpisodes
            var allEpisodes = new List<(Guid id, int? seasonNumber, int? episodeOrder, DateTime createdAt, bool isListenable)>();

            // Thêm episodes có trong sortedEpisodes
            foreach (var (episode, isListenable, createdAt) in episodesWithListenability)
            {
                allEpisodes.Add((episode.Id, episode.SeasonNumber, episode.EpisodeOrder, createdAt, isListenable));
            }

            // Thêm removed episodes với IsListenable = false
            foreach (var (id, seasonNumber, episodeOrder, createdAt) in removedEpisodesInfo)
            {
                allEpisodes.Add((id, seasonNumber, episodeOrder, createdAt, isListenable: false));
            }

            // STEP 4: Sort toàn bộ theo SeasonNumber → EpisodeOrder → CreatedAt
            var sortedAll = allEpisodes
                .OrderBy(e => e.seasonNumber ?? int.MaxValue)
                .ThenBy(e => e.episodeOrder ?? int.MaxValue)
                .ThenBy(e => e.createdAt)
                .ToList();

            // STEP 5: Tạo final order với Order liên tục
            return sortedAll.Select((item, index) => new ListenSessionProcedureListenObjectQueueItem
            {
                ListenObjectId = item.id,
                Order = index + 1,
                IsListenable = item.isListenable
            }).ToList();
        }

        private List<ListenSessionProcedureListenObjectQueueItem> RefreshRandomOrderWithOrderForSpecifyShowEpisodesSource(
            List<(PodcastEpisode episode, bool isListenable, DateTime createdAt)> episodesWithListenability,
            List<ListenSessionProcedureListenObjectQueueItem> currentOrder,
            HashSet<Guid> existingIds,
            Guid? currentListenObjectId)
        {
            var random = new Random();
            var newOrder = new List<ListenSessionProcedureListenObjectQueueItem>(currentOrder);

            // STEP 1: Tìm valid episode IDs
            var validEpisodeIds = episodesWithListenability.Select(e => e.episode.Id).ToHashSet();

            // STEP 2: Update IsListenable cho tất cả items trong currentOrder
            foreach (var item in newOrder)
            {
                if (validEpisodeIds.Contains(item.ListenObjectId))
                {
                    // Episode còn tồn tại → Update IsListenable
                    var episode = episodesWithListenability.First(e => e.episode.Id == item.ListenObjectId);
                    item.IsListenable = episode.isListenable;
                }
                else
                {
                    // Episode đã bị removed (unpublished) → Mark IsListenable = false, GIỮ NGUYÊN VỊ TRÍ
                    item.IsListenable = false;
                }
            }

            // STEP 3: Tìm new episodes (chưa có trong currentOrder)
            var newEpisodes = episodesWithListenability
                .Where(e => !existingIds.Contains(e.episode.Id))
                .ToList();

            if (newEpisodes.Count == 0)
            {
                // Không có episodes mới → chỉ re-index
                return newOrder.Select((item, index) => new ListenSessionProcedureListenObjectQueueItem
                {
                    ListenObjectId = item.ListenObjectId,
                    Order = index + 1,
                    IsListenable = item.IsListenable
                }).ToList();
            }

            Console.WriteLine($"[RefreshRandomOrder] {newEpisodes.Count} new episodes to insert");

            // STEP 4: Xử lý insert logic
            // CASE 1: Procedure mới (currentOrder rỗng) → Full shuffle
            if (currentOrder.Count == 0)
            {
                Console.WriteLine("[RefreshRandomOrder] New procedure (empty order) → Full shuffle");

                foreach (var (episode, isListenable, _) in newEpisodes)
                {
                    newOrder.Add(new ListenSessionProcedureListenObjectQueueItem
                    {
                        ListenObjectId = episode.Id,
                        Order = 0,
                        IsListenable = isListenable
                    });
                }

                // Shuffle toàn bộ
                var shuffled = newOrder.OrderBy(x => random.Next()).ToList();
                return shuffled.Select((item, index) => new ListenSessionProcedureListenObjectQueueItem
                {
                    ListenObjectId = item.ListenObjectId,
                    Order = index + 1,
                    IsListenable = item.IsListenable
                }).ToList();
            }

            // CASE 2: Procedure cũ → LUÔN có currentListenObjectId
            if (currentListenObjectId == null || !existingIds.Contains(currentListenObjectId.Value))
            {
                throw new Exception("Invalid state: Existing procedure must have valid currentListenObjectId");
            }

            var currentIndex = newOrder.FindIndex(o => o.ListenObjectId == currentListenObjectId.Value);

            if (currentIndex == -1)
            {
                throw new Exception("Current position not found in order");
            }

            bool isLastPosition = currentIndex == newOrder.Count - 1;
            Console.WriteLine($"[RefreshRandomOrder] Current position index: {currentIndex}/{newOrder.Count}, isLast: {isLastPosition}");

            if (isLastPosition)
            {
                // ✅ Last position → INSERT vào TRƯỚC
                Console.WriteLine("[RefreshRandomOrder] Last position → Insert BEFORE current");

                foreach (var (episode, isListenable, _) in newEpisodes)
                {
                    var insertIndex = currentIndex > 0
                        ? random.Next(0, currentIndex)
                        : 0;

                    newOrder.Insert(insertIndex, new ListenSessionProcedureListenObjectQueueItem
                    {
                        ListenObjectId = episode.Id,
                        Order = 0,
                        IsListenable = isListenable
                    });

                    currentIndex++;
                }
            }
            else
            {
                // ✅ Not last → INSERT vào SAU
                Console.WriteLine("[RefreshRandomOrder] Not last position → Insert AFTER current");

                foreach (var (episode, isListenable, _) in newEpisodes)
                {
                    var insertIndex = random.Next(currentIndex + 1, newOrder.Count + 1);

                    newOrder.Insert(insertIndex, new ListenSessionProcedureListenObjectQueueItem
                    {
                        ListenObjectId = episode.Id,
                        Order = 0,
                        IsListenable = isListenable
                    });
                }
            }

            // Re-index Order liên tục (1, 2, 3, 4...)
            return newOrder.Select((item, index) => new ListenSessionProcedureListenObjectQueueItem
            {
                ListenObjectId = item.ListenObjectId,
                Order = index + 1,
                IsListenable = item.IsListenable
            }).ToList();
        }
        #endregion

        public async Task<byte[]> GetEpisodeHlsEncryptionKeyFileAsync(Guid episodeId, Guid keyId, string? token = null)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (token == null)
                    {
                        throw new Exception("Token is required");
                    }
                    // Check jwt đã hết hạn chưa và có tương đồng với jwt đang lưu trong session hay không
                    var isValidToken = _jwtHelper.DecodeToken_OneSecretKey(token, _podcastListenSessionConfig.TokenSecretKey);

                    var sessionId = isValidToken.FindFirst("SessionId")?.Value;

                    var existingPodcastEpisodeListenSessionHlsEnckeyRequestToken = await _podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository.FindAll(
                        predicate: pelsert => pelsert.Token == token && pelsert.PodcastEpisodeListenSessionId.ToString() == sessionId,
                        includeFunc: null
                    ).FirstOrDefaultAsync();

                    if (existingPodcastEpisodeListenSessionHlsEnckeyRequestToken == null)
                    {
                        throw new Exception("Invalid token");
                    }
                    if (existingPodcastEpisodeListenSessionHlsEnckeyRequestToken.IsUsed == true)
                    {
                        throw new Exception("Token has been used");
                    }

                    var session = await _podcastEpisodeListenSessionGenericRepository.FindByIdAsync(
                        id: Guid.Parse(sessionId),
                        includeFunc: null
                    );
                    if (session == null)
                    {
                        throw new Exception("Session does not exist");
                    }
                    // else if (session.Token != token)
                    // {
                    //     throw new Exception("Token does not match the session");
                    // }
                    else if (session.IsCompleted == true)
                    {
                        throw new Exception("Session has been completed");
                    }

                    // đánh dấu token đã được sử dụng
                    // var newSessionToken = GenerateEpisodeListenToken(session.Id, true);
                    // session.Token = newSessionToken;
                    // await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(session.Id, session);
                    var existingTokenRecord = await _podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository.FindAll(
                        predicate: pelsert => pelsert.Token == token && pelsert.PodcastEpisodeListenSessionId == session.Id,
                        includeFunc: null
                    ).FirstOrDefaultAsync();
                    existingTokenRecord.IsUsed = true;
                    await _unitOfWork.PodcastEpisodeListenSessionHlsEnckeyRequestTokenRepository.IsUsedUpdateByTokenAndSessionIdAsync(token, session.Id, true);

                    var episode = await _podcastEpisodeGenericRepository.FindAll(
                        predicate: pe => pe.Id == episodeId && pe.DeletedAt == null && pe.AudioEncryptionKeyId == keyId,
                        includeFunc: pe => pe.Include(p => p.PodcastEpisodeStatusTrackings)
                    ).FirstOrDefaultAsync();

                    if (episode == null)
                    {
                        throw new Exception("Podcast episode with id " + episodeId + " does not exist, or keyId does not match");
                    }
                    else if (episode.DeletedAt != null)
                    {
                        throw new Exception("Podcast episode with id " + episodeId + " has been deleted");
                    }
                    else if (episode.PodcastEpisodeStatusTrackings
                        .OrderByDescending(pet => pet.CreatedAt)
                        .FirstOrDefault()
                        .PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Published)
                    {
                        throw new Exception("Podcast episode with id " + episodeId + " is not in Published status");
                    }

                    await transaction.CommitAsync();

                    return await _fileIOHelper.GetFileBytesAsync(episode.AudioEncryptionKeyFileKey);



                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new HttpRequestException("An error occurred while processing your request, error: " + ex.Message);
                }
            }

        }

        public async Task<bool> IsAudioFileOwnedByPodcasterAsync(string audioFileKey, int podcasterId)
        {
            var episode = await _podcastEpisodeGenericRepository.FindAll(
                predicate: pe => pe.AudioFileKey == audioFileKey && pe.DeletedAt == null && pe.PodcastShow.DeletedAt == null && (
                    pe.PodcastShow.PodcastChannel == null || pe.PodcastShow.PodcastChannel.DeletedAt == null
                ) && pe.PodcastShow.PodcasterId == podcasterId,
                includeFunc: pe => pe.Include(p => p.PodcastShow)
            ).FirstOrDefaultAsync();

            if (episode == null)
            {
                Console.WriteLine("Audio file with key " + audioFileKey + " does not exist or podcast show/channel has been deleted");
                return false;
            }
            return true;
        }

        public async Task DiscardPodcastEpisodePublishReviewDmcaRemoveEpisodeForce(DiscardEpisodePublishReviewDmcaRemoveEpisodeForceParameterDTO discardEpisodePublishReviewDmcaRemoveEpisodeForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // PodcastEpisodePublishReviewSession discard status khác 2, 3, 4 (EpisodeId)
                    var session = await _podcastEpisodePublishReviewSessionGenericRepository.FindAll(
                        predicate: pe => pe.PodcastEpisodeId == discardEpisodePublishReviewDmcaRemoveEpisodeForceParameterDTO.PodcastEpisodeId,
                        // && pe.PodcastEpisodePublishReviewSessionStatusTrackings
                        // .OrderByDescending(pet => pet.CreatedAt)
                        // .FirstOrDefault()
                        // .PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview,
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodePublishReviewSessionStatusTrackings)
                    ).ToListAsync();

                    session = session.Where(s => s.PodcastEpisodePublishReviewSessionStatusTrackings
                        .OrderByDescending(pet => pet.CreatedAt)
                        .FirstOrDefault()
                        .PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview
                    ).ToList();

                    foreach (var s in session)
                    {
                        // tạo tracking mới với status Discard
                        var newTracking = new PodcastEpisodePublishReviewSessionStatusTracking
                        {
                            PodcastEpisodePublishReviewSessionId = s.Id,
                            PodcastEpisodePublishReviewSessionStatusId = (int)PodcastEpisodePublishReviewSessionStatusEnum.Discard,
                        };
                        await _podcastEpisodePublishReviewSessionStatusTrackingGenericRepository.CreateAsync(newTracking);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = discardEpisodePublishReviewDmcaRemoveEpisodeForceParameterDTO.PodcastEpisodeId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = discardEpisodePublishReviewDmcaRemoveEpisodeForceParameterDTO.PodcastEpisodeId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "discard-episode-publish-review-dmca-remove-episode-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);


                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Discard episode publish review dmca remove episode force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "discard-episode-publish-review-dmca-remove-episode-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DiscardEpisodePublishReviewEpisodeDeletionForce(DiscardEpisodePublishReviewEpisodeDeletionForceParameterDTO discardEpisodePublishReviewEpisodeDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // PodcastEpisodePublishReviewSession discard status khác 2, 3, 4 (EpisodeId)
                    var sessions = await _podcastEpisodePublishReviewSessionGenericRepository.FindAll(
                        predicate: pe => pe.PodcastEpisodeId == discardEpisodePublishReviewEpisodeDeletionForceParameterDTO.PodcastEpisodeId,
                        // && pe.PodcastEpisodePublishReviewSessionStatusTrackings
                        // .OrderByDescending(pet => pet.CreatedAt)
                        // .FirstOrDefault()
                        // .PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview,
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodePublishReviewSessionStatusTrackings)
                    ).ToListAsync();

                    sessions = sessions.Where(s => s.PodcastEpisodePublishReviewSessionStatusTrackings
                        .OrderByDescending(pet => pet.CreatedAt)
                        .FirstOrDefault()
                        .PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview
                    ).ToList();

                    foreach (var s in sessions)
                    {
                        // tạo tracking mới với status Discard
                        var newTracking = new PodcastEpisodePublishReviewSessionStatusTracking
                        {
                            PodcastEpisodePublishReviewSessionId = s.Id,
                            PodcastEpisodePublishReviewSessionStatusId = (int)PodcastEpisodePublishReviewSessionStatusEnum.Discard,
                        };
                        await _podcastEpisodePublishReviewSessionStatusTrackingGenericRepository.CreateAsync(newTracking);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = discardEpisodePublishReviewEpisodeDeletionForceParameterDTO.PodcastEpisodeId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = discardEpisodePublishReviewEpisodeDeletionForceParameterDTO.PodcastEpisodeId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "discard-episode-publish-review-episode-deletion-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Discard episode publish review episode deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "discard-episode-publish-review-episode-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DiscardPodcastShowEpisodesPublishReviewDmcaRemoveShowForce(DiscardShowEpisodesPublishReviewDmcaRemoveShowForceParameterDTO discardShowEpisodesPublishReviewDmcaRemoveShowForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // PodcastEpisodePublishReviewSession discard status khác 2, 3, 4 (EpisodeId)
                    var sessions = await _podcastEpisodePublishReviewSessionGenericRepository.FindAll(
                        predicate: pe => pe.PodcastEpisode.PodcastShowId == discardShowEpisodesPublishReviewDmcaRemoveShowForceParameterDTO.PodcastShowId,
                        // && pe.PodcastEpisodePublishReviewSessionStatusTrackings
                        // .OrderByDescending(pet => pet.CreatedAt)
                        // .FirstOrDefault()
                        // .PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview,
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodePublishReviewSessionStatusTrackings)
                    ).ToListAsync();

                    sessions = sessions.Where(s => s.PodcastEpisodePublishReviewSessionStatusTrackings
                        .OrderByDescending(pet => pet.CreatedAt)
                        .FirstOrDefault()
                        .PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview
                    ).ToList();

                    foreach (var s in sessions)
                    {
                        // tạo tracking mới với status Discard
                        var newTracking = new PodcastEpisodePublishReviewSessionStatusTracking
                        {
                            PodcastEpisodePublishReviewSessionId = s.Id,
                            PodcastEpisodePublishReviewSessionStatusId = (int)PodcastEpisodePublishReviewSessionStatusEnum.Discard,
                        };
                        await _podcastEpisodePublishReviewSessionStatusTrackingGenericRepository.CreateAsync(newTracking);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = discardShowEpisodesPublishReviewDmcaRemoveShowForceParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = discardShowEpisodesPublishReviewDmcaRemoveShowForceParameterDTO.PodcastShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "discard-show-episodes-publish-review-dmca-remove-show-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Discard show episode publish review dmca remove show force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "discard-show-episodes-publish-review-dmca-remove-show-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DiscardChannelEpisodesPublishReviewChannelDeletionForce(DiscardChannelEpisodesPublishReviewChannelDeletionForceParameterDTO discardChannelEpisodesPublishReviewChannelDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // PodcastEpisodePublishReviewSession discard status khác 2, 3, 4 (EpisodeId)
                    var sessions = await _podcastEpisodePublishReviewSessionGenericRepository.FindAll(
                        predicate: pe => pe.PodcastEpisode.PodcastShow.PodcastChannelId == discardChannelEpisodesPublishReviewChannelDeletionForceParameterDTO.PodcastChannelId,
                        // && pe.PodcastEpisodePublishReviewSessionStatusTrackings
                        // .OrderByDescending(pet => pet.CreatedAt)
                        // .FirstOrDefault()
                        // .PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview,
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodePublishReviewSessionStatusTrackings)
                    ).ToListAsync();

                    sessions = sessions.Where(s => s.PodcastEpisodePublishReviewSessionStatusTrackings
                        .OrderByDescending(pet => pet.CreatedAt)
                        .FirstOrDefault()
                        .PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview
                    ).ToList();

                    foreach (var s in sessions)
                    {
                        // tạo tracking mới với status Discard
                        var newTracking = new PodcastEpisodePublishReviewSessionStatusTracking
                        {
                            PodcastEpisodePublishReviewSessionId = s.Id,
                            PodcastEpisodePublishReviewSessionStatusId = (int)PodcastEpisodePublishReviewSessionStatusEnum.Discard,
                        };
                        await _podcastEpisodePublishReviewSessionStatusTrackingGenericRepository.CreateAsync(newTracking);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = discardChannelEpisodesPublishReviewChannelDeletionForceParameterDTO.PodcastChannelId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = discardChannelEpisodesPublishReviewChannelDeletionForceParameterDTO.PodcastChannelId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "discard-channel-episodes-publish-review-channel-deletion-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Discard channel episode publish review channel deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "discard-channel-episodes-publish-review-channel-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DiscardPodcasterEpisodesPublishReviewTerminatePodcasterForce(DiscardPodcasterEpisodesPublishReviewTerminatePodcasterForceParameterDTO discardPodcasterEpisodesPublishReviewTerminatePodcasterForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // PodcastEpisodePublishReviewSession discard status khác 2, 3, 4 (EpisodeId)
                    var sessions = await _podcastEpisodePublishReviewSessionGenericRepository.FindAll(
                        predicate: pe => pe.PodcastEpisode.PodcastShow.PodcasterId == discardPodcasterEpisodesPublishReviewTerminatePodcasterForceParameterDTO.PodcasterId,
                        // && pe.PodcastEpisodePublishReviewSessionStatusTrackings
                        // .OrderByDescending(pet => pet.CreatedAt)
                        // .FirstOrDefault()
                        // .PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview,
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodePublishReviewSessionStatusTrackings)
                    ).ToListAsync();

                    sessions = sessions.Where(s => s.PodcastEpisodePublishReviewSessionStatusTrackings
                        .OrderByDescending(pet => pet.CreatedAt)
                        .FirstOrDefault()
                        .PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview
                    ).ToList();

                    foreach (var s in sessions)
                    {
                        // tạo tracking mới với status Discard
                        var newTracking = new PodcastEpisodePublishReviewSessionStatusTracking
                        {
                            PodcastEpisodePublishReviewSessionId = s.Id,
                            PodcastEpisodePublishReviewSessionStatusId = (int)PodcastEpisodePublishReviewSessionStatusEnum.Discard,
                        };
                        await _podcastEpisodePublishReviewSessionStatusTrackingGenericRepository.CreateAsync(newTracking);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcasterId"] = discardPodcasterEpisodesPublishReviewTerminatePodcasterForceParameterDTO.PodcasterId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcasterId = discardPodcasterEpisodesPublishReviewTerminatePodcasterForceParameterDTO.PodcasterId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "discard-podcaster-episodes-publish-review-terminate-podcaster-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Discard podcaster episode publish review terminate podcaster force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "discard-podcaster-episodes-publish-review-terminate-podcaster-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DiscardShowEpisodesPublishReviewShowDeletionForce(DiscardShowEpisodesPublishReviewShowDeletionForceParameterDTO discardShowEpisodesPublishReviewShowDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // PodcastEpisodePublishReviewSession discard status khác 2, 3, 4 (EpisodeId)
                    var sessions = await _podcastEpisodePublishReviewSessionGenericRepository.FindAll(
                        predicate: pe => pe.PodcastEpisode.PodcastShowId == discardShowEpisodesPublishReviewShowDeletionForceParameterDTO.PodcastShowId,
                        // && pe.PodcastEpisodePublishReviewSessionStatusTrackings
                        // .OrderByDescending(pet => pet.CreatedAt)
                        // .FirstOrDefault()
                        // .PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview,
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodePublishReviewSessionStatusTrackings)
                    ).ToListAsync();

                    sessions = sessions.Where(s => s.PodcastEpisodePublishReviewSessionStatusTrackings
                        .OrderByDescending(pet => pet.CreatedAt)
                        .FirstOrDefault()
                        .PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview
                    ).ToList();

                    foreach (var s in sessions)
                    {
                        // tạo tracking mới với status Discard
                        var newTracking = new PodcastEpisodePublishReviewSessionStatusTracking
                        {
                            PodcastEpisodePublishReviewSessionId = s.Id,
                            PodcastEpisodePublishReviewSessionStatusId = (int)PodcastEpisodePublishReviewSessionStatusEnum.Discard,
                        };
                        await _podcastEpisodePublishReviewSessionStatusTrackingGenericRepository.CreateAsync(newTracking);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = discardShowEpisodesPublishReviewShowDeletionForceParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = discardShowEpisodesPublishReviewShowDeletionForceParameterDTO.PodcastShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "discard-show-episodes-publish-review-show-deletion-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Discard show episode publish review show deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "discard-show-episodes-publish-review-show-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveEpisodeListenSessionContentDmcaRemoveEpisodeForce(RemoveEpisodeListenSessionContentDmcaRemoveEpisodeForceParameterDTO removeEpisodeListenSessionContentDmcaRemoveEpisodeForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // xóa tất cả listen session của episode
                    var sessions = await _podcastEpisodeListenSessionGenericRepository.FindAll(
                        predicate: pes => pes.PodcastEpisodeId == removeEpisodeListenSessionContentDmcaRemoveEpisodeForceParameterDTO.PodcastEpisodeId,
                        includeFunc: null
                    ).ToListAsync();

                    foreach (var session in sessions)
                    {
                        // await _podcastEpisodeListenSessionGenericRepository.DeleteAsync(session.Id);
                        session.IsContentRemoved = true;
                        session.IsCompleted = true;
                        await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(session.Id, session);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = removeEpisodeListenSessionContentDmcaRemoveEpisodeForceParameterDTO.PodcastEpisodeId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = removeEpisodeListenSessionContentDmcaRemoveEpisodeForceParameterDTO.PodcastEpisodeId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-episode-listen-session-content-dmca-remove-episode-force.success"
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
                            ErrorMessage = $"Delete episode listen session dmca remove episode force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-episode-listen-session-content-dmca-remove-episode-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveEpisodeDmcaRemoveEpisodeForce(RemoveEpisodeDmcaRemoveEpisodeForceParameterDTO removeEpisodeDmcaRemoveEpisodeForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // xóa episode
                    var episode = await _podcastEpisodeGenericRepository.FindByIdAsync(
                        id: removeEpisodeDmcaRemoveEpisodeForceParameterDTO.PodcastEpisodeId,
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodeStatusTrackings)
                    );

                    if (episode == null)
                    {
                        throw new Exception("Podcast episode with id " + removeEpisodeDmcaRemoveEpisodeForceParameterDTO.PodcastEpisodeId + " does not exist");
                    }

                    var currentStatusTracking = episode.PodcastEpisodeStatusTrackings
                        .OrderByDescending(pet => pet.CreatedAt)
                        .FirstOrDefault();

                    if (currentStatusTracking.PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Removed)
                    {
                        // Remove khác với delete
                        var newStatusTracking = new PodcastEpisodeStatusTracking
                        {
                            PodcastEpisodeId = episode.Id,
                            PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Removed,
                        };

                        await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);

                        // kiểm tra show còn episode nào đang published không, nếu không và show đang ở trạng thái ready to release thì chuyển show về trạng thái Draft
                        var show = await _podcastShowGenericRepository.FindByIdAsync(
                            id: episode.PodcastShowId,
                            includeFunc: ps => ps.Include(ps => ps.PodcastEpisodes)
                                                    .ThenInclude(pe => pe.PodcastEpisodeStatusTrackings)
                                                  .Include(ps => ps.PodcastShowStatusTrackings)
                        );
                        if (show != null)
                        {
                            var publishedEpisodesCount = show.PodcastEpisodes.Count(pe =>
                                pe.Id != episode.Id &&
                                (pe.PodcastEpisodeStatusTrackings
                                .OrderByDescending(pet => pet.CreatedAt)
                                .FirstOrDefault()
                                .PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published || pe.PodcastEpisodeStatusTrackings
                                .OrderByDescending(pet => pet.CreatedAt)
                                .FirstOrDefault()
                                .PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.TakenDown)
                            );

                            var currentShowStatusTracking = show.PodcastShowStatusTrackings
                                .OrderByDescending(pst => pst.CreatedAt)
                                .FirstOrDefault();

                            if (publishedEpisodesCount == 0 &&
                                currentShowStatusTracking.PodcastShowStatusId == (int)PodcastShowStatusEnum.ReadyToRelease)
                            {
                                var newShowStatusTracking = new PodcastShowStatusTracking
                                {
                                    PodcastShowId = show.Id,
                                    PodcastShowStatusId = (int)PodcastShowStatusEnum.Draft,
                                };

                                await _podcastShowStatusTrackingGenericRepository.CreateAsync(newShowStatusTracking);
                            }
                        }
                    }
                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = removeEpisodeDmcaRemoveEpisodeForceParameterDTO.PodcastEpisodeId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = removeEpisodeDmcaRemoveEpisodeForceParameterDTO.PodcastEpisodeId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-episode-dmca-remove-episode-force.success"
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
                            ErrorMessage = $"Remove episode dmca remove episode force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-episode-dmca-remove-episode-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveDismissedEpisodeDmcaUnpublishEpisodeForce(RemoveDismissedEpisodeDmcaUnpublishEpisodeForceParameterDTO removeDismissedEpisodeDmcaUnpublishEpisodeForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // unpublish episode
                    var episode = await _podcastEpisodeGenericRepository.FindByIdAsync(
                        id: removeDismissedEpisodeDmcaUnpublishEpisodeForceParameterDTO.DmcaDismissedEpisodeId,
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodeStatusTrackings)
                    );

                    if (episode != null)
                    {
                        var currentStatusTracking = episode.PodcastEpisodeStatusTrackings
                                                .OrderByDescending(pet => pet.CreatedAt)
                                                .FirstOrDefault();

                        if (currentStatusTracking.PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Removed)
                        {
                            // Unpublish khác với delete
                            var newStatusTracking = new PodcastEpisodeStatusTracking
                            {
                                PodcastEpisodeId = episode.Id,
                                PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Removed,
                            };

                            await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);
                        }
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = removeDismissedEpisodeDmcaUnpublishEpisodeForceParameterDTO.PodcastEpisodeId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = removeDismissedEpisodeDmcaUnpublishEpisodeForceParameterDTO.PodcastEpisodeId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-dismissed-episode-dmca-unpublish-episode-force.success"
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
                            ErrorMessage = $"Remove dismissed episode dmca unpublish episode force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-dismissed-episode-dmca-unpublish-episode-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveDismissedEpisodeDmcaEpisodeDeletionForce(RemoveDismissedEpisodeDmcaEpisodeDeletionForceParameterDTO removeDismissedEpisodeDmcaEpisodeDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // xóa episode
                    var episode = await _podcastEpisodeGenericRepository.FindByIdAsync(
                        id: removeDismissedEpisodeDmcaEpisodeDeletionForceParameterDTO.DmcaDismissedEpisodeId,
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodeStatusTrackings)
                    );

                    if (episode != null)
                    {
                        var currentStatusTracking = episode.PodcastEpisodeStatusTrackings
                                                .OrderByDescending(pet => pet.CreatedAt)
                                                .FirstOrDefault();

                        if (currentStatusTracking.PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Removed)
                        {
                            // Remove khác với delete
                            var newStatusTracking = new PodcastEpisodeStatusTracking
                            {
                                PodcastEpisodeId = episode.Id,
                                PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Removed,
                            };

                            await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);
                        }
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = removeDismissedEpisodeDmcaEpisodeDeletionForceParameterDTO.PodcastEpisodeId;
                    messageNextRequestData["DmcaDismissedEpisodeId"] = removeDismissedEpisodeDmcaEpisodeDeletionForceParameterDTO.DmcaDismissedEpisodeId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = removeDismissedEpisodeDmcaEpisodeDeletionForceParameterDTO.PodcastEpisodeId,
                        DmcaDismissedEpisodeId = removeDismissedEpisodeDmcaEpisodeDeletionForceParameterDTO.DmcaDismissedEpisodeId
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-dismissed-episode-dmca-episode-deletion-force.success"
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
                            ErrorMessage = $"Remove dismissed episode dmca episode deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-dismissed-episode-dmca-episode-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteEpisodeEpisodeDeletionForce(DeleteEpisodeEpisodeDeletionForceParameterDTO deleteEpisodeEpisodeDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // xóa episode
                    var episode = await _podcastEpisodeGenericRepository.FindByIdAsync(
                        id: deleteEpisodeEpisodeDeletionForceParameterDTO.PodcastEpisodeId,
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodeStatusTrackings)
                    );

                    if (episode != null)
                    {
                        episode.DeletedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastEpisodeGenericRepository.UpdateAsync(episode.Id, episode);
                    }


                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = deleteEpisodeEpisodeDeletionForceParameterDTO.PodcastEpisodeId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = deleteEpisodeEpisodeDeletionForceParameterDTO.PodcastEpisodeId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-episode-episode-deletion-force.success"
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
                            ErrorMessage = $"Delete episode episode deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-episode-episode-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }
        public async Task RemoveDismissedShowEpisodesDmcaUnpublishShowForce(RemoveDismissedShowEpisodesDmcaUnpublishShowForceParameterDTO removeDismissedShowEpisodesDmcaUnpublishShowForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // unpublish tất cả episode trong show
                    var episodes = await _podcastEpisodeGenericRepository.FindAll(
                        predicate: pe => removeDismissedShowEpisodesDmcaUnpublishShowForceParameterDTO.DmcaDismissedEpisodeIds.Contains(pe.Id),
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodeStatusTrackings)
                    ).ToListAsync();

                    foreach (var episode in episodes)
                    {
                        var currentStatusTracking = episode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault();

                        if (currentStatusTracking.PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Removed)
                        {
                            // Unpublish khác với delete
                            var newStatusTracking = new PodcastEpisodeStatusTracking
                            {
                                PodcastEpisodeId = episode.Id,
                                PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Removed,
                            };

                            await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);
                        }
                    }
                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = removeDismissedShowEpisodesDmcaUnpublishShowForceParameterDTO.PodcastShowId;
                    messageNextRequestData["DmcaDismissedEpisodeIds"] = JArray.FromObject(removeDismissedShowEpisodesDmcaUnpublishShowForceParameterDTO.DmcaDismissedEpisodeIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = removeDismissedShowEpisodesDmcaUnpublishShowForceParameterDTO.PodcastShowId,
                        DmcaDismissedEpisodeIds = removeDismissedShowEpisodesDmcaUnpublishShowForceParameterDTO.DmcaDismissedEpisodeIds
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-dismissed-show-episodes-dmca-unpublish-show-force.success"
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
                            ErrorMessage = $"Remove dismissed show episodes dmca unpublish show force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-dismissed-show-episodes-dmca-unpublish-show-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveDismissedShowEpisodesDmcaShowDeletionForce(RemoveDismissedShowEpisodesDmcaShowDeletionForceParameterDTO removeDismissedShowEpisodesDmcaShowDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // xóa tất cả episode trong show
                    var episodes = await _podcastEpisodeGenericRepository.FindAll(
                        predicate: pe => removeDismissedShowEpisodesDmcaShowDeletionForceParameterDTO.DmcaDismissedEpisodeIds.Contains(pe.Id),
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodeStatusTrackings)
                    ).ToListAsync();

                    foreach (var episode in episodes)
                    {
                        var currentStatusTracking = episode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault();

                        if (currentStatusTracking.PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Removed)
                        {
                            // Remove khác với delete
                            var newStatusTracking = new PodcastEpisodeStatusTracking
                            {
                                PodcastEpisodeId = episode.Id,
                                PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Removed,
                            };

                            await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);
                        }
                    }
                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = removeDismissedShowEpisodesDmcaShowDeletionForceParameterDTO.PodcastShowId;
                    messageNextRequestData["DmcaDismissedEpisodeIds"] = JArray.FromObject(removeDismissedShowEpisodesDmcaShowDeletionForceParameterDTO.DmcaDismissedEpisodeIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = removeDismissedShowEpisodesDmcaShowDeletionForceParameterDTO.PodcastShowId,
                        DmcaDismissedEpisodeIds = removeDismissedShowEpisodesDmcaShowDeletionForceParameterDTO.DmcaDismissedEpisodeIds
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-dismissed-show-episodes-dmca-show-deletion-force.success"
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
                            ErrorMessage = $"Remove dismissed show episodes dmca show deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-dismissed-show-episodes-dmca-show-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveDismissedEpisodeDmcaTerminatePodcasterForce(RemoveDismissedEpisodeDmcaTerminatePodcasterForceParameterDTO removeDismissedEpisodeDmcaTerminatePodcasterForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // xóa tất cả episode trong show
                    var episodes = await _podcastEpisodeGenericRepository.FindAll(
                        predicate: pe => removeDismissedEpisodeDmcaTerminatePodcasterForceParameterDTO.DmcaDismissedEpisodeIds.Contains(pe.Id),
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodeStatusTrackings)
                    ).ToListAsync();

                    foreach (var episode in episodes)
                    {
                        var currentStatusTracking = episode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault();

                        if (currentStatusTracking.PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Removed)
                        {
                            // Remove khác với delete
                            var newStatusTracking = new PodcastEpisodeStatusTracking
                            {
                                PodcastEpisodeId = episode.Id,
                                PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Removed,
                            };

                            await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);
                        }
                    }
                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcasterId"] = removeDismissedEpisodeDmcaTerminatePodcasterForceParameterDTO.PodcasterId;
                    messageNextRequestData["DmcaDismissedEpisodeIds"] = JArray.FromObject(removeDismissedEpisodeDmcaTerminatePodcasterForceParameterDTO.DmcaDismissedEpisodeIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcasterId = removeDismissedEpisodeDmcaTerminatePodcasterForceParameterDTO.PodcasterId,
                        DmcaDismissedEpisodeIds = removeDismissedEpisodeDmcaTerminatePodcasterForceParameterDTO.DmcaDismissedEpisodeIds
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-dismissed-episode-dmca-terminate-podcaster-force.success"
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
                            ErrorMessage = $"Remove dismissed episode dmca terminate podcaster force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-dismissed-episode-dmca-terminate-podcaster-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveChannelDismissedEpisodesDmcaUnpublishChannelForce(RemoveChannelDismissedEpisodesDmcaUnpublishChannelForceParameterDTO removeChannelDismissedEpisodesDmcaUnpublishChannelForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // unpublish tất cả episode trong show
                    var episodes = await _podcastEpisodeGenericRepository.FindAll(
                        predicate: pe => removeChannelDismissedEpisodesDmcaUnpublishChannelForceParameterDTO.DmcaDismissedEpisodeIds.Contains(pe.Id),
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodeStatusTrackings)
                    ).ToListAsync();

                    foreach (var episode in episodes)
                    {
                        var currentStatusTracking = episode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault();

                        if (currentStatusTracking.PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Removed)
                        {
                            // Unpublish khác với delete
                            var newStatusTracking = new PodcastEpisodeStatusTracking
                            {
                                PodcastEpisodeId = episode.Id,
                                PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Removed,
                            };

                            await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);
                        }
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = removeChannelDismissedEpisodesDmcaUnpublishChannelForceParameterDTO.PodcastChannelId;
                    messageNextRequestData["DmcaDismissedEpisodeIds"] = JArray.FromObject(removeChannelDismissedEpisodesDmcaUnpublishChannelForceParameterDTO.DmcaDismissedEpisodeIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = removeChannelDismissedEpisodesDmcaUnpublishChannelForceParameterDTO.PodcastChannelId,
                        DmcaDismissedEpisodeIds = removeChannelDismissedEpisodesDmcaUnpublishChannelForceParameterDTO.DmcaDismissedEpisodeIds
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-channel-dismissed-episodes-dmca-unpublish-channel-force.success"
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
                            ErrorMessage = $"Remove channel dismissed episode dmca unpublish channel force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-channel-dismissed-episodes-dmca-unpublish-channel-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveChannelDismissedEpisodesDmcaChannelDeletionForce(RemoveChannelDismissedEpisodesDmcaChannelDeletionForceParameterDTO removeChannelDismissedEpisodesDmcaChannelDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // xóa tất cả episode trong show
                    var episodes = await _podcastEpisodeGenericRepository.FindAll(
                        predicate: pe => removeChannelDismissedEpisodesDmcaChannelDeletionForceParameterDTO.DmcaDismissedEpisodeIds.Contains(pe.Id),
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodeStatusTrackings)
                    ).ToListAsync();

                    foreach (var episode in episodes)
                    {
                        var currentStatusTracking = episode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault();

                        if (currentStatusTracking.PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Removed)
                        {
                            // Remove khác với delete
                            var newStatusTracking = new PodcastEpisodeStatusTracking
                            {
                                PodcastEpisodeId = episode.Id,
                                PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Removed,
                            };

                            await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);
                        }
                    }
                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = removeChannelDismissedEpisodesDmcaChannelDeletionForceParameterDTO.PodcastChannelId;
                    messageNextRequestData["DmcaDismissedEpisodeIds"] = JArray.FromObject(removeChannelDismissedEpisodesDmcaChannelDeletionForceParameterDTO.DmcaDismissedEpisodeIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = removeChannelDismissedEpisodesDmcaChannelDeletionForceParameterDTO.PodcastChannelId,
                        DmcaDismissedEpisodeIds = removeChannelDismissedEpisodesDmcaChannelDeletionForceParameterDTO.DmcaDismissedEpisodeIds
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-channel-dismissed-episodes-dmca-channel-deletion-force.success"
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
                            ErrorMessage = $"Remove channel dismissed episode dmca channel deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-channel-dismissed-episodes-dmca-channel-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveShowEpisodesDmcaRemoveShowForce(RemoveShowEpisodesDmcaRemoveShowForceParameterDTO removeShowEpisodesDmcaRemoveShowForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // xóa tất cả episode trong show
                    var episodes = await _podcastEpisodeGenericRepository.FindAll(
                        predicate: pe => pe.PodcastShowId == removeShowEpisodesDmcaRemoveShowForceParameterDTO.PodcastShowId,
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodeStatusTrackings)
                    ).ToListAsync();

                    foreach (var episode in episodes)
                    {
                        var currentStatusTracking = episode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault();

                        if (currentStatusTracking.PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Removed)
                        {
                            // Remove khác với delete
                            var newStatusTracking = new PodcastEpisodeStatusTracking
                            {
                                PodcastEpisodeId = episode.Id,
                                PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Removed,
                            };

                            await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);
                        }
                    }
                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = removeShowEpisodesDmcaRemoveShowForceParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = removeShowEpisodesDmcaRemoveShowForceParameterDTO.PodcastShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-show-episodes-dmca-remove-show-force.success"
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
                            ErrorMessage = $"Remove show episodes dmca remove show force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-show-episodes-dmca-remove-show-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveEpisodeListenSessionContentUnpublishEpisodeForce(RemoveEpisodeListenSessionContentUnpublishEpisodeForceParameterDTO removeEpisodeListenSessionContentUnpublishEpisodeForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // xóa tất cả listen session của episode
                    var sessions = await _podcastEpisodeListenSessionGenericRepository.FindAll(
                        predicate: pes => pes.PodcastEpisodeId == removeEpisodeListenSessionContentUnpublishEpisodeForceParameterDTO.PodcastEpisodeId,
                        includeFunc: null
                    ).ToListAsync();

                    foreach (var session in sessions)
                    {
                        // await _podcastEpisodeListenSessionGenericRepository.DeleteAsync(session.Id);
                        session.IsContentRemoved = true;
                        session.IsCompleted = true;
                        await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(session.Id, session);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = removeEpisodeListenSessionContentUnpublishEpisodeForceParameterDTO.PodcastEpisodeId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = removeEpisodeListenSessionContentUnpublishEpisodeForceParameterDTO.PodcastEpisodeId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-episode-listen-session-content-unpublish-episode-force.success"
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
                            ErrorMessage = $"Delete episode listen session unpublish episode force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-episode-listen-session-content-unpublish-episode-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveShowEpisodesListenSessionContentDmcaRemoveShowForce(RemoveShowEpisodesListenSessionContentDmcaRemoveShowForceParameterDTO removeShowEpisodesListenSessionContentDmcaRemoveShowForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // xóa tất cả listen session của tất cả episode trong show

                    var episodeIds = await _podcastEpisodeGenericRepository.FindAll(
                        predicate: pe => pe.PodcastShowId == removeShowEpisodesListenSessionContentDmcaRemoveShowForceParameterDTO.PodcastShowId,
                        includeFunc: null
                    ).Select(pe => pe.Id).ToListAsync();

                    foreach (var episodeId in episodeIds)
                    {
                        await _unitOfWork.PodcastEpisodeListenSessionRepository.RemoveContentByPodcastEpisodeIdAsync(episodeId);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = removeShowEpisodesListenSessionContentDmcaRemoveShowForceParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = removeShowEpisodesListenSessionContentDmcaRemoveShowForceParameterDTO.PodcastShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-show-episodes-listen-session-content-dmca-remove-show-force.success"
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
                            ErrorMessage = $"Delete show episode listen session dmca remove show force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-show-episodes-listen-session-content-dmca-remove-show-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveShowEpisodesListenSessionContentUnpublishShowForce(RemoveShowEpisodesListenSessionContentUnpublishShowForceParameterDTO removeShowEpisodesListenSessionContentUnpublishShowForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    foreach (var episodeId in removeShowEpisodesListenSessionContentUnpublishShowForceParameterDTO.DmcaDismissedEpisodeIds)
                    {
                        await _unitOfWork.PodcastEpisodeListenSessionRepository.RemoveContentByPodcastEpisodeIdAsync(episodeId);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = removeShowEpisodesListenSessionContentUnpublishShowForceParameterDTO.PodcastShowId;
                    messageNextRequestData["DmcaDismissedEpisodeIds"] = JArray.FromObject(removeShowEpisodesListenSessionContentUnpublishShowForceParameterDTO.DmcaDismissedEpisodeIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = removeShowEpisodesListenSessionContentUnpublishShowForceParameterDTO.PodcastShowId,
                        DmcaDismissedEpisodeIds = removeShowEpisodesListenSessionContentUnpublishShowForceParameterDTO.DmcaDismissedEpisodeIds
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-show-episodes-listen-session-content-unpublish-show-force.success"
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
                            ErrorMessage = $"Delete show episode listen session unpublish show force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-show-episodes-listen-session-content-unpublish-show-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveShowEpisodesListenSessionContentShowDeletionForce(RemoveShowEpisodesListenSessionContentShowDeletionForceParameterDTO removeShowEpisodesListenSessionContentShowDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // xóa tất cả listen session của tất cả episode trong show
                    // var sessions = await _podcastEpisodeListenSessionGenericRepository.FindAll(
                    //     predicate: pes => pes.PodcastEpisode.PodcastShowId == deleteShowEpisodesListenSessionShowDeletionForceParameterDTO.PodcastShowId,
                    //     includeFunc: null
                    // ).ToListAsync();

                    // foreach (var session in sessions)
                    // {
                    //     await _podcastEpisodeListenSessionGenericRepository.DeleteAsync(session.Id);
                    // }
                    var episodeIds = await _podcastEpisodeGenericRepository.FindAll(
                        predicate: pe => pe.PodcastShowId == removeShowEpisodesListenSessionContentShowDeletionForceParameterDTO.PodcastShowId,
                        includeFunc: null
                    ).Select(pe => pe.Id).ToListAsync();

                    foreach (var episodeId in episodeIds)
                    {
                        await _unitOfWork.PodcastEpisodeListenSessionRepository.RemoveContentByPodcastEpisodeIdAsync(episodeId);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = removeShowEpisodesListenSessionContentShowDeletionForceParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = removeShowEpisodesListenSessionContentShowDeletionForceParameterDTO.PodcastShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-show-episodes-listen-session-content-show-deletion-force.success"
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
                            ErrorMessage = $"Delete show episode listen session show deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-show-episodes-listen-session-content-show-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveChannelEpisodesListenSessionContentUnpublishChannelForce(RemoveChannelEpisodesListenSessionContentUnpublishChannelForceParameterDTO removeChannelEpisodesListenSessionContentUnpublishChannelForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    foreach (var episodeId in removeChannelEpisodesListenSessionContentUnpublishChannelForceParameterDTO.DmcaDismissedEpisodeIds)
                    {
                        await _unitOfWork.PodcastEpisodeListenSessionRepository.RemoveContentByPodcastEpisodeIdAsync(episodeId);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = removeChannelEpisodesListenSessionContentUnpublishChannelForceParameterDTO.PodcastChannelId;
                    messageNextRequestData["DmcaDismissedEpisodeIds"] = JArray.FromObject(removeChannelEpisodesListenSessionContentUnpublishChannelForceParameterDTO.DmcaDismissedEpisodeIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = removeChannelEpisodesListenSessionContentUnpublishChannelForceParameterDTO.PodcastChannelId,
                        DmcaDismissedEpisodeIds = removeChannelEpisodesListenSessionContentUnpublishChannelForceParameterDTO.DmcaDismissedEpisodeIds
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-channel-episodes-listen-session-content-unpublish-channel-force.success"
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
                            ErrorMessage = $"Delete channel episode listen session unpublish channel force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-channel-episodes-listen-session-content-unpublish-channel-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveChannelEpisodesListenSessionContentChannelDeletionForce(RemoveChannelEpisodesListenSessionContentChannelDeletionForceParameterDTO removeChannelEpisodesListenSessionContentChannelDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // xóa tất cả listen session của tất cả episode trong channel
                    // var sessions = await _podcastEpisodeListenSessionGenericRepository.FindAll(
                    //     predicate: pes => pes.PodcastEpisode.PodcastShow.PodcastChannelId == deleteChannelEpisodesListenSessionChannelDeletionForceParameterDTO.PodcastChannelId,
                    //     includeFunc: null
                    // ).ToListAsync();

                    // foreach (var session in sessions)
                    // {
                    //     await _podcastEpisodeListenSessionGenericRepository.DeleteAsync(session.Id);
                    // }

                    var episodeIds = await _podcastEpisodeGenericRepository.FindAll(
                        predicate: pe => pe.PodcastShow.PodcastChannelId == removeChannelEpisodesListenSessionContentChannelDeletionForceParameterDTO.PodcastChannelId,
                        includeFunc: null
                    ).Select(pe => pe.Id).ToListAsync();

                    foreach (var episodeId in episodeIds)
                    {
                        await _unitOfWork.PodcastEpisodeListenSessionRepository.RemoveContentByPodcastEpisodeIdAsync(episodeId);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = removeChannelEpisodesListenSessionContentChannelDeletionForceParameterDTO.PodcastChannelId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = removeChannelEpisodesListenSessionContentChannelDeletionForceParameterDTO.PodcastChannelId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-channel-episodes-listen-session-content-channel-deletion-force.success"
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
                            ErrorMessage = $"Delete channel episode listen session channel deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-channel-episodes-listen-session-content-channel-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemovePodcasterEpisodesListenSessionContentTerminatePodcasterForce(RemovePodcasterEpisodesListenSessionContentTerminatePodcasterForceParameterDTO removePodcasterEpisodesListenSessionContentTerminatePodcasterForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    var episodeIds = await _podcastEpisodeGenericRepository.FindAll(
                        predicate: pe => pe.PodcastShow.PodcasterId == removePodcasterEpisodesListenSessionContentTerminatePodcasterForceParameterDTO.PodcasterId,
                        includeFunc: null
                    ).Select(pe => pe.Id).ToListAsync();

                    foreach (var episodeId in episodeIds)
                    {
                        await _unitOfWork.PodcastEpisodeListenSessionRepository.RemoveContentByPodcastEpisodeIdAsync(episodeId);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcasterId"] = removePodcasterEpisodesListenSessionContentTerminatePodcasterForceParameterDTO.PodcasterId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcasterId = removePodcasterEpisodesListenSessionContentTerminatePodcasterForceParameterDTO.PodcasterId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-podcaster-episodes-listen-session-content-terminate-podcaster-force.success"
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
                            ErrorMessage = $"Delete podcaster episode listen session terminate podcaster force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-podcaster-episodes-listen-session-content-terminate-podcaster-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveEpisodeListenSessionContentEpisodeDeletionForce(RemoveEpisodeListenSessionContentEpisodeDeletionForceParameterDTO removeEpisodeListenSessionContentEpisodeDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // xóa tất cả listen session của episode
                    var sessions = await _podcastEpisodeListenSessionGenericRepository.FindAll(
                        predicate: pes => pes.PodcastEpisodeId == removeEpisodeListenSessionContentEpisodeDeletionForceParameterDTO.PodcastEpisodeId,
                        includeFunc: null
                    ).ToListAsync();

                    foreach (var session in sessions)
                    {
                        // await _podcastEpisodeListenSessionGenericRepository.DeleteAsync(session.Id);
                        session.IsContentRemoved = true;
                        session.IsCompleted = true;
                        await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(session.Id, session);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = removeEpisodeListenSessionContentEpisodeDeletionForceParameterDTO.PodcastEpisodeId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = removeEpisodeListenSessionContentEpisodeDeletionForceParameterDTO.PodcastEpisodeId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-episode-listen-session-content-episode-deletion-force.success"
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
                            ErrorMessage = $"Delete episode listen session episode deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-episode-listen-session-content-episode-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task UnpublishEpisodeUnpublishEpisodeForce(UnpublishEpisodeUnpublishEpisodeForceParameterDTO unpublishEpisodeUnpublishEpisodeForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // unpublish episode
                    var episode = await _podcastEpisodeGenericRepository.FindByIdAsync(
                        id: unpublishEpisodeUnpublishEpisodeForceParameterDTO.PodcastEpisodeId,
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodeStatusTrackings)
                    );

                    if (episode != null)
                    {
                        var currentStatusTracking = episode.PodcastEpisodeStatusTrackings
                                                .OrderByDescending(pet => pet.CreatedAt)
                                                .FirstOrDefault();

                        if (currentStatusTracking.PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published || currentStatusTracking.PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.TakenDown)
                        {
                            // Unpublish khác với delete
                            var newStatusTracking = new PodcastEpisodeStatusTracking
                            {
                                PodcastEpisodeId = episode.Id,
                                PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.ReadyToRelease,
                            };

                            await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);

                            episode.TotalSave = 0;
                            episode.ListenCount = 0;
                            await _podcastEpisodeGenericRepository.UpdateAsync(episode.Id, episode);

                            // kiểm tra show còn episode nào đang published không, nếu không và show đang ở trạng thái ready to release thì chuyển show về trạng thái Draft
                            var show = await _podcastShowGenericRepository.FindByIdAsync(
                            id: episode.PodcastShowId,
                            includeFunc: ps => ps.Include(ps => ps.PodcastEpisodes)
                                                    .ThenInclude(pe => pe.PodcastEpisodeStatusTrackings)
                                                  .Include(ps => ps.PodcastShowStatusTrackings)
                            );
                            if (show != null)
                            {
                                var publishedEpisodesCount = show.PodcastEpisodes.Count(pe =>
                                    pe.Id != episode.Id &&
                                    (pe.PodcastEpisodeStatusTrackings
                                    .OrderByDescending(pet => pet.CreatedAt)
                                    .FirstOrDefault()
                                    .PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published || pe.PodcastEpisodeStatusTrackings
                                    .OrderByDescending(pet => pet.CreatedAt)
                                    .FirstOrDefault()
                                    .PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.TakenDown)
                                );

                                var currentShowStatusTracking = show.PodcastShowStatusTrackings
                                    .OrderByDescending(pst => pst.CreatedAt)
                                    .FirstOrDefault();

                                if (publishedEpisodesCount == 0 &&
                                    currentShowStatusTracking.PodcastShowStatusId == (int)PodcastShowStatusEnum.ReadyToRelease)
                                {
                                    var newShowStatusTracking = new PodcastShowStatusTracking
                                    {
                                        PodcastShowId = show.Id,
                                        PodcastShowStatusId = (int)PodcastShowStatusEnum.Draft,
                                    };

                                    await _podcastShowStatusTrackingGenericRepository.CreateAsync(newShowStatusTracking);
                                }
                            }
                        }
                    }


                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = unpublishEpisodeUnpublishEpisodeForceParameterDTO.PodcastEpisodeId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = unpublishEpisodeUnpublishEpisodeForceParameterDTO.PodcastEpisodeId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "unpublish-episode-unpublish-episode-force.success"
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
                            ErrorMessage = $"Unpublish episode unpublish episode force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "unpublish-episode-unpublish-episode-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task UnpublishPodcasterEpisodesTerminatePodcasterForce(UnpublishPodcasterEpisodesTerminatePodcasterForceParameterDTO unpublishPodcasterEpisodesTerminatePodcasterForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // lấy danh sách mọi episode chưa bị xoá trong podcaster
                    var episodes = await _podcastEpisodeGenericRepository.FindAll(
                        predicate: pe => pe.PodcastShow.PodcasterId == unpublishPodcasterEpisodesTerminatePodcasterForceParameterDTO.PodcasterId,
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodeStatusTrackings)
                    ).ToListAsync();

                    foreach (var episode in episodes)
                    {
                        var currentStatusTracking = episode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault();

                        if (currentStatusTracking.PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Draft && currentStatusTracking.PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Removed)
                        {
                            // Unpublish khác với delete
                            var newStatusTracking = new PodcastEpisodeStatusTracking
                            {
                                PodcastEpisodeId = episode.Id,
                                PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Draft,
                            };

                            await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);

                            episode.TotalSave = 0;
                            episode.ListenCount = 0;
                            await _podcastEpisodeGenericRepository.UpdateAsync(episode.Id, episode);
                        }
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcasterId"] = unpublishPodcasterEpisodesTerminatePodcasterForceParameterDTO.PodcasterId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcasterId = unpublishPodcasterEpisodesTerminatePodcasterForceParameterDTO.PodcasterId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "unpublish-podcaster-episodes-terminate-podcaster-force.success"
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
                            ErrorMessage = $"Unpublish podcaster episodes terminate podcaster force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "unpublish-podcaster-episodes-terminate-podcaster-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteShowEpisodesShowDeletionForce(DeleteShowEpisodesShowDeletionForceParameterDTO deleteShowEpisodesShowDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // lấy danh sách mọi episode chưa bị xoá trong show
                    var episodes = await _podcastEpisodeGenericRepository.FindAll(
                        predicate: pe => pe.PodcastShowId == deleteShowEpisodesShowDeletionForceParameterDTO.PodcastShowId &&
                                         pe.DeletedAt == null,
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodeStatusTrackings)
                    ).ToListAsync();

                    foreach (var episode in episodes)
                    {
                        episode.DeletedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastEpisodeGenericRepository.UpdateAsync(episode.Id, episode);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = deleteShowEpisodesShowDeletionForceParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = deleteShowEpisodesShowDeletionForceParameterDTO.PodcastShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-show-episodes-show-deletion-force.success"
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
                            ErrorMessage = $"Delete show episodes show deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-show-episodes-show-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteChannelEpisodesChannelDeletionForce(DeleteChannelEpisodesChannelDeletionForceParameterDTO deleteChannelEpisodesChannelDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // lấy danh sách mọi episode chưa bị xoá trong channel
                    var episodes = await _podcastEpisodeGenericRepository.FindAll(
                        predicate: pe => pe.PodcastShow.PodcastChannelId == deleteChannelEpisodesChannelDeletionForceParameterDTO.PodcastChannelId &&
                                         pe.DeletedAt == null,
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodeStatusTrackings)
                    ).ToListAsync();

                    foreach (var episode in episodes)
                    {
                        episode.DeletedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastEpisodeGenericRepository.UpdateAsync(episode.Id, episode);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = deleteChannelEpisodesChannelDeletionForceParameterDTO.PodcastChannelId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = deleteChannelEpisodesChannelDeletionForceParameterDTO.PodcastChannelId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-channel-episodes-channel-deletion-force.success"
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
                            ErrorMessage = $"Delete channel episodes channel deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-channel-episodes-channel-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }


        public async Task<List<EpisodeListenHistoryListItemResponseDTO>> GetPodcastEpisodeListenHistoryAsync(int listenerId)
        {
            try
            {
                var listenSessionHistoryQuery = _podcastEpisodeListenSessionGenericRepository.FindAll(
                    predicate: pes => pes.AccountId == listenerId && pes.IsContentRemoved == false,
                    includeFunc: pe => pe
                        .Include(pes => pes.PodcastEpisode)
                            .ThenInclude(pe => pe.PodcastShow)
                );

                // có thể có nhiều session có cùng episode id và được rải rác ở nhiều thời điểm
                // group lại các session theo episode id (lọc ra các session trong đo các episode không trùng lập và ngày tạo mới nhất trở xuống)
                var listenSessionHistoryGroupedByEpisode = await listenSessionHistoryQuery
                    .GroupBy(pes => pes.PodcastEpisodeId)
                    .Select(g => new EpisodeListenHistoryListItemResponseDTO
                    {
                        PodcastEpisode = g.Select(pes => new PodcastEpisodeSnippetResponseDTO
                        {
                            Id = pes.PodcastEpisodeId,
                            Name = pes.PodcastEpisode.Name,
                            Description = pes.PodcastEpisode.Description,
                            MainImageFileKey = pes.PodcastEpisode.MainImageFileKey,
                            IsReleased = pes.PodcastEpisode.IsReleased,
                            ReleaseDate = pes.PodcastEpisode.ReleaseDate,
                            AudioLength = pes.PodcastEpisode.AudioLength
                        }).FirstOrDefault(),
                        Podcaster = g.Select(pes => new AccountSnippetResponseDTO
                        {
                            Id = pes.PodcastEpisode.PodcastShow.PodcasterId,
                            FullName = "",
                            Email = "",
                            // DisplayName và MainImageFileKey sẽ được gán sau
                        }).FirstOrDefault()!,
                        CreatedAt = g.Max(pes => pes.CreatedAt)
                    })
                    .OrderByDescending(g => g.CreatedAt)
                    .ToListAsync();
                foreach (var item in listenSessionHistoryGroupedByEpisode)
                {
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(item.Podcaster.Id);
                    if (podcaster != null)
                    {
                        item.Podcaster = new AccountSnippetResponseDTO
                        {
                            Id = podcaster.Id,
                            FullName = podcaster.PodcasterProfileName,
                            Email = podcaster.Email,
                            MainImageFileKey = podcaster.MainImageFileKey,
                        };
                    }
                }

                return listenSessionHistoryGroupedByEpisode;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                // throw new HttpRequestException("Get episode by id failed, error: " + ex.Message);
                throw new Exception("Get episode listen history failed, error: " + ex.Message);
            }
        }

        public async Task<EpisodeListenResponseDTO> GetLatestPodcastEpisodeListenSessionAsync(int listenerId, DeviceInfoDTO deviceInfo)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // hàm này có pipeline như sau:
                    // 1. Lấy session mới nhất của listener, nếu là IsCompleted = true hoặc đã quá ExpiredAt thì trả null ngay , ngược lại thì qua bước 2
                    // 2. Check điều kiện cần đề nghe (bao gồm status của episode/show/channle và các subscription type) , làm tương tự như cách ở hàm GetEpisodeListenAsync
                    //      --> không thoả thì: mark IsCompleted = true và trả về null ngay, ngược lại thì qua bước 3
                    // 3. tạo jwt mới chứa các thông tin và update lại session hiện tại (thời lượng expired của jwt là cấu hình now + PodcastListenSessionConfig.TokenEncryptionKeyRequestExpirationMinutes):
                    //      + session id
                    //      + isUsed = false
                    // 4. trả về playlist file key và token mới vào DTO EpisodeListenResponseDTO

                    var latestListenSession = await _podcastEpisodeListenSessionGenericRepository.FindAll(
                        predicate: pes => pes.AccountId == listenerId,
                        includeFunc: null
                    // .Include(pes => pes.PodcastEpisode)
                    // .ThenInclude(pe => pe.PodcastShow)
                    )
                        .OrderByDescending(pes => pes.CreatedAt)
                        .FirstOrDefaultAsync();
                    if (latestListenSession == null || latestListenSession.IsCompleted == true || latestListenSession.ExpiredAt <= _dateHelper.GetNowByAppTimeZone())
                    {
                        // return null!;
                        Console.WriteLine("No valid latest listen session found for listener id: " + listenerId);
                        // in các thông tin của latestListenSession
                        // Console.WriteLine($"LatestListenSession: {latestListenSession.ToString()}");
                        return new EpisodeListenResponseDTO
                        {
                            ListenSessionProcedure = null,
                            ListenSession = null
                        };
                    }
                    // var episode = latestListenSession.PodcastEpisode;
                    var episode = await GetValidEpisodeListenPermission(latestListenSession.PodcastEpisodeId);

                    UserPodcastSubscriptionRegistrationEpisodeBaseQueryRequestDTO userPodcastSubscriptionRegistrationEpisodeBaseQueryRequestDTO = new UserPodcastSubscriptionRegistrationEpisodeBaseQueryRequestDTO
                    {
                        EpisodeBaseSourceInfoList = new List<EpisodeBaseSourceInfoDTO>
                            {
                                new EpisodeBaseSourceInfoDTO
                                {
                                    EpisodeId = episode.Id,
                                    ShowId = episode.PodcastShowId,
                                    ChannelId = episode.PodcastShow.PodcastChannel != null ? episode.PodcastShow.PodcastChannel.Id : (Guid?)null
                                }
                            }
                    };

                    UserPodcastSubscriptionRegistrationEpisodeBaseQueryResponseDTO userPodcastSubscriptionRegistrationEpisodeBaseQueryResponseDTO =
                            await _crossServiceHttpService.PostManualAsync<UserPodcastSubscriptionRegistrationEpisodeBaseQueryResponseDTO>(
                                serviceName: "SubscriptionService",
                                body: userPodcastSubscriptionRegistrationEpisodeBaseQueryRequestDTO,
                                relativePath: $"api/podcast-subscriptions/service-query/{listenerId}"
                            );

                    List<PodcastSubscriptionBenefitDTO> listenerCurrentPodcastSubscriptionRegistrationBenefitListOfLatestListenSession =
                        userPodcastSubscriptionRegistrationEpisodeBaseQueryResponseDTO.EpisodeBaseBenefitList
                                .First(e => e.EpisodeId == episode.Id)
                                .PodcastSubscriptionBenefitIds
                                .Select(id => new PodcastSubscriptionBenefitDTO { Id = id })
                                .ToList();
                    // in ra tất cả các benefit
                    foreach (var benefit in listenerCurrentPodcastSubscriptionRegistrationBenefitListOfLatestListenSession)
                    {
                        Console.WriteLine("Listener current podcast subscription benefit id: " + benefit.Id);
                    }

                    foreach (var benefit in listenerCurrentPodcastSubscriptionRegistrationBenefitListOfLatestListenSession)
                    {
                        Console.WriteLine("Listener current podcast subscription benefit id: " + benefit.Id);
                    }

                    var canListen = await this.CheckListenerCanListenToEpisodeAsync(
                        listenerId: listenerId,
                        validEpisode: episode,
                        isNonQuotaListeningCheck: false,
                        listenerCurrentPodcastSubscriptionRegistrationBenefitList: listenerCurrentPodcastSubscriptionRegistrationBenefitListOfLatestListenSession
                    );

                    EpisodeListenResponseDTO responseDTO = new EpisodeListenResponseDTO
                    {
                        ListenSessionProcedure = null,
                        ListenSession = null
                    };
                    if (!canListen.CanListen)
                    {
                        Console.WriteLine("Listener id " + listenerId + " cannot listen to episode id " + episode.Id + ", reason: " + canListen.Reason);
                        // mark session là completed
                        latestListenSession.IsCompleted = true;
                        await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(latestListenSession.Id, latestListenSession);
                    }
                    else
                    {
                        var playlistFileKey = FilePathHelper.CombinePaths(
                                        _filePathConfig.PODCAST_EPISODE_FILE_PATH,
                                        episode.Id.ToString(),
                                        "playlist",
                                        _hlsConfig.PlaylistFileName
                                    );
                        // tạo jwt mới
                        // var token = GenerateEpisodeListenToken(latestListenSession.Id, false);
                        // // update lại session hiện tại
                        // Console.WriteLine("Session id: " + latestListenSession.Id);
                        // Console.WriteLine("Old token for latest listen session: " + latestListenSession.Token);
                        // latestListenSession.Token = token;
                        var newSessionToken = GenerateEpisodeListenHlsEnckeyRequestToken(latestListenSession.Id);
                        await _podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository.CreateAsync(new PodcastEpisodeListenSessionHlsEnckeyRequestToken
                        {
                            PodcastEpisodeListenSessionId = latestListenSession.Id,
                            Token = newSessionToken,
                            IsUsed = false,
                        });

                        Console.WriteLine("Generated new token for latest listen session: " + newSessionToken);
                        await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(latestListenSession.Id, latestListenSession);

                        var podcaster = await _accountCachingService.GetAccountStatusCacheById(episode.PodcastShow.PodcasterId);

                        var currentProcedure = await _customerListenSessionProcedureCachingService.GetActiveProcedureByCustomerIdAsync(listenerId);


                        //refresh
                        if (currentProcedure.SourceDetail.Type == CustomerListenSessionProcedureSourceDetailTypeEnum.SavedEpisodes.ToString())
                        {
                            currentProcedure = await RefreshEpisodeOrderForSavedEpisodesSourceAsync(
                                currentProcedure,
                                listenerId,
                                latestListenSession.PodcastEpisodeId,
                                null
                            );
                        }
                        else if (currentProcedure.SourceDetail.Type == CustomerListenSessionProcedureSourceDetailTypeEnum.SpecifyShowEpisodes.ToString())
                        {
                            currentProcedure = await RefreshEpisodeOrderForSpecifyShowEpisodesSourceAsync(
                                currentProcedure,
                                listenerId,
                                latestListenSession.PodcastEpisodeId,
                                listenerCurrentPodcastSubscriptionRegistrationBenefitListOfLatestListenSession
                            );
                        }
                        await _customerListenSessionProcedureCachingService.UpdateProcedureAsync(listenerId, currentProcedure.Id, currentProcedure);

                        responseDTO = new EpisodeListenResponseDTO
                        {
                            ListenSessionProcedure = currentProcedure,
                            ListenSession = new EpisodeListenSessionResponseDTO
                            {
                                PlaylistFileKey = playlistFileKey,
                                // Token = latestListenSession.Token,
                                Token = newSessionToken,
                                // LastListenDurationSeconds = latestListenSession.LastListenDurationSeconds,
                                PodcastEpisodeListenSession = new PodcastEpisodeListenSessionSnippetResponseDTO
                                {
                                    Id = latestListenSession.Id,
                                    LastListenDurationSeconds = latestListenSession.LastListenDurationSeconds,
                                },
                                PodcastEpisode = new PodcastEpisodeSnippetResponseDTO
                                {
                                    Id = episode.Id,
                                    Name = episode.Name,
                                    Description = episode.Description,
                                    MainImageFileKey = episode.MainImageFileKey,
                                    IsReleased = episode.IsReleased,
                                    ReleaseDate = episode.ReleaseDate,
                                    AudioLength = episode.AudioLength
                                },
                                Podcaster = new AccountSnippetResponseDTO
                                {
                                    Id = podcaster.Id,
                                    FullName = podcaster.PodcasterProfileName,
                                    Email = podcaster.Email,
                                    MainImageFileKey = podcaster.MainImageFileKey,
                                },
                                AudioFileUrl = deviceInfo.Platform == DevicePlatform.ios.ToString() || deviceInfo.Platform == DevicePlatform.android.ToString()
                                ? await _fileIOHelper.GeneratePresignedUrlAsync(
                                    episode.AudioFileKey, _podcastListenSessionConfig.SessionAudioUrlExpirationSeconds
                                )
                                : null
                            }

                        };
                    }


                    await transaction.CommitAsync();

                    return responseDTO!;

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new Exception("Get latest episode listen session failed, error: " + ex.Message);
                }
            }

        }



        public async Task<ListenPermissionResult> CheckListenerCanListenToEpisodeAsync(int listenerId, PodcastEpisode validEpisode, bool isNonQuotaListeningCheck, List<PodcastSubscriptionBenefitDTO> listenerCurrentPodcastSubscriptionRegistrationBenefitList = null!)
        {
            try
            {
                // 1. Validate Episode/Show/Channel status
                // var validEpisode = await GetValidEpisodeListenPermission(podcastEpisodeId);

                // 2. Get listener account
                var account = await GetAccountById(listenerId);
                if (account == null)
                {
                    return new ListenPermissionResult
                    {
                        CanListen = false,
                        Reason = $"Listener with id {listenerId} does not exist"
                    };
                }

                // 3. Check subscription conditions
                HashSet<PodcastSubscriptionBenefitEnum> listenPermissionConditions = await GetEpisodeListenPermissionConditionsAsync(validEpisode, account);

                // 4. If no special conditions, can listen
                if (listenPermissionConditions.Count == 0)
                {
                    return new ListenPermissionResult
                    {
                        CanListen = true,
                        Reason = null
                    };
                }

                // in ra tất cả các điều kiện cần thiết ban đầu
                foreach (var condition in listenPermissionConditions)
                {
                    Console.WriteLine("Initial listen permission condition: " + condition.ToString());
                    Console.WriteLine("isNonQuotaListeningCheck value: " + isNonQuotaListeningCheck);
                    Console.WriteLine("Condition int value: " + (listenPermissionConditions.Contains(PodcastSubscriptionBenefitEnum.NonQuotaListening)));
                }

                // remove NonQuotaListening condition if not checking for it
                if (isNonQuotaListeningCheck == false && listenPermissionConditions.Contains(PodcastSubscriptionBenefitEnum.NonQuotaListening))
                {
                    listenPermissionConditions.Remove(PodcastSubscriptionBenefitEnum.NonQuotaListening);
                }

                // in ra tất cả các điều kiện cần thiết
                foreach (var condition in listenPermissionConditions)
                {
                    Console.WriteLine("Listen permission condition: " + condition.ToString());
                }
                if (listenPermissionConditions.Count == 0)
                {
                    return new ListenPermissionResult
                    {
                        CanListen = true,
                        Reason = null
                    };
                }

                // 5. Check subscription requirements
                // PodcastSubscriptionDTO channelSubscription = null;
                // PodcastSubscriptionDTO showSubscription = null;

                // if (validEpisode.PodcastShow.PodcastChannelId != null)
                // {
                //     channelSubscription = await GetActivePodcastSubscriptionByChannelId(validEpisode.PodcastShow.PodcastChannelId);
                // }
                // showSubscription = await GetActivePodcastSubscriptionByShowId(validEpisode.PodcastShow.Id);

                // if (channelSubscription == null && showSubscription == null)
                // {
                //     return new ListenPermissionResult
                //     {
                //         CanListen = false,
                //         Reason = "No active subscription available",
                //         MissingConditions = listenPermissionConditions
                //     };
                // }
                // [CHỈNH LẠI] sau này sẽ xem th subscription nào != và dựa vào mảng listenerSubscriptionRegistration để xong cho ở level đó không từ đó so sánh contain
                // ở hàm gọi thì nếu slot còn lại n và không có benefit nonquota thì chỉ giữa lại n item đầu tiên trong mảng đủ điều kiện nghe trả về với n là slot còn lại (nếu ngay từ đầu không có benefit nonquota và slot còn lại là 0 thì trả về order rỗng luôn)
                // 6. Check listener's subscription registration
                // PodcastSubscriptionRegistrationDTO listenerSubscriptionRegistration = channelSubscription != null ?
                //     await GetAccountSubscriptionRegistrationByAccountIdAndSubscriptionId(listenerId, channelSubscription.Id) :
                //     await GetAccountSubscriptionRegistrationByAccountIdAndSubscriptionId(listenerId, showSubscription.Id);


                if (listenerCurrentPodcastSubscriptionRegistrationBenefitList == null || listenerCurrentPodcastSubscriptionRegistrationBenefitList.Count == 0)
                {
                    return new ListenPermissionResult
                    {
                        CanListen = false,
                        Reason = "No subscription registration found",
                        MissingConditions = listenPermissionConditions
                    };
                }

                // 7. Check listener's benefits vs required conditions

                List<int> listenerBenefits = listenerCurrentPodcastSubscriptionRegistrationBenefitList
                                    .Select(psb => psb.Id)
                                    .ToList();
                // in ra tất cả các benefit của listener
                foreach (var benefitId in listenerBenefits)
                {
                    Console.WriteLine("Listener benefit id: " + benefitId);
                }

                HashSet<PodcastSubscriptionBenefitEnum> missingConditions = new HashSet<PodcastSubscriptionBenefitEnum>();
                foreach (var condition in listenPermissionConditions)
                {
                    if (!listenerBenefits.Contains((int)condition))
                    {
                        missingConditions.Add(condition);
                    }
                }

                if (missingConditions.Count > 0)
                {
                    return new ListenPermissionResult
                    {
                        CanListen = false,
                        Reason = $"Insufficient benefits - missing conditions: {string.Join(", ", missingConditions)}",
                        MissingConditions = missingConditions
                    };
                }

                // 8. All checks passed
                return new ListenPermissionResult
                {
                    CanListen = true,
                    Reason = null
                };
            }
            catch (Exception ex)
            {
                return new ListenPermissionResult
                {
                    CanListen = false,
                    Reason = $"Validation failed: {ex.Message}"
                };
            }
        }

        public async Task UpdateEpisodeListenSessionDuration(UpdateEpisodeListenSessionDurationParameterDTO updateEpisodeListenSessionDurationDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                bool transactionCompleted = false;
                try
                {
                    // logic thực hiện cũ:
                    // 1. session phải != null và session isCompleted phải != true nếu không thoả điều kiện ở đâu thì trả exception lỗi tên lỗi
                    // 2. kiểm tra điều kiện nghe bằng CheckListenerCanListenToEpisodeAsync, nếu không thoả thì trả exception tương ứng với tên reason
                    // 3. kiểm tra expiredat của session đã quá hạn chưa:
                    //      + rồi : cập nhật LastListenDurationSeconds + công thêm vào expiredAt với PodcastListenSessionConfig.SessionAdditionalUpdateBufferExpirationMinutes
                    //      + chưa: chỉ cập nhật LastListenDurationSeconds
                    // 4. commit transaction và gửi message saga thành công

                    // logic thực hiện mới:
                    // 1. session phải != null nếu không thoả điều kiện ở đâu thì trả exception lỗi tên lỗi
                    // 2. kiểm tra điều kiện nghe bằng CheckListenerCanListenToEpisodeAsync, nếu không thoả thì trả exception tương ứng với tên reason
                    // 3. kiểm tra expiredat của session đã quá hạn chưa (chỉ thực hiện bước này trên session lấy ra là session gần nhất của listener):
                    //      + rồi : cập nhật LastListenDurationSeconds + công thêm vào expiredAt với PodcastListenSessionConfig.SessionAdditionalUpdateBufferExpirationMinutes
                    //      + chưa: chỉ cập nhật LastListenDurationSeconds
                    // 4. commit transaction và gửi message saga thành công



                    var listenSession = await _podcastEpisodeListenSessionGenericRepository.FindByIdAsync(
                        id: updateEpisodeListenSessionDurationDTO.PodcastEpisodeListenSessionId,
                        includeFunc: null
                    );
                    if (listenSession == null)
                    {
                        throw new Exception($"Podcast episode listen session with id {updateEpisodeListenSessionDurationDTO.PodcastEpisodeListenSessionId} does not exist");
                    }


                    var canListen = await this.CheckListenerCanListenToEpisodeAsync(
                        listenerId: updateEpisodeListenSessionDurationDTO.ListenerId,
                        validEpisode: await GetValidEpisodeListenPermission(listenSession.PodcastEpisodeId),
                        isNonQuotaListeningCheck: false,
                        listenerCurrentPodcastSubscriptionRegistrationBenefitList: updateEpisodeListenSessionDurationDTO.CurrentPodcastSubscriptionRegistrationBenefitList
                    );
                    if (!canListen.CanListen)
                    {
                        // đánh completed cho listen session
                        listenSession.IsCompleted = true;
                        await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(listenSession.Id, listenSession);
                        transactionCompleted = true;
                        await transaction.CommitAsync();

                        throw new Exception($"Listener cannot listen to episode: {canListen.Reason}");
                    }

                    var latestListenSessionOfListener = await _podcastEpisodeListenSessionGenericRepository.FindAll(
                        predicate: pes => pes.AccountId == updateEpisodeListenSessionDurationDTO.ListenerId,
                        includeFunc: pe => pe
                            .Include(pes => pes.PodcastEpisode)
                            .ThenInclude(pe => pe.PodcastShow)
                    )
                        .OrderByDescending(pes => pes.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (latestListenSessionOfListener != null && latestListenSessionOfListener.Id == listenSession.Id && latestListenSessionOfListener.ExpiredAt <= _dateHelper.GetNowByAppTimeZone())
                    {
                        // đã quá hạn
                        listenSession.IsCompleted = false; // đảm bảo session không bị completed
                        listenSession.LastListenDurationSeconds = updateEpisodeListenSessionDurationDTO.LastListenDurationSeconds;
                        listenSession.ExpiredAt = _dateHelper.GetNowByAppTimeZone().AddMinutes(_podcastListenSessionConfig.SessionAdditionalUpdateBufferExpirationMinutes);
                    }
                    else
                    {
                        // chưa quá hạn
                        listenSession.LastListenDurationSeconds = updateEpisodeListenSessionDurationDTO.LastListenDurationSeconds;
                    }
                    await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(listenSession.Id, listenSession);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeListenSessionId"] = updateEpisodeListenSessionDurationDTO.PodcastEpisodeListenSessionId;
                    messageNextRequestData["LastListenDurationSeconds"] = updateEpisodeListenSessionDurationDTO.LastListenDurationSeconds;
                    messageNextRequestData["ListenerId"] = updateEpisodeListenSessionDurationDTO.ListenerId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeListenSessionId = updateEpisodeListenSessionDurationDTO.PodcastEpisodeListenSessionId,
                        LastListenDurationSeconds = updateEpisodeListenSessionDurationDTO.LastListenDurationSeconds,
                        ListenerId = updateEpisodeListenSessionDurationDTO.ListenerId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-episode-listen-session-duration.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    if (!transactionCompleted)
                    {
                        await transaction.RollbackAsync();
                    }
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Update episode listen session duration failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-episode-listen-session-duration.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task TakedownContentDmcaEpisode(TakedownContentDmcaParameterDTO takedownContentDmcaParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingPodcastEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(takedownContentDmcaParameterDTO.PodcastEpisodeId,
                        includeFunc: q => q
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastChannel)
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                    );
                    if (existingPodcastEpisode == null)
                    {
                        throw new Exception("Podcast episode with id " + takedownContentDmcaParameterDTO.PodcastEpisodeId + " does not exist");
                    }
                    else if (existingPodcastEpisode.DeletedAt != null)
                    {
                        throw new Exception("Podcast episode with id " + takedownContentDmcaParameterDTO.PodcastEpisodeId + " has been deleted");
                    }
                    else if (existingPodcastEpisode.PodcastEpisodeStatusTrackings
                        .OrderByDescending(pet => pet.CreatedAt)
                        .FirstOrDefault()
                        .PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Published)
                    {
                        throw new Exception("Podcast episode with id " + takedownContentDmcaParameterDTO.PodcastEpisodeId + " is not in Published status");
                    }


                    if (existingPodcastEpisode.PodcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " does not exist");
                    }
                    else if (existingPodcastEpisode.PodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been deleted");
                    }
                    else if (existingPodcastEpisode.PodcastShow.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnum.Removed)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been removed");
                    }

                    if (existingPodcastEpisode.PodcastShow.PodcastChannel != null)
                    {
                        if (existingPodcastEpisode.PodcastShow.PodcastChannel.DeletedAt != null)
                        {
                            throw new Exception("Podcast channel with id " + existingPodcastEpisode.PodcastShow.PodcastChannelId + " has been deleted");
                        }
                    }

                    var existingPodcaster = await _accountCachingService.GetAccountStatusCacheById(existingPodcastEpisode.PodcastShow.PodcasterId);
                    if (existingPodcaster == null || existingPodcaster.Id != existingPodcastEpisode.PodcastShow.PodcasterId || existingPodcaster.IsVerified == false || existingPodcaster.DeactivatedAt != null || existingPodcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + existingPodcastEpisode.PodcastShow.PodcasterId + " does not exist");
                    }

                    // Change episode status to TakenDown
                    var newStatusTracking = new PodcastEpisodeStatusTracking
                    {
                        PodcastEpisodeId = existingPodcastEpisode.Id,
                        PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.TakenDown
                    };
                    await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);

                    existingPodcastEpisode.TakenDownReason = takedownContentDmcaParameterDTO.TakenDownReason;
                    await _podcastEpisodeGenericRepository.UpdateAsync(existingPodcastEpisode.Id, existingPodcastEpisode);


                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = takedownContentDmcaParameterDTO.PodcastEpisodeId;
                    messageNextRequestData["TakenDownReason"] = takedownContentDmcaParameterDTO.TakenDownReason;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = takedownContentDmcaParameterDTO.PodcastEpisodeId,
                        TakenDownReason = takedownContentDmcaParameterDTO.TakenDownReason,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "takedown-content-dmca.success"
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
                            ErrorMessage = $"Takedown content DMCA episode failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "takedown-content-dmca.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RestoreContentDmcaEpisode(RestoreContentDmcaParameterDTO restoreContentDmcaParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingPodcastEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(restoreContentDmcaParameterDTO.PodcastEpisodeId,
                        includeFunc: q => q
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastChannel)
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                    );
                    if (existingPodcastEpisode == null)
                    {
                        throw new Exception("Podcast episode with id " + restoreContentDmcaParameterDTO.PodcastEpisodeId + " does not exist");
                    }
                    else if (existingPodcastEpisode.DeletedAt != null)
                    {
                        throw new Exception("Podcast episode with id " + restoreContentDmcaParameterDTO.PodcastEpisodeId + " has been deleted");
                    }
                    else if (existingPodcastEpisode.PodcastEpisodeStatusTrackings
                        .OrderByDescending(pet => pet.CreatedAt)
                        .FirstOrDefault()
                        .PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.TakenDown)
                    {
                        throw new Exception("Podcast episode with id " + restoreContentDmcaParameterDTO.PodcastEpisodeId + " is not in TakenDown status");
                    }

                    if (existingPodcastEpisode.PodcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " does not exist");
                    }
                    else if (existingPodcastEpisode.PodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been deleted");
                    }
                    else if (existingPodcastEpisode.PodcastShow.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnum.Removed)
                    {
                        throw new Exception("Podcast show with id " + existingPodcastEpisode.PodcastShow.Id + " has been removed");
                    }

                    if (existingPodcastEpisode.PodcastShow.PodcastChannel != null)
                    {
                        if (existingPodcastEpisode.PodcastShow.PodcastChannel.DeletedAt != null)
                        {
                            throw new Exception("Podcast channel with id " + existingPodcastEpisode.PodcastShow.PodcastChannelId + " has been deleted");
                        }
                    }

                    var existingPodcaster = await _accountCachingService.GetAccountStatusCacheById(existingPodcastEpisode.PodcastShow.PodcasterId);
                    if (existingPodcaster == null || existingPodcaster.Id != existingPodcastEpisode.PodcastShow.PodcasterId || existingPodcaster.IsVerified == false || existingPodcaster.DeactivatedAt != null || existingPodcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + existingPodcastEpisode.PodcastShow.PodcasterId + " does not exist");
                    }

                    // Change episode status to Published
                    var newStatusTracking = new PodcastEpisodeStatusTracking
                    {
                        PodcastEpisodeId = existingPodcastEpisode.Id,
                        PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Published
                    };
                    await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);

                    existingPodcastEpisode.TakenDownReason = null;
                    await _podcastEpisodeGenericRepository.UpdateAsync(existingPodcastEpisode.Id, existingPodcastEpisode);
                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodeId"] = restoreContentDmcaParameterDTO.PodcastEpisodeId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodeId = restoreContentDmcaParameterDTO.PodcastEpisodeId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "restore-content-dmca.success"
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
                            ErrorMessage = $"Restore content DMCA episode failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "restore-content-dmca.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task ReleasePublishEpisodesAsync()
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // lấy tất cả các episode đang ở trạng thái Published và isReleased = false và ReleaseDate != null và chưa bị xoá và và nằm trong show chưa bị xoá với đang Published nằm trong channel chưa bị xoá (channel có thể có hoặc không)   
                    // lấy cột releaseDate ra xem nó có == hôm nay không, nếu có thì chuyển isReleased = true

                    var episodesToRelease = await _podcastEpisodeGenericRepository.FindAll(
                        predicate: pe => pe.IsReleased == false
                                    && pe.ReleaseDate != null
                                    && pe.DeletedAt == null
                                    && pe.PodcastShow.DeletedAt == null
                                    && (pe.PodcastShow.PodcastChannel == null || pe.PodcastShow.PodcastChannel.DeletedAt == null),
                        includeFunc: pe => pe.Include(pe => pe.PodcastEpisodeStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                    ).ToListAsync();

                    foreach (var episode in episodesToRelease)
                    {
                        var currentEpisodeStatusTracking = episode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault();

                        var currentShowStatusTracking = episode.PodcastShow.PodcastShowStatusTrackings
                            .OrderByDescending(pst => pst.CreatedAt)
                            .FirstOrDefault();

                        if (episode.ReleaseDate <= DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone())
                            && currentEpisodeStatusTracking.PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published
                            && currentShowStatusTracking.PodcastShowStatusId == (int)PodcastShowStatusEnum.Published)
                        {
                            episode.IsReleased = true;
                            await _podcastEpisodeGenericRepository.UpdateAsync(episode.Id, episode);
                        }
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new Exception("Release publish shows job failed, error: " + ex.Message);
                }
            }
        }

        public async Task RevertPendingEditRequiredEpisodesAsync()
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Lấy các episode đang ở trong trạng thái Pending Edit Required và created at của status trackig Pending Edit Required lâu hơn ReviewSessionConfig.podcastEpisodePublishEditRequirementExpiredHours giờ trước và chưa bị xoá  và nằm trong show chưa bị xoá và show nằm trong channel chưa bị xoá (channel là optional) , buộc phải đang có publish review session gần nhất đang Pending Review
                    // chuyển episode về Draft
                    // chuyển publish review session gần nhất về Discard
                    var activeSystemConfigProfile = await GetActiveSystemConfigProfile();
                    var expirationTime = _dateHelper.GetNowByAppTimeZone().AddHours(-activeSystemConfigProfile.ReviewSessionConfig.PodcastEpisodePublishEditRequirementExpiredHours);

                    var episodesToRevert = await _podcastEpisodeGenericRepository.FindAll(
                        predicate: pe => pe.DeletedAt == null
                                    && pe.PodcastShow.DeletedAt == null
                                    && (pe.PodcastShow.PodcastChannel == null || pe.PodcastShow.PodcastChannel.DeletedAt == null),
                        includeFunc: pe => pe
                            .Include(pe => pe.PodcastEpisodeStatusTrackings)
                            .Include(pe => pe.PodcastEpisodePublishReviewSessions)
                            .ThenInclude(pers => pers.PodcastEpisodePublishReviewSessionStatusTrackings)
                    ).ToListAsync();

                    foreach (var episode in episodesToRevert)
                    {
                        var currentEpisodeStatusTracking = episode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault();

                        if (currentEpisodeStatusTracking.PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.PendingEditRequired
                            && currentEpisodeStatusTracking.CreatedAt <= expirationTime)
                        {
                            var latestPublishReviewSession = episode.PodcastEpisodePublishReviewSessions
                                .OrderByDescending(pers => pers.CreatedAt)
                                .FirstOrDefault();

                            if (latestPublishReviewSession != null
                                && latestPublishReviewSession.PodcastEpisodePublishReviewSessionStatusTrackings
                                    .OrderByDescending(pesst => pesst.CreatedAt)
                                    .FirstOrDefault()
                                    .PodcastEpisodePublishReviewSessionStatusId == (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview)
                            {
                                // revert episode to Draft
                                var newStatusTracking = new PodcastEpisodeStatusTracking
                                {
                                    PodcastEpisodeId = episode.Id,
                                    PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Draft
                                };
                                await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);

                                // revert publish review session to Discard
                                var newPublishReviewSessionStatusTracking = new PodcastEpisodePublishReviewSessionStatusTracking
                                {
                                    PodcastEpisodePublishReviewSessionId = latestPublishReviewSession.Id,
                                    PodcastEpisodePublishReviewSessionStatusId = (int)PodcastEpisodePublishReviewSessionStatusEnum.Discard
                                };
                                await _podcastEpisodePublishReviewSessionStatusTrackingGenericRepository.CreateAsync(newPublishReviewSessionStatusTracking);
                            }
                        }
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new Exception("Revert pending edit required episodes job failed, error: " + ex.Message);
                }
            }
        }

        public async Task ExpireEpisodeListenSessionsAsync()
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // lấy tất cả listensession có iscompleted =false và expiredat <= now
                    var now = _dateHelper.GetNowByAppTimeZone();
                    var sessionsToExpire = await _podcastEpisodeListenSessionGenericRepository.FindAll(
                        predicate: pes => pes.IsCompleted == false && pes.ExpiredAt <= now
                    ).ToListAsync();

                    foreach (var session in sessionsToExpire)
                    {
                        session.IsCompleted = true;
                        await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(session.Id, session);
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                    throw new Exception("Expire episode listen sessions job failed, error: " + ex.Message);
                }
            }
        }

        public async Task CompleteAllUserEpisodeListenSessions(CompleteAllUserEpisodeListenSessionsParameterDTO completeAllUserEpisodeListenSessionsParameterDTO, SagaCommandMessage command)
        {

            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (completeAllUserEpisodeListenSessionsParameterDTO.IsEpisodeListenSessionCompleted == true)
                    {
                        var sessionsToComplete = await _podcastEpisodeListenSessionGenericRepository.FindAll(
                                                predicate: pes => pes.AccountId == completeAllUserEpisodeListenSessionsParameterDTO.AccountId && pes.IsCompleted == false
                                            ).ToListAsync();

                        foreach (var session in sessionsToComplete)
                        {
                            session.IsCompleted = true;
                            await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(session.Id, session);
                        }
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = completeAllUserEpisodeListenSessionsParameterDTO.AccountId;
                    messageNextRequestData["IsEpisodeListenSessionCompleted"] = completeAllUserEpisodeListenSessionsParameterDTO.IsEpisodeListenSessionCompleted;

                    var messageResponseData = JObject.FromObject(new
                    {
                        AccountId = completeAllUserEpisodeListenSessionsParameterDTO.AccountId,
                        IsEpisodeListenSessionCompleted = completeAllUserEpisodeListenSessionsParameterDTO.IsEpisodeListenSessionCompleted,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "complete-all-user-episode-listen-sessions.success"
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
                            ErrorMessage = $"Complete all user episode listen sessions failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "complete-all-user-episode-listen-sessions.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

    }
    public class ListenPermissionResult
    {
        public bool CanListen { get; set; }
        public string? Reason { get; set; }
        public HashSet<PodcastSubscriptionBenefitEnum>? MissingConditions { get; set; }
    }
}