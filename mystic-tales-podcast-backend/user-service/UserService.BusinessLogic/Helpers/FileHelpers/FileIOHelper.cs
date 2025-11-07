using UserService.Common.AppConfigurations.App.interfaces;
using UserService.Common.AppConfigurations.Media.interfaces;
using UserService.Infrastructure.Services.AWS.S3;

namespace UserService.BusinessLogic.Helpers.FileHelpers
{
    public class FileIOHelper
    {
        // CONFIG
        private readonly IAppConfig _appConfig;
        private readonly IMediaTypeConfig _mediaTypeConfig;

        // HELPER
        private readonly LocalBinaryFileHelper _localBinaryFileHelper;
        private readonly LocalBase64FileHelper _localBase64FileHelper;

        // SERVICE
        private readonly AWSS3Base64FileService _awsS3Base64FileService;
        private readonly AWSS3BinaryFileService _awsS3BinaryFileService;

        public FileIOHelper(
            IAppConfig appConfig,
            IMediaTypeConfig mediaTypeConfig,
            LocalBinaryFileHelper localBinaryFileHelper,
            LocalBase64FileHelper localBase64FileHelper,
            AWSS3Base64FileService awsS3Base64FileService,
            AWSS3BinaryFileService awsS3BinaryFileService)
        {
            _appConfig = appConfig;
            _mediaTypeConfig = mediaTypeConfig;
            _localBinaryFileHelper = localBinaryFileHelper;
            _localBase64FileHelper = localBase64FileHelper;
            _awsS3Base64FileService = awsS3Base64FileService;
            _awsS3BinaryFileService = awsS3BinaryFileService;
        }


        public async Task<string> GeneratePresignedUrlAsync(string filePath, int? expirationInSeconds = null)
        {
            if (_appConfig.FILE_STORAGE_SRC == "awsS3")
            {
                // Normalize the file path for S3
                var normalizedFilePath = FilePathHelper.NormalizeFilePath(filePath);

                return await _awsS3BinaryFileService.GeneratePresignedUrlAsync(normalizedFilePath, expirationInSeconds);
            }

            // For local storage, normalize and extract folder and filename
            var normalizedLocalPath = FilePathHelper.NormalizeFilePath(filePath);
            return await _localBinaryFileHelper.GenerateFileUrl(normalizedLocalPath);
        }

        public async Task UploadBinaryFileAsync(byte[] fileData, string folderPath, string fileName, string? mimeType = null)
        {
            if (_appConfig.FILE_STORAGE_SRC == "awsS3")
            {
                var normalizedFolderPath = FilePathHelper.NormalizeFolderPath(folderPath);
                await _awsS3BinaryFileService.UploadBinaryFileAsync(fileData, normalizedFolderPath, fileName, mimeType);
                return;
            }

            // For local storage, normalize folder path
            var normalizedLocalFolderPath = FilePathHelper.NormalizeFolderPath(folderPath);
            await _localBinaryFileHelper.UploadBinaryFileAsync(fileData, normalizedLocalFolderPath, fileName, mimeType);
        }


        public async Task UploadBinaryFileWithStreamAsync(Stream fileStream, string folderPath, string fileName, string? mimeType = null)
        {
            if (_appConfig.FILE_STORAGE_SRC == "awsS3")
            {
                var normalizedFolderPath = FilePathHelper.NormalizeFolderPath(folderPath);
                await _awsS3BinaryFileService.UploadBinaryFileWithStreamAsync(fileStream, normalizedFolderPath, fileName, mimeType);
                return;
            }

            // For local storage, normalize folder path
            var normalizedLocalFolderPath = FilePathHelper.NormalizeFolderPath(folderPath);
            await _localBinaryFileHelper.UploadBinaryFileWithStreamAsync(fileStream, normalizedLocalFolderPath, fileName, mimeType);
        }

        public async Task UploadBase64FileAsync(string base64Data, string folderPath, string fileName)
        {
            if (_appConfig.FILE_STORAGE_SRC == "awsS3")
            {
                var normalizedFolderPath = FilePathHelper.NormalizeFolderPath(folderPath);
                await _awsS3Base64FileService.UploadBase64FileAsync(base64Data, normalizedFolderPath, fileName);
                return;
            }

            // For local storage, normalize folder path
            var normalizedLocalFolderPath = FilePathHelper.NormalizeFolderPath(folderPath);
            await _localBase64FileHelper.UploadBase64FileAsync(base64Data, normalizedLocalFolderPath, fileName);
        }

        public async Task CopyFileToFileAsync(string sourceFilePath, string destinationFilePath)
        {
            if (_appConfig.FILE_STORAGE_SRC == "awsS3")
            {
                var normalizedSourcePath = FilePathHelper.NormalizeFilePath(sourceFilePath);
                var normalizedDestPath = FilePathHelper.NormalizeFilePath(destinationFilePath);

                await _awsS3BinaryFileService.CopyFileToFileAsync(normalizedSourcePath, normalizedDestPath);
                return;
            }

            // For local storage
            var sourceLocalPath = FilePathHelper.NormalizeFilePath(sourceFilePath);
            var destLocalPath = FilePathHelper.NormalizeFilePath(destinationFilePath);
            Console.WriteLine($"Copying file from {sourceLocalPath} to {destLocalPath}");
            await _localBinaryFileHelper.CopyFileToFileAsync(sourceLocalPath, destLocalPath);
        }

