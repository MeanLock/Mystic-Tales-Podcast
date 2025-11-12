using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;
using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.UOW;
using PodcastService.Infrastructure.Configurations.Payos.interfaces;
using PodcastService.BusinessLogic.Helpers.AuthHelpers;
using PodcastService.BusinessLogic.Helpers.FileHelpers;
using PodcastService.DataAccess.Repositories.interfaces;
using PodcastService.DataAccess.Entities.SqlServer;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using PodcastService.Infrastructure.Services.Redis;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.BusinessLogic.DTOs.Cache.QueryMetric;
using PodcastService.BusinessLogic.Helpers.DateHelpers;
using PodcastService.BusinessLogic.Enums.Podcast;
using System.Linq.Expressions;
using PodcastService.BusinessLogic.Models.CrossService;
using PodcastService.BusinessLogic.DTOs.Account;

namespace PodcastService.BusinessLogic.Services.DbServices.CachingServices
{
    public class QueryMetricCachingService
    {
        // LOGGER
        private readonly ILogger<QueryMetricCachingService> _logger;

        // CONFIG
        public readonly IAppConfig _appConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly IPayosConfig _payosConfig;
        private readonly IBackgroundJobsConfig _jobsConfig;

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

        private readonly RedisSharedCacheService _redisSharedCacheService;


        public QueryMetricCachingService(
            ILogger<QueryMetricCachingService> logger,
            AppDbContext appDbContext,
            BcryptHelper bcryptHelper,
            DateHelper dateHelper,
            JwtHelper jwtHelper,
            IUnitOfWork unitOfWork,


            FileIOHelper fileIOHelper,
            IFilePathConfig filePathConfig,
            IAppConfig appConfig,
            IPayosConfig payosConfig,
            IBackgroundJobsConfig jobsConfig,

            HttpServiceQueryClient httpServiceQueryClient,

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


            RedisSharedCacheService redisSharedCacheService
            )
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _bcryptHelper = bcryptHelper;
            _dateHelper = dateHelper;
            _jwtHelper = jwtHelper;
            _unitOfWork = unitOfWork;


            _fileIOHelper = fileIOHelper;
            _filePathConfig = filePathConfig;
            _appConfig = appConfig;
            _payosConfig = payosConfig;
            _jobsConfig = jobsConfig;

            _httpServiceQueryClient = httpServiceQueryClient;

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

            _redisSharedCacheService = redisSharedCacheService;
        }


        private async Task<(List<TEntity> entities, int daysBack, int distinctCount)> QueryIncrementalWithMinDistinctAsync<TEntity, TKey>(
            int daysBatchSize,
            int minDistinctEntityCount,
            int minDaysBack,
            int maxDaysBack,
            Func<DateTime, DateTime, Expression<Func<TEntity, bool>>> predicateFactory,
            Func<IQueryable<TEntity>, IQueryable<TEntity>> includeFunc,
            Func<TEntity, TKey> distinctKeySelector
        ) where TEntity : class
        {
            var now = _dateHelper.GetNowByAppTimeZone();
            int currentDaysBack = 0;

            List<TEntity> accumulatedEntities = new List<TEntity>();
            int distinctCount = 0;

            Console.WriteLine($"[QueryIncremental] Starting - BatchSize={daysBatchSize}, MinDistinct={minDistinctEntityCount}, MinDays={minDaysBack}, MaxDays={maxDaysBack}");

            // Lấy repository tương ứng với TEntity
            var repository = GetRepositoryForEntity<TEntity>();

            while (currentDaysBack < maxDaysBack)
            {
                var batchStartDate = now.AddDays(-(currentDaysBack + daysBatchSize));
                var batchEndDate = now.AddDays(-currentDaysBack);

                Console.WriteLine($"[QueryIncremental] Querying batch: from {batchStartDate:yyyy-MM-dd} to {batchEndDate:yyyy-MM-dd}");

                // Tạo predicate cho batch hiện tại
                var predicate = predicateFactory(batchStartDate, batchEndDate);

                // Query batch
                var batchEntities = await repository.FindAll(
                    predicate: predicate,
                    includeFunc: includeFunc
                ).ToListAsync();

                Console.WriteLine($"[QueryIncremental] Batch returned {batchEntities.Count} entities");

                // Nếu batch rỗng, dừng vì không còn data
                if (batchEntities.Count == 0)
                {
                    Console.WriteLine($"[QueryIncremental] No more data found, stopping at {currentDaysBack + daysBatchSize} days back");
                    break;
                }

                // Accumulate
                accumulatedEntities.AddRange(batchEntities);

                // Tính distinct count
                distinctCount = accumulatedEntities
                    .Select(distinctKeySelector)
                    .Distinct()
                    .Count();

                Console.WriteLine($"[QueryIncremental] Total accumulated: {accumulatedEntities.Count}, Distinct: {distinctCount}");

                // Di chuyển sang batch tiếp theo
                currentDaysBack += daysBatchSize;

                // Kiểm tra điều kiện dừng
                if (distinctCount >= minDistinctEntityCount && currentDaysBack >= minDaysBack)
                {
                    Console.WriteLine($"[QueryIncremental] Condition met: {distinctCount} distinct entities in {currentDaysBack} days");
                    break;
                }
            }

            Console.WriteLine($"[QueryIncremental] Completed - Total entities: {accumulatedEntities.Count}, Days back: {currentDaysBack}, Distinct: {distinctCount}");

            return (accumulatedEntities, currentDaysBack, distinctCount);
        }

        /// <summary>
        /// Helper method để lấy repository phù hợp với entity type
        /// </summary>
        private IGenericRepository<TEntity> GetRepositoryForEntity<TEntity>() where TEntity : class
        {
            var entityType = typeof(TEntity);

            if (entityType == typeof(PodcastEpisodeListenSession))
                return (IGenericRepository<TEntity>)_podcastEpisodeListenSessionGenericRepository;

            // if (entityType == typeof(PodcastShowFollow))
            //     return (IGenericRepository<TEntity>)_podcastShowFollowGenericRepository; // Cần thêm repository này vào constructor

            // if (entityType == typeof(PodcastChannelFavorite))
            //     return (IGenericRepository<TEntity>)_podcastChannelFavoriteGenericRepository; // Cần thêm repository này vào constructor

            throw new NotSupportedException($"Repository for entity type {entityType.Name} is not registered");
        }

