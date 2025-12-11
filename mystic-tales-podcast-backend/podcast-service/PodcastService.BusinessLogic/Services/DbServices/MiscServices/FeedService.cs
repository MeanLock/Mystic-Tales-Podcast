using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.DataAccess.Data;
using PodcastService.DataAccess.UOW;
using PodcastService.DataAccess.Repositories.interfaces;
using PodcastService.DataAccess.Entities.SqlServer;
using PodcastService.BusinessLogic.Helpers.AuthHelpers;
using PodcastService.BusinessLogic.Helpers.FileHelpers;
using PodcastService.BusinessLogic.Helpers.DateHelpers;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using PodcastService.BusinessLogic.Services.DbServices.CachingServices;
using PodcastService.Infrastructure.Services.Redis;
using PodcastService.BusinessLogic.DTOs.Feed;
using PodcastService.BusinessLogic.DTOs.Show.ListItems;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.Category.ListItems;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Episode;
using PodcastService.BusinessLogic.DTOs.Show;
using PodcastService.BusinessLogic.DTOs.Channel;
using PodcastService.BusinessLogic.DTOs.Hashtag;
using PodcastService.BusinessLogic.DTOs.Cache;
using PodcastService.BusinessLogic.DTOs.Cache.QueryMetric;
using PodcastService.BusinessLogic.Enums.Podcast;
using PodcastService.BusinessLogic.Models.CrossService;
using Newtonsoft.Json.Linq;
using PodcastService.BusinessLogic.DTOs.Episode.ListItems;
using PodcastService.BusinessLogic.DTOs.Feed.ListItems;
using System.Text;
using System.Globalization;

namespace PodcastService.BusinessLogic.Services.DbServices.PodcastServices
{
    public class FeedService
    {
        // LOGGER
        private readonly ILogger<FeedService> _logger;

        // CONFIG
        private readonly IAppConfig _appConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly IBackgroundJobsConfig _backgroundJobsConfig;

        // DB CONTEXT
        private readonly AppDbContext _appDbContext;

        // HELPERS
        private readonly DateHelper _dateHelper;

        // UNIT OF WORK
        private readonly IUnitOfWork _unitOfWork;

        // REPOSITORIES
        private readonly IGenericRepository<PodcastChannel> _podcastChannelGenericRepository;
        private readonly IGenericRepository<PodcastShow> _podcastShowGenericRepository;
        private readonly IGenericRepository<PodcastEpisode> _podcastEpisodeGenericRepository;
        private readonly IGenericRepository<PodcastEpisodeListenSession> _podcastEpisodeListenSessionGenericRepository;
        private readonly IGenericRepository<PodcastChannelHashtag> _podcastChannelHashtagGenericRepository;
        private readonly IGenericRepository<PodcastCategory> _podcastCategoryGenericRepository;
        private readonly IGenericRepository<PodcastSubCategory> _podcastSubCategoryGenericRepository;
        private readonly IGenericRepository<PodcastShowHashtag> _podcastShowHashtagGenericRepository;
        // SERVICES
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly AccountCachingService _accountCachingService;
        private readonly RedisSharedCacheService _redisSharedCacheService;

        public FeedService(
            ILogger<FeedService> logger,
            AppDbContext appDbContext,
            DateHelper dateHelper,
            IUnitOfWork unitOfWork,
            IGenericRepository<PodcastChannel> podcastChannelGenericRepository,
            IGenericRepository<PodcastShow> podcastShowGenericRepository,
            IGenericRepository<PodcastEpisode> podcastEpisodeGenericRepository,
            IGenericRepository<PodcastEpisodeListenSession> podcastEpisodeListenSessionGenericRepository,
            IGenericRepository<PodcastChannelHashtag> podcastChannelHashtagGenericRepository,
            IGenericRepository<PodcastCategory> podcastCategoryGenericRepository,
            IGenericRepository<PodcastSubCategory> podcastSubCategoryGenericRepository,
            IGenericRepository<PodcastShowHashtag> podcastShowHashtagGenericRepository,

            IFilePathConfig filePathConfig,
            IAppConfig appConfig,
            IBackgroundJobsConfig backgroundJobsConfig,
            HttpServiceQueryClient httpServiceQueryClient,
            AccountCachingService accountCachingService,
            RedisSharedCacheService redisSharedCacheService)
        {
            _logger = logger;
            _appDbContext = appDbContext;
            _dateHelper = dateHelper;
            _unitOfWork = unitOfWork;

            _podcastChannelGenericRepository = podcastChannelGenericRepository;
            _podcastShowGenericRepository = podcastShowGenericRepository;
            _podcastEpisodeGenericRepository = podcastEpisodeGenericRepository;
            _podcastEpisodeListenSessionGenericRepository = podcastEpisodeListenSessionGenericRepository;
            _podcastChannelHashtagGenericRepository = podcastChannelHashtagGenericRepository;
            _podcastCategoryGenericRepository = podcastCategoryGenericRepository;
            _podcastSubCategoryGenericRepository = podcastSubCategoryGenericRepository;
            _podcastShowHashtagGenericRepository = podcastShowHashtagGenericRepository;

            _filePathConfig = filePathConfig;
            _appConfig = appConfig;
            _backgroundJobsConfig = backgroundJobsConfig;
            _httpServiceQueryClient = httpServiceQueryClient;
            _accountCachingService = accountCachingService;
            _redisSharedCacheService = redisSharedCacheService;
        }

        #region Discovery Entry Point

