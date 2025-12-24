using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastService.API.Filters.ExceptionFilters;
using PodcastService.BusinessLogic.Enums.App;
using PodcastService.BusinessLogic.Helpers.FileHelpers;
using PodcastService.BusinessLogic.Services.DbServices.PodcastServices;
using PodcastService.BusinessLogic.Services.MessagingServices.interfaces;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;
using PodcastService.DataAccess.Data;
using PodcastService.Infrastructure.Services.Kafka;

namespace PodcastService.API.Controllers.BaseControllers
{
    [Route("api/categories")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class CategoryController : ControllerBase
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly PodcastBackgroundSoundTrackService _backgroundSoundTrackService;
        private readonly PodcastCategoryService _podcastCategoryService;
        private readonly IFileValidationConfig _fileValidationConfig;
        private readonly IFilePathConfig _filePathConfig;
        private readonly FileIOHelper _fileIOHelper;
        public CategoryController(ILogger<CategoryController> logger, KafkaProducerService kafkaProducerService, IMessagingService messagingService, PodcastBackgroundSoundTrackService backgroundSoundTrackService, PodcastCategoryService podcastCategoryService, IFileValidationConfig fileValidationConfig, IFilePathConfig filePathConfig, FileIOHelper fileIOHelper)
        {
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _backgroundSoundTrackService = backgroundSoundTrackService;
            _podcastCategoryService = podcastCategoryService;
            _fileValidationConfig = fileValidationConfig;
            _filePathConfig = filePathConfig;
            _fileIOHelper = fileIOHelper;
        }


        // /api/podcast-service/api/misc/category/podcast-categories
        [HttpGet("podcast-categories")]
        public async Task<IActionResult> GetPodcastCategories()
        {
            var categories = await _podcastCategoryService.GetAllPodcastCategoriesAsync();

            return Ok(new
            {
                PodcastCategoryList = categories
            });
        }

        // /api/podcast-service/api/misc/category/podcast-sub-categories
        [HttpGet("podcast-sub-categories")]
        public async Task<IActionResult> GetPodcastSubCategories()
        {
            var subCategories = await _podcastCategoryService.GetAllPodcastSubCategoriesAsync();

            return Ok(new
            {
                PodcastSubCategoryList = subCategories
            });
        }

        // /api/podcast-service/api/categories/podcast-categories/get-file-url/{**FileKey}
        [HttpGet("podcast-categories/get-file-url/{**FileKey}")]
        public async Task<IActionResult> GetPodcastCategoryFileUrl([FromRoute] string FileKey)
        {
            var (category, accessLevel) = FileAccessValidator.GetFileCategoryAndLevel(FileKey);

            if (category != FileCategoryEnum.PodcastCategoryMainImage)
            {
                return StatusCode(403, new
                {
                    error = $"Invalid file key: Must be a podcast category main image file",
                    actualCategory = category.ToString()
                });
            }
            var url = await _fileIOHelper.GeneratePresignedUrlAsync(FileKey);

            return Ok(new { FileUrl = url });
        }
    }
}
