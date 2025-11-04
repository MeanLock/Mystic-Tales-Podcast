using Microsoft.AspNetCore.Mvc;
using PodcastService.API.Filters.ExceptionFilters;
using PodcastService.BusinessLogic.Helpers.FileHelpers;
using PodcastService.BusinessLogic.Models.CrossService;
using PodcastService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using PodcastService.Infrastructure.Services.Audio.AcoustID;
using PodcastService.Infrastructure.Models.Audio.AcoustID;
using PodcastService.Infrastructure.Services.Audio.Hls;
using System.Text;
using PodcastService.Common.AppConfigurations.Media;
using PodcastService.Common.AppConfigurations.Media.interfaces;
using PodcastService.Infrastructure.Models.Audio.Hls;

namespace PodcastService.API.Controllers.QueryControllers
{
    [Route("api/hls")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class HlsController : ControllerBase
    {

        private readonly FileIOHelper _fileIOHelper;
        private readonly FFMegLocalHlsService _ffMegLocalHlsService;
        private readonly FFMpegCoreHlsService _ffMpegCoreHlsService;
        private readonly IMediaTypeConfig _mediaTypeConfig;

        public HlsController(FileIOHelper fileIOHelper, FFMegLocalHlsService ffMegLocalHlsService, FFMpegCoreHlsService ffMpegCoreHlsService, IMediaTypeConfig mediaTypeConfig)
        {
            _mediaTypeConfig = mediaTypeConfig;
            _fileIOHelper = fileIOHelper;
            _ffMegLocalHlsService = ffMegLocalHlsService;
            _ffMpegCoreHlsService = ffMpegCoreHlsService;
        }

        [HttpPost("upload-audio")] // upload-audio return file key
        public async Task<IActionResult> UploadAudio(IFormFile file,
            [FromForm] string folderPath, [FromForm] string fileName)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file provided");

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var fileData = memoryStream.ToArray();

                await _fileIOHelper.UploadBinaryFileAsync(fileData, folderPath, fileName, file.ContentType);

                var stream = await _fileIOHelper.GetFileStreamAsync(FilePathHelper.CombinePaths(folderPath, fileName));
                if (stream == null)
                {
                    return BadRequest(new { error = "Failed to retrieve uploaded file stream" });
                }

                Console.WriteLine($"[DEBUG] Starting HLS processing using FFMegLocalHlsService...");
                HlsProcessingResult hlsResult = await _ffMegLocalHlsService.ProcessAudioToHlsAsync(stream);
                Console.WriteLine($"[DEBUG] HLS processing completed. Success: {hlsResult.Success}");
                if (hlsResult.Success == false)
                {
                    return BadRequest(new { error = hlsResult.ErrorMessage });
                }

                // xoá hết các file cũ trong thư mục playlist (nếu có)
                await _fileIOHelper.DeleteFolderAsync(FilePathHelper.CombinePaths(folderPath, "playlist"));

                foreach (var segment in hlsResult.GeneratedFiles)
                {
                    // var segmentData = await _fileIOHelper.GetFileBytesAsync(segment.FilePath);
                    // if (segmentData == null)
                    // {
                    //     return BadRequest(new { error = $"Failed to read segment file: {segment.FilePath}" });
                    // }
                    var segmentData = segment.FileContent;

                    await _fileIOHelper.UploadBinaryFileAsync(segmentData, FilePathHelper.CombinePaths(folderPath, "playlist"), segment.FileName);
                }

                string playlistUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(folderPath, "playlist", "playlist.m3u8"), 20);
                string playlistFileKey = await _fileIOHelper.GetFullFileKeyAsync(FilePathHelper.CombinePaths(folderPath, "playlist", "playlist.m3u8"));
                // string ePlaylistUrl = Convert.ToBase64String(Encoding.UTF8.GetBytes(playlistUrl));
                string ePlaylistFileKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(playlistFileKey));



                return Ok(new
                {
                    message = "Binary file uploaded successfully",
                    fileName,
                    size = fileData.Length,
                    contentType = file.ContentType,
                    fileKey = FilePathHelper.NormalizeFilePath(FilePathHelper.CombinePaths(folderPath, $"{fileName}{_mediaTypeConfig.GetExtensionFromMimeType(file.ContentType)}")),
                    playlistFileKey,
                    folderPath = FilePathHelper.CombinePaths(folderPath, "playlist"),
                    // playlistUrl,
                    ePlaylistFileKey
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

        }

        [HttpPost("upload-audio-ffmpegCore")] // upload-audio return file key
        public async Task<IActionResult> UploadAudioFFmpegCore(IFormFile file,
            [FromForm] string folderPath, [FromForm] string fileName)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file provided");

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var fileData = memoryStream.ToArray();

                await _fileIOHelper.UploadBinaryFileAsync(fileData, folderPath, fileName, file.ContentType);

                var stream = await _fileIOHelper.GetFileStreamAsync(FilePathHelper.CombinePaths(folderPath, fileName));
                if (stream == null)
                {
                    return BadRequest(new { error = "Failed to retrieve uploaded file stream" });
                }

                Console.WriteLine($"[DEBUG] Starting HLS processing using FFMegLocalHlsService...");
                HlsProcessingResult hlsResult = await _ffMpegCoreHlsService.ProcessAudioToHlsAsync(stream);
                Console.WriteLine($"[DEBUG] HLS processing completed. Success: {hlsResult.Success}");
                if (hlsResult.Success == false)
                {
                    return BadRequest(new { error = hlsResult.ErrorMessage });
                }

                // xoá hết các file cũ trong thư mục playlist (nếu có)
                await _fileIOHelper.DeleteFolderAsync(FilePathHelper.CombinePaths(folderPath, "playlist"));

                foreach (var segment in hlsResult.GeneratedFiles)
                {
                    // var segmentData = await _fileIOHelper.GetFileBytesAsync(segment.FilePath);
                    // if (segmentData == null)
                    // {
                    //     return BadRequest(new { error = $"Failed to read segment file: {segment.FilePath}" });
                    // }
                    var segmentData = segment.FileContent;

                    await _fileIOHelper.UploadBinaryFileAsync(segmentData, FilePathHelper.CombinePaths(folderPath, "playlist"), segment.FileName);
                }

                string playlistUrl = await _fileIOHelper.GeneratePresignedUrlAsync(FilePathHelper.CombinePaths(folderPath, "playlist", "playlist.m3u8"), 20);
                string playlistFileKey = await _fileIOHelper.GetFullFileKeyAsync(FilePathHelper.CombinePaths(folderPath, "playlist", "playlist.m3u8"));
                // string ePlaylistUrl = Convert.ToBase64String(Encoding.UTF8.GetBytes(playlistUrl));
                string ePlaylistFileKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(playlistFileKey));



