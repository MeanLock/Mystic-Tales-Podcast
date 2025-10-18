using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SystemConfigurationService.Infrastructure.Configurations.AWS.interfaces;
using SystemConfigurationService.Common.AppConfigurations.Media.interfaces;

namespace SystemConfigurationService.Infrastructure.Services.AWS.S3
{
    public class AWSS3Base64FileService
    {
        // LOGGER
        private readonly ILogger<AWSS3Base64FileService> _logger;

        // CONFIG
        private readonly IAWSS3Config _awsS3Config;
        private readonly IMediaTypeConfig _mediaTypeConfig;

        // SERVICES
        private readonly AWSS3BinaryFileService _awsS3BinaryFileService;

        public AWSS3Base64FileService(IAWSS3Config awsS3Config, IMediaTypeConfig mediaTypeConfig, ILogger<AWSS3Base64FileService> logger, AWSS3BinaryFileService awsS3BinaryFileService)
        {
            _awsS3Config = awsS3Config;
            _mediaTypeConfig = mediaTypeConfig;
            _logger = logger;
            _awsS3BinaryFileService = awsS3BinaryFileService;
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

                byte[] fileBytes = Convert.FromBase64String(data);
                await _awsS3BinaryFileService.UploadBinaryFileAsync(fileBytes, folderPath, fileName, mimeType);
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
                byte[]? fileBytes = await _awsS3BinaryFileService.GetFileBytesAsync(filePath);
                if (fileBytes == null)
                {
                    _logger.LogWarning($"File not found: {filePath}");
                    return null;
                }

                // Convert to base64
                string base64String = Convert.ToBase64String(fileBytes);

                // Get file extension to determine MIME type
                string? fileKey = await _awsS3BinaryFileService.GetFullFileKeyAsync(filePath);
                if (!string.IsNullOrEmpty(fileKey))
                {
                    string extension = Path.GetExtension(fileKey);
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
