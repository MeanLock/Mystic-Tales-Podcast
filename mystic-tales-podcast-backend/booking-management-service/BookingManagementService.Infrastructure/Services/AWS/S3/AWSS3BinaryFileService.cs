using System.Drawing;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using BookingManagementService.Infrastructure.Configurations.AWS.interfaces;
using BookingManagementService.Common.AppConfigurations.Media.interfaces;

namespace BookingManagementService.Infrastructure.Services.AWS.S3
{
    public class AWSS3BinaryFileService
    {
        // LOGGER
        private readonly ILogger<AWSS3BinaryFileService> _logger;

        // CONFIG
        private readonly IAWSS3Config _awsS3Config;
        private readonly IMediaTypeConfig _mediaTypeConfig;

        // SERVICES
        private static string? _bucketName;
        private static AmazonS3Client? _s3Client;

        public AWSS3BinaryFileService(IAWSS3Config awsS3Config, IMediaTypeConfig mediaTypeConfig, ILogger<AWSS3BinaryFileService> logger)
        {
            _awsS3Config = awsS3Config;
            _mediaTypeConfig = mediaTypeConfig;
            _bucketName = awsS3Config.BucketName;
            AWSConfigsS3.UseSignatureVersion4 = true;

            _s3Client = new AmazonS3Client(awsS3Config.AccessKey, awsS3Config.SecretKey, RegionEndpoint.APSoutheast1);
            _logger = logger;
        }

        public byte[] ResizeBinaryImage(byte[] imageBytes, int maxWidth, int maxHeight)
        {
            // Validate input
            if (imageBytes == null || imageBytes.Length == 0)
            {
                _logger.LogWarning("Image bytes are null or empty, returning original");
                return imageBytes ?? new byte[0];
            }

            try
            {
                using (MemoryStream ms = new MemoryStream(imageBytes))
                using (Bitmap originalBitmap = new Bitmap(ms))
                {
                    // Check if resizing is needed
                    if (originalBitmap.Width <= maxWidth && originalBitmap.Height <= maxHeight)
                    {
                        // No need to resize, return original bytes
                        return imageBytes;
                    }

                    // For GIF files, preserve animation by not resizing
                    if (originalBitmap.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif))
                    {
                        _logger.LogInformation("GIF file detected, preserving animation by not resizing");
                        return imageBytes;
                    }

                    // Calculate new dimensions while maintaining aspect ratio
                    double ratioX = (double)maxWidth / originalBitmap.Width;
                    double ratioY = (double)maxHeight / originalBitmap.Height;
                    double ratio = Math.Min(ratioX, ratioY);

                    int newWidth = (int)(originalBitmap.Width * ratio);
                    int newHeight = (int)(originalBitmap.Height * ratio);

                    using (Bitmap resizedBitmap = new Bitmap(originalBitmap, newWidth, newHeight))
                    using (MemoryStream outputMs = new MemoryStream())
                    {
                        // Preserve original image format instead of forcing JPEG
                        System.Drawing.Imaging.ImageFormat format = originalBitmap.RawFormat;

                        // Handle special cases
                        if (format.Equals(System.Drawing.Imaging.ImageFormat.MemoryBmp))
                        {
                            // MemoryBmp is not a valid save format, use PNG instead
                            format = System.Drawing.Imaging.ImageFormat.Png;
                        }

                        resizedBitmap.Save(outputMs, format);
                        return outputMs.ToArray();
                    }
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"Invalid image format or corrupted image data: {ex.Message}");
                return imageBytes; // Return original if can't process
            }
            catch (OutOfMemoryException ex)
            {
                _logger.LogError($"Out of memory while processing image: {ex.Message}");
                return imageBytes; // Return original if can't process
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error resizing image: {ex.Message}");
                return imageBytes; // Return original if can't process
            }
        }
        /////////////////////////////////////////////////////////////////
        public async Task UploadFileStreamAsync(Stream fileStream, string key)
        {
            try
            {
                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(fileStream, _bucketName, key);
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogError($"S3 Error uploading file: {e.Message}");
                throw new HttpRequestException("Error uploading file: " + e.Message.ToString());
            }
            catch (Exception e)
            {
                _logger.LogError($"Unknown error uploading file: {e.Message}");
                throw new HttpRequestException("Error uploading file: " + e.Message.ToString());
            }
        }