                return Ok(new
                {
                    message = "Binary file uploaded successfully",
                    fileName,
                    size = fileData.Length,
                    contentType = file.ContentType,
                    fileKey = FilePathHelper.NormalizeFilePath(FilePathHelper.CombinePaths(folderPath, $"{fileName}{_mediaTypeConfig.GetExtensionFromMimeType(file.ContentType)}")),
                    playlistFileKey,
                    folderPath = FilePathHelper.CombinePaths(folderPath, "playlist"),
                    // playlistUrl,
                    ePlaylistFileKey
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

        }


        [HttpGet("playlist")]
        public async Task<IActionResult> GetPlaylist(
            // [FromForm] string folderPath,
            [FromQuery] string ePlaylistFileKey
        )
        {
            try
            {
                string folderPath = "test_hls/1/playlist";
                string playlistPath;
                try
                {
                    playlistPath = Encoding.UTF8.GetString(Convert.FromBase64String(ePlaylistFileKey));
                }
                catch
                {
                    return BadRequest("Invalid playlist token");
                }

                // Check if playlist file exists using FileIOHelper
                if (!await _fileIOHelper.FileExistsAsync(playlistPath))
                    return NotFound("Playlist not found");

                Console.WriteLine($"Playlist file pathhhhhhhhhhhh: {playlistPath}");
                // Get playlist content using FileIOHelper
                var playlistBytes = await _fileIOHelper.GetFileBytesAsync(playlistPath);
                if (playlistBytes == null)
                    return NotFound("Unable to read playlist");

                var playlistContent = Encoding.UTF8.GetString(playlistBytes);
                var lines = playlistContent.Split('\n');

                var rewritten = new List<string>(lines.Length);
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.Length == 0)
                    {
                        rewritten.Add(line);
                        continue;
                    }

                    // Giữ nguyên metadata (#EXT...)
                    if (trimmed.StartsWith("#"))
                    {
                        rewritten.Add(line);
                        continue;
                    }

                    if (trimmed.EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
                    {
                        var safe = System.IO.Path.GetFileName(trimmed); // tránh path traversal
                        // lấy folder chứa playlist
                        var playlistDir = System.IO.Path.GetDirectoryName(playlistPath);
                        safe = FilePathHelper.CombinePaths(playlistDir, safe);
                        Console.WriteLine($"Playlist dir: {playlistDir}");
                        Console.WriteLine($"Playlist dir: {trimmed}");

                        Console.WriteLine($"Segment safe name: {safe}");
                        safe = Convert.ToBase64String(Encoding.UTF8.GetBytes(safe));
                        rewritten.Add($"/api/hls/segment?s={Uri.EscapeDataString(safe)}");  
                        // rewritten.Add($"/api/hls/segment?s={Uri.EscapeDataString(safe)}");
                    }
                    else
                    {
                        // fallback: giữ nguyên
                        rewritten.Add(line);
                    }
                }

                Response.Headers.CacheControl = "no-store";
                return Content(string.Join("\n", rewritten), "application/vnd.apple.mpegurl");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("segment")]
        public async Task<IActionResult> GetSegment(
            // [FromQuery] string folderPath,
            [FromQuery] string s)
        {
            try
            {
                // string folderPath = "test_hls/1/playlist";

                if (string.IsNullOrWhiteSpace(s))
                    return BadRequest("Segment name is required");

                if (s.Contains(".."))
                    return BadRequest("Invalid segment name");

                // Construct the segment file path using the session structure
                // var segmentFilePath = FilePathHelper.CombinePaths("uploads", "sessions", "hls", s);

                // var segmentFilePath = FilePathHelper.CombinePaths(folderPath, s);
                var segmentFilePath = Encoding.UTF8.GetString(Convert.FromBase64String(s));

                // Check if segment file exists using FileIOHelper
                if (!await _fileIOHelper.FileExistsAsync(segmentFilePath))
                    return NotFound("Segment not found");

                Console.WriteLine($"Segment file path: {segmentFilePath}");
                // Get segment file bytes using FileIOHelper
                var segmentBytes = await _fileIOHelper.GetFileBytesAsync(segmentFilePath);
                if (segmentBytes == null)
                    return NotFound("Unable to read segment");

                Response.Headers.CacheControl = "no-store";
                return File(segmentBytes, "video/MP2T", s);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }



    }

}
