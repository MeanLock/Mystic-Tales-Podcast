using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastService.API.Filters.ExceptionFilters;
using PodcastService.BusinessLogic.DTOs.Hashtag;
using PodcastService.BusinessLogic.Models.CrossService;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using PodcastService.BusinessLogic.Services.DbServices.PodcastServices;

namespace PodcastService.API.Controllers.BaseControllers
{
    [Route("api/hash-tags")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class HashtagController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly HashtagService _hashtagService;

        public HashtagController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient, HashtagService hashtagService)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _hashtagService = hashtagService;
        }

        // /api/podcast-service/api/hash-tags
        [HttpGet]
        public async Task<IActionResult> GetAllHashtagsAsync([FromQuery] string? keyword = null)
        {
            var hashtags = await _hashtagService.GetAllHashtagsByKeywordAsync(keyword);
            return Ok(new
            {
                HashtagList = hashtags
            });
        }

        // /api/podcast-service/api/hash-tags
        [HttpPost]
        public async Task<IActionResult> CreateHashtagAsync([FromBody] HashtagCreateRequestDTO hashtagCreateRequestDTO)
        {
            var createdHashtag = await _hashtagService.CreateHashtagAsync(hashtagCreateRequestDTO);
            return Ok(new
            {
                NewHashtag = createdHashtag
            });
        }


    }
}