        public async Task<string> GeneratePresignedUrlAsync(string filePath, int? expirationInSeconds = null)
        {
            EnsureS3ClientInitialized();

            // Normalize path
            filePath = filePath.Replace("\\", "/").TrimStart('/');

            try
            {
                // check if file has extension then only get presigned url
                // var fileName = Path.GetFileName(filePath);
                // var extension = !string.IsNullOrEmpty(Path.GetExtension(fileName));
                // if (!extension)
                // {
                //     filePath = await GetFullFileKeyAsync(filePath) ?? filePath;
                // }
                // Get the actual file key with extension
                string? fileKey = await GetFullFileKeyAsync(filePath);
                if (string.IsNullOrEmpty(fileKey))
                {
                    _logger.LogWarning($"File not found: {filePath}");
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                var request = new GetPreSignedUrlRequest()
                {
                    BucketName = _bucketName!,
                    Key = fileKey,
                    // Expires = DateTime.Now.AddHours(_awsS3Config.PresignedURLExpirationHours),
                    // Expires = DateTime.Now.AddSeconds(_awsS3Config.PresignedURLExpirationSeconds),
                    Expires = DateTime.Now.AddSeconds(expirationInSeconds ?? _awsS3Config.PresignedURLExpirationSeconds),
                };
                return _s3Client!.GetPreSignedURL(request);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError($"Error generating presigned URL: {ex.Message}");
                throw new HttpRequestException("Error generating presigned URL: " + ex.Message.ToString());
            }
        }

        public async Task UploadBinaryFileAsync(byte[] fileData, string folderPath, string fileName, string? mimeType = null)
        {
            EnsureS3ClientInitialized();

            try
            {
                // Auto-detect MIME type if not provided
                if (string.IsNullOrEmpty(mimeType))
                {
                    string extension = Path.GetExtension(fileName);
                    mimeType = _mediaTypeConfig.GetMimeTypeFromExtension(extension);
                }

                // Handle image resize logic
                if (IsImageFile(mimeType) && fileData.Length > 1000)
                {
                    try
                    {
                        fileData = ResizeBinaryImage(fileData, _awsS3Config.UploadImageMaxWidth, _awsS3Config.UploadImageMaxHeight);
                    }
                    catch (Exception resizeEx)
                    {
                        _logger.LogWarning($"Image resize failed, using original: {resizeEx.Message}");
                        // Continue with original fileData
                    }
                }

                // If fileName doesn't have extension, add it based on MIME type
                if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                {
                    string extension = _mediaTypeConfig.GetExtensionFromMimeType(mimeType) ?? ".bin";
                    fileName = $"{fileName}{extension}";
                }

                folderPath = folderPath.Replace("\\", "/").TrimStart('/');
                string path = (folderPath + "/" + fileName).Replace("\\", "/").TrimStart('/');

                // Delete old files with same name if exists
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                string? fileKey = null;
                if (!fileNameWithoutExt.Contains("."))
                {
                    fileKey = await GetFullFileKeyAsync(folderPath + "/" + fileNameWithoutExt);
                }
                
                while (!string.IsNullOrEmpty(fileKey))
                {
                    var deleteRequest = new DeleteObjectRequest
                    {
                        BucketName = _bucketName!,
                        Key = fileKey
                    };
                    await _s3Client!.DeleteObjectAsync(deleteRequest);
                    fileKey = await GetFullFileKeyAsync(folderPath + "/" + fileNameWithoutExt);
                }

                using (var stream = new MemoryStream(fileData))
                {
                    await UploadFileStreamAsync(stream, path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading binary file: {ex.Message}");
                throw new HttpRequestException("Error uploading binary file: " + ex.Message);
            }
        }
        public async Task UploadBinaryFileWithStreamAsync(Stream fileStream, string folderPath, string fileName, string? mimeType = null)
        {
            EnsureS3ClientInitialized();

            try
            {
                // Auto-detect MIME type if not provided
                if (string.IsNullOrEmpty(mimeType))
                {
                    string extension = Path.GetExtension(fileName);
                    mimeType = _mediaTypeConfig.GetMimeTypeFromExtension(extension);
                }

                // If fileName doesn't have extension, add it based on MIME type
                if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
                {
                    string extension = _mediaTypeConfig.GetExtensionFromMimeType(mimeType) ?? ".bin";
                    fileName = $"{fileName}{extension}";
                }

                folderPath = folderPath.Replace("\\", "/").TrimStart('/');
                string path = (folderPath + "/" + fileName).Replace("\\", "/").TrimStart('/');

                // Delete old files with same name if exists
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                string? fileKey = null;
                if (!fileNameWithoutExt.Contains("."))
                {
                    fileKey = await GetFullFileKeyAsync(folderPath + "/" + fileNameWithoutExt);
                }
                
                while (!string.IsNullOrEmpty(fileKey))
                {
                    var deleteRequest = new DeleteObjectRequest
                    {
                        BucketName = _bucketName!,
                        Key = fileKey
                    };
                    await _s3Client!.DeleteObjectAsync(deleteRequest);
                    fileKey = await GetFullFileKeyAsync(folderPath + "/" + fileNameWithoutExt);
                }

                // If it's an image stream, resize it
                if (IsImageFile(mimeType) && fileStream.Length > 1000)
                {
                    try
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await fileStream.CopyToAsync(memoryStream);
                            byte[] imageBytes = memoryStream.ToArray();
                            byte[] resizedBytes = ResizeBinaryImage(imageBytes, _awsS3Config.UploadImageMaxWidth, _awsS3Config.UploadImageMaxHeight);

                            using (var resizedStream = new MemoryStream(resizedBytes))
                            {
                                await UploadFileStreamAsync(resizedStream, path);
                            }
                        }
                    }
                    catch (Exception resizeEx)
                    {
                        _logger.LogWarning($"Image resize failed, uploading original: {resizeEx.Message}");
                        // Reset stream position and upload original
                        fileStream.Position = 0;
                        await UploadFileStreamAsync(fileStream, path);
                    }
                }
                else
                {
                    await UploadFileStreamAsync(fileStream, path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading binary file with stream: {ex.Message}");
                throw new HttpRequestException("Error uploading binary file with stream: " + ex.Message);
            }
        }
        public async Task CopyFileToFileAsync(string sourceFilePath, string destinationFilePath)
        {
            EnsureS3ClientInitialized();

            // Normalize paths
            sourceFilePath = sourceFilePath.Replace("\\", "/").TrimStart('/');
            destinationFilePath = destinationFilePath.Replace("\\", "/").TrimStart('/');

            try
            {
                // Get full source file key (with extension)
                string? sourceFileKey = await GetFullFileKeyAsync(sourceFilePath);
                if (string.IsNullOrEmpty(sourceFileKey))
                {
                    _logger.LogWarning($"Source file not found: {sourceFilePath}");
                    throw new FileNotFoundException($"Source file not found: {sourceFilePath}");
                }

                // Use source file extension for destination
                string sourceExtension = sourceFileKey.Substring(sourceFileKey.LastIndexOf('.'));

                // Remove extension from destination path if present  
                int destDotIndex = destinationFilePath.LastIndexOf('.');
                string destinationFilePathWithoutExt = destDotIndex > 0 ? destinationFilePath.Substring(0, destDotIndex) : destinationFilePath;

                string destinationFileKey = (destinationFilePathWithoutExt + sourceExtension).Replace("\\", "/").TrimStart('/');

                var copyRequest = new CopyObjectRequest
                {
                    SourceBucket = _bucketName!,
                    SourceKey = sourceFileKey,
                    DestinationBucket = _bucketName!,
                    DestinationKey = destinationFileKey
                };

                await _s3Client!.CopyObjectAsync(copyRequest);
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogError($"S3 Error copying file: {e.Message}");
                throw new HttpRequestException("Error copying file: " + e.Message.ToString());
            }
            catch (Exception e)
            {
                _logger.LogError($"Unknown error copying file: {e.Message}");
                throw new HttpRequestException("Error copying file: " + e.Message.ToString());
            }
        }


        public async Task DeleteFileAsync(string filePath)
        {
            EnsureS3ClientInitialized();

            try
            {
                string? fileKey = await GetFullFileKeyAsync(filePath);
                if (!string.IsNullOrEmpty(fileKey))
                {
                    var deleteRequest = new DeleteObjectRequest
                    {
                        BucketName = _bucketName!,
                        Key = fileKey
                    };
                    await _s3Client!.DeleteObjectAsync(deleteRequest);
                }
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError($"Error deleting file: {ex.Message}");
                throw new HttpRequestException("Error deleting file: " + ex.Message.ToString());
            }
        }

        public async Task DeleteFolderAsync(string folderPath)
        {
            EnsureS3ClientInitialized();

            folderPath = folderPath.Replace("\\", "/").TrimStart('/');

            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = _bucketName!,
                    Prefix = folderPath + "/"
                };

                var response = await _s3Client!.ListObjectsV2Async(request);
                foreach (var obj in response.S3Objects)
                {
                    var deleteRequest = new DeleteObjectRequest
                    {
                        BucketName = _bucketName!,
                        Key = obj.Key
                    };
                    await _s3Client!.DeleteObjectAsync(deleteRequest);
                }
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError($"Error deleting folder: {ex.Message}");
                throw new HttpRequestException("Error deleting folder: " + ex.Message.ToString());
            }
        }

        public async Task<byte[]?> GetFileBytesAsync(string filePath)
        {
            EnsureS3ClientInitialized();

            try
            {
                // Get the actual file key with extension
                string? fileKey = await GetFullFileKeyAsync(filePath);
                if (string.IsNullOrEmpty(fileKey))
                {
                    _logger.LogWarning($"File not found: {filePath}");
                    return null;
                }

                var request = new GetObjectRequest
                {
                    BucketName = _bucketName!,
                    Key = fileKey
                };

                using (var response = await _s3Client!.GetObjectAsync(request))
                using (var ms = new MemoryStream())
                {
                    await response.ResponseStream.CopyToAsync(ms);
                    return ms.ToArray();
                }
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogError($"Error getting file bytes from S3: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                _logger.LogError($"Unknown error getting file bytes: {e.Message}");
                return null;
            }
        }

        public async Task<Stream?> GetFileStreamAsync(string filePath)
        {
            EnsureS3ClientInitialized();

            try
            {
                // Get the actual file key with extension
                string? fileKey = await GetFullFileKeyAsync(filePath);
                if (string.IsNullOrEmpty(fileKey))
                {
                    _logger.LogWarning($"File not found: {filePath}");
                    return null;
                }

                var request = new GetObjectRequest
                {
                    BucketName = _bucketName!,
                    Key = fileKey
                };

                var response = await _s3Client!.GetObjectAsync(request);
                return response.ResponseStream;
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogError($"Error getting file stream from S3: {e.Message}");
                return null;
            }
            catch (Exception e)
            {
                _logger.LogError($"Unknown error getting file stream: {e.Message}");
                return null;
            }
        }

        public async Task<string?> GetFullFileKeyAsync(string filePath)
        {
            EnsureS3ClientInitialized();

            // Normalize path
            filePath = filePath.Replace("\\", "/").TrimStart('/');

            // Check if filePath already has extension
            string extension = Path.GetExtension(filePath);
            if (!string.IsNullOrEmpty(extension))
            {
                // File path already has extension, return as is
                return filePath;
            }

            // Extract folder and filename from path
            int lastSlash = filePath.LastIndexOf('/');
            string folderPath = lastSlash > 0 ? filePath.Substring(0, lastSlash) : "";
            string fileName = lastSlash > 0 ? filePath.Substring(lastSlash + 1) : filePath;

            folderPath = folderPath.Replace("\\", "/").TrimStart('/');
            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = _bucketName!,
                    Prefix = folderPath + "/"
                };

                var response = await _s3Client!.ListObjectsV2Async(request);
                foreach (var obj in response.S3Objects)
                {
                    string fileWithoutExt = Path.GetFileNameWithoutExtension(obj.Key);
                    if (fileWithoutExt == fileName)
                    {
                        return obj.Key;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting full file key for {filePath}: {ex.Message}");
            }
            return null;
        }




        // Helper method
        private static void EnsureS3ClientInitialized()
        {
            if (_s3Client == null || string.IsNullOrEmpty(_bucketName))
                throw new InvalidOperationException("S3 client not initialized");
        }

        private bool IsImageFile(string mimeType)
        {
            return mimeType != null && mimeType.StartsWith("image/");
        }

        public async Task<bool> FileExistsAsync(string filePath)
        {
            string? fileKey = await GetFullFileKeyAsync(filePath);
            return !string.IsNullOrEmpty(fileKey);
        }

        public string? GetExtensionFromMimeType(string mimeType)
        {
            return _mediaTypeConfig.GetExtensionFromMimeType(mimeType);
        }

        public string GetMimeTypeFromExtension(string extension)
        {
            return _mediaTypeConfig.GetMimeTypeFromExtension(extension);
        }
    }

}
