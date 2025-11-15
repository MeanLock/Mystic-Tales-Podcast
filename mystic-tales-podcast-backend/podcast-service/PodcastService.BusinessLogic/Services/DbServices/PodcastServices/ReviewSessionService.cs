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
using PodcastService.BusinessLogic.DTOs.Channel;
using HotChocolate.Execution.Processing;
using PodcastService.BusinessLogic.DTOs.ReviewSession.ListItems;
using PodcastService.BusinessLogic.DTOs.ReviewSession.Details;
using PodcastService.BusinessLogic.DTOs.ReviewSession;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.RequireEpisodePublishReviewSessionEdit;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.AcceptEpisodePublishReviewSession;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.RejectEpisodePublishReviewSession;
using PodcastService.BusinessLogic.Services.DbServices.CachingServices;
using PodcastService.BusinessLogic.DTOs.SystemConfiguration;

namespace PodcastService.BusinessLogic.Services.DbServices.PodcastServices
{
    public class ReviewSessionService
    {
        // LOGGER
        private readonly ILogger<ReviewSessionService> _logger;

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
        private readonly IGenericRepository<PodcastEpisodeIllegalContentTypeMarking> _podcastEpisodeIllegalContentTypeMarkingGenericRepository;
        private readonly IGenericRepository<PodcastIllegalContentType> _podcastIllegalContentTypeGenericRepository;
        private readonly IGenericRepository<PodcastEpisodePublishReviewSessionStatusTracking> _podcastEpisodePublishReviewSessionStatusTrackingGenericRepository;

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