        private async Task<(List<AccountFollowedPodcastShowDTO> follows, int daysBack, int distinctCount)> GetAccountNewFollowedPodcastShowAsync(
            int daysBatchSize,
            int minDistinctEntityCount,
            int minDaysBack,
            int maxDaysBack
        )
        {
            var now = _dateHelper.GetNowByAppTimeZone();

            Console.WriteLine($"[GetAccountNewFollowedPodcastShow] Starting - BatchSize={daysBatchSize}, MinDistinct={minDistinctEntityCount}, MinDays={minDaysBack}, MaxDays={maxDaysBack}");

            // Query toàn bộ follows từ UserService (vì không support date range filter)
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
        {
            new BatchQueryItem
            {
                Key = "accountFollowedPodcastShows",
                QueryType = "findall",
                EntityType = "AccountFollowedPodcastShow",
                Parameters = JObject.FromObject(new { })
            }
        }
            };

            var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
            var allFollows = result.Results["accountFollowedPodcastShows"].ToObject<List<AccountFollowedPodcastShowDTO>>();

            Console.WriteLine($"[GetAccountNewFollowedPodcastShow] Total follows from UserService: {allFollows.Count}");

            // Incremental filtering logic
            int currentDaysBack = 0;
            List<AccountFollowedPodcastShowDTO> accumulatedFollows = new List<AccountFollowedPodcastShowDTO>();
            int distinctCount = 0;

            while (currentDaysBack < maxDaysBack)
            {
                // Tính date range cho batch hiện tại
                var batchStartDate = now.AddDays(-(currentDaysBack + daysBatchSize));
                var batchEndDate = now.AddDays(-currentDaysBack);


                // Filter follows trong batch hiện tại
                var batchFollows = allFollows
                    .Where(f => f.CreatedAt >= batchStartDate && f.CreatedAt < batchEndDate)
                    .ToList();


                // Nếu batch rỗng, dừng vì không còn data cũ hơn
                if (batchFollows.Count == 0)
                {
                    break;
                }

                // Accumulate
                accumulatedFollows.AddRange(batchFollows);

                // Tính distinct show count
                distinctCount = accumulatedFollows
                    .Select(f => f.PodcastShowId)
                    .Distinct()
                    .Count();

                // Di chuyển sang batch tiếp theo
                currentDaysBack += daysBatchSize;

                // Kiểm tra điều kiện dừng
                if (distinctCount >= minDistinctEntityCount && currentDaysBack >= minDaysBack)
                {
                    break;
                }
            }

            return (accumulatedFollows, currentDaysBack, distinctCount);
        }

        private async Task<(List<AccountFavoritedPodcastChannelDTO> favorites, int daysBack, int distinctCount)> GetAccountNewFavoritedPodcastChannelAsync(
    int daysBatchSize,
    int minDistinctEntityCount,
    int minDaysBack,
    int maxDaysBack
)
        {
            var now = _dateHelper.GetNowByAppTimeZone();

            Console.WriteLine($"[GetAccountNewFavoritedPodcastChannel] Starting - BatchSize={daysBatchSize}, MinDistinct={minDistinctEntityCount}, MinDays={minDaysBack}, MaxDays={maxDaysBack}");

            // Query toàn bộ favorites từ UserService (vì không support date range filter)
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
        {
            new BatchQueryItem
            {
                Key = "accountFavoritedPodcastChannels",
                QueryType = "findall",
                EntityType = "AccountFavoritedPodcastChannel",
                Parameters = JObject.FromObject(new { })
            }
        }
            };

            var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
            var allFavorites = result.Results["accountFavoritedPodcastChannels"].ToObject<List<AccountFavoritedPodcastChannelDTO>>();

            Console.WriteLine($"[GetAccountNewFavoritedPodcastChannel] Total favorites from UserService: {allFavorites.Count}");

            // Incremental filtering logic
            int currentDaysBack = 0;
            List<AccountFavoritedPodcastChannelDTO> accumulatedFavorites = new List<AccountFavoritedPodcastChannelDTO>();
            int distinctCount = 0;

            while (currentDaysBack < maxDaysBack)
            {
                // Tính date range cho batch hiện tại
                var batchStartDate = now.AddDays(-(currentDaysBack + daysBatchSize));
                var batchEndDate = now.AddDays(-currentDaysBack);

                Console.WriteLine($"[GetAccountNewFavoritedPodcastChannel] Processing batch: from {batchStartDate:yyyy-MM-dd} to {batchEndDate:yyyy-MM-dd}");

                // Filter favorites trong batch hiện tại
                var batchFavorites = allFavorites
                    .Where(f => f.CreatedAt >= batchStartDate && f.CreatedAt < batchEndDate)
                    .ToList();

                Console.WriteLine($"[GetAccountNewFavoritedPodcastChannel] Batch returned {batchFavorites.Count} favorites");

                // Nếu batch rỗng, dừng vì không còn data cũ hơn
                if (batchFavorites.Count == 0)
                {
                    Console.WriteLine($"[GetAccountNewFavoritedPodcastChannel] No more data found in this batch, stopping");
                    break;
                }

                // Accumulate
                accumulatedFavorites.AddRange(batchFavorites);

                // Tính distinct channel count
                distinctCount = accumulatedFavorites
                    .Select(f => f.PodcastChannelId)
                    .Distinct()
                    .Count();

                Console.WriteLine($"[GetAccountNewFavoritedPodcastChannel] Total accumulated: {accumulatedFavorites.Count}, Distinct channels: {distinctCount}");

                // Di chuyển sang batch tiếp theo
                currentDaysBack += daysBatchSize;

                // Kiểm tra điều kiện dừng
                if (distinctCount >= minDistinctEntityCount && currentDaysBack >= minDaysBack)
                {
                    Console.WriteLine($"[GetAccountNewFavoritedPodcastChannel] Condition met: {distinctCount} distinct channels in {currentDaysBack} days");
                    break;
                }
            }

            Console.WriteLine($"[GetAccountNewFavoritedPodcastChannel] Completed - Total favorites: {accumulatedFavorites.Count}, Days back: {currentDaysBack}, Distinct channels: {distinctCount}");

            return (accumulatedFavorites, currentDaysBack, distinctCount);
        }

        private async Task<(List<AccountFollowedPodcasterDTO> follows, int daysBack, int distinctCount)> GetAccountNewFollowedPodcasterAsync(
            int daysBatchSize,
            int minDistinctEntityCount,
            int minDaysBack,
            int maxDaysBack
        )
        {
            var now = _dateHelper.GetNowByAppTimeZone();

            Console.WriteLine($"[GetAccountNewFollowedPodcaster] Starting - BatchSize={daysBatchSize}, MinDistinct={minDistinctEntityCount}, MinDays={minDaysBack}, MaxDays={maxDaysBack}");

            // Query toàn bộ follows từ UserService (vì không support date range filter)
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
        {
            new BatchQueryItem
            {
                Key = "accountFollowedPodcasters",
                QueryType = "findall",
                EntityType = "AccountFollowedPodcaster",
                Parameters = JObject.FromObject(new { })
            }
        }
            };

            var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
            var allFollows = result.Results["accountFollowedPodcasters"].ToObject<List<AccountFollowedPodcasterDTO>>();

            Console.WriteLine($"[GetAccountNewFollowedPodcaster] Total follows from UserService: {allFollows.Count}");

            // Incremental filtering logic
            int currentDaysBack = 0;
            List<AccountFollowedPodcasterDTO> accumulatedFollows = new List<AccountFollowedPodcasterDTO>();
            int distinctCount = 0;

            while (currentDaysBack < maxDaysBack)
            {
                // Tính date range cho batch hiện tại
                var batchStartDate = now.AddDays(-(currentDaysBack + daysBatchSize));
                var batchEndDate = now.AddDays(-currentDaysBack);

                Console.WriteLine($"[GetAccountNewFollowedPodcaster] Processing batch: from {batchStartDate:yyyy-MM-dd} to {batchEndDate:yyyy-MM-dd}");

                // Filter follows trong batch hiện tại
                var batchFollows = allFollows
                    .Where(f => f.CreatedAt >= batchStartDate && f.CreatedAt < batchEndDate)
                    .ToList();

                Console.WriteLine($"[GetAccountNewFollowedPodcaster] Batch returned {batchFollows.Count} follows");

                // Nếu batch rỗng, dừng vì không còn data cũ hơn
                if (batchFollows.Count == 0)
                {
                    Console.WriteLine($"[GetAccountNewFollowedPodcaster] No more data found in this batch, stopping");
                    break;
                }

                // Accumulate
                accumulatedFollows.AddRange(batchFollows);

                // Tính distinct podcaster count
                distinctCount = accumulatedFollows
                    .Select(f => f.PodcasterId)
                    .Distinct()
                    .Count();

                Console.WriteLine($"[GetAccountNewFollowedPodcaster] Total accumulated: {accumulatedFollows.Count}, Distinct podcasters: {distinctCount}");

                // Di chuyển sang batch tiếp theo
                currentDaysBack += daysBatchSize;

                // Kiểm tra điều kiện dừng
                if (distinctCount >= minDistinctEntityCount && currentDaysBack >= minDaysBack)
                {
                    Console.WriteLine($"[GetAccountNewFollowedPodcaster] Condition met: {distinctCount} distinct podcasters in {currentDaysBack} days");
                    break;
                }
            }

            Console.WriteLine($"[GetAccountNewFollowedPodcaster] Completed - Total follows: {accumulatedFollows.Count}, Days back: {currentDaysBack}, Distinct podcasters: {distinctCount}");

            return (accumulatedFollows, currentDaysBack, distinctCount);
        }


        /////////////////////////////////////////////////////////////
        public async Task UpdatePodcasterAllTimeMaxQueryMetric()
        {
            try
            {
                // Logic to update show all-time max query metric goes here
                // Logic to update podcaster all-time max query metric goes here
                // truy danh sách podcaster đã được isVerified = true từ UserService
                // 1: lấy MaxTotalFollow từ cột totalFollow của tất cả các podcaster trong bảng PodcasterProfile của UserService
                // 2: lấy MaxListenCount từ cột ListenCount của tất cả các podcaster trong bảng PodcasterProfile của UserService
                // 3: lấy MaxRatingTerm trong đó RatingTerm của mỗi podcaster được tính bằng: RT = AverageRating * log(RatingCount + 1)
                // 4: lấy MaxAge từ cột verifiedAt của tất cả các podcaster trong bảng PodcasterProfile của UserService tính từ thời điểm hiện tại
                // 4: lấy ra LastUpdated là hiện tại
                // 5: lưu vào DTO sau đó đưa vào cache
                // 4: cập nhật vào cache key trong config của job

                var cacheKey = _jobsConfig.PodcasterAllTimeMaxQueryMetricUpdateJob.RedisKeyName;
                var cacheTTL = _jobsConfig.PodcasterAllTimeMaxQueryMetricUpdateJob.RedisKeyTTLSeconds;

                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                {
                    new BatchQueryItem
                    {
                        Key = "verifiedPodcasterProfiles",
                        QueryType = "findall",
                        EntityType = "PodcasterProfile",
                        Parameters = JObject.FromObject(new
                        {
                            IsVerified = true
                        })
                    }
                }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
                var verifiedPodcasterProfiles = result.Results["verifiedPodcasterProfiles"].ToObject<List<PodcasterProfileDTO>>();

                var podcasterAllTimeMaxQueryMetric = new PodcasterAllTimeMaxQueryMetric
                {
                    MaxTotalFollow = verifiedPodcasterProfiles
                        .Select(g => g.TotalFollow)
                        .DefaultIfEmpty(0)
                        .Max(),

                    MaxListenCount = verifiedPodcasterProfiles
                        .Select(g => g.ListenCount)
                        .DefaultIfEmpty(0)
                        .Max(),
                    MaxAge = verifiedPodcasterProfiles
                        .Select(pp => pp.VerifiedAt.HasValue
                            ? (int)(_dateHelper.GetNowByAppTimeZone() - pp.VerifiedAt.Value).TotalDays
                            : 0)
                        .DefaultIfEmpty(0)
                        .Max(),

                    MaxRatingTerm = verifiedPodcasterProfiles
                        .Select(pp => pp.AverageRating * Math.Log(pp.RatingCount + 1))
                        .DefaultIfEmpty(0)
                        .Max(),

                    LastUpdated = _dateHelper.GetNowByAppTimeZone()
                };

                await _redisSharedCacheService.KeySetAsync(cacheKey, podcasterAllTimeMaxQueryMetric, TimeSpan.FromSeconds(cacheTTL ?? 3600));
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new Exception("UpdatePodcasterAllTimeMaxQueryMetric failed, error: " + ex.Message);
            }
        }
        public async Task UpdatePodcasterTemporal7dMaxQueryMetric()
        {
            try
            {
                // Logic to update show all-time max query metric goes here
                // truy danh sách podcaster đã được isVerified = true từ UserService
                // 1: lấy MaxNewListenSession 7 ngày tối thiểu về trước (entity >= 5 podcaster) từ bảng PodcastEpisodeListenSession liên kết với PodcasterProfile của UserService
                // 2: lấy MaxNewFollow 7 ngày tối thiểu về trước (entity >= 5 podcaster) từ bảng AccountFollowedPodcaster của UserService
                // 3: lấy MaxGrowth 7 ngày tối thiểu về trước (entity >= 5 podcaster) trong đó Growth của mỗi podcaster được tính bằng: G = NewListenSessionCount + NewFollowCount
                // 4: lấy ra LastUpdated là hiện tại
                // 5: lưu vào DTO sau đó đưa vào cache
                // 4: cập nhật vào cache key trong config của job

                var cacheKey = _jobsConfig.PodcasterTemporal7dMaxQueryMetricUpdateJob.RedisKeyName;
                var cacheTTL = _jobsConfig.PodcasterTemporal7dMaxQueryMetricUpdateJob.RedisKeyTTLSeconds;

                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                {
                    new BatchQueryItem
                    {
                        Key = "verifiedPodcasterProfiles",
                        QueryType = "findall",
                        EntityType = "PodcasterProfile",
                        Parameters = JObject.FromObject(new
                        {
                            IsVerified = true
                        })
                    }
                }
                };
                var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
                var verifiedPodcasterProfiles = result.Results["verifiedPodcasterProfiles"].ToObject<List<PodcasterProfileDTO>>();
                var verifiedPodcasterIds = verifiedPodcasterProfiles
                    .Select(pp => pp.AccountId)
                    .ToHashSet();

                var (newFollows, followDaysBack, followDistinctCount) = await GetAccountNewFollowedPodcasterAsync(
                    daysBatchSize: 7,
                    minDistinctEntityCount: 5,
                    minDaysBack: 7,
                    maxDaysBack: 365
                );

                newFollows = newFollows
                    .Where(f => verifiedPodcasterIds.Contains(f.PodcasterId))
                    .ToList();


                var (listenSessions, listenDaysBack, distinctShowCountForListen) =
                    await QueryIncrementalWithMinDistinctAsync<PodcastEpisodeListenSession, int>(
                        daysBatchSize: 7,
                        minDistinctEntityCount: 5,
                        minDaysBack: 7,
                        maxDaysBack: 365,
                        predicateFactory: (startDate, endDate) => pes =>
                            pes.CreatedAt >= startDate &&
                            pes.CreatedAt < endDate &&
                            pes.IsContentRemoved == false &&
                            pes.PodcastEpisode.DeletedAt == null &&
                            pes.PodcastEpisode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault()
                            .PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Removed &&
                            pes.PodcastEpisode.PodcastShow.DeletedAt == null &&
                            pes.PodcastEpisode.PodcastShow.PodcastShowStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault()
                            .PodcastShowStatusId != (int)PodcastShowStatusEnum.Removed &&
                            (pes.PodcastEpisode.PodcastShow.PodcastChannel == null || (pes.PodcastEpisode.PodcastShow.PodcastChannel != null &&
                                pes.PodcastEpisode.PodcastShow.PodcastChannel.DeletedAt == null
                            )),
                        includeFunc: q => q.Include(pes => pes.PodcastEpisode)
                            .ThenInclude(pe => pe.PodcastShow),
                        distinctKeySelector: pes => pes.PodcastEpisode.PodcastShow.PodcasterId
                    );

                listenSessions = listenSessions
                    .Where(ls => verifiedPodcasterIds.Contains(ls.PodcastEpisode.PodcastShow.PodcasterId))
                    .ToList();

                var podcasterIdsInListenSessions = listenSessions
                    .Select(ls => ls.PodcastEpisode.PodcastShow.PodcasterId)
                    .Distinct();

                var podcasterIdsInFollows = newFollows
                    .Select(nf => nf.PodcasterId)
                    .Distinct();

                var allTemporalPodcasterIds = podcasterIdsInListenSessions
                    .Union(podcasterIdsInFollows)
                    .Distinct()
                    .ToList();

                var podcasterTemporal7dMaxQueryMetric = new PodcasterTemporal7dMaxQueryMetric
                {
                    MaxNewListenSession = listenSessions
                        .GroupBy(pes => pes.PodcastEpisode.PodcastShow.PodcasterId)
                        .Select(g => new
                        {
                            PodcasterId = g.Key,
                            NewListenSessionCount = g.Count()
                        })
                        .Select(x => x.NewListenSessionCount)
                        .DefaultIfEmpty(0)
                        .Max(),

                    MaxNewFollow = newFollows
                        .GroupBy(afps => afps.PodcasterId)
                        .Select(g => new
                        {
                            PodcasterId = g.Key,
                            NewFollowCount = g.Count()
                        })
                        .Select(x => x.NewFollowCount)
                        .DefaultIfEmpty(0)
                        .Max(),

                    MaxGrowth = allTemporalPodcasterIds
                        .Select(podcasterId =>
                        {
                            var newListenSessionCount = listenSessions
                                .Count(ls => ls.PodcastEpisode.PodcastShow.PodcasterId == podcasterId);
                            var newFollowCount = newFollows
                                .Count(nf => nf.PodcasterId == podcasterId);
                            return newListenSessionCount + newFollowCount;
                        })
                        .DefaultIfEmpty(0)
                        .Max(),

                    LastUpdated = _dateHelper.GetNowByAppTimeZone()
                };

                await _redisSharedCacheService.KeySetAsync(cacheKey, podcasterTemporal7dMaxQueryMetric, TimeSpan.FromSeconds(cacheTTL ?? 3600));

            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new Exception("UpdatePodcasterTemporal7dMaxQueryMetric failed, error: " + ex.Message);
            }
        }

        public async Task UpdateShowAllTimeMaxQueryMetric()
        {
            try
            {
                // Logic to update show all-time max query metric goes here
                // 1: lấy MaxTotalFollow từ cột totalFollow của tất cả các show (chưa bị xoá / Removed) trong bảng PodcastShow
                // 2: lấy MaxListenCount từ cột ListenCount của tất cả các show (chưa bị xoá / Removed) trong bảng PodcastShow
                // 3: lấy MaxRatingTerm trong đó RatingTerm của mỗi show được tính bằng: RT = AverageRating * log(RatingCount + 1)
                // 4: lấy ra LastUpdated là hiện tại
                // 5: lưu vào DTO sau đó đưa vào cache
                // 4: cập nhật vào cache key trong config của job

                var cacheKey = _jobsConfig.ShowAllTimeMaxQueryMetricUpdateJob.RedisKeyName;
                var cacheTTL = _jobsConfig.ShowAllTimeMaxQueryMetricUpdateJob.RedisKeyTTLSeconds;

                var allTimeShows = await _podcastShowGenericRepository.FindAll(
                    predicate: ps => ps.DeletedAt == null &&
                    // trạng thái cuối cùng không phải là Removed
                    ps.PodcastShowStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault()
                            .PodcastShowStatusId != (int)PodcastShowStatusEnum.Removed
                    ,
                    includeFunc: null
                ).ToListAsync();

                var showAllTimeMaxQueryMetric = new ShowAllTimeMaxQueryMetric
                {
                    MaxTotalFollow = allTimeShows
                        .Select(g => g.TotalFollow)
                        .DefaultIfEmpty(0)
                        .Max(),

                    MaxListenCount = allTimeShows
                        .Select(g => g.ListenCount)
                        .DefaultIfEmpty(0)
                        .Max(),

                    MaxRatingTerm = allTimeShows
                        .Select(ps => ps.AverageRating * Math.Log(ps.RatingCount + 1))
                        .DefaultIfEmpty(0)
                        .Max(),


                    LastUpdated = _dateHelper.GetNowByAppTimeZone()
                };

                await _redisSharedCacheService.KeySetAsync(cacheKey, showAllTimeMaxQueryMetric, TimeSpan.FromSeconds(cacheTTL ?? 3600));


            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new Exception("UpdateShowAllTimeMaxQueryMetric failed, error: " + ex.Message);
            }
        }

        public async Task UpdateChannelAllTimeMaxQueryMetric()
        {
            try
            {
                // Logic to update channel all-time max query metric goes here
                // 1: lấy MaxListenCount từ cột ListenCount của tất cả các channel (chưa bị xoá) trong bảng PodcastChannel
                // 2: lấy MaxTotalFavorite từ cột TotalFavorite của tất cả các channel (chưa bị xoá) trong bảng PodcastChannel
                // 3: lấy ra LastUpdated là hiện tại
                // 4: lưu vào DTO sau đó đưa vào cache
                // 4: cập nhật vào cache key trong config của job

                var cacheKey = _jobsConfig.ChannelAllTimeMaxQueryMetricUpdateJob.RedisKeyName;
                var cacheTTL = _jobsConfig.ChannelAllTimeMaxQueryMetricUpdateJob.RedisKeyTTLSeconds;


                var allTimeChannels = await _podcastChannelGenericRepository.FindAll(
                    predicate: pc => pc.DeletedAt == null,
                    includeFunc: null
                ).ToListAsync();
                var channelAllTimeMaxQueryMetric = new ChannelAllTimeMaxQueryMetric
                {
                    MaxListenCount = allTimeChannels
                        .Select(g => g.ListenCount)
                        .DefaultIfEmpty(0)
                        .Max(),
                    MaxTotalFavorite = allTimeChannels
                        .Select(g => g.TotalFavorite)
                        .DefaultIfEmpty(0)
                        .Max(),

                    LastUpdated = _dateHelper.GetNowByAppTimeZone()
                };

                await _redisSharedCacheService.KeySetAsync(cacheKey, channelAllTimeMaxQueryMetric, TimeSpan.FromSeconds(cacheTTL ?? 3600));


            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new Exception("UpdateChannelAllTimeMaxQueryMetric failed, error: " + ex.Message);
            }

        }

        public async Task UpdateShowTemporal7dMaxQueryMetric()
        {
            try
            {
                // NGUYÊN TẮT TRONG TEMPORAL: 7 NGÀY LÀ TỐI THIẾU CÓ THỂ HƠN 7 NGÀY MIỄN SAO ĐẢM BẢO MIN RECORD ENTITY = 5 SHOW / CHANNEL / PODCASTER
                // điều kiện của listen session: isContentRemoved = false
                // Logic to update show temporal 7-day max query metric goes here
                // 1: lấy MaxNewListenSession 7 ngày tối thiểu về trước (entity phải min =5 nếu không đủ 5 trong 7 ngày thì lấy quá đến khi đủ 5 entity) từ bảng PodcastShowListenSession
                // 2: lấy MaxNewFollow 7 ngày tối thiểu về trước (entity phải min =5 nếu không đủ 5 trong 7 ngày thì lấy quá đến khi đủ 5 entity) từ bảng AccountFollowedPodcastShow
                // 3: lấy ra LastUpdated là hiện tại
                // 4: lưu vào DTO sau đó đưa vào cache
                // 4: cập nhật vào cache key trong config của job

                var cacheKey = _jobsConfig.ShowTemporal7dMaxQueryMetricUpdateJob.RedisKeyName;
                var cacheTTL = _jobsConfig.ShowTemporal7dMaxQueryMetricUpdateJob.RedisKeyTTLSeconds;


                // ============ Query MaxNewListenSession ============
                var (listenSessions, listenDaysBack, distinctShowCountForListen) =
                    await QueryIncrementalWithMinDistinctAsync<PodcastEpisodeListenSession, Guid>(
                        daysBatchSize: 7,
                        minDistinctEntityCount: 5,
                        minDaysBack: 7,
                        maxDaysBack: 365,
                        predicateFactory: (startDate, endDate) => pes =>
                            pes.CreatedAt >= startDate &&
                            pes.CreatedAt < endDate &&
                            pes.IsContentRemoved == false &&
                            pes.PodcastEpisode.DeletedAt == null &&
                            pes.PodcastEpisode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault()
                            .PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Removed &&
                            pes.PodcastEpisode.PodcastShow.DeletedAt == null &&
                            pes.PodcastEpisode.PodcastShow.PodcastShowStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault().PodcastShowStatusId != (int)PodcastShowStatusEnum.Removed &&
                            (pes.PodcastEpisode.PodcastShow.PodcastChannel == null || (pes.PodcastEpisode.PodcastShow.PodcastChannel != null &&
                                pes.PodcastEpisode.PodcastShow.PodcastChannel.DeletedAt == null
                            )),
                        includeFunc: q => q.Include(pes => pes.PodcastEpisode)
                            .ThenInclude(pe => pe.PodcastShow),
                        distinctKeySelector: pes => pes.PodcastEpisode.PodcastShowId
                    );

                // Tính MaxNewListenSession
                int maxNewListenSession = 0;
                if (listenSessions.Count > 0)
                {
                    var listenSessionByShow = listenSessions
                        .GroupBy(pes => pes.PodcastEpisode.PodcastShowId)
                        .Select(g => new
                        {
                            PodcastShowId = g.Key,
                            Count = g.Count()
                        })
                        .ToList();

                    maxNewListenSession = listenSessionByShow
                        .Select(x => x.Count)
                        .Max();
                }

                // ============ Query MaxNewFollow ============
                var (follows, followDaysBack, distinctShowCountForFollow) =
                    await GetAccountNewFollowedPodcastShowAsync(
                        daysBatchSize: 7,
                        minDistinctEntityCount: 5,
                        minDaysBack: 7,
                        maxDaysBack: 365
                    );

                int maxNewFollow = 0;
                if (follows.Count > 0)
                {
                    var followsByShow = follows
                        .GroupBy(f => f.PodcastShowId)
                        .Select(g => new { PodcastShowId = g.Key, Count = g.Count() })
                        .ToList();

                    maxNewFollow = followsByShow.Select(x => x.Count).Max();
                }

                // ============ Lưu vào cache ============
                var now = _dateHelper.GetNowByAppTimeZone();
                int finalDaysBack = Math.Max(listenDaysBack, followDaysBack);

                var metric = new ShowTemporal7dMaxQueryMetric
                {
                    MaxNewListenSession = maxNewListenSession,
                    MaxNewFollow = maxNewFollow,
                    LastUpdated = now
                };

                await _redisSharedCacheService.KeySetAsync(cacheKey, metric, TimeSpan.FromSeconds(cacheTTL ?? 3600));


            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new Exception("UpdateShowTemporal7dMaxQueryMetric failed, error: " + ex.Message);
            }

        }

        public async Task UpdateChannelTemporal7dMaxQueryMetric()
        {
            try
            {
                // NGUYÊN TẮT TRONG TEMPORAL: 7 NGÀY LÀ TỐI THIẾU CÓ THỂ HƠN 7 NGÀY MIỄN SAO ĐẢM BẢO MIN RECORD ENTITY = 5 SHOW / CHANNEL / PODCASTER
                // điều kiện của listen session: isContentRemoved = false
                // Logic to update channel temporal 7-day max query metric goes here
                // 1: lấy MaxNewListenSession 7 ngày tối thiểu về trước (entity phải min =5 nếu không đủ 5 trong 7 ngày thì lấy quá đến khi đủ 5 entity) từ bảng PodcastShowListenSession
                // 2: lấy MaxNewFavorite 7 ngày tối thiểu về trước (entity phải min =5 nếu không đủ 5 trong 7 ngày thì lấy quá đến khi đủ 5 entity) từ bảng AccountFavoritedPodcastChannel
                // 3: lấy ra LastUpdated là hiện tại
                // 4: lưu vào DTO sau đó đưa vào cache
                // 4: cập nhật vào cache key trong config của job
                var cacheKey = _jobsConfig.ChannelTemporal7dMaxQueryMetricUpdateJob.RedisKeyName;
                var cacheTTL = _jobsConfig.ChannelTemporal7dMaxQueryMetricUpdateJob.RedisKeyTTLSeconds;

                // ============ Query MaxNewListenSession ============
                // Query listen sessions từ episodes thuộc channels
                var (listenSessions, listenDaysBack, distinctChannelCountForListen) =
                    await QueryIncrementalWithMinDistinctAsync<PodcastEpisodeListenSession, Guid?>(
                        daysBatchSize: 7,
                        minDistinctEntityCount: 5,
                        minDaysBack: 7,
                        maxDaysBack: 365,
                        predicateFactory: (startDate, endDate) => pes =>
                            pes.CreatedAt >= startDate &&
                            pes.CreatedAt < endDate &&
                            pes.IsContentRemoved == false &&
                            pes.PodcastEpisode.DeletedAt == null &&
                            pes.PodcastEpisode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault()
                            .PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Removed &&
                            pes.PodcastEpisode.PodcastShow.DeletedAt == null &&
                            pes.PodcastEpisode.PodcastShow.PodcastShowStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault().PodcastShowStatusId != (int)PodcastShowStatusEnum.Removed &&
                            pes.PodcastEpisode.PodcastShow.PodcastChannel != null &&
                            pes.PodcastEpisode.PodcastShow.PodcastChannel.DeletedAt == null,
                        includeFunc: q => q
                            .Include(pes => pes.PodcastEpisode)
                            .ThenInclude(pe => pe.PodcastShow)
                            .ThenInclude(ps => ps.PodcastChannel),
                        distinctKeySelector: pes => pes.PodcastEpisode.PodcastShow.PodcastChannelId
                    );

                // Tính MaxNewListenSession
                int maxNewListenSession = 0;
                if (listenSessions.Count > 0)
                {
                    var listenSessionByChannel = listenSessions
                        .GroupBy(pes => pes.PodcastEpisode.PodcastShow.PodcastChannelId)
                        .Select(g => new
                        {
                            PodcastChannelId = g.Key,
                            Count = g.Count()
                        })
                        .ToList();

                    maxNewListenSession = listenSessionByChannel
                        .Select(x => x.Count)
                        .Max();
                }

                // ============ Query MaxNewFavorite ============
                var (favorites, favoriteDaysBack, distinctChannelCountForFavorite) =
                    await GetAccountNewFavoritedPodcastChannelAsync(
                        daysBatchSize: 7,
                        minDistinctEntityCount: 5,
                        minDaysBack: 7,
                        maxDaysBack: 365
                    );

                int maxNewFavorite = 0;
                if (favorites.Count > 0)
                {
                    var favoritesByChannel = favorites
                        .GroupBy(f => f.PodcastChannelId)
                        .Select(g => new
                        {
                            PodcastChannelId = g.Key,
                            Count = g.Count()
                        })
                        .ToList();

                    maxNewFavorite = favoritesByChannel
                        .Select(x => x.Count)
                        .Max();
                }

                // ============ Lưu vào cache ============
                var now = _dateHelper.GetNowByAppTimeZone();
                int finalDaysBack = Math.Max(listenDaysBack, favoriteDaysBack);

                var metric = new ChannelTemporal7dMaxQueryMetric
                {
                    MaxNewListenSession = maxNewListenSession,
                    MaxNewFavorite = maxNewFavorite,
                    LastUpdated = now
                };

                await _redisSharedCacheService.KeySetAsync(cacheKey, metric, TimeSpan.FromSeconds(cacheTTL ?? 3600));
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new Exception("UpdateChannelTemporal7dMaxQueryMetric failed, error: " + ex.Message);
            }

        }

        public async Task UpdateSystemPreferencesTemporal30dQueryMetric()
        {
            try
            {
                // điều kiện của listen session: isContentRemoved = false
                // Logic to update System Preferences temporal 30-day goes here
                // 1: lấy ListenedPodcastCategories[] 30 ngày tối thiểu về trước (entity phải 4 categories min nếu không đủ trong 30 ngày thì lấy quá đến khi đủ 4 categories) từ bảng PodcastEpisodeListenSession
                //  --> lấy nhiều object ListenedPodcastCategoriy có categoryId, listenCount, PodcastSubCategories (có subcategoryId, listenCount)
                // 2: lấy ListenedPodcaster[] 30 ngày tối thiểu về trước (entity phải 2 podcasters min nếu không đủ trong 30 ngày thì lấy quá đến khi đủ 2 podcasters) từ bảng PodcastEpisodeListenSession
                //  --> lấy nhiều object ListenedPodcaster có podcasterId, listenCount
                // 3: lấy ra LastUpdated là hiện tại
                // 4: lưu vào DTO sau đó đưa vào cache
                // 4: cập nhật vào cache key trong config của job

                var cacheKey = _jobsConfig.SystemPreferencesTemporal30dQueryMetricUpdateJob.RedisKeyName;
                var cacheTTL = _jobsConfig.SystemPreferencesTemporal30dQueryMetricUpdateJob.RedisKeyTTLSeconds;

                // ============ Query ListenedPodcastCategory[] ============
                var (listenSessions, listenDaysBack, distinctCategoryCount) =
                    await QueryIncrementalWithMinDistinctAsync<PodcastEpisodeListenSession, int?>(
                        daysBatchSize: 15,
                        minDistinctEntityCount: 4,
                        minDaysBack: 30,
                        maxDaysBack: 365,
                        predicateFactory: (startDate, endDate) => pes =>
                            pes.CreatedAt >= startDate &&
                            pes.CreatedAt < endDate &&
                            pes.IsContentRemoved == false &&
                            pes.PodcastEpisode.DeletedAt == null &&
                            pes.PodcastEpisode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault()
                            .PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Removed &&
                            pes.PodcastEpisode.PodcastShow.DeletedAt == null &&
                            pes.PodcastEpisode.PodcastShow.PodcastShowStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault()
                            .PodcastShowStatusId != (int)PodcastShowStatusEnum.Removed &&
                            (pes.PodcastEpisode.PodcastShow.PodcastChannel == null || (pes.PodcastEpisode.PodcastShow.PodcastChannel != null &&
                                pes.PodcastEpisode.PodcastShow.PodcastChannel.DeletedAt == null
                            )),
                        includeFunc:
                        q => q
                            .Include(pes => pes.PodcastEpisode)
                            .ThenInclude(pe => pe.PodcastShow),
                        //     .ThenInclude(ps => ps.PodcastShowHashtags)
                        //     .ThenInclude(psh => psh.Hashtag),
                        distinctKeySelector: pes =>
                            pes.PodcastCategoryId
                    );

                List<SystemListenedPodcastCategory> ListenedPodcastCategories = new List<SystemListenedPodcastCategory>();
                if (listenSessions.Count > 0)
                {
                    var categoryListenCounts = listenSessions
                        .Where(pes => pes.PodcastCategoryId.HasValue)
                        .GroupBy(pes => pes.PodcastCategoryId.Value)
                        .Select(g => new SystemListenedPodcastCategory
                        {
                            PodcastCategoryId = g.Key,
                            ListenCount = g.Count(),
                            PodcastSubCategories = g
                                .Where(pes => pes.PodcastSubCategoryId.HasValue)
                                .GroupBy(pes => pes.PodcastSubCategoryId.Value)
                                .Select(sg => new SystemListenedPodcastSubCategory
                                {
                                    PodcastSubCategoryId = sg.Key,
                                    ListenCount = sg.Count()
                                })
                                .OrderByDescending(x => x.ListenCount)
                                .ToList()
                        })
                        .OrderByDescending(x => x.ListenCount)
                        .ToList();

                    ListenedPodcastCategories = categoryListenCounts;
                }

                // ============ Query ListenedPodcaster[] ============
                var (listenSessionsForPodcaster, listenDaysBackForPodcaster, distinctPodcasterCount) =
                    await QueryIncrementalWithMinDistinctAsync<PodcastEpisodeListenSession, int>(
                        daysBatchSize: 15,
                        minDistinctEntityCount: 2,
                        minDaysBack: 30,
                        maxDaysBack: 365,
                        predicateFactory: (startDate, endDate) => pes =>
                            pes.CreatedAt >= startDate &&
                            pes.CreatedAt < endDate &&
                            pes.IsContentRemoved == false &&
                            pes.PodcastEpisode.DeletedAt == null &&
                            pes.PodcastEpisode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault()
                            .PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Removed &&
                            pes.PodcastEpisode.PodcastShow.DeletedAt == null &&
                            pes.PodcastEpisode.PodcastShow.PodcastShowStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault()
                            .PodcastShowStatusId != (int)PodcastShowStatusEnum.Removed &&
                            (pes.PodcastEpisode.PodcastShow.PodcastChannel == null || (pes.PodcastEpisode.PodcastShow.PodcastChannel != null &&
                                pes.PodcastEpisode.PodcastShow.PodcastChannel.DeletedAt == null
                            )),
                        includeFunc:
                        q => q
                            .Include(pes => pes.PodcastEpisode)
                            .ThenInclude(pe => pe.PodcastShow),
                        distinctKeySelector: pes =>
                            pes.PodcastEpisode.PodcastShow.PodcasterId
                    );

                List<SystemListenedPodcaster> ListenedPodcasters = new List<SystemListenedPodcaster>();
                if (listenSessionsForPodcaster.Count > 0)
                {
                    var podcasterListenCounts = listenSessionsForPodcaster
                        .GroupBy(pes => pes.PodcastEpisode.PodcastShow.PodcasterId)
                        .Select(g => new SystemListenedPodcaster
                        {
                            PodcasterId = g.Key,
                            ListenCount = g.Count()
                        })
                        .OrderByDescending(x => x.ListenCount)
                        .ToList();

                    ListenedPodcasters = podcasterListenCounts;
                }

                // ============ Lưu vào cache ============
                var now = _dateHelper.GetNowByAppTimeZone();
                int finalDaysBack = Math.Max(listenDaysBack, listenDaysBackForPodcaster);
                var metric = new SystemPreferencesTemporal30dQueryMetric
                {
                    ListenedPodcastCategories = ListenedPodcastCategories,
                    ListenedPodcasters = ListenedPodcasters,
                    LastUpdated = now
                };
                await _redisSharedCacheService.KeySetAsync(cacheKey, metric, TimeSpan.FromSeconds(cacheTTL ?? 3600));
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new Exception("UpdateSystemPreferencesTemporal30dQueryMetric failed, error: " + ex.Message);
            }

        }

        public async Task UpdateUserPreferencesTemporal30dQueryMetric()
        {
            try
            {
                // điều kiện của listen session: isContentRemoved = false
                // Logic to update User Preferences temporal 30-day goes here
                // group by AccountId và đưa vào field UserId trong DTO
                // 1: lấy ListenedPodcastCategories[] 30 ngày tối thiểu về trước (entity phải 4 categories min nếu không đủ trong 30 ngày thì lấy quá đến khi đủ 4 categories) từ bảng PodcastEpisodeListenSession
                //  --> lấy nhiều object ListenedPodcastCategoriy có categoryId, listenCount, PodcastSubCategories (có subcategoryId, listenCount)
                // 2: lấy ListenedPodcaster[] 30 ngày tối thiểu về trước (entity phải 2 podcasters min nếu không đủ trong 30 ngày thì lấy quá đến khi đủ 2 podcasters) từ bảng PodcastEpisodeListenSession
                //  --> lấy nhiều object ListenedPodcaster có podcasterId, listenCount
                // 3: lấy ra LastUpdated là hiện tại
                // 4: lưu vào DTO sau đó đưa vào cache
                // 4: cập nhật vào cache key trong config của job

                var cacheKey = _jobsConfig.UserPreferencesTemporal30dQueryMetricUpdateJob.RedisKeyName;
                var cacheTTL = _jobsConfig.UserPreferencesTemporal30dQueryMetricUpdateJob.RedisKeyTTLSeconds;

                // ============ Bước 1: Query để lấy distinct users (không giới hạn số lượng user) ============
                var (allUserListenSessions, _, distinctUserCount) =
                    await QueryIncrementalWithMinDistinctAsync<PodcastEpisodeListenSession, int>(
                        daysBatchSize: 30,
                        minDistinctEntityCount: 1, // Lấy tất cả users, không giới hạn
                        minDaysBack: 30,
                        maxDaysBack: 365,
                        predicateFactory: (startDate, endDate) => pes =>
                            pes.CreatedAt >= startDate &&
                            pes.CreatedAt < endDate &&
                            pes.IsContentRemoved == false &&
                            pes.PodcastEpisode.DeletedAt == null &&
                            pes.PodcastEpisode.PodcastEpisodeStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault()
                            .PodcastEpisodeStatusId != (int)PodcastEpisodeStatusEnum.Removed &&
                            pes.PodcastEpisode.PodcastShow.DeletedAt == null &&
                            pes.PodcastEpisode.PodcastShow.PodcastShowStatusTrackings
                            .OrderByDescending(pet => pet.CreatedAt)
                            .FirstOrDefault()
                            .PodcastShowStatusId != (int)PodcastShowStatusEnum.Removed &&
                            (pes.PodcastEpisode.PodcastShow.PodcastChannel == null ||
                             (pes.PodcastEpisode.PodcastShow.PodcastChannel != null &&
                              pes.PodcastEpisode.PodcastShow.PodcastChannel.DeletedAt == null)),
                        includeFunc: q => q
                            .Include(pes => pes.PodcastEpisode)
                            .ThenInclude(pe => pe.PodcastShow),
                        distinctKeySelector: pes => pes.AccountId
                    );

                // Extract distinct user IDs
                var distinctUserIds = allUserListenSessions
                    .Select(pes => pes.AccountId)
                    .Distinct()
                    .ToList();

                Console.WriteLine($"[UserPreferencesTemporal30d] Found {distinctUserIds.Count} distinct users");

                // ============ Bước 2: Xử lý từng user ============
                var now = _dateHelper.GetNowByAppTimeZone();

                foreach (var userId in distinctUserIds)
                {
                    // Filter sessions của user này
                    var userSessions = allUserListenSessions
                        .Where(pes => pes.AccountId == userId)
                        .ToList();

                    // ============ Incremental filtering cho Categories (min 4) ============
                    int daysBatchSize = 15;
                    int currentDaysBackForCategory = 0;
                    int minDaysBack = 30;
                    int maxDaysBack = 365;

                    List<PodcastEpisodeListenSession> accumulatedCategorySessions = new List<PodcastEpisodeListenSession>();
                    int distinctCategoryCount = 0;

                    while (currentDaysBackForCategory < maxDaysBack)
                    {
                        var batchStartDate = now.AddDays(-(currentDaysBackForCategory + daysBatchSize));
                        var batchEndDate = now.AddDays(-currentDaysBackForCategory);

                        var batchSessions = userSessions
                            .Where(pes => pes.CreatedAt >= batchStartDate && pes.CreatedAt < batchEndDate)
                            .ToList();

                        if (batchSessions.Count == 0)
                            break;

                        accumulatedCategorySessions.AddRange(batchSessions);

                        distinctCategoryCount = accumulatedCategorySessions
                            .Where(pes => pes.PodcastCategoryId.HasValue)
                            .Select(pes => pes.PodcastCategoryId.Value)
                            .Distinct()
                            .Count();

                        currentDaysBackForCategory += daysBatchSize;

                        if (distinctCategoryCount >= 4 && currentDaysBackForCategory >= minDaysBack)
                            break;
                    }

                    // Tính ListenedPodcastCategories
                    var listenedCategories = accumulatedCategorySessions
                        .Where(pes => pes.PodcastCategoryId.HasValue)
                        .GroupBy(pes => pes.PodcastCategoryId.Value)
                        .Select(g => new UserListenedPodcastCategory
                        {
                            PodcastCategoryId = g.Key,
                            ListenCount = g.Count(),
                            PodcastSubCategories = g
                                .Where(pes => pes.PodcastSubCategoryId.HasValue)
                                .GroupBy(pes => pes.PodcastSubCategoryId.Value)
                                .Select(sg => new UserListenedPodcastSubCategory
                                {
                                    PodcastSubCategoryId = sg.Key,
                                    ListenCount = sg.Count()
                                })
                                .OrderByDescending(x => x.ListenCount)
                                .ToList()
                        })
                        .OrderByDescending(x => x.ListenCount)
                        .ToList();

                    // ============ Incremental filtering cho Podcasters (min 2) ============
                    int currentDaysBackForPodcaster = 0;
                    List<PodcastEpisodeListenSession> accumulatedPodcasterSessions = new List<PodcastEpisodeListenSession>();
                    int distinctPodcasterCount = 0;

                    while (currentDaysBackForPodcaster < maxDaysBack)
                    {
                        var batchStartDate = now.AddDays(-(currentDaysBackForPodcaster + daysBatchSize));
                        var batchEndDate = now.AddDays(-currentDaysBackForPodcaster);

                        var batchSessions = userSessions
                            .Where(pes => pes.CreatedAt >= batchStartDate && pes.CreatedAt < batchEndDate)
                            .ToList();

                        if (batchSessions.Count == 0)
                            break;

                        accumulatedPodcasterSessions.AddRange(batchSessions);

                        distinctPodcasterCount = accumulatedPodcasterSessions
                            .Select(pes => pes.PodcastEpisode.PodcastShow.PodcasterId)
                            .Distinct()
                            .Count();

                        currentDaysBackForPodcaster += daysBatchSize;

                        if (distinctPodcasterCount >= 2 && currentDaysBackForPodcaster >= minDaysBack)
                            break;
                    }

                    // Tính ListenedPodcasters
                    var listenedPodcasters = accumulatedPodcasterSessions
                        .GroupBy(pes => pes.PodcastEpisode.PodcastShow.PodcasterId)
                        .Select(g => new UserListenedPodcaster
                        {
                            PodcasterId = g.Key,
                            ListenCount = g.Count()
                        })
                        .OrderByDescending(x => x.ListenCount)
                        .ToList();

                    // ============ Lưu metric cho user ============
                    var userMetric = new UserPreferencesTemporal30dQueryMetric
                    {
                        UserId = userId,
                        ListenedPodcastCategories = listenedCategories,
                        ListenedPodcasters = listenedPodcasters,
                        LastUpdated = now
                    };

                    // Cache riêng cho từng user
                    await _redisSharedCacheService.KeySetAsync(cacheKey, userMetric, TimeSpan.FromSeconds(cacheTTL ?? 86400));

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.StackTrace + "\n");
                throw new Exception("UpdateUserPreferencesTemporal30dQueryMetric failed, error: " + ex.Message);
            }
        }
    }
}
