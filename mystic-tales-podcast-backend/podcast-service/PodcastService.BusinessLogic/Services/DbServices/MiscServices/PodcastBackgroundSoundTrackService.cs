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
using PodcastService.BusinessLogic.DTOs.BackgroundSoundTrack;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateBackgroundSoundTrack;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateBackgroundSoundTrack;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteBackgroundSoundTrack;

namespace PodcastService.BusinessLogic.Services.DbServices.PodcastServices
{
    public class PodcastBackgroundSoundTrackService
    {
        // LOGGER
        private readonly ILogger<PodcastBackgroundSoundTrackService> _logger;

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
        private readonly IGenericRepository<PodcastBackgroundSoundTrack> _podcastBackgroundSoundTrackGenericRepository;

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


        public PodcastBackgroundSoundTrackService(
            ILogger<PodcastBackgroundSoundTrackService> logger,
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
            IGenericRepository<PodcastBackgroundSoundTrack> podcastBackgroundSoundTrackGenericRepository,

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
            _podcastBackgroundSoundTrackGenericRepository = podcastBackgroundSoundTrackGenericRepository;

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

        public async Task<List<PodcastBackgroundSoundTrackDTO>> GetBackgroundSoundTrackAsync()
        {
            try
            {
                var backgroundSoundTracks = await _podcastBackgroundSoundTrackGenericRepository.FindAll(
                    predicate: null,
                    includeFunc: null
                ).ToListAsync();

                return backgroundSoundTracks.Select(bst => new PodcastBackgroundSoundTrackDTO
                {
                    Id = bst.Id,
                    Name = bst.Name,
                    Description = bst.Description,
                    AudioFileKey = bst.AudioFileKey,
                    MainImageFileKey = bst.MainImageFileKey,
                    CreatedAt = bst.CreatedAt,
                    UpdatedAt = bst.UpdatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("An error occurred while processing your request, error: " + ex.Message);
            }
        }

        public async Task CreateBackgroundSoundTrack(CreateBackgroundSoundTrackParameterDTO backgroundSoundTrackCreateInfo, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // copy từ temp location đến final location


                    PodcastBackgroundSoundTrack newBackgroundSoundTrack = new PodcastBackgroundSoundTrack
                    {
                        Name = backgroundSoundTrackCreateInfo.Name,
                        Description = backgroundSoundTrackCreateInfo.Description,
                        MainImageFileKey = backgroundSoundTrackCreateInfo.MainImageFileKey,
                        AudioFileKey = backgroundSoundTrackCreateInfo.AudioFileKey,
                    };

                    await _podcastBackgroundSoundTrackGenericRepository.CreateAsync(newBackgroundSoundTrack);

                    var folderPath = _filePathConfig.PODCAST_BACKGROUND_SOUND_TRACK_FILE_PATH + "\\" + newBackgroundSoundTrack.Id;
                    if (backgroundSoundTrackCreateInfo.MainImageFileKey != null && backgroundSoundTrackCreateInfo.MainImageFileKey != "")
                    {
                        var MainImageFileKey = FilePathHelper.CombinePaths(folderPath, $"main_image{FilePathHelper.GetExtension(backgroundSoundTrackCreateInfo.MainImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(backgroundSoundTrackCreateInfo.MainImageFileKey, MainImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(backgroundSoundTrackCreateInfo.MainImageFileKey);
                        newBackgroundSoundTrack.MainImageFileKey = MainImageFileKey;
                    }
                    if (backgroundSoundTrackCreateInfo.AudioFileKey != null && backgroundSoundTrackCreateInfo.AudioFileKey != "")
                    {
                        var AudioFileKey = FilePathHelper.CombinePaths(folderPath, $"audio{FilePathHelper.GetExtension(backgroundSoundTrackCreateInfo.AudioFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(backgroundSoundTrackCreateInfo.AudioFileKey, AudioFileKey);
                        await _fileIOHelper.DeleteFileAsync(backgroundSoundTrackCreateInfo.AudioFileKey);
                        newBackgroundSoundTrack.AudioFileKey = AudioFileKey;
                    }
                    await _podcastBackgroundSoundTrackGenericRepository.UpdateAsync(newBackgroundSoundTrack.Id, newBackgroundSoundTrack);
                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["Name"] = newBackgroundSoundTrack.Name;
                    messageNextRequestData["Description"] = newBackgroundSoundTrack.Description;
                    messageNextRequestData["MainImageFileKey"] = newBackgroundSoundTrack.MainImageFileKey;
                    messageNextRequestData["AudioFileKey"] = newBackgroundSoundTrack.AudioFileKey;

                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastBackgroundSoundTrackId = newBackgroundSoundTrack.Id,
                        Name = newBackgroundSoundTrack.Name,
                        Description = newBackgroundSoundTrack.Description,
                        MainImageFileKey = newBackgroundSoundTrack.MainImageFileKey,
                        AudioFileKey = newBackgroundSoundTrack.AudioFileKey,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-background-sound-track.success"
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
                            ErrorMessage = $"Create background sound track failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-background-sound-track.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }

        }

        public async Task UpdateBackgroundSoundTrack(UpdateBackgroundSoundTrackParameterDTO backgroundSoundTrackUpdateInfo, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingBackgroundSoundTrack = await _podcastBackgroundSoundTrackGenericRepository.FindByIdAsync(
                        id: backgroundSoundTrackUpdateInfo.PodcastBackgroundSoundTrackId,
                        includeFunc: null
                    );
                    if (existingBackgroundSoundTrack == null)
                    {
                        throw new Exception("Podcast background sound track with id " + backgroundSoundTrackUpdateInfo.PodcastBackgroundSoundTrackId + " does not exist");
                    }



                    existingBackgroundSoundTrack.Name = backgroundSoundTrackUpdateInfo.Name;
                    existingBackgroundSoundTrack.Description = backgroundSoundTrackUpdateInfo.Description;

                    if (backgroundSoundTrackUpdateInfo.MainImageFileKey != null && backgroundSoundTrackUpdateInfo.MainImageFileKey != "")
                    {
                        if (existingBackgroundSoundTrack.MainImageFileKey != null && existingBackgroundSoundTrack.MainImageFileKey != "")
                        {
                            await _fileIOHelper.DeleteFileAsync(existingBackgroundSoundTrack.MainImageFileKey);
                        }
                        var folderPath = _filePathConfig.PODCAST_BACKGROUND_SOUND_TRACK_FILE_PATH + "\\" + existingBackgroundSoundTrack.Id;
                        var MainImageFileKey = FilePathHelper.CombinePaths(folderPath, $"main_image{FilePathHelper.GetExtension(backgroundSoundTrackUpdateInfo.MainImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(backgroundSoundTrackUpdateInfo.MainImageFileKey, MainImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(backgroundSoundTrackUpdateInfo.MainImageFileKey);
                        existingBackgroundSoundTrack.MainImageFileKey = MainImageFileKey;
                    }
                    if (backgroundSoundTrackUpdateInfo.AudioFileKey != null && backgroundSoundTrackUpdateInfo.AudioFileKey != "")
                    {
                        if (existingBackgroundSoundTrack.AudioFileKey != null && existingBackgroundSoundTrack.AudioFileKey != "")
                        {
                            await _fileIOHelper.DeleteFileAsync(existingBackgroundSoundTrack.AudioFileKey);
                        }
                        var folderPath = _filePathConfig.PODCAST_BACKGROUND_SOUND_TRACK_FILE_PATH + "\\" + existingBackgroundSoundTrack.Id;
                        var AudioFileKey = FilePathHelper.CombinePaths(folderPath, $"audio{FilePathHelper.GetExtension(backgroundSoundTrackUpdateInfo.AudioFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(backgroundSoundTrackUpdateInfo.AudioFileKey, AudioFileKey);
                        await _fileIOHelper.DeleteFileAsync(backgroundSoundTrackUpdateInfo.AudioFileKey);
                        existingBackgroundSoundTrack.AudioFileKey = AudioFileKey;
                    }

                    await _podcastBackgroundSoundTrackGenericRepository.UpdateAsync(existingBackgroundSoundTrack.Id, existingBackgroundSoundTrack);
                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastBackgroundSoundTrackId"] = existingBackgroundSoundTrack.Id;
                    messageNextRequestData["Name"] = existingBackgroundSoundTrack.Name;
                    messageNextRequestData["Description"] = existingBackgroundSoundTrack.Description;
                    messageNextRequestData["MainImageFileKey"] = existingBackgroundSoundTrack.MainImageFileKey;
                    messageNextRequestData["AudioFileKey"] = existingBackgroundSoundTrack.AudioFileKey;

                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastBackgroundSoundTrackId = existingBackgroundSoundTrack.Id,
                        Name = existingBackgroundSoundTrack.Name,
                        Description = existingBackgroundSoundTrack.Description,
                        MainImageFileKey = existingBackgroundSoundTrack.MainImageFileKey,
                        AudioFileKey = existingBackgroundSoundTrack.AudioFileKey,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-background-sound-track.success"
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
                            ErrorMessage = $"Update background sound track failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-background-sound-track.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteBackgroundSoundTrack(DeleteBackgroundSoundTrackParameterDTO deleteBackgroundSoundTrackInfo, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingBackgroundSoundTrack = await _podcastBackgroundSoundTrackGenericRepository.FindByIdAsync(
                        id: deleteBackgroundSoundTrackInfo.PodcastBackgroundSoundTrackId,
                        includeFunc: null
                    );
                    if (existingBackgroundSoundTrack == null)
                    {
                        throw new Exception("Podcast background sound track with id " + deleteBackgroundSoundTrackInfo.PodcastBackgroundSoundTrackId + " does not exist");
                    }
                    // if (existingBackgroundSoundTrack.MainImageFileKey != null && existingBackgroundSoundTrack.MainImageFileKey != "")
                    // {
                    //     await _fileIOHelper.DeleteFileAsync(existingBackgroundSoundTrack.MainImageFileKey);
                    // }
                    // if (existingBackgroundSoundTrack.AudioFileKey != null && existingBackgroundSoundTrack.AudioFileKey != "")
                    // {
                    //     await _fileIOHelper.DeleteFileAsync(existingBackgroundSoundTrack.AudioFileKey);
                    // }

                    await _fileIOHelper.DeleteFolderAsync(_filePathConfig.PODCAST_BACKGROUND_SOUND_TRACK_FILE_PATH + "\\" + existingBackgroundSoundTrack.Id);

                    await _podcastBackgroundSoundTrackGenericRepository.DeleteAsync(existingBackgroundSoundTrack.Id);
                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastBackgroundSoundTrackId"] = existingBackgroundSoundTrack.Id;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastBackgroundSoundTrackId = existingBackgroundSoundTrack.Id,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-background-sound-track.success"
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
                            ErrorMessage = $"Delete background sound track failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-background-sound-track.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }
    }
}