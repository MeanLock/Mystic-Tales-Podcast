using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.API.Enums.Api;
using UserService.API.Filters.ExceptionFilters;
using UserService.BusinessLogic.DTOs.Cache;
using UserService.BusinessLogic.Enums.App;
using UserService.BusinessLogic.Helpers.FileHelpers;
using UserService.BusinessLogic.Models.CrossService;
using UserService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using UserService.Common.AppConfigurations.FilePath.interfaces;
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
        private readonly IFilePathConfig _filePathConfig;
        public PublicSourceController(ILogger<PublicSourceController> logger, FileIOHelper fileIOHelper, IFilePathConfig filePathConfig)
        {
            _logger = logger;
            _fileIOHelper = fileIOHelper;
            _filePathConfig = filePathConfig;
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
            return Ok(new { FileUrl = url });
        }

        // /api/user-service/api/misc/public-source/podcaster-documents/{PodcasterDocumentFileType}/get-file-url
        [HttpGet("podcaster-documents/{PodcasterDocumentFileType}/get-file-url")]
        [Authorize(Policy = "AdminOrStaffOrCustomer.BasicAccess")]
        public async Task<IActionResult> GetPodcasterDocumentTemplateFileUrl(PodcasterDocumentFileTypeEnum PodcasterDocumentFileType)
        {
            var account = HttpContext.Items["LoggedInAccount"] as AccountStatusCache;

            if (PodcasterDocumentFileType == PodcasterDocumentFileTypeEnum.MainBuddyCommitmentDocumentTemplate)
            {
                var fileKey = $"{_filePathConfig.SYSTEM_PODCASTER_DOCUMENTS_FILE_PATH}/main_buddy_commitment_document_template.pdf";
                var url = await _fileIOHelper.GeneratePresignedUrlAsync(fileKey, 120);
                return Ok(new { FileUrl = url });
            }
            return BadRequest("Invalid PodcasterDocumentFileType.");
        }
    }
}
