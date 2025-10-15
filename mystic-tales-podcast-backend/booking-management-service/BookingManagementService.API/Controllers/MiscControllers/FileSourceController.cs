using BookingManagementService.API.Filters.ExceptionFilters;
using BookingManagementService.BusinessLogic.Helpers.FileHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UserService.API.Controllers.MiscControllers
{
    [Route("api/misc/file-source")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class FileSourceController : ControllerBase
    {
        private readonly FileIOHelper _fileIOHelper;
        public FileSourceController(FileIOHelper fileIOHelper)
        {
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


            if (fileKey.StartsWith("main_files/Bookings/") && fileKey.Contains("_track_audio."))
            {
                return StatusCode(403, "Cannot get URL for booking track audio files.");
            }
            if (fileKey.StartsWith("main_files/PodcastEpisodes/") && fileKey.Contains("/audio."))
            {
                return StatusCode(403, "Cannot get URL for podcast episode audio files.");
            }
            var fileUrl = await _fileIOHelper.GeneratePresignedUrlAsync(fileKey);

            return Ok(new { FileUrl = fileUrl });
        }

        // /api/user-service/api/misc/file-source/get-audio-file-stream-url/{**fileKey}
        [HttpGet("get-audio-file-stream-url/{**fileKey}")]
        public async Task<IActionResult> GetAudioFileStreamUrl(string fileKey)
        {
            // chỉ cho phép lấy url của các file có pattern là "main_files/Bookings/<BookingId>/<BookingPodcastTrackId>_track_audio.<audio extension>" hoặc "main_files/PodcastEpisodes/<PodcastEpisodeId>/audio.<audio extension>"
            if (fileKey.StartsWith("main_files/Bookings/") && fileKey.Contains("_track_audio."))
            {
                var fileUrl = await _fileIOHelper.GeneratePresignedUrlAsync(fileKey);
                return Ok(new { FileUrl = fileUrl });
            }
            if (fileKey.StartsWith("main_files/PodcastEpisodes/") && fileKey.Contains("/audio."))
            {
                var fileUrl = await _fileIOHelper.GeneratePresignedUrlAsync(fileKey);
                return Ok(new { FileUrl = fileUrl });
            }
            return StatusCode(403, "Cannot get stream URL for non-audio files.");
        }
    }
}
