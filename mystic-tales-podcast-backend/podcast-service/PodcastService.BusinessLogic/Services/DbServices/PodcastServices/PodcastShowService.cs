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
using PodcastService.BusinessLogic.DTOs.Cachegory;
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
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateShow;
using PodcastService.BusinessLogic.DTOs.Show.Details;
using PodcastService.BusinessLogic.DTOs.Episode.ListItems;
using PodcastService.BusinessLogic.DTOs.Episode;
using PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateShow;

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


        /////////////////////////////////////////////////////////////

        #region Sample coding format must be followed


        // public async Task<List<ChannelListItemResponseDTO>> GetChannelByPodcasterIdAsync(int podcasterId)
        // {
        //     // tuân thủ cách viết ở trên
        //     try
        //     {
        //         var query = _podcastChannelGenericRepository.FindAll(
        //             predicate: c => c.DeletedAt == null && c.PodcasterId == podcasterId,
        //             includeFunc: q => q
        //                 .Include(pc => pc.PodcastCategory)
        //                 .Include(pc => pc.PodcastSubCategory)
        //                 .Include(pc => pc.PodcastChannelStatusTrackings)
        //                 .ThenInclude(pct => pct.PodcastChannelStatus)
        //                 .Include(pc => pc.PodcastChannelHashtags)
        //                 .ThenInclude(pch => pch.Hashtag)
        //                 .Include(pc => pc.PodcastShows)
        //                 .ThenInclude(ps => ps.PodcastShowStatusTrackings)
        //         );

        //         var channels = await query.ToListAsync();

        //         var channelList = (await Task.WhenAll(channels.Select(async pc =>
        //         {
        //             var podcaster = await _accountCachingService.GetAccountStatusCacheById(pc.PodcasterId);
        //             if (podcaster == null || podcaster.Id != pc.PodcasterId || podcaster.IsVerified == false || podcaster.HasVerifiedPodcasterProfile == false)
        //             {
        //                 throw new Exception("Podcaster with id " + pc.PodcasterId + " does not exist");
        //             }
        //             return new ChannelListItemResponseDTO
        //             {
        //                 Id = pc.Id,
        //                 Name = pc.Name,
        //                 Description = pc.Description,
        //                 MainImageFileKey = pc.MainImageFileKey,
        //                 BackgroundImageFileKey = pc.BackgroundImageFileKey,
        //                 PodcastCategory = new PodcastCategoryDTO
        //                 {
        //                     Id = pc.PodcastCategory.Id,
        //                     Name = pc.PodcastCategory.Name
        //                 },
        //                 PodcastSubCategory = new PodcastSubCategoryDTO
        //                 {
        //                     Id = pc.PodcastSubCategory.Id,
        //                     Name = pc.PodcastSubCategory.Name,
        //                     PodcastCategoryId = pc.PodcastSubCategory.PodcastCategoryId
        //                 },
        //                 CurrentStatus = pc.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).Select(pct => new PodcastChannelStatusDTO
        //                 {
        //                     Id = pct.PodcastChannelStatus.Id,
        //                     Name = pct.PodcastChannelStatus.Name
        //                 }).FirstOrDefault()!,
        //                 Hashtags = pc.PodcastChannelHashtags.Select(pch => new HashtagDTO
        //                 {
        //                     Id = pch.Hashtag.Id,
        //                     Name = pch.Hashtag.Name
        //                 }).ToList(),
        //                 TotalFavorite = pc.TotalFavorite,
        //                 ListenCount = pc.ListenCount,
        //                 ShowCount = pc.PodcastShows != null ? pc.PodcastShows.Count(ps => ps.DeletedAt == null) : 0,
        //                 Podcaster = new AccountSnippetResponseDTO
        //                 {
        //                     Id = podcaster.Id,
        //                     FullName = podcaster.FullName,
        //                     Email = podcaster.Email,
        //                     MainImageFileKey = podcaster.MainImageFileKey
        //                 },
        //                 CreatedAt = pc.CreatedAt,
        //                 UpdatedAt = pc.UpdatedAt
        //             };
        //         }))

        //         ).ToList();
        //         return channelList;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         throw new HttpRequestException("Get channels failed, error: " + ex.Message);
        //     }
        // }

        // public async Task<ChannelDetailResponseDTO> GetChannelByIdAsync(Guid channelId, int? role)
        // {
        //     try
        //     {
        //         var query = _podcastChannelGenericRepository.FindAll(
        //             predicate: c => c.DeletedAt == null && c.Id == channelId,
        //             includeFunc: q => q
        //                 .Include(pc => pc.PodcastCategory)
        //                 .Include(pc => pc.PodcastSubCategory)
        //                 .Include(pc => pc.PodcastChannelStatusTrackings)
        //                 .ThenInclude(pct => pct.PodcastChannelStatus)
        //                 .Include(pc => pc.PodcastChannelHashtags)
        //                 .ThenInclude(pch => pch.Hashtag)
        //                 .Include(pc => pc.PodcastShows)
        //                 .ThenInclude(ps => ps.PodcastShowStatusTrackings)
        //         );

        //         if (role == null || role == 1)
        //         {
        //             query = query.Where(pc => pc.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).FirstOrDefault().PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published);
        //         }

        //         var channel = await query.FirstOrDefaultAsync();

        //         if (channel == null)
        //         {
        //             throw new Exception("Channel with id " + channelId + " does not exist");
        //         }

        //         var podcastSubscriptionBatchRequest = new BatchQueryRequest
        //         {
        //             Queries = new List<BatchQueryItem>
        //             {
        //                 new BatchQueryItem
        //                 {
        //                     Key = "podcastSubscriptionList",
        //                     QueryType = "findall",
        //                     EntityType = "PodcastSubscription",
        //                     Parameters = JObject.FromObject(new
        //                     {
        //                         where = (role == null || role == 1) ? new
        //                         {
        //                             IsActive = (bool?)true,
        //                             DeletedAt = (DateTime?)null,
        //                             PodcastChannelId = channel.Id,
        //                         } : new {
        //                             IsActive = (bool?)null,
        //                             DeletedAt = (DateTime?)null,
        //                             PodcastChannelId = channel.Id,
        //                         },
        //                         include = "PodcastSubscriptionBenefitMappings.PodcastSubscriptionBenefit , PodcastSubscriptionCycleTypePrices.SubscriptionCycleType"
        //                     }),
        //                 }
        //             }
        //         };

        //         var result = await _httpServiceQueryClient.ExecuteBatchAsync("SubscriptionService", podcastSubscriptionBatchRequest);


        //         var showByChannelIdQuery = _podcastShowGenericRepository.FindAll(
        //             predicate: ps => ps.DeletedAt == null && ps.PodcastChannelId == channel.Id,
        //             includeFunc: q => q
        //                 .Include(ps => ps.PodcastShowStatusTrackings)
        //                 .ThenInclude(pst => pst.PodcastShowStatus)
        //                 .Include(ps => ps.PodcastCategory)
        //                 .Include(ps => ps.PodcastSubCategory)
        //                 .Include(ps => ps.PodcastShowHashtags)
        //                 .ThenInclude(psh => psh.Hashtag)
        //                 .Include(ps => ps.PodcastShowSubscriptionType)
        //         );

        //         if (role == null || role == 1)
        //         {
        //             showByChannelIdQuery = showByChannelIdQuery.Where(ps => ps.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnums.Published && ps.IsReleased == true);
        //         }
        //         var showList = await showByChannelIdQuery.ToListAsync();

        //         var podcaster = await _accountCachingService.GetAccountStatusCacheById(channel.PodcasterId);
        //         if (podcaster == null || podcaster.Id != channel.PodcasterId || podcaster.IsVerified == false || podcaster.HasVerifiedPodcasterProfile == false)
        //         {
        //             throw new Exception("Podcaster with id " + channel.PodcasterId + " does not exist");
        //         }

        //         var channelDetail = new ChannelDetailResponseDTO
        //         {
        //             Id = channel.Id,
        //             Name = channel.Name,
        //             Description = channel.Description,
        //             MainImageFileKey = channel.MainImageFileKey,
        //             BackgroundImageFileKey = channel.BackgroundImageFileKey,
        //             PodcastCategory = new PodcastCategoryDTO
        //             {
        //                 Id = channel.PodcastCategory.Id,
        //                 Name = channel.PodcastCategory.Name
        //             },
        //             PodcastSubCategory = new PodcastSubCategoryDTO
        //             {
        //                 Id = channel.PodcastSubCategory.Id,
        //                 Name = channel.PodcastSubCategory.Name,
        //                 PodcastCategoryId = channel.PodcastSubCategory.PodcastCategoryId
        //             },
        //             CurrentStatus = channel.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).Select(pct => new PodcastChannelStatusDTO
        //             {
        //                 Id = pct.PodcastChannelStatus.Id,
        //                 Name = pct.PodcastChannelStatus.Name
        //             }).FirstOrDefault()!,
        //             Hashtags = channel.PodcastChannelHashtags.Select(pch => new HashtagDTO
        //             {
        //                 Id = pch.Hashtag.Id,
        //                 Name = pch.Hashtag.Name
        //             }).ToList(),
        //             TotalFavorite = channel.TotalFavorite,
        //             ListenCount = channel.ListenCount,
        //             ShowCount = showList.Count,
        //             Podcaster = new AccountSnippetResponseDTO
        //             {
        //                 Id = podcaster.Id,
        //                 FullName = podcaster.FullName,
        //                 Email = podcaster.Email,
        //                 MainImageFileKey = podcaster.MainImageFileKey
        //             },
        //             CreatedAt = channel.CreatedAt,
        //             UpdatedAt = channel.UpdatedAt,
        //             ShowList = showList.Select(ps => new ShowListItemResponseDTO
        //             {
        //                 Id = ps.Id,
        //                 Name = ps.Name,
        //                 Description = ps.Description,
        //                 MainImageFileKey = ps.MainImageFileKey,
        //                 TrailerAudioFileKey = ps.TrailerAudioFileKey,
        //                 TotalFollow = ps.TotalFollow,
        //                 ListenCount = ps.ListenCount,
        //                 AverageRating = ps.AverageRating,
        //                 RatingCount = ps.RatingCount,
        //                 Copyright = ps.Copyright,
        //                 IsReleased = ps.IsReleased,
        //                 Language = ps.Language,
        //                 UploadFrequency = ps.UploadFrequency,
        //                 ReleaseDate = ps.ReleaseDate,
        //                 TakenDownReason = role == null || role == 1 ? null : ps.TakenDownReason,
        //                 PodcastCategory = new PodcastCategoryDTO
        //                 {
        //                     Id = ps.PodcastCategory.Id,
        //                     Name = ps.PodcastCategory.Name
        //                 },
        //                 PodcastSubCategory = new PodcastSubCategoryDTO
        //                 {
        //                     Id = ps.PodcastSubCategory.Id,
        //                     Name = ps.PodcastSubCategory.Name,
        //                     PodcastCategoryId = ps.PodcastSubCategory.PodcastCategoryId
        //                 },
        //                 PodcastChannel = new PodcastChannelSnippetResponseDTO
        //                 {
        //                     Id = channel.Id,
        //                     Name = channel.Name,
        //                     MainImageFileKey = channel.MainImageFileKey
        //                 },
        //                 Podcaster = new AccountSnippetResponseDTO
        //                 {
        //                     Id = podcaster.Id,
        //                     Email = podcaster.Email,
        //                     FullName = podcaster.FullName,
        //                     MainImageFileKey = podcaster.MainImageFileKey
        //                 },
        //                 PodcastShowSubscriptionType = ps.PodcastShowSubscriptionType != null ? new PodcastShowSubscriptionTypeDTO
        //                 {
        //                     Id = ps.PodcastShowSubscriptionType.Id,
        //                     Name = ps.PodcastShowSubscriptionType.Name
        //                 } : null,
        //                 Hashtags = ps.PodcastShowHashtags.Select(psh => new HashtagDTO
        //                 {
        //                     Id = psh.Hashtag.Id,
        //                     Name = psh.Hashtag.Name
        //                 }).ToList(),
        //                 CreatedAt = ps.CreatedAt,
        //                 UpdatedAt = ps.UpdatedAt,
        //                 CurrentStatus = ps.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).Select(pst => new PodcastShowStatusDTO
        //                 {
        //                     Id = pst.PodcastShowStatus.Id,
        //                     Name = pst.PodcastShowStatus.Name
        //                 }).FirstOrDefault()!,
        //             }).ToList(),
        //             PodcastSubscriptionList = ((JArray)result.Results["podcastSubscriptionList"]).Select(ps =>
        //             {
        //                 var psObj = ps.ToObject<PodcastSubscriptionListItemResponseDTO>();
        //                 return new PodcastSubscriptionListItemResponseDTO
        //                 {
        //                     Id = psObj.Id,
        //                     Name = psObj.Name,
        //                     Description = psObj.Description,
        //                     CurrentVersion = psObj.CurrentVersion,
        //                     PodcastShowId = psObj.PodcastShowId,
        //                     IsActive = psObj.IsActive,
        //                     CreatedAt = psObj.CreatedAt,
        //                     UpdatedAt = psObj.UpdatedAt,
        //                     PodcastChannelId = psObj.PodcastChannelId,
        //                     PodcastSubscriptionCycleTypePriceList = ((JArray)ps["PodcastSubscriptionCycleTypePrices"]).ToObject<List<PodcastSubscriptionCycleTypePriceListItemResponseDTO>>().Select(psctp => new PodcastSubscriptionCycleTypePriceListItemResponseDTO
        //                     {
        //                         PodcastSubscriptionId = psctp.PodcastSubscriptionId,
        //                         Price = psctp.Price,
        //                         Version = psctp.Version,
        //                         CreatedAt = psctp.CreatedAt,
        //                         UpdatedAt = psctp.UpdatedAt,
        //                         SubscriptionCycleType = psctp.SubscriptionCycleType != null ? new SubscriptionCycleTypeDTO
        //                         {
        //                             Id = psctp.SubscriptionCycleType.Id,
        //                             Name = psctp.SubscriptionCycleType.Name,
        //                         } : null
        //                     }).ToList(),
        //                     DeletedAt = psObj.DeletedAt,
        //                     PodcastSubscriptionBenefitMappingList = ((JArray)ps["PodcastSubscriptionBenefitMappings"]).ToObject<List<PodcastSubscriptionBenefitMappingListItemResponseDTO>>().Select(psbm => new PodcastSubscriptionBenefitMappingListItemResponseDTO
        //                     {
        //                         PodcastSubscriptionId = psbm.PodcastSubscriptionId,
        //                         Version = psbm.Version,
        //                         CreatedAt = psbm.CreatedAt,
        //                         UpdatedAt = psbm.UpdatedAt,
        //                         PodcastSubscriptionBenefit = psbm.PodcastSubscriptionBenefit != null ? new PodcastSubscriptionBenefitDTO
        //                         {
        //                             Id = psbm.PodcastSubscriptionBenefit.Id,
        //                             Name = psbm.PodcastSubscriptionBenefit.Name
        //                         } : null
        //                     }).ToList()
        //                 };
        //             }).ToList()

        //         };
        //         return channelDetail;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         throw new HttpRequestException("Get channel by id failed, error: " + ex.Message);
        //     }
        // }


        // public async Task UpdatePodcastChannel(UpdateChannelParameterDTO updateChannelParameterDTO, SagaCommandMessage command)
        // {
        //     using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             var podcastChannel = await _podcastChannelGenericRepository.FindByIdAsync(updateChannelParameterDTO.PodcastChannelId);
        //             if (podcastChannel == null)
        //             {
        //                 throw new Exception("Podcast channel with id " + updateChannelParameterDTO.PodcastChannelId + " does not exist");
        //             }
        //             else if (podcastChannel.DeletedAt != null)
        //             {
        //                 throw new Exception("Podcast channel with id " + updateChannelParameterDTO.PodcastChannelId + " has been deleted");
        //             }
        //             else if (podcastChannel.PodcasterId != updateChannelParameterDTO.PodcasterId)
        //             {
        //                 throw new Exception("Podcast channel with id " + updateChannelParameterDTO.PodcastChannelId + " does not belong to podcaster with id " + updateChannelParameterDTO.PodcasterId);
        //             }

        //             podcastChannel.Name = updateChannelParameterDTO.Name;
        //             podcastChannel.Description = updateChannelParameterDTO.Description;
        //             podcastChannel.PodcastCategoryId = updateChannelParameterDTO.PodcastCategoryId;
        //             podcastChannel.PodcastSubCategoryId = updateChannelParameterDTO.PodcastSubCategoryId;

        //             var folderPath = _filePathConfig.PODCAST_CHANNEL_FILE_PATH + "\\" + podcastChannel.Id;
        //             if (updateChannelParameterDTO.MainImageFileKey != null && updateChannelParameterDTO.MainImageFileKey != "")
        //             {
        //                 if (!string.IsNullOrEmpty(podcastChannel?.MainImageFileKey))
        //                 {
        //                     await _fileIOHelper.DeleteFileAsync(podcastChannel.MainImageFileKey);
        //                 }
        //                 var MainImageFileKey = FilePathHelper.CombinePaths(folderPath, $"main_image{FilePathHelper.GetExtension(updateChannelParameterDTO.MainImageFileKey)}");
        //                 await _fileIOHelper.CopyFileToFileAsync(updateChannelParameterDTO.MainImageFileKey, MainImageFileKey);
        //                 await _fileIOHelper.DeleteFileAsync(updateChannelParameterDTO.MainImageFileKey);
        //                 podcastChannel.MainImageFileKey = MainImageFileKey;

        //             }
        //             if (updateChannelParameterDTO.BackgroundImageFileKey != null && updateChannelParameterDTO.BackgroundImageFileKey != "")
        //             {
        //                 if (!string.IsNullOrEmpty(podcastChannel?.BackgroundImageFileKey))
        //                 {
        //                     await _fileIOHelper.DeleteFileAsync(podcastChannel.BackgroundImageFileKey);
        //                 }
        //                 var BackgroundImageFileKey = FilePathHelper.CombinePaths(folderPath, $"background_image{FilePathHelper.GetExtension(updateChannelParameterDTO.BackgroundImageFileKey)}");
        //                 await _fileIOHelper.CopyFileToFileAsync(updateChannelParameterDTO.BackgroundImageFileKey, BackgroundImageFileKey);
        //                 await _fileIOHelper.DeleteFileAsync(updateChannelParameterDTO.BackgroundImageFileKey);
        //                 podcastChannel.BackgroundImageFileKey = BackgroundImageFileKey;
        //             }

        //             await _podcastChannelGenericRepository.UpdateAsync(podcastChannel.Id, podcastChannel);

        //             // Update hashtags
        //             await _unitOfWork.PodcastChannelHashtagRepository.DeleteByPodcastChannelIdAsync(podcastChannel.Id);

        //             foreach (var hashtagId in updateChannelParameterDTO.HashtagIds)
        //             {
        //                 var existingHashtag = await _hashtagGenericRepository.FindByIdAsync(hashtagId);
        //                 if (existingHashtag == null)
        //                 {
        //                     throw new Exception("Hashtag with id " + hashtagId + " does not exist");
        //                 }
        //                 var podcastChannelHashtag = new PodcastChannelHashtag
        //                 {
        //                     PodcastChannelId = podcastChannel.Id,
        //                     HashtagId = hashtagId
        //                 };
        //                 await _podcastChannelHashtagGenericRepository.CreateAsync(podcastChannelHashtag);
        //             }
        //             await transaction.CommitAsync();
        //             var messageNextRequestData = command.RequestData;
        //             messageNextRequestData["Name"] = podcastChannel.Name;
        //             messageNextRequestData["Description"] = podcastChannel.Description;
        //             messageNextRequestData["MainImageFileKey"] = podcastChannel.MainImageFileKey;
        //             messageNextRequestData["BackgroundImageFileKey"] = podcastChannel.BackgroundImageFileKey;
        //             messageNextRequestData["PodcastCategoryId"] = podcastChannel.PodcastCategoryId;
        //             messageNextRequestData["PodcastSubCategoryId"] = podcastChannel.PodcastSubCategoryId;
        //             messageNextRequestData["HashtagIds"] = JArray.FromObject(updateChannelParameterDTO.HashtagIds);
        //             messageNextRequestData["PodcasterId"] = podcastChannel.PodcasterId;
        //             messageNextRequestData["PodcastChannelId"] = podcastChannel.Id;

        //             var messageResponseData = JObject.FromObject(new
        //             {
        //                 PodcastChannelId = podcastChannel.Id,
        //                 Name = podcastChannel.Name,
        //                 Description = podcastChannel.Description,
        //                 MainImageFileKey = podcastChannel.MainImageFileKey,
        //                 BackgroundImageFileKey = podcastChannel.BackgroundImageFileKey,
        //                 PodcastCategoryId = podcastChannel.PodcastCategoryId,
        //                 PodcastSubCategoryId = podcastChannel.PodcastSubCategoryId,
        //                 HashtagIds = updateChannelParameterDTO.HashtagIds,
        //                 PodcasterId = podcastChannel.PodcasterId,
        //             });
        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.ContentManagementDomain,
        //                 requestData: messageNextRequestData,
        //                 responseData: messageResponseData,
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "update-channel.success"
        //             );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);
        //         }
        //         catch (Exception ex)
        //         {
        //             await transaction.RollbackAsync();

        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.ContentManagementDomain,
        //                 requestData: command.RequestData,
        //                 responseData: JObject.FromObject(new
        //                 {
        //                     ErrorMessage = $"Update podcast channel failed, error: {ex.Message}"
        //                 }),
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "update-channel.failed"
        //             );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);

        //             Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         }
        //     }
        // }

        // public async Task PublishPodcastChannel(PublishChannelParameterDTO publishChannelParameterDTO, SagaCommandMessage command)
        // {
        //     using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             var podcastChannel = await _podcastChannelGenericRepository.FindByIdAsync(publishChannelParameterDTO.PodcastChannelId);
        //             if (podcastChannel == null)
        //             {
        //                 throw new Exception("Podcast channel with id " + publishChannelParameterDTO.PodcastChannelId + " does not exist");
        //             }
        //             else if (podcastChannel.DeletedAt != null)
        //             {
        //                 throw new Exception("Podcast channel with id " + publishChannelParameterDTO.PodcastChannelId + " has been deleted");
        //             }
        //             else if (podcastChannel.PodcasterId != publishChannelParameterDTO.PodcasterId)
        //             {
        //                 throw new Exception("Podcast channel with id " + publishChannelParameterDTO.PodcastChannelId + " does not belong to podcaster with id " + publishChannelParameterDTO.PodcasterId);
        //             }

        //             var newPodcastChannelStatusTracking = new PodcastChannelStatusTracking
        //             {
        //                 PodcastChannelId = podcastChannel.Id,
        //                 PodcastChannelStatusId = (int)PodcastChannelStatusEnum.Published, // setting to "Published" status
        //             };
        //             await _podcastChannelStatusTrackingGenericRepository.CreateAsync(newPodcastChannelStatusTracking);

        //             await transaction.CommitAsync();

        //             var messageNextRequestData = command.RequestData;
        //             messageNextRequestData["PodcastChannelId"] = podcastChannel.Id;
        //             var messageResponseData = JObject.FromObject(new
        //             {
        //                 PodcastChannelId = podcastChannel.Id,
        //             });
        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.ContentManagementDomain,
        //                 requestData: messageNextRequestData,
        //                 responseData: messageResponseData,
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "publish-channel.success"
        //             );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);
        //         }
        //         catch (Exception ex)
        //         {
        //             await transaction.RollbackAsync();

        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.ContentManagementDomain,
        //                 requestData: command.RequestData,
        //                 responseData: JObject.FromObject(new
        //                 {
        //                     ErrorMessage = $"Publish podcast channel failed, error: {ex.Message}"
        //                 }),
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "publish-channel.failed"
        //             );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);

        //             Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         }
        //     }

        // }

        // public async Task PlusPodcastChannelTotalFavorite(PlusChannelTotalFavoriteParameterDTO plusChannelTotalFavoriteParameterDTO, SagaCommandMessage command)
        // {
        //     using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             var podcastChannel = await _podcastChannelGenericRepository.FindByIdAsync(plusChannelTotalFavoriteParameterDTO.PodcastChannelId);
        //             if (podcastChannel == null)
        //             {
        //                 throw new Exception("Podcast channel with id " + plusChannelTotalFavoriteParameterDTO.PodcastChannelId + " does not exist");
        //             }
        //             else if (podcastChannel.DeletedAt != null)
        //             {
        //                 throw new Exception("Podcast channel with id " + plusChannelTotalFavoriteParameterDTO.PodcastChannelId + " has been deleted");
        //             }

        //             podcastChannel.TotalFavorite += 1;
        //             await _podcastChannelGenericRepository.UpdateAsync(podcastChannel.Id, podcastChannel);

        //             await transaction.CommitAsync();

        //             var messageNextRequestData = command.RequestData;
        //             messageNextRequestData["PodcastChannelId"] = podcastChannel.Id;
        //             messageNextRequestData["AccountId"] = command.RequestData["AccountId"];
        //             var messageResponseData = JObject.FromObject(new
        //             {
        //                 PodcastChannelId = podcastChannel.Id,
        //                 AccountId = command.RequestData["AccountId"],
        //             });
        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.ContentManagementDomain,
        //                 requestData: messageNextRequestData,
        //                 responseData: messageResponseData,
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "plus-channel-total-favorite.success"
        //             );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);
        //         }
        //         catch (Exception ex)
        //         {
        //             await transaction.RollbackAsync();
        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.ContentManagementDomain,
        //                 requestData: command.RequestData,
        //                 responseData: JObject.FromObject(new
        //                 {
        //                     ErrorMessage = $"Plus podcast channel total favorite failed, error: {ex.Message}"
        //                 }),
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "plus-channel-total-favorite.failed"
        //             );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);
        //             Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         }
        //     }
        // }

        // public async Task SubtractPodcastChannelTotalFavorite(SubtractChannelTotalFavoriteParameterDTO subtractChannelTotalFavoriteParameterDTO, SagaCommandMessage command)
        // {
        //     using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
        //     {
        //         try
        //         {
        //             var podcastChannel = await _podcastChannelGenericRepository.FindByIdAsync(subtractChannelTotalFavoriteParameterDTO.PodcastChannelId);
        //             if (podcastChannel == null)
        //             {
        //                 throw new Exception("Podcast channel with id " + subtractChannelTotalFavoriteParameterDTO.PodcastChannelId + " does not exist");
        //             }
        //             else if (podcastChannel.DeletedAt != null)
        //             {
        //                 throw new Exception("Podcast channel with id " + subtractChannelTotalFavoriteParameterDTO.PodcastChannelId + " has been deleted");
        //             }

        //             podcastChannel.TotalFavorite = Math.Max(0, podcastChannel.TotalFavorite - 1);
        //             await _podcastChannelGenericRepository.UpdateAsync(podcastChannel.Id, podcastChannel);

        //             await transaction.CommitAsync();

        //             var messageNextRequestData = command.RequestData;
        //             messageNextRequestData["PodcastChannelId"] = podcastChannel.Id;
        //             messageNextRequestData["AccountId"] = command.RequestData["AccountId"];
        //             var messageResponseData = JObject.FromObject(new
        //             {
        //                 PodcastChannelId = podcastChannel.Id,
        //                 AccountId = command.RequestData["AccountId"],
        //             });
        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.ContentManagementDomain,
        //                 requestData: messageNextRequestData,
        //                 responseData: messageResponseData,
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "subtract-channel-total-favorite.success"
        //             );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);
        //         }
        //         catch (Exception ex)
        //         {
        //             await transaction.RollbackAsync();
        //             var sagaEventMessage = _kafkaProducerService.PrepareSagaEventMessage(
        //                 topic: KafkaTopicEnum.ContentManagementDomain,
        //                 requestData: command.RequestData,
        //                 responseData: JObject.FromObject(new
        //                 {
        //                     ErrorMessage = $"Subtract podcast channel total favorite failed, error: {ex.Message}"
        //                 }),
        //                 sagaInstanceId: command.SagaInstanceId,
        //                 flowName: command.FlowName,
        //                 messageName: "subtract-channel-total-favorite.failed"
        //             );
        //             await _messagingService.SendSagaMessageAsync(sagaEventMessage);
        //             Console.WriteLine("\n" + ex.StackTrace + "\n");
        //         }
        //     }
        // }

        #endregion

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
                        PodcastCategory = ps.PodcastCategory != null ? new PodcastCategoryDTO
                        {
                            Id = ps.PodcastCategory.Id,
                            Name = ps.PodcastCategory.Name
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
                            MainImageFileKey = ps.PodcastChannel.MainImageFileKey
                        } : null,
                        Podcaster = new AccountSnippetResponseDTO
                        {
                            Id = podcaster.Id,
                            Email = podcaster.Email,
                            FullName = podcaster.FullName,
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
                    PodcastCategory = ps.PodcastCategory != null ? new PodcastCategoryDTO
                    {
                        Id = ps.PodcastCategory.Id,
                        Name = ps.PodcastCategory.Name
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


                // var showByChannelIdQuery = _podcastShowGenericRepository.FindAll(
                //     predicate: ps => ps.DeletedAt == null && ps.PodcastChannelId == channel.Id,
                //     includeFunc: q => q
                //         .Include(ps => ps.PodcastShowStatusTrackings)
                //         .ThenInclude(pst => pst.PodcastShowStatus)
                //         .Include(ps => ps.PodcastCategory)
                //         .Include(ps => ps.PodcastSubCategory)
                //         .Include(ps => ps.PodcastShowHashtags)
                //         .ThenInclude(psh => psh.Hashtag)
                //         .Include(ps => ps.PodcastShowSubscriptionType)
                // );

                // if (role == null || role == 1)
                // {
                //     showByChannelIdQuery = showByChannelIdQuery.Where(ps => ps.PodcastShowStatusTrackings.OrderByDescending(pst => pst.CreatedAt).FirstOrDefault().PodcastShowStatusId == (int)PodcastShowStatusEnums.Published && ps.IsReleased != null);
                // }
                // var showList = await showByChannelIdQuery.ToListAsync();

                // var podcaster = await _accountCachingService.GetAccountStatusCacheById(channel.PodcasterId);
                // if (podcaster == null || podcaster.Id != channel.PodcasterId || podcaster.IsVerified == false || podcaster.HasVerifiedPodcasterProfile == false)
                // {
                //     throw new Exception("Podcaster with id " + channel.PodcasterId + " does not exist");
                // }

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

                // var channelDetail = new ChannelDetailResponseDTO
                // {
                //     Id = channel.Id,
                //     Name = channel.Name,
                //     Description = channel.Description,
                //     MainImageFileKey = channel.MainImageFileKey,
                //     BackgroundImageFileKey = channel.BackgroundImageFileKey,
                //     PodcastCategory = channel.PodcastCategory != null ? new PodcastCategoryDTO
                //     {
                //         Id = channel.PodcastCategory.Id,
                //         Name = channel.PodcastCategory.Name
                //     } : null,
                //     PodcastSubCategory = channel.PodcastSubCategory != null ? new PodcastSubCategoryDTO
                //     {
                //         Id = channel.PodcastSubCategory.Id,
                //         Name = channel.PodcastSubCategory.Name,
                //         PodcastCategoryId = channel.PodcastSubCategory.PodcastCategoryId
                //     } : null,
                //     CurrentStatus = channel.PodcastChannelStatusTrackings.OrderByDescending(pct => pct.CreatedAt).Select(pct => new PodcastChannelStatusDTO
                //     {
                //         Id = pct.PodcastChannelStatus.Id,
                //         Name = pct.PodcastChannelStatus.Name
                //     }).FirstOrDefault()!,
                //     Hashtags = channel.PodcastChannelHashtags.Select(pch => new HashtagDTO
                //     {
                //         Id = pch.Hashtag.Id,
                //         Name = pch.Hashtag.Name
                //     }).ToList(),
                //     TotalFavorite = channel.TotalFavorite,
                //     ListenCount = channel.ListenCount,
                //     ShowCount = showList.Count,
                //     Podcaster = new AccountSnippetResponseDTO
                //     {
                //         Id = podcaster.Id,
                //         FullName = podcaster.FullName,
                //         Email = podcaster.Email,
                //         MainImageFileKey = podcaster.MainImageFileKey
                //     },
                //     CreatedAt = channel.CreatedAt,
                //     UpdatedAt = channel.UpdatedAt,
                //     ShowList = showList.Select(ps => new ShowListItemResponseDTO
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
                //         TakenDownReason = role == null || role == 1 ? null : ps.TakenDownReason,
                //         PodcastCategory = ps.PodcastCategory != null ? new PodcastCategoryDTO
                //         {
                //             Id = ps.PodcastCategory.Id,
                //             Name = ps.PodcastCategory.Name
                //         } : null,
                //         PodcastSubCategory = ps.PodcastSubCategory != null ? new PodcastSubCategoryDTO
                //         {
                //             Id = ps.PodcastSubCategory.Id,
                //             Name = ps.PodcastSubCategory.Name,
                //             PodcastCategoryId = ps.PodcastSubCategory.PodcastCategoryId
                //         } : null,
                //         PodcastChannel = new PodcastChannelSnippetResponseDTO
                //         {
                //             Id = channel.Id,
                //             Name = channel.Name,
                //             MainImageFileKey = channel.MainImageFileKey
                //         },
                //         Podcaster = new AccountSnippetResponseDTO
                //         {
                //             Id = podcaster.Id,
                //             Email = podcaster.Email,
                //             FullName = podcaster.FullName,
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
                //     }).ToList(),
                //     PodcastSubscriptionList = ((JArray)result.Results["podcastSubscriptionList"]).Select(ps =>
                //     {
                //         var psObj = ps.ToObject<PodcastSubscriptionListItemResponseDTO>();
                //         return new PodcastSubscriptionListItemResponseDTO
                //         {
                //             Id = psObj.Id,
                //             Name = psObj.Name,
                //             Description = psObj.Description,
                //             CurrentVersion = psObj.CurrentVersion,
                //             PodcastShowId = psObj.PodcastShowId,
                //             IsActive = psObj.IsActive,
                //             CreatedAt = psObj.CreatedAt,
                //             UpdatedAt = psObj.UpdatedAt,
                //             PodcastChannelId = psObj.PodcastChannelId,
                //             PodcastSubscriptionCycleTypePriceList = ((JArray)ps["PodcastSubscriptionCycleTypePrices"]).ToObject<List<PodcastSubscriptionCycleTypePriceListItemResponseDTO>>().Select(psctp => new PodcastSubscriptionCycleTypePriceListItemResponseDTO
                //             {
                //                 PodcastSubscriptionId = psctp.PodcastSubscriptionId,
                //                 Price = psctp.Price,
                //                 Version = psctp.Version,
                //                 CreatedAt = psctp.CreatedAt,
                //                 UpdatedAt = psctp.UpdatedAt,
                //                 SubscriptionCycleType = psctp.SubscriptionCycleType != null ? new SubscriptionCycleTypeDTO
                //                 {
                //                     Id = psctp.SubscriptionCycleType.Id,
                //                     Name = psctp.SubscriptionCycleType.Name,
                //                 } : null
                //             }).ToList(),
                //             DeletedAt = psObj.DeletedAt,
                //             PodcastSubscriptionBenefitMappingList = ((JArray)ps["PodcastSubscriptionBenefitMappings"]).ToObject<List<PodcastSubscriptionBenefitMappingListItemResponseDTO>>().Select(psbm => new PodcastSubscriptionBenefitMappingListItemResponseDTO
                //             {
                //                 PodcastSubscriptionId = psbm.PodcastSubscriptionId,
                //                 Version = psbm.Version,
                //                 CreatedAt = psbm.CreatedAt,
                //                 UpdatedAt = psbm.UpdatedAt,
                //                 PodcastSubscriptionBenefit = psbm.PodcastSubscriptionBenefit != null ? new PodcastSubscriptionBenefitDTO
                //                 {
                //                     Id = psbm.PodcastSubscriptionBenefit.Id,
                //                     Name = psbm.PodcastSubscriptionBenefit.Name
                //                 } : null
                //             }).ToList()
                //         };
                //     }).ToList()

                // };
                // return channelDetail;
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
                    TakenDownReason = role == null || role == 1 ? null : show.TakenDownReason,
                    PodcastCategory = show.PodcastCategory != null ? new PodcastCategoryDTO
                    {
                        Id = show.PodcastCategory.Id,
                        Name = show.PodcastCategory.Name
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
                        FullName = podcaster.FullName,
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
                            MainImageFileKey = show.MainImageFileKey
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
                    var existingPodcastShow = await _podcastShowGenericRepository.FindByIdAsync(updateShowParameterDTO.PodcastShowId);
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


    }
}