        public async Task CopyFolderToFolderAsync(string sourceFolderPath, string destinationFolderPath)
        {
            if (_appConfig.FILE_STORAGE_SRC == "awsS3")
            {
                var normalizedSourcePath = FilePathHelper.NormalizeFolderPath(sourceFolderPath);
                var normalizedDestPath = FilePathHelper.NormalizeFolderPath(destinationFolderPath);

                await _awsS3BinaryFileService.CopyFolderToFolderAsync(normalizedSourcePath, normalizedDestPath);
                return;
            }

            // For local storage
            var sourceLocalPath = FilePathHelper.NormalizeFolderPath(sourceFolderPath);
            var destLocalPath = FilePathHelper.NormalizeFolderPath(destinationFolderPath);
            Console.WriteLine($"Copying folder from {sourceLocalPath} to {destLocalPath}");
            await _localBinaryFileHelper.CopyFolderToFolderAsync(sourceLocalPath, destLocalPath);
        }


        public async Task DeleteFolderAsync(string folderPath)
        {
            if (_appConfig.FILE_STORAGE_SRC == "awsS3")
            {
                var normalizedFolderPath = FilePathHelper.NormalizeFolderPath(folderPath);
                await _awsS3BinaryFileService.DeleteFolderAsync(normalizedFolderPath);
                return;
            }

            // For local storage, normalize folder path
            var normalizedLocalFolderPath = FilePathHelper.NormalizeFolderPath(folderPath);
            await _localBinaryFileHelper.DeleteFolderAsync(normalizedLocalFolderPath);
        }


        public async Task DeleteFileAsync(string filePath)
        {
            if (_appConfig.FILE_STORAGE_SRC == "awsS3")
            {
                var normalizedFilePath = FilePathHelper.NormalizeFilePath(filePath);
                await _awsS3BinaryFileService.DeleteFileAsync(normalizedFilePath);
                return;
            }

            // For local storage, normalize file path
            var normalizedLocalFilePath = FilePathHelper.NormalizeFilePath(filePath);
            await _localBinaryFileHelper.DeleteFileAsync(normalizedLocalFilePath);
        }

        public async Task<string?> GetFullFileKeyAsync(string filePath)
        {
            if (_appConfig.FILE_STORAGE_SRC == "awsS3")
            {
                var normalizedFilePath = FilePathHelper.NormalizeFilePath(filePath);
                return await _awsS3BinaryFileService.GetFullFileKeyAsync(normalizedFilePath);
            }

            // For local storage, normalize file path
            var normalizedLocalFilePath = FilePathHelper.NormalizeFilePath(filePath);
            var fullFileKey = await _localBinaryFileHelper.GetFullFileKeyAsync(normalizedLocalFilePath);
            return fullFileKey != null ? fullFileKey : null;
        }


        public async Task<bool> FileExistsAsync(string filePath)
        {
            if (_appConfig.FILE_STORAGE_SRC == "awsS3")
            {
                var normalizedFilePath = FilePathHelper.NormalizeFilePath(filePath);
                return await _awsS3BinaryFileService.FileExistsAsync(normalizedFilePath);
            }

            // For local storage, normalize file path
            var normalizedLocalFilePath = FilePathHelper.NormalizeFilePath(filePath);
            var fullFileKey = await _localBinaryFileHelper.GetFullFileKeyAsync(normalizedLocalFilePath);
            return !string.IsNullOrEmpty(fullFileKey);
        }


        public async Task<byte[]?> GetFileBytesAsync(string filePath)
        {
            if (_appConfig.FILE_STORAGE_SRC == "awsS3")
            {
                var normalizedFilePath = FilePathHelper.NormalizeFilePath(filePath);
                return await _awsS3BinaryFileService.GetFileBytesAsync(normalizedFilePath);
            }

            // For local storage, normalize file path
            var normalizedLocalFilePath = FilePathHelper.NormalizeFilePath(filePath);
            return await _localBinaryFileHelper.GetFileBytesAsync(normalizedLocalFilePath);
        }

        public async Task<Stream?> GetFileStreamAsync(string filePath)
        {
            if (_appConfig.FILE_STORAGE_SRC == "awsS3")
            {
                var normalizedFilePath = FilePathHelper.NormalizeFilePath(filePath);
                return await _awsS3BinaryFileService.GetFileStreamAsync(normalizedFilePath);
            }

            // For local storage, normalize file path
            var normalizedLocalFilePath = FilePathHelper.NormalizeFilePath(filePath);
            return await _localBinaryFileHelper.GetFileStreamAsync(normalizedLocalFilePath);
        }


        public async Task<string> ConvertFileToBase64Async(string filePath)
        {
            try
            {
                var fileBytes = await GetFileBytesAsync(filePath);
                if (fileBytes == null || fileBytes.Length == 0)
                    return string.Empty;

                // Determine MIME type from file extension
                string extension = Path.GetExtension(filePath).ToLowerInvariant();
                string mimeType = _mediaTypeConfig.GetMimeTypeFromExtension(extension);

                return $"data:{mimeType};base64,{Convert.ToBase64String(fileBytes)}";
            }
            catch (Exception ex)
            {
                // Log error if needed
                Console.WriteLine($"Error converting file to Base64: {ex.Message}");
                return string.Empty;
            }
        }


    }

}
