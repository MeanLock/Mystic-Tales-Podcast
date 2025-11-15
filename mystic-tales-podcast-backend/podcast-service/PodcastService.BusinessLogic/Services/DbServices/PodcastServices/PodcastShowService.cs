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
using PodcastService.BusinessLogic.DTOs.Channel;
using PodcastService.BusinessLogic.DTOs.Channel.Details;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Hashtag;
using PodcastService.BusinessLogic.DTOs.Subscription.ListItems;
using PodcastService.BusinessLogic.DTOs.Subscription;
using PodcastService.BusinessLogic.DTOs.Show.ListItems;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.Services.DbServices.MiscServices;
using PodcastService.BusinessLogic.DTOs.Show;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateShow;
using PodcastService.BusinessLogic.DTOs.Show.Details;
using PodcastService.BusinessLogic.DTOs.Episode.ListItems;
using PodcastService.BusinessLogic.DTOs.Episode;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateShow;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubmitShowTrailerAudioFile;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PublishShow;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PlusShowTotalFollow;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubtractShowTotalFollow;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.CreateShowReview;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.UpdateShowReview;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeleteShowReview;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeleteShowReviewDMCARemoveShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveShowDmcaDmcaRemoveShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedShowDmcaUnpublishShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeleteShowReviewUnpublishShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UnpublishShowUnpublishShowForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeleteShowReviewShowDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedShowDmcaShowDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteShowShowShowDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeleteChannelShowsReviewUnpublishChannelForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeleteChannelShowsReviewChannelDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveChannelDismissedShowsDmcaUnpublishChannelForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveChannelDismissedShowsDmcaChannelDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteChannelShowsChannelDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UnpublishPodcasterShowsTerminatePodcasterForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.PublicReviewManagementDomain.DeletePodcasterShowsReviewTerminatePodcasterForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedShowDmcaTerminatePodcasterForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.TakedownContentDmca;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RestoreContentDmca;
using PodcastService.BusinessLogic.Services.DbServices.CachingServices;
using PodcastService.BusinessLogic.DTOs.SystemConfiguration;

namespace PodcastService.BusinessLogic.Services.DbServices.PodcastServices
{
    public class PodcastShowService
    {
        // LOGGER
        private readonly ILogger<PodcastShowService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly IAccountConfig _accountConfig;
        private readonly IGoogleMailConfig _googleMailConfig;

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

        public PodcastShowService(
            ILogger<PodcastShowService> logger,
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

            FileIOHelper fileIOHelper,
            DateHelper dateHelper,

            IFilePathConfig filePathConfig,
            IGoogleMailConfig googleMailConfig,
            IAppConfig appConfig,
            IAccountConfig accountConfig,

            HttpServiceQueryClient httpServiceQueryClient,

            AccountCachingService accountCachingService,

            IMessagingService messagingService,
            KafkaProducerService kafkaProducerService,

            RedisSharedCacheService redisSharedCacheService
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

            _fileIOHelper = fileIOHelper;
            _jwtHelper = jwtHelper;
            _bcryptHelper = bcryptHelper;
            _dateHelper = dateHelper;

            _fluentEmailService = fluentEmailService;

            _filePathConfig = filePathConfig;
            _accountConfig = accountConfig;
            _googleMailConfig = googleMailConfig;
            _appConfig = appConfig;

            _httpServiceQueryClient = httpServiceQueryClient;

            _accountCachingService = accountCachingService;

            _messagingService = messagingService;
            _kafkaProducerService = kafkaProducerService;

            _redisSharedCacheService = redisSharedCacheService;
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

        /////////////////////////////////////////////////////////////
        public async Task<List<ShowListItemResponseDTO>> GetShows(int? roleId)
        {
            try
            {
                var showsQuery = _podcastShowGenericRepository.FindAll(
                    predicate: ps => ps.DeletedAt == null,
                    includeFunc: q => q
                        .Include(ps => ps.PodcastShowStatusTrackings)
                        .ThenInclude(pst => pst.PodcastShowStatus)
                        .Include(ps => ps.PodcastCategory)
                        .Include(ps => ps.PodcastSubCategory)
                        .Include(ps => ps.PodcastShowHashtags)
                        .ThenInclude(psh => psh.Hashtag)
                        .Include(ps => ps.PodcastShowSubscriptionType)
                        .Include(ps => ps.PodcastChannel)
                        .Include(ps => ps.PodcastEpisodes)
                        .ThenInclude(pe => pe.PodcastEpisodeStatusTrackings)
                );

                if (roleId == null || roleId == 1)
                {
                    showsQuery = showsQuery.Where(ps => ps.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnum.Published && ps.IsReleased != null);
                }
                var showList = await showsQuery.ToListAsync();

                var shows = (await Task.WhenAll(showList.Select(async ps =>
                {
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(ps.PodcasterId);
                    if (podcaster == null || podcaster.Id != ps.PodcasterId || podcaster.IsVerified == false || podcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + ps.PodcasterId + " does not exist");
                    }
                    return new ShowListItemResponseDTO
                    {
                        Id = ps.Id,
                        Name = ps.Name,
                        Description = ps.Description,
                        MainImageFileKey = ps.MainImageFileKey,
                        TrailerAudioFileKey = ps.TrailerAudioFileKey,
                        TotalFollow = ps.TotalFollow,
                        ListenCount = ps.ListenCount,
                        AverageRating = ps.AverageRating,
                        RatingCount = ps.RatingCount,
                        Copyright = ps.Copyright,
                        IsReleased = ps.IsReleased,
                        Language = ps.Language,
                        UploadFrequency = ps.UploadFrequency,
                        ReleaseDate = ps.ReleaseDate,
                        TakenDownReason = roleId == null || roleId == 1 ? null : ps.TakenDownReason,
                        EpisodeCount = ps.PodcastEpisodes.Count(pe =>
                        {
                            if (roleId == null || roleId == 1)
                            {
                                return pe.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published && pe.DeletedAt == null;
                            }
                            else
                            {
                                return pe.DeletedAt == null;
                            }
                        }),
                        PodcastCategory = ps.PodcastCategory != null ? new PodcastCategoryDTO
                        {
                            Id = ps.PodcastCategory.Id,
                            Name = ps.PodcastCategory.Name,
                            MainImageFileKey = ps.PodcastCategory.MainImageFileKey
                        } : null,
                        PodcastSubCategory = ps.PodcastSubCategory != null ? new PodcastSubCategoryDTO
                        {
                            Id = ps.PodcastSubCategory.Id,
                            Name = ps.PodcastSubCategory.Name,
                            PodcastCategoryId = ps.PodcastSubCategory.PodcastCategoryId
                        } : null,
                        PodcastChannel = ps.PodcastChannel != null ? new PodcastChannelSnippetResponseDTO
                        {
                            Id = ps.PodcastChannel.Id,
                            Name = ps.PodcastChannel.Name,
                            Description = ps.PodcastChannel.Description,
                            MainImageFileKey = ps.PodcastChannel.MainImageFileKey
                        } : null,
                        Podcaster = new AccountSnippetResponseDTO
                        {
                            Id = podcaster.Id,
                            Email = podcaster.Email,
                            FullName = podcaster.PodcasterProfileName,
                            MainImageFileKey = podcaster.MainImageFileKey
                        },
                        PodcastShowSubscriptionType = ps.PodcastShowSubscriptionType != null ? new PodcastShowSubscriptionTypeDTO
                        {
                            Id = ps.PodcastShowSubscriptionType.Id,
                            Name = ps.PodcastShowSubscriptionType.Name
                        } : null,
                        Hashtags = ps.PodcastShowHashtags.Select(psh => new HashtagDTO
                        {
                            Id = psh.Hashtag.Id,
                            Name = psh.Hashtag.Name
                        }).ToList(),
                        CreatedAt = ps.CreatedAt,
                        UpdatedAt = ps.UpdatedAt,
                        CurrentStatus = ps.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).Select(pst => new PodcastShowStatusDTO
                        {
                            Id = pst.PodcastShowStatus.Id,
                            Name = pst.PodcastShowStatus.Name
                        }).FirstOrDefault()!,
                    };
                }))).ToList();

                return shows;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get shows failed, error: " + ex.Message);
            }
        }

        public async Task<List<ShowListItemResponseDTO>> GetShowsByPodcasterIdAsync(int podcasterId)
        {
            try
            {
                var query = _podcastShowGenericRepository.FindAll(
                    predicate: c => c.DeletedAt == null && c.PodcasterId == podcasterId,
                    includeFunc: q => q
                        .Include(pc => pc.PodcastCategory)
                        .Include(pc => pc.PodcastSubCategory)
                        .Include(pc => pc.PodcastShowStatusTrackings)
                        .ThenInclude(pct => pct.PodcastShowStatus)
                        .Include(pc => pc.PodcastShowHashtags)
                        .ThenInclude(pch => pch.Hashtag)
                        .Include(pc => pc.PodcastChannel)
                        .Include(pc => pc.PodcastShowSubscriptionType)
                        .Include(pc => pc.PodcastEpisodes)
                        .ThenInclude(pe => pe.PodcastEpisodeStatusTrackings)
                );

                var shows = await query.ToListAsync();

                var showList = shows.Select(ps => new ShowListItemResponseDTO
                {
                    Id = ps.Id,
                    Name = ps.Name,
                    Description = ps.Description,
                    MainImageFileKey = ps.MainImageFileKey,
                    TrailerAudioFileKey = ps.TrailerAudioFileKey,
                    TotalFollow = ps.TotalFollow,
                    ListenCount = ps.ListenCount,
                    AverageRating = ps.AverageRating,
                    RatingCount = ps.RatingCount,
                    Copyright = ps.Copyright,
                    IsReleased = ps.IsReleased,
                    Language = ps.Language,
                    EpisodeCount = ps.PodcastEpisodes.Count(pe => pe.DeletedAt == null),
                    PodcastCategory = ps.PodcastCategory != null ? new PodcastCategoryDTO
                    {
                        Id = ps.PodcastCategory.Id,
                        Name = ps.PodcastCategory.Name,
                        MainImageFileKey = ps.PodcastCategory.MainImageFileKey
                    } : null,
                    PodcastSubCategory = ps.PodcastSubCategory != null ? new PodcastSubCategoryDTO
                    {
                        Id = ps.PodcastSubCategory.Id,
                        Name = ps.PodcastSubCategory.Name,
                        PodcastCategoryId = ps.PodcastSubCategory.PodcastCategoryId
                    } : null,
                    PodcastChannel = ps.PodcastChannel != null ? new PodcastChannelSnippetResponseDTO
                    {
                        Id = ps.PodcastChannel.Id,
                        Name = ps.PodcastChannel.Name,
                        Description = ps.PodcastChannel.Description,
                        MainImageFileKey = ps.PodcastChannel.MainImageFileKey
                    } : null,
                    PodcastShowSubscriptionType = ps.PodcastShowSubscriptionType != null ? new PodcastShowSubscriptionTypeDTO
                    {
                        Id = ps.PodcastShowSubscriptionType.Id,
                        Name = ps.PodcastShowSubscriptionType.Name
                    } : null,
                    Podcaster = null,
                    ReleaseDate = ps.ReleaseDate,
                    TakenDownReason = ps.TakenDownReason,
                    UploadFrequency = ps.UploadFrequency,
                    Hashtags = ps.PodcastShowHashtags.Select(psh => new HashtagDTO
                    {
                        Id = psh.Hashtag.Id,
                        Name = psh.Hashtag.Name
                    }).ToList(),
                    CreatedAt = ps.CreatedAt,
                    UpdatedAt = ps.UpdatedAt,
                    CurrentStatus = ps.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).Select(pst => new PodcastShowStatusDTO
                    {
                        Id = pst.PodcastShowStatus.Id,
                        Name = pst.PodcastShowStatus.Name
                    }).FirstOrDefault()!,
                }).ToList();
                return showList;

            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get shows failed, error: " + ex.Message);
            }
        }

        public async Task<ShowDetailResponseDTO> GetShowByIdAsync(Guid showId, int? role)
        {
            try
            {
                var query = _podcastShowGenericRepository.FindAll(
                    predicate: c => c.DeletedAt == null && c.Id == showId,
                    includeFunc: q => q
                        .Include(pc => pc.PodcastCategory)
                        .Include(pc => pc.PodcastSubCategory)
                        .Include(pc => pc.PodcastShowStatusTrackings)
                        .ThenInclude(pct => pct.PodcastShowStatus)
                        .Include(pc => pc.PodcastShowHashtags)
                        .ThenInclude(pch => pch.Hashtag)
                        .Include(pc => pc.PodcastChannel)
                        .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                        .Include(pc => pc.PodcastShowSubscriptionType)
                );

                if (role == null || role == 1)
                {
                    query = query.Where(pc => pc.PodcastShowStatusTrackings.OrderByDescending(pct => pct.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnum.Published && pc.IsReleased != null); // đã đăng và có ngày phát hành (có thể là đã phát hành hoặc sắp phát hành)
                }
                var show = await query.FirstOrDefaultAsync();

                if (show == null)
                {
                    throw new Exception("Show with id " + showId + " does not exist");
                }
                // kiểm tra có thuộc về 1 channel đang được publish hay không, Customer không thấy được những show thuộc kênh chưa được publish hoặc đã bị xóa
                if (show.PodcastChannel != null && (show.PodcastChannel.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).FirstOrDefault().PodcastChannelStatusId != (int)PodcastChannelStatusEnum.Published || show.PodcastChannel.DeletedAt != null))
                {
                    if (role == null || role == 1)
                    {
                        throw new Exception("Show with id " + showId + " does not exist");
                    }
                }

                var podcaster = await _accountCachingService.GetAccountStatusCacheById(show.PodcasterId);
                if (podcaster == null || podcaster.Id != show.PodcasterId || podcaster.IsVerified == false || podcaster.HasVerifiedPodcasterProfile == false)
                {
                    throw new Exception("Podcaster with id " + show.PodcasterId + " does not exist");
                }

                var podcastSubscriptionBatchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastSubscriptionList",
                            QueryType = "findall",
                            EntityType = "PodcastSubscription",
                            Parameters = JObject.FromObject(new
                            {
                                where = (role == null || role == 1) ? new
                                {
                                    IsActive = (bool?)true,
                                    DeletedAt = (DateTime?)null,
                                    PodcastShowId = show.Id,
                                } : new {
                                    IsActive = (bool?)null,
                                    DeletedAt = (DateTime?)null,
                                    PodcastShowId = show.Id,
                                },
                                include = "PodcastSubscriptionBenefitMappings.PodcastSubscriptionBenefit , PodcastSubscriptionCycleTypePrices.SubscriptionCycleType"
                            }),
                        }
                    }
                };

                var result = await _httpServiceQueryClient.ExecuteBatchAsync("SubscriptionService", podcastSubscriptionBatchRequest);


                var episodeByShowIdQuery = _podcastEpisodeGenericRepository.FindAll(
                    predicate: pe => pe.DeletedAt == null && pe.PodcastShowId == show.Id,
                    includeFunc: q => q
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                        .ThenInclude(pet => pet.PodcastEpisodeStatus)
                        .Include(pe => pe.PodcastEpisodeHashtags)
                        .ThenInclude(peh => peh.Hashtag)
                        .Include(pe => pe.PodcastEpisodeSubscriptionType)
                );

                if (role == null || role == 1)
                {
                    episodeByShowIdQuery = episodeByShowIdQuery.Where(pe => pe.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published && pe.IsReleased != null);
                }

                var episodeList = await episodeByShowIdQuery.ToListAsync();


                var showDetail = new ShowDetailResponseDTO
                {
                    Id = show.Id,
                    Name = show.Name,
                    Description = show.Description,
                    MainImageFileKey = show.MainImageFileKey,
                    TrailerAudioFileKey = show.TrailerAudioFileKey,
                    TotalFollow = show.TotalFollow,
                    ListenCount = show.ListenCount,
                    AverageRating = show.AverageRating,
                    RatingCount = show.RatingCount,
                    EpisodeCount = episodeList.Count,
                    CurrentStatus = show.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).Select(pst => new PodcastShowStatusDTO
                    {
                        Id = pst.PodcastShowStatus.Id,
                        Name = pst.PodcastShowStatus.Name
                    }).FirstOrDefault()!,
                    Copyright = show.Copyright,
                    IsReleased = show.IsReleased,
                    Language = show.Language,
                    UploadFrequency = show.UploadFrequency,
                    ReleaseDate = show.ReleaseDate,
                    TakenDownReason = role == null || role == 1 ? null : show.TakenDownReason,
                    PodcastCategory = show.PodcastCategory != null ? new PodcastCategoryDTO
                    {
                        Id = show.PodcastCategory.Id,
                        Name = show.PodcastCategory.Name,
                        MainImageFileKey = show.PodcastCategory.MainImageFileKey
                    } : null,
                    PodcastSubCategory = show.PodcastSubCategory != null ? new PodcastSubCategoryDTO
                    {
                        Id = show.PodcastSubCategory.Id,
                        Name = show.PodcastSubCategory.Name,
                        PodcastCategoryId = show.PodcastSubCategory.PodcastCategoryId
                    } : null,
                    PodcastChannel = show.PodcastChannel != null ? new PodcastChannelSnippetResponseDTO
                    {
                        Id = show.PodcastChannel.Id,
                        Name = show.PodcastChannel.Name,
                        Description = show.PodcastChannel.Description,
                        MainImageFileKey = show.PodcastChannel.MainImageFileKey
                    } : null,
                    PodcastShowSubscriptionType = show.PodcastShowSubscriptionType != null ? new PodcastShowSubscriptionTypeDTO
                    {
                        Id = show.PodcastShowSubscriptionType.Id,
                        Name = show.PodcastShowSubscriptionType.Name
                    } : null,
                    Podcaster = new AccountSnippetResponseDTO
                    {
                        Id = podcaster.Id,
                        Email = podcaster.Email,
                        FullName = podcaster.PodcasterProfileName,
                        MainImageFileKey = podcaster.MainImageFileKey
                    },
                    Hashtags = show.PodcastShowHashtags.Select(psh => new HashtagDTO
                    {
                        Id = psh.Hashtag.Id,
                        Name = psh.Hashtag.Name
                    }).ToList(),
                    PodcastSubscriptionList = ((JArray)result.Results["podcastSubscriptionList"]).Select(ps =>
                    {
                        var psObj = ps.ToObject<PodcastSubscriptionListItemResponseDTO>();
                        return new PodcastSubscriptionListItemResponseDTO
                        {
                            Id = psObj.Id,
                            Name = psObj.Name,
                            Description = psObj.Description,
                            CurrentVersion = psObj.CurrentVersion,
                            PodcastShowId = psObj.PodcastShowId,
                            IsActive = psObj.IsActive,
                            CreatedAt = psObj.CreatedAt,
                            UpdatedAt = psObj.UpdatedAt,
                            PodcastChannelId = psObj.PodcastChannelId,
                            PodcastSubscriptionCycleTypePriceList = ((JArray)ps["PodcastSubscriptionCycleTypePrices"]).ToObject<List<PodcastSubscriptionCycleTypePriceListItemResponseDTO>>().Select(psctp => new PodcastSubscriptionCycleTypePriceListItemResponseDTO
                            {
                                PodcastSubscriptionId = psctp.PodcastSubscriptionId,
                                Price = psctp.Price,
                                Version = psctp.Version,
                                CreatedAt = psctp.CreatedAt,
                                UpdatedAt = psctp.UpdatedAt,
                                SubscriptionCycleType = psctp.SubscriptionCycleType != null ? new SubscriptionCycleTypeDTO
                                {
                                    Id = psctp.SubscriptionCycleType.Id,
                                    Name = psctp.SubscriptionCycleType.Name,
                                } : null
                            }).ToList(),
                            DeletedAt = psObj.DeletedAt,
                            PodcastSubscriptionBenefitMappingList = ((JArray)ps["PodcastSubscriptionBenefitMappings"]).ToObject<List<PodcastSubscriptionBenefitMappingListItemResponseDTO>>().Select(psbm => new PodcastSubscriptionBenefitMappingListItemResponseDTO
                            {
                                PodcastSubscriptionId = psbm.PodcastSubscriptionId,
                                Version = psbm.Version,
                                CreatedAt = psbm.CreatedAt,
                                UpdatedAt = psbm.UpdatedAt,
                                PodcastSubscriptionBenefit = psbm.PodcastSubscriptionBenefit != null ? new PodcastSubscriptionBenefitDTO
                                {
                                    Id = psbm.PodcastSubscriptionBenefit.Id,
                                    Name = psbm.PodcastSubscriptionBenefit.Name
                                } : null
                            }).ToList()
                        };
                    }).ToList(),
                    EpisodeList = episodeList.Select(pe => new EpisodeListItemResponseDTO
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
                        TakenDownReason = role == null || role == 1 ? null : pe.TakenDownReason,
                        TotalSave = pe.TotalSave,
                        Hashtags = pe.PodcastEpisodeHashtags.Select(peh => new HashtagDTO
                        {
                            Id = peh.Hashtag.Id,
                            Name = peh.Hashtag.Name
                        }).ToList(),
                        PodcastShow = new PodcastShowSnippetResponseDTO
                        {
                            Id = show.Id,
                            Name = show.Name,
                            Description = show.Description,
                            MainImageFileKey = show.MainImageFileKey,
                            IsReleased = show.IsReleased,
                            ReleaseDate = show.ReleaseDate
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
                    }).ToList(),
                    CreatedAt = show.CreatedAt,
                    UpdatedAt = show.UpdatedAt,
                };

                return showDetail;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get show by id failed, error: " + ex.Message);
            }
        }

        public async Task<ShowDetailResponseDTO> GetShowByIdForPodcasterAsync(Guid showId)
        {
            try
            {
                var query = _podcastShowGenericRepository.FindAll(
                    predicate: c => c.DeletedAt == null && c.Id == showId,
                    includeFunc: q => q
                        .Include(pc => pc.PodcastCategory)
                        .Include(pc => pc.PodcastSubCategory)
                        .Include(pc => pc.PodcastShowStatusTrackings)
                        .ThenInclude(pct => pct.PodcastShowStatus)
                        .Include(pc => pc.PodcastShowHashtags)
                        .ThenInclude(pch => pch.Hashtag)
                        .Include(pc => pc.PodcastChannel)
                        .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                        .Include(pc => pc.PodcastShowSubscriptionType)
                );


                var show = await query.FirstOrDefaultAsync();

                if (show == null)
                {
                    throw new Exception("Show with id " + showId + " does not exist");
                }
                // kiểm tra có thuộc về 1 channel đang tồn tại hay không, Podcaster không thể thấy được những show thuộc kênh đã bị xóa
                if (show.PodcastChannel != null && show.PodcastChannel.DeletedAt != null)
                {
                    throw new Exception("Show with id " + showId + " does not exist");
                }

                var podcaster = await _accountCachingService.GetAccountStatusCacheById(show.PodcasterId);
                if (podcaster == null || podcaster.Id != show.PodcasterId || podcaster.IsVerified == false || podcaster.HasVerifiedPodcasterProfile == false)
                {
                    throw new Exception("Podcaster with id " + show.PodcasterId + " does not exist");
                }

                var podcastSubscriptionBatchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "podcastSubscriptionList",
                            QueryType = "findall",
                            EntityType = "PodcastSubscription",
                            Parameters = JObject.FromObject(new
                            {
                                where = new
                                {
                                    DeletedAt = (DateTime?)null,
                                    PodcastShowId = show.Id,
                                },
                                include = "PodcastSubscriptionBenefitMappings.PodcastSubscriptionBenefit , PodcastSubscriptionCycleTypePrices.SubscriptionCycleType"
                            }),
                        }
                    }
                };

                var result = await _httpServiceQueryClient.ExecuteBatchAsync("SubscriptionService", podcastSubscriptionBatchRequest);


                var episodeByShowIdQuery = _podcastEpisodeGenericRepository.FindAll(
                    predicate: pe => pe.DeletedAt == null && pe.PodcastShowId == show.Id,
                    includeFunc: q => q
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                        .ThenInclude(pet => pet.PodcastEpisodeStatus)
                        .Include(pe => pe.PodcastEpisodeHashtags)
                        .ThenInclude(peh => peh.Hashtag)
                        .Include(pe => pe.PodcastEpisodeSubscriptionType)
                );



                var episodeList = await episodeByShowIdQuery.ToListAsync();


                var showDetail = new ShowDetailResponseDTO
                {
                    Id = show.Id,
                    Name = show.Name,
                    Description = show.Description,
                    MainImageFileKey = show.MainImageFileKey,
                    TrailerAudioFileKey = show.TrailerAudioFileKey,
                    TotalFollow = show.TotalFollow,
                    ListenCount = show.ListenCount,
                    AverageRating = show.AverageRating,
                    RatingCount = show.RatingCount,
                    CurrentStatus = show.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).Select(pst => new PodcastShowStatusDTO
                    {
                        Id = pst.PodcastShowStatus.Id,
                        Name = pst.PodcastShowStatus.Name
                    }).FirstOrDefault()!,
                    Copyright = show.Copyright,
                    IsReleased = show.IsReleased,
                    Language = show.Language,
                    UploadFrequency = show.UploadFrequency,
                    ReleaseDate = show.ReleaseDate,
                    TakenDownReason = show.TakenDownReason,
                    EpisodeCount = episodeList.Count,
                    PodcastCategory = show.PodcastCategory != null ? new PodcastCategoryDTO
                    {
                        Id = show.PodcastCategory.Id,
                        Name = show.PodcastCategory.Name,
                        MainImageFileKey = show.PodcastCategory.MainImageFileKey
                    } : null,
                    PodcastSubCategory = show.PodcastSubCategory != null ? new PodcastSubCategoryDTO
                    {
                        Id = show.PodcastSubCategory.Id,
                        Name = show.PodcastSubCategory.Name,
                        PodcastCategoryId = show.PodcastSubCategory.PodcastCategoryId
                    } : null,
                    PodcastChannel = show.PodcastChannel != null ? new PodcastChannelSnippetResponseDTO
                    {
                        Id = show.PodcastChannel.Id,
                        Name = show.PodcastChannel.Name,
                        Description = show.PodcastChannel.Description,
                        MainImageFileKey = show.PodcastChannel.MainImageFileKey
                    } : null,
                    PodcastShowSubscriptionType = show.PodcastShowSubscriptionType != null ? new PodcastShowSubscriptionTypeDTO
                    {
                        Id = show.PodcastShowSubscriptionType.Id,
                        Name = show.PodcastShowSubscriptionType.Name
                    } : null,
                    Podcaster = new AccountSnippetResponseDTO
                    {
                        Id = podcaster.Id,
                        Email = podcaster.Email,
                        FullName = podcaster.PodcasterProfileName,
                        MainImageFileKey = podcaster.MainImageFileKey
                    },
                    Hashtags = show.PodcastShowHashtags.Select(psh => new HashtagDTO
                    {
                        Id = psh.Hashtag.Id,
                        Name = psh.Hashtag.Name
                    }).ToList(),
                    PodcastSubscriptionList = ((JArray)result.Results["podcastSubscriptionList"]).Select(ps =>
                    {
                        var psObj = ps.ToObject<PodcastSubscriptionListItemResponseDTO>();
                        return new PodcastSubscriptionListItemResponseDTO
                        {
                            Id = psObj.Id,
                            Name = psObj.Name,
                            Description = psObj.Description,
                            CurrentVersion = psObj.CurrentVersion,
                            PodcastShowId = psObj.PodcastShowId,
                            IsActive = psObj.IsActive,
                            CreatedAt = psObj.CreatedAt,
                            UpdatedAt = psObj.UpdatedAt,
                            PodcastChannelId = psObj.PodcastChannelId,
                            PodcastSubscriptionCycleTypePriceList = ((JArray)ps["PodcastSubscriptionCycleTypePrices"]).ToObject<List<PodcastSubscriptionCycleTypePriceListItemResponseDTO>>().Select(psctp => new PodcastSubscriptionCycleTypePriceListItemResponseDTO
                            {
                                PodcastSubscriptionId = psctp.PodcastSubscriptionId,
                                Price = psctp.Price,
                                Version = psctp.Version,
                                CreatedAt = psctp.CreatedAt,
                                UpdatedAt = psctp.UpdatedAt,
                                SubscriptionCycleType = psctp.SubscriptionCycleType != null ? new SubscriptionCycleTypeDTO
                                {
                                    Id = psctp.SubscriptionCycleType.Id,
                                    Name = psctp.SubscriptionCycleType.Name,
                                } : null
                            }).ToList(),
                            DeletedAt = psObj.DeletedAt,
                            PodcastSubscriptionBenefitMappingList = ((JArray)ps["PodcastSubscriptionBenefitMappings"]).ToObject<List<PodcastSubscriptionBenefitMappingListItemResponseDTO>>().Select(psbm => new PodcastSubscriptionBenefitMappingListItemResponseDTO
                            {
                                PodcastSubscriptionId = psbm.PodcastSubscriptionId,
                                Version = psbm.Version,
                                CreatedAt = psbm.CreatedAt,
                                UpdatedAt = psbm.UpdatedAt,
                                PodcastSubscriptionBenefit = psbm.PodcastSubscriptionBenefit != null ? new PodcastSubscriptionBenefitDTO
                                {
                                    Id = psbm.PodcastSubscriptionBenefit.Id,
                                    Name = psbm.PodcastSubscriptionBenefit.Name
                                } : null
                            }).ToList()
                        };
                    }).ToList(),
                    EpisodeList = episodeList.Select(pe => new EpisodeListItemResponseDTO
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
                        TakenDownReason = pe.TakenDownReason,
                        TotalSave = pe.TotalSave,
                        Hashtags = pe.PodcastEpisodeHashtags.Select(peh => new HashtagDTO
                        {
                            Id = peh.Hashtag.Id,
                            Name = peh.Hashtag.Name
                        }).ToList(),
                        PodcastShow = new PodcastShowSnippetResponseDTO
                        {
                            Id = show.Id,
                            Name = show.Name,
                            Description = show.Description,
                            MainImageFileKey = show.MainImageFileKey,
                            IsReleased = show.IsReleased,
                            ReleaseDate = show.ReleaseDate
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
                    }).ToList(),
                    CreatedAt = show.CreatedAt,
                    UpdatedAt = show.UpdatedAt,
                };

                return showDetail;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get show by id failed, error: " + ex.Message);
            }
        }

        public async Task CreatePodcastShow(CreateShowParameterDTO createShowParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var podcastShow = new PodcastShow
                    {
                        Name = createShowParameterDTO.Name,
                        Description = createShowParameterDTO.Description,
                        Language = createShowParameterDTO.Language,
                        Copyright = createShowParameterDTO.Copyright,
                        UploadFrequency = createShowParameterDTO.UploadFrequency,
                        PodcasterId = createShowParameterDTO.PodcasterId,
                        PodcastCategoryId = createShowParameterDTO.PodcastCategoryId,
                        PodcastSubCategoryId = createShowParameterDTO.PodcastSubCategoryId,
                        PodcastShowSubscriptionTypeId = createShowParameterDTO.PodcastShowSubscriptionTypeId,
                        PodcastChannelId = createShowParameterDTO.PodcastChannelId,
                    };

                    var existingPodcaster = await _accountCachingService.GetAccountStatusCacheById(podcastShow.PodcasterId);
                    if (existingPodcaster == null || existingPodcaster.Id != podcastShow.PodcasterId || existingPodcaster.IsVerified == false || existingPodcaster.DeactivatedAt != null || existingPodcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + podcastShow.PodcasterId + " does not exist");
                    }

                    if (createShowParameterDTO.PodcastChannelId != null)
                    {
                        var existingPodcastChannel = await _podcastChannelGenericRepository.FindByIdAsync(createShowParameterDTO.PodcastChannelId.Value);
                        if (existingPodcastChannel == null || existingPodcastChannel.DeletedAt != null)
                        {
                            throw new Exception("Podcast channel with id " + createShowParameterDTO.PodcastChannelId + " does not exist");
                        }
                        else if (existingPodcastChannel.PodcasterId != podcastShow.PodcasterId)
                        {
                            throw new Exception("Podcast channel with id " + createShowParameterDTO.PodcastChannelId + " does not belong to podcaster with id " + podcastShow.PodcasterId);
                        }
                    }

                    await _podcastShowGenericRepository.CreateAsync(podcastShow);

                    var newPodcastShowStatusTracking = new PodcastShowStatusTracking
                    {
                        PodcastShowId = podcastShow.Id,
                        PodcastShowStatusId = (int)PodcastChannelStatusEnum.Unpublished
                    };
                    await _podcastShowStatusTrackingGenericRepository.CreateAsync(newPodcastShowStatusTracking);


                    var folderPath = _filePathConfig.PODCAST_SHOW_FILE_PATH + "\\" + podcastShow.Id;
                    if (createShowParameterDTO.MainImageFileKey != null && createShowParameterDTO.MainImageFileKey != "")
                    {
                        var MainImageFileKey = FilePathHelper.CombinePaths(folderPath, $"main_image{FilePathHelper.GetExtension(createShowParameterDTO.MainImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(createShowParameterDTO.MainImageFileKey, MainImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(createShowParameterDTO.MainImageFileKey);
                        podcastShow.MainImageFileKey = MainImageFileKey;
                    }


                    await _podcastShowGenericRepository.UpdateAsync(podcastShow.Id, podcastShow);

                    foreach (var hashtagId in createShowParameterDTO.HashtagIds)
                    {
                        var existingHashtag = await _hashtagGenericRepository.FindByIdAsync(hashtagId);
                        if (existingHashtag == null)
                        {
                            throw new Exception("Hashtag with id " + hashtagId + " does not exist");
                        }
                        var podcastShowHashtag = new PodcastShowHashtag
                        {
                            PodcastShowId = podcastShow.Id,
                            HashtagId = hashtagId
                        };
                        await _podcastShowHashtagGenericRepository.CreateAsync(podcastShowHashtag);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["Name"] = podcastShow.Name;
                    messageNextRequestData["Description"] = podcastShow.Description;
                    messageNextRequestData["Language"] = podcastShow.Language;
                    messageNextRequestData["Copyright"] = podcastShow.Copyright;
                    messageNextRequestData["UploadFrequency"] = podcastShow.UploadFrequency;
                    messageNextRequestData["MainImageFileKey"] = podcastShow.MainImageFileKey;
                    messageNextRequestData["PodcasterId"] = podcastShow.PodcasterId;
                    messageNextRequestData["PodcastCategoryId"] = podcastShow.PodcastCategoryId;
                    messageNextRequestData["PodcastSubCategoryId"] = podcastShow.PodcastSubCategoryId;
                    messageNextRequestData["PodcastShowSubscriptionTypeId"] = podcastShow.PodcastShowSubscriptionTypeId;
                    messageNextRequestData["PodcastChannelId"] = podcastShow.PodcastChannelId;
                    messageNextRequestData["HashtagIds"] = JArray.FromObject(createShowParameterDTO.HashtagIds);


                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = podcastShow.Id,
                        Name = podcastShow.Name,
                        Description = podcastShow.Description,
                        Language = podcastShow.Language,
                        Copyright = podcastShow.Copyright,
                        UploadFrequency = podcastShow.UploadFrequency,
                        MainImageFileKey = podcastShow.MainImageFileKey,
                        PodcasterId = podcastShow.PodcasterId,
                        PodcastCategoryId = podcastShow.PodcastCategoryId,
                        PodcastSubCategoryId = podcastShow.PodcastSubCategoryId,
                        PodcastShowSubscriptionTypeId = podcastShow.PodcastShowSubscriptionTypeId,
                        PodcastChannelId = podcastShow.PodcastChannelId,
                        HashtagIds = createShowParameterDTO.HashtagIds,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-show.success"
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
                            ErrorMessage = $"Create podcast show failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-show.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }


        public async Task UpdatePodcastShow(UpdateShowParameterDTO updateShowParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingPodcastShow = await _podcastShowGenericRepository.FindByIdAsync(updateShowParameterDTO.PodcastShowId, includeFunc: q => q.Include(ps => ps.PodcastShowStatusTrackings)
                    .Include(ps => ps.PodcastChannel)

                    );
                    if (existingPodcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + updateShowParameterDTO.PodcastShowId + " does not exist");
                    }
                    else if (existingPodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + updateShowParameterDTO.PodcastShowId + " has been deleted");
                    }
                    else if (existingPodcastShow.PodcasterId != updateShowParameterDTO.PodcasterId)
                    {
                        throw new Exception("Podcast show with id " + updateShowParameterDTO.PodcastShowId + " does not belong to podcaster with id " + updateShowParameterDTO.PodcasterId);
                    }
                    else if (existingPodcastShow.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnum.Removed)
                    {
                        throw new Exception("Podcast show with id " + updateShowParameterDTO.PodcastShowId + " has been removed");
                    }

                    if (existingPodcastShow.PodcastChannel != null && existingPodcastShow.PodcastChannel.DeletedAt != null)
                    {
                        throw new Exception("Podcast channel with id " + updateShowParameterDTO.PodcastShowId + " does not exist");
                    }
                    else if (existingPodcastShow.PodcastChannelId != null && existingPodcastShow.PodcasterId != updateShowParameterDTO.PodcasterId)
                    {
                        throw new Exception("Podcast channel with id " + existingPodcastShow.PodcastChannelId + " does not belong to podcaster with id " + existingPodcastShow.PodcasterId);
                    }

                    var existingPodcaster = await _accountCachingService.GetAccountStatusCacheById(existingPodcastShow.PodcasterId);
                    if (existingPodcaster == null || existingPodcaster.Id != existingPodcastShow.PodcasterId || existingPodcaster.IsVerified == false || existingPodcaster.DeactivatedAt != null || existingPodcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + existingPodcastShow.PodcasterId + " does not exist");
                    }

                    if (updateShowParameterDTO.PodcastChannelId != null)
                    {
                        var existingPodcastChannel = await _podcastChannelGenericRepository.FindByIdAsync(updateShowParameterDTO.PodcastChannelId.Value);
                        if (existingPodcastChannel == null)
                        {
                            throw new Exception("Podcast channel with id " + updateShowParameterDTO.PodcastChannelId + " does not exist");
                        }
                        else if (existingPodcastChannel.DeletedAt != null)
                        {
                            throw new Exception("Podcast channel with id " + updateShowParameterDTO.PodcastChannelId + " has been deleted");
                        }
                        else if (existingPodcastChannel.PodcasterId != existingPodcastShow.PodcasterId)
                        {
                            throw new Exception("Podcast channel with id " + updateShowParameterDTO.PodcastChannelId + " does not belong to podcaster with id " + existingPodcastShow.PodcasterId);
                        }
                    }
                    existingPodcastShow.Name = updateShowParameterDTO.Name;
                    existingPodcastShow.Description = updateShowParameterDTO.Description;
                    existingPodcastShow.Language = updateShowParameterDTO.Language;
                    existingPodcastShow.Copyright = updateShowParameterDTO.Copyright;
                    existingPodcastShow.UploadFrequency = updateShowParameterDTO.UploadFrequency;
                    existingPodcastShow.PodcastCategoryId = updateShowParameterDTO.PodcastCategoryId;
                    existingPodcastShow.PodcastSubCategoryId = updateShowParameterDTO.PodcastSubCategoryId;
                    existingPodcastShow.PodcastShowSubscriptionTypeId = updateShowParameterDTO.PodcastShowSubscriptionTypeId;
                    existingPodcastShow.PodcastChannelId = updateShowParameterDTO.PodcastChannelId;
                    if (updateShowParameterDTO.MainImageFileKey != null && updateShowParameterDTO.MainImageFileKey != "")
                    {
                        if (existingPodcastShow.MainImageFileKey != null && existingPodcastShow.MainImageFileKey != "")
                        {
                            await _fileIOHelper.DeleteFileAsync(existingPodcastShow.MainImageFileKey);
                        }
                        var folderPath = _filePathConfig.PODCAST_SHOW_FILE_PATH + "\\" + existingPodcastShow.Id;
                        var MainImageFileKey = FilePathHelper.CombinePaths(folderPath, $"main_image{FilePathHelper.GetExtension(updateShowParameterDTO.MainImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(updateShowParameterDTO.MainImageFileKey, MainImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(updateShowParameterDTO.MainImageFileKey);
                        existingPodcastShow.MainImageFileKey = MainImageFileKey;
                    }
                    await _podcastShowGenericRepository.UpdateAsync(existingPodcastShow.Id, existingPodcastShow);
                    // Update hashtags
                    await _unitOfWork.PodcastShowHashtagRepository.DeleteByPodcastShowIdAsync(existingPodcastShow.Id);

                    foreach (var hashtagId in updateShowParameterDTO.HashtagIds)
                    {
                        var existingHashtag = await _hashtagGenericRepository.FindByIdAsync(hashtagId);
                        if (existingHashtag == null)
                        {
                            throw new Exception("Hashtag with id " + hashtagId + " does not exist");
                        }
                        var podcastShowHashtag = new PodcastShowHashtag
                        {
                            PodcastShowId = existingPodcastShow.Id,
                            HashtagId = hashtagId
                        };
                        await _podcastShowHashtagGenericRepository.CreateAsync(podcastShowHashtag);
                    }
                    await transaction.CommitAsync();
                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = existingPodcastShow.Id;
                    messageNextRequestData["Name"] = existingPodcastShow.Name;
                    messageNextRequestData["Description"] = existingPodcastShow.Description;
                    messageNextRequestData["Language"] = existingPodcastShow.Language;
                    messageNextRequestData["Copyright"] = existingPodcastShow.Copyright;
                    messageNextRequestData["UploadFrequency"] = existingPodcastShow.UploadFrequency;
                    messageNextRequestData["MainImageFileKey"] = existingPodcastShow.MainImageFileKey;
                    messageNextRequestData["PodcasterId"] = existingPodcastShow.PodcasterId;
                    messageNextRequestData["PodcastCategoryId"] = existingPodcastShow.PodcastCategoryId;
                    messageNextRequestData["PodcastSubCategoryId"] = existingPodcastShow.PodcastSubCategoryId;
                    messageNextRequestData["PodcastShowSubscriptionTypeId"] = existingPodcastShow.PodcastShowSubscriptionTypeId;
                    messageNextRequestData["PodcastChannelId"] = existingPodcastShow.PodcastChannelId;
                    messageNextRequestData["HashtagIds"] = JArray.FromObject(updateShowParameterDTO.HashtagIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = existingPodcastShow.Id,
                        Name = existingPodcastShow.Name,
                        Description = existingPodcastShow.Description,
                        Language = existingPodcastShow.Language,
                        Copyright = existingPodcastShow.Copyright,
                        UploadFrequency = existingPodcastShow.UploadFrequency,
                        MainImageFileKey = existingPodcastShow.MainImageFileKey,
                        PodcasterId = existingPodcastShow.PodcasterId,
                        PodcastCategoryId = existingPodcastShow.PodcastCategoryId,
                        PodcastSubCategoryId = existingPodcastShow.PodcastSubCategoryId,
                        PodcastShowSubscriptionTypeId = existingPodcastShow.PodcastShowSubscriptionTypeId,
                        PodcastChannelId = existingPodcastShow.PodcastChannelId,
                        HashtagIds = updateShowParameterDTO.HashtagIds,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-show.success"
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
                            ErrorMessage = $"Update podcast show failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-show.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task SubmitPodcastShowTrailerAudioFile(SubmitShowTrailerAudioFileParameterDTO submitShowTrailerAudioFileParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingPodcastShow = await _podcastShowGenericRepository.FindByIdAsync(submitShowTrailerAudioFileParameterDTO.PodcastShowId);
                    if (existingPodcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + submitShowTrailerAudioFileParameterDTO.PodcastShowId + " does not exist");
                    }
                    else if (existingPodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + submitShowTrailerAudioFileParameterDTO.PodcastShowId + " has been deleted");
                    }
                    else if (existingPodcastShow.PodcasterId != submitShowTrailerAudioFileParameterDTO.PodcasterId)
                    {
                        throw new Exception("Podcast show with id " + submitShowTrailerAudioFileParameterDTO.PodcastShowId + " does not belong to podcaster with id " + submitShowTrailerAudioFileParameterDTO.PodcasterId);
                    }

                    var folderPath = _filePathConfig.PODCAST_SHOW_FILE_PATH + "\\" + submitShowTrailerAudioFileParameterDTO.PodcastShowId;
                    if (submitShowTrailerAudioFileParameterDTO.TrailerAudioFileKey != null && submitShowTrailerAudioFileParameterDTO.TrailerAudioFileKey != "")
                    {
                        if (!string.IsNullOrEmpty(existingPodcastShow?.TrailerAudioFileKey))
                        {
                            await _fileIOHelper.DeleteFileAsync(existingPodcastShow.TrailerAudioFileKey);
                        }
                        var TrailerAudioFileKey = FilePathHelper.CombinePaths(folderPath, $"trailer_audio{FilePathHelper.GetExtension(submitShowTrailerAudioFileParameterDTO.TrailerAudioFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(submitShowTrailerAudioFileParameterDTO.TrailerAudioFileKey, TrailerAudioFileKey);
                        await _fileIOHelper.DeleteFileAsync(submitShowTrailerAudioFileParameterDTO.TrailerAudioFileKey);
                        existingPodcastShow.TrailerAudioFileKey = TrailerAudioFileKey;

                    }

                    await _podcastShowGenericRepository.UpdateAsync(existingPodcastShow.Id, existingPodcastShow);
                    await transaction.CommitAsync();
                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = existingPodcastShow.Id;
                    messageNextRequestData["PodcasterId"] = existingPodcastShow.PodcasterId;
                    messageNextRequestData["TrailerAudioFileKey"] = existingPodcastShow.TrailerAudioFileKey;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = existingPodcastShow.Id,
                        PodcasterId = existingPodcastShow.PodcasterId,
                        TrailerAudioFileKey = existingPodcastShow.TrailerAudioFileKey,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "submit-show-trailer-audio-file.success"
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
                            ErrorMessage = $"Submit podcast show trailer audio file failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "submit-show-trailer-audio-file.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task PublishPodcastShow(PublishShowParameterDTO publishShowParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingPodcastShow = await _podcastShowGenericRepository.FindByIdAsync(
                        publishShowParameterDTO.PodcastShowId,
                        includeFunc: query => query
                            .Include(ps => ps.PodcastShowStatusTrackings));
                    if (existingPodcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + publishShowParameterDTO.PodcastShowId + " does not exist");
                    }
                    else if (existingPodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + publishShowParameterDTO.PodcastShowId + " has been deleted");
                    }
                    else if (existingPodcastShow.PodcasterId != publishShowParameterDTO.PodcasterId)
                    {
                        throw new Exception("Podcast show with id " + publishShowParameterDTO.PodcastShowId + " does not belong to podcaster with id " + publishShowParameterDTO.PodcasterId);
                    }
                    var currentStatusId = existingPodcastShow.PodcastShowStatusTrackings.OrderByDescending(psst => psst.CreatedAt).FirstOrDefault()?.PodcastShowStatusId;

                    // [NOTE]: nếu status hiện tại là Published hoặc TakenDown thì không thể publish lại, nếu status hiện tại không phải là ReadyToRelease thì không thể publish
                    var publishedStatusIds = new List<int>
                    {
                        (int)PodcastShowStatusEnum.Published,
                        (int)PodcastShowStatusEnum.TakenDown,
                    };

                    if (publishedStatusIds.Contains(currentStatusId.Value))
                    {
                        throw new Exception("Podcast show with id " + publishShowParameterDTO.PodcastShowId + " has already been published");
                    }
                    else if (currentStatusId != (int)PodcastShowStatusEnum.ReadyToRelease)
                    {
                        throw new Exception("Podcast show with id " + publishShowParameterDTO.PodcastShowId + " cannot be published as its current status is not 'Ready to Release'");
                    }


                    existingPodcastShow.ReleaseDate = publishShowParameterDTO.ReleaseDate;

                    var newPodcastChannelStatusTracking = new PodcastShowStatusTracking
                    {
                        PodcastShowId = existingPodcastShow.Id,
                        PodcastShowStatusId = (int)PodcastShowStatusEnum.Published
                    };

                    if (publishShowParameterDTO.ReleaseDate == null)
                    {
                        existingPodcastShow.IsReleased = true;
                    }
                    else
                    {
                        existingPodcastShow.IsReleased = false;
                    }


                    await _podcastShowGenericRepository.UpdateAsync(existingPodcastShow.Id, existingPodcastShow);
                    await _podcastShowStatusTrackingGenericRepository.CreateAsync(newPodcastChannelStatusTracking);

                    await transaction.CommitAsync();
                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = publishShowParameterDTO.PodcastShowId;
                    messageNextRequestData["PodcasterId"] = publishShowParameterDTO.PodcasterId;
                    messageNextRequestData["ReleaseDate"] = publishShowParameterDTO.ReleaseDate?.ToString("yyyy-MM-dd");
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = publishShowParameterDTO.PodcastShowId,
                        PodcasterId = publishShowParameterDTO.PodcasterId,
                        ReleaseDate = publishShowParameterDTO.ReleaseDate,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "publish-show.success"
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
                            ErrorMessage = $"Publish podcast show failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "publish-show.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task PlusPodcastShowTotalFollow(PlusShowTotalFollowParameterDTO plusShowTotalFollowParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var podcastShow = await _podcastShowGenericRepository.FindByIdAsync(plusShowTotalFollowParameterDTO.PodcastShowId);
                    if (podcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + plusShowTotalFollowParameterDTO.PodcastShowId + " does not exist");
                    }
                    else if (podcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + plusShowTotalFollowParameterDTO.PodcastShowId + " has been deleted");
                    }

                    podcastShow.TotalFollow += 1;
                    await _podcastShowGenericRepository.UpdateAsync(podcastShow.Id, podcastShow);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = podcastShow.Id;
                    messageNextRequestData["AccountId"] = command.RequestData["AccountId"];
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = podcastShow.Id,
                        AccountId = command.RequestData["AccountId"],
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "plus-show-total-follow.success"
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
                            ErrorMessage = $"Plus podcast show total follow failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "plus-show-total-follow.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task SubtractPodcastShowTotalFollow(SubtractShowTotalFollowParameterDTO subtractShowTotalFollowParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var podcastShow = await _podcastShowGenericRepository.FindByIdAsync(subtractShowTotalFollowParameterDTO.PodcastShowId);
                    if (podcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + subtractShowTotalFollowParameterDTO.PodcastShowId + " does not exist");
                    }
                    else if (podcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + subtractShowTotalFollowParameterDTO.PodcastShowId + " has been deleted");
                    }

                    podcastShow.TotalFollow = Math.Max(0, podcastShow.TotalFollow - subtractShowTotalFollowParameterDTO.AffectedAccountIds.Count);
                    await _podcastShowGenericRepository.UpdateAsync(podcastShow.Id, podcastShow);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = podcastShow.Id;
                    messageNextRequestData["AccountId"] = command.RequestData["AccountId"];
                    messageNextRequestData["AffectedAccountIds"] = JArray.FromObject(subtractShowTotalFollowParameterDTO.AffectedAccountIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = podcastShow.Id,
                        AccountId = command.RequestData["AccountId"],
                        AffectedAccountIds = subtractShowTotalFollowParameterDTO.AffectedAccountIds,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "subtract-show-total-follow.success"
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
                            ErrorMessage = $"Subtract podcast show total follow failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "subtract-show-total-follow.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }



        public async Task CreatePodcastShowReview(CreateShowReviewParameterDTO createShowReviewParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var podcastShow = await _podcastShowGenericRepository.FindAll(
                        predicate: ps => ps.Id == createShowReviewParameterDTO.PodcastShowId && ps.DeletedAt == null,
                        includeFunc: ps => ps
                            .Include(p => p.PodcastShowStatusTrackings)
                    ).FirstOrDefaultAsync();
                    if (podcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + createShowReviewParameterDTO.PodcastShowId + " does not exist");
                    }
                    var currentStatusId = podcastShow.PodcastShowStatusTrackings.OrderByDescending(psst => psst.CreatedAt).FirstOrDefault()?.PodcastShowStatusId;
                    if (currentStatusId != (int)PodcastShowStatusEnum.Published)
                    {
                        throw new Exception("Podcast show with id " + createShowReviewParameterDTO.PodcastShowId + " is not published");
                    }

                    var existingReview = (await _podcastShowReviewGenericRepository.FindAll(
                        predicate: a => a.AccountId == createShowReviewParameterDTO.AccountId && a.PodcastShowId == createShowReviewParameterDTO.PodcastShowId,
                        includeFunc: null
                        ).ToListAsync()).FirstOrDefault();
                    if (existingReview != null)
                    {
                        throw new Exception("Account with id " + createShowReviewParameterDTO.AccountId + " has already reviewed podcast show with id " + createShowReviewParameterDTO.PodcastShowId);
                    }

                    var podcastShowReview = new PodcastShowReview
                    {
                        AccountId = createShowReviewParameterDTO.AccountId,
                        PodcastShowId = createShowReviewParameterDTO.PodcastShowId,
                        Title = createShowReviewParameterDTO.Title,
                        Content = createShowReviewParameterDTO.Content,
                        Rating = createShowReviewParameterDTO.Rating,
                    };

                    await _podcastShowReviewGenericRepository.CreateAsync(podcastShowReview);
                    // cập nhật rating count cho podcast show 
                    podcastShow.RatingCount += 1;
                    podcastShow.AverageRating = ((podcastShow.AverageRating * (podcastShow.RatingCount - 1)) + createShowReviewParameterDTO.Rating) / podcastShow.RatingCount;
                    await _podcastShowGenericRepository.UpdateAsync(podcastShow.Id, podcastShow);

                    await transaction.CommitAsync();


                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["AccountId"] = podcastShowReview.AccountId;
                    messageNextRequestData["PodcastShowId"] = podcastShowReview.PodcastShowId;
                    messageNextRequestData["Title"] = podcastShowReview.Title;
                    messageNextRequestData["Content"] = podcastShowReview.Content;
                    messageNextRequestData["Rating"] = podcastShowReview.Rating;

                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Create podcast show review successfully",
                        PodcastShowReviewId = podcastShowReview.Id,
                        AccountId = podcastShowReview.AccountId,
                        PodcastShowId = podcastShowReview.PodcastShowId,
                        Title = podcastShowReview.Title,
                        Content = podcastShowReview.Content,
                        Rating = podcastShowReview.Rating
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-show-review.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Create podcast show review failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-show-review.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task UpdatePodcastShowReview(UpdateShowReviewParameterDTO updateShowReviewParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingReview = await _podcastShowReviewGenericRepository.FindByIdAsync(updateShowReviewParameterDTO.PodcastShowReviewId);

                    if (existingReview == null)
                    {
                        throw new Exception("Account with id " + updateShowReviewParameterDTO.AccountId + " has not reviewed podcast show with id " + updateShowReviewParameterDTO.PodcastShowReviewId);
                    }
                    else if (existingReview.AccountId != updateShowReviewParameterDTO.AccountId)
                    {
                        throw new Exception("Account with id " + updateShowReviewParameterDTO.AccountId + " is not the owner of podcast show review with id " + updateShowReviewParameterDTO.PodcastShowReviewId);
                    }
                    var previousRating = existingReview.Rating;
                    existingReview.Title = updateShowReviewParameterDTO.Title ?? existingReview.Title;
                    existingReview.Content = updateShowReviewParameterDTO.Content ?? existingReview.Content;
                    existingReview.Rating = updateShowReviewParameterDTO.Rating;
                    await _podcastShowReviewGenericRepository.UpdateAsync(existingReview.Id, existingReview);

                    var podcastShow = await _podcastShowGenericRepository.FindAll(
                         predicate: ps => ps.Id == existingReview.PodcastShowId && ps.DeletedAt == null,
                         includeFunc: null
                    ).FirstOrDefaultAsync();
                    if (podcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + existingReview.PodcastShowId + " does not exist");
                    }
                    // cập nhật lại average rating cho podcast show
                    podcastShow.AverageRating = ((podcastShow.AverageRating * podcastShow.RatingCount) - previousRating + existingReview.Rating) / podcastShow.RatingCount;
                    await _podcastShowGenericRepository.UpdateAsync(podcastShow.Id, podcastShow);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowReviewId"] = existingReview.Id;
                    messageNextRequestData["AccountId"] = existingReview.AccountId;
                    messageNextRequestData["Title"] = existingReview.Title;
                    messageNextRequestData["Content"] = existingReview.Content;
                    messageNextRequestData["Rating"] = existingReview.Rating;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Create podcast show review successfully",
                        PodcastShowReviewId = existingReview.Id,
                        AccountId = existingReview.AccountId,
                        Title = existingReview.Title,
                        Content = existingReview.Content,
                        Rating = existingReview.Rating

                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-show-review.success"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Update show review failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-show-review.failed"
                        );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }


        public async Task DeletePodcastShowReview(DeleteShowReviewParameterDTO deletePodcastShowReviewParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingReview = await _podcastShowReviewGenericRepository.FindByIdAsync(deletePodcastShowReviewParameterDTO.PodcastShowReviewId);
                    if (existingReview == null)
                    {
                        throw new Exception("Podcast show review not found");
                    }
                    else if (existingReview.AccountId != deletePodcastShowReviewParameterDTO.AccountId)
                    {
                        throw new Exception("Account with id " + deletePodcastShowReviewParameterDTO.AccountId + " is not the owner of podcast show review with id " + deletePodcastShowReviewParameterDTO.PodcastShowReviewId);
                    }
                    await _podcastShowReviewGenericRepository.DeleteAsync(existingReview.Id);

                    var podcastShow = await _podcastShowGenericRepository.FindAll(
                         predicate: ps => ps.Id == existingReview.PodcastShowId && ps.DeletedAt == null,
                         includeFunc: null
                    ).FirstOrDefaultAsync();
                    if (podcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + existingReview.PodcastShowId + " does not exist");
                    }

                    // cập nhật rating count cho podcaster profile
                    podcastShow.RatingCount -= 1;
                    podcastShow.AverageRating = podcastShow.RatingCount == 0 ? 0 : ((podcastShow.AverageRating * (podcastShow.RatingCount + 1)) - existingReview.Rating) / podcastShow.RatingCount;
                    await _podcastShowGenericRepository.UpdateAsync(podcastShow.Id, podcastShow);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowReviewId"] = existingReview.Id;
                    messageNextRequestData["AccountId"] = existingReview.AccountId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        // Message = "Delete podcast show review successfully",
                        PodcastShowReviewId = existingReview.Id,
                        AccountId = existingReview.AccountId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-show-review.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete show review failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-show-review.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteShowReviewDMCARemoveShowForce(DeleteShowReviewDMCARemoveShowForceParameterDTO deleteShowReviewDMCARemoveShowForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingReviews = await _podcastShowReviewGenericRepository.FindAll(
                        predicate: psr => psr.PodcastShowId == deleteShowReviewDMCARemoveShowForceParameterDTO.PodcastShowId,
                        includeFunc: null
                    ).ToListAsync();

                    foreach (var review in existingReviews)
                    {
                        await _podcastShowReviewGenericRepository.DeleteAsync(review.Id);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = deleteShowReviewDMCARemoveShowForceParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = deleteShowReviewDMCARemoveShowForceParameterDTO.PodcastShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-show-review-dmca-remove-show-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete show review DMCA remove show force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-show-review-dmca-remove-show-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeletePodcastShowReviewUnpublishShowForce(DeleteShowReviewUnpublishShowForceParameterDTO deleteShowReviewUnpublishShowForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingReviews = await _podcastShowReviewGenericRepository.FindAll(
                        predicate: psr => psr.PodcastShowId == deleteShowReviewUnpublishShowForceParameterDTO.PodcastShowId,
                        includeFunc: null
                    ).ToListAsync();

                    foreach (var review in existingReviews)
                    {
                        await _podcastShowReviewGenericRepository.DeleteAsync(review.Id);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = deleteShowReviewUnpublishShowForceParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = deleteShowReviewUnpublishShowForceParameterDTO.PodcastShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-show-review-unpublish-show-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete show review unpublish show force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-show-review-unpublish-show-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeletePodcastShowReviewShowDeletionForce(DeleteShowReviewShowDeletionForceParameterDTO deleteShowReviewShowDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await _unitOfWork.PodcastShowReviewRepository.DeleteByPodcastShowIdAsync(deleteShowReviewShowDeletionForceParameterDTO.PodcastShowId);


                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = deleteShowReviewShowDeletionForceParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = deleteShowReviewShowDeletionForceParameterDTO.PodcastShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-show-review-show-deletion-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete show review show deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-show-review-show-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteChannelShowsReviewUnpublishChannelForce(DeleteChannelShowsReviewUnpublishChannelForceParameterDTO deleteChannelShowsReviewUnpublishChannelForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var showId in deleteChannelShowsReviewUnpublishChannelForceParameterDTO.DmcaDismissedShowIds)
                    {
                        await _unitOfWork.PodcastShowReviewRepository.DeleteByPodcastShowIdAsync(showId);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = deleteChannelShowsReviewUnpublishChannelForceParameterDTO.PodcastChannelId;
                    messageNextRequestData["DmcaDismissedShowIds"] = JArray.FromObject(deleteChannelShowsReviewUnpublishChannelForceParameterDTO.DmcaDismissedShowIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = deleteChannelShowsReviewUnpublishChannelForceParameterDTO.PodcastChannelId,
                        DmcaDismissedShowIds = deleteChannelShowsReviewUnpublishChannelForceParameterDTO.DmcaDismissedShowIds,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-channel-shows-review-unpublish-channel-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete channel show review unpublish channel force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-channel-shows-review-unpublish-channel-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteChannelShowsReviewChannelDeletionForce(DeleteChannelShowsReviewChannelDeletionForceParameterDTO deleteChannelShowsReviewChannelDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // delete review của tất cả các show trong channel
                    var shows = await _podcastShowGenericRepository.FindAll(
                        predicate: ps => ps.PodcastChannelId == deleteChannelShowsReviewChannelDeletionForceParameterDTO.PodcastChannelId,
                        includeFunc: null
                    ).ToListAsync();

                    foreach (var show in shows)
                    {
                        await _unitOfWork.PodcastShowReviewRepository.DeleteByPodcastShowIdAsync(show.Id);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = deleteChannelShowsReviewChannelDeletionForceParameterDTO.PodcastChannelId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = deleteChannelShowsReviewChannelDeletionForceParameterDTO.PodcastChannelId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-channel-shows-review-channel-deletion-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete channel shows review channel deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-channel-shows-review-channel-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeletePodcasterShowsReviewTerminatePodcasterForce(DeletePodcasterShowsReviewTerminatePodcasterForceParameterDTO deletePodcasterShowsReviewTerminatePodcasterForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // delete review của tất cả các show của podcaster
                    var shows = await _podcastShowGenericRepository.FindAll(
                        predicate: ps => ps.PodcasterId == deletePodcasterShowsReviewTerminatePodcasterForceParameterDTO.PodcasterId,
                        includeFunc: null
                    ).ToListAsync();

                    foreach (var show in shows)
                    {
                        await _unitOfWork.PodcastShowReviewRepository.DeleteByPodcastShowIdAsync(show.Id);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcasterId"] = deletePodcasterShowsReviewTerminatePodcasterForceParameterDTO.PodcasterId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcasterId = deletePodcasterShowsReviewTerminatePodcasterForceParameterDTO.PodcasterId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-podcaster-shows-review-terminate-podcaster-force.success"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.PublicReviewManagementDomain,
                        requestData: command.RequestData,
                        responseData: JObject.FromObject(new
                        {
                            ErrorMessage = $"Delete podcaster shows review terminate podcaster force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-podcaster-shows-review-terminate-podcaster-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }
        public async Task RemoveShowDmcaRemoveShowForce(RemoveShowDmcaDmcaRemoveShowForceParameterDTO removeShowDmcaRemoveShowForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // xóa show
                    var show = await _podcastShowGenericRepository.FindByIdAsync(
                        removeShowDmcaRemoveShowForceParameterDTO.PodcastShowId,
                        includeFunc: ps => ps
                            .Include(s => s.PodcastShowStatusTrackings)
                    );

                    // foreach (var episode in showEpisodes)
                    // {
                    //     var currentStatusTracking = episode.PodcastEpisodeStatusTrackings
                    //         .OrderByDescending(pet => pet.CreatedAt)
                    //         .FirstOrDefault();

                    //     if (currentStatusTracking.PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Removed)
                    //     {
                    //         // Remove khác với delete
                    //         var newStatusTracking = new PodcastEpisodeStatusTracking
                    //         {
                    //             PodcastEpisodeId = episode.Id,
                    //             PodcastEpisodeStatusId = (int)PodcastEpisodeStatusEnum.Removed,
                    //         };

                    //         await _podcastEpisodeStatusTrackingGenericRepository.CreateAsync(newStatusTracking);
                    //     }
                    // }

                    if (show != null)
                    {
                        var currentStatusTracking = show.PodcastShowStatusTrackings
                            .OrderByDescending(pst => pst.CreatedAt)
                            .FirstOrDefault();

                        if (currentStatusTracking.PodcastShowStatusId != (int)PodcastShowStatusEnum.Removed)
                        {
                            // Remove khác với delete
                            var newStatusTracking = new PodcastShowStatusTracking
                            {
                                PodcastShowId = show.Id,
                                PodcastShowStatusId = (int)PodcastShowStatusEnum.Removed,
                            };

                            await _podcastShowStatusTrackingGenericRepository.CreateAsync(newStatusTracking);
                        }
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = removeShowDmcaRemoveShowForceParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = removeShowDmcaRemoveShowForceParameterDTO.PodcastShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-show-dmca-dmca-remove-show-force.success"
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
                            ErrorMessage = $"Remove show dmca remove show force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-show-dmca-dmca-remove-show-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveDismissedShowDmcaUnpublishShowForce(RemoveDismissedShowDmcaUnpublishShowForceParameterDTO removeDismissedShowDmcaUnpublishShowForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // unpublish show
                    var show = await _podcastShowGenericRepository.FindByIdAsync(
                        id: removeDismissedShowDmcaUnpublishShowForceParameterDTO.DmcaDismissedShowId,
                        includeFunc: pe => pe.Include(pe => pe.PodcastShowStatusTrackings)
                    );

                    if (show != null)
                    {
                        var currentStatusTracking = show.PodcastShowStatusTrackings
                                                .OrderByDescending(pet => pet.CreatedAt)
                                                .FirstOrDefault();

                        if (currentStatusTracking.PodcastShowStatusId != (int)PodcastShowStatusEnum.Removed)
                        {
                            // Unpublish khác với delete
                            var newStatusTracking = new PodcastShowStatusTracking
                            {
                                PodcastShowId = show.Id,
                                PodcastShowStatusId = (int)PodcastShowStatusEnum.Removed,
                            };

                            await _podcastShowStatusTrackingGenericRepository.CreateAsync(newStatusTracking);
                        }
                    }
                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = removeDismissedShowDmcaUnpublishShowForceParameterDTO.PodcastShowId;
                    messageNextRequestData["DmcaDismissedShowId"] = removeDismissedShowDmcaUnpublishShowForceParameterDTO.DmcaDismissedShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = removeDismissedShowDmcaUnpublishShowForceParameterDTO.PodcastShowId,
                        DmcaDismissedShowId = removeDismissedShowDmcaUnpublishShowForceParameterDTO.DmcaDismissedShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-dismissed-show-dmca-unpublish-show-force.success"
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
                            ErrorMessage = $"Remove dismissed show dmca unpublish show force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-dismissed-show-dmca-unpublish-show-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveDismissedShowDmcaShowDeletionForce(RemoveDismissedShowDmcaShowDeletionForceParameterDTO removeDismissedShowDmcaShowDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // xóa show
                    var show = await _podcastShowGenericRepository.FindByIdAsync(
                        id: removeDismissedShowDmcaShowDeletionForceParameterDTO.DmcaDismissedShowId,
                        includeFunc: pe => pe.Include(pe => pe.PodcastShowStatusTrackings)
                    );

                    if (show != null)
                    {
                        var currentStatusTracking = show.PodcastShowStatusTrackings
                                                .OrderByDescending(pet => pet.CreatedAt)
                                                .FirstOrDefault();

                        if (currentStatusTracking.PodcastShowStatusId != (int)PodcastShowStatusEnum.Removed)
                        {
                            // Unpublish khác với delete
                            var newStatusTracking = new PodcastShowStatusTracking
                            {
                                PodcastShowId = show.Id,
                                PodcastShowStatusId = (int)PodcastShowStatusEnum.Removed,
                            };

                            await _podcastShowStatusTrackingGenericRepository.CreateAsync(newStatusTracking);
                        }
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = removeDismissedShowDmcaShowDeletionForceParameterDTO.PodcastShowId;
                    messageNextRequestData["DmcaDismissedShowId"] = removeDismissedShowDmcaShowDeletionForceParameterDTO.DmcaDismissedShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = removeDismissedShowDmcaShowDeletionForceParameterDTO.PodcastShowId,
                        DmcaDismissedShowId = removeDismissedShowDmcaShowDeletionForceParameterDTO.DmcaDismissedShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-dismissed-show-dmca-show-deletion-force.success"
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
                            ErrorMessage = $"Remove dismissed show dmca show deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-dismissed-show-dmca-show-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveDismissedShowDmcaTerminatePodcasterForce(RemoveDismissedShowDmcaTerminatePodcasterForceParameterDTO removeDismissedShowDmcaTerminatePodcasterForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // unpublish tất cả các show của podcaster
                    var shows = await _podcastShowGenericRepository.FindAll(
                       predicate: ps => removeDismissedShowDmcaTerminatePodcasterForceParameterDTO.DmcaDismissedShowIds.Contains(ps.Id),
                       includeFunc: ps => ps.Include(ps => ps.PodcastShowStatusTrackings)
                   ).ToListAsync();

                    foreach (var show in shows)
                    {
                        var currentStatusTracking = show.PodcastShowStatusTrackings
                            .OrderByDescending(pst => pst.CreatedAt)
                            .FirstOrDefault();

                        if (currentStatusTracking.PodcastShowStatusId != (int)PodcastShowStatusEnum.Removed)
                        {
                            // Unpublish khác với delete
                            var newStatusTracking = new PodcastShowStatusTracking
                            {
                                PodcastShowId = show.Id,
                                PodcastShowStatusId = (int)PodcastShowStatusEnum.Removed,
                            };

                            await _podcastShowStatusTrackingGenericRepository.CreateAsync(newStatusTracking);
                        }
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcasterId"] = removeDismissedShowDmcaTerminatePodcasterForceParameterDTO.PodcasterId;
                    messageNextRequestData["DmcaDismissedShowIds"] = JArray.FromObject(removeDismissedShowDmcaTerminatePodcasterForceParameterDTO.DmcaDismissedShowIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcasterId = removeDismissedShowDmcaTerminatePodcasterForceParameterDTO.PodcasterId,
                        DmcaDismissedShowIds = removeDismissedShowDmcaTerminatePodcasterForceParameterDTO.DmcaDismissedShowIds,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-dismissed-show-dmca-terminate-podcaster-force.success"
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
                            ErrorMessage = $"Remove dismissed show dmca terminate podcaster force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-dismissed-show-dmca-terminate-podcaster-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task UnpublishShowUnpublishShowForce(UnpublishShowUnpublishShowForceParameterDTO unpublishShowUnpublishShowForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // unpublish episode
                    var show = await _podcastShowGenericRepository.FindByIdAsync(
                        id: unpublishShowUnpublishShowForceParameterDTO.PodcastShowId,
                        includeFunc: pe => pe.Include(pe => pe.PodcastShowStatusTrackings)
                        .Include(pe => pe.PodcastEpisodes)
                            .ThenInclude(e => e.PodcastEpisodeStatusTrackings)
                    );

                    if (show != null)
                    {
                        var currentStatusTracking = show.PodcastShowStatusTrackings
                                                .OrderByDescending(pet => pet.CreatedAt)
                                                .FirstOrDefault();

                        if (currentStatusTracking.PodcastShowStatusId != (int)PodcastShowStatusEnum.Published && currentStatusTracking.PodcastShowStatusId != (int)PodcastShowStatusEnum.TakenDown)
                        {
                            // nếu có ít nhất 1 episode ở trạng thái published thì chuyển thành ready to release, ngược lại là draft
                            bool hasPublishedEpisode = false;
                            foreach (var episode in show.PodcastEpisodes)
                            {
                                var episodeCurrentStatusTracking = episode.PodcastEpisodeStatusTrackings
                                    .OrderByDescending(pet => pet.CreatedAt)
                                    .FirstOrDefault();

                                if (episodeCurrentStatusTracking.PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published)
                                {
                                    hasPublishedEpisode = true;
                                    break;
                                }
                            }
                            var newStatusTracking = new PodcastShowStatusTracking
                            {
                                PodcastShowId = show.Id,
                                PodcastShowStatusId = hasPublishedEpisode ? (int)PodcastShowStatusEnum.ReadyToRelease : (int)PodcastShowStatusEnum.Draft,
                            };

                            await _podcastShowStatusTrackingGenericRepository.CreateAsync(newStatusTracking);

                            show.TotalFollow = 0;
                            show.ListenCount = 0;
                            await _podcastShowGenericRepository.UpdateAsync(show.Id, show);
                        }
                    }


                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = unpublishShowUnpublishShowForceParameterDTO.PodcastShowId;
                    messageNextRequestData["DmcaDismissedEpisodeIds"] = command.RequestData["DmcaDismissedEpisodeIds"];
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = unpublishShowUnpublishShowForceParameterDTO.PodcastShowId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "unpublish-show-unpublish-show-force.success"
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
                            ErrorMessage = $"Unpublish show unpublish show force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "unpublish-show-unpublish-show-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task UnpublishPodcasterShowsTerminatePodcasterForce(UnpublishPodcasterShowsTerminatePodcasterForceParameterDTO unpublishPodcasterShowsTerminatePodcasterForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var shows = await _podcastShowGenericRepository.FindAll(
                        predicate: ps => ps.PodcasterId == unpublishPodcasterShowsTerminatePodcasterForceParameterDTO.PodcasterId,
                        includeFunc: ps => ps.Include(ps => ps.PodcastShowStatusTrackings)
                    ).ToListAsync();

                    foreach (var show in shows)
                    {
                        var currentStatusTracking = show.PodcastShowStatusTrackings
                            .OrderByDescending(pst => pst.CreatedAt)
                            .FirstOrDefault();

                        if (currentStatusTracking.PodcastShowStatusId != (int)PodcastShowStatusEnum.Draft)
                        {
                            var newStatusTracking = new PodcastShowStatusTracking
                            {
                                PodcastShowId = show.Id,
                                PodcastShowStatusId = (int)PodcastShowStatusEnum.Draft,
                            };

                            await _podcastShowStatusTrackingGenericRepository.CreateAsync(newStatusTracking);

                            show.TotalFollow = 0;
                            show.ListenCount = 0;
                            await _podcastShowGenericRepository.UpdateAsync(show.Id, show);
                        }
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcasterId"] = unpublishPodcasterShowsTerminatePodcasterForceParameterDTO.PodcasterId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcasterId = unpublishPodcasterShowsTerminatePodcasterForceParameterDTO.PodcasterId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "unpublish-podcaster-shows-terminate-podcaster-force.success"
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
                            ErrorMessage = $"Unpublish podcaster shows terminate podcaster force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "unpublish-podcaster-shows-terminate-podcaster-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveChannelDismissedShowsDmcaUnpublishChannelForce(RemoveChannelDismissedShowsDmcaUnpublishChannelForceParameterDTO removeChannelDismissedShowsDmcaUnpublishChannelForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var shows = await _podcastShowGenericRepository.FindAll(
                        predicate: ps => removeChannelDismissedShowsDmcaUnpublishChannelForceParameterDTO.DmcaDismissedShowIds.Contains(ps.Id),
                        includeFunc: ps => ps.Include(ps => ps.PodcastShowStatusTrackings)
                    ).ToListAsync();

                    foreach (var show in shows)
                    {
                        var currentStatusTracking = show.PodcastShowStatusTrackings
                            .OrderByDescending(pst => pst.CreatedAt)
                            .FirstOrDefault();

                        if (currentStatusTracking.PodcastShowStatusId != (int)PodcastShowStatusEnum.Removed)
                        {
                            // Unpublish khác với delete
                            var newStatusTracking = new PodcastShowStatusTracking
                            {
                                PodcastShowId = show.Id,
                                PodcastShowStatusId = (int)PodcastShowStatusEnum.Removed,
                            };

                            await _podcastShowStatusTrackingGenericRepository.CreateAsync(newStatusTracking);
                        }
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = removeChannelDismissedShowsDmcaUnpublishChannelForceParameterDTO.PodcastChannelId;
                    messageNextRequestData["DmcaDismissedShowIds"] = JArray.FromObject(removeChannelDismissedShowsDmcaUnpublishChannelForceParameterDTO.DmcaDismissedShowIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = removeChannelDismissedShowsDmcaUnpublishChannelForceParameterDTO.PodcastChannelId,
                        DmcaDismissedShowIds = removeChannelDismissedShowsDmcaUnpublishChannelForceParameterDTO.DmcaDismissedShowIds
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-channel-dismissed-shows-dmca-unpublish-channel-force.success"
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
                            ErrorMessage = $"Remove channel dismissed show dmca unpublish channel force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-channel-dismissed-shows-dmca-unpublish-channel-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task RemoveChannelDismissedShowsDmcaChannelDeletionForce(RemoveChannelDismissedShowsDmcaChannelDeletionForceParameterDTO removeChannelDismissedShowsDmcaChannelDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var shows = await _podcastShowGenericRepository.FindAll(
                        predicate: ps => removeChannelDismissedShowsDmcaChannelDeletionForceParameterDTO.DmcaDismissedShowIds.Contains(ps.Id),
                        includeFunc: ps => ps.Include(ps => ps.PodcastShowStatusTrackings)
                    ).ToListAsync();

                    foreach (var show in shows)
                    {
                        var currentStatusTracking = show.PodcastShowStatusTrackings
                            .OrderByDescending(pst => pst.CreatedAt)
                            .FirstOrDefault();

                        if (currentStatusTracking.PodcastShowStatusId != (int)PodcastShowStatusEnum.Removed)
                        {
                            // Unpublish khác với delete
                            var newStatusTracking = new PodcastShowStatusTracking
                            {
                                PodcastShowId = show.Id,
                                PodcastShowStatusId = (int)PodcastShowStatusEnum.Removed,
                            };

                            await _podcastShowStatusTrackingGenericRepository.CreateAsync(newStatusTracking);
                        }
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = removeChannelDismissedShowsDmcaChannelDeletionForceParameterDTO.PodcastChannelId;
                    messageNextRequestData["DmcaDismissedShowIds"] = JArray.FromObject(removeChannelDismissedShowsDmcaChannelDeletionForceParameterDTO.DmcaDismissedShowIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = removeChannelDismissedShowsDmcaChannelDeletionForceParameterDTO.PodcastChannelId,
                        DmcaDismissedShowIds = removeChannelDismissedShowsDmcaChannelDeletionForceParameterDTO.DmcaDismissedShowIds
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-channel-dismissed-shows-dmca-channel-deletion-force.success"
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
                            ErrorMessage = $"Remove channel dismissed show dmca channel deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "remove-channel-dismissed-shows-dmca-channel-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteShowShowShowDeletionForce(DeleteShowShowShowDeletionForceParameterDTO deleteShowShowShowDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var show = await _podcastShowGenericRepository.FindByIdAsync(
                        id: deleteShowShowShowDeletionForceParameterDTO.PodcastShowId,
                        includeFunc: ps => ps.Include(ps => ps.PodcastShowStatusTrackings)
                    );

                    if (show != null)
                    {
                        show.DeletedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastShowGenericRepository.UpdateAsync(show.Id, show);
                    }

                    await transaction.CommitAsync();
                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = deleteShowShowShowDeletionForceParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = deleteShowShowShowDeletionForceParameterDTO.PodcastShowId,
                    }
                    );
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-show-show-deletion-force.success"
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
                            ErrorMessage = $"Delete show show deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-show-show-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteChannelShowsChannelDeletionForce(DeleteChannelShowsChannelDeletionForceParameterDTO deleteChannelShowsChannelDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var shows = await _podcastShowGenericRepository.FindAll(
                        predicate: ps => ps.PodcastChannelId == deleteChannelShowsChannelDeletionForceParameterDTO.PodcastChannelId
                                    && ps.DeletedAt == null,
                        includeFunc: ps => ps.Include(ps => ps.PodcastShowStatusTrackings)
                    ).ToListAsync();

                    foreach (var show in shows)
                    {
                        show.DeletedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastShowGenericRepository.UpdateAsync(show.Id, show);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = deleteChannelShowsChannelDeletionForceParameterDTO.PodcastChannelId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = deleteChannelShowsChannelDeletionForceParameterDTO.PodcastChannelId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-channel-shows-channel-deletion-force.success"
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
                            ErrorMessage = $"Delete channel shows channel deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-channel-shows-channel-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task TakedownContentDmcaShow(TakedownContentDmcaParameterDTO takedownContentDmcaParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingPodcastShow = await _podcastShowGenericRepository.FindByIdAsync(takedownContentDmcaParameterDTO.PodcastShowId, includeFunc: q => q.Include(ps => ps.PodcastShowStatusTrackings)
                    .Include(ps => ps.PodcastChannel)

                    );
                    if (existingPodcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + takedownContentDmcaParameterDTO.PodcastShowId + " does not exist");
                    }
                    else if (existingPodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + takedownContentDmcaParameterDTO.PodcastShowId + " has been deleted");
                    }
                    else if (existingPodcastShow.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId != (int)PodcastShowStatusEnum.Published)
                    {
                        throw new Exception("Podcast show with id " + takedownContentDmcaParameterDTO.PodcastShowId + " is not in Published status");
                    }

                    if (existingPodcastShow.PodcastChannel != null && existingPodcastShow.PodcastChannel.DeletedAt != null)
                    {
                        throw new Exception("Podcast channel with id " + existingPodcastShow.PodcastChannelId + " does not exist");
                    }

                    // Change show status to TakenDown
                    var newStatusTracking = new PodcastShowStatusTracking
                    {
                        PodcastShowId = existingPodcastShow.Id,
                        PodcastShowStatusId = (int)PodcastShowStatusEnum.TakenDown,
                    };

                    await _podcastShowStatusTrackingGenericRepository.CreateAsync(newStatusTracking);

                    existingPodcastShow.TakenDownReason = takedownContentDmcaParameterDTO.TakenDownReason;
                    await _podcastShowGenericRepository.UpdateAsync(existingPodcastShow.Id, existingPodcastShow);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = takedownContentDmcaParameterDTO.PodcastShowId;
                    messageNextRequestData["TakenDownReason"] = takedownContentDmcaParameterDTO.TakenDownReason;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = takedownContentDmcaParameterDTO.PodcastShowId,
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
                            ErrorMessage = $"Takedown content dmca show failed, error: {ex.Message}"
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

        public async Task RestoreContentDmcaShow(RestoreContentDmcaParameterDTO restoreContentDmcaParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingPodcastShow = await _podcastShowGenericRepository.FindByIdAsync(restoreContentDmcaParameterDTO.PodcastShowId, includeFunc: q => q.Include(ps => ps.PodcastShowStatusTrackings)
                    .Include(ps => ps.PodcastChannel)

                    );
                    if (existingPodcastShow == null)
                    {
                        throw new Exception("Podcast show with id " + restoreContentDmcaParameterDTO.PodcastShowId + " does not exist");
                    }
                    else if (existingPodcastShow.DeletedAt != null)
                    {
                        throw new Exception("Podcast show with id " + restoreContentDmcaParameterDTO.PodcastShowId + " has been deleted");
                    }
                    else if (existingPodcastShow.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId != (int)PodcastShowStatusEnum.TakenDown)
                    {
                        throw new Exception("Podcast show with id " + restoreContentDmcaParameterDTO.PodcastShowId + " is not in TakenDown status");
                    }

                    if (existingPodcastShow.PodcastChannel != null && existingPodcastShow.PodcastChannel.DeletedAt != null)
                    {
                        throw new Exception("Podcast channel with id " + existingPodcastShow.PodcastChannelId + " does not exist");
                    }

                    // Change show status to Published
                    var newStatusTracking = new PodcastShowStatusTracking
                    {
                        PodcastShowId = existingPodcastShow.Id,
                        PodcastShowStatusId = (int)PodcastShowStatusEnum.Published,
                    };

                    await _podcastShowStatusTrackingGenericRepository.CreateAsync(newStatusTracking);

                    existingPodcastShow.TakenDownReason = string.Empty;
                    await _podcastShowGenericRepository.UpdateAsync(existingPodcastShow.Id, existingPodcastShow);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastShowId"] = restoreContentDmcaParameterDTO.PodcastShowId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastShowId = restoreContentDmcaParameterDTO.PodcastShowId,
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
                            ErrorMessage = $"Restore content dmca show failed, error: {ex.Message}"
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

        public async Task ReleasePublishShowsAsync()
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // lấy tất cả các show đang ở trạng thái Published và isReleased = false và chưa bị xoá và nằm trong channel chưa bị xoá (channel có thể có hoặc không)   
                    // lấy cột releaseDate ra xem nó có == hôm nay không, nếu có thì chuyển isReleased = true
                    var showsToRelease = await _podcastShowGenericRepository.FindAll(
                        predicate: ps => ps.IsReleased == false
                                    && ps.ReleaseDate != null
                                    && ps.DeletedAt == null
                                    && (ps.PodcastChannel == null || ps.PodcastChannel.DeletedAt == null),
                        includeFunc: ps => ps.Include(ps => ps.PodcastShowStatusTrackings)
                    ).ToListAsync();

                    foreach (var show in showsToRelease)
                    {
                        var currentStatusTracking = show.PodcastShowStatusTrackings
                            .OrderByDescending(pst => pst.CreatedAt)
                            .FirstOrDefault();

                        if (show.ReleaseDate <= DateOnly.FromDateTime(_dateHelper.GetNowByAppTimeZone())
                            && currentStatusTracking.PodcastShowStatusId == (int)PodcastShowStatusEnum.Published)
                        {
                            show.IsReleased = true;
                            await _podcastShowGenericRepository.UpdateAsync(show.Id, show);
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

    }
}