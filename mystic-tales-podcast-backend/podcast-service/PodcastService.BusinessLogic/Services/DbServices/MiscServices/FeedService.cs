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
                    ContinueListening = continueListening,
                    BasedOnYourTaste = basedOnYourTaste,
                    NewReleases = newReleases,
                    HotThisWeek = hotThisWeek,
                    TopSubCategory = topSubCategory,
                    TopPodcasters = topPodcasters,
                    RandomCategory = randomCategory,
                    TalentedRookies = talentedRookies
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
                ChannelTemporal = channelTemporal
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
                        ls.IsCompleted == false &&
                        ls.LastListenDurationSeconds < ls.PodcastEpisode.AudioLength &&
                        ls.IsContentRemoved == false &&
                        ls.PodcastEpisode.DeletedAt == null &&
                        ls.PodcastEpisode.PodcastShow.DeletedAt == null,
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

                // Get unique podcaster IDs
                var podcasterIds = listenSessions
                    .Select(ls => ls.PodcastEpisode.PodcastShow.PodcasterId)
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
                            MainImageFileKey = ls.PodcastEpisode.MainImageFileKey,
                            IsReleased = ls.PodcastEpisode.IsReleased,
                            ReleaseDate = ls.PodcastEpisode.ReleaseDate
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
                        Name = randomCategory.Name
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

                var popularEpisodes = await BuildTrendingPopularEpisodesSection();

                // STEP 3: Build 6 dynamic category sections with stable random
                var dynamicCategories = await BuildTrendingDynamicCategorySectionsAsync(systemPrefs, cacheMetrics);

                Console.WriteLine("[GetTrendingPodcastFeed] Completed successfully");

                return new TrendingPodcastFeedDTO
                {
                    PopularPodcasters = popularPodcasters,
                    Category1 = dynamicCategories[0],
                    HotPodcasters = hotPodcasters,
                    Category2 = dynamicCategories[1],
                    PopularChannels = popularChannels,
                    Category3 = dynamicCategories[2],
                    HotChannels = hotChannels,
                    Category4 = dynamicCategories[3],
                    PopularShows = popularShows,
                    Category5 = dynamicCategories[4],
                    HotShows = hotShows,
                    Category6 = dynamicCategories[5],
                    NewEpisodes = newEpisodes,
                    PopularEpisodes = popularEpisodes
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
                    predicate: ps => ps.DeletedAt == null,
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
                    predicate: ps => ps.DeletedAt == null,
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
                        pe.PodcastShow.DeletedAt == null,
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
                    MainImageFileKey = ep.MainImageFileKey,
                    IsReleased = ep.IsReleased,
                    ReleaseDate = ep.ReleaseDate
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

        private async Task<TrendingPodcastFeedDTO.PopularEpisodesTrendingPodcastFeedSection?> BuildTrendingPopularEpisodesSection()
        {
            try
            {
                Console.WriteLine("[TrendingPopularEpisodes] Building section...");

                // Part A: Top all-time (70% = 10 episodes)
                var allEpisodes = await _podcastEpisodeGenericRepository.FindAll(
                    predicate: pe =>
                        pe.DeletedAt == null &&
                        pe.PodcastShow.DeletedAt == null &&
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

                var partAEpisodes = publishedEpisodes
                    .OrderByDescending(pe => pe.ListenCount)
                    .Take(10)
                    .ToList();

                // Part B: Diversity fallback (30% = 5 episodes from different shows)
                var partAShowIds = partAEpisodes.Select(ep => ep.PodcastShowId).ToHashSet();

                var partBEpisodes = publishedEpisodes
                    .Where(pe => !partAShowIds.Contains(pe.PodcastShowId))
                    .OrderByDescending(pe => pe.ListenCount)
                    .Take(5)
                    .ToList();

                var finalEpisodes = partAEpisodes.Concat(partBEpisodes).Take(15).ToList();

                var episodeListItems = finalEpisodes.Select(ep => new PodcastEpisodeSnippetResponseDTO
                {
                    Id = ep.Id,
                    Name = ep.Name,
                    MainImageFileKey = ep.MainImageFileKey,
                    IsReleased = ep.IsReleased,
                    ReleaseDate = ep.ReleaseDate
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
                        Name = category.Name
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
    }
}