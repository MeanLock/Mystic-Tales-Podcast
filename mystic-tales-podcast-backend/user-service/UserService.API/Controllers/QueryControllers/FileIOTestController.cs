using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.API.Filters.ExceptionFilters;
using UserService.BusinessLogic.DTOs.Auth;
using UserService.BusinessLogic.DTOs.Feedback;
using UserService.BusinessLogic.Services.DbServices.MiscServices;
using UserService.BusinessLogic.Services.DbServices.UserServices;
using UserService.DataAccess.Entities;
using UserService.BusinessLogic.Helpers.FileHelpers;
using System.Text;
using UserService.Common.AppConfigurations.Media.interfaces;

namespace UserService.API.Controllers.MiscControllers
{
    [Route("api/Misc/file-io-test")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    public class FileIOTestController : ControllerBase
    {
        private ILogger<FileIOTestController> _logger;
        private readonly FileIOHelper _fileIOHelper;
        private readonly IMediaTypeConfig _mediaTypeConfig;

        public FileIOTestController(
            ILogger<FileIOTestController> logger,
            FileIOHelper fileIOHelper,
            IMediaTypeConfig mediaTypeConfig)
        {
            _logger = logger;
            _fileIOHelper = fileIOHelper;
            _mediaTypeConfig = mediaTypeConfig;
        }

        #region Upload Methods

        /// <summary>
        /// Test UploadBase64FileAsync method - Upload file from base64 string
        /// </summary>
        [HttpPost("upload-base64")]
        public async Task<IActionResult> UploadBase64File([FromBody] UploadBase64Request request)
        {
            try
            {
                await _fileIOHelper.UploadBase64FileAsync(request.Base64Data, request.FolderPath, request.FileName);
                return Ok(new { message = "Base64 file uploaded successfully", folderPath = request.FolderPath, fileName = request.FileName });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test UploadBinaryFileAsync method - Upload binary file
        /// </summary>
        [HttpPost("upload-binary")]
        public async Task<IActionResult> UploadBinaryFile(IFormFile file, [FromForm] string folderPath, [FromForm] string fileName)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file provided");

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var fileData = memoryStream.ToArray();

                await _fileIOHelper.UploadBinaryFileAsync(fileData, folderPath, fileName, file.ContentType);
                return Ok(new
                {
                    message = "Binary file uploaded successfully",
                    folderPath,
                    fileName,
                    size = fileData.Length,
                    contentType = file.ContentType,
                    fileKey = FilePathHelper.NormalizeFilePath(FilePathHelper.CombinePaths(folderPath, $"{fileName}{_mediaTypeConfig.GetExtensionFromMimeType(file.ContentType)}"))
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test UploadBinaryFileWithStreamAsync method - Upload binary file using stream
        /// </summary>
        [HttpPost("upload-stream")]
        public async Task<IActionResult> UploadBinaryFileWithStream(IFormFile file, [FromForm] string folderPath, [FromForm] string fileName)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file provided");

                using var stream = file.OpenReadStream();
                await _fileIOHelper.UploadBinaryFileWithStreamAsync(stream, folderPath, fileName, file.ContentType);
                return Ok(new
                {
                    message = "Stream file uploaded successfully",
                    folderPath,
                    fileName,
                    size = file.Length,
                    contentType = file.ContentType,
                    fileKey = FilePathHelper.NormalizeFilePath(FilePathHelper.CombinePaths(folderPath, $"{fileName}{_mediaTypeConfig.GetExtensionFromMimeType(file.ContentType)}"))
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region Download/Read Methods

        /// <summary>
        /// Test GeneratePresignedUrlAsync method - Generate presigned URL for file access
        /// </summary>
        [HttpGet("generate-url/{**filePath}")]
        public async Task<IActionResult> GeneratePresignedUrl(string filePath)
        {
            try
            {
                var url = await _fileIOHelper.GeneratePresignedUrlAsync(filePath);
                return Ok(new { message = "Presigned URL generated successfully", filePath, url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test GetFileBytesAsync method - Get file as byte array
        /// </summary>
        [HttpGet("download-bytes/{**filePath}")]
        public async Task<IActionResult> GetFileBytes(string filePath)
        {
            try
            {
                var fileBytes = await _fileIOHelper.GetFileBytesAsync(filePath);
                if (fileBytes == null)
                    return NotFound(new { message = "File not found", filePath });

                return Ok(new { message = "File bytes retrieved successfully", filePath, size = fileBytes.Length, bytes = Convert.ToBase64String(fileBytes) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test ConvertFileToBase64Async method - Convert file to base64 with MIME type
        /// </summary>
        [HttpGet("convert-to-base64/{**filePath}")]
        public async Task<IActionResult> ConvertFileToBase64(string filePath)
        {
            try
            {
                var base64String = await _fileIOHelper.ConvertFileToBase64Async(filePath);
                if (string.IsNullOrEmpty(base64String))
                    return NotFound(new { message = "File not found or empty", filePath });

                return Ok(new { message = "File converted to base64 successfully", filePath, base64String, length = base64String.Length });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region File Management Methods

        /// <summary>
        /// Test CopyFileToFileAsync method - Copy file from source to destination
        /// </summary>
        [HttpPost("copy-file")]
        public async Task<IActionResult> CopyFile([FromBody] CopyFileRequest request)
        {
            try
            {
                await _fileIOHelper.CopyFileToFileAsync(request.SourceFilePath, request.DestinationFilePath);
                return Ok(new { message = "File copied successfully", sourceFilePath = request.SourceFilePath, destinationFilePath = request.DestinationFilePath });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test DeleteFileAsync method - Delete file
        /// </summary>
        [HttpDelete("delete-file/{**filePath}")]
        public async Task<IActionResult> DeleteFile(string filePath)
        {
            try
            {
                await _fileIOHelper.DeleteFileAsync(filePath);
                return Ok(new { message = "File deleted successfully", filePath });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test DeleteFolderAsync method - Delete folder and all contents
        /// </summary>
        [HttpDelete("delete-folder/{**folderPath}")]
        public async Task<IActionResult> DeleteFolder(string folderPath)
        {
            try
            {
                await _fileIOHelper.DeleteFolderAsync(folderPath);
                return Ok(new { message = "Folder deleted successfully", folderPath });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Test FileExistsAsync method - Check if file exists
        /// </summary>
        [HttpGet("file-exists/{**filePath}")]
        public async Task<IActionResult> FileExists(string filePath)
        {
            try
            {
                var exists = await _fileIOHelper.FileExistsAsync(filePath);
                return Ok(new { message = "File existence checked", filePath, exists });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test GetFullFileKeyAsync method - Get full file key with extension
        /// </summary>
        [HttpGet("full-file-key/{**filePath}")]
        public async Task<IActionResult> GetFullFileKey(string filePath)
        {
            try
            {
                var fullKey = await _fileIOHelper.GetFullFileKeyAsync(filePath);
                return Ok(new { message = "Full file key retrieved", filePath, fullKey });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        #endregion

        #region Test Helpers

        /// <summary>
        /// Generate sample base64 file for testing
        /// </summary>
        [HttpGet("generate-sample-base64")]
        public IActionResult GenerateSampleBase64([FromQuery] string fileType = "txt")
        {
            try
            {
                string base64Data;
                string mimeType;

                switch (fileType.ToLower())
                {
                    case "txt":
                        var textContent = "This is a sample text file for testing FileIOHelper methods.";
                        base64Data = $"data:text/plain;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(textContent))}";
                        mimeType = "text/plain";
                        break;
                    case "json":
                        var jsonContent = "{ \"message\": \"This is a sample JSON file for testing\", \"timestamp\": \"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\" }";
                        base64Data = $"data:application/json;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonContent))}";
                        mimeType = "application/json";
                        break;
                    case "html":
                        var htmlContent = "<html><head><title>Test File</title></head><body><h1>Sample HTML File</h1><p>This is for testing FileIOHelper methods.</p></body></html>";
                        base64Data = $"data:text/html;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(htmlContent))}";
                        mimeType = "text/html";
                        break;
                    default:
                        return BadRequest("Unsupported file type. Use: txt, json, html");
                }

                return Ok(new { message = "Sample base64 generated", fileType, mimeType, base64Data, size = base64Data.Length });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get list of all available test endpoints
        /// </summary>
        [HttpGet("endpoints")]
        public IActionResult GetTestEndpoints()
        {
            var endpoints = new
            {
                upload_methods = new
                {
                    upload_base64 = "POST /api/Misc/file-io-test/upload-base64",
                    upload_binary = "POST /api/Misc/file-io-test/upload-binary",
                    upload_stream = "POST /api/Misc/file-io-test/upload-stream"
                },
                download_methods = new
                {
                    generate_url = "GET /api/Misc/file-io-test/generate-url/{filePath}",
                    download_bytes = "GET /api/Misc/file-io-test/download-bytes/{filePath}",
                    convert_to_base64 = "GET /api/Misc/file-io-test/convert-to-base64/{filePath}"
                },
                file_management = new
                {
                    copy_file = "POST /api/Misc/file-io-test/copy-file",
                    delete_file = "DELETE /api/Misc/file-io-test/delete-file/{filePath}",
                    delete_folder = "DELETE /api/Misc/file-io-test/delete-folder/{folderPath}"
                },
                utilities = new
                {
                    file_exists = "GET /api/Misc/file-io-test/file-exists/{filePath}",
                    full_file_key = "GET /api/Misc/file-io-test/full-file-key/{filePath}"
                },
                test_helpers = new
                {
                    generate_sample_base64 = "GET /api/Misc/file-io-test/generate-sample-base64?fileType={txt|json|html}",
                    endpoints = "GET /api/Misc/file-io-test/endpoints"
                }
            };

            return Ok(new { message = "Available FileIOHelper test endpoints", endpoints });
        }

        #endregion

        #region Download/Preview Methods
        // [HttpGet("download/{**filePath}")]
        // public async Task<IActionResult> DownloadFile(string filePath)
        // {
        //     try
        //     {
        //         var fileBytes = await _fileIOHelper.GetFileBytesAsync(filePath);
        //         if (fileBytes == null)
        //             return NotFound(new { message = "File not found", filePath });

        //         if (FilePathHelper.HasExtension(filePath) == false)
        //         {
        //             filePath = _fileIOHelper.GetFullFileKeyAsync(filePath).Result ?? filePath;
        //         }
        //         var contentType = _mediaTypeConfig.GetMimeTypeFromExtension(FilePathHelper.GetExtension(filePath));
        //         Console.WriteLine("Content Type: " + contentType);

        //         return File(fileBytes, contentType, System.IO.Path.GetFileName(filePath));
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(new { error = ex.Message });
        //     }
        // }
        [HttpGet("download/{**filePath}")]
        public async Task<IActionResult> DownloadFile(string filePath)
        {
            try
            {
                var fileBytes = await _fileIOHelper.GetFileBytesAsync(filePath);
                if (fileBytes == null)
                    return NotFound(new { message = "File not found", filePath });

                if (FilePathHelper.HasExtension(filePath) == false)
                {
                    filePath = _fileIOHelper.GetFullFileKeyAsync(filePath).Result ?? filePath;
                }
                var contentType = _mediaTypeConfig.GetMimeTypeFromExtension(FilePathHelper.GetExtension(filePath));
                Console.WriteLine("Content Type: " + contentType);

                return File(fileBytes, contentType, System.IO.Path.GetFileName(filePath));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("preview/{**filePath}")]
        [HttpHead("preview/{**filePath}")]
        [HttpOptions("preview/{**filePath}")]
        public async Task<IActionResult> PreviewFile(string filePath)
        {
            // 🚨 CRITICAL: Add CORS headers IMMEDIATELY at the start
            AddCorsHeaders();

            // Add debug logging
            _logger.LogInformation("=== PREVIEW FILE REQUEST ===");
            _logger.LogInformation("Method: {Method}", Request.Method);
            _logger.LogInformation("FilePath: {FilePath}", filePath);
            _logger.LogInformation("Origin: {Origin}", Request.Headers["Origin"].FirstOrDefault());
            _logger.LogInformation("User-Agent: {UserAgent}", Request.Headers["User-Agent"].FirstOrDefault());

            try
            {
                // Handle OPTIONS preflight FIRST
                if (Request.Method == "OPTIONS")
                {
                    _logger.LogInformation("Handling CORS preflight request");
                    Response.StatusCode = 200;
                    return new EmptyResult(); // Don't use Ok() for preflight
                }

                // Check if file exists using FileIOHelper
                var fileExists = await _fileIOHelper.FileExistsAsync(filePath);
                if (!fileExists)
                {
                    _logger.LogWarning("File not found: {FilePath}", filePath);
                    return NotFound("File not found");
                }

                // Get full file key
                var fullFileKey = await _fileIOHelper.GetFullFileKeyAsync(filePath);
                if (string.IsNullOrEmpty(fullFileKey))
                {
                    _logger.LogWarning("File key not found: {FilePath}", filePath);
                    return NotFound("File key not found");
                }

                _logger.LogInformation("Full File Key: {FullFileKey}", fullFileKey);

                // Get file extension and MIME type
                var extension = FilePathHelper.GetExtension(fullFileKey);
                var mimeType = _mediaTypeConfig.GetMimeTypeFromExtension(extension);

                _logger.LogInformation("Extension: {Extension}, MimeType: {MimeType}", extension, mimeType);

                // Check if file type supports preview
                if (!IsPreviewableFile(extension))
                {
                    _logger.LogWarning("File type not previewable: {Extension}", extension);
                    return StatusCode(415, "File type not supported for preview");
                }

                // Get file bytes
                var fileBytes = await _fileIOHelper.GetFileBytesAsync(filePath);
                if (fileBytes == null || fileBytes.Length == 0)
                {
                    _logger.LogWarning("File bytes not found or empty: {FilePath}", filePath);
                    return NotFound("File content not found");
                }

                _logger.LogInformation("File size: {FileSize} bytes", fileBytes.Length);

                // Handle HEAD requests
                if (Request.Method == "HEAD")
                {
                    Response.Headers.Add("Content-Length", fileBytes.Length.ToString());
                    Response.Headers.Add("Content-Type", mimeType);
                    return new EmptyResult();
                }

                // 🚨 CRITICAL: Ensure CORS headers are set before returning file
                AddCorsHeaders();

                // Set content headers
                Response.Headers.Add("Content-Type", mimeType);
                Response.Headers.Add("Content-Length", fileBytes.Length.ToString());
                Response.Headers.Add("Content-Disposition", "inline");

                // Add cache headers
                if (IsImageFile(extension))
                {
                    Response.Headers.Add("Cache-Control", "public, max-age=2592000"); // 30 days
                }
                else if (IsAudioFile(extension) || extension.ToLowerInvariant() == ".pdf")
                {
                    Response.Headers.Add("Cache-Control", "public, max-age=86400"); // 1 day
                    Response.Headers.Add("Accept-Ranges", "bytes");
                }

                // Log successful access
                _logger.LogInformation("Successfully serving file: {FilePath} ({FileSize} bytes)", filePath, fileBytes.Length);

                return File(fileBytes, mimeType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serving preview file: {FilePath}", filePath);
                return StatusCode(500, "Internal server error");
            }
        }

        private void AddCorsHeaders()
        {
            Response.Headers.Remove("Access-Control-Allow-Origin");
            Response.Headers.Remove("Access-Control-Allow-Methods");
            Response.Headers.Remove("Access-Control-Allow-Headers");
            Response.Headers.Remove("Access-Control-Expose-Headers");
            Response.Headers.Remove("Access-Control-Max-Age");

            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, HEAD, OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept, Authorization, Range");
            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Length, Content-Range, Accept-Ranges, Content-Type, Content-Disposition");
            Response.Headers.Add("Access-Control-Max-Age", "86400");

            // Add debug logging
            _logger.LogDebug("CORS headers added: Origin=*, Methods=GET,POST,PUT,DELETE,HEAD,OPTIONS");
        }


        /// <summary>
        /// Check if file type supports preview in browser
        /// </summary>
        private bool IsPreviewableFile(string extension)
        {
            var previewableExtensions = new[]
            {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg", // Images
            ".pdf", // PDF
            ".txt", ".html", ".htm", ".css", ".js", ".json", ".xml", ".md", ".csv", // Text/Code
            ".mp4", ".webm", ".ogv", // Video
            ".mp3", ".wav", ".ogg", ".m4a", ".aac", ".flac" // Audio
        };

            return previewableExtensions.Contains(extension.ToLowerInvariant());
            // return true; // For testing, allow all file types
        }

        private bool IsImageFile(string extension)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };
            return imageExtensions.Contains(extension.ToLowerInvariant());
        }

        private bool IsAudioFile(string extension)
        {
            var audioExtensions = new[] { ".mp3", ".wav", ".ogg", ".m4a", ".aac", ".flac" };
            return audioExtensions.Contains(extension.ToLowerInvariant());
        }

        private bool IsStreamableFile(string extension)
        {
            return IsAudioFile(extension) ||
                   new[] { ".mp4", ".webm", ".ogv", ".pdf" }.Contains(extension.ToLowerInvariant());
        }

        private void LogFileAccess(string filePath, string fullFileKey, string extension, string action)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";

            _logger.LogInformation("File {Action}: {FilePath} -> {FullFileKey} ({Extension}) from {IPAddress}",
                action, filePath, fullFileKey, extension.ToUpper(), ipAddress);
        }


        #endregion

    }

    #region Request Models

    public class UploadBase64Request
    {
        public string Base64Data { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }

    public class CopyFileRequest
    {
        public string SourceFilePath { get; set; } = string.Empty;
        public string DestinationFilePath { get; set; } = string.Empty;
    }

    #endregion
}
