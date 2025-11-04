using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodcastService.API.Filters.ExceptionFilters;
using PodcastService.BusinessLogic.Enums.App;
using PodcastService.BusinessLogic.Helpers.FileHelpers;
using PodcastService.BusinessLogic.Models.CrossService;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using PodcastService.DataAccess.Data;

namespace PodcastService.API.Controllers.MiscControllers
{
    [Route("api/misc/public-source")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class PublicSourceController : ControllerBase
    {
        private readonly ILogger<PublicSourceController> _logger;
        private readonly FileIOHelper _fileIOHelper;
        public PublicSourceController(ILogger<PublicSourceController> logger, FileIOHelper fileIOHelper)
        {
            _logger = logger;
            _fileIOHelper = fileIOHelper;
        }


        // /api/user-service/get-file-url/{**FileKey}
        [HttpGet("get-file-url/{**FileKey}")]
        public async Task<IActionResult> GetFileUrl(string FileKey)
        {
            // kiểm tra filkey có phải có pattern là "main_files/Bookings/<BookingId>/<BookingPodcastTrackId>_track_audio.<audio extension>" hoặc "main_files/PodcastEpisodes/<PodcastEpisodeId>/audio.<audio extension>" không, nếu có thì trả về exception 400
            // tức là sẽ có 2 loại fileKey không thể lấy url được từ url , các file còn lại thì được lấy binh thường
            // Console.WriteLine($"[DEBUG] Requested fileKey: {FileKey.StartsWith("main_files/Bookings/")}");
            // Console.WriteLine($"[DEBUG] Requested fileKey: {FileKey.Contains("_track_audio.")}");
            // Console.WriteLine($"[DEBUG] Requested fileKey: {FileKey.StartsWith("main_files/PodcastEpisodes/")}");
            // Console.WriteLine($"[DEBUG] Requested fileKey: {FileKey.Contains("/audio.")}");


            // Determine access level dựa trên user auth status
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var requiredLevel = isAuthenticated ? FileAccessLevelEnum.RequiresAuth : FileAccessLevelEnum.Public;

            var validation = FileAccessValidator.ValidateFileAccess(FileKey, requiredLevel);

            if (!validation.IsValid)
            {
                return StatusCode(403, new { error = validation.ErrorMessage });
            }

            // Additional ownership checks...

            var url = await _fileIOHelper.GeneratePresignedUrlAsync(FileKey);
            return Ok(new { fileUrl = url });
        }
    }
}
