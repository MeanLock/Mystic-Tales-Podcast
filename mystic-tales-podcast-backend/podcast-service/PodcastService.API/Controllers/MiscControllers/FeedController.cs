using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastService.API.Filters.ExceptionFilters;
using PodcastService.BusinessLogic.DTOs.Cache;
using PodcastService.BusinessLogic.Helpers.FileHelpers;
using PodcastService.BusinessLogic.Services.DbServices.PodcastServices;
using PodcastService.BusinessLogic.Services.MessagingServices.interfaces;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;
using PodcastService.DataAccess.Data;
using PodcastService.Infrastructure.Services.Kafka;

namespace PodcastService.API.Controllers.MiscControllers
{
    [Route("api/misc/feed")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class FeedController : ControllerBase
    {
        private readonly ILogger<FeedController> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly FeedService _feedService;
        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;
        public FeedController(ILogger<FeedController> logger, KafkaProducerService kafkaProducerService, IMessagingService messagingService, FeedService feedService, IFileValidationConfig fileValidationConfig, IFilePathConfig filePathConfig, FileIOHelper fileIOHelper)
        {
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;

            _feedService = feedService;

            _fileValidationConfig = fileValidationConfig;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
        }




        // /api/podcast-service/api/misc/feed/podcast-contents/discovery
        [HttpGet("podcast-contents/discovery")]
        public async Task<IActionResult> GetDiscoveryPodcastFeedContents()
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var feedContents = await _feedService.GetDiscoveryPodcastFeedContentsAsync(account);

            return Ok(feedContents);
        }


        // /api/podcast-service/api/misc/feed/podcast-contents/trending
        [HttpGet("podcast-contents/trending")]
        public async Task<IActionResult> GetTrendingPodcastFeedContents()
        {
            // var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            var feedContents = await _feedService.GetTrendingPodcastFeedContentsAsync();

            return Ok(feedContents);
        }

        // /api/podcast-service/api/misc/feed/search-keyword-suggestion
        [HttpGet("search-keyword-suggestion")]
        public async Task<IActionResult> GetPodcastKeywordSearchSuggestions([FromQuery] string keyword, [FromQuery] int limit = 10)
        {
            var suggestions = await _feedService.GetPodcastKeywordSearchSuggestionsAsync(keyword, limit);

            return Ok(suggestions);
        }

        // /api/podcast-service/api/misc/feed/podcast-contents/keyword-search
        [HttpGet("podcast-contents/keyword-search")]
        public async Task<IActionResult> GetPodcastFeedContentsByKeywordSearch([FromQuery] string keyword)
        {

            var feedContents = await _feedService.GetPodcastFeedContentsByKeywordSearchAsync(keyword);

            return Ok(feedContents);
        }

        // /api/podcast-service/api/misc/feed/podcast-contents/podcast-categories/{PodcastCategoryId}
        [HttpGet("podcast-contents/podcast-categories/{podcastCategoryId}")]
        public async Task<IActionResult> GetPodcastFeedContentsByPodcastCategoryId([FromRoute] int podcastCategoryId)
        {

            var feedContents = await _feedService.GetPodcastFeedContentsByPodcastCategoryIdAsync(podcastCategoryId);

            return Ok(feedContents);
        }


    }
}