        public ReviewSessionService(
            ILogger<ReviewSessionService> logger,
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
            IGenericRepository<PodcastIllegalContentType> podcastIllegalContentTypeGenericRepository,
            IGenericRepository<PodcastEpisodePublishReviewSessionStatusTracking> podcastEpisodePublishReviewSessionStatusTrackingGenericRepository,

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
            AcoustIDAudioFingerprintComparator audioFingerprintComparator,
            FFMpegCoreHlsService ffMpegCoreHlsService
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
            _podcastIllegalContentTypeGenericRepository = podcastIllegalContentTypeGenericRepository;
            _podcastEpisodePublishReviewSessionStatusTrackingGenericRepository = podcastEpisodePublishReviewSessionStatusTrackingGenericRepository;

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
            _ffMpegCoreHlsService = ffMpegCoreHlsService;
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



        public async Task<List<EpisodePublishReviewSessionListItemResponseDTO>> GetEpisodePublishReviewSessionsByPodcasterIdAsync(AccountStatusCache requestAccount)
        {
            try
            {
                var reviewSessionsQuery = _podcastEpisodePublishReviewSessionGenericRepository.FindAll(
                    predicate: null,
                    includeFunc: q => q
                        .Include(pers => pers.PodcastEpisodePublishReviewSessionStatusTrackings)
                        .ThenInclude(pes => pes.PodcastEpisodePublishReviewSessionStatus)
                        .Include(pers => pers.PodcastEpisode)
                        .ThenInclude(pe => pe.PodcastEpisodeStatusTrackings)
                        .Include(pers => pers.PodcastEpisode)
                        .ThenInclude(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(pers => pers.PodcastEpisode)
                        .ThenInclude(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastChannel)
                        .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                );

                if (requestAccount.RoleId == (int)RoleEnum.Staff)
                {
                    reviewSessionsQuery = reviewSessionsQuery.Where(pers =>
                        pers.AssignedStaff == requestAccount.Id
                    );
                }


                var reviewSessions = (await reviewSessionsQuery.ToListAsync()).Select(ps => new EpisodePublishReviewSessionListItemResponseDTO
                {
                    Id = ps.Id,
                    AssignedStaff = ps.AssignedStaff,
                    CreatedAt = ps.CreatedAt,
                    UpdatedAt = ps.UpdatedAt,
                    Deadline = ps.Deadline,
                    Note = ps.Note,
                    ReReviewCount = ps.ReReviewCount,
                    PodcastEpisode = new PodcastEpisodeSnippetResponseDTO
                    {
                        Id = ps.PodcastEpisode.Id,
                        Name = ps.PodcastEpisode.Name,
                        Description = ps.PodcastEpisode.Description,
                        MainImageFileKey = ps.PodcastEpisode.MainImageFileKey,
                        IsReleased = ps.PodcastEpisode.IsReleased,
                        ReleaseDate = ps.PodcastEpisode.ReleaseDate
                    },
                    CurrentStatus = ps.PodcastEpisodePublishReviewSessionStatusTrackings
                        .OrderByDescending(s => s.CreatedAt).Select(s => new PodcastEpisodePublishReviewSessionStatusDTO
                        {
                            Id = s.PodcastEpisodePublishReviewSessionStatusId,
                            Name = s.PodcastEpisodePublishReviewSessionStatus.Name
                        })
                        .FirstOrDefault()!

                }).ToList();
                return reviewSessions;

            }
            catch (Exception ex)
            {
                throw new Exception("Get episode publish review sessions by podcaster id failed, error: " + ex.Message);
            }
        }

        public async Task<EpisodePublishReviewSessionDetailResponseDTO> GetEpisodePublishReviewSessionByIdAsync(int reviewSessionId, AccountStatusCache requestAccount)
        {
            try
            {
                var reviewSessionQuery = _podcastEpisodePublishReviewSessionGenericRepository.FindAll(
                    // predicate: pers => pers.Id == reviewSessionId && pers.PodcastEpisodePublishDuplicateDetections.Any(d => d.DuplicatePodcastEpisode.PodcastEpisodeStatusTrackings.OrderByDescending(s => s.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published || d.DuplicatePodcastEpisode.PodcastEpisodeStatusTrackings.OrderByDescending(s => s.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.TakenDown),
                    predicate: pers => pers.Id == reviewSessionId,
                    includeFunc: q => q
                        .Include(pers => pers.PodcastEpisode)
                        .ThenInclude(pe => pe.PodcastEpisodeStatusTrackings)
                        .ThenInclude(pe => pe.PodcastEpisodeStatus)
                        .Include(pers => pers.PodcastEpisode)
                        .ThenInclude(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .ThenInclude(pst => pst.PodcastShowStatus)
                        .Include(pers => pers.PodcastEpisode)
                        .ThenInclude(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastChannel)
                        .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                        .ThenInclude(pct => pct.PodcastChannelStatus)
                        .Include(pers => pers.PodcastEpisode)
                        .ThenInclude(pe => pe.PodcastEpisodeIllegalContentTypeMarkings)
                        .ThenInclude(pictm => pictm.PodcastIllegalContentType)
                        .Include(pers => pers.PodcastEpisodePublishDuplicateDetections)
                        .ThenInclude(ped => ped.DuplicatePodcastEpisode)
                        .ThenInclude(dpe => dpe.PodcastEpisodeStatusTrackings)
                        .Include(pers => pers.PodcastEpisodePublishReviewSessionStatusTrackings)
                        .ThenInclude(pes => pes.PodcastEpisodePublishReviewSessionStatus)
                );


                if (requestAccount.RoleId == (int)RoleEnum.Staff)
                {
                    reviewSessionQuery = reviewSessionQuery.Where(pers =>
                        pers.AssignedStaff == requestAccount.Id
                    );
                }

                var reviewSession = await reviewSessionQuery.FirstOrDefaultAsync();
                // trong các episode của PodcastEpisodePublishDuplicateDetections chỉ lấy các episode trong trạng thái cuối cùng là  "Published" và taken down
                reviewSession.PodcastEpisodePublishDuplicateDetections = reviewSession.PodcastEpisodePublishDuplicateDetections
                    .Where(d => d.DuplicatePodcastEpisode.PodcastEpisodeStatusTrackings
                        .OrderByDescending(s => s.CreatedAt)
                        .FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published ||
                        d.DuplicatePodcastEpisode.PodcastEpisodeStatusTrackings
                        .OrderByDescending(s => s.CreatedAt)
                        .FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.TakenDown)
                    .ToList();

                if (reviewSession == null)
                {
                    throw new Exception("Review session not found");
                }

                var podcaster = await _accountCachingService.GetAccountStatusCacheById(reviewSession.PodcastEpisode.PodcastShow.PodcastChannel.PodcasterId);

                var episodeCurrentStatus = reviewSession.PodcastEpisode.PodcastEpisodeStatusTrackings
                    .OrderByDescending(s => s.CreatedAt).Select(s => new PodcastEpisodeStatusDTO
                    {
                        Id = s.PodcastEpisodeStatusId,
                        Name = s.PodcastEpisodeStatus.Name
                    })
                    .FirstOrDefault();

                var showCurrentStatus = reviewSession.PodcastEpisode.PodcastShow.PodcastShowStatusTrackings
                    .OrderByDescending(s => s.CreatedAt).Select(s => new PodcastShowStatusDTO
                    {
                        Id = s.PodcastShowStatusId,
                        Name = s.PodcastShowStatus.Name
                    })
                    .FirstOrDefault();

                var channelCurrentStatus = reviewSession.PodcastEpisode.PodcastShow.PodcastChannel != null ? reviewSession.PodcastEpisode.PodcastShow.PodcastChannel.PodcastChannelStatusTrackings
                    .OrderByDescending(s => s.CreatedAt).Select(s => new PodcastChannelStatusDTO
                    {
                        Id = s.PodcastChannelStatusId,
                        Name = s.PodcastChannelStatus.Name
                    })
                    .FirstOrDefault() : null;

                var restrictedTerms = await GetAllPodcastRestrictedTerms();
                List<string> detectedRestrictTerms = ScanTranscriptionForRestrictedTerms(reviewSession.PodcastEpisode.AudioTranscript, restrictedTerms);

                var sessionCurrentStatus = reviewSession.PodcastEpisodePublishReviewSessionStatusTrackings
                    .OrderByDescending(s => s.CreatedAt).Select(s => new PodcastEpisodePublishReviewSessionStatusDTO
                    {
                        Id = s.PodcastEpisodePublishReviewSessionStatusId,
                        Name = s.PodcastEpisodePublishReviewSessionStatus.Name
                    })
                    .FirstOrDefault();
                return new EpisodePublishReviewSessionDetailResponseDTO
                {
                    Id = reviewSession.Id,
                    AssignedStaff = reviewSession.AssignedStaff,
                    CreatedAt = reviewSession.CreatedAt,
                    UpdatedAt = reviewSession.UpdatedAt,
                    Deadline = reviewSession.Deadline,
                    Note = reviewSession.Note,
                    ReReviewCount = reviewSession.ReReviewCount,
                    PodcastEpisode = new PodcastEpisodeDTO
                    {
                        Id = reviewSession.PodcastEpisode.Id,
                        Name = reviewSession.PodcastEpisode.Name,
                        Description = reviewSession.PodcastEpisode.Description,
                        MainImageFileKey = reviewSession.PodcastEpisode.MainImageFileKey,
                        AudioFileKey = reviewSession.PodcastEpisode.AudioFileKey,
                        AudioFileSize = reviewSession.PodcastEpisode.AudioFileSize,
                        AudioFingerPrint = reviewSession.PodcastEpisode.AudioFingerPrint,
                        AudioLength = reviewSession.PodcastEpisode.AudioLength,
                        EpisodeOrder = reviewSession.PodcastEpisode.EpisodeOrder,
                        ReleaseDate = reviewSession.PodcastEpisode.ReleaseDate,
                        ExplicitContent = reviewSession.PodcastEpisode.ExplicitContent,
                        IsAudioPublishable = reviewSession.PodcastEpisode.IsAudioPublishable,
                        TakenDownReason = reviewSession.PodcastEpisode.TakenDownReason,
                        PodcastShowId = reviewSession.PodcastEpisode.PodcastShowId,
                        AudioTranscript = reviewSession.PodcastEpisode.AudioTranscript,
                        IsReleased = reviewSession.PodcastEpisode.IsReleased,
                        ListenCount = reviewSession.PodcastEpisode.ListenCount,
                        PodcastEpisodeSubscriptionTypeId = reviewSession.PodcastEpisode.PodcastEpisodeSubscriptionTypeId,
                        SeasonNumber = reviewSession.PodcastEpisode.SeasonNumber,
                        TotalSave = reviewSession.PodcastEpisode.TotalSave,
                        CreatedAt = reviewSession.PodcastEpisode.CreatedAt,
                        UpdatedAt = reviewSession.PodcastEpisode.UpdatedAt,
                        DeletedAt = reviewSession.PodcastEpisode.DeletedAt,
                    },
                    PodcastShow = new PodcastShowSnippetResponseDTO
                    {
                        Id = reviewSession.PodcastEpisode.PodcastShow.Id,
                        Name = reviewSession.PodcastEpisode.PodcastShow.Name,
                        Description = reviewSession.PodcastEpisode.PodcastShow.Description,
                        MainImageFileKey = reviewSession.PodcastEpisode.PodcastShow.MainImageFileKey,
                        IsReleased = reviewSession.PodcastEpisode.PodcastShow.IsReleased,
                        ReleaseDate = reviewSession.PodcastEpisode.PodcastShow.ReleaseDate
                    },
                    PodcastChannel = reviewSession.PodcastEpisode.PodcastShow.PodcastChannel != null ? new PodcastChannelSnippetResponseDTO
                    {
                        Id = reviewSession.PodcastEpisode.PodcastShow.PodcastChannel.Id,
                        Name = reviewSession.PodcastEpisode.PodcastShow.PodcastChannel.Name,
                        Description = reviewSession.PodcastEpisode.PodcastShow.PodcastChannel.Description,
                        MainImageFileKey = reviewSession.PodcastEpisode.PodcastShow.PodcastChannel.MainImageFileKey,
                    } : null,
                    Podcaster = new AccountStatusSnippetResponseDTO
                    {
                        Id = podcaster.Id,
                        Email = podcaster.Email,
                        RoleId = podcaster.RoleId,
                        IsVerified = podcaster.IsVerified,
                        FullName = podcaster.FullName,
                        MainImageFileKey = podcaster.MainImageFileKey,
                        PodcasterProfileIsVerified = podcaster.PodcasterProfileIsVerified,
                        PodcasterProfileName = podcaster.PodcasterProfileName,
                        ViolationLevel = podcaster.ViolationLevel,
                        ViolationPoint = podcaster.ViolationPoint,
                        HasVerifiedPodcasterProfile = podcaster.HasVerifiedPodcasterProfile,
                    },
                    EpisodeCurrentStatus = episodeCurrentStatus,
                    ShowCurrentStatus = showCurrentStatus,
                    ChannelCurrentStatus = channelCurrentStatus,
                    PodcastIllegalContentTypeList = reviewSession.PodcastEpisode.PodcastEpisodeIllegalContentTypeMarkings
                        .Select(pictm => new PodcastIllegalContentTypeDTO
                        {
                            Id = pictm.PodcastIllegalContentType.Id,
                            Name = pictm.PodcastIllegalContentType.Name,
                        })
                        .ToList(),
                    PublishDuplicateDetectedPodcastEpisodes = reviewSession.PodcastEpisodePublishDuplicateDetections
                        .Select(ped => new PodcastEpisodeDTO
                        {
                            Id = ped.DuplicatePodcastEpisode.Id,
                            Name = ped.DuplicatePodcastEpisode.Name,
                            MainImageFileKey = ped.DuplicatePodcastEpisode.MainImageFileKey,
                            AudioFileKey = ped.DuplicatePodcastEpisode.AudioFileKey,
                            AudioFileSize = ped.DuplicatePodcastEpisode.AudioFileSize,
                            AudioFingerPrint = ped.DuplicatePodcastEpisode.AudioFingerPrint,
                            AudioLength = ped.DuplicatePodcastEpisode.AudioLength,
                            EpisodeOrder = ped.DuplicatePodcastEpisode.EpisodeOrder,
                            ReleaseDate = ped.DuplicatePodcastEpisode.ReleaseDate,
                            ExplicitContent = ped.DuplicatePodcastEpisode.ExplicitContent,
                            IsAudioPublishable = ped.DuplicatePodcastEpisode.IsAudioPublishable,
                            TakenDownReason = ped.DuplicatePodcastEpisode.TakenDownReason,
                            PodcastShowId = ped.DuplicatePodcastEpisode.PodcastShowId,
                            AudioTranscript = ped.DuplicatePodcastEpisode.AudioTranscript,
                            IsReleased = ped.DuplicatePodcastEpisode.IsReleased,
                            ListenCount = ped.DuplicatePodcastEpisode.ListenCount,
                            PodcastEpisodeSubscriptionTypeId = ped.DuplicatePodcastEpisode.PodcastEpisodeSubscriptionTypeId,
                            SeasonNumber = ped.DuplicatePodcastEpisode.SeasonNumber,
                            TotalSave = ped.DuplicatePodcastEpisode.TotalSave,
                            Description = ped.DuplicatePodcastEpisode.Description,
                            CreatedAt = ped.DuplicatePodcastEpisode.CreatedAt,
                            DeletedAt = ped.DuplicatePodcastEpisode.DeletedAt,
                            UpdatedAt = ped.DuplicatePodcastEpisode.UpdatedAt,
                        })
                        .ToList(),
                    RestrictedTermFoundList = detectedRestrictTerms,
                    CurrentStatus = new PodcastEpisodePublishReviewSessionStatusDTO
                    {
                        Id = sessionCurrentStatus.Id,
                        Name = sessionCurrentStatus.Name
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get episode publish review session by id failed, error: " + ex.Message);
            }
        }


        public async Task RequirePodcastEpisodePublishReviewSessionEdit(RequireEpisodePublishReviewSessionEditParameterDTO requireEpisodePublishReviewSessionEditParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Yêu cầu chỉnh sửa phiên kiểm duyệt publish episode
                    var existingPodcastEpisodePublishReviewSession = await _podcastEpisodePublishReviewSessionGenericRepository.FindByIdAsync(
                        requireEpisodePublishReviewSessionEditParameterDTO.PodcastEpisodePublishReviewSessionId,
                        includeFunc: q => q
                            .Include(pers => pers.PodcastEpisode)
                        .ThenInclude(pe => pe.PodcastEpisodeStatusTrackings)
                        .ThenInclude(pe => pe.PodcastEpisodeStatus)
                        .Include(pers => pers.PodcastEpisode)
                        .ThenInclude(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .ThenInclude(pst => pst.PodcastShowStatus)
                        .Include(pers => pers.PodcastEpisode)
                        .ThenInclude(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastChannel)
                        .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                        .ThenInclude(pct => pct.PodcastChannelStatus)
                        .Include(pers => pers.PodcastEpisode)
                        .ThenInclude(pe => pe.PodcastEpisodeIllegalContentTypeMarkings)
                        .ThenInclude(pictm => pictm.PodcastIllegalContentType)
                        .Include(pers => pers.PodcastEpisodePublishDuplicateDetections)
                        .ThenInclude(ped => ped.DuplicatePodcastEpisode)
                        .ThenInclude(dpe => dpe.PodcastEpisodeStatusTrackings)
                        .Include(pers => pers.PodcastEpisodePublishReviewSessionStatusTrackings)
                        .ThenInclude(pes => pes.PodcastEpisodePublishReviewSessionStatus)
                    );

                    if (existingPodcastEpisodePublishReviewSession == null)
                    {
                        throw new Exception("Podcast episode publish review session with id " + requireEpisodePublishReviewSessionEditParameterDTO.PodcastEpisodePublishReviewSessionId + " does not exist");
                    }
                    else if (existingPodcastEpisodePublishReviewSession.AssignedStaff != requireEpisodePublishReviewSessionEditParameterDTO.StaffId)
                    {
                        throw new Exception("Podcast episode publish review session with id " + requireEpisodePublishReviewSessionEditParameterDTO.PodcastEpisodePublishReviewSessionId + " is not assigned to staff with id " + requireEpisodePublishReviewSessionEditParameterDTO.StaffId);
                    }
                    else if (existingPodcastEpisodePublishReviewSession.PodcastEpisodePublishReviewSessionStatusTrackings.OrderByDescending(s => s.CreatedAt).FirstOrDefault().PodcastEpisodePublishReviewSessionStatusId != (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview)
                    {
                        throw new Exception("Podcast episode publish review session with id " + requireEpisodePublishReviewSessionEditParameterDTO.PodcastEpisodePublishReviewSessionId + " is not in Pending Review status, cannot request edit");
                    }

                    var episode = existingPodcastEpisodePublishReviewSession.PodcastEpisode;
                    var show = episode.PodcastShow;
                    var channel = show.PodcastChannel;

                    var podcastEpisodeCurrentStatus = existingPodcastEpisodePublishReviewSession.PodcastEpisode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(s => s.CreatedAt).Select(s => new PodcastEpisodeStatusDTO
                            {
                                Id = s.PodcastEpisodeStatusId,
                                Name = s.PodcastEpisodeStatus.Name
                            })
                            .FirstOrDefault();
                    var podcastShowCurrentStatus = existingPodcastEpisodePublishReviewSession.PodcastEpisode.PodcastShow.PodcastShowStatusTrackings
                        .OrderByDescending(s => s.CreatedAt).Select(s => new PodcastShowStatusDTO
                        {
                            Id = s.PodcastShowStatusId,
                            Name = s.PodcastShowStatus.Name
                        })
                        .FirstOrDefault();
                    var podcastChannelCurrentStatus = existingPodcastEpisodePublishReviewSession.PodcastEpisode.PodcastShow.PodcastChannel != null ? existingPodcastEpisodePublishReviewSession.PodcastEpisode.PodcastShow.PodcastChannel.PodcastChannelStatusTrackings
                        .OrderByDescending(s => s.CreatedAt).Select(s => new PodcastChannelStatusDTO
                        {
                            Id = s.PodcastChannelStatusId,
                            Name = s.PodcastChannelStatus.Name
                        })
                        .FirstOrDefault() : null;

                    if (channel != null && channel.DeletedAt != null)
                    {
                        throw new Exception("Podcast channel with id " + channel.Id + " has been deleted");
                    }
                    else if (show.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + show.Id + " has been deleted");
                    }
                    else if (episode.DeletedAt != null)
                    {
                        throw new Exception("Podcast episode with id " + episode.Id + " has been deleted");
                    }
                    else if (podcastShowCurrentStatus.Id == (int)PodcastShowStatusEnum.Removed)
                    {
                        throw new Exception("Podcast show with id " + show.Id + " is in Removed status");
                    }
                    else if (podcastEpisodeCurrentStatus.Id == (int)PodcastEpisodeStatusEnum.Removed)
                    {
                        throw new Exception("Podcast episode with id " + episode.Id + " is in Removed status");
                    }

                    var activeSystemConfigProfile = await GetActiveSystemConfigProfile();
                    // var newDeadline = _dateHelper.GetNowByAppTimeZone().AddHours(activeSystemConfigProfile["ReviewSessionConfig"]["PodcastEpisodePublishEditRequirementExpiredHours"].Value<int>());
                    var newDeadline = _dateHelper.GetNowByAppTimeZone().AddHours(activeSystemConfigProfile.ReviewSessionConfig.PodcastEpisodePublishEditRequirementExpiredHours);   

                    // cập nhật lại ghi chú và deadline của phiên kiểm duyệt publish episode
                    existingPodcastEpisodePublishReviewSession.Note = requireEpisodePublishReviewSessionEditParameterDTO.Note;
                    existingPodcastEpisodePublishReviewSession.Deadline = newDeadline;
                    await _podcastEpisodePublishReviewSessionGenericRepository.UpdateAsync(existingPodcastEpisodePublishReviewSession.Id, existingPodcastEpisodePublishReviewSession);


                    // xoá và thêm lại các đánh dấu loại nội dung vi phạm    
                    await _unitOfWork.PodcastEpisodeIllegalContentTypeMarkingRepository.DeleteByPodcastEpisodeIdAsync(episode.Id);

                    foreach (var illegalContentTypeId in requireEpisodePublishReviewSessionEditParameterDTO.PodcastIllegalContentTypeIds)
                    {
                        var existingPodcastIllegalContentType = await _podcastIllegalContentTypeGenericRepository.FindByIdAsync(illegalContentTypeId);
                        if (existingPodcastIllegalContentType == null)
                        {
                            throw new Exception("Podcast illegal content type with id " + illegalContentTypeId + " does not exist");
                        }
                        var newPodcastEpisodeIllegalContentTypeMarking = new PodcastEpisodeIllegalContentTypeMarking
                        {
                            PodcastEpisodeId = episode.Id,
                            PodcastIllegalContentTypeId = illegalContentTypeId,
                            MarkerId = requireEpisodePublishReviewSessionEditParameterDTO.StaffId
                        };
                        await _podcastEpisodeIllegalContentTypeMarkingGenericRepository.CreateAsync(newPodcastEpisodeIllegalContentTypeMarking);
                    }

                    // chuyển trạng thái phiên kiểm duyệt publish episode về Pending Edit Required
                    var episodeStatusTracking = new PodcastEpisodeStatusTracking
                    {
                        PodcastEpisodeId = episode.Id,
                        PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.PendingEditRequired,
                    };
                    await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(episodeStatusTracking);



                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodePublishReviewSessionId"] = requireEpisodePublishReviewSessionEditParameterDTO.PodcastEpisodePublishReviewSessionId;
                    messageNextRequestData["Note"] = requireEpisodePublishReviewSessionEditParameterDTO.Note;
                    messageNextRequestData["PodcastIllegalContentTypeIds"] = JArray.FromObject(requireEpisodePublishReviewSessionEditParameterDTO.PodcastIllegalContentTypeIds);
                    messageNextRequestData["StaffId"] = requireEpisodePublishReviewSessionEditParameterDTO.StaffId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodePublishReviewSessionId = requireEpisodePublishReviewSessionEditParameterDTO.PodcastEpisodePublishReviewSessionId,
                        Note = requireEpisodePublishReviewSessionEditParameterDTO.Note,
                        PodcastIllegalContentTypeIds = requireEpisodePublishReviewSessionEditParameterDTO.PodcastIllegalContentTypeIds,
                        StaffId = requireEpisodePublishReviewSessionEditParameterDTO.StaffId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "require-episode-publish-review-session-edit.success"
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
                            ErrorMessage = $"Require podcast episode publish review session edit failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "require-episode-publish-review-session-edit.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task AcceptPodcastEpisodePublishReviewSession(AcceptEpisodePublishReviewSessionParameterDTO acceptEpisodePublishReviewSessionParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingPodcastEpisodePublishReviewSession = await _podcastEpisodePublishReviewSessionGenericRepository.FindByIdAsync(
                        acceptEpisodePublishReviewSessionParameterDTO.PodcastEpisodePublishReviewSessionId,
                        includeFunc: q => q
                        .Include(pers => pers.PodcastEpisodePublishReviewSessionStatusTrackings)
                        .ThenInclude(pes => pes.PodcastEpisodePublishReviewSessionStatus)
                    );
                    if (existingPodcastEpisodePublishReviewSession == null)
                    {
                        throw new Exception("Podcast episode publish review session with id " + acceptEpisodePublishReviewSessionParameterDTO.PodcastEpisodePublishReviewSessionId + " does not exist");
                    }
                    else if (existingPodcastEpisodePublishReviewSession.AssignedStaff != acceptEpisodePublishReviewSessionParameterDTO.StaffId)
                    {
                        throw new Exception("Podcast episode publish review session with id " + acceptEpisodePublishReviewSessionParameterDTO.PodcastEpisodePublishReviewSessionId + " is not assigned to staff with id " + acceptEpisodePublishReviewSessionParameterDTO.StaffId);
                    }
                    else if (existingPodcastEpisodePublishReviewSession.PodcastEpisodePublishReviewSessionStatusTrackings.OrderByDescending(s => s.CreatedAt).FirstOrDefault().PodcastEpisodePublishReviewSessionStatusId != (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview)
                    {
                        throw new Exception("Podcast episode publish review session with id " + acceptEpisodePublishReviewSessionParameterDTO.PodcastEpisodePublishReviewSessionId + " is not in Pending Review status, cannot accept");
                    }

                    var existingEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(existingPodcastEpisodePublishReviewSession.PodcastEpisodeId,
                        includeFunc: q => q
                            .Include(pe => pe.PodcastEpisodeStatusTrackings)

                            .Include(pe => pe.PodcastShow)
                            .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                            .Include(pe => pe.PodcastShow)
                            .ThenInclude(ps => ps.PodcastChannel)
                            .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                    );

                    if (existingEpisode == null)
                    {
                        throw new Exception("Podcast episode with id " + existingPodcastEpisodePublishReviewSession.PodcastEpisodeId + " does not exist");
                    }
                    else if (existingEpisode.DeletedAt != null)
                    {
                        throw new Exception("Podcast episode with id " + existingEpisode.Id + " has been deleted");
                    }
                    else if (existingEpisode.PodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + existingEpisode.PodcastShow.Id + " has been deleted");
                    }
                    else if (existingEpisode.PodcastShow.PodcastChannel != null && existingEpisode.PodcastShow.PodcastChannel.DeletedAt != null)
                    {
                        throw new Exception("Podcast channel with id " + existingEpisode.PodcastShow.PodcastChannel.Id + " has been deleted");
                    }

                    var episodeCurrentStatusId = existingEpisode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(s => s.CreatedAt).Select(s =>s.PodcastEpisodeStatusId)
                            .FirstOrDefault();
                    var showCurrentStatusId = existingEpisode.PodcastShow.PodcastShowStatusTrackings
                            .OrderByDescending(s => s.CreatedAt).Select(s => s.PodcastShowStatusId)
                            .FirstOrDefault();

                    if (episodeCurrentStatusId != (int)PodcastEpisodeStatusEnum.PendingReview)
                    {
                        throw new Exception("Podcast episode with id " + existingEpisode.Id + " is not in Pending Review status, cannot accept publish review session");
                    }
                    else if (showCurrentStatusId == (int)PodcastShowStatusEnum.Removed)
                    {
                        throw new Exception("Podcast show with id " + existingEpisode.PodcastShow.Id + " is in Removed status");
                    }

                    // cập nhật lại ghi chú và xoá deadline của phiên kiểm duyệt publish episode
                    existingPodcastEpisodePublishReviewSession.Note = acceptEpisodePublishReviewSessionParameterDTO.Note;
                    existingPodcastEpisodePublishReviewSession.Deadline = null;
                    await _podcastEpisodePublishReviewSessionGenericRepository.UpdateAsync(existingPodcastEpisodePublishReviewSession.Id, existingPodcastEpisodePublishReviewSession);


                    // xoá và thêm lại các đánh dấu loại nội dung vi phạm    
                    await _unitOfWork.PodcastEpisodeIllegalContentTypeMarkingRepository.DeleteByPodcastEpisodeIdAsync(existingEpisode.Id);

                    foreach (var illegalContentTypeId in acceptEpisodePublishReviewSessionParameterDTO.PodcastIllegalContentTypeIds)
                    {
                        var existingPodcastIllegalContentType = await _podcastIllegalContentTypeGenericRepository.FindByIdAsync(illegalContentTypeId);
                        if (existingPodcastIllegalContentType == null)
                        {
                            throw new Exception("Podcast illegal content type with id " + illegalContentTypeId + " does not exist");
                        }
                        var newPodcastEpisodeIllegalContentTypeMarking = new PodcastEpisodeIllegalContentTypeMarking
                        {
                            PodcastEpisodeId = existingEpisode.Id,
                            PodcastIllegalContentTypeId = illegalContentTypeId,
                            MarkerId = acceptEpisodePublishReviewSessionParameterDTO.StaffId
                        };
                        await _podcastEpisodeIllegalContentTypeMarkingGenericRepository.CreateAsync(newPodcastEpisodeIllegalContentTypeMarking);
                    }

                    // chuyển trạng thái phiên kiểm duyệt publish episode về Accepted
                    var episodePublishReviewSessionStatusTracking = new PodcastEpisodePublishReviewSessionStatusTracking
                    {
                        PodcastEpisodePublishReviewSessionId = existingPodcastEpisodePublishReviewSession.Id,
                        PodcastEpisodePublishReviewSessionStatusId = (int)PodcastEpisodePublishReviewSessionStatusEnum.Accepted,
                    };
                    await _podcastEpisodePublishReviewSessionStatusTrackingGenericRepository.CreateAsync(episodePublishReviewSessionStatusTracking);



                    // chuyển trạng thái episode về Ready to Release và set ispublishable = true
                    existingEpisode.IsAudioPublishable = true;
                    await _podcastEpisodeGenericRepository.UpdateAsync(existingEpisode.Id, existingEpisode);
                    var episodeStatusTracking = new PodcastEpisodeStatusTracking
                    {
                        PodcastEpisodeId = existingEpisode.Id,
                        PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.ReadyToRelease,
                    };
                    await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(episodeStatusTracking);



                    await transaction.CommitAsync();
                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodePublishReviewSessionId"] = acceptEpisodePublishReviewSessionParameterDTO.PodcastEpisodePublishReviewSessionId;
                    messageNextRequestData["Note"] = acceptEpisodePublishReviewSessionParameterDTO.Note;
                    messageNextRequestData["PodcastIllegalContentTypeIds"] = JArray.FromObject(acceptEpisodePublishReviewSessionParameterDTO.PodcastIllegalContentTypeIds);
                    messageNextRequestData["StaffId"] = acceptEpisodePublishReviewSessionParameterDTO.StaffId;

                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodePublishReviewSessionId = acceptEpisodePublishReviewSessionParameterDTO.PodcastEpisodePublishReviewSessionId,
                        Note = acceptEpisodePublishReviewSessionParameterDTO.Note,
                        PodcastIllegalContentTypeIds = acceptEpisodePublishReviewSessionParameterDTO.PodcastIllegalContentTypeIds,
                        StaffId = acceptEpisodePublishReviewSessionParameterDTO.StaffId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "accept-episode-publish-review-session.success"
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
                            ErrorMessage = $"Accept podcast episode publish review session failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "accept-episode-publish-review-session.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");

                }
            }
        }

        public async Task RejectPodcastEpisodePublishReviewSession(RejectEpisodePublishReviewSessionParameterDTO rejectEpisodePublishReviewSessionParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingPodcastEpisodePublishReviewSession = await _podcastEpisodePublishReviewSessionGenericRepository.FindByIdAsync(
                        rejectEpisodePublishReviewSessionParameterDTO.PodcastEpisodePublishReviewSessionId,
                        includeFunc: q => q
                        .Include(pers => pers.PodcastEpisodePublishReviewSessionStatusTrackings)
                        .ThenInclude(pes => pes.PodcastEpisodePublishReviewSessionStatus)
                    );
                    if (existingPodcastEpisodePublishReviewSession == null)
                    {
                        throw new Exception("Podcast episode publish review session with id " + rejectEpisodePublishReviewSessionParameterDTO.PodcastEpisodePublishReviewSessionId + " does not exist");
                    }
                    else if (existingPodcastEpisodePublishReviewSession.AssignedStaff != rejectEpisodePublishReviewSessionParameterDTO.StaffId)
                    {
                        throw new Exception("Podcast episode publish review session with id " + rejectEpisodePublishReviewSessionParameterDTO.PodcastEpisodePublishReviewSessionId + " is not assigned to staff with id " + rejectEpisodePublishReviewSessionParameterDTO.StaffId);
                    }
                    else if (existingPodcastEpisodePublishReviewSession.PodcastEpisodePublishReviewSessionStatusTrackings.OrderByDescending(s => s.CreatedAt).FirstOrDefault().PodcastEpisodePublishReviewSessionStatusId != (int)PodcastEpisodePublishReviewSessionStatusEnum.PendingReview)
                    {
                        throw new Exception("Podcast episode publish review session with id " + rejectEpisodePublishReviewSessionParameterDTO.PodcastEpisodePublishReviewSessionId + " is not in Pending Review status, cannot accept");
                    }

                    var existingEpisode = await _podcastEpisodeGenericRepository.FindByIdAsync(existingPodcastEpisodePublishReviewSession.PodcastEpisodeId,
                        includeFunc: q => q
                            .Include(pe => pe.PodcastEpisodeStatusTrackings)
                            .Include(pe => pe.PodcastShow)
                            .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                            .Include(pe => pe.PodcastShow)
                            .ThenInclude(ps => ps.PodcastChannel)
                            .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                    );

                    if (existingEpisode == null)
                    {
                        throw new Exception("Podcast episode with id " + existingPodcastEpisodePublishReviewSession.PodcastEpisodeId + " does not exist");
                    }
                    else if (existingEpisode.DeletedAt != null)
                    {
                        throw new Exception("Podcast episode with id " + existingEpisode.Id + " has been deleted");
                    }
                    else if (existingEpisode.PodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + existingEpisode.PodcastShow.Id + " has been deleted");
                    }
                    else if (existingEpisode.PodcastShow.PodcastChannel != null && existingEpisode.PodcastShow.PodcastChannel.DeletedAt != null)
                    {
                        throw new Exception("Podcast channel with id " + existingEpisode.PodcastShow.PodcastChannel.Id + " has been deleted");
                    }

                    var episodeCurrentStatusId = existingEpisode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(s => s.CreatedAt).Select(s =>s.PodcastEpisodeStatusId)
                            .FirstOrDefault();
                    var showCurrentStatusId = existingEpisode.PodcastShow.PodcastShowStatusTrackings
                            .OrderByDescending(s => s.CreatedAt).Select(s => s.PodcastShowStatusId)
                            .FirstOrDefault();

                    if (episodeCurrentStatusId != (int)PodcastEpisodeStatusEnum.PendingReview)
                    {
                        throw new Exception("Podcast episode with id " + existingEpisode.Id + " is not in Pending Review status, cannot reject publish review session");
                    }
                    else if (showCurrentStatusId == (int)PodcastShowStatusEnum.Removed)
                    {
                        throw new Exception("Podcast show with id " + existingEpisode.PodcastShow.Id + " is in Removed status");
                    }

                    // cập nhật lại ghi chú và xoá deadline của phiên kiểm duyệt publish episode
                    existingPodcastEpisodePublishReviewSession.Note = rejectEpisodePublishReviewSessionParameterDTO.Note;
                    existingPodcastEpisodePublishReviewSession.Deadline = null;
                    await _podcastEpisodePublishReviewSessionGenericRepository.UpdateAsync(existingPodcastEpisodePublishReviewSession.Id, existingPodcastEpisodePublishReviewSession);
                    // chuyển trạng thái phiên kiểm duyệt publish episode về Rejected
                    var episodePublishReviewSessionStatusTracking = new PodcastEpisodePublishReviewSessionStatusTracking
                    {
                        PodcastEpisodePublishReviewSessionId = existingPodcastEpisodePublishReviewSession.Id,
                        PodcastEpisodePublishReviewSessionStatusId = (int)PodcastEpisodePublishReviewSessionStatusEnum.Rejected,
                    };
                    await _podcastEpisodePublishReviewSessionStatusTrackingGenericRepository.CreateAsync(episodePublishReviewSessionStatusTracking);

                    // chuyển trạng thái episode về Ready to Release và set ispublishable = true
                    existingEpisode.IsAudioPublishable = false;
                    await _podcastEpisodeGenericRepository.UpdateAsync(existingEpisode.Id, existingEpisode);
                    var episodeStatusTracking = new PodcastEpisodeStatusTracking
                    {
                        PodcastEpisodeId = existingEpisode.Id,
                        PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Draft,
                    };
                    await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(episodeStatusTracking);

                    await transaction.CommitAsync();
                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastEpisodePublishReviewSessionId"] = rejectEpisodePublishReviewSessionParameterDTO.PodcastEpisodePublishReviewSessionId;
                    messageNextRequestData["Note"] = rejectEpisodePublishReviewSessionParameterDTO.Note;
                    messageNextRequestData["PodcastIllegalContentTypeIds"] = JArray.FromObject(rejectEpisodePublishReviewSessionParameterDTO.PodcastIllegalContentTypeIds);
                    messageNextRequestData["StaffId"] = rejectEpisodePublishReviewSessionParameterDTO.StaffId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastEpisodePublishReviewSessionId = rejectEpisodePublishReviewSessionParameterDTO.PodcastEpisodePublishReviewSessionId,
                        Note = rejectEpisodePublishReviewSessionParameterDTO.Note,
                        PodcastIllegalContentTypeIds = rejectEpisodePublishReviewSessionParameterDTO.PodcastIllegalContentTypeIds,
                        StaffId = rejectEpisodePublishReviewSessionParameterDTO.StaffId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentModerationDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "reject-episode-publish-review-session.success"
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
                            ErrorMessage = $"Reject podcast episode publish review session failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "reject-episode-publish-review-session.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }
    }
}