        public async Task<DiscoveryPodcastFeedDTO> GetDiscoveryPodcastFeedContentsAsync(AccountStatusCache? account = null)
        {
            try
            {
                Console.WriteLine($"[GetDiscoveryPodcastFeed] Starting - Account: {(account != null ? $"UserId={account.Id}" : "Anonymous")}");

                // STEP 1: Load all cache metrics in parallel
                var (userPrefs, systemPrefs, cacheMetrics) = await LoadAllCacheMetricsAsync(account?.Id);

                // STEP 2: Initialize deduplication trackers
                var dedupShowIds = new HashSet<Guid>();
                var dedupChannelIds = new HashSet<Guid>();
                var dedupPodcasterIds = new HashSet<int>();

                // STEP 3: Build sections in order (following deduplication priority)
                var continueListening = await BuildDiscoveryContinueListeningSection(account?.Id);

                var basedOnYourTaste = await BuildDiscoveryBasedOnYourTasteSection(
                    account?.Id, userPrefs, systemPrefs, dedupShowIds);

                var newReleases = await BuildDiscoveryNewReleasesSection(
                    userPrefs, systemPrefs, dedupShowIds);

                var hotThisWeek = await BuildDiscoveryHotThisWeekSection(
                    cacheMetrics, dedupShowIds, dedupChannelIds);

                var topSubCategory = await BuildDiscoveryTopSubCategorySection(
                    account?.Id, userPrefs, systemPrefs, cacheMetrics, dedupShowIds);

                var topPodcasters = await BuildDiscoveryTopPodcastersSection(
                    cacheMetrics, dedupPodcasterIds);

                var randomCategory = await BuildDiscoveryRandomCategorySection(
                    userPrefs, systemPrefs, cacheMetrics, dedupShowIds);

                var talentedRookies = await BuildDiscoveryTalentedRookiesSection(
                    cacheMetrics, dedupPodcasterIds);

                Console.WriteLine("[GetDiscoveryPodcastFeed] Completed successfully");

                return new DiscoveryPodcastFeedDTO
                {
                    ContinueListening = continueListening ?? new DiscoveryPodcastFeedDTO.ContinueListeningDiscoveryPodcastFeedSection { ListenSessionList = new List<DiscoveryPodcastFeedDTO.ListenSessionDiscoveryPodcastFeedListItem>() },
                    BasedOnYourTaste = basedOnYourTaste ?? new DiscoveryPodcastFeedDTO.BasedOnYourTasteDiscoveryPodcastFeedSection { ShowList = new List<ShowListItemResponseDTO>() },
                    NewReleases = newReleases ?? new DiscoveryPodcastFeedDTO.NewReleasesDiscoveryPodcastFeedSection { ShowList = new List<ShowListItemResponseDTO>() },
                    HotThisWeek = hotThisWeek ?? new DiscoveryPodcastFeedDTO.HotThisWeekDiscoveryPodcastFeedSection { ShowList = new List<ShowListItemResponseDTO>(), ChannelList = new List<ChannelListItemResponseDTO>() },
                    TopSubCategory = topSubCategory ?? new DiscoveryPodcastFeedDTO.TopSubCategoryDiscoveryPodcastFeedSection { ShowList = new List<ShowListItemResponseDTO>() },
                    TopPodcasters = topPodcasters ?? new DiscoveryPodcastFeedDTO.TopPodcastersDiscoveryPodcastFeedSection { PodcasterList = new List<AccountSnippetResponseDTO>() },
                    RandomCategory = randomCategory ?? new DiscoveryPodcastFeedDTO.RandomCategoryDiscoveryPodcastFeedSection { PodcastCategory = new PodcastCategoryDTO(), ShowList = new List<ShowListItemResponseDTO>() },
                    TalentedRookies = talentedRookies ?? new DiscoveryPodcastFeedDTO.TalentedRookiesDiscoveryPodcastFeedSection { PodcasterList = new List<AccountSnippetResponseDTO>() }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[GetDiscoveryPodcastFeed] ERROR: {ex.Message}\n{ex.StackTrace}\n");
                throw new HttpRequestException("Error while getting discovery podcast feed contents: " + ex.Message);
            }
        }

        #endregion

        #region Cache Loading

        private async Task<(
            UserPreferencesTemporal30dQueryMetric? userPrefs,
            SystemPreferencesTemporal30dQueryMetric? systemPrefs,
            CacheMetricsContainer cacheMetrics
        )> LoadAllCacheMetricsAsync(int? userId)
        {
            Console.WriteLine("[LoadAllCacheMetrics] Loading all cache metrics in parallel...");

            var tasks = new List<Task>();
            UserPreferencesTemporal30dQueryMetric? userPrefs = null;
            SystemPreferencesTemporal30dQueryMetric? systemPrefs = null;
            PodcasterAllTimeMaxQueryMetric? podcasterAllTime = null;
            PodcasterTemporal7dMaxQueryMetric? podcasterTemporal = null;
            ShowAllTimeMaxQueryMetric? showAllTime = null;
            ShowTemporal7dMaxQueryMetric? showTemporal = null;
            ChannelAllTimeMaxQueryMetric? channelAllTime = null;
            ChannelTemporal7dMaxQueryMetric? channelTemporal = null;
            EpisodeAllTimeMaxQueryMetric? episodeAllTime = null;

            // User preferences (if logged in)
            if (userId.HasValue)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var cacheKey = _backgroundJobsConfig.UserPreferencesTemporal30dQueryMetricUpdateJob.RedisKeyName;
                    var allUserPrefs = await _redisSharedCacheService
                        .KeyGetAsync<List<UserPreferencesTemporal30dQueryMetric>>(cacheKey);
                    userPrefs = allUserPrefs?.FirstOrDefault(up => up.UserId == userId.Value);
                }));
            }

            // System preferences
            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = _backgroundJobsConfig.SystemPreferencesTemporal30dQueryMetricUpdateJob.RedisKeyName;
                systemPrefs = await _redisSharedCacheService
                    .KeyGetAsync<SystemPreferencesTemporal30dQueryMetric>(cacheKey);
            }));

            // All-time metrics
            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = _backgroundJobsConfig.PodcasterAllTimeMaxQueryMetricUpdateJob.RedisKeyName;
                podcasterAllTime = await _redisSharedCacheService
                    .KeyGetAsync<PodcasterAllTimeMaxQueryMetric>(cacheKey);
            }));

            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = _backgroundJobsConfig.ShowAllTimeMaxQueryMetricUpdateJob.RedisKeyName;
                showAllTime = await _redisSharedCacheService
                    .KeyGetAsync<ShowAllTimeMaxQueryMetric>(cacheKey);
            }));

            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = _backgroundJobsConfig.ChannelAllTimeMaxQueryMetricUpdateJob.RedisKeyName;
                channelAllTime = await _redisSharedCacheService
                    .KeyGetAsync<ChannelAllTimeMaxQueryMetric>(cacheKey);
            }));

            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = _backgroundJobsConfig.EpisodeAllTimeMaxQueryMetricUpdateJob.RedisKeyName;
                episodeAllTime = await _redisSharedCacheService
                    .KeyGetAsync<EpisodeAllTimeMaxQueryMetric>(cacheKey);
            }));

            // Temporal 7d metrics
            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = _backgroundJobsConfig.PodcasterTemporal7dMaxQueryMetricUpdateJob.RedisKeyName;
                podcasterTemporal = await _redisSharedCacheService
                    .KeyGetAsync<PodcasterTemporal7dMaxQueryMetric>(cacheKey);
            }));

            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = _backgroundJobsConfig.ShowTemporal7dMaxQueryMetricUpdateJob.RedisKeyName;
                showTemporal = await _redisSharedCacheService
                    .KeyGetAsync<ShowTemporal7dMaxQueryMetric>(cacheKey);
            }));

            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = _backgroundJobsConfig.ChannelTemporal7dMaxQueryMetricUpdateJob.RedisKeyName;
                channelTemporal = await _redisSharedCacheService
                    .KeyGetAsync<ChannelTemporal7dMaxQueryMetric>(cacheKey);
            }));

            await Task.WhenAll(tasks);

            var cacheMetrics = new CacheMetricsContainer
            {
                PodcasterAllTime = podcasterAllTime,
                PodcasterTemporal = podcasterTemporal,
                ShowAllTime = showAllTime,
                ShowTemporal = showTemporal,
                ChannelAllTime = channelAllTime,
                ChannelTemporal = channelTemporal,
                EpisodeAllTime = episodeAllTime
            };

            Console.WriteLine($"[LoadAllCacheMetrics] Loaded - UserPrefs: {userPrefs != null}, SystemPrefs: {systemPrefs != null}");
            return (userPrefs, systemPrefs, cacheMetrics);
        }

        #endregion

        #region Discovery Section 1: Continue Listening

        private async Task<DiscoveryPodcastFeedDTO.ContinueListeningDiscoveryPodcastFeedSection?> BuildDiscoveryContinueListeningSection(int? userId)
        {
            try
            {
                if (!userId.HasValue)
                {
                    Console.WriteLine("[ContinueListening] Skipped - Anonymous user");
                    return null;
                }

                Console.WriteLine($"[ContinueListening] Building for userId={userId}");

                var listenSessions = await _podcastEpisodeListenSessionGenericRepository.FindAll(
                    predicate: ls =>
                        ls.AccountId == userId.Value &&
                        // ls.IsCompleted == false &&
                        ls.LastListenDurationSeconds < ls.PodcastEpisode.AudioLength &&
                        ls.IsContentRemoved == false &&
                        ls.PodcastEpisode.DeletedAt == null &&
                        ls.PodcastEpisode.PodcastShow.DeletedAt == null &&
                        (ls.PodcastEpisode.PodcastShow.PodcastChannel == null || ls.PodcastEpisode.PodcastShow.PodcastChannel.DeletedAt == null),
                    includeFunc: q => q
                        .Include(ls => ls.PodcastEpisode)
                            .ThenInclude(pe => pe.PodcastShow)
                        .Include(ls => ls.PodcastEpisode)
                            .ThenInclude(pe => pe.PodcastEpisodeStatusTrackings)
                        .Include(ls => ls.PodcastEpisode)
                            .ThenInclude(pe => pe.PodcastShow)
                            .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(ls => ls.PodcastEpisode)
                            .ThenInclude(pe => pe.PodcastShow)
                            .ThenInclude(ps => ps.PodcastChannel)
                            .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                        .OrderByDescending(ls => ls.CreatedAt)
                        .Take(10)
                ).ToListAsync();

                // Filter by current status
                listenSessions = listenSessions.Where(ls =>
                {
                    var episodeCurrentStatus = ls.PodcastEpisode.PodcastEpisodeStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastEpisodeStatusId;

                    var showCurrentStatus = ls.PodcastEpisode.PodcastShow.PodcastShowStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastShowStatusId;

                    var channelCurrentStatus = ls.PodcastEpisode.PodcastShow.PodcastChannel == null ? null : ls.PodcastEpisode.PodcastShow.PodcastChannel.PodcastChannelStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastChannelStatusId;

                    return episodeCurrentStatus == (int)PodcastEpisodeStatusEnum.Published &&
                           showCurrentStatus == (int)PodcastShowStatusEnum.Published &&
                            (channelCurrentStatus == null || channelCurrentStatus == (int)PodcastChannelStatusEnum.Published)
                           ;
                }).ToList();

                if (!listenSessions.Any())
                {
                    Console.WriteLine("[ContinueListening] No incomplete sessions found");
                    return null;
                }

                // distint and order by last episode listened id
                listenSessions = listenSessions
                    .GroupBy(ls => ls.PodcastEpisodeId)
                    .Select(g => g.OrderByDescending(ls => ls.CreatedAt).First())
                    .OrderByDescending(ls => ls.CreatedAt)
                    .ToList();

                // Get unique podcaster IDs
                var podcasterIds = listenSessions
                    .Select(ls => ls.PodcastEpisode.PodcastShow.PodcasterId)
                    // .Select(ls => ls.PodcastEpisodeId)
                    .Distinct()
                    .ToList();

                // Fetch all podcaster accounts
                var podcasterAccounts = new Dictionary<int, AccountStatusCache>();
                foreach (var podcasterId in podcasterIds)
                {
                    var account = await _accountCachingService.GetAccountStatusCacheById(podcasterId);
                    if (account != null && account.DeactivatedAt == null && account.HasVerifiedPodcasterProfile == true)
                    {
                        podcasterAccounts[podcasterId] = account;
                    }
                }

                var listItems = listenSessions.Select(ls =>
                {
                    var podcasterId = ls.PodcastEpisode.PodcastShow.PodcasterId;
                    var podcasterAccount = podcasterAccounts.ContainsKey(podcasterId)
                        ? podcasterAccounts[podcasterId]
                        : null;

                    return new DiscoveryPodcastFeedDTO.ListenSessionDiscoveryPodcastFeedListItem
                    {
                        Episode = new PodcastEpisodeSnippetResponseDTO
                        {
                            Id = ls.PodcastEpisode.Id,
                            Name = ls.PodcastEpisode.Name,
                            Description = ls.PodcastEpisode.Description,
                            MainImageFileKey = ls.PodcastEpisode.MainImageFileKey,
                            IsReleased = ls.PodcastEpisode.IsReleased,
                            ReleaseDate = ls.PodcastEpisode.ReleaseDate,
                            AudioLength = ls.PodcastEpisode.AudioLength
                        },
                        Podcaster = podcasterAccount != null ? new AccountSnippetResponseDTO
                        {
                            Id = podcasterAccount.Id,
                            FullName = podcasterAccount.PodcasterProfileName ?? podcasterAccount.FullName,
                            Email = podcasterAccount.Email,
                            MainImageFileKey = podcasterAccount.MainImageFileKey
                        } : null,
                        PodcastEpisodeListenSession = new PodcastEpisodeListenSessionSnippetResponseDTO
                        {
                            Id = ls.Id,
                            LastListenDurationSeconds = ls.LastListenDurationSeconds
                        }
                    };
                }).ToList();

                Console.WriteLine($"[ContinueListening] Built with {listItems.Count} items");

                return new DiscoveryPodcastFeedDTO.ContinueListeningDiscoveryPodcastFeedSection
                {
                    ListenSessionList = listItems
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ContinueListening] ERROR: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Discovery Section 2: Based On Your Taste

        private async Task<DiscoveryPodcastFeedDTO.BasedOnYourTasteDiscoveryPodcastFeedSection?> BuildDiscoveryBasedOnYourTasteSection(
    int? userId,
    UserPreferencesTemporal30dQueryMetric? userPrefs,
    SystemPreferencesTemporal30dQueryMetric? systemPrefs,
    HashSet<Guid> dedupShowIds)
        {
            try
            {
                Console.WriteLine("[BasedOnYourTaste] Building section...");

                var partAShows = new List<PodcastShow>();

                // Part A: From user top categories (75% = 9 shows)
                if (userPrefs?.ListenedPodcastCategories != null && userPrefs.ListenedPodcastCategories.Any())
                {
                    // var topCategories = userPrefs.ListenedPodcastCategories.Take(4).ToList();
                    // lấy top 4 categories theo listenCount của nó
                    var topCategories = userPrefs.ListenedPodcastCategories
                        .OrderByDescending(c => c.ListenCount)
                        .Take(4)
                        .ToList();
                    foreach (var category in topCategories)
                    {
                        if (partAShows.Count >= 9) break; // Đủ rồi thì dừng

                        // Query tất cả shows trong category
                        var shows = await _podcastShowGenericRepository.FindAll(
                                                    predicate: ps =>
                                                        ps.PodcastCategoryId == category.PodcastCategoryId &&
                                                        ps.DeletedAt == null,
                                                    includeFunc: q => q.Include(ps => ps.PodcastShowStatusTrackings)
                                                    .Include(ps => ps.PodcastChannel)
                .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                                                ).ToListAsync();

                        // Filter by current Published status
                        var publishedShows = shows.Where(ps =>
                        {
                            var currentStatus = ps.PodcastShowStatusTrackings
                                .OrderByDescending(t => t.CreatedAt)
                                .FirstOrDefault()?.PodcastShowStatusId;
                            return currentStatus == (int)PodcastShowStatusEnum.Published &&
                            (ps.PodcastChannel == null || (ps.PodcastChannel.DeletedAt == null && ps.PodcastChannel.PodcastChannelStatusTrackings
                                .OrderByDescending(t => t.CreatedAt)
                                .FirstOrDefault()?.PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published));
                        }).ToList();

                        // FIX: Random và take đủ số lượng cần
                        var needed = Math.Min(3, 9 - partAShows.Count);
                        var selectedShows = publishedShows
                            .OrderBy(x => Guid.NewGuid())
                            .Take(needed)
                            .ToList();

                        partAShows.AddRange(selectedShows);
                    }
                }

                // Fallback Part C: System categories nếu chưa đủ 9
                if (partAShows.Count < 9 && systemPrefs?.ListenedPodcastCategories != null)
                {
                    var existingShowIds = partAShows.Select(s => s.Id).ToHashSet();
                    // var systemCategories = systemPrefs.ListenedPodcastCategories.Take(4).ToList();
                    // lấy top 4 categories theo listenCount của nó
                    var systemCategories = systemPrefs.ListenedPodcastCategories
                        .OrderByDescending(c => c.ListenCount)
                        .Take(4)
                        .ToList();

                    foreach (var category in systemCategories)
                    {
                        if (partAShows.Count >= 9) break;

                        var shows = await _podcastShowGenericRepository.FindAll(
                            predicate: ps =>
                                ps.PodcastCategoryId == category.PodcastCategoryId &&
                                !existingShowIds.Contains(ps.Id) &&
                                ps.DeletedAt == null,
                            includeFunc: q => q.Include(ps => ps.PodcastShowStatusTrackings)
                            .Include(ps => ps.PodcastChannel)
                .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                        ).ToListAsync();

                        var publishedShows = shows.Where(ps =>
                        {
                            var currentStatus = ps.PodcastShowStatusTrackings
                                .OrderByDescending(t => t.CreatedAt)
                                .FirstOrDefault()?.PodcastShowStatusId;
                            return currentStatus == (int)PodcastShowStatusEnum.Published &&
                            (ps.PodcastChannel == null || (ps.PodcastChannel.DeletedAt == null && ps.PodcastChannel.PodcastChannelStatusTrackings
                                .OrderByDescending(t => t.CreatedAt)
                                .FirstOrDefault()?.PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published));
                        }).ToList();

                        var needed = Math.Min(3, 9 - partAShows.Count);
                        var selectedShows = publishedShows
                            .OrderBy(x => Guid.NewGuid())
                            .Take(needed)
                            .ToList();

                        partAShows.AddRange(selectedShows);
                        existingShowIds.UnionWith(selectedShows.Select(s => s.Id));
                    }
                }

                // Part B: From user top podcasters (25% = 3 shows)
                var partBShows = new List<PodcastShow>();
                if (userPrefs?.ListenedPodcasters != null && userPrefs.ListenedPodcasters.Any())
                {
                    var existingShowIds = partAShows.Select(s => s.Id).ToHashSet();
                    // var topPodcasters = userPrefs.ListenedPodcasters.Take(2).ToList();
                    // lấy top 2 podcasters theo listenCount của nó
                    var topPodcasters = userPrefs.ListenedPodcasters
                        .OrderByDescending(p => p.ListenCount)
                        .Take(2)
                        .ToList();

                    foreach (var podcaster in topPodcasters)
                    {
                        if (partBShows.Count >= 3) break;

                        var shows = await _podcastShowGenericRepository.FindAll(
                            predicate: ps =>
                                ps.PodcasterId == podcaster.PodcasterId &&
                                !existingShowIds.Contains(ps.Id) &&
                                ps.DeletedAt == null,
                            includeFunc: q => q.Include(ps => ps.PodcastShowStatusTrackings)
                            .Include(ps => ps.PodcastChannel)
                .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                        ).ToListAsync();

                        var publishedShows = shows.Where(ps =>
                        {
                            var currentStatus = ps.PodcastShowStatusTrackings
                                .OrderByDescending(t => t.CreatedAt)
                                .FirstOrDefault()?.PodcastShowStatusId;
                            return currentStatus == (int)PodcastShowStatusEnum.Published &&
                            (ps.PodcastChannel == null || (ps.PodcastChannel.DeletedAt == null && ps.PodcastChannel.PodcastChannelStatusTrackings
                                .OrderByDescending(t => t.CreatedAt)
                                .FirstOrDefault()?.PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published));
                        }).ToList();

                        var needed = Math.Min(2, 3 - partBShows.Count);
                        var selectedShows = publishedShows
                            .OrderBy(x => Guid.NewGuid())
                            .Take(needed)
                            .ToList();

                        partBShows.AddRange(selectedShows);
                        existingShowIds.UnionWith(selectedShows.Select(s => s.Id));
                    }
                }

                // Fallback Part D: System podcasters nếu chưa đủ 3
                if (partBShows.Count < 3 && systemPrefs?.ListenedPodcasters != null)
                {
                    var existingShowIds = partAShows.Concat(partBShows).Select(s => s.Id).ToHashSet();
                    // var systemPodcasters = systemPrefs.ListenedPodcasters.Take(2).ToList();
                    var systemPodcasters = systemPrefs.ListenedPodcasters
                        .OrderByDescending(p => p.ListenCount)
                        .Take(2)
                        .ToList();

                    foreach (var podcaster in systemPodcasters)
                    {
                        if (partBShows.Count >= 3) break;

                        var shows = await _podcastShowGenericRepository.FindAll(
                            predicate: ps =>
                                ps.PodcasterId == podcaster.PodcasterId &&
                                !existingShowIds.Contains(ps.Id) &&
                                ps.DeletedAt == null,
                            includeFunc: q => q.Include(ps => ps.PodcastShowStatusTrackings)
                            .Include(ps => ps.PodcastChannel)
                .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                        ).ToListAsync();

                        var publishedShows = shows.Where(ps =>
                        {
                            var currentStatus = ps.PodcastShowStatusTrackings
                                .OrderByDescending(t => t.CreatedAt)
                                .FirstOrDefault()?.PodcastShowStatusId;
                            return currentStatus == (int)PodcastShowStatusEnum.Published &&
                            (ps.PodcastChannel == null || (ps.PodcastChannel.DeletedAt == null && ps.PodcastChannel.PodcastChannelStatusTrackings
                                .OrderByDescending(t => t.CreatedAt)
                                .FirstOrDefault()?.PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published));
                        }).ToList();

                        var needed = Math.Min(2, 3 - partBShows.Count);
                        var selectedShows = publishedShows
                            .OrderBy(x => Guid.NewGuid())
                            .Take(needed)
                            .ToList();

                        partBShows.AddRange(selectedShows);
                        existingShowIds.UnionWith(selectedShows.Select(s => s.Id));
                    }
                }

                var finalShows = partAShows.Concat(partBShows).Take(12).ToList();

                // Update deduplication tracker
                foreach (var show in finalShows)
                {
                    dedupShowIds.Add(show.Id);
                }

                var showListItems = await MapToShowListItemsAsync(finalShows);

                Console.WriteLine($"[BasedOnYourTaste] Built with {showListItems.Count} shows (A:{partAShows.Count}, B:{partBShows.Count})");

                return new DiscoveryPodcastFeedDTO.BasedOnYourTasteDiscoveryPodcastFeedSection
                {
                    ShowList = showListItems
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BasedOnYourTaste] ERROR: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Discovery Section 3: New Releases

        private async Task<DiscoveryPodcastFeedDTO.NewReleasesDiscoveryPodcastFeedSection?> BuildDiscoveryNewReleasesSection(
    UserPreferencesTemporal30dQueryMetric? userPrefs,
    SystemPreferencesTemporal30dQueryMetric? systemPrefs,
    HashSet<Guid> dedupShowIds)
        {
            try
            {
                Console.WriteLine("[NewReleases] Building section...");

                var now = _dateHelper.GetNowByAppTimeZone();
                var twoDaysAgo = now.AddDays(-2);

                var partAShows = new List<PodcastShow>();

                // Part A: From user top podcasters (50% = 5 shows)
                if (userPrefs?.ListenedPodcasters != null && userPrefs.ListenedPodcasters.Any())
                {
                    var topPodcasters = userPrefs.ListenedPodcasters
                        .OrderByDescending(p => p.ListenCount)
                        .Take(2)
                        .ToList();

                    foreach (var podcaster in topPodcasters)
                    {
                        if (partAShows.Count >= 5) break;

                        var shows = await _podcastShowGenericRepository.FindAll(
                            predicate: ps =>
                                ps.PodcasterId == podcaster.PodcasterId &&
                                !dedupShowIds.Contains(ps.Id) &&
                                ps.DeletedAt == null,
                            includeFunc: q => q.Include(ps => ps.PodcastShowStatusTrackings)
                            .Include(ps => ps.PodcastChannel)
                .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                        ).ToListAsync();

                        // Filter shows that became Published in last 2 days
                        var recentlyPublishedShows = shows.Where(ps =>
                        {
                            var publishedTracking = ps.PodcastShowStatusTrackings
                                .Where(t => t.PodcastShowStatusId == (int)PodcastShowStatusEnum.Published)
                                .OrderByDescending(t => t.CreatedAt)
                                .FirstOrDefault();

                            return publishedTracking != null && publishedTracking.CreatedAt >= twoDaysAgo &&
                            (ps.PodcastChannel == null || (ps.PodcastChannel.DeletedAt == null && ps.PodcastChannel.PodcastChannelStatusTrackings
                                .OrderByDescending(t => t.CreatedAt)
                                .FirstOrDefault()?.PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published));
                        })
                        .OrderByDescending(ps => ps.PodcastShowStatusTrackings
                            .Where(t => t.PodcastShowStatusId == (int)PodcastShowStatusEnum.Published)
                            .Max(t => t.CreatedAt))
                        .ToList();

                        // FIX: Take đủ số lượng cần
                        var needed = Math.Min(recentlyPublishedShows.Count, 5 - partAShows.Count);
                        partAShows.AddRange(recentlyPublishedShows.Take(needed));
                    }
                }

                // Part B: System-wide new releases (bù đủ 10 total)
                var existingShowIds = partAShows.Select(s => s.Id).ToHashSet();
                existingShowIds.UnionWith(dedupShowIds);

                var allShows = await _podcastShowGenericRepository.FindAll(
                    predicate: ps =>
                        !existingShowIds.Contains(ps.Id) &&
                        ps.DeletedAt == null,
                    includeFunc: q => q.Include(ps => ps.PodcastShowStatusTrackings)
                    .Include(ps => ps.PodcastChannel)
                .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                ).ToListAsync();

                var partBShows = allShows.Where(ps =>
                {
                    var publishedTracking = ps.PodcastShowStatusTrackings
                        .Where(t => t.PodcastShowStatusId == (int)PodcastShowStatusEnum.Published)
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault();

                    return publishedTracking != null && publishedTracking.CreatedAt >= twoDaysAgo &&
                    (ps.PodcastChannel == null || (ps.PodcastChannel.DeletedAt == null && ps.PodcastChannel.PodcastChannelStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published));
                })
                .OrderByDescending(ps => ps.PodcastShowStatusTrackings
                    .Where(t => t.PodcastShowStatusId == (int)PodcastShowStatusEnum.Published)
                    .Max(t => t.CreatedAt))
                .Take(10 - partAShows.Count) // FIX: Bù chính xác số còn thiếu
                .ToList();

                var finalShows = partAShows.Concat(partBShows).Take(10).ToList();

                // Update deduplication
                foreach (var show in finalShows)
                {
                    dedupShowIds.Add(show.Id);
                }

                var showListItems = await MapToShowListItemsAsync(finalShows);

                Console.WriteLine($"[NewReleases] Built with {showListItems.Count} shows (A:{partAShows.Count}, B:{partBShows.Count})");

                return new DiscoveryPodcastFeedDTO.NewReleasesDiscoveryPodcastFeedSection
                {
                    ShowList = showListItems
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NewReleases] ERROR: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region Discovery Section 4: Hot This Week

        private async Task<DiscoveryPodcastFeedDTO.HotThisWeekDiscoveryPodcastFeedSection?> BuildDiscoveryHotThisWeekSection(
            CacheMetricsContainer cacheMetrics,
            HashSet<Guid> dedupShowIds,
            HashSet<Guid> dedupChannelIds)
        {
            try
            {
                Console.WriteLine("[HotThisWeek] Building section...");

                // Fetch all shows
                var allShows = await _podcastShowGenericRepository.FindAll(
                    predicate: ps =>
                        ps.DeletedAt == null &&
                        !dedupShowIds.Contains(ps.Id),
                    includeFunc: q => q
                        .Include(ps => ps.PodcastShowStatusTrackings)
                        .Include(ps => ps.PodcastChannel)
                            .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                ).ToListAsync();

                // Filter by current Published status
                allShows = allShows.Where(ps =>
                {
                    var currentStatus = ps.PodcastShowStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastShowStatusId;
                    return currentStatus == (int)PodcastShowStatusEnum.Published &&
                    (ps.PodcastChannel == null || (ps.PodcastChannel.DeletedAt == null && ps.PodcastChannel.PodcastChannelStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published));
                }).ToList();

                // Fetch all channels
                var allChannels = await _podcastChannelGenericRepository.FindAll(
                    predicate: pc =>
                        pc.DeletedAt == null &&
                        !dedupChannelIds.Contains(pc.Id),
                    includeFunc: q => q.Include(pc => pc.PodcastChannelStatusTrackings)
                ).ToListAsync();

                // Filter by current Published status
                allChannels = allChannels.Where(pc =>
                {
                    var currentStatus = pc.PodcastChannelStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastChannelStatusId;
                    return currentStatus == (int)PodcastChannelStatusEnum.Published;
                }).ToList();

                // Calculate hot scores (7d)
                var showsWithHotScore = await CalculateShowHotScoresAsync(allShows, cacheMetrics.ShowTemporal);
                var channelsWithHotScore = await CalculateChannelHotScoresAsync(allChannels, cacheMetrics.ChannelTemporal);

                // Part A (80%): 8 hot shows + 4 hot channels
                var partAShows = showsWithHotScore
                    .OrderByDescending(x => x.score)
                    .Take(8)
                    .Select(x => x.show)
                    .ToList();

                var partAChannels = channelsWithHotScore
                    .OrderByDescending(x => x.score)
                    .Take(4)
                    .Select(x => x.channel)
                    .ToList();

                // Calculate popular scores (all-time) for Part B
                var showsWithPopularScore = CalculateShowPopularScores(allShows, cacheMetrics.ShowAllTime);
                var channelsWithPopularScore = CalculateChannelPopularScores(allChannels, cacheMetrics.ChannelAllTime);

                var partAShowIds = partAShows.Select(s => s.Id).ToHashSet();
                var partAChannelIds = partAChannels.Select(c => c.Id).ToHashSet();

                // Part B (20%): 2 popular shows + 1 popular channel
                var partBShows = showsWithPopularScore
                    .Where(x => !partAShowIds.Contains(x.show.Id))
                    .OrderByDescending(x => x.score)
                    .Take(2)
                    .Select(x => x.show)
                    .ToList();

                var partBChannels = channelsWithPopularScore
                    .Where(x => !partAChannelIds.Contains(x.channel.Id))
                    .OrderByDescending(x => x.score)
                    .Take(1)
                    .Select(x => x.channel)
                    .ToList();

                var finalShows = partAShows.Concat(partBShows).ToList();
                var finalChannels = partAChannels.Concat(partBChannels).ToList();

                // Update deduplication
                foreach (var show in finalShows) dedupShowIds.Add(show.Id);
                foreach (var channel in finalChannels) dedupChannelIds.Add(channel.Id);

                var showListItems = await MapToShowListItemsAsync(finalShows);
                var channelListItems = await MapToChannelListItemsAsync(finalChannels);

                Console.WriteLine($"[HotThisWeek] Built with {showListItems.Count} shows, {channelListItems.Count} channels");

                return new DiscoveryPodcastFeedDTO.HotThisWeekDiscoveryPodcastFeedSection
                {
                    ShowList = showListItems,
                    ChannelList = channelListItems
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HotThisWeek] ERROR: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Discovery Section 5: Top SubCategory

        private async Task<DiscoveryPodcastFeedDTO.TopSubCategoryDiscoveryPodcastFeedSection?> BuildDiscoveryTopSubCategorySection(
            int? userId,
            UserPreferencesTemporal30dQueryMetric? userPrefs,
            SystemPreferencesTemporal30dQueryMetric? systemPrefs,
            CacheMetricsContainer cacheMetrics,
            HashSet<Guid> dedupShowIds)
        {
            try
            {
                Console.WriteLine("[TopSubCategory] Building section...");

                // Determine top subcategory
                int? targetSubCategoryId = null;
                int? targetCategoryId = null;

                if (userPrefs?.ListenedPodcastCategories != null && userPrefs.ListenedPodcastCategories.Any())
                {
                    // var topCategory = userPrefs.ListenedPodcastCategories.First();
                    // lấy top category theo listenCount của nó
                    var topCategory = userPrefs.ListenedPodcastCategories
                        .OrderByDescending(c => c.ListenCount)
                        .First();
                    targetCategoryId = topCategory.PodcastCategoryId;
                    if (topCategory.PodcastSubCategories != null && topCategory.PodcastSubCategories.Any())
                    {
                        targetSubCategoryId = topCategory.PodcastSubCategories.First().PodcastSubCategoryId;
                    }
                }

                // Fallback to system
                if (!targetSubCategoryId.HasValue && systemPrefs?.ListenedPodcastCategories != null && systemPrefs.ListenedPodcastCategories.Any())
                {
                    // var topCategory = systemPrefs.ListenedPodcastCategories.First();
                    // lấy top category theo listenCount của nó
                    var topCategory = systemPrefs.ListenedPodcastCategories
                        .OrderByDescending(c => c.ListenCount)
                        .First();
                    targetCategoryId = topCategory.PodcastCategoryId;
                    if (topCategory.PodcastSubCategories != null && topCategory.PodcastSubCategories.Any())
                    {
                        targetSubCategoryId = topCategory.PodcastSubCategories.First().PodcastSubCategoryId;
                    }
                }

                if (!targetSubCategoryId.HasValue)
                {
                    Console.WriteLine("[TopSubCategory] No subcategory found");
                    return null;
                }

                // Fetch subcategory entity
                var subCategory = await _podcastSubCategoryGenericRepository.FindAll(
                    predicate: psc => psc.Id == targetSubCategoryId.Value,
                    includeFunc: q => q.Include(psc => psc.PodcastCategory)
                ).FirstOrDefaultAsync();

                if (subCategory == null)
                {
                    Console.WriteLine("[TopSubCategory] Subcategory not found in DB");
                    return null;
                }

                // Fetch shows in subcategory
                var shows = await _podcastShowGenericRepository.FindAll(
                    predicate: ps =>
                        ps.PodcastSubCategoryId == targetSubCategoryId.Value &&
                        !dedupShowIds.Contains(ps.Id) &&
                        ps.DeletedAt == null,
                    includeFunc: q => q.Include(ps => ps.PodcastShowStatusTrackings)
                        .Include(ps => ps.PodcastChannel)
                .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                ).ToListAsync();

                // Filter by current Published status
                shows = shows.Where(ps =>
                {
                    var currentStatus = ps.PodcastShowStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastShowStatusId;
                    return currentStatus == (int)PodcastShowStatusEnum.Published &&
                    (ps.PodcastChannel == null || (ps.PodcastChannel.DeletedAt == null && ps.PodcastChannel.PodcastChannelStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published));
                }).ToList();

                // Part A (80%): 8 personal shows
                var showsWithPersonalScore = userId.HasValue
                    ? await CalculatePersonalShowScoresAsync(shows, userId.Value)
                    : shows.Select(s => (show: s, score: 0.0)).ToList();

                var partAShows = showsWithPersonalScore
                    .Where(x => x.score > 0)
                    .OrderByDescending(x => x.score)
                    .Take(8)
                    .Select(x => x.show)
                    .ToList();

                // Part B (20%): Popular shows to fill deficit
                var needed = 8 - partAShows.Count;
                var partBShows = new List<PodcastShow>();

                if (needed > 0)
                {
                    var existingIds = partAShows.Select(s => s.Id).ToHashSet();
                    var showsWithPopularScore = CalculateShowPopularScores(
                        shows.Where(s => !existingIds.Contains(s.Id)).ToList(),
                        cacheMetrics.ShowAllTime
                    );

                    partBShows = showsWithPopularScore
                        .OrderByDescending(x => x.score)
                        .Take(2 + needed)
                        .Select(x => x.show)
                        .ToList();
                }

                var finalShows = partAShows.Concat(partBShows.Take(2 + needed)).Take(10).ToList();

                // Update deduplication
                foreach (var show in finalShows) dedupShowIds.Add(show.Id);

                var showListItems = await MapToShowListItemsAsync(finalShows);

                var subCategoryDTO = new PodcastSubCategoryListItemResponseDTO
                {
                    Id = subCategory.Id,
                    Name = subCategory.Name,
                    PodcastCategoryId = subCategory.PodcastCategoryId,
                    PodcastCategory = new PodcastCategoryDTO
                    {
                        Id = subCategory.PodcastCategory.Id,
                        Name = subCategory.PodcastCategory.Name,
                        MainImageFileKey = subCategory.PodcastCategory.MainImageFileKey
                    }
                };

                Console.WriteLine($"[TopSubCategory] Built with {showListItems.Count} shows for subcategory {targetSubCategoryId}");

                return new DiscoveryPodcastFeedDTO.TopSubCategoryDiscoveryPodcastFeedSection
                {
                    PodcastSubCategory = subCategoryDTO,
                    ShowList = showListItems
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TopSubCategory] ERROR: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Discovery Section 6: Top Podcasters

        private async Task<DiscoveryPodcastFeedDTO.TopPodcastersDiscoveryPodcastFeedSection?> BuildDiscoveryTopPodcastersSection(
            CacheMetricsContainer cacheMetrics,
            HashSet<int> dedupPodcasterIds)
        {
            try
            {
                Console.WriteLine("[TopPodcasters] Building section...");

                // Fetch all verified podcasters from UserService
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "verifiedPodcasters",
                            QueryType = "findall",
                            EntityType = "PodcasterProfile",
                            Parameters = JObject.FromObject(new { IsVerified = true })
                        }
                    }
                };

                var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
                var allPodcasters = result.Results["verifiedPodcasters"].ToObject<List<PodcasterProfileDTO>>();

                // Filter out already used podcasters
                allPodcasters = allPodcasters.Where(p => !dedupPodcasterIds.Contains(p.AccountId)).ToList();

                // Calculate hot scores (7d)
                var podcastersWithHotScore = await CalculatePodcasterHotScoresAsync(allPodcasters, cacheMetrics.PodcasterTemporal);

                // Part A (20%): 4 hot podcasters
                var partAPodcasters = podcastersWithHotScore
                    .Where(x => x.score > 0)
                    .OrderByDescending(x => x.score)
                    .Take(4)
                    .Select(x => x.podcaster)
                    .ToList();

                // Calculate popular scores (all-time)
                var podcastersWithPopularScore = CalculatePodcasterPopularScores(allPodcasters, cacheMetrics.PodcasterAllTime);

                var partAPodcasterIds = partAPodcasters.Select(p => p.AccountId).ToHashSet();

                // Part B (80%): 8 popular podcasters
                var partBPodcasters = podcastersWithPopularScore
                    .Where(x => !partAPodcasterIds.Contains(x.podcaster.AccountId))
                    .OrderByDescending(x => x.score)
                    .Take(8)
                    .Select(x => x.podcaster)
                    .ToList();

                var finalPodcasters = partAPodcasters.Concat(partBPodcasters).Take(12).ToList();

                // Update deduplication
                foreach (var podcaster in finalPodcasters)
                {
                    dedupPodcasterIds.Add(podcaster.AccountId);
                }

                // Fetch full account details
                var podcasterListItems = new List<AccountSnippetResponseDTO>();
                foreach (var podcaster in finalPodcasters)
                {
                    var account = await _accountCachingService.GetAccountStatusCacheById(podcaster.AccountId);
                    if (account != null)
                    {
                        podcasterListItems.Add(new AccountSnippetResponseDTO
                        {
                            Id = account.Id,
                            FullName = account.PodcasterProfileName ?? account.FullName,
                            Email = account.Email,
                            MainImageFileKey = account.MainImageFileKey
                        });
                    }
                }

                Console.WriteLine($"[TopPodcasters] Built with {podcasterListItems.Count} podcasters");

                return new DiscoveryPodcastFeedDTO.TopPodcastersDiscoveryPodcastFeedSection
                {
                    PodcasterList = podcasterListItems
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TopPodcasters] ERROR: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Discovery Section 7: Random Category

        private async Task<DiscoveryPodcastFeedDTO.RandomCategoryDiscoveryPodcastFeedSection?> BuildDiscoveryRandomCategorySection(
            UserPreferencesTemporal30dQueryMetric? userPrefs,
            SystemPreferencesTemporal30dQueryMetric? systemPrefs,
            CacheMetricsContainer cacheMetrics,
            HashSet<Guid> dedupShowIds)
        {
            try
            {
                Console.WriteLine("[RandomCategory] Building section...");

                // Get top 2 categories to exclude
                var excludeCategoryIds = new HashSet<int>();
                if (userPrefs?.ListenedPodcastCategories != null && userPrefs.ListenedPodcastCategories.Any())
                {
                    excludeCategoryIds = userPrefs.ListenedPodcastCategories
                        .OrderByDescending(c => c.ListenCount)
                        .Take(2)
                        .Select(c => c.PodcastCategoryId)
                        .ToHashSet();
                }

                // Fetch all category IDs from database
                var allCategoryIds = await _podcastCategoryGenericRepository.FindAll()
                    .Select(pc => pc.Id)
                    .ToListAsync();

                var availableCategoryIds = allCategoryIds
                    .Where(id => !excludeCategoryIds.Contains(id))
                    .ToList();

                if (!availableCategoryIds.Any())
                {
                    Console.WriteLine("[RandomCategory] No available categories");
                    return null;
                }

                // availableCategoryIds.ForEach(id => Console.WriteLine($"[RandomCategory] Available category: {id}"));

                // Pick random category
                var random = new Random();
                var randomCategoryId = availableCategoryIds[random.Next(availableCategoryIds.Count)];
                var randomCategory = await _podcastCategoryGenericRepository.FindByIdAsync(randomCategoryId);

                Console.WriteLine($"[RandomCategory] Selected category: {randomCategoryId}");

                // Fetch shows in category
                var shows = await _podcastShowGenericRepository.FindAll(
                    predicate: ps =>
                        ps.PodcastCategoryId == randomCategoryId &&
                        !dedupShowIds.Contains(ps.Id) &&
                        ps.DeletedAt == null,
                    includeFunc: q => q.Include(ps => ps.PodcastShowStatusTrackings)
                        .Include(ps => ps.PodcastChannel)
                .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                ).ToListAsync();

                // Filter by current Published status
                shows = shows.Where(ps =>
                {
                    var currentStatus = ps.PodcastShowStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastShowStatusId;
                    return currentStatus == (int)PodcastShowStatusEnum.Published &&
                    (ps.PodcastChannel == null || (ps.PodcastChannel.DeletedAt == null && ps.PodcastChannel.PodcastChannelStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published));
                }).ToList();

                // Part A (30%): 3 hot shows
                var showsWithHotScore = await CalculateShowHotScoresAsync(shows, cacheMetrics.ShowTemporal);

                var partAShows = showsWithHotScore
                    .Where(x => x.score > 0)
                    .OrderByDescending(x => x.score)
                    .Take(3)
                    .Select(x => x.show)
                    .ToList();

                // Part B (70%): 9 popular shows
                var existingIds = partAShows.Select(s => s.Id).ToHashSet();
                var showsWithPopularScore = CalculateShowPopularScores(
                    shows.Where(s => !existingIds.Contains(s.Id)).ToList(),
                    cacheMetrics.ShowAllTime
                );

                var needed = 3 - partAShows.Count;
                var partBShows = showsWithPopularScore
                    .OrderByDescending(x => x.score)
                    .Take(9 + needed)
                    .Select(x => x.show)
                    .ToList();

                var finalShows = partAShows.Concat(partBShows).Take(12).ToList();

                // Update deduplication
                foreach (var show in finalShows) dedupShowIds.Add(show.Id);

                var showListItems = await MapToShowListItemsAsync(finalShows);

                Console.WriteLine($"[RandomCategory] Built with {showListItems.Count} shows");

                return new DiscoveryPodcastFeedDTO.RandomCategoryDiscoveryPodcastFeedSection
                {
                    PodcastCategory = new PodcastCategoryDTO
                    {
                        Id = randomCategory.Id,
                        Name = randomCategory.Name,
                        MainImageFileKey = randomCategory.MainImageFileKey
                    },
                    ShowList = showListItems
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RandomCategory] ERROR: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Discovery Section 8: Talented Rookies

        private async Task<DiscoveryPodcastFeedDTO.TalentedRookiesDiscoveryPodcastFeedSection?> BuildDiscoveryTalentedRookiesSection(
            CacheMetricsContainer cacheMetrics,
            HashSet<int> dedupPodcasterIds)
        {
            try
            {
                Console.WriteLine("[TalentedRookies] Building section...");

                var now = _dateHelper.GetNowByAppTimeZone();
                var ninetyDaysAgo = now.AddDays(-90);

                // Fetch all verified podcasters from UserService
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
                    {
                        new BatchQueryItem
                        {
                            Key = "rookiePodcasters",
                            QueryType = "findall",
                            EntityType = "PodcasterProfile",
                            Parameters = JObject.FromObject(new { IsVerified = true })
                        }
                    }
                };

                var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
                var allPodcasters = result.Results["rookiePodcasters"].ToObject<List<PodcasterProfileDTO>>();

                // Filter rookies (verified ≤ 90 days and not already used)
                var rookies = allPodcasters
                    .Where(p => p.VerifiedAt.HasValue && p.VerifiedAt.Value >= ninetyDaysAgo)
                    .Where(p => !dedupPodcasterIds.Contains(p.AccountId))
                    .ToList();

                // Calculate rookie scores
                var rookiesWithScore = CalculateRookieScores(rookies, cacheMetrics.PodcasterAllTime);

                var finalRookies = rookiesWithScore
                    .OrderByDescending(x => x.score)
                    .Take(8)
                    .Select(x => x.podcaster)
                    .ToList();

                // Update deduplication
                foreach (var rookie in finalRookies)
                {
                    dedupPodcasterIds.Add(rookie.AccountId);
                }

                // Fetch full account details
                var rookieListItems = new List<AccountSnippetResponseDTO>();
                foreach (var rookie in finalRookies)
                {
                    var account = await _accountCachingService.GetAccountStatusCacheById(rookie.AccountId);
                    if (account != null)
                    {
                        rookieListItems.Add(new AccountSnippetResponseDTO
                        {
                            Id = account.Id,
                            FullName = account.PodcasterProfileName ?? account.FullName,
                            Email = account.Email,
                            MainImageFileKey = account.MainImageFileKey
                        });
                    }
                }

                Console.WriteLine($"[TalentedRookies] Built with {rookieListItems.Count} rookies");

                return new DiscoveryPodcastFeedDTO.TalentedRookiesDiscoveryPodcastFeedSection
                {
                    PodcasterList = rookieListItems
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TalentedRookies] ERROR: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Score Calculation Helpers
        // ============ EPISPODE SCORES ============
        private List<(PodcastEpisode episode, double score)> CalculateEpisodePopularScores(
            List<PodcastEpisode> episodes,
            EpisodeAllTimeMaxQueryMetric? allTimeMetric)
        {
            if (allTimeMetric == null)
                return episodes.Select(c => (c, 0.0)).ToList();

            var results = new List<(PodcastEpisode episode, double score)>();
            foreach (var episode in episodes)
            {
                var normalizedLC = allTimeMetric.MaxListenCount > 0
                    ? (double)episode.ListenCount / allTimeMetric.MaxListenCount
                    : 0;
                var normalizedTF = allTimeMetric.MaxTotalSave > 0
                    ? (double)episode.TotalSave / allTimeMetric.MaxTotalSave
                    : 0;

                var popularScore = 0.6 * normalizedLC + 0.4 * normalizedTF;
                results.Add((episode, popularScore));
            }

            return results;
        }

        // ============ SHOW SCORES ============
        private async Task<List<(PodcastShow show, double score)>> CalculateShowHotScoresAsync(
    List<PodcastShow> shows,
    ShowTemporal7dMaxQueryMetric? temporal7dMetric)
        {
            if (temporal7dMetric == null || temporal7dMetric.MaxNewListenSession == 0)
                return shows.Select(s => (s, 0.0)).ToList();

            var now = _dateHelper.GetNowByAppTimeZone();
            var sevenDaysAgo = now.AddDays(-7);

            // Query listen sessions (7 days)
            var showIds = shows.Select(s => s.Id).ToList();
            var listenSessions = await _podcastEpisodeListenSessionGenericRepository.FindAll(
                predicate: ls =>
                    ls.CreatedAt >= sevenDaysAgo &&
                    ls.IsContentRemoved == false &&
                    ls.PodcastEpisode.DeletedAt == null && // FIX: Add episode check
                    ls.PodcastEpisode.PodcastShow.DeletedAt == null && // FIX: Add show check
                    showIds.Contains(ls.PodcastEpisode.PodcastShowId),
                includeFunc: q => q
                    .Include(ls => ls.PodcastEpisode)
                        .ThenInclude(pe => pe.PodcastEpisodeStatusTrackings)
                    .Include(ls => ls.PodcastEpisode)
                        .ThenInclude(pe => pe.PodcastShow)
            ).ToListAsync();

            // FIX: Filter by Published status
            listenSessions = listenSessions.Where(ls =>
            {
                var episodeStatus = ls.PodcastEpisode.PodcastEpisodeStatusTrackings
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefault()?.PodcastEpisodeStatusId;
                return episodeStatus == (int)PodcastEpisodeStatusEnum.Published;
            }).ToList();

            var listenSessionsByShow = listenSessions
                .GroupBy(ls => ls.PodcastEpisode.PodcastShowId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Query follows (7 days) from UserService
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
        {
            new BatchQueryItem
            {
                Key = "recentFollows",
                QueryType = "findall",
                EntityType = "AccountFollowedPodcastShow",
                Parameters = JObject.FromObject(new { })
            }
        }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
            var allFollows = result.Results["recentFollows"].ToObject<List<AccountFollowedPodcastShowDTO>>();

            var recentFollows = allFollows
                .Where(f => f.CreatedAt >= sevenDaysAgo && showIds.Contains(f.PodcastShowId))
                .GroupBy(f => f.PodcastShowId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Calculate scores
            var results = new List<(PodcastShow show, double score)>();
            foreach (var show in shows)
            {
                var nls = listenSessionsByShow.ContainsKey(show.Id) ? listenSessionsByShow[show.Id] : 0;
                var nf = recentFollows.ContainsKey(show.Id) ? recentFollows[show.Id] : 0;

                var normalizedNLS = temporal7dMetric.MaxNewListenSession > 0
                    ? (double)nls / temporal7dMetric.MaxNewListenSession
                    : 0;
                var normalizedNF = temporal7dMetric.MaxNewFollow > 0
                    ? (double)nf / temporal7dMetric.MaxNewFollow
                    : 0;

                var hotScore = 0.6 * normalizedNLS + 0.4 * normalizedNF;
                results.Add((show, hotScore));
            }

            return results;
        }

        private List<(PodcastShow show, double score)> CalculateShowPopularScores(
            List<PodcastShow> shows,
            ShowAllTimeMaxQueryMetric? allTimeMetric)
        {
            if (allTimeMetric == null)
                return shows.Select(s => (s, 0.0)).ToList();

            var results = new List<(PodcastShow show, double score)>();
            foreach (var show in shows)
            {
                var normalizedTF = allTimeMetric.MaxTotalFollow > 0
                    ? (double)show.TotalFollow / allTimeMetric.MaxTotalFollow
                    : 0;
                var normalizedLC = allTimeMetric.MaxListenCount > 0
                    ? (double)show.ListenCount / allTimeMetric.MaxListenCount
                    : 0;

                var rt = show.AverageRating * Math.Log(show.RatingCount + 1);
                var normalizedRT = allTimeMetric.MaxRatingTerm > 0
                    ? rt / allTimeMetric.MaxRatingTerm
                    : 0;

                var popularScore = 0.4 * normalizedTF + 0.4 * normalizedLC + 0.2 * normalizedRT;
                results.Add((show, popularScore));
            }

            return results;
        }

        // ============ CHANNEL SCORES ============
        private async Task<List<(PodcastChannel channel, double score)>> CalculateChannelHotScoresAsync(
    List<PodcastChannel> channels,
    ChannelTemporal7dMaxQueryMetric? temporal7dMetric)
        {
            if (temporal7dMetric == null || temporal7dMetric.MaxNewListenSession == 0)
                return channels.Select(c => (c, 0.0)).ToList();

            var now = _dateHelper.GetNowByAppTimeZone();
            var sevenDaysAgo = now.AddDays(-7);

            // Query listen sessions (7 days)
            var channelIds = channels.Select(c => c.Id).ToList();
            var listenSessions = await _podcastEpisodeListenSessionGenericRepository.FindAll(
                predicate: ls =>
                    ls.CreatedAt >= sevenDaysAgo &&
                    ls.IsContentRemoved == false &&
                    ls.PodcastEpisode.DeletedAt == null && // FIX: Add checks
                    ls.PodcastEpisode.PodcastShow.DeletedAt == null &&
                    ls.PodcastEpisode.PodcastShow.PodcastChannelId != null &&
                    channelIds.Contains(ls.PodcastEpisode.PodcastShow.PodcastChannelId.Value),
                includeFunc: q => q
                    .Include(ls => ls.PodcastEpisode)
                        .ThenInclude(pe => pe.PodcastEpisodeStatusTrackings)
                    .Include(ls => ls.PodcastEpisode)
                        .ThenInclude(pe => pe.PodcastShow)
            ).ToListAsync();

            // FIX: Filter by Published status
            listenSessions = listenSessions.Where(ls =>
            {
                var episodeStatus = ls.PodcastEpisode.PodcastEpisodeStatusTrackings
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefault()?.PodcastEpisodeStatusId;
                return episodeStatus == (int)PodcastEpisodeStatusEnum.Published;
            }).ToList();

            var listenSessionsByChannel = listenSessions
                .GroupBy(ls => ls.PodcastEpisode.PodcastShow.PodcastChannelId.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            // Query favorites from UserService
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
        {
            new BatchQueryItem
            {
                Key = "recentFavorites",
                QueryType = "findall",
                EntityType = "AccountFavoritedPodcastChannel",
                Parameters = JObject.FromObject(new { })
            }
        }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
            var allFavorites = result.Results["recentFavorites"].ToObject<List<AccountFavoritedPodcastChannelDTO>>();

            var recentFavorites = allFavorites
                .Where(f => f.CreatedAt >= sevenDaysAgo && channelIds.Contains(f.PodcastChannelId))
                .GroupBy(f => f.PodcastChannelId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Calculate scores
            var results = new List<(PodcastChannel channel, double score)>();
            foreach (var channel in channels)
            {
                var nls = listenSessionsByChannel.ContainsKey(channel.Id) ? listenSessionsByChannel[channel.Id] : 0;
                var nf = recentFavorites.ContainsKey(channel.Id) ? recentFavorites[channel.Id] : 0;

                var normalizedNLS = temporal7dMetric.MaxNewListenSession > 0
                    ? (double)nls / temporal7dMetric.MaxNewListenSession
                    : 0;
                var normalizedNF = temporal7dMetric.MaxNewFavorite > 0
                    ? (double)nf / temporal7dMetric.MaxNewFavorite
                    : 0;

                var hotScore = 0.6 * normalizedNLS + 0.4 * normalizedNF;
                results.Add((channel, hotScore));
            }

            return results;
        }

        private List<(PodcastChannel channel, double score)> CalculateChannelPopularScores(
            List<PodcastChannel> channels,
            ChannelAllTimeMaxQueryMetric? allTimeMetric)
        {
            if (allTimeMetric == null)
                return channels.Select(c => (c, 0.0)).ToList();

            var results = new List<(PodcastChannel channel, double score)>();
            foreach (var channel in channels)
            {
                var normalizedLC = allTimeMetric.MaxListenCount > 0
                    ? (double)channel.ListenCount / allTimeMetric.MaxListenCount
                    : 0;
                var normalizedTF = allTimeMetric.MaxTotalFavorite > 0
                    ? (double)channel.TotalFavorite / allTimeMetric.MaxTotalFavorite
                    : 0;

                var popularScore = 0.6 * normalizedLC + 0.4 * normalizedTF;
                results.Add((channel, popularScore));
            }

            return results;
        }

        // ============ PODCASTER SCORES ============
        private async Task<List<(PodcasterProfileDTO podcaster, double score)>> CalculatePodcasterHotScoresAsync(
    List<PodcasterProfileDTO> podcasters,
    PodcasterTemporal7dMaxQueryMetric? temporal7dMetric)
        {
            if (temporal7dMetric == null || temporal7dMetric.MaxNewListenSession == 0)
                return podcasters.Select(p => (p, 0.0)).ToList();

            var now = _dateHelper.GetNowByAppTimeZone();
            var sevenDaysAgo = now.AddDays(-7);

            // FIX: Filter podcasters với DeactivatedAt từ cache
            var validPodcasterIds = new HashSet<int>();
            foreach (var podcaster in podcasters)
            {
                var account = await _accountCachingService.GetAccountStatusCacheById(podcaster.AccountId);
                if (account != null && account.DeactivatedAt == null && account.HasVerifiedPodcasterProfile == true)
                {
                    validPodcasterIds.Add(podcaster.AccountId);
                }
            }

            // Query listen sessions (7 days)
            var listenSessions = await _podcastEpisodeListenSessionGenericRepository.FindAll(
                predicate: ls =>
                    ls.CreatedAt >= sevenDaysAgo &&
                    ls.IsContentRemoved == false &&
                    ls.PodcastEpisode.DeletedAt == null &&
                    ls.PodcastEpisode.PodcastShow.DeletedAt == null &&
                    validPodcasterIds.Contains(ls.PodcastEpisode.PodcastShow.PodcasterId),
                includeFunc: q => q
                    .Include(ls => ls.PodcastEpisode)
                        .ThenInclude(pe => pe.PodcastEpisodeStatusTrackings)
                    .Include(ls => ls.PodcastEpisode)
                        .ThenInclude(pe => pe.PodcastShow)
            ).ToListAsync();

            // FIX: Filter by Published status
            listenSessions = listenSessions.Where(ls =>
            {
                var episodeStatus = ls.PodcastEpisode.PodcastEpisodeStatusTrackings
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefault()?.PodcastEpisodeStatusId;
                return episodeStatus == (int)PodcastEpisodeStatusEnum.Published;
            }).ToList();

            var listenSessionsByPodcaster = listenSessions
                .GroupBy(ls => ls.PodcastEpisode.PodcastShow.PodcasterId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Query follows from UserService
            var batchRequest = new BatchQueryRequest
            {
                Queries = new List<BatchQueryItem>
        {
            new BatchQueryItem
            {
                Key = "recentFollows",
                QueryType = "findall",
                EntityType = "AccountFollowedPodcaster",
                Parameters = JObject.FromObject(new { })
            }
        }
            };
            var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
            var allFollows = result.Results["recentFollows"].ToObject<List<AccountFollowedPodcasterDTO>>();

            var recentFollows = allFollows
                .Where(f => f.CreatedAt >= sevenDaysAgo && validPodcasterIds.Contains(f.PodcasterId))
                .GroupBy(f => f.PodcasterId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Calculate scores - chỉ cho valid podcasters
            var results = new List<(PodcasterProfileDTO podcaster, double score)>();
            foreach (var podcaster in podcasters.Where(p => validPodcasterIds.Contains(p.AccountId)))
            {
                var nls = listenSessionsByPodcaster.ContainsKey(podcaster.AccountId)
                    ? listenSessionsByPodcaster[podcaster.AccountId] : 0;
                var nf = recentFollows.ContainsKey(podcaster.AccountId)
                    ? recentFollows[podcaster.AccountId] : 0;

                var g = nls + nf;

                var normalizedNLS = temporal7dMetric.MaxNewListenSession > 0
                    ? (double)nls / temporal7dMetric.MaxNewListenSession
                    : 0;
                var normalizedNF = temporal7dMetric.MaxNewFollow > 0
                    ? (double)nf / temporal7dMetric.MaxNewFollow
                    : 0;
                var normalizedG = temporal7dMetric.MaxGrowth > 0
                    ? (double)g / (2 * temporal7dMetric.MaxGrowth)
                    : 0;
                var normalizedRating = podcaster.AverageRating / 5.0;

                var hotScore = 0.5 * normalizedNLS + 0.3 * normalizedNF + 0.15 * normalizedG + 0.05 * normalizedRating;
                results.Add((podcaster, hotScore));
            }

            return results;
        }

        private List<(PodcasterProfileDTO podcaster, double score)> CalculatePodcasterPopularScores(
            List<PodcasterProfileDTO> podcasters,
            PodcasterAllTimeMaxQueryMetric? allTimeMetric)
        {
            if (allTimeMetric == null)
                return podcasters.Select(p => (p, 0.0)).ToList();

            var now = _dateHelper.GetNowByAppTimeZone();
            var results = new List<(PodcasterProfileDTO podcaster, double score)>();

            foreach (var podcaster in podcasters)
            {
                var normalizedTF = allTimeMetric.MaxTotalFollow > 0
                    ? (double)podcaster.TotalFollow / allTimeMetric.MaxTotalFollow
                    : 0;
                var normalizedLC = allTimeMetric.MaxListenCount > 0
                    ? (double)podcaster.ListenCount / allTimeMetric.MaxListenCount
                    : 0;

                var rt = podcaster.AverageRating * Math.Log(podcaster.RatingCount + 1);
                var normalizedRT = allTimeMetric.MaxRatingTerm > 0
                    ? rt / allTimeMetric.MaxRatingTerm
                    : 0;

                var age = podcaster.VerifiedAt.HasValue
                    ? (now - podcaster.VerifiedAt.Value).TotalDays
                    : 0;
                var normalizedAge = allTimeMetric.MaxAge > 0
                    ? age / allTimeMetric.MaxAge
                    : 0;

                var popularScore = 0.4 * normalizedTF + 0.4 * normalizedLC + 0.15 * normalizedRT + 0.05 * normalizedAge;
                results.Add((podcaster, popularScore));
            }

            return results;
        }

        private List<(PodcasterProfileDTO podcaster, double score)> CalculateRookieScores(
            List<PodcasterProfileDTO> podcasters,
            PodcasterAllTimeMaxQueryMetric? allTimeMetric)
        {
            if (allTimeMetric == null)
                return podcasters.Select(p => (p, 0.0)).ToList();

            var now = _dateHelper.GetNowByAppTimeZone();
            var results = new List<(PodcasterProfileDTO podcaster, double score)>();

            foreach (var podcaster in podcasters)
            {
                var normalizedLC = allTimeMetric.MaxListenCount > 0
                    ? (double)podcaster.ListenCount / allTimeMetric.MaxListenCount
                    : 0;

                var ageInDays = podcaster.VerifiedAt.HasValue
                    ? Math.Max(1, (now - podcaster.VerifiedAt.Value).TotalDays)
                    : 1;
                var growth = podcaster.TotalFollow / ageInDays;

                // Calculate MaxGrowth dynamically if not in cache
                var maxGrowth = allTimeMetric.MaxAge > 0
                    ? (double)allTimeMetric.MaxTotalFollow / allTimeMetric.MaxAge
                    : 1;
                var normalizedG = maxGrowth > 0
                    ? growth / maxGrowth
                    : 0;

                var rt = podcaster.AverageRating * Math.Log(podcaster.RatingCount + 1);
                var normalizedRT = allTimeMetric.MaxRatingTerm > 0
                    ? rt / allTimeMetric.MaxRatingTerm
                    : 0;

                var rookieScore = 0.4 * normalizedLC + 0.4 * normalizedG + 0.2 * normalizedRT;
                results.Add((podcaster, rookieScore));
            }

            return results;
        }

        private async Task<List<(PodcastShow show, double score)>> CalculatePersonalShowScoresAsync(
            List<PodcastShow> shows,
            int userId)
        {
            var result = new List<(PodcastShow show, double score)>();

            foreach (var show in shows)
            {
                // Calculate user engagement
                var totalEpisodes = await _podcastEpisodeGenericRepository.FindAll(
                    predicate: pe =>
                        pe.PodcastShowId == show.Id &&
                        pe.DeletedAt == null,
                    includeFunc: q => q.Include(pe => pe.PodcastEpisodeStatusTrackings)
                ).CountAsync(pe => pe.PodcastEpisodeStatusTrackings
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefault().PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published);

                var listenedEpisodeIds = await _podcastEpisodeListenSessionGenericRepository.FindAll(
                    predicate: ls =>
                        ls.AccountId == userId &&
                        ls.PodcastEpisode.PodcastShowId == show.Id,
                    includeFunc: q => q.Include(ls => ls.PodcastEpisode)
                ).Select(ls => ls.PodcastEpisodeId).Distinct().CountAsync();

                var userEngagement = totalEpisodes > 0 ? (double)listenedEpisodeIds / totalEpisodes : 0;

                // Calculate show quality
                var normalizedTF = Math.Min(show.TotalFollow, 100000) / 100000.0;
                var normalizedRating = show.AverageRating / 5.0;
                var showQuality = 0.5 * normalizedTF + 0.5 * normalizedRating;

                var personalScore = 0.6 * userEngagement + 0.4 * showQuality;

                result.Add((show, personalScore));
            }

            return result;
        }

        #endregion

        #region Mapping Helpers

        private async Task<List<ShowListItemResponseDTO>> MapToShowListItemsAsync(List<PodcastShow> shows)
        {
            var result = new List<ShowListItemResponseDTO>();

            // Collect unique IDs
            var podcasterIds = shows.Select(s => s.PodcasterId).Distinct().ToList();
            var channelIds = shows.Where(s => s.PodcastChannelId.HasValue).Select(s => s.PodcastChannelId.Value).Distinct().ToList();
            var categoryIds = shows.Where(s => s.PodcastCategoryId.HasValue).Select(s => s.PodcastCategoryId.Value).Distinct().ToList();
            var subCategoryIds = shows.Where(s => s.PodcastSubCategoryId.HasValue).Select(s => s.PodcastSubCategoryId.Value).Distinct().ToList();

            // Batch fetch podcasters
            var podcasterAccounts = new Dictionary<int, AccountStatusCache>();
            foreach (var id in podcasterIds)
            {
                var account = await _accountCachingService.GetAccountStatusCacheById(id);
                if (account != null && account.DeactivatedAt == null && account.HasVerifiedPodcasterProfile == true) // FIX: Check DeactivatedAt
                {
                    podcasterAccounts[id] = account;
                }
            }

            // Fetch categories
            var categories = await _podcastCategoryGenericRepository.FindAll(
                predicate: pc => categoryIds.Contains(pc.Id)
            ).ToDictionaryAsync(pc => pc.Id, pc => pc);

            // Fetch subcategories
            var subCategories = await _podcastSubCategoryGenericRepository.FindAll(
                predicate: psc => subCategoryIds.Contains(psc.Id)
            ).ToDictionaryAsync(psc => psc.Id, psc => psc);

            // Fetch channels
            var channels = await _podcastChannelGenericRepository.FindAll(
                predicate: pc => channelIds.Contains(pc.Id),
                includeFunc: q => q.Include(pc => pc.PodcastChannelStatusTrackings)
            ).ToDictionaryAsync(pc => pc.Id, pc => pc);

            // Fetch hashtags
            var showHashtags = await _podcastShowHashtagGenericRepository.FindAll(
                predicate: psh => shows.Select(s => s.Id).Contains(psh.PodcastShowId),
                includeFunc: q => q.Include(psh => psh.Hashtag)
            ).ToListAsync();

            var hashtagsByShow = showHashtags
                .GroupBy(psh => psh.PodcastShowId)
                .ToDictionary(g => g.Key, g => g.Select(psh => psh.Hashtag).ToList());

            // Fetch subscription types
            var subscriptionTypes = await _appDbContext.PodcastShowSubscriptionTypes
                .ToDictionaryAsync(psst => psst.Id, psst => psst);

            // Fetch episodes count for each show
            var episodeCounts = await _podcastEpisodeGenericRepository.FindAll(
                predicate: pe =>
                    shows.Select(s => s.Id).Contains(pe.PodcastShowId) &&
                    pe.DeletedAt == null,
                includeFunc: q => q.Include(pe => pe.PodcastEpisodeStatusTrackings)
            )
            .ToListAsync()
            .ContinueWith(task =>
            {
                return task.Result
                    .Where(pe => pe.PodcastEpisodeStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published)
                    .GroupBy(pe => pe.PodcastShowId)
                    .ToDictionary(g => g.Key, g => g.Count());
            });

            // Map each show
            foreach (var show in shows)
            {
                var podcasterAccount = podcasterAccounts.ContainsKey(show.PodcasterId)
                    ? podcasterAccounts[show.PodcasterId]
                    : null;

                var currentStatus = show.PodcastShowStatusTrackings
                    .OrderByDescending(pst => pst.CreatedAt)
                    .FirstOrDefault();

                var item = new ShowListItemResponseDTO
                {
                    Id = show.Id,
                    Name = show.Name,
                    Description = show.Description,
                    Language = show.Language,
                    ReleaseDate = show.ReleaseDate,
                    IsReleased = show.IsReleased,
                    Copyright = show.Copyright,
                    UploadFrequency = show.UploadFrequency,
                    RatingCount = show.RatingCount,
                    AverageRating = show.AverageRating,
                    MainImageFileKey = show.MainImageFileKey,
                    TrailerAudioFileKey = show.TrailerAudioFileKey,
                    ListenCount = show.ListenCount,
                    TotalFollow = show.TotalFollow,
                    EpisodeCount = episodeCounts.ContainsKey(show.Id) ? episodeCounts[show.Id] : 0,
                    TakenDownReason = show.TakenDownReason,
                    CreatedAt = show.CreatedAt,
                    UpdatedAt = show.UpdatedAt,

                    Podcaster = podcasterAccount != null ? new AccountSnippetResponseDTO
                    {
                        Id = podcasterAccount.Id,
                        FullName = podcasterAccount.PodcasterProfileName ?? podcasterAccount.FullName,
                        Email = podcasterAccount.Email,
                        MainImageFileKey = podcasterAccount.MainImageFileKey
                    } : null,

                    PodcastCategory = show.PodcastCategoryId.HasValue && categories.ContainsKey(show.PodcastCategoryId.Value)
                        ? new PodcastCategoryDTO
                        {
                            Id = categories[show.PodcastCategoryId.Value].Id,
                            Name = categories[show.PodcastCategoryId.Value].Name,
                            MainImageFileKey = categories[show.PodcastCategoryId.Value].MainImageFileKey
                        }
                        : null,

                    PodcastSubCategory = show.PodcastSubCategoryId.HasValue && subCategories.ContainsKey(show.PodcastSubCategoryId.Value)
                        ? new PodcastSubCategoryDTO
                        {
                            Id = subCategories[show.PodcastSubCategoryId.Value].Id,
                            Name = subCategories[show.PodcastSubCategoryId.Value].Name,
                            PodcastCategoryId = subCategories[show.PodcastSubCategoryId.Value].PodcastCategoryId
                        }
                        : null,

                    PodcastShowSubscriptionType = subscriptionTypes.ContainsKey(show.PodcastShowSubscriptionTypeId)
                        ? new PodcastShowSubscriptionTypeDTO
                        {
                            Id = subscriptionTypes[show.PodcastShowSubscriptionTypeId].Id,
                            Name = subscriptionTypes[show.PodcastShowSubscriptionTypeId].Name
                        }
                        : null,

                    PodcastChannel = show.PodcastChannelId.HasValue && channels.ContainsKey(show.PodcastChannelId.Value)
                        ? new PodcastChannelSnippetResponseDTO
                        {
                            Id = channels[show.PodcastChannelId.Value].Id,
                            Name = channels[show.PodcastChannelId.Value].Name,
                            Description = channels[show.PodcastChannelId.Value].Description,
                            MainImageFileKey = channels[show.PodcastChannelId.Value].MainImageFileKey
                        }
                        : null,

                    Hashtags = hashtagsByShow.ContainsKey(show.Id)
                        ? hashtagsByShow[show.Id].Select(h => new HashtagDTO
                        {
                            Id = h.Id,
                            Name = h.Name
                        }).ToList()
                        : null,

                    CurrentStatus = new PodcastShowStatusDTO
                    {
                        Id = currentStatus.PodcastShowStatusId,
                        Name = ((PodcastShowStatusEnum)currentStatus.PodcastShowStatusId).ToString()
                    }
                };

                result.Add(item);
            }

            return result;
        }

        private async Task<List<ChannelListItemResponseDTO>> MapToChannelListItemsAsync(List<PodcastChannel> channels)
        {
            var result = new List<ChannelListItemResponseDTO>();

            // Collect unique IDs
            var podcasterIds = channels.Select(c => c.PodcasterId).Distinct().ToList();
            var categoryIds = channels.Where(c => c.PodcastCategoryId.HasValue).Select(c => c.PodcastCategoryId.Value).Distinct().ToList();
            var subCategoryIds = channels.Where(c => c.PodcastSubCategoryId.HasValue).Select(c => c.PodcastSubCategoryId.Value).Distinct().ToList();

            // Batch fetch podcasters
            var podcasterAccounts = new Dictionary<int, AccountStatusCache>();
            foreach (var id in podcasterIds)
            {
                var account = await _accountCachingService.GetAccountStatusCacheById(id);
                if (account != null && account.DeactivatedAt == null && account.HasVerifiedPodcasterProfile == true) // FIX: Check DeactivatedAt
                {
                    podcasterAccounts[id] = account;
                }
            }

            // Fetch categories
            var categories = await _podcastCategoryGenericRepository.FindAll(
                predicate: pc => categoryIds.Contains(pc.Id)
            ).ToDictionaryAsync(pc => pc.Id, pc => pc);

            // Fetch subcategories
            var subCategories = await _podcastSubCategoryGenericRepository.FindAll(
                predicate: psc => subCategoryIds.Contains(psc.Id)
            ).ToDictionaryAsync(psc => psc.Id, psc => psc);

            // Fetch hashtags
            var channelHashtags = await _podcastChannelHashtagGenericRepository.FindAll(
                predicate: pch => channels.Select(c => c.Id).Contains(pch.PodcastChannelId),
                includeFunc: q => q.Include(pch => pch.Hashtag)
            ).ToListAsync();

            var hashtagsByChannel = channelHashtags
                .GroupBy(pch => pch.PodcastChannelId)
                .ToDictionary(g => g.Key, g => g.Select(pch => pch.Hashtag).ToList());

            // Fetch show counts for each channel
            var showCounts = await _podcastShowGenericRepository.FindAll(
                predicate: ps =>
                    ps.PodcastChannelId.HasValue &&
                    channels.Select(c => c.Id).Contains(ps.PodcastChannelId.Value) &&
                    ps.DeletedAt == null,
                includeFunc: q => q.Include(ps => ps.PodcastShowStatusTrackings)
                .Include(ps => ps.PodcastChannel)
                .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
            )
            .ToListAsync()
            .ContinueWith(task =>
            {
                return task.Result
                    .Where(ps => ps.PodcastShowStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastShowStatusId == (int)PodcastShowStatusEnum.Published &&
                        (ps.PodcastChannel == null || (ps.PodcastChannel.DeletedAt == null && ps.PodcastChannel.PodcastChannelStatusTrackings
                            .OrderByDescending(t => t.CreatedAt)
                            .FirstOrDefault()?.PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published)))
                    .GroupBy(ps => ps.PodcastChannelId.Value)
                    .ToDictionary(g => g.Key, g => g.Count());
            });

            // Map each channel
            foreach (var channel in channels)
            {
                var podcasterAccount = podcasterAccounts.ContainsKey(channel.PodcasterId)
                    ? podcasterAccounts[channel.PodcasterId]
                    : null;

                var currentStatus = channel.PodcastChannelStatusTrackings
                    .OrderByDescending(pst => pst.CreatedAt)
                    .FirstOrDefault();

                var item = new ChannelListItemResponseDTO
                {
                    Id = channel.Id,
                    Name = channel.Name,
                    Description = channel.Description,
                    BackgroundImageFileKey = channel.BackgroundImageFileKey,
                    MainImageFileKey = channel.MainImageFileKey,
                    TotalFavorite = channel.TotalFavorite,
                    ListenCount = channel.ListenCount,
                    ShowCount = showCounts.ContainsKey(channel.Id) ? showCounts[channel.Id] : 0,
                    CreatedAt = channel.CreatedAt,
                    UpdatedAt = channel.UpdatedAt,

                    Podcaster = podcasterAccount != null ? new AccountSnippetResponseDTO
                    {
                        Id = podcasterAccount.Id,
                        FullName = podcasterAccount.PodcasterProfileName ?? podcasterAccount.FullName,
                        Email = podcasterAccount.Email,
                        MainImageFileKey = podcasterAccount.MainImageFileKey
                    } : null,

                    PodcastCategory = channel.PodcastCategoryId.HasValue && categories.ContainsKey(channel.PodcastCategoryId.Value)
                        ? new PodcastCategoryDTO
                        {
                            Id = categories[channel.PodcastCategoryId.Value].Id,
                            Name = categories[channel.PodcastCategoryId.Value].Name,
                            MainImageFileKey = categories[channel.PodcastCategoryId.Value].MainImageFileKey
                        }
                        : null,

                    PodcastSubCategory = channel.PodcastSubCategoryId.HasValue && subCategories.ContainsKey(channel.PodcastSubCategoryId.Value)
                        ? new PodcastSubCategoryDTO
                        {
                            Id = subCategories[channel.PodcastSubCategoryId.Value].Id,
                            Name = subCategories[channel.PodcastSubCategoryId.Value].Name,
                            PodcastCategoryId = subCategories[channel.PodcastSubCategoryId.Value].PodcastCategoryId
                        }
                        : null,

                    Hashtags = hashtagsByChannel.ContainsKey(channel.Id)
                        ? hashtagsByChannel[channel.Id].Select(h => new HashtagDTO
                        {
                            Id = h.Id,
                            Name = h.Name
                        }).ToList()
                        : null,

                    CurrentStatus = new PodcastChannelStatusDTO
                    {
                        Id = currentStatus.PodcastChannelStatusId,
                        Name = ((PodcastChannelStatusEnum)currentStatus.PodcastChannelStatusId).ToString()
                    }
                };

                result.Add(item);
            }

            return result;
        }

        #endregion

        #region Helper Classes

        private class CacheMetricsContainer
        {
            public PodcasterAllTimeMaxQueryMetric? PodcasterAllTime { get; set; }
            public PodcasterTemporal7dMaxQueryMetric? PodcasterTemporal { get; set; }
            public ShowAllTimeMaxQueryMetric? ShowAllTime { get; set; }
            public ShowTemporal7dMaxQueryMetric? ShowTemporal { get; set; }
            public ChannelAllTimeMaxQueryMetric? ChannelAllTime { get; set; }
            public ChannelTemporal7dMaxQueryMetric? ChannelTemporal { get; set; }
            public EpisodeAllTimeMaxQueryMetric? EpisodeAllTime { get; set; }
        }

        #endregion

        #region Trending Entry Point

        public async Task<TrendingPodcastFeedDTO> GetTrendingPodcastFeedContentsAsync()
        {
            try
            {
                Console.WriteLine("[GetTrendingPodcastFeed] Starting - Anonymous access");

                // STEP 1: Load cache metrics (NO user preferences needed)
                var cacheMetrics = await LoadTrendingCacheMetricsAsync();
                var systemPrefs = await LoadSystemPreferencesAsync();

                // STEP 2: Build sections independently (NO deduplication between sections)
                var popularPodcasters = await BuildTrendingPopularPodcastersSection(cacheMetrics);

                var hotPodcasters = await BuildTrendingHotPodcastersSection(cacheMetrics);

                var popularChannels = await BuildTrendingPopularChannelsSection(cacheMetrics);

                var hotChannels = await BuildTrendingHotChannelsSection(cacheMetrics);

                var popularShows = await BuildTrendingPopularShowsSection(cacheMetrics);

                var hotShows = await BuildTrendingHotShowsSection(cacheMetrics);

                var newEpisodes = await BuildTrendingNewEpisodesSection();

                var popularEpisodes = await BuildTrendingPopularEpisodesSection(cacheMetrics);

                // STEP 3: Build 6 dynamic category sections with stable random
                var dynamicCategories = await BuildTrendingDynamicCategorySectionsAsync(systemPrefs, cacheMetrics);

                Console.WriteLine("[GetTrendingPodcastFeed] Completed successfully");

                return new TrendingPodcastFeedDTO
                {
                    PopularPodcasters = popularPodcasters ?? new TrendingPodcastFeedDTO.PopularPodcastersTrendingPodcastFeedSection { PodcasterList = new List<AccountSnippetResponseDTO>() },
                    Category1 = dynamicCategories[0] ?? new TrendingPodcastFeedDTO.CategoryTrendingPodcastFeedSection { PodcastCategory = new PodcastCategoryDTO(), ShowList = new List<ShowListItemResponseDTO>() },
                    HotPodcasters = hotPodcasters ?? new TrendingPodcastFeedDTO.HotPodcastersTrendingPodcastFeedSection { PodcasterList = new List<AccountSnippetResponseDTO>() },
                    Category2 = dynamicCategories[1] ?? new TrendingPodcastFeedDTO.CategoryTrendingPodcastFeedSection { PodcastCategory = new PodcastCategoryDTO(), ShowList = new List<ShowListItemResponseDTO>() },
                    PopularChannels = popularChannels ?? new TrendingPodcastFeedDTO.PopularChannelsTrendingPodcastFeedSection { ChannelList = new List<ChannelListItemResponseDTO>() },
                    Category3 = dynamicCategories[2] ?? new TrendingPodcastFeedDTO.CategoryTrendingPodcastFeedSection { PodcastCategory = new PodcastCategoryDTO(), ShowList = new List<ShowListItemResponseDTO>() },
                    HotChannels = hotChannels ?? new TrendingPodcastFeedDTO.HotChannelsTrendingPodcastFeedSection { ChannelList = new List<ChannelListItemResponseDTO>() },
                    Category4 = dynamicCategories[3] ?? new TrendingPodcastFeedDTO.CategoryTrendingPodcastFeedSection { PodcastCategory = new PodcastCategoryDTO(), ShowList = new List<ShowListItemResponseDTO>() },
                    PopularShows = popularShows ?? new TrendingPodcastFeedDTO.PopularShowsTrendingPodcastFeedSection { ShowList = new List<ShowListItemResponseDTO>() },
                    Category5 = dynamicCategories[4] ?? new TrendingPodcastFeedDTO.CategoryTrendingPodcastFeedSection { PodcastCategory = new PodcastCategoryDTO(), ShowList = new List<ShowListItemResponseDTO>() },
                    HotShows = hotShows ?? new TrendingPodcastFeedDTO.HotShowsTrendingPodcastFeedSection { ShowList = new List<ShowListItemResponseDTO>() },
                    Category6 = dynamicCategories[5] ?? new TrendingPodcastFeedDTO.CategoryTrendingPodcastFeedSection { PodcastCategory = new PodcastCategoryDTO(), ShowList = new List<ShowListItemResponseDTO>() },
                    NewEpisodes = newEpisodes ?? new TrendingPodcastFeedDTO.NewEpisodesTrendingPodcastFeedSection { EpisodeList = new List<PodcastEpisodeSnippetResponseDTO>() },
                    PopularEpisodes = popularEpisodes ?? new TrendingPodcastFeedDTO.PopularEpisodesTrendingPodcastFeedSection { EpisodeList = new List<PodcastEpisodeSnippetResponseDTO>() }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[GetTrendingPodcastFeed] ERROR: {ex.Message}\n{ex.StackTrace}\n");
                throw new HttpRequestException("Error while getting trending podcast feed contents: " + ex.Message);
            }
        }

        #endregion

        #region Trending Cache Loading

        private async Task<CacheMetricsContainer> LoadTrendingCacheMetricsAsync()
        {
            Console.WriteLine("[LoadTrendingCacheMetrics] Loading cache metrics...");

            var tasks = new List<Task>();
            PodcasterAllTimeMaxQueryMetric? podcasterAllTime = null;
            PodcasterTemporal7dMaxQueryMetric? podcasterTemporal = null;
            ShowAllTimeMaxQueryMetric? showAllTime = null;
            ShowTemporal7dMaxQueryMetric? showTemporal = null;
            ChannelAllTimeMaxQueryMetric? channelAllTime = null;
            ChannelTemporal7dMaxQueryMetric? channelTemporal = null;

            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = _backgroundJobsConfig.PodcasterAllTimeMaxQueryMetricUpdateJob.RedisKeyName;
                podcasterAllTime = await _redisSharedCacheService.KeyGetAsync<PodcasterAllTimeMaxQueryMetric>(cacheKey);
            }));

            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = _backgroundJobsConfig.PodcasterTemporal7dMaxQueryMetricUpdateJob.RedisKeyName;
                podcasterTemporal = await _redisSharedCacheService.KeyGetAsync<PodcasterTemporal7dMaxQueryMetric>(cacheKey);
            }));

            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = _backgroundJobsConfig.ShowAllTimeMaxQueryMetricUpdateJob.RedisKeyName;
                showAllTime = await _redisSharedCacheService.KeyGetAsync<ShowAllTimeMaxQueryMetric>(cacheKey);
            }));

            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = _backgroundJobsConfig.ShowTemporal7dMaxQueryMetricUpdateJob.RedisKeyName;
                showTemporal = await _redisSharedCacheService.KeyGetAsync<ShowTemporal7dMaxQueryMetric>(cacheKey);
            }));

            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = _backgroundJobsConfig.ChannelAllTimeMaxQueryMetricUpdateJob.RedisKeyName;
                channelAllTime = await _redisSharedCacheService.KeyGetAsync<ChannelAllTimeMaxQueryMetric>(cacheKey);
            }));

            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = _backgroundJobsConfig.ChannelTemporal7dMaxQueryMetricUpdateJob.RedisKeyName;
                channelTemporal = await _redisSharedCacheService.KeyGetAsync<ChannelTemporal7dMaxQueryMetric>(cacheKey);
            }));

            await Task.WhenAll(tasks);

            return new CacheMetricsContainer
            {
                PodcasterAllTime = podcasterAllTime,
                PodcasterTemporal = podcasterTemporal,
                ShowAllTime = showAllTime,
                ShowTemporal = showTemporal,
                ChannelAllTime = channelAllTime,
                ChannelTemporal = channelTemporal
            };
        }

        private async Task<SystemPreferencesTemporal30dQueryMetric?> LoadSystemPreferencesAsync()
        {
            var cacheKey = _backgroundJobsConfig.SystemPreferencesTemporal30dQueryMetricUpdateJob.RedisKeyName;
            return await _redisSharedCacheService.KeyGetAsync<SystemPreferencesTemporal30dQueryMetric>(cacheKey);
        }

        #endregion

        #region Trending Section 1: Popular Podcasters

        private async Task<TrendingPodcastFeedDTO.PopularPodcastersTrendingPodcastFeedSection?> BuildTrendingPopularPodcastersSection(
            CacheMetricsContainer cacheMetrics)
        {
            try
            {
                Console.WriteLine("[TrendingPopularPodcasters] Building section...");

                // Fetch all verified podcasters
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
            {
                new BatchQueryItem
                {
                    Key = "verifiedPodcasters",
                    QueryType = "findall",
                    EntityType = "PodcasterProfile",
                    Parameters = JObject.FromObject(new { IsVerified = true })
                }
            }
                };

                var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
                var allPodcasters = result.Results["verifiedPodcasters"].ToObject<List<PodcasterProfileDTO>>();

                // Calculate popular scores
                var podcastersWithScore = CalculatePodcasterPopularScores(allPodcasters, cacheMetrics.PodcasterAllTime);

                var finalPodcasters = podcastersWithScore
                    .OrderByDescending(x => x.score)
                    .Take(10)
                    .Select(x => x.podcaster)
                    .ToList();

                // Fetch full account details
                var podcasterListItems = new List<AccountSnippetResponseDTO>();
                foreach (var podcaster in finalPodcasters)
                {
                    var account = await _accountCachingService.GetAccountStatusCacheById(podcaster.AccountId);
                    if (account != null && account.DeactivatedAt == null && account.HasVerifiedPodcasterProfile == true)
                    {
                        podcasterListItems.Add(new AccountSnippetResponseDTO
                        {
                            Id = account.Id,
                            FullName = account.PodcasterProfileName ?? account.FullName,
                            Email = account.Email,
                            MainImageFileKey = account.MainImageFileKey
                        });
                    }
                }

                Console.WriteLine($"[TrendingPopularPodcasters] Built with {podcasterListItems.Count} podcasters");

                return new TrendingPodcastFeedDTO.PopularPodcastersTrendingPodcastFeedSection
                {
                    PodcasterList = podcasterListItems
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrendingPopularPodcasters] ERROR: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Trending Section 2: Hot Podcasters

        private async Task<TrendingPodcastFeedDTO.HotPodcastersTrendingPodcastFeedSection?> BuildTrendingHotPodcastersSection(
            CacheMetricsContainer cacheMetrics)
        {
            try
            {
                Console.WriteLine("[TrendingHotPodcasters] Building section...");

                // Fetch all verified podcasters
                var batchRequest = new BatchQueryRequest
                {
                    Queries = new List<BatchQueryItem>
            {
                new BatchQueryItem
                {
                    Key = "verifiedPodcasters",
                    QueryType = "findall",
                    EntityType = "PodcasterProfile",
                    Parameters = JObject.FromObject(new { IsVerified = true })
                }
            }
                };

                var result = await _httpServiceQueryClient.ExecuteBatchAsync("UserService", batchRequest);
                var allPodcasters = result.Results["verifiedPodcasters"].ToObject<List<PodcasterProfileDTO>>();

                // Calculate hot scores
                var podcastersWithScore = await CalculatePodcasterHotScoresAsync(allPodcasters, cacheMetrics.PodcasterTemporal);

                var finalPodcasters = podcastersWithScore
                    .Where(x => x.score > 0)
                    .OrderByDescending(x => x.score)
                    .Take(8)
                    .Select(x => x.podcaster)
                    .ToList();

                // Fetch full account details
                var podcasterListItems = new List<AccountSnippetResponseDTO>();
                foreach (var podcaster in finalPodcasters)
                {
                    var account = await _accountCachingService.GetAccountStatusCacheById(podcaster.AccountId);
                    if (account != null && account.DeactivatedAt == null && account.HasVerifiedPodcasterProfile == true)
                    {
                        podcasterListItems.Add(new AccountSnippetResponseDTO
                        {
                            Id = account.Id,
                            FullName = account.PodcasterProfileName ?? account.FullName,
                            Email = account.Email,
                            MainImageFileKey = account.MainImageFileKey
                        });
                    }
                }

                Console.WriteLine($"[TrendingHotPodcasters] Built with {podcasterListItems.Count} podcasters");

                return new TrendingPodcastFeedDTO.HotPodcastersTrendingPodcastFeedSection
                {
                    PodcasterList = podcasterListItems
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrendingHotPodcasters] ERROR: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Trending Section 3: Popular Channels

        private async Task<TrendingPodcastFeedDTO.PopularChannelsTrendingPodcastFeedSection?> BuildTrendingPopularChannelsSection(
            CacheMetricsContainer cacheMetrics)
        {
            try
            {
                Console.WriteLine("[TrendingPopularChannels] Building section...");

                // Fetch all channels
                var allChannels = await _podcastChannelGenericRepository.FindAll(
                    predicate: pc => pc.DeletedAt == null,
                    includeFunc: q => q.Include(pc => pc.PodcastChannelStatusTrackings)
                ).ToListAsync();

                // Filter by Published status
                allChannels = allChannels.Where(pc =>
                {
                    var currentStatus = pc.PodcastChannelStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastChannelStatusId;
                    return currentStatus == (int)PodcastChannelStatusEnum.Published;
                }).ToList();

                // Calculate popular scores
                var channelsWithScore = CalculateChannelPopularScores(allChannels, cacheMetrics.ChannelAllTime);

                var finalChannels = channelsWithScore
                    .OrderByDescending(x => x.score)
                    .Take(8)
                    .Select(x => x.channel)
                    .ToList();

                var channelListItems = await MapToChannelListItemsAsync(finalChannels);

                Console.WriteLine($"[TrendingPopularChannels] Built with {channelListItems.Count} channels");

                return new TrendingPodcastFeedDTO.PopularChannelsTrendingPodcastFeedSection
                {
                    ChannelList = channelListItems
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrendingPopularChannels] ERROR: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Trending Section 4: Hot Channels

        private async Task<TrendingPodcastFeedDTO.HotChannelsTrendingPodcastFeedSection?> BuildTrendingHotChannelsSection(
            CacheMetricsContainer cacheMetrics)
        {
            try
            {
                Console.WriteLine("[TrendingHotChannels] Building section...");

                // Fetch all channels
                var allChannels = await _podcastChannelGenericRepository.FindAll(
                    predicate: pc => pc.DeletedAt == null,
                    includeFunc: q => q.Include(pc => pc.PodcastChannelStatusTrackings)
                ).ToListAsync();

                // Filter by Published status
                allChannels = allChannels.Where(pc =>
                {
                    var currentStatus = pc.PodcastChannelStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastChannelStatusId;
                    return currentStatus == (int)PodcastChannelStatusEnum.Published;
                }).ToList();

                // Calculate hot scores
                var channelsWithScore = await CalculateChannelHotScoresAsync(allChannels, cacheMetrics.ChannelTemporal);

                var finalChannels = channelsWithScore
                    .Where(x => x.score > 0)
                    .OrderByDescending(x => x.score)
                    .Take(8)
                    .Select(x => x.channel)
                    .ToList();

                var channelListItems = await MapToChannelListItemsAsync(finalChannels);

                Console.WriteLine($"[TrendingHotChannels] Built with {channelListItems.Count} channels");

                return new TrendingPodcastFeedDTO.HotChannelsTrendingPodcastFeedSection
                {
                    ChannelList = channelListItems
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrendingHotChannels] ERROR: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Trending Section 5: Popular Shows

        private async Task<TrendingPodcastFeedDTO.PopularShowsTrendingPodcastFeedSection?> BuildTrendingPopularShowsSection(
            CacheMetricsContainer cacheMetrics)
        {
            try
            {
                Console.WriteLine("[TrendingPopularShows] Building section...");

                // Fetch all shows
                var allShows = await _podcastShowGenericRepository.FindAll(
                    predicate: ps => ps.DeletedAt == null &&
                    (ps.PodcastChannel == null || ps.PodcastChannel.DeletedAt == null),
                    includeFunc: q => q
                        .Include(ps => ps.PodcastShowStatusTrackings)
                        .Include(ps => ps.PodcastChannel)
                            .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                ).ToListAsync();

                // Filter by Published status
                allShows = allShows.Where(ps =>
                {
                    var currentStatus = ps.PodcastShowStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastShowStatusId;
                    return currentStatus == (int)PodcastShowStatusEnum.Published &&
                        (ps.PodcastChannel == null || (ps.PodcastChannel.DeletedAt == null && ps.PodcastChannel.PodcastChannelStatusTrackings
                            .OrderByDescending(t => t.CreatedAt)
                            .FirstOrDefault()?.PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published));
                }).ToList();

                // Calculate popular scores
                var showsWithScore = CalculateShowPopularScores(allShows, cacheMetrics.ShowAllTime);

                var finalShows = showsWithScore
                    .OrderByDescending(x => x.score)
                    .Take(12)
                    .Select(x => x.show)
                    .ToList();

                var showListItems = await MapToShowListItemsAsync(finalShows);

                Console.WriteLine($"[TrendingPopularShows] Built with {showListItems.Count} shows");

                return new TrendingPodcastFeedDTO.PopularShowsTrendingPodcastFeedSection
                {
                    ShowList = showListItems
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrendingPopularShows] ERROR: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Trending Section 6: Hot Shows

        private async Task<TrendingPodcastFeedDTO.HotShowsTrendingPodcastFeedSection?> BuildTrendingHotShowsSection(
            CacheMetricsContainer cacheMetrics)
        {
            try
            {
                Console.WriteLine("[TrendingHotShows] Building section...");

                // Fetch all shows
                var allShows = await _podcastShowGenericRepository.FindAll(
                    predicate: ps => ps.DeletedAt == null &&
                    (ps.PodcastChannel == null || ps.PodcastChannel.DeletedAt == null),
                    includeFunc: q => q
                        .Include(ps => ps.PodcastShowStatusTrackings)
                        .Include(ps => ps.PodcastChannel)
                            .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                ).ToListAsync();

                // Filter by Published status
                allShows = allShows.Where(ps =>
                {
                    var currentStatus = ps.PodcastShowStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastShowStatusId;
                    return currentStatus == (int)PodcastShowStatusEnum.Published &&
                        (ps.PodcastChannel == null || (ps.PodcastChannel.DeletedAt == null && ps.PodcastChannel.PodcastChannelStatusTrackings
                            .OrderByDescending(t => t.CreatedAt)
                            .FirstOrDefault()?.PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published));
                }).ToList();

                // Calculate hot scores
                var showsWithScore = await CalculateShowHotScoresAsync(allShows, cacheMetrics.ShowTemporal);

                var finalShows = showsWithScore
                    .Where(x => x.score > 0)
                    .OrderByDescending(x => x.score)
                    .Take(10)
                    .Select(x => x.show)
                    .ToList();

                var showListItems = await MapToShowListItemsAsync(finalShows);

                Console.WriteLine($"[TrendingHotShows] Built with {showListItems.Count} shows");

                return new TrendingPodcastFeedDTO.HotShowsTrendingPodcastFeedSection
                {
                    ShowList = showListItems
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrendingHotShows] ERROR: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Trending Section 7: New Episodes

        private async Task<TrendingPodcastFeedDTO.NewEpisodesTrendingPodcastFeedSection?> BuildTrendingNewEpisodesSection()
        {
            try
            {
                Console.WriteLine("[TrendingNewEpisodes] Building section...");

                var now = _dateHelper.GetNowByAppTimeZone();
                var twoDaysAgo = now.AddDays(-2);

                // Query episodes published in last 2 days
                var episodes = await _podcastEpisodeGenericRepository.FindAll(
                    predicate: pe =>
                        pe.DeletedAt == null &&
                        pe.PodcastShow.DeletedAt == null &&
                        (pe.PodcastShow.PodcastChannel == null || pe.PodcastShow.PodcastChannel.DeletedAt == null),
                    includeFunc: q => q
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                            .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                            .ThenInclude(ps => ps.PodcastChannel)
                                .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                ).ToListAsync();

                // Filter by recent published tracking
                var recentEpisodes = episodes.Where(pe =>
                {
                    var publishedTracking = pe.PodcastEpisodeStatusTrackings
                        .Where(t => t.PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published)
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault();

                    if (publishedTracking == null || publishedTracking.CreatedAt < twoDaysAgo)
                        return false;

                    var showStatus = pe.PodcastShow.PodcastShowStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastShowStatusId;

                    var channelStatus = pe.PodcastShow.PodcastChannel == null ? (int?)null :
                        pe.PodcastShow.PodcastChannel.PodcastChannelStatusTrackings
                            .OrderByDescending(t => t.CreatedAt)
                            .FirstOrDefault()?.PodcastChannelStatusId;

                    return showStatus == (int)PodcastShowStatusEnum.Published &&
                           (channelStatus == null || channelStatus == (int)PodcastChannelStatusEnum.Published);
                })
                .OrderByDescending(pe => pe.PodcastEpisodeStatusTrackings
                    .Where(t => t.PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published)
                    .Max(t => t.CreatedAt))
                .ToList();

                // Fallback: extend to 7 days if less than 15
                if (recentEpisodes.Count < 15)
                {
                    var sevenDaysAgo = now.AddDays(-7);
                    var fallbackEpisodes = episodes.Where(pe =>
                    {
                        var publishedTracking = pe.PodcastEpisodeStatusTrackings
                            .Where(t => t.PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published)
                            .OrderByDescending(t => t.CreatedAt)
                            .FirstOrDefault();

                        if (publishedTracking == null || publishedTracking.CreatedAt < sevenDaysAgo)
                            return false;

                        var showStatus = pe.PodcastShow.PodcastShowStatusTrackings
                            .OrderByDescending(t => t.CreatedAt)
                            .FirstOrDefault()?.PodcastShowStatusId;

                        var channelStatus = pe.PodcastShow.PodcastChannel == null ? (int?)null :
                            pe.PodcastShow.PodcastChannel.PodcastChannelStatusTrackings
                                .OrderByDescending(t => t.CreatedAt)
                                .FirstOrDefault()?.PodcastChannelStatusId;

                        return showStatus == (int)PodcastShowStatusEnum.Published &&
                               (channelStatus == null || channelStatus == (int)PodcastChannelStatusEnum.Published);
                    })
                    .OrderByDescending(pe => pe.PodcastEpisodeStatusTrackings
                        .Where(t => t.PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published)
                        .Max(t => t.CreatedAt))
                    .ToList();

                    recentEpisodes = fallbackEpisodes.Take(15).ToList();
                }

                var finalEpisodes = recentEpisodes.Take(15).ToList();

                var episodeListItems = finalEpisodes.Select(ep => new PodcastEpisodeSnippetResponseDTO
                {
                    Id = ep.Id,
                    Name = ep.Name,
                    Description = ep.Description,
                    MainImageFileKey = ep.MainImageFileKey,
                    IsReleased = ep.IsReleased,
                    ReleaseDate = ep.ReleaseDate,
                    AudioLength = ep.AudioLength
                }).ToList();

                Console.WriteLine($"[TrendingNewEpisodes] Built with {episodeListItems.Count} episodes");

                return new TrendingPodcastFeedDTO.NewEpisodesTrendingPodcastFeedSection
                {
                    EpisodeList = episodeListItems
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrendingNewEpisodes] ERROR: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Trending Section 8: Popular Episodes

        private async Task<TrendingPodcastFeedDTO.PopularEpisodesTrendingPodcastFeedSection?> BuildTrendingPopularEpisodesSection(CacheMetricsContainer cacheMetrics)
        {
            try
            {
                Console.WriteLine("[TrendingPopularEpisodes] Building section...");

                // Part A: Top all-time (70% = 10 episodes)
                var allEpisodes = await _podcastEpisodeGenericRepository.FindAll(
                    predicate: pe =>
                        pe.DeletedAt == null &&
                        pe.PodcastShow.DeletedAt == null &&
                        (pe.PodcastShow.PodcastChannel == null || pe.PodcastShow.PodcastChannel.DeletedAt == null) &&
                        pe.ListenCount > 0,
                    includeFunc: q => q
                        .Include(pe => pe.PodcastEpisodeStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                            .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                        .Include(pe => pe.PodcastShow)
                            .ThenInclude(ps => ps.PodcastChannel)
                                .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                ).ToListAsync();

                // Filter by Published status
                var publishedEpisodes = allEpisodes.Where(pe =>
                {
                    var episodeStatus = pe.PodcastEpisodeStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastEpisodeStatusId;

                    var showStatus = pe.PodcastShow.PodcastShowStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastShowStatusId;

                    var channelStatus = pe.PodcastShow.PodcastChannel == null ? (int?)null :
                        pe.PodcastShow.PodcastChannel.PodcastChannelStatusTrackings
                            .OrderByDescending(t => t.CreatedAt)
                            .FirstOrDefault()?.PodcastChannelStatusId;

                    return episodeStatus == (int)PodcastEpisodeStatusEnum.Published &&
                           showStatus == (int)PodcastShowStatusEnum.Published &&
                           (channelStatus == null || channelStatus == (int)PodcastChannelStatusEnum.Published);
                }).ToList();

                // Calculate popular scores
                var episodesWithScore = CalculateEpisodePopularScores(publishedEpisodes, cacheMetrics.EpisodeAllTime);

                var partAEpisodes = episodesWithScore
                    .OrderByDescending(pe => pe.score)
                    .Take(10)
                    .Select(pe => pe.episode)
                    .ToList();

                // Part B: Diversity fallback (30% = 5 episodes from different shows)
                var partAShowIds = partAEpisodes.Select(ep => ep.PodcastShowId).ToHashSet();

                var partBEpisodes = episodesWithScore
                    .Where(pe => !partAShowIds.Contains(pe.episode.PodcastShowId))
                    .OrderByDescending(pe => pe.score)
                    .Take(5)
                    .Select(pe => pe.episode)
                    .ToList();

                var finalEpisodes = partAEpisodes.Concat(partBEpisodes).Take(15).ToList();

                var episodeListItems = finalEpisodes.Select(ep => new PodcastEpisodeSnippetResponseDTO
                {
                    Id = ep.Id,
                    Name = ep.Name,
                    Description = ep.Description,
                    MainImageFileKey = ep.MainImageFileKey,
                    IsReleased = ep.IsReleased,
                    ReleaseDate = ep.ReleaseDate,
                    AudioLength = ep.AudioLength
                }).ToList();

                Console.WriteLine($"[TrendingPopularEpisodes] Built with {episodeListItems.Count} episodes (A:{partAEpisodes.Count}, B:{partBEpisodes.Count})");

                return new TrendingPodcastFeedDTO.PopularEpisodesTrendingPodcastFeedSection
                {
                    EpisodeList = episodeListItems
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrendingPopularEpisodes] ERROR: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Trending Section 9-14: Dynamic Categories

        private async Task<List<TrendingPodcastFeedDTO.CategoryTrendingPodcastFeedSection?>> BuildTrendingDynamicCategorySectionsAsync(
            SystemPreferencesTemporal30dQueryMetric? systemPrefs,
            CacheMetricsContainer cacheMetrics)
        {
            try
            {
                Console.WriteLine("[TrendingDynamicCategories] Building 6 dynamic category sections...");

                // Select 6 stable random categories
                var selectedCategories = SelectStableRandomCategories(systemPrefs);

                if (selectedCategories == null || selectedCategories.Count == 0)
                {
                    Console.WriteLine("[TrendingDynamicCategories] No categories available");
                    return Enumerable.Range(0, 6).Select(_ => (TrendingPodcastFeedDTO.CategoryTrendingPodcastFeedSection?)null).ToList();
                }

                var results = new List<TrendingPodcastFeedDTO.CategoryTrendingPodcastFeedSection?>();

                foreach (var category in selectedCategories)
                {
                    var section = await BuildSingleDynamicCategorySection(category, cacheMetrics);
                    results.Add(section);
                }

                // FIX: Ensure we always return exactly 6 elements, fill with null if needed
                while (results.Count < 6)
                {
                    results.Add(null);
                }

                Console.WriteLine($"[TrendingDynamicCategories] Built {results.Count} category sections (actual: {selectedCategories.Count})");
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TrendingDynamicCategories] ERROR: {ex.Message}");
                return Enumerable.Range(0, 6).Select(_ => (TrendingPodcastFeedDTO.CategoryTrendingPodcastFeedSection?)null).ToList();
            }
        }

        private List<PodcastCategory> SelectStableRandomCategories(SystemPreferencesTemporal30dQueryMetric? systemPrefs)
        {
            if (systemPrefs?.ListenedPodcastCategories == null || !systemPrefs.ListenedPodcastCategories.Any())
            {
                Console.WriteLine("[SelectStableRandomCategories] No system preferences available");
                return new List<PodcastCategory>();
            }

            // Get top 10 trending categories
            var top10CategoryIds = systemPrefs.ListenedPodcastCategories
                .OrderByDescending(c => c.ListenCount)
                .Take(10)
                .Select(c => c.PodcastCategoryId)
                .ToList();

            // Calculate stable seed based on 6-hour window
            var now = _dateHelper.GetNowByAppTimeZone();
            var seed = (int)Math.Floor(now.Hour / 6.0);

            // Use seed to create stable random selection
            var random = new Random(seed + now.Year * 10000 + now.DayOfYear * 100);

            // Shuffle and take 6
            var selectedIds = top10CategoryIds
                .OrderBy(x => random.Next())
                .Take(6)
                .ToList();

            // Fetch category entities
            var categories = _podcastCategoryGenericRepository.FindAll(
                predicate: pc => selectedIds.Contains(pc.Id)
            ).ToList();

            Console.WriteLine($"[SelectStableRandomCategories] Selected {categories.Count} categories (seed={seed})");
            return categories;
        }

        private async Task<TrendingPodcastFeedDTO.CategoryTrendingPodcastFeedSection?> BuildSingleDynamicCategorySection(
            PodcastCategory category,
            CacheMetricsContainer cacheMetrics)
        {
            try
            {
                Console.WriteLine($"[DynamicCategory-{category.Id}] Building section for '{category.Name}'...");

                // Fetch all shows in category
                var shows = await _podcastShowGenericRepository.FindAll(
                    predicate: ps =>
                        ps.PodcastCategoryId == category.Id &&
                        ps.DeletedAt == null,
                    includeFunc: q => q
                        .Include(ps => ps.PodcastShowStatusTrackings)
                        .Include(ps => ps.PodcastChannel)
                            .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                ).ToListAsync();

                // Filter by Published status
                shows = shows.Where(ps =>
                {
                    var currentStatus = ps.PodcastShowStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastShowStatusId;
                    return currentStatus == (int)PodcastShowStatusEnum.Published &&
                        (ps.PodcastChannel == null || (ps.PodcastChannel.DeletedAt == null && ps.PodcastChannel.PodcastChannelStatusTrackings
                            .OrderByDescending(t => t.CreatedAt)
                            .FirstOrDefault()?.PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published));
                }).ToList();

                // Part A (40%): 4 hot shows
                var showsWithHotScore = await CalculateShowHotScoresAsync(shows, cacheMetrics.ShowTemporal);

                var partAShows = showsWithHotScore
                    .Where(x => x.score > 0)
                    .OrderByDescending(x => x.score)
                    .Take(4)
                    .Select(x => x.show)
                    .ToList();

                // Part B (60%): 6 popular shows
                var existingIds = partAShows.Select(s => s.Id).ToHashSet();
                var showsWithPopularScore = CalculateShowPopularScores(
                    shows.Where(s => !existingIds.Contains(s.Id)).ToList(),
                    cacheMetrics.ShowAllTime
                );

                var needed = 4 - partAShows.Count;
                var partBShows = showsWithPopularScore
                    .OrderByDescending(x => x.score)
                    .Take(6 + needed)
                    .Select(x => x.show)
                    .ToList();

                var finalShows = partAShows.Concat(partBShows).Take(10).ToList();

                var showListItems = await MapToShowListItemsAsync(finalShows);

                Console.WriteLine($"[DynamicCategory-{category.Id}] Built with {showListItems.Count} shows (A:{partAShows.Count}, B:{partBShows.Count})");

                // Return proper DTO type
                return new TrendingPodcastFeedDTO.CategoryTrendingPodcastFeedSection
                {
                    PodcastCategory = new PodcastCategoryDTO
                    {
                        Id = category.Id,
                        Name = category.Name,
                        MainImageFileKey = category.MainImageFileKey
                    },
                    ShowList = showListItems
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DynamicCategory-{category.Id}] ERROR: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Keyword search Entry Point
        public async Task<List<string>> GetPodcastKeywordSearchSuggestionsAsync(string prefix, int limit = 10)
        {
            try
            {
                // Normalize prefix: lowercase + trim
                prefix = prefix.ToLower().Trim();

                if (string.IsNullOrWhiteSpace(prefix))
                {
                    throw new ArgumentException("Prefix cannot be empty");
                }

                var cacheKey = "query:search_keyword:podcast_content:customer";

                // Get existing cache
                var cache = await _redisSharedCacheService.KeyGetAsync<CustomerRecordedPodcastContentSearchKeywordCache>(cacheKey);

                if (cache == null || cache.KeywordList == null || !cache.KeywordList.Any())
                {
                    return new List<string>();
                }

                // Find matching keywords
                var matchingKeywords = cache.KeywordList
                    .Where(x =>
                    {
                        // split khoảng trắng và kiểm tra từng từ có bắt đầu bằng prefix không
                        var words = x.Keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        return words.Any(w => w.StartsWith(prefix));
                    })
                    .OrderByDescending(x => x.SearchCount)
                    .Take(limit)
                    .Select(x => x.Keyword)
                    .ToList();

                Console.WriteLine($"[KeywordSearchSuggestions] Found {matchingKeywords.Count} suggestions for prefix '{prefix}'");

                return matchingKeywords;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KeywordSearchSuggestions] ERROR: {ex.Message}\n{ex.StackTrace}");
                throw new Exception("Error while fetching podcast keyword search suggestions: " + ex.Message);
            }
        }
        public async Task<bool> UpdatePodcastFeedContentKeywordSearchCacheAsync(string keyword)
        {
            try
            {
                // Normalize keyword: lowercase + trim
                keyword = keyword.ToLower().Trim();

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    throw new ArgumentException("Keyword cannot be empty");
                }

                var cacheKey = "query:search_keyword:podcast_content:customer";

                // Get existing cache
                var cache = await _redisSharedCacheService.KeyGetAsync<CustomerRecordedPodcastContentSearchKeywordCache>(cacheKey);

                // Initialize if not exists
                if (cache == null)
                {
                    cache = new CustomerRecordedPodcastContentSearchKeywordCache
                    {
                        KeywordList = new List<CustomerRecordedPodcastContentSearchKeywordCacheItem>()
                    };
                }

                // Find existing keyword
                var existingItem = cache.KeywordList.FirstOrDefault(x => x.Keyword == keyword);

                if (existingItem != null)
                {
                    // Increment search count
                    existingItem.SearchCount++;
                }
                else
                {
                    // Add new keyword with count = 1
                    cache.KeywordList.Add(new CustomerRecordedPodcastContentSearchKeywordCacheItem
                    {
                        Keyword = keyword,
                        SearchCount = 1
                    });
                }

                // Save back to cache (no expiration)
                await _redisSharedCacheService.KeySetAsync(cacheKey, cache, expiry: null);

                Console.WriteLine($"[KeywordSearch] Recorded keyword '{keyword}' (count: {existingItem?.SearchCount ?? 1})");

                // TODO: Implement actual search logic here
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KeywordSearch] ERROR: {ex.Message}\n{ex.StackTrace}");
                throw new Exception("Error while searching podcast contents by keyword: " + ex.Message);
            }
        }

        public async Task<List<PodcastShowOrEpisodeKeywordSearchedListItemResponseDTO>> GetPodcastFeedContentsByKeywordQueryAsync(string keyword, int limit)
        {
            try
            {
                Console.WriteLine($"[KeywordSearch] Starting search for keyword: '{keyword}'");

                // Normalize keyword
                keyword = keyword.ToLower().Trim();
                var queryTerms = keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // STEP 1: Load cache metrics for engagement scores
                var cacheMetrics = await LoadSearchCacheMetricsAsync();

                // STEP 2: Query all entities in parallel
                var shows = await QueryShowsForSearchAsync(queryTerms);
                var episodes = await QueryEpisodesForSearchAsync(queryTerms);

                Console.WriteLine($"[KeywordQuery] Found - Shows: {shows.Count}, Episodes: {episodes.Count}");

                // STEP 3: Calculate BM25 + Engagement scores
                var showScores = CalculateShowSearchScores(shows, queryTerms, cacheMetrics);
                var episodeScores = CalculateEpisodeSearchScores(episodes, queryTerms, cacheMetrics);

                // STEP 4: Filter by minimum threshold
                const double minScoreThreshold = 0.15;
                showScores = showScores.Where(x => x.finalScore >= minScoreThreshold).ToList();
                episodeScores = episodeScores.Where(x => x.finalScore >= minScoreThreshold).ToList();

                // STEP 5: Normalize scores for mixed results
                var (normalizedShows, normalizedEpisodes) = NormalizeScoresForMixedResults(showScores, episodeScores);

                // STEP 6: Build Top Query Results (mixed Show + Episode)
                var topQueryResults = BuildTopSearchResults(normalizedShows, normalizedEpisodes);

                // STEP 7: Map to DTOs
                var showList = await MapToShowListItemsAsync(showScores.OrderByDescending(x => x.finalScore).Select(x => x.show).ToList());
                var episodeList = await MapToEpisodeListItemsAsync(episodeScores.OrderByDescending(x => x.finalScore).Select(x => x.episode).ToList());

                Console.WriteLine($"[KeywordQuery] Results - Top: {topQueryResults.Count}, Shows: {showList.Count}, Episodes: {episodeList.Count}");

                return topQueryResults.Take(limit).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KeywordQuery] ERROR: {ex.Message}\n{ex.StackTrace}");
                throw new Exception("Error while query podcast contents by keyword: " + ex.Message);
            }

        }
        public async Task<PodcastContentKeywordSearchResultResponseDTO> GetPodcastFeedContentsByKeywordSearchAsync(string keyword)
        {
            try
            {
                await UpdatePodcastFeedContentKeywordSearchCacheAsync(keyword);
                Console.WriteLine($"[KeywordSearch] Starting search for keyword: '{keyword}'");

                // Normalize keyword
                keyword = keyword.ToLower().Trim();
                var searchTerms = keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // STEP 1: Load cache metrics for engagement scores
                var cacheMetrics = await LoadSearchCacheMetricsAsync();

                // STEP 2: Query all entities in parallel
                // var channelsTask = QueryChannelsForSearchAsync(searchTerms);
                // var showsTask = QueryShowsForSearchAsync(searchTerms);
                // var episodesTask = QueryEpisodesForSearchAsync(searchTerms);

                // await Task.WhenAll(channelsTask, showsTask, episodesTask);

                // var channels = await channelsTask;
                // var shows = await showsTask;
                // var episodes = await episodesTask;
                var channels = await QueryChannelsForSearchAsync(searchTerms);
                var shows = await QueryShowsForSearchAsync(searchTerms);
                var episodes = await QueryEpisodesForSearchAsync(searchTerms);

                Console.WriteLine($"[KeywordSearch] Found - Channels: {channels.Count}, Shows: {shows.Count}, Episodes: {episodes.Count}");

                // STEP 3: Calculate BM25 + Engagement scores
                var channelScores = CalculateChannelSearchScores(channels, searchTerms, cacheMetrics);
                var showScores = CalculateShowSearchScores(shows, searchTerms, cacheMetrics);
                var episodeScores = CalculateEpisodeSearchScores(episodes, searchTerms, cacheMetrics);

                foreach (var cs in channelScores)
                {
                    Console.WriteLine($"[KeywordSearch] Channel '{cs.channel.Name}' - BM25: {cs.bm25Score:F4}, Engagement: {cs.engagementScore:F4}, Final: {cs.finalScore:F4}");
                }
                foreach (var ss in showScores)
                {
                    Console.WriteLine($"[KeywordSearch] Show '{ss.show.Name}' - BM25: {ss.bm25Score:F4}, Engagement: {ss.engagementScore:F4}, Final: {ss.finalScore:F4}");
                }
                foreach (var es in episodeScores)
                {
                    Console.WriteLine($"[KeywordSearch] Episode '{es.episode.Name}' - BM25: {es.bm25Score:F4}, Engagement: {es.engagementScore:F4}, Final: {es.finalScore:F4}");
                }

                // // STEP 4: Filter by minimum threshold
                // const double minScoreThreshold = 0.15;
                // channelScores = channelScores.Where(x => x.finalScore >= minScoreThreshold).ToList();
                // showScores = showScores.Where(x => x.finalScore >= minScoreThreshold).ToList();
                // episodeScores = episodeScores.Where(x => x.finalScore >= minScoreThreshold).ToList();

                // // STEP 5: Normalize scores for mixed results
                // var (normalizedShows, normalizedEpisodes) = NormalizeScoresForMixedResults(showScores, episodeScores);

                // // STEP 6: Build Top Search Results (mixed Show + Episode)
                // var topSearchResults = BuildTopSearchResults(normalizedShows, normalizedEpisodes);

                // // STEP 7: Map to DTOs
                // var channelList = await MapToChannelListItemsAsync(channelScores.OrderByDescending(x => x.finalScore).Select(x => x.channel).ToList());
                // var showList = await MapToShowListItemsAsync(showScores.OrderByDescending(x => x.finalScore).Select(x => x.show).ToList());
                // var episodeList = await MapToEpisodeListItemsAsync(episodeScores.OrderByDescending(x => x.finalScore).Select(x => x.episode).ToList());


                // STEP 4: PHASE 1 - Build TopSearchResults with 4-Tier Adaptive Balancing (20 items)
                var topShowScores = ApplyAdaptiveBalancing(showScores, targetCount: 20, entityType: "Show");
                var topEpisodeScores = ApplyAdaptiveBalancing(episodeScores, targetCount: 20, entityType: "Episode");

                // STEP 5: Normalize and build TopSearchResults
                var (normalizedShows, normalizedEpisodes) = NormalizeScoresForMixedResults(topShowScores, topEpisodeScores);
                var topSearchResults = BuildTopSearchResults(normalizedShows, normalizedEpisodes);

                // STEP 6: PHASE 2 - Build Full Lists (ALL items, no filtering)
                Console.WriteLine($"[KeywordSearch] Full Lists - Channels: {channelScores.Count}, Shows: {showScores.Count}, Episodes: {episodeScores.Count}");

                var channelList = await MapToChannelListItemsAsync(
                    channelScores.OrderByDescending(x => x.finalScore).Select(x => x.channel).ToList());
                var showList = await MapToShowListItemsAsync(
                    showScores.OrderByDescending(x => x.finalScore).Select(x => x.show).ToList());
                var episodeList = await MapToEpisodeListItemsAsync(
                    episodeScores.OrderByDescending(x => x.finalScore).Select(x => x.episode).ToList());

                Console.WriteLine($"[KeywordSearch] Results - Top: {topSearchResults.Count}, Channels: {channelList.Count}, Shows: {showList.Count}, Episodes: {episodeList.Count}");

                return new PodcastContentKeywordSearchResultResponseDTO
                {
                    TopSearchResults = topSearchResults,
                    ChannelList = channelList,
                    ShowList = showList,
                    EpisodeList = episodeList
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[KeywordSearch] ERROR: {ex.Message}\n{ex.StackTrace}");
                throw new Exception("Error while searching podcast contents by keyword: " + ex.Message);
            }
        }

        #endregion
        #region Keyword Search Helpers

        /// <summary>
        /// 4-Tier Adaptive Balancing cho TopSearchResults
        /// TIER 1: finalScore >= 0.15 (strict quality)
        /// TIER 2: finalScore >= 0.08 (relaxed quality) 
        /// TIER 3: bm25Score >= 0.05 (relevance-focused, catch new content)
        /// TIER 4: ALL matched items (fallback, never empty)
        /// </summary>
        private List<(PodcastShow show, double bm25Score, double engagementScore, double finalScore)> ApplyAdaptiveBalancing(
            List<(PodcastShow show, double bm25Score, double engagementScore, double finalScore)> showScores,
            int targetCount,
            string entityType)
        {
            if (!showScores.Any())
            {
                Console.WriteLine($"[AdaptiveBalancing-{entityType}] No items to process");
                return new List<(PodcastShow, double, double, double)>();
            }

            // TIER 1: Strict quality
            var tier1 = showScores.Where(x => x.finalScore >= 0.15).ToList();
            if (tier1.Count >= targetCount)
            {
                Console.WriteLine($"[AdaptiveBalancing-{entityType}] ✅ TIER 1: {tier1.Count} items >= 0.15");
                return tier1.OrderByDescending(x => x.finalScore).Take(targetCount).ToList();
            }

            // TIER 2: Relaxed quality
            var tier2 = showScores.Where(x => x.finalScore >= 0.08).ToList();
            if (tier2.Count >= targetCount)
            {
                Console.WriteLine($"[AdaptiveBalancing-{entityType}] ⚠️ TIER 2: {tier2.Count} items >= 0.08");
                return tier2.OrderByDescending(x => x.finalScore).Take(targetCount).ToList();
            }

            // TIER 3: BM25-only (catch new content)
            var tier3 = showScores.Where(x => x.bm25Score >= 0.05).ToList();
            if (tier3.Count >= targetCount / 2) // Minimum 10 items for target=20
            {
                Console.WriteLine($"[AdaptiveBalancing-{entityType}] 🔍 TIER 3: {tier3.Count} items with BM25 >= 0.05");
                return tier3.OrderByDescending(x => x.finalScore).Take(targetCount).ToList();
            }

            // TIER 4: Fallback - return all available
            Console.WriteLine($"[AdaptiveBalancing-{entityType}] 🆘 TIER 4: Returning all {showScores.Count} items");
            return showScores.OrderByDescending(x => x.finalScore).Take(targetCount).ToList();
        }

        // Overload cho Episode
        private List<(PodcastEpisode episode, double bm25Score, double engagementScore, double finalScore)> ApplyAdaptiveBalancing(
            List<(PodcastEpisode episode, double bm25Score, double engagementScore, double finalScore)> episodeScores,
            int targetCount,
            string entityType)
        {
            if (!episodeScores.Any())
            {
                Console.WriteLine($"[AdaptiveBalancing-{entityType}] No items to process");
                return new List<(PodcastEpisode, double, double, double)>();
            }

            // TIER 1
            var tier1 = episodeScores.Where(x => x.finalScore >= 0.15).ToList();
            if (tier1.Count >= targetCount)
            {
                Console.WriteLine($"[AdaptiveBalancing-{entityType}] ✅ TIER 1: {tier1.Count} items >= 0.15");
                return tier1.OrderByDescending(x => x.finalScore).Take(targetCount).ToList();
            }

            // TIER 2
            var tier2 = episodeScores.Where(x => x.finalScore >= 0.08).ToList();
            if (tier2.Count >= targetCount)
            {
                Console.WriteLine($"[AdaptiveBalancing-{entityType}] ⚠️ TIER 2: {tier2.Count} items >= 0.08");
                return tier2.OrderByDescending(x => x.finalScore).Take(targetCount).ToList();
            }

            // TIER 3
            var tier3 = episodeScores.Where(x => x.bm25Score >= 0.05).ToList();
            if (tier3.Count >= targetCount / 2)
            {
                Console.WriteLine($"[AdaptiveBalancing-{entityType}] 🔍 TIER 3: {tier3.Count} items with BM25 >= 0.05");
                return tier3.OrderByDescending(x => x.finalScore).Take(targetCount).ToList();
            }

            // TIER 4
            Console.WriteLine($"[AdaptiveBalancing-{entityType}] 🆘 TIER 4: Returning all {episodeScores.Count} items");
            return episodeScores.OrderByDescending(x => x.finalScore).Take(targetCount).ToList();
        }

        private async Task<(
            ChannelAllTimeMaxQueryMetric? channelAllTime,
            ShowAllTimeMaxQueryMetric? showAllTime,
            EpisodeAllTimeMaxQueryMetric? episodeAllTime
        )> LoadSearchCacheMetricsAsync()
        {
            Console.WriteLine("[LoadSearchCacheMetrics] Loading cache metrics...");

            ChannelAllTimeMaxQueryMetric? channelAllTime = null;
            ShowAllTimeMaxQueryMetric? showAllTime = null;
            EpisodeAllTimeMaxQueryMetric? episodeAllTime = null;

            var tasks = new List<Task>();

            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = _backgroundJobsConfig.ChannelAllTimeMaxQueryMetricUpdateJob.RedisKeyName;
                channelAllTime = await _redisSharedCacheService.KeyGetAsync<ChannelAllTimeMaxQueryMetric>(cacheKey);
            }));

            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = _backgroundJobsConfig.ShowAllTimeMaxQueryMetricUpdateJob.RedisKeyName;
                showAllTime = await _redisSharedCacheService.KeyGetAsync<ShowAllTimeMaxQueryMetric>(cacheKey);
            }));

            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = _backgroundJobsConfig.EpisodeAllTimeMaxQueryMetricUpdateJob.RedisKeyName;
                episodeAllTime = await _redisSharedCacheService.KeyGetAsync<EpisodeAllTimeMaxQueryMetric>(cacheKey);
            }));

            await Task.WhenAll(tasks);

            return (channelAllTime, showAllTime, episodeAllTime);
        }

        // hàm xử lí tiếng việt chuyển về không dấu và như bảng chữ cái tiếng anh
        private string RemoveVietnameseDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Bước 1: Normalize về FormD (tách base letter + diacritics)
            var normalized = text.Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder();

            foreach (var ch in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            // Bước 2: Normalize về FormC
            var result = sb.ToString().Normalize(NormalizationForm.FormC);

            // Bước 3: Xử lý riêng chữ đ/Đ
            result = result.Replace("đ", "d").Replace("Đ", "D");

            return result;
        }

        private async Task<List<PodcastChannel>> QueryChannelsForSearchAsync(string[] searchTerms)
        {
            var channels = await _podcastChannelGenericRepository.FindAll(
                predicate: pc => pc.DeletedAt == null,
                includeFunc: q => q
                    .Include(pc => pc.PodcastChannelStatusTrackings)
                    .Include(pc => pc.PodcastChannelHashtags)
                        .ThenInclude(pch => pch.Hashtag)
            ).ToListAsync();

            // Filter by Published status
            channels = channels.Where(pc =>
            {
                var currentStatus = pc.PodcastChannelStatusTrackings
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefault()?.PodcastChannelStatusId;
                return currentStatus == (int)PodcastChannelStatusEnum.Published;
            }).ToList();

            // Get podcaster info
            var podcasterIds = channels.Select(c => c.PodcasterId).Distinct().ToList();
            var podcasterAccounts = new Dictionary<int, AccountStatusCache>();

            foreach (var id in podcasterIds)
            {
                var account = await _accountCachingService.GetAccountStatusCacheById(id);
                if (account != null && account.DeactivatedAt == null && account.HasVerifiedPodcasterProfile == true)
                {
                    podcasterAccounts[id] = account;
                }
            }


            // Filter channels that have valid podcasters and match search terms
            channels = channels.Where(c =>
            {
                if (!podcasterAccounts.ContainsKey(c.PodcasterId))
                    return false;

                var podcaster = podcasterAccounts[c.PodcasterId];
                var matchesKeyword = searchTerms.Any(term =>
                    c.Name.ToLower().Contains(term) ||
                    c.Description.ToLower().Contains(term) ||
                    podcaster.FullName.ToLower().Contains(term) ||
                    podcaster.PodcasterProfileName?.ToLower().Contains(term) == true ||
                    (c.PodcastChannelHashtags?.Any(pch => pch.Hashtag.Name.ToLower().Contains(RemoveVietnameseDiacritics(term))) ?? false)
                );

                return matchesKeyword;
            }).ToList();
            return channels;
        }

        private async Task<List<PodcastShow>> QueryShowsForSearchAsync(string[] searchTerms)
        {
            var shows = await _podcastShowGenericRepository.FindAll(
                predicate: ps => ps.DeletedAt == null,
                includeFunc: q => q
                    .Include(ps => ps.PodcastShowStatusTrackings)
                    .Include(ps => ps.PodcastChannel)
                        .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                    .Include(ps => ps.PodcastShowHashtags)
                        .ThenInclude(psh => psh.Hashtag)
            ).ToListAsync();

            // Filter by Published status
            shows = shows.Where(ps =>
            {
                var currentStatus = ps.PodcastShowStatusTrackings
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefault()?.PodcastShowStatusId;
                var channelValid = ps.PodcastChannel == null ||
                    (ps.PodcastChannel.DeletedAt == null &&
                     ps.PodcastChannel.PodcastChannelStatusTrackings
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.PodcastChannelStatusId == (int)PodcastChannelStatusEnum.Published);
                return currentStatus == (int)PodcastShowStatusEnum.Published && channelValid;
            }).ToList();

            // Get podcaster info
            var podcasterIds = shows.Select(s => s.PodcasterId).Distinct().ToList();
            var podcasterAccounts = new Dictionary<int, AccountStatusCache>();

            foreach (var id in podcasterIds)
            {
                var account = await _accountCachingService.GetAccountStatusCacheById(id);
                if (account != null && account.DeactivatedAt == null && account.HasVerifiedPodcasterProfile == true)
                {
                    podcasterAccounts[id] = account;
                }
            }

            // Filter shows that have valid podcasters and match search terms
            shows = shows.Where(s =>
            {
                if (!podcasterAccounts.ContainsKey(s.PodcasterId))
                    return false;

                var podcaster = podcasterAccounts[s.PodcasterId];
                var matchesKeyword = searchTerms.Any(term =>
                    s.Name.ToLower().Contains(term) ||
                    s.Description.ToLower().Contains(term) ||
                    podcaster.FullName.ToLower().Contains(term) ||
                    podcaster.PodcasterProfileName?.ToLower().Contains(term) == true ||
                    s.PodcastChannel?.Name.ToLower().Contains(term) == true ||
                    (s.PodcastShowHashtags?.Any(psh => psh.Hashtag.Name.ToLower().Contains(RemoveVietnameseDiacritics(term))) ?? false)
                );

                return matchesKeyword;
            }).ToList();
            return shows;
        }

        private async Task<List<PodcastEpisode>> QueryEpisodesForSearchAsync(string[] searchTerms)
        {
            var episodes = await _podcastEpisodeGenericRepository.FindAll(
                predicate: pe =>
                    pe.DeletedAt == null &&
                    pe.PodcastShow.DeletedAt == null,
                includeFunc: q => q
                    .Include(pe => pe.PodcastEpisodeStatusTrackings)
                    .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastShowStatusTrackings)
                    .Include(pe => pe.PodcastShow)
                        .ThenInclude(ps => ps.PodcastChannel)
                            .ThenInclude(pc => pc.PodcastChannelStatusTrackings)
                    .Include(pe => pe.PodcastEpisodeSubscriptionType)
                    .Include(pe => pe.PodcastEpisodeHashtags)
                        .ThenInclude(peh => peh.Hashtag)
            ).ToListAsync();

            // Filter by Published status
            episodes = episodes.Where(pe =>
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

                return episodeStatus == (int)PodcastEpisodeStatusEnum.Published &&
                       showStatus == (int)PodcastShowStatusEnum.Published &&
                       channelValid;
            }).ToList();

            // Get podcaster info
            var podcasterIds = episodes.Select(e => e.PodcastShow.PodcasterId).Distinct().ToList();
            var podcasterAccounts = new Dictionary<int, AccountStatusCache>();

            foreach (var id in podcasterIds)
            {
                var account = await _accountCachingService.GetAccountStatusCacheById(id);
                if (account != null && account.DeactivatedAt == null && account.HasVerifiedPodcasterProfile == true)
                {
                    podcasterAccounts[id] = account;
                }
            }

            // Filter episodes that match search terms
            episodes = episodes.Where(e =>
            {
                if (!podcasterAccounts.ContainsKey(e.PodcastShow.PodcasterId))
                    return false;

                var podcaster = podcasterAccounts[e.PodcastShow.PodcasterId];
                var matchesKeyword = searchTerms.Any(term =>
                    e.Name.ToLower().Contains(term) ||
                    e.Description.ToLower().Contains(term) ||
                    e.PodcastShow.Name.ToLower().Contains(term) ||
                    podcaster.FullName.ToLower().Contains(term) ||
                    podcaster.PodcasterProfileName?.ToLower().Contains(term) == true ||
                    (e.PodcastEpisodeHashtags?.Any(peh => peh.Hashtag.Name.ToLower().Contains(RemoveVietnameseDiacritics(term))) ?? false)
                );

                return matchesKeyword;
            }).ToList();
            return episodes;
        }

        private List<(PodcastChannel channel, double bm25Score, double engagementScore, double finalScore)> CalculateChannelSearchScores(
            List<PodcastChannel> channels,
            string[] searchTerms,
            (ChannelAllTimeMaxQueryMetric? channelAllTime, ShowAllTimeMaxQueryMetric? showAllTime, EpisodeAllTimeMaxQueryMetric? episodeAllTime) cacheMetrics)
        {
            var results = new List<(PodcastChannel, double, double, double)>();

            if (cacheMetrics.channelAllTime == null)
            {
                Console.WriteLine("[ChannelSearchScores] WARNING: Cache metrics not available");
                return results;
            }

            // Calculate average document length for BM25
            var avgNameLength = channels.Count() > 0 ? channels.Average(c => c.Name.Length) : 0;
            var avgDescLength = channels.Count() > 0 ? channels.Average(c => c.Description.Length) : 0;

            foreach (var channel in channels)
            {
                // BM25 scores for each field
                double bm25Name = CalculateBM25ForField(channel.Name, searchTerms, channels.Count,
                    GetDocumentFrequency(searchTerms, channels.Select(c => c.Name).ToList()),
                    avgNameLength, k1: 1.5, b: 0.75);

                double bm25Desc = CalculateBM25ForField(channel.Description, searchTerms, channels.Count,
                    GetDocumentFrequency(searchTerms, channels.Select(c => c.Description).ToList()),
                    avgDescLength, k1: 1.5, b: 0.75);

                // Weighted BM25 score
                double bm25Score = 5.0 * bm25Name + 2.0 * bm25Desc;

                // Engagement score
                double engagementScore = 0.6 * (channel.ListenCount / (double)Math.Max(1, cacheMetrics.channelAllTime.MaxListenCount)) +
                                        0.4 * (channel.TotalFavorite / (double)Math.Max(1, cacheMetrics.channelAllTime.MaxTotalFavorite));

                // Final score
                double finalScore = 0.65 * bm25Score + 0.35 * engagementScore;

                results.Add((channel, bm25Score, engagementScore, finalScore));
            }

            return results;
        }

        private List<(PodcastShow show, double bm25Score, double engagementScore, double finalScore)> CalculateShowSearchScores(
            List<PodcastShow> shows,
            string[] searchTerms,
            (ChannelAllTimeMaxQueryMetric? channelAllTime, ShowAllTimeMaxQueryMetric? showAllTime, EpisodeAllTimeMaxQueryMetric? episodeAllTime) cacheMetrics)
        {
            var results = new List<(PodcastShow, double, double, double)>();

            if (cacheMetrics.showAllTime == null)
            {
                Console.WriteLine("[ShowSearchScores] WARNING: Cache metrics not available");
                return results;
            }

            var avgNameLength = shows.Count() > 0 ? shows.Average(s => s.Name.Length) : 0;
            var avgDescLength = shows.Count() > 0 ? shows.Average(s => s.Description.Length) : 0;

            foreach (var show in shows)
            {
                double bm25Name = CalculateBM25ForField(show.Name, searchTerms, shows.Count,
                    GetDocumentFrequency(searchTerms, shows.Select(s => s.Name).ToList()),
                    avgNameLength, k1: 1.5, b: 0.75);

                double bm25Desc = CalculateBM25ForField(show.Description, searchTerms, shows.Count,
                    GetDocumentFrequency(searchTerms, shows.Select(s => s.Description).ToList()),
                    avgDescLength, k1: 1.5, b: 0.75);

                double bm25Score = 5.0 * bm25Name + 2.0 * bm25Desc;

                // Engagement score with recency boost
                double ratingTerm = show.AverageRating * Math.Log(show.RatingCount + 1);
                double recencyBoost = GetRecencyBoostForShow(show.PodcastShowStatusTrackings);

                double engagementScore = 0.3 * (show.TotalFollow / (double)Math.Max(1, cacheMetrics.showAllTime.MaxTotalFollow)) +
                                        0.3 * (show.ListenCount / (double)Math.Max(1, cacheMetrics.showAllTime.MaxListenCount)) +
                                        0.2 * (ratingTerm / Math.Max(0.001, cacheMetrics.showAllTime.MaxRatingTerm)) +
                                        0.2 * recencyBoost;

                double finalScore = 0.65 * bm25Score + 0.35 * engagementScore;

                results.Add((show, bm25Score, engagementScore, finalScore));
            }

            return results;
        }

        private List<(PodcastEpisode episode, double bm25Score, double engagementScore, double finalScore)> CalculateEpisodeSearchScores(
            List<PodcastEpisode> episodes,
            string[] searchTerms,
            (ChannelAllTimeMaxQueryMetric? channelAllTime, ShowAllTimeMaxQueryMetric? showAllTime, EpisodeAllTimeMaxQueryMetric? episodeAllTime) cacheMetrics)
        {
            var results = new List<(PodcastEpisode, double, double, double)>();

            if (cacheMetrics.episodeAllTime == null)
            {
                Console.WriteLine("[EpisodeSearchScores] WARNING: Cache metrics not available");
                return results;
            }

            var avgNameLength = episodes.Count() > 0 ? episodes.Average(e => e.Name.Length) : 0;
            var avgDescLength = episodes.Count() > 0 ? episodes.Average(e => e.Description.Length) : 0;

            foreach (var episode in episodes)
            {
                double bm25Name = CalculateBM25ForField(episode.Name, searchTerms, episodes.Count,
                    GetDocumentFrequency(searchTerms, episodes.Select(e => e.Name).ToList()),
                    avgNameLength, k1: 1.5, b: 0.75);

                double bm25Desc = CalculateBM25ForField(episode.Description, searchTerms, episodes.Count,
                    GetDocumentFrequency(searchTerms, episodes.Select(e => e.Description).ToList()),
                    avgDescLength, k1: 1.5, b: 0.75);

                double bm25Score = 5.0 * bm25Name + 2.0 * bm25Desc;

                // Engagement score with recency boost
                double recencyBoost = GetRecencyBoostForEpisode(episode.PodcastEpisodeStatusTrackings);

                double engagementScore = 0.5 * (episode.ListenCount / (double)Math.Max(1, cacheMetrics.episodeAllTime.MaxListenCount)) +
                                        0.3 * (episode.TotalSave / (double)Math.Max(1, cacheMetrics.episodeAllTime.MaxTotalSave)) +
                                        0.2 * recencyBoost;

                double finalScore = 0.65 * bm25Score + 0.35 * engagementScore;

                results.Add((episode, bm25Score, engagementScore, finalScore));
            }

            return results;
        }

        private double CalculateBM25ForField(string text, string[] terms, int totalDocs, Dictionary<string, int> docFreq, double avgLength, double k1, double b)
        {
            text = text.ToLower();
            double score = 0;

            foreach (var term in terms)
            {
                // Calculate IDF
                int df = docFreq.ContainsKey(term) ? docFreq[term] : 0;
                if (df == 0) continue;

                double idf = Math.Log((totalDocs - df + 0.5) / (df + 0.5) + 1);

                // Calculate TF
                int tf = CountOccurrences(text, term);
                if (tf == 0) continue;

                // BM25 formula
                double docLength = text.Length;
                double normalizedTF = (tf * (k1 + 1)) / (tf + k1 * (1 - b + b * (docLength / avgLength)));

                score += idf * normalizedTF;
            }

            return score;
        }

        private int CountOccurrences(string text, string term)
        {
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(term, index, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                count++;
                index += term.Length;
            }
            return count;
        }

        private Dictionary<string, int> GetDocumentFrequency(string[] terms, List<string> documents)
        {
            var docFreq = new Dictionary<string, int>();

            foreach (var term in terms)
            {
                int count = documents.Count(doc => doc.ToLower().Contains(term));
                docFreq[term] = count;
            }

            return docFreq;
        }

        private double GetRecencyBoost<T>(ICollection<T> statusTrackings) where T : class
        {
            var publishedTracking = statusTrackings
                .OrderByDescending(t => (DateTime)t.GetType().GetProperty("CreatedAt").GetValue(t))
                .FirstOrDefault(t =>
                {
                    var statusId = (int)t.GetType().GetProperty(t.GetType().Name.Replace("Tracking", "Id")).GetValue(t);
                    return statusId == (int)PodcastShowStatusEnum.Published ||
                           statusId == (int)PodcastEpisodeStatusEnum.Published;
                });

            if (publishedTracking == null) return 0;

            var publishedAt = (DateTime)publishedTracking.GetType().GetProperty("CreatedAt").GetValue(publishedTracking);
            var daysSince = (_dateHelper.GetNowByAppTimeZone() - publishedAt).TotalDays;

            if (daysSince <= 7) return 1.0;
            if (daysSince <= 30) return 0.5;
            return 0;
        }

        private double GetRecencyBoostForShow(ICollection<PodcastShowStatusTracking> trackings)
        {
            var publishedTracking = trackings
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefault(t => t.PodcastShowStatusId == (int)PodcastShowStatusEnum.Published);

            if (publishedTracking == null) return 0;

            var daysSince = (_dateHelper.GetNowByAppTimeZone() - publishedTracking.CreatedAt).TotalDays;

            if (daysSince <= 7) return 1.0;
            if (daysSince <= 30) return 0.5;
            return 0;
        }

        private double GetRecencyBoostForEpisode(ICollection<PodcastEpisodeStatusTracking> trackings)
        {
            var publishedTracking = trackings
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefault(t => t.PodcastEpisodeStatusId == (int)PodcastEpisodeStatusEnum.Published);

            if (publishedTracking == null) return 0;

            var daysSince = (_dateHelper.GetNowByAppTimeZone() - publishedTracking.CreatedAt).TotalDays;

            if (daysSince <= 7) return 1.0;
            if (daysSince <= 30) return 0.5;
            return 0;
        }

        private (
            List<(PodcastShow show, double normalizedScore)> normalizedShows,
            List<(PodcastEpisode episode, double normalizedScore)> normalizedEpisodes
        ) NormalizeScoresForMixedResults(
            List<(PodcastShow show, double bm25Score, double engagementScore, double finalScore)> showScores,
            List<(PodcastEpisode episode, double bm25Score, double engagementScore, double finalScore)> episodeScores)
        {
            var maxShowScore = showScores.Any() ? showScores.Max(x => x.finalScore) : 1.0;
            var maxEpisodeScore = episodeScores.Any() ? episodeScores.Max(x => x.finalScore) : 1.0;

            var normalizedShows = showScores.Select(x => (x.show, x.finalScore / maxShowScore)).ToList();
            var normalizedEpisodes = episodeScores.Select(x => (x.episode, x.finalScore / maxEpisodeScore)).ToList();

            return (normalizedShows, normalizedEpisodes);
        }

        private List<PodcastShowOrEpisodeKeywordSearchedListItemResponseDTO> BuildTopSearchResults(
            List<(PodcastShow show, double normalizedScore)> normalizedShows,
            List<(PodcastEpisode episode, double normalizedScore)> normalizedEpisodes)
        {
            var mixed = new List<(object entity, double score, bool isShow)>();

            foreach (var (show, score) in normalizedShows)
            {
                mixed.Add((show, score, true));
            }

            foreach (var (episode, score) in normalizedEpisodes)
            {
                mixed.Add((episode, score, false));
            }

            var topResults = mixed
                .OrderByDescending(x => x.score)
                .Take(20)
                .Select(x =>
                {
                    if (x.isShow)
                    {
                        var show = (PodcastShow)x.entity;
                        return new PodcastShowOrEpisodeKeywordSearchedListItemResponseDTO
                        {
                            Show = new PodcastShowSnippetResponseDTO
                            {
                                Id = show.Id,
                                Name = show.Name,
                                Description = show.Description,
                                MainImageFileKey = show.MainImageFileKey,
                                IsReleased = show.IsReleased,
                                ReleaseDate = show.ReleaseDate
                            },
                            Episode = null
                        };
                    }
                    else
                    {
                        var episode = (PodcastEpisode)x.entity;
                        return new PodcastShowOrEpisodeKeywordSearchedListItemResponseDTO
                        {
                            Show = null,
                            Episode = new PodcastEpisodeSnippetResponseDTO
                            {
                                Id = episode.Id,
                                Name = episode.Name,
                                Description = episode.Description,
                                MainImageFileKey = episode.MainImageFileKey,
                                IsReleased = episode.IsReleased,
                                ReleaseDate = episode.ReleaseDate,
                                AudioLength = episode.AudioLength
                            }
                        };
                    }
                })
                .ToList();

            return topResults;
        }

        private async Task<List<EpisodeListItemResponseDTO>> MapToEpisodeListItemsAsync(List<PodcastEpisode> episodes)
        {
            var result = new List<EpisodeListItemResponseDTO>();

            // Collect unique IDs
            var showIds = episodes.Select(e => e.PodcastShowId).Distinct().ToList();
            var podcasterIds = episodes.Select(e => e.PodcastShow.PodcasterId).Distinct().ToList();

            // Batch fetch podcasters
            var podcasterAccounts = new Dictionary<int, AccountStatusCache>();
            foreach (var id in podcasterIds)
            {
                var account = await _accountCachingService.GetAccountStatusCacheById(id);
                if (account != null)
                {
                    podcasterAccounts[id] = account;
                }
            }

            // Map each episode
            foreach (var episode in episodes)
            {
                var show = episode.PodcastShow;
                if (show == null) continue;

                var podcasterAccount = podcasterAccounts.ContainsKey(show.PodcasterId)
                    ? podcasterAccounts[show.PodcasterId]
                    : null;
                if (podcasterAccount == null) continue;

                var currentStatus = episode.PodcastEpisodeStatusTrackings
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefault();

                var item = new EpisodeListItemResponseDTO
                {
                    Id = episode.Id,
                    Name = episode.Name,
                    Description = episode.Description,
                    AudioLength = episode.AudioLength,
                    AudioFileKey = episode.AudioFileKey,
                    MainImageFileKey = episode.MainImageFileKey,
                    ListenCount = episode.ListenCount,
                    TotalSave = episode.TotalSave,
                    IsReleased = episode.IsReleased,
                    ReleaseDate = episode.ReleaseDate,
                    CreatedAt = episode.CreatedAt,
                    UpdatedAt = episode.UpdatedAt,


                    PodcastEpisodeSubscriptionType = new PodcastEpisodeSubscriptionTypeDTO
                    {
                        Id = episode.PodcastEpisodeSubscriptionType.Id,
                        Name = episode.PodcastEpisodeSubscriptionType.Name,
                    },
                    AudioFileSize = episode.AudioFileSize,
                    EpisodeOrder = episode.EpisodeOrder,
                    ExplicitContent = episode.ExplicitContent,
                    Hashtags = episode.PodcastEpisodeHashtags?
                        .Select(peh => new HashtagDTO
                        {
                            Id = peh.Hashtag.Id,
                            Name = peh.Hashtag.Name
                        }).ToList(),
                    IsAudioPublishable = episode.IsAudioPublishable,
                    SeasonNumber = episode.SeasonNumber,
                    TakenDownReason = episode.TakenDownReason,
                    PodcastShow = new PodcastShowSnippetResponseDTO
                    {
                        Id = show.Id,
                        Name = show.Name,
                        Description = show.Description,
                        MainImageFileKey = show.MainImageFileKey,
                        IsReleased = show.IsReleased,
                        ReleaseDate = show.ReleaseDate
                    },

                    CurrentStatus = new PodcastEpisodeStatusDTO
                    {
                        Id = currentStatus.PodcastEpisodeStatusId,
                        Name = ((PodcastEpisodeStatusEnum)currentStatus.PodcastEpisodeStatusId).ToString()
                    }
                };

                result.Add(item);
            }

            return result;
        }

        #endregion

        #region Category Base Entry Point

        public async Task<CategoryBasePodcastFeedDTO> GetPodcastFeedContentsByPodcastCategoryIdAsync(int podcastCategoryId)
        {
            try
            {
                Console.WriteLine($"[CategoryBaseFeed] Starting for CategoryId={podcastCategoryId}");

                // STEP 1: Validate category exists
                var category = await _podcastCategoryGenericRepository.FindByIdAsync(podcastCategoryId);
                if (category == null)
                {
                    Console.WriteLine($"[CategoryBaseFeed] Category {podcastCategoryId} not found");
                    throw new Exception($"Category with ID {podcastCategoryId} not found");
                }

                // STEP 2: Load cache metrics (reuse Trending cache)
                var cacheMetrics = await LoadTrendingCacheMetricsAsync();

                // STEP 3: Build fixed sections (independent - NO deduplication)
                var topChannels = await BuildCategoryBaseTopChannelsSection(podcastCategoryId, cacheMetrics);
                var topShows = await BuildCategoryBaseTopShowsSection(podcastCategoryId, cacheMetrics);
                var hotShows = await BuildCategoryBaseHotShowsSection(podcastCategoryId, cacheMetrics);
                var topEpisodes = await BuildCategoryBaseTopEpisodesSection(podcastCategoryId, cacheMetrics);

                // STEP 4: Collect dedup IDs from TopShows + HotShows
                var dedupShowIds = topShows.Concat(hotShows).Select(s => s.Id).ToHashSet();

                // STEP 5: Build dynamic subcategory sections (with deduplication)
                var subcategorySections = await BuildCategoryBaseSubcategorySections(
                    podcastCategoryId, cacheMetrics, dedupShowIds);

                Console.WriteLine($"[CategoryBaseFeed] Completed for CategoryId={podcastCategoryId}");

                return new CategoryBasePodcastFeedDTO
                {
                    PodcastCategory = new PodcastCategoryDTO
                    {
                        Id = category.Id,
                        Name = category.Name,
                        MainImageFileKey = category.MainImageFileKey
                    },
                    TopChannels = topChannels,
                    TopShows = topShows,
                    HotShows = hotShows,
                    TopEpisodes = topEpisodes,
                    SubCategorySections = subcategorySections
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CategoryBaseFeed] ERROR: {ex.Message}\n{ex.StackTrace}");
                throw new HttpRequestException("Error while getting category base podcast feed: " + ex.Message);
            }
        }

        #endregion

        #region Category Base - Top Channels (Popular Score, 16 items)
        private async Task<List<ChannelListItemResponseDTO>> BuildCategoryBaseTopChannelsSection(
            int podcastCategoryId,
            CacheMetricsContainer cacheMetrics)
        {
            try
            {
                Console.WriteLine($"[CategoryBase-TopChannels] Loading channels for CategoryId={podcastCategoryId}");

                // Query all channels with navigation properties
                var allChannels = await _podcastChannelGenericRepository.FindAll(
                    predicate: c => c.DeletedAt == null,
                    includeFunc: query => query
                        .Include(c => c.PodcastChannelStatusTrackings)
                        .Include(c => c.PodcastShows)
                            .ThenInclude(s => s.PodcastShowStatusTrackings)
                        .Include(c => c.PodcastShows)
                            .ThenInclude(s => s.PodcastSubCategory)
                ).ToListAsync();

                // Filter channels that have shows in this category
                var categoryChannels = allChannels
                    .Where(c =>
                    {
                        // Get current channel status
                        var channelStatus = c.PodcastChannelStatusTrackings
                            ?.OrderByDescending(st => st.CreatedAt)
                            .FirstOrDefault()?.PodcastChannelStatusId;

                        if (channelStatus != (int)PodcastChannelStatusEnum.Published) return false;

                        // Has at least one published show in target category
                        return c.PodcastShows?.Any(s =>
                        {
                            var showStatus = s.PodcastShowStatusTrackings
                                ?.OrderByDescending(st => st.CreatedAt)
                                .FirstOrDefault()?.PodcastShowStatusId;

                            return s.DeletedAt == null &&
                                   showStatus == (int)PodcastShowStatusEnum.Published &&
                                   s.PodcastSubCategory?.PodcastCategoryId == podcastCategoryId;
                        }) == true;
                    })
                    .ToList();

                // Calculate popular scores (all-time)
                var channelsWithScore = CalculateChannelPopularScores(categoryChannels, cacheMetrics.ChannelAllTime);

                var finalChannels = channelsWithScore
                    .OrderByDescending(x => x.score)
                    .Take(16)
                    .Select(x => x.channel)
                    .ToList();

                // Map to DTOs
                var result = await MapToChannelListItemsAsync(finalChannels);

                Console.WriteLine($"[CategoryBase-TopChannels] Loaded {result.Count} channels");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CategoryBase-TopChannels] ERROR: {ex.Message}");
                return new List<ChannelListItemResponseDTO>();
            }
        }
        #endregion

        #region Category Base - Top Shows (Popular Score, 24 items)
        private async Task<List<ShowListItemResponseDTO>> BuildCategoryBaseTopShowsSection(
            int podcastCategoryId,
            CacheMetricsContainer cacheMetrics)
        {
            try
            {
                Console.WriteLine($"[CategoryBase-TopShows] Loading shows for CategoryId={podcastCategoryId}");

                // Query all shows with navigation properties
                var allShows = await _podcastShowGenericRepository.FindAll(
                    predicate: s => s.DeletedAt == null,
                    includeFunc: query => query
                        .Include(s => s.PodcastShowStatusTrackings)
                        .Include(s => s.PodcastSubCategory)
                        .Include(s => s.PodcastChannel)
                            .ThenInclude(c => c.PodcastChannelStatusTrackings)
                ).ToListAsync();

                // Filter shows in this category
                var categoryShows = allShows
                    .Where(s =>
                    {
                        // Must belong to target category
                        if (s.PodcastSubCategory?.PodcastCategoryId != podcastCategoryId) return false;

                        // Get current show status
                        var showStatus = s.PodcastShowStatusTrackings
                            ?.OrderByDescending(st => st.CreatedAt)
                            .FirstOrDefault()?.PodcastShowStatusId;

                        if (showStatus != (int)PodcastShowStatusEnum.Published) return false;

                        // If has channel, check channel status
                        if (s.PodcastChannel != null)
                        {
                            var channelStatus = s.PodcastChannel.PodcastChannelStatusTrackings
                                ?.OrderByDescending(st => st.CreatedAt)
                                .FirstOrDefault()?.PodcastChannelStatusId;

                            return channelStatus == (int)PodcastChannelStatusEnum.Published;
                        }

                        return true; // Show without channel is OK
                    })
                    .ToList();

                // Calculate popular scores (all-time)
                var showsWithScore = CalculateShowPopularScores(categoryShows, cacheMetrics.ShowAllTime);

                var finalShows = showsWithScore
                    .OrderByDescending(x => x.score)
                    .Take(24)
                    .Select(x => x.show)
                    .ToList();

                // Map to DTOs
                var result = await MapToShowListItemsAsync(finalShows);

                Console.WriteLine($"[CategoryBase-TopShows] Loaded {result.Count} shows");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CategoryBase-TopShows] ERROR: {ex.Message}");
                return new List<ShowListItemResponseDTO>();
            }
        }
        #endregion

        #region Category Base - Hot Shows (Hot Score 7d, 20 items, NO FILTER score > 0)
        private async Task<List<ShowListItemResponseDTO>> BuildCategoryBaseHotShowsSection(
            int podcastCategoryId,
            CacheMetricsContainer cacheMetrics)
        {
            try
            {
                Console.WriteLine($"[CategoryBase-HotShows] Loading shows for CategoryId={podcastCategoryId}");

                // Query all shows with navigation properties
                var allShows = await _podcastShowGenericRepository.FindAll(
                    predicate: s => s.DeletedAt == null,
                    includeFunc: query => query
                        .Include(s => s.PodcastShowStatusTrackings)
                        .Include(s => s.PodcastSubCategory)
                        .Include(s => s.PodcastChannel)
                            .ThenInclude(c => c!.PodcastChannelStatusTrackings)
                ).ToListAsync();

                // Filter shows in this category
                var categoryShows = allShows
                    .Where(s =>
                    {
                        // Must belong to target category
                        if (s.PodcastSubCategory?.PodcastCategoryId != podcastCategoryId) return false;

                        // Get current show status
                        var showStatus = s.PodcastShowStatusTrackings
                            ?.OrderByDescending(st => st.CreatedAt)
                            .FirstOrDefault()?.PodcastShowStatusId;

                        if (showStatus != (int)PodcastShowStatusEnum.Published) return false;

                        // If has channel, check channel status
                        if (s.PodcastChannel != null)
                        {
                            var channelStatus = s.PodcastChannel.PodcastChannelStatusTrackings
                                ?.OrderByDescending(st => st.CreatedAt)
                                .FirstOrDefault()?.PodcastChannelStatusId;

                            return channelStatus == (int)PodcastChannelStatusEnum.Published;
                        }

                        return true; // Show without channel is OK
                    })
                    .ToList();

                // Calculate hot scores (7-day window)
                var showsWithScore = await CalculateShowHotScoresAsync(categoryShows, cacheMetrics.ShowTemporal);

                // IMPORTANT: NO score > 0 filter - take all 20 sorted by score (guarantee 20 results)
                var finalShows = showsWithScore
                    .OrderByDescending(x => x.score)
                    .Take(20)
                    .Select(x => x.show)
                    .ToList();

                // Map to DTOs
                var result = await MapToShowListItemsAsync(finalShows);

                Console.WriteLine($"[CategoryBase-HotShows] Loaded {result.Count} shows");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CategoryBase-HotShows] ERROR: {ex.Message}");
                return new List<ShowListItemResponseDTO>();
            }
        }
        #endregion

        #region Category Base - Top Episodes (Popular Score, 30 items)
        private async Task<List<EpisodeListItemResponseDTO>> BuildCategoryBaseTopEpisodesSection(
            int podcastCategoryId,
            CacheMetricsContainer cacheMetrics)
        {
            try
            {
                Console.WriteLine($"[CategoryBase-TopEpisodes] Loading episodes for CategoryId={podcastCategoryId}");

                // Query all episodes with navigation properties
                var allEpisodes = await _podcastEpisodeGenericRepository.FindAll(
                    predicate: e => e.DeletedAt == null,
                    includeFunc: query => query
                        .Include(e => e.PodcastEpisodeStatusTrackings)
                        .Include(e => e.PodcastShow)
                            .ThenInclude(s => s!.PodcastShowStatusTrackings)
                        .Include(e => e.PodcastShow)
                            .ThenInclude(s => s!.PodcastSubCategory)
                        .Include(e => e.PodcastShow)
                            .ThenInclude(s => s!.PodcastChannel)
                                .ThenInclude(c => c!.PodcastChannelStatusTrackings)
                ).ToListAsync();

                // Filter episodes in this category
                var categoryEpisodes = allEpisodes
                    .Where(e =>
                    {
                        // Must have show
                        if (e.PodcastShow == null) return false;

                        // Must belong to target category
                        if (e.PodcastShow.PodcastSubCategory?.PodcastCategoryId != podcastCategoryId) return false;

                        // Get current episode status
                        var episodeStatus = e.PodcastEpisodeStatusTrackings
                            ?.OrderByDescending(st => st.CreatedAt)
                            .FirstOrDefault()?.PodcastEpisodeStatusId;

                        if (episodeStatus != (int)PodcastEpisodeStatusEnum.Published) return false;

                        // Get current show status
                        var showStatus = e.PodcastShow.PodcastShowStatusTrackings
                            ?.OrderByDescending(st => st.CreatedAt)
                            .FirstOrDefault()?.PodcastShowStatusId;

                        if (showStatus != (int)PodcastShowStatusEnum.Published) return false;

                        // If has channel, check channel status
                        if (e.PodcastShow.PodcastChannel != null)
                        {
                            var channelStatus = e.PodcastShow.PodcastChannel.PodcastChannelStatusTrackings
                                ?.OrderByDescending(st => st.CreatedAt)
                                .FirstOrDefault()?.PodcastChannelStatusId;

                            return channelStatus == (int)PodcastChannelStatusEnum.Published;
                        }

                        return true; // Show without channel is OK
                    })
                    .ToList();

                // Calculate popular scores (all-time)
                var episodesWithScore = CalculateEpisodePopularScores(categoryEpisodes, cacheMetrics.EpisodeAllTime);

                var finalEpisodes = episodesWithScore
                    .OrderByDescending(x => x.score)
                    .Take(30)
                    .Select(x => x.episode)
                    .ToList();

                // Map to DTOs
                var result = await MapToEpisodeListItemsAsync(finalEpisodes);

                Console.WriteLine($"[CategoryBase-TopEpisodes] Loaded {result.Count} episodes");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CategoryBase-TopEpisodes] ERROR: {ex.Message}");
                return new List<EpisodeListItemResponseDTO>();
            }
        }
        #endregion

        #region Category Base - Subcategory Sections (Dynamic, 20 shows each, exclude TopShows + HotShows)
        private async Task<List<CategoryBasePodcastFeedDTO.SubCategoryCategoryBasePodcastFeedSection>> BuildCategoryBaseSubcategorySections(
            int podcastCategoryId,
            CacheMetricsContainer cacheMetrics,
            HashSet<Guid> dedupShowIds)
        {
            try
            {
                Console.WriteLine($"[CategoryBase-Subcategories] Loading subcategories for CategoryId={podcastCategoryId}");

                // Get all subcategories for this category (SubCategory has no DeletedAt field)
                var allSubcategories = await _podcastSubCategoryGenericRepository.FindAll(
                    predicate: sc => sc.PodcastCategoryId == podcastCategoryId,
                    includeFunc: null
                ).ToListAsync();

                Console.WriteLine($"[CategoryBase-Subcategories] Found {allSubcategories.Count} subcategories");

                var sections = new List<CategoryBasePodcastFeedDTO.SubCategoryCategoryBasePodcastFeedSection>();

                foreach (var subcategory in allSubcategories)
                {
                    Console.WriteLine($"[CategoryBase-Subcategories] Processing subcategoryId={subcategory.Id}");

                    // Query shows for this subcategory
                    var allShows = await _podcastShowGenericRepository.FindAll(
                        predicate: s => s.DeletedAt == null &&
                                       s.PodcastSubCategoryId == subcategory.Id &&
                                       !dedupShowIds.Contains(s.Id), // Exclude TopShows + HotShows
                        includeFunc: query => query
                            .Include(s => s.PodcastShowStatusTrackings)
                            .Include(s => s.PodcastChannel)
                                .ThenInclude(c => c!.PodcastChannelStatusTrackings)
                    ).ToListAsync();

                    // Filter by status
                    var publishedShows = allShows
                        .Where(s =>
                        {
                            // Get current show status
                            var showStatus = s.PodcastShowStatusTrackings
                                ?.OrderByDescending(st => st.CreatedAt)
                                .FirstOrDefault()?.PodcastShowStatusId;

                            if (showStatus != (int)PodcastShowStatusEnum.Published) return false;

                            // If has channel, check channel status
                            if (s.PodcastChannel != null)
                            {
                                var channelStatus = s.PodcastChannel.PodcastChannelStatusTrackings
                                    ?.OrderByDescending(st => st.CreatedAt)
                                    .FirstOrDefault()?.PodcastChannelStatusId;

                                return channelStatus == (int)PodcastChannelStatusEnum.Published;
                            }

                            return true; // Show without channel is OK
                        })
                        .ToList();

                    // Calculate popular scores (all-time)
                    var showsWithScore = CalculateShowPopularScores(publishedShows, cacheMetrics.ShowAllTime);

                    var finalShows = showsWithScore
                        .OrderByDescending(x => x.score)
                        .Take(20)
                        .Select(x => x.show)
                        .ToList();

                    // Map to DTOs
                    var showListItems = await MapToShowListItemsAsync(finalShows);

                    // Map subcategory DTO manually
                    var subcategoryDTO = new PodcastSubCategoryDTO
                    {
                        Id = subcategory.Id,
                        Name = subcategory.Name,
                        PodcastCategoryId = subcategory.PodcastCategoryId
                    };

                    sections.Add(new CategoryBasePodcastFeedDTO.SubCategoryCategoryBasePodcastFeedSection
                    {
                        PodcastSubCategory = subcategoryDTO,
                        ShowList = showListItems
                    });

                    Console.WriteLine($"[CategoryBase-Subcategories] SubcategoryId={subcategory.Id} completed with {showListItems.Count} shows");
                }

                Console.WriteLine($"[CategoryBase-Subcategories] Completed {sections.Count} sections");
                return sections;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CategoryBase-Subcategories] ERROR: {ex.Message}");
                return new List<CategoryBasePodcastFeedDTO.SubCategoryCategoryBasePodcastFeedSection>();
            }
        }
        #endregion

    }
}