using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.Extensions.Logging;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.Common.AppConfigurations.Media.interfaces;
using PodcastService.Infrastructure.Configurations.AWS.interfaces;

namespace PodcastService.BusinessLogic.Helpers.FileHelpers
{
    public class LocalBase64FileHelper
    {
        // CONFIG
        private readonly IAWSS3Config _awsS3Config;
        private readonly IMediaTypeConfig _mediaTypeConfig;

        // LOGGER
        private readonly ILogger<LocalBase64FileHelper> _logger;

        // SERVICES
        private readonly LocalBinaryFileHelper _localBinaryFileHelper;
        

        public LocalBase64FileHelper(
            IAWSS3Config awsS3Config,
            IMediaTypeConfig mediaTypeConfig,
            LocalBinaryFileHelper localBinaryFileHelper, ILogger<LocalBase64FileHelper> logger)
        {
            _awsS3Config = awsS3Config;
            _mediaTypeConfig = mediaTypeConfig;
            _localBinaryFileHelper = localBinaryFileHelper;
            _logger = logger;
        }

        public async Task UploadBase64FileAsync(string base64Data, string folderPath, string fileName)
        {
            try
            {
                var match = Regex.Match(base64Data, "^data:(.+);base64,(.+)$");
                string mimeType = "application/octet-stream";
                string data = base64Data;
                if (match.Success)
                {
                    mimeType = match.Groups[1].Value;
                    data = match.Groups[2].Value;
                }
                Console.WriteLine("Uploading base64 file to " + mimeType);

                byte[] fileBytes = Convert.FromBase64String(data);
                await _localBinaryFileHelper.UploadBinaryFileAsync(fileBytes, folderPath, fileName, mimeType);
            }
            catch (FormatException ex)
            {
                _logger.LogError($"Invalid base64 string format: {ex}");
                throw new HttpRequestException("Invalid base64 string format. " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading file: {ex.Message}");
                throw new HttpRequestException("Error uploading file. " + ex.Message);
            }
        }

        public async Task<string?> GetFileBase64Async(string filePath)
        {
            try
            {
                // Get file bytes from binary service
                byte[]? fileBytes = await _localBinaryFileHelper.GetFileBytesAsync(filePath);
                if (fileBytes == null)
                {
                    _logger.LogWarning($"File not found: {filePath}");
                    return null;
                }

                // Convert to base64
                string base64String = Convert.ToBase64String(fileBytes);

                // Get file extension to determine MIME type using binary service method
                string? fullFileKey = await _localBinaryFileHelper.GetFullFileKeyAsync(filePath);
                if (!string.IsNullOrEmpty(fullFileKey))
                {
                    string extension = Path.GetExtension(fullFileKey);
                    string mimeType = _mediaTypeConfig.GetMimeTypeFromExtension(extension);
                    return $"data:{mimeType};base64,{base64String}";
                }

                return $"data:application/octet-stream;base64,{base64String}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting file base64: {ex.Message}");
                return null;
            }
        }

    }
}