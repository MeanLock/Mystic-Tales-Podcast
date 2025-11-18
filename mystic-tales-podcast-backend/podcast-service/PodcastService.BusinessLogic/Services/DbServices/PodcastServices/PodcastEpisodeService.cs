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
        private readonly IGenericRepository<PodcastEpisodeIllegalContentTypeMarking> _podcastEpisodeIllegalContentTypeMarkingGenericRepository;
        private readonly IGenericRepository<PodcastEpisodePublishReviewSessionStatusTracking> _podcastEpisodePublishReviewSessionStatusTrackingGenericRepository;
        private readonly IGenericRepository<PodcastEpisodeListenSession> _podcastEpisodeListenSessionGenericRepository;
        private readonly IGenericRepository<PodcastEpisodeListenSessionHlsEnckeyRequestToken> _podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository;

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
        private readonly FFMpegCoreHlsService _ffMpegCoreHlsService;
        private readonly AudioTuningService _audioTuningService;


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

            HttpServiceQueryClient httpServiceQueryClient,

            AccountCachingService accountCachingService,

            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,

            RedisSharedCacheService redisSharedCacheService,

            AudioTranscriptionService audioTranscriptionService,
            AcoustIDAudioFingerprintGenerator audioFingerprintService,
            AcoustIDAudioFingerprintComparator audioFingerprintComparator,
            FFMpegCoreHlsService ffMpegCoreHlsService,
            AudioTuningService audioTuningService
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

            _httpServiceQueryClient = httpServiceQueryClient;

            _accountCachingService = accountCachingService;

            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;

            _redisSharedCacheService = redisSharedCacheService;

            _audioTranscriptionService = audioTranscriptionService;
            _acoustIDAudioFingerprintGenerator = audioFingerprintService;
            _acoustIDAudioFingerprintComparator = audioFingerprintComparator;
            _ffMpegCoreHlsService = ffMpegCoreHlsService;
            _audioTuningService = audioTuningService;
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
                throw new HttpRequestException("Get all available staffs failed, error: " + ex.Message);
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
                        // lọc các episode đã published hoặc taken down, khác episode hiện tại, khác podcaster
                        var publishedEpisode = await _podcastEpisodeGenericRepository.FindAll(
                            predicate: pe => pe.Id != existingPodcastEpisode.Id &&
                                pe.AudioFingerPrint != null &&
                                pe.DeletedAt == null &&
                                pe.PodcastShow.PodcasterId != existingPodcastEpisode.PodcastShow.PodcasterId, // không so sánh với episode của cùng podcaster
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
                        ).ToListAsync();

                        publishedEpisode = publishedEpisode.Where(pe =>
                        {
                            var latestStatusId = pe.PodcastEpisodeStatusTrackings
                                .OrderByDescending(pet => pet.CreatedAt)
                                .FirstOrDefault()
                                .PodcastEpisodeStatusId;
                            return latestStatusId == (int)PodcastEpisodeStatusEnum.Published ||
                                   latestStatusId == (int)PodcastEpisodeStatusEnum.TakenDown;
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

                            // Lọc ra các staff id đã được assign vào các phiên review trước đó trong hệ thống
                            // từ đó so với danh sách available staff truy vấn được từ Userservice để group lại các staff ít được assign nhất, nếu danh sách > 1 thì random chọn
                            List<int> assignedStaffIds = await _podcastEpisodePublishReviewSessionGenericRepository.FindAll(
                                predicate: null,
                                includeFunc: null
                            ).Select(pprs => pprs.AssignedStaff).ToListAsync();

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
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task<PodcastEpisode> GetValidEpisodeListenPermission(Guid podcastEpisodeId, int listenerAccountId)
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
            if (podcastEpisode.IsReleased == false)
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

        public async Task<EpisodeListenResponseDTO> GetEpisodeListenAsync(Guid podcastEpisodeId, int listenerAccountId, string? token, DeviceInfoDTO deviceInfo)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                bool transactionCompleted = false;
                try
                {
                    // nếu có token:
                    // + kiểm tra token phải hợp lệ với secret key và có trường ExpiredAt chưa hết hạn
                    // + lấy trường SessionId từ token để kiểm tra session tồn tại và IsCompleted = false 
                    // + Check điều kiện cần đề nghe (alway cho mọi trường hợp): 
                    //      + check channel nếu có thì deleted == null và phải đang publish
                    //      + check show deleted == null và phải đang publish
                    //      + check episode deleted == null và phải đang publish
                    // + Check điều kiện liên quan đến subscription type của show và episode đang yêu cầu nghe, để append vào danh sách điều kiện (kiểu Set PodcastSubscriptionBenefitEnum điều kiện cần):
                    //      + isreleased = false (Show) : PodcastSubscriptionBenefitEnum.ShowsEpisodesEarlyAccess
                    //      + PodcastListenSlot == 0 : PodcastSubscriptionBenefitEnum.NonQuotaListening
                    //      + Subscriber only (PodcastShowSubscriptionType) : PodcastSubscriptionBenefitEnum.SubscriberOnlyShows
                    //      + Subscriber only (PodcastEpisodeSubscriptionType) : PodcastSubscriptionBenefitEnum.SubscriberOnlyEpisodes
                    //      + Bonus (PodcastEpisodeSubscriptionType) : PodcastSubscriptionBenefitEnum.BonusEpisodes
                    //      + Archive (PodcastEpisodeSubscriptionType) : PodcastSubscriptionBenefitEnum.ArchiveEpisodesAccess
                    //  * nếu không tồn tại điều kiện nào trong số trên thì cho nghe bình thường
                    //  * nếu tồn tại điều kiện và Show có ít nhất 1 gói subscription đang active hoặc channel (nếu channel != null) có ít nhất 1 gói subscription đang active, thì query vào subscription service để kiểm tra listenerAccountId đang đăng kí 1 trong 2 gói subscription đó hay không:
                    //      + nếu không thì từ chối nghe + set IsCompleted = true cho session
                    //      + nếu có thì lấy ra danh sách beneifit của gói đó để kiểm tra với danh sách điều kiện cần:
                    //            + nếu bao gồm tất cả các điều kiện cần thì cho nghe
                    //            + nếu không bao gồm tất cả các điều kiện cần thì từ chối nghe + set IsCompleted = true cho session
                    // + tạo token mới với (SessionId, IsUsed = false, ExpiredAt = now + PodcastListenSessionConfig.TokenEncryptionKeyRequestExpirationMinutes)
                    // + update token vào session
                    // + gọi hàm cập nhật listenCount ở các đối tượng liên quan
                    // + trả về token mới  + playlist file key
                    // nếu không có token:
                    // + Check điều kiện cần đề nghe như trên:
                    //      + nếu không tồn tại điều kiện nào trong số trên thì cho nghe bình thường
                    //      + nếu tồn tại điều kiện và Show có ít nhất 1 gói subscription đang active hoặc channel (nếu channel != null) có ít nhất 1 gói subscription đang active, thì query vào subscription service để kiểm tra listenerAccountId đang đăng kí 1 trong 2 gói subscription đó hay không:
                    //          + nếu không thì từ chối nghe
                    //          + nếu có thì lấy ra danh sách beneifit của gói đó để kiểm tra với danh sách điều kiện cần:
                    //              + nếu bao gồm tất cả các điều kiện cần thì cho nghe
                    //              + nếu không bao gồm tất cả các điều kiện cần thì từ chối nghe
                    // + tạo mới session với IsCompleted = false và ExpiredAt = now + PodcastListenSessionConfig.SessionExpirationMinutes
                    // + đánh isCompleted = true ở tất cả các session cũ chưa completed của episode này và account này
                    // + tạo token mới với (SessionId, IsUsed = false, ExpiredAt = now + PodcastListenSessionConfig.TokenEncryptionKeyRequestExpirationMinutes)
                    // + update token vào session
                    // + gọi hàm cập nhật listenCount ở các đối tượng liên quan
                    // + trả về token mới  + playlist file key

                    // kiểm tra token sơ bộ
                    ClaimsPrincipal? principal = null;
                    if (token != null)
                    {
                        principal = _jwtHelper.DecodeToken_OneSecretKey(token, _podcastListenSessionConfig.TokenSecretKey);
                        // kiểm tra session
                        var sessionId = principal?.FindFirst("SessionId")?.Value;
                        var existingSession = await (_podcastEpisodeListenSessionGenericRepository.FindAll(
                            predicate: pes => pes.Id.ToString() == sessionId && pes.AccountId == listenerAccountId && pes.IsCompleted == false,
                            includeFunc: null
                        )).FirstOrDefaultAsync();
                        if (existingSession == null)
                        {
                            throw new Exception("Invalid or already used token, session does not exist or already completed");
                        }


                        var validEpisode = await GetValidEpisodeListenPermission(podcastEpisodeId, listenerAccountId);
                        var podcaster = await _accountCachingService.GetAccountStatusCacheById(validEpisode.PodcastShow.PodcasterId);
                        var playlistFileKey = FilePathHelper.CombinePaths(
                                        _filePathConfig.PODCAST_EPISODE_FILE_PATH,
                                        validEpisode.Id.ToString(),
                                        "playlist",
                                        _hlsConfig.PlaylistFileName
                                    );
                        string sessionToken = null;
                        var account = await GetAccountById(listenerAccountId);
                        if (account == null)
                        {
                            throw new Exception("Listener with id " + listenerAccountId + " does not exist");
                        }
                        HashSet<PodcastSubscriptionBenefitEnum> listenPermissionConditions = await GetEpisodeListenPermissionConditionsAsync(validEpisode, account);
                        if (listenPermissionConditions.Count == 0)
                        {
                            // sessionToken = GenerateEpisodeListenToken(existingSession.Id, false);
                            // existingSession.Token = sessionToken;
                            // await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(existingSession.Id, existingSession);
                            sessionToken = GenerateEpisodeListenHlsEnckeyRequestToken(existingSession.Id);
                            await _podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository.CreateAsync(new PodcastEpisodeListenSessionHlsEnckeyRequestToken
                            {
                                PodcastEpisodeListenSessionId = existingSession.Id,
                                Token = sessionToken,
                                IsUsed = false,
                            });
                        }
                        else
                        {
                            PodcastSubscriptionDTO channelSubscription = null;
                            PodcastSubscriptionDTO showSubscription = null;
                            // kiểm tra điều kiện subscription
                            if (validEpisode.PodcastShow.PodcastChannelId != null)
                            {
                                channelSubscription = await GetActivePodcastSubscriptionByChannelId(validEpisode.PodcastShow.PodcastChannelId);
                            }
                            showSubscription = await GetActivePodcastSubscriptionByShowId(validEpisode.PodcastShow.Id);

                            if (channelSubscription == null && showSubscription == null)
                            {
                                // không có gói subscription active nào => từ chối nghe
                                existingSession.IsCompleted = true;
                                await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(existingSession.Id, existingSession);
                                await transaction.CommitAsync();
                                transactionCompleted = true;
                                throw new Exception("Listener does not have permission to listen to this episode, reason: no active subscription");
                            }
                            else
                            {
                                // nếu có gói channel subscription active thì chỉ cần 1 query vào channel subscription , nếu không thì query vào show subscription
                                PodcastSubscriptionRegistrationDTO listenerSubscriptionRegistration = channelSubscription != null ?
                                    await GetAccountSubscriptionRegistrationByAccountIdAndSubscriptionId(listenerAccountId, channelSubscription.Id) :
                                    await GetAccountSubscriptionRegistrationByAccountIdAndSubscriptionId(listenerAccountId, showSubscription.Id);

                                if (listenerSubscriptionRegistration == null)
                                {
                                    // không đăng kí gói subscription active nào => từ chối nghe
                                    existingSession.IsCompleted = true;
                                    await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(existingSession.Id, existingSession);
                                    await transaction.CommitAsync();
                                    transactionCompleted = true;
                                    throw new Exception("Listener does not have permission to listen to this episode, reason: no subscription registration");
                                }
                                else
                                {
                                    // kiểm tra benefit đang có 
                                    List<int> listenerBenefits = listenerSubscriptionRegistration.PodcastSubscription.PodcastSubscriptionBenefitMappings
                                        .Where(psbm => psbm.Version == listenerSubscriptionRegistration.CurrentVersion)
                                        .Select(psbm => psbm.PodcastSubscriptionBenefitId)
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
                                        existingSession.IsCompleted = true;
                                        await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(existingSession.Id, existingSession);
                                        await transaction.CommitAsync();
                                        transactionCompleted = true;
                                        throw new Exception("Listener does not have permission to listen to this episode, reason: insufficient benefits - missing conditions: " + string.Join(", ", missingConditions));
                                    }
                                    else
                                    {
                                        // sessionToken = GenerateEpisodeListenToken(existingSession.Id, false);
                                        // existingSession.Token = sessionToken;
                                        // await _podcastEpisodeListenSessionGenericRepository.UpdateAsync(existingSession.Id, existingSession);
                                        sessionToken = GenerateEpisodeListenHlsEnckeyRequestToken(existingSession.Id);
                                        await _podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository.CreateAsync(new PodcastEpisodeListenSessionHlsEnckeyRequestToken
                                        {
                                            PodcastEpisodeListenSessionId = existingSession.Id,
                                            Token = sessionToken,
                                            IsUsed = false,
                                        });
                                    }
                                }

                            }

                        }

                        // Cập nhật listenCount ở các đối tượng liên quan
                        // await UpdateListenCountAsync(validEpisode, account, podcaster);
                        await transaction.CommitAsync();
                        transactionCompleted = true;
                        return new EpisodeListenResponseDTO
                        {
                            // Token = existingSession.Token,
                            Token = sessionToken,
                            PlaylistFileKey = playlistFileKey,
                            PodcastEpisodeListenSession = new PodcastEpisodeListenSessionSnippetResponseDTO
                            {
                                Id = existingSession.Id,
                                LastListenDurationSeconds = existingSession.LastListenDurationSeconds
                            },
                            PodcastEpisode = new PodcastEpisodeSnippetResponseDTO
                            {
                                Id = validEpisode.Id,
                                Name = validEpisode.Name,
                                Description = validEpisode.Description,
                                MainImageFileKey = validEpisode.MainImageFileKey,
                                IsReleased = validEpisode.IsReleased,
                                ReleaseDate = validEpisode.ReleaseDate,
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
                                    validEpisode.AudioFileKey
                                )
                                : null
                        };
                    }
                    else // token == null
                    {
                        var validEpisode = await GetValidEpisodeListenPermission(podcastEpisodeId, listenerAccountId);
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
                                IsContentRemoved = false,
                                ExpiredAt = _dateHelper.GetNowByAppTimeZone().AddMinutes(_podcastListenSessionConfig.SessionExpirationMinutes)
                            };

                            // sessionToken = GenerateEpisodeListenToken(newSession.Id, false);
                            // newSession.Token = sessionToken;
                            await _podcastEpisodeListenSessionGenericRepository.CreateAsync(newSession);
                            sessionToken = GenerateEpisodeListenHlsEnckeyRequestToken(newSession.Id);
                            await _podcastEpisodeListenSessionHlsEnckeyRequestTokenGenericRepository.CreateAsync(new PodcastEpisodeListenSessionHlsEnckeyRequestToken
                            {
                                PodcastEpisodeListenSessionId = newSession.Id,
                                Token = sessionToken,
                                IsUsed = false,
                            });

                            await UpdateListenCountAsync(validEpisode, account, podcaster, new List<int>());


                        }
                        else
                        {
                            PodcastSubscriptionDTO channelSubscription = null;
                            PodcastSubscriptionDTO showSubscription = null;

                            // kiểm tra điều kiện subscription
                            if (validEpisode.PodcastShow.PodcastChannelId != null)
                            {
                                channelSubscription = await GetActivePodcastSubscriptionByChannelId(validEpisode.PodcastShow.PodcastChannelId);
                            }
                            showSubscription = await GetActivePodcastSubscriptionByShowId(validEpisode.PodcastShow.Id);

                            if (channelSubscription == null && showSubscription == null)
                            {
                                // không có gói subscription active nào => từ chối nghe
                                await transaction.CommitAsync();
                                transactionCompleted = true;
                                throw new Exception("Listener does not have permission to listen to this episode, reason: no active subscription");
                            }
                            else
                            {
                                // nếu có gói channel subscription active thì chỉ cần 1 query vào channel subscription , nếu không thì query vào show subscription
                                PodcastSubscriptionRegistrationDTO listenerSubscriptionRegistration = channelSubscription != null ?
                                    await GetAccountSubscriptionRegistrationByAccountIdAndSubscriptionId(listenerAccountId, channelSubscription.Id) :
                                    await GetAccountSubscriptionRegistrationByAccountIdAndSubscriptionId(listenerAccountId, showSubscription.Id);

                                if (listenerSubscriptionRegistration == null)
                                {
                                    // không đăng kí gói subscription active nào => từ chối nghe
                                    await transaction.CommitAsync();
                                    transactionCompleted = true;
                                    throw new Exception("Listener does not have permission to listen to this episode, reason: no subscription registration");
                                }
                                else
                                {
                                    // kiểm tra benefit đang có 
                                    List<int> listenerBenefits = listenerSubscriptionRegistration.PodcastSubscription.PodcastSubscriptionBenefitMappings
                                        .Where(psbm => psbm.Version == listenerSubscriptionRegistration.CurrentVersion)
                                        .Select(psbm => psbm.PodcastSubscriptionBenefitId)
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
                                            ExpiredAt = _dateHelper.GetNowByAppTimeZone().AddMinutes(_podcastListenSessionConfig.SessionExpirationMinutes)
                                        };

                                        // sessionToken = GenerateEpisodeListenToken(newSession.Id, false);
                                        // newSession.Token = sessionToken;
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
                            }
                        }

                        // Cập nhật listenCount ở các đối tượng liên quan
                        await transaction.CommitAsync();
                        transactionCompleted = true;

                        return new EpisodeListenResponseDTO
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
                                    validEpisode.AudioFileKey
                                )
                                : null
                        };
                    }
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

                        if (currentStatusTracking.PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Published && currentStatusTracking.PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.TakenDown)
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

                        if (currentStatusTracking.PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Draft)
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
                            ReleaseDate = pes.PodcastEpisode.ReleaseDate
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
                        includeFunc: pe => pe
                            .Include(pes => pes.PodcastEpisode)
                            .ThenInclude(pe => pe.PodcastShow)
                    )
                        .OrderByDescending(pes => pes.CreatedAt)
                        .FirstOrDefaultAsync();
                    if (latestListenSession == null || latestListenSession.IsCompleted == true || latestListenSession.ExpiredAt <= _dateHelper.GetNowByAppTimeZone())
                    {
                        return null!;
                    }
                    var episode = latestListenSession.PodcastEpisode;
                    var canListen = await this.CheckListenerCanListenToEpisodeAsync(
                        listenerId: listenerId,
                        podcastEpisodeId: episode.Id
                    );

                    EpisodeListenResponseDTO responseDTO = null!;
                    if (!canListen.CanListen)
                    {
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


                        responseDTO = new EpisodeListenResponseDTO
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
                                ReleaseDate = episode.ReleaseDate
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
                                    episode.AudioFileKey
                                )
                                : null
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



        public async Task<ListenPermissionResult> CheckListenerCanListenToEpisodeAsync(int listenerId, Guid podcastEpisodeId)
        {
            try
            {
                // 1. Validate Episode/Show/Channel status
                var validEpisode = await GetValidEpisodeListenPermission(podcastEpisodeId, listenerId);

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

                // 5. Check subscription requirements
                PodcastSubscriptionDTO channelSubscription = null;
                PodcastSubscriptionDTO showSubscription = null;

                if (validEpisode.PodcastShow.PodcastChannelId != null)
                {
                    channelSubscription = await GetActivePodcastSubscriptionByChannelId(validEpisode.PodcastShow.PodcastChannelId);
                }
                showSubscription = await GetActivePodcastSubscriptionByShowId(validEpisode.PodcastShow.Id);

                if (channelSubscription == null && showSubscription == null)
                {
                    return new ListenPermissionResult
                    {
                        CanListen = false,
                        Reason = "No active subscription available",
                        MissingConditions = listenPermissionConditions
                    };
                }

                // 6. Check listener's subscription registration
                PodcastSubscriptionRegistrationDTO listenerSubscriptionRegistration = channelSubscription != null ?
                    await GetAccountSubscriptionRegistrationByAccountIdAndSubscriptionId(listenerId, channelSubscription.Id) :
                    await GetAccountSubscriptionRegistrationByAccountIdAndSubscriptionId(listenerId, showSubscription.Id);

                if (listenerSubscriptionRegistration == null)
                {
                    return new ListenPermissionResult
                    {
                        CanListen = false,
                        Reason = "No subscription registration found",
                        MissingConditions = listenPermissionConditions
                    };
                }

                // 7. Check listener's benefits vs required conditions
                List<int> listenerBenefits = listenerSubscriptionRegistration.PodcastSubscription.PodcastSubscriptionBenefitMappings
                    .Where(psbm => psbm.Version == listenerSubscriptionRegistration.CurrentVersion)
                    .Select(psbm => psbm.PodcastSubscriptionBenefitId)
                    .ToList();

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
                        includeFunc: pe => pe
                            .Include(pes => pes.PodcastEpisode)
                            .ThenInclude(pe => pe.PodcastShow)
                    );
                    if (listenSession == null)
                    {
                        throw new Exception($"Podcast episode listen session with id {updateEpisodeListenSessionDurationDTO.PodcastEpisodeListenSessionId} does not exist");
                    }
                    // if (listenSession.IsCompleted)
                    // {
                    //     throw new Exception($"Podcast episode listen session with id {updateEpisodeListenSessionDurationDTO.PodcastEpisodeListenSessionId} is already completed");
                    // }
                    var canListen = await this.CheckListenerCanListenToEpisodeAsync(
                        listenerId: updateEpisodeListenSessionDurationDTO.ListenerId,
                        podcastEpisodeId: listenSession.PodcastEpisodeId
                    );
                    if (!canListen.CanListen)
                    {
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
                    await transaction.RollbackAsync();
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

    }
    public class ListenPermissionResult
    {
        public bool CanListen { get; set; }
        public string? Reason { get; set; }
        public HashSet<PodcastSubscriptionBenefitEnum>? MissingConditions { get; set; }
    }
}