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
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateChannel;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PublishChannel;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.PlusChannelTotalFavorite;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubtractChannelTotalFavorite;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UnpublishChannelUnpublishChannelForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteChannelChannelDeletionForce;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UnpublishPodcasterChannelsTerminatePodcasterForce;
using PodcastService.BusinessLogic.Services.DbServices.CachingServices;
using PodcastService.BusinessLogic.DTOs.SystemConfiguration;

namespace PodcastService.BusinessLogic.Services.DbServices.PodcastServices
{
    public class PodcastChannelService
    {
        // LOGGER
        private readonly ILogger<PodcastChannelService> _logger;

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

        public PodcastChannelService(
            ILogger<PodcastChannelService> logger,
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

        #region Sample coding format must be followed
        #endregion

        public async Task<List<ChannelListItemResponseDTO>> GetChannels(int? roleId, int? podcasterId)
        {
            // tuân thủ cách viết ở trên
            try
            {
                var query = _podcastChannelGenericRepository.FindAll(
                    predicate: c => c.DeletedAt == null && (podcasterId == null || c.PodcasterId == podcasterId),
                    includeFunc: q => q
                        .Include(pc => pc.PodcastCategory)
                        .Include(pc => pc.PodcastSubCategory)
                        .Include(pc => pc.PodcastChannelStatusTrackings)
                        .ThenInclude(pct => pct.PodcastChannelStatus)
                        .Include(pc => pc.PodcastChannelHashtags)
                        .ThenInclude(pch => pch.Hashtag)
                        .Include(pc => pc.PodcastShows)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                );

                if (roleId == null || roleId == 1)
                {
                    query = query.Where(pc => pc.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).FirstOrDefault().PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published);
                }
                var channels = await query.ToListAsync();

                var channelList = (await Task.WhenAll(channels.Select(async pc =>
                {
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(pc.PodcasterId);
                    if (podcaster == null || podcaster.Id != pc.PodcasterId || podcaster.IsVerified == false || podcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + pc.PodcasterId + " does not exist");
                    }
                    return new ChannelListItemResponseDTO
                    {
                        Id = pc.Id,
                        Name = pc.Name,
                        Description = pc.Description,
                        MainImageFileKey = pc.MainImageFileKey,
                        BackgroundImageFileKey = pc.BackgroundImageFileKey,
                        PodcastCategory = pc.PodcastCategory != null ? new PodcastCategoryDTO
                        {
                            Id = pc.PodcastCategory.Id,
                            Name = pc.PodcastCategory.Name,
                            MainImageFileKey = pc.PodcastCategory.MainImageFileKey
                        } : null,
                        PodcastSubCategory = pc.PodcastSubCategory != null ? new PodcastSubCategoryDTO
                        {
                            Id = pc.PodcastSubCategory.Id,
                            Name = pc.PodcastSubCategory.Name,
                            PodcastCategoryId = pc.PodcastSubCategory.PodcastCategoryId
                        } : null,
                        CurrentStatus = pc.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).Select(pct => new PodcastChannelStatusDTO
                        {
                            Id = pct.PodcastChannelStatus.Id,
                            Name = pct.PodcastChannelStatus.Name
                        }).FirstOrDefault()!,
                        Hashtags = pc.PodcastChannelHashtags.Select(pch => new HashtagDTO
                        {
                            Id = pch.Hashtag.Id,
                            Name = pch.Hashtag.Name
                        }).ToList(),
                        TotalFavorite = pc.TotalFavorite,
                        ListenCount = pc.ListenCount,
                        ShowCount = pc.PodcastShows != null ? pc.PodcastShows.Count(ps =>
                        {
                            if (roleId == null || roleId == 1)
                            {
                                return ps.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId != (int)PodcastShowStatusEnum.Published && ps.DeletedAt == null;
                            }
                            return ps.DeletedAt == null;
                        }) : 0,
                        Podcaster = new AccountSnippetResponseDTO
                        {
                            Id = podcaster.Id,
                            FullName = podcaster.PodcasterProfileName,
                            Email = podcaster.Email,
                            MainImageFileKey = podcaster.MainImageFileKey
                        },
                        CreatedAt = pc.CreatedAt,
                        UpdatedAt = pc.UpdatedAt
                    };
                }))).ToList();
                return channelList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get channels failed, error: " + ex.Message);
            }
        }

        public async Task<List<ChannelListItemResponseDTO>> GetChannelByPodcasterIdAsync(int podcasterId)
        {
            // tuân thủ cách viết ở trên
            try
            {
                var query = _podcastChannelGenericRepository.FindAll(
                    predicate: c => c.DeletedAt == null && c.PodcasterId == podcasterId,
                    includeFunc: q => q
                        .Include(pc => pc.PodcastCategory)
                        .Include(pc => pc.PodcastSubCategory)
                        .Include(pc => pc.PodcastChannelStatusTrackings)
                        .ThenInclude(pct => pct.PodcastChannelStatus)
                        .Include(pc => pc.PodcastChannelHashtags)
                        .ThenInclude(pch => pch.Hashtag)
                        .Include(pc => pc.PodcastShows)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                );

                var channels = await query.ToListAsync();

                var channelList = (await Task.WhenAll(channels.Select(async pc =>
                {
                    var podcaster = await _accountCachingService.GetAccountStatusCacheById(pc.PodcasterId);
                    if (podcaster == null || podcaster.Id != pc.PodcasterId || podcaster.IsVerified == false || podcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + pc.PodcasterId + " does not exist");
                    }
                    return new ChannelListItemResponseDTO
                    {
                        Id = pc.Id,
                        Name = pc.Name,
                        Description = pc.Description,
                        MainImageFileKey = pc.MainImageFileKey,
                        BackgroundImageFileKey = pc.BackgroundImageFileKey,
                        PodcastCategory = pc.PodcastCategory != null ? new PodcastCategoryDTO
                        {
                            Id = pc.PodcastCategory.Id,
                            Name = pc.PodcastCategory.Name,
                            MainImageFileKey = pc.PodcastCategory.MainImageFileKey
                        } : null,
                        PodcastSubCategory = pc.PodcastSubCategory != null ? new PodcastSubCategoryDTO
                        {
                            Id = pc.PodcastSubCategory.Id,
                            Name = pc.PodcastSubCategory.Name,
                            PodcastCategoryId = pc.PodcastSubCategory.PodcastCategoryId
                        } : null,
                        CurrentStatus = pc.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).Select(pct => new PodcastChannelStatusDTO
                        {
                            Id = pct.PodcastChannelStatus.Id,
                            Name = pct.PodcastChannelStatus.Name
                        }).FirstOrDefault()!,
                        Hashtags = pc.PodcastChannelHashtags.Select(pch => new HashtagDTO
                        {
                            Id = pch.Hashtag.Id,
                            Name = pch.Hashtag.Name
                        }).ToList(),
                        TotalFavorite = pc.TotalFavorite,
                        ListenCount = pc.ListenCount,
                        ShowCount = pc.PodcastShows != null ? pc.PodcastShows.Count(ps => ps.DeletedAt == null) : 0,
                        Podcaster = new AccountSnippetResponseDTO
                        {
                            Id = podcaster.Id,
                            FullName = podcaster.PodcasterProfileName,
                            Email = podcaster.Email,
                            MainImageFileKey = podcaster.MainImageFileKey
                        },
                        CreatedAt = pc.CreatedAt,
                        UpdatedAt = pc.UpdatedAt
                    };
                }))

                ).ToList();
                return channelList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get channels failed, error: " + ex.Message);
            }
        }

        public async Task<ChannelDetailResponseDTO> GetChannelByIdAsync(Guid channelId, int? role)
        {
            try
            {
                var query = _podcastChannelGenericRepository.FindAll(
                    predicate: c => c.DeletedAt == null && c.Id == channelId,
                    includeFunc: q => q
                        .Include(pc => pc.PodcastCategory)
                        .Include(pc => pc.PodcastSubCategory)
                        .Include(pc => pc.PodcastChannelStatusTrackings)
                        .ThenInclude(pct => pct.PodcastChannelStatus)
                        .Include(pc => pc.PodcastChannelHashtags)
                        .ThenInclude(pch => pch.Hashtag)
                        .Include(pc => pc.PodcastShows)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                );

                if (role == null || role == 1)
                {
                    query = query.Where(pc => pc.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).FirstOrDefault().PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published);
                }

                var channel = await query.FirstOrDefaultAsync();

                if (channel == null)
                {
                    throw new Exception("Channel with id " + channelId + " does not exist");
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
                                    PodcastChannelId = channel.Id,
                                } : new {
                                    IsActive = (bool?)null,
                                    DeletedAt = (DateTime?)null,
                                    PodcastChannelId = channel.Id,
                                },
                                include = "PodcastSubscriptionBenefitMappings.PodcastSubscriptionBenefit , PodcastSubscriptionCycleTypePrices.SubscriptionCycleType"
                            }),
                        }
                    }
                };

                var result = await _httpServiceQueryClient.ExecuteBatchAsync("SubscriptionService", podcastSubscriptionBatchRequest);


                var showByChannelIdQuery = _podcastShowGenericRepository.FindAll(
                    predicate: ps => ps.DeletedAt == null && ps.PodcastChannelId == channel.Id,
                    includeFunc: q => q
                        .Include(ps => ps.PodcastShowStatusTrackings)
                        .ThenInclude(pst => pst.PodcastShowStatus)
                        .Include(ps => ps.PodcastCategory)
                        .Include(ps => ps.PodcastSubCategory)
                        .Include(ps => ps.PodcastShowHashtags)
                        .ThenInclude(psh => psh.Hashtag)
                        .Include(ps => ps.PodcastShowSubscriptionType)
                        .Include(ps => ps.PodcastEpisodes)
                        .ThenInclude(pe => pe.PodcastEpisodeStatusTrackings)
                );

                if (role == null || role == 1)
                {
                    showByChannelIdQuery = showByChannelIdQuery.Where(ps => ps.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnum.Published && ps.IsReleased != null); // đã đăng và có ngày phát hành (có thể là đã phát hành hoặc sắp phát hành)
                }
                var showList = await showByChannelIdQuery.ToListAsync();

                var podcaster = await _accountCachingService.GetAccountStatusCacheById(channel.PodcasterId);
                if (podcaster == null || podcaster.Id != channel.PodcasterId || podcaster.IsVerified == false || podcaster.HasVerifiedPodcasterProfile == false)
                {
                    throw new Exception("Podcaster with id " + channel.PodcasterId + " does not exist");
                }


                var channelDetail = new ChannelDetailResponseDTO
                {
                    Id = channel.Id,
                    Name = channel.Name,
                    Description = channel.Description,
                    MainImageFileKey = channel.MainImageFileKey,
                    BackgroundImageFileKey = channel.BackgroundImageFileKey,
                    PodcastCategory = channel.PodcastCategory != null ? new PodcastCategoryDTO
                    {
                        Id = channel.PodcastCategory.Id,
                        Name = channel.PodcastCategory.Name,
                        MainImageFileKey = channel.PodcastCategory.MainImageFileKey
                    } : null,
                    PodcastSubCategory = channel.PodcastSubCategory != null ? new PodcastSubCategoryDTO
                    {
                        Id = channel.PodcastSubCategory.Id,
                        Name = channel.PodcastSubCategory.Name,
                        PodcastCategoryId = channel.PodcastSubCategory.PodcastCategoryId
                    } : null,
                    CurrentStatus = channel.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).Select(pct => new PodcastChannelStatusDTO
                    {
                        Id = pct.PodcastChannelStatus.Id,
                        Name = pct.PodcastChannelStatus.Name
                    }).FirstOrDefault()!,
                    Hashtags = channel.PodcastChannelHashtags.Select(pch => new HashtagDTO
                    {
                        Id = pch.Hashtag.Id,
                        Name = pch.Hashtag.Name
                    }).ToList(),
                    TotalFavorite = channel.TotalFavorite,
                    ListenCount = channel.ListenCount,
                    ShowCount = showList.Count,
                    Podcaster = new AccountSnippetResponseDTO
                    {
                        Id = podcaster.Id,
                        FullName = podcaster.PodcasterProfileName,
                        Email = podcaster.Email,
                        MainImageFileKey = podcaster.MainImageFileKey
                    },
                    CreatedAt = channel.CreatedAt,
                    UpdatedAt = channel.UpdatedAt,
                    ShowList = showList.Select(ps => new ShowListItemResponseDTO
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
                        TakenDownReason = role == null || role == 1 ? null : ps.TakenDownReason,
                        // EpisodeCount = ps.PodcastEpisodes != null ? (
                        //     role == null || role == 1 ?
                        //      (ps.PodcastEpisodes.Count(pe => pe.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Published && pe.DeletedAt == null)) : ((pe.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published && pe.DeletedAt == null)
                        //     )),
                        EpisodeCount = ps.PodcastEpisodes != null ? ps.PodcastEpisodes.Count(pe =>
                            {
                                if (role == null || role == 1)
                                {
                                    return pe.PodcastEpisodeStatusTrackings.OrderByDescending(pet => pet.CreatedAt).FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published && pe.DeletedAt == null;
                                }
                                else
                                {
                                    return pe.DeletedAt == null;
                                }
                            }) : 0,
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
                        PodcastChannel = new PodcastChannelSnippetResponseDTO
                        {
                            Id = channel.Id,
                            Name = channel.Name,
                            Description = channel.Description,
                            MainImageFileKey = channel.MainImageFileKey
                        },
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
                    }).ToList()

                };
                return channelDetail;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get channel by id failed, error: " + ex.Message);
            }
        }

        public async Task<ChannelDetailResponseDTO> GetChannelByIdForPodcasterAsync(Guid channelId)
        {
            try
            {
                var query = _podcastChannelGenericRepository.FindAll(
                    predicate: c => c.DeletedAt == null && c.Id == channelId,
                    includeFunc: q => q
                        .Include(pc => pc.PodcastCategory)
                        .Include(pc => pc.PodcastSubCategory)
                        .Include(pc => pc.PodcastChannelStatusTrackings)
                        .ThenInclude(pct => pct.PodcastChannelStatus)
                        .Include(pc => pc.PodcastChannelHashtags)
                        .ThenInclude(pch => pch.Hashtag)
                        .Include(pc => pc.PodcastShows)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                );

                var channel = await query.FirstOrDefaultAsync();

                if (channel == null)
                {
                    throw new Exception("Channel with id " + channelId + " does not exist");
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
                                where = new {
                                    DeletedAt = (DateTime?)null,
                                    PodcastChannelId = channel.Id,
                                },
                                include = "PodcastSubscriptionBenefitMappings.PodcastSubscriptionBenefit , PodcastSubscriptionCycleTypePrices.SubscriptionCycleType"
                            }),
                        }
                    }
                };

                var result = await _httpServiceQueryClient.ExecuteBatchAsync("SubscriptionService", podcastSubscriptionBatchRequest);


                var showByChannelIdQuery = _podcastShowGenericRepository.FindAll(
                    predicate: ps => ps.DeletedAt == null && ps.PodcastChannelId == channel.Id,
                    includeFunc: q => q
                        .Include(ps => ps.PodcastShowStatusTrackings)
                        .ThenInclude(pst => pst.PodcastShowStatus)
                        .Include(ps => ps.PodcastCategory)
                        .Include(ps => ps.PodcastSubCategory)
                        .Include(ps => ps.PodcastShowHashtags)
                        .ThenInclude(psh => psh.Hashtag)
                        .Include(ps => ps.PodcastShowSubscriptionType)
                        .Include(ps => ps.PodcastEpisodes)
                        .ThenInclude(pe => pe.PodcastEpisodeStatusTrackings)
                );


                var showList = await showByChannelIdQuery.ToListAsync();

                var podcaster = await _accountCachingService.GetAccountStatusCacheById(channel.PodcasterId);
                if (podcaster == null || podcaster.Id != channel.PodcasterId || podcaster.IsVerified == false || podcaster.HasVerifiedPodcasterProfile == false)
                {
                    throw new Exception("Podcaster with id " + channel.PodcasterId + " does not exist");
                }

                var channelDetail = new ChannelDetailResponseDTO
                {
                    Id = channel.Id,
                    Name = channel.Name,
                    Description = channel.Description,
                    MainImageFileKey = channel.MainImageFileKey,
                    BackgroundImageFileKey = channel.BackgroundImageFileKey,
                    PodcastCategory = channel.PodcastCategory != null ? new PodcastCategoryDTO
                    {
                        Id = channel.PodcastCategory.Id,
                        Name = channel.PodcastCategory.Name,
                        MainImageFileKey = channel.PodcastCategory.MainImageFileKey
                    } : null,
                    PodcastSubCategory = channel.PodcastSubCategory != null ? new PodcastSubCategoryDTO
                    {
                        Id = channel.PodcastSubCategory.Id,
                        Name = channel.PodcastSubCategory.Name,
                        PodcastCategoryId = channel.PodcastSubCategory.PodcastCategoryId
                    } : null,
                    CurrentStatus = channel.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).Select(pct => new PodcastChannelStatusDTO
                    {
                        Id = pct.PodcastChannelStatus.Id,
                        Name = pct.PodcastChannelStatus.Name
                    }).FirstOrDefault()!,
                    Hashtags = channel.PodcastChannelHashtags.Select(pch => new HashtagDTO
                    {
                        Id = pch.Hashtag.Id,
                        Name = pch.Hashtag.Name
                    }).ToList(),
                    TotalFavorite = channel.TotalFavorite,
                    ListenCount = channel.ListenCount,
                    ShowCount = showList.Count,
                    Podcaster = new AccountSnippetResponseDTO
                    {
                        Id = podcaster.Id,
                        FullName = podcaster.PodcasterProfileName,
                        Email = podcaster.Email,
                        MainImageFileKey = podcaster.MainImageFileKey
                    },
                    CreatedAt = channel.CreatedAt,
                    UpdatedAt = channel.UpdatedAt,
                    ShowList = showList.Select(ps => new ShowListItemResponseDTO
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
                        TakenDownReason = ps.TakenDownReason,
                        EpisodeCount = ps.PodcastEpisodes != null ? ps.PodcastEpisodes.Count(pe => pe.DeletedAt == null) : 0,
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
                        PodcastChannel = new PodcastChannelSnippetResponseDTO
                        {
                            Id = channel.Id,
                            Name = channel.Name,
                            Description = channel.Description,
                            MainImageFileKey = channel.MainImageFileKey
                        },
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
                    }).ToList()

                };
                return channelDetail;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new HttpRequestException("Get channel by id failed, error: " + ex.Message);
            }
        }


        public async Task CreatePodcastChannel(CreateChannelParameterDTO createChannelParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var podcastChannel = new PodcastChannel
                    {
                        Name = createChannelParameterDTO.Name,
                        Description = createChannelParameterDTO.Description,
                        PodcasterId = createChannelParameterDTO.PodcasterId,
                        PodcastCategoryId = createChannelParameterDTO.PodcastCategoryId,
                        PodcastSubCategoryId = createChannelParameterDTO.PodcastSubCategoryId,
                    };

                    var existingPodcaster = await _accountCachingService.GetAccountStatusCacheById(podcastChannel.PodcasterId);
                    if (existingPodcaster == null || existingPodcaster.Id != podcastChannel.PodcasterId || existingPodcaster.IsVerified == false || existingPodcaster.DeactivatedAt != null || existingPodcaster.HasVerifiedPodcasterProfile == false)
                    {
                        throw new Exception("Podcaster with id " + podcastChannel.PodcasterId + " does not exist");
                    }

                    await _podcastChannelGenericRepository.CreateAsync(podcastChannel);

                    var newPodcastChannelStatusTracking = new PodcastChannelStatusTracking
                    {
                        PodcastChannelId = podcastChannel.Id,
                        PodcastChannelStatusId = (int)PodcastChannelStatusEnum.Unpublished, // setting to "Unpublished" status
                    };
                    await _podcastChannelStatusTrackingGenericRepository.CreateAsync(newPodcastChannelStatusTracking);


                    var folderPath = _filePathConfig.PODCAST_CHANNEL_FILE_PATH + "\\" + podcastChannel.Id;
                    if (createChannelParameterDTO.MainImageFileKey != null && createChannelParameterDTO.MainImageFileKey != "")
                    {
                        var MainImageFileKey = FilePathHelper.CombinePaths(folderPath, $"main_image{FilePathHelper.GetExtension(createChannelParameterDTO.MainImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(createChannelParameterDTO.MainImageFileKey, MainImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(createChannelParameterDTO.MainImageFileKey);
                        podcastChannel.MainImageFileKey = MainImageFileKey;

                    }
                    if (createChannelParameterDTO.BackgroundImageFileKey != null && createChannelParameterDTO.BackgroundImageFileKey != "")
                    {
                        var BackgroundImageFileKey = FilePathHelper.CombinePaths(folderPath, $"background_image{FilePathHelper.GetExtension(createChannelParameterDTO.BackgroundImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(createChannelParameterDTO.BackgroundImageFileKey, BackgroundImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(createChannelParameterDTO.BackgroundImageFileKey);
                        podcastChannel.BackgroundImageFileKey = BackgroundImageFileKey;
                    }

                    await _podcastChannelGenericRepository.UpdateAsync(podcastChannel.Id, podcastChannel);

                    foreach (var hashtagId in createChannelParameterDTO.HashtagIds)
                    {
                        var existingHashtag = await _hashtagGenericRepository.FindByIdAsync(hashtagId);
                        if (existingHashtag == null)
                        {
                            throw new Exception("Hashtag with id " + hashtagId + " does not exist");
                        }
                        var podcastChannelHashtag = new PodcastChannelHashtag
                        {
                            PodcastChannelId = podcastChannel.Id,
                            HashtagId = hashtagId
                        };
                        await _podcastChannelHashtagGenericRepository.CreateAsync(podcastChannelHashtag);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["Name"] = podcastChannel.Name;
                    messageNextRequestData["Description"] = podcastChannel.Description;
                    messageNextRequestData["MainImageFileKey"] = podcastChannel.MainImageFileKey;
                    messageNextRequestData["BackgroundImageFileKey"] = podcastChannel.BackgroundImageFileKey;
                    messageNextRequestData["PodcastCategoryId"] = podcastChannel.PodcastCategoryId;
                    messageNextRequestData["PodcastSubCategoryId"] = podcastChannel.PodcastSubCategoryId;
                    messageNextRequestData["HashtagIds"] = JArray.FromObject(createChannelParameterDTO.HashtagIds);
                    messageNextRequestData["PodcasterId"] = podcastChannel.PodcasterId;

                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = podcastChannel.Id,
                        Name = podcastChannel.Name,
                        Description = podcastChannel.Description,
                        MainImageFileKey = podcastChannel.MainImageFileKey,
                        BackgroundImageFileKey = podcastChannel.BackgroundImageFileKey,
                        PodcastCategoryId = podcastChannel.PodcastCategoryId,
                        PodcastSubCategoryId = podcastChannel.PodcastSubCategoryId,
                        HashtagIds = createChannelParameterDTO.HashtagIds,
                        PodcasterId = podcastChannel.PodcasterId
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-channel.success"
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
                            ErrorMessage = $"Create podcast channel failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "create-channel.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }


        public async Task UpdatePodcastChannel(UpdateChannelParameterDTO updateChannelParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var podcastChannel = await _podcastChannelGenericRepository.FindByIdAsync(updateChannelParameterDTO.PodcastChannelId);
                    if (podcastChannel == null)
                    {
                        throw new Exception("Podcast channel with id " + updateChannelParameterDTO.PodcastChannelId + " does not exist");
                    }
                    else if (podcastChannel.DeletedAt != null)
                    {
                        throw new Exception("Podcast channel with id " + updateChannelParameterDTO.PodcastChannelId + " has been deleted");
                    }
                    else if (podcastChannel.PodcasterId != updateChannelParameterDTO.PodcasterId)
                    {
                        throw new Exception("Podcast channel with id " + updateChannelParameterDTO.PodcastChannelId + " does not belong to podcaster with id " + updateChannelParameterDTO.PodcasterId);
                    }

                    podcastChannel.Name = updateChannelParameterDTO.Name;
                    podcastChannel.Description = updateChannelParameterDTO.Description;
                    podcastChannel.PodcastCategoryId = updateChannelParameterDTO.PodcastCategoryId;
                    podcastChannel.PodcastSubCategoryId = updateChannelParameterDTO.PodcastSubCategoryId;

                    var folderPath = _filePathConfig.PODCAST_CHANNEL_FILE_PATH + "\\" + podcastChannel.Id;
                    if (updateChannelParameterDTO.MainImageFileKey != null && updateChannelParameterDTO.MainImageFileKey != "")
                    {
                        if (!string.IsNullOrEmpty(podcastChannel?.MainImageFileKey))
                        {
                            await _fileIOHelper.DeleteFileAsync(podcastChannel.MainImageFileKey);
                        }
                        var MainImageFileKey = FilePathHelper.CombinePaths(folderPath, $"main_image{FilePathHelper.GetExtension(updateChannelParameterDTO.MainImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(updateChannelParameterDTO.MainImageFileKey, MainImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(updateChannelParameterDTO.MainImageFileKey);
                        podcastChannel.MainImageFileKey = MainImageFileKey;

                    }
                    if (updateChannelParameterDTO.BackgroundImageFileKey != null && updateChannelParameterDTO.BackgroundImageFileKey != "")
                    {
                        if (!string.IsNullOrEmpty(podcastChannel?.BackgroundImageFileKey))
                        {
                            await _fileIOHelper.DeleteFileAsync(podcastChannel.BackgroundImageFileKey);
                        }
                        var BackgroundImageFileKey = FilePathHelper.CombinePaths(folderPath, $"background_image{FilePathHelper.GetExtension(updateChannelParameterDTO.BackgroundImageFileKey)}");
                        await _fileIOHelper.CopyFileToFileAsync(updateChannelParameterDTO.BackgroundImageFileKey, BackgroundImageFileKey);
                        await _fileIOHelper.DeleteFileAsync(updateChannelParameterDTO.BackgroundImageFileKey);
                        podcastChannel.BackgroundImageFileKey = BackgroundImageFileKey;
                    }

                    await _podcastChannelGenericRepository.UpdateAsync(podcastChannel.Id, podcastChannel);

                    // Update hashtags
                    await _unitOfWork.PodcastChannelHashtagRepository.DeleteByPodcastChannelIdAsync(podcastChannel.Id);

                    foreach (var hashtagId in updateChannelParameterDTO.HashtagIds)
                    {
                        var existingHashtag = await _hashtagGenericRepository.FindByIdAsync(hashtagId);
                        if (existingHashtag == null)
                        {
                            throw new Exception("Hashtag with id " + hashtagId + " does not exist");
                        }
                        var podcastChannelHashtag = new PodcastChannelHashtag
                        {
                            PodcastChannelId = podcastChannel.Id,
                            HashtagId = hashtagId
                        };
                        await _podcastChannelHashtagGenericRepository.CreateAsync(podcastChannelHashtag);
                    }
                    await transaction.CommitAsync();
                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["Name"] = podcastChannel.Name;
                    messageNextRequestData["Description"] = podcastChannel.Description;
                    messageNextRequestData["MainImageFileKey"] = podcastChannel.MainImageFileKey;
                    messageNextRequestData["BackgroundImageFileKey"] = podcastChannel.BackgroundImageFileKey;
                    messageNextRequestData["PodcastCategoryId"] = podcastChannel.PodcastCategoryId;
                    messageNextRequestData["PodcastSubCategoryId"] = podcastChannel.PodcastSubCategoryId;
                    messageNextRequestData["HashtagIds"] = JArray.FromObject(updateChannelParameterDTO.HashtagIds);
                    messageNextRequestData["PodcasterId"] = podcastChannel.PodcasterId;
                    messageNextRequestData["PodcastChannelId"] = podcastChannel.Id;

                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = podcastChannel.Id,
                        Name = podcastChannel.Name,
                        Description = podcastChannel.Description,
                        MainImageFileKey = podcastChannel.MainImageFileKey,
                        BackgroundImageFileKey = podcastChannel.BackgroundImageFileKey,
                        PodcastCategoryId = podcastChannel.PodcastCategoryId,
                        PodcastSubCategoryId = podcastChannel.PodcastSubCategoryId,
                        HashtagIds = updateChannelParameterDTO.HashtagIds,
                        PodcasterId = podcastChannel.PodcasterId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-channel.success"
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
                            ErrorMessage = $"Update podcast channel failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "update-channel.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task PublishPodcastChannel(PublishChannelParameterDTO publishChannelParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var podcastChannel = await _podcastChannelGenericRepository.FindByIdAsync(publishChannelParameterDTO.PodcastChannelId);
                    if (podcastChannel == null)
                    {
                        throw new Exception("Podcast channel with id " + publishChannelParameterDTO.PodcastChannelId + " does not exist");
                    }
                    else if (podcastChannel.DeletedAt != null)
                    {
                        throw new Exception("Podcast channel with id " + publishChannelParameterDTO.PodcastChannelId + " has been deleted");
                    }
                    else if (podcastChannel.PodcasterId != publishChannelParameterDTO.PodcasterId)
                    {
                        throw new Exception("Podcast channel with id " + publishChannelParameterDTO.PodcastChannelId + " does not belong to podcaster with id " + publishChannelParameterDTO.PodcasterId);
                    }

                    var newPodcastChannelStatusTracking = new PodcastChannelStatusTracking
                    {
                        PodcastChannelId = podcastChannel.Id,
                        PodcastChannelStatusId = (int)PodcastChannelStatusEnum.Published, // setting to "Published" status
                    };
                    await _podcastChannelStatusTrackingGenericRepository.CreateAsync(newPodcastChannelStatusTracking);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = podcastChannel.Id;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = podcastChannel.Id,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "publish-channel.success"
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
                            ErrorMessage = $"Publish podcast channel failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "publish-channel.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);

                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }

        }

        public async Task PlusPodcastChannelTotalFavorite(PlusChannelTotalFavoriteParameterDTO plusChannelTotalFavoriteParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var podcastChannel = await _podcastChannelGenericRepository.FindByIdAsync(plusChannelTotalFavoriteParameterDTO.PodcastChannelId);
                    if (podcastChannel == null)
                    {
                        throw new Exception("Podcast channel with id " + plusChannelTotalFavoriteParameterDTO.PodcastChannelId + " does not exist");
                    }
                    else if (podcastChannel.DeletedAt != null)
                    {
                        throw new Exception("Podcast channel with id " + plusChannelTotalFavoriteParameterDTO.PodcastChannelId + " has been deleted");
                    }

                    podcastChannel.TotalFavorite += 1;
                    await _podcastChannelGenericRepository.UpdateAsync(podcastChannel.Id, podcastChannel);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = podcastChannel.Id;
                    messageNextRequestData["AccountId"] = command.RequestData["AccountId"];
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = podcastChannel.Id,
                        AccountId = command.RequestData["AccountId"],
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "plus-channel-total-favorite.success"
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
                            ErrorMessage = $"Plus podcast channel total favorite failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "plus-channel-total-favorite.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task SubtractPodcastChannelTotalFavorite(SubtractChannelTotalFavoriteParameterDTO subtractChannelTotalFavoriteParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var podcastChannel = await _podcastChannelGenericRepository.FindByIdAsync(subtractChannelTotalFavoriteParameterDTO.PodcastChannelId);
                    if (podcastChannel == null)
                    {
                        throw new Exception("Podcast channel with id " + subtractChannelTotalFavoriteParameterDTO.PodcastChannelId + " does not exist");
                    }
                    else if (podcastChannel.DeletedAt != null)
                    {
                        throw new Exception("Podcast channel with id " + subtractChannelTotalFavoriteParameterDTO.PodcastChannelId + " has been deleted");
                    }

                    podcastChannel.TotalFavorite = Math.Max(0, podcastChannel.TotalFavorite - subtractChannelTotalFavoriteParameterDTO.AffectedAccountIds.Count);
                    await _podcastChannelGenericRepository.UpdateAsync(podcastChannel.Id, podcastChannel);

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = podcastChannel.Id;
                    messageNextRequestData["AccountId"] = command.RequestData["AccountId"];
                    messageNextRequestData["AffectedAccountIds"] = JArray.FromObject(subtractChannelTotalFavoriteParameterDTO.AffectedAccountIds);
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = podcastChannel.Id,
                        AccountId = command.RequestData["AccountId"],
                        AffectedAccountIds = subtractChannelTotalFavoriteParameterDTO.AffectedAccountIds
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "subtract-channel-total-favorite.success"
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
                            ErrorMessage = $"Subtract podcast channel total favorite failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "subtract-channel-total-favorite.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task UnpublishChannelUnpublishChannelForce(UnpublishChannelUnpublishChannelForceParameterDTO unpublishChannelUnpublishChannelForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // unpublish episode
                    var channel = await _podcastChannelGenericRepository.FindByIdAsync(
                        id: unpublishChannelUnpublishChannelForceParameterDTO.PodcastChannelId,
                        includeFunc: pe => pe.Include(pe => pe.PodcastChannelStatusTrackings)
                    );
                    Console.WriteLine("Channel found: " + (channel != null));

                    if (channel != null)
                    {
                        Console.WriteLine("Channel Status Trackings count: " + channel.Name + " - " + channel.PodcastChannelStatusTrackings.Count);
                        var currentStatusTracking = channel.PodcastChannelStatusTrackings
                                                .OrderByDescending(pet => pet.CreatedAt)
                                                .FirstOrDefault();

                        if (currentStatusTracking.PodcastChannelStatusId != (int)PodcastChannelStatusEnum.Unpublished)
                        {
                            var newStatusTracking = new PodcastChannelStatusTracking
                            {
                                PodcastChannelId = channel.Id,
                                PodcastChannelStatusId = (int)PodcastChannelStatusEnum.Unpublished,
                            };

                            await _podcastChannelStatusTrackingGenericRepository.CreateAsync(newStatusTracking);

                            channel.TotalFavorite = 0;
                            channel.ListenCount = 0;
                            await _podcastChannelGenericRepository.UpdateAsync(channel.Id, channel);
                        }
                    }


                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = unpublishChannelUnpublishChannelForceParameterDTO.PodcastChannelId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = unpublishChannelUnpublishChannelForceParameterDTO.PodcastChannelId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "unpublish-channel-unpublish-channel-force.success"
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
                            ErrorMessage = $"Unpublish channel unpublish channel force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "unpublish-channel-unpublish-channel-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task UnpublishPodcasterChannelsTerminatePodcasterForce(UnpublishPodcasterChannelsTerminatePodcasterForceParameterDTO unpublishPodcasterChannelsTerminatePodcasterForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var channels = await _podcastChannelGenericRepository.FindAll(
                        predicate: pc => pc.PodcasterId == unpublishPodcasterChannelsTerminatePodcasterForceParameterDTO.PodcasterId
                                    && pc.DeletedAt == null,
                        includeFunc: pc => pc.Include(pc => pc.PodcastChannelStatusTrackings)
                    ).ToListAsync();

                    foreach (var channel in channels)
                    {
                        var currentStatusTracking = channel.PodcastChannelStatusTrackings
                                                .OrderByDescending(pet => pet.CreatedAt)
                                                .FirstOrDefault();

                        if (currentStatusTracking.PodcastChannelStatusId != (int)PodcastChannelStatusEnum.Unpublished)
                        {
                            var newStatusTracking = new PodcastChannelStatusTracking
                            {
                                PodcastChannelId = channel.Id,
                                PodcastChannelStatusId = (int)PodcastChannelStatusEnum.Unpublished,
                            };

                            await _podcastChannelStatusTrackingGenericRepository.CreateAsync(newStatusTracking);

                            channel.TotalFavorite = 0;
                            channel.ListenCount = 0;
                            await _podcastChannelGenericRepository.UpdateAsync(channel.Id, channel);
                        }
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcasterId"] = unpublishPodcasterChannelsTerminatePodcasterForceParameterDTO.PodcasterId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcasterId = unpublishPodcasterChannelsTerminatePodcasterForceParameterDTO.PodcasterId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "unpublish-podcaster-channels-terminate-podcaster-force.success"
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
                            ErrorMessage = $"Unpublish podcaster channels terminate podcaster force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "unpublish-podcaster-channels-terminate-podcaster-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }

        public async Task DeleteChannelChannelDeletionForce(DeleteChannelChannelDeletionForceParameterDTO deleteChannelChannelDeletionForceParameterDTO, SagaCommandMessage command)
        {
            using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var channel = await _podcastChannelGenericRepository.FindByIdAsync(
                        id: deleteChannelChannelDeletionForceParameterDTO.PodcastChannelId
                    );

                    if (channel != null)
                    {
                        channel.DeletedAt = _dateHelper.GetNowByAppTimeZone();
                        await _podcastChannelGenericRepository.UpdateAsync(channel.Id, channel);
                    }

                    await transaction.CommitAsync();

                    var messageNextRequestData = command.RequestData;
                    messageNextRequestData["PodcastChannelId"] = deleteChannelChannelDeletionForceParameterDTO.PodcastChannelId;
                    var messageResponseData = JObject.FromObject(new
                    {
                        PodcastChannelId = deleteChannelChannelDeletionForceParameterDTO.PodcastChannelId,
                    });
                    var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
                        topic: KafkaTopicEnum.ContentManagementDomain,
                        requestData: messageNextRequestData,
                        responseData: messageResponseData,
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-channel-channel-deletion-force.success"
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
                            ErrorMessage = $"Delete channel channel deletion force failed, error: {ex.Message}"
                        }),
                        sagaInstanceId: command.SagaInstanceId,
                        flowName: command.FlowName,
                        messageName: "delete-channel-channel-deletion-force.failed"
                    );
                    await _messagingService.SendSagaMessageAsync(sagaEventMessage);
                    Console.WriteLine("\n" + ex.StackTrace + "\n");
                }
            }
        }
    }
}