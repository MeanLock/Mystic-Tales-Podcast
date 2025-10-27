using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.API.Filters.ExceptionFilters;
using UserService.BusinessLogic.Enums.App;
using UserService.BusinessLogic.Helpers.FileHelpers;
using UserService.BusinessLogic.Models.CrossService;
using UserService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using UserService.DataAccess.Data;

namespace UserService.API.Controllers.MiscControllers
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


        // /api/user-service/get-file-url/{fileKey}
        [HttpGet("get-file-url/{**fileKey}")]
        public async Task<IActionResult> GetFileUrl(string fileKey)
        {
            // kiểm tra filkey có phải có pattern là "main_files/Bookings/<BookingId>/<BookingPodcastTrackId>_track_audio.<audio extension>" hoặc "main_files/PodcastEpisodes/<PodcastEpisodeId>/audio.<audio extension>" không, nếu có thì trả về exception 400
            // tức là sẽ có 2 loại fileKey không thể lấy url được từ url , các file còn lại thì được lấy binh thường
            // Console.WriteLine($"[DEBUG] Requested fileKey: {fileKey.StartsWith("main_files/Bookings/")}");
            // Console.WriteLine($"[DEBUG] Requested fileKey: {fileKey.Contains("_track_audio.")}");
            // Console.WriteLine($"[DEBUG] Requested fileKey: {fileKey.StartsWith("main_files/PodcastEpisodes/")}");
            // Console.WriteLine($"[DEBUG] Requested fileKey: {fileKey.Contains("/audio.")}");


            // Determine access level dựa trên user auth status
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var requiredLevel = isAuthenticated ? FileAccessLevelEnum.RequiresAuth : FileAccessLevelEnum.Public;

            var validation = FileAccessValidator.ValidateFileAccess(fileKey, requiredLevel);

            if (!validation.IsValid)
            {
                return StatusCode(403, new { error = validation.ErrorMessage });
            }

            // Additional ownership checks...

            var url = await _fileIOHelper.GeneratePresignedUrlAsync(fileKey);
            return Ok(new { fileUrl = url });
        }
    }
}
