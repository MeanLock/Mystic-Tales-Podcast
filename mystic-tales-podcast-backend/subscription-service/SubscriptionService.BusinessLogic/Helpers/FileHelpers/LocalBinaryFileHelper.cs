using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Drawing;
using SubscriptionService.Common.AppConfigurations.App.interfaces;
using SubscriptionService.Common.AppConfigurations.Media.interfaces;
using SubscriptionService.Infrastructure.Configurations.AWS.interfaces;

namespace SubscriptionService.BusinessLogic.Helpers.FileHelpers
{
    public class LocalBinaryFileHelper
    {
        // CONFIG
        private readonly IAppConfig _appConfig;
        private readonly IMediaTypeConfig _mediaTypeConfig;
        private readonly IAWSS3Config _awsS3Config;
        private readonly string _baseUrl;
        private readonly string _wwwRootPath;

        public LocalBinaryFileHelper(IAppConfig appConfig, IMediaTypeConfig mediaTypeConfig, IAWSS3Config awsS3Config, IWebHostEnvironment env)
        {
            _appConfig = appConfig;
            _mediaTypeConfig = mediaTypeConfig;
            _awsS3Config = awsS3Config;
            _baseUrl = appConfig.APP_BASE_URL ?? "";
            _wwwRootPath = env.WebRootPath; // Đường dẫn tuyệt đối đến wwwroot
        }

        public byte[] ResizeBinaryImage(byte[] imageBytes, int maxWidth, int maxHeight)
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
                    // GIF animations will be lost if resized, return original
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
                    // Preserve original image format
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

        /////////////////////////////////////////////////////////////////

        public async Task SaveFile(byte[] fileData, string folderPath, string fileName, string fullFileName)
        {
            try
            {
                // Ensure the directory path is clean and convert to system format
                string fullDirectoryPath = Path.Combine(_wwwRootPath, folderPath.Replace('/', '\\'));

                // Create directory if it doesn't exist
                if (!Directory.Exists(fullDirectoryPath))
                {
                    Directory.CreateDirectory(fullDirectoryPath);
                }

                // Build full file path
                string fullFilePath = Path.Combine(fullDirectoryPath, fullFileName);

                // Delete existing file if it exists (with same name but different extension)
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                string[] existingFiles = Directory.GetFiles(fullDirectoryPath);
                foreach (var existingFile in existingFiles)
                {
                    string existingFileNameWithoutExt = Path.GetFileNameWithoutExtension(existingFile);
                    if (existingFileNameWithoutExt == fileNameWithoutExt)
                    {
                        File.Delete(existingFile);
                    }
                }

                // Write the new file
                await File.WriteAllBytesAsync(fullFilePath, fileData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file. Message: {ex.Message}");
                throw new Exception("Error saving file. " + ex.Message);
            }
        }

        public async Task<string> GenerateFileUrl(string filePath)
        {
            try
            {
                string? fullFileKey = await GetFullFileKeyAsync(filePath);
                if (fullFileKey == null)
                {
                    // Extract root folder from filePath to build unknown file URL
                    string[] pathParts = filePath.Split('/');
                    string rootFolderPath = pathParts.Length > 0 ? pathParts[0] : "uploads";
                    return $"{_baseUrl}/{rootFolderPath}/unknown.jpg";
                }
                Console.WriteLine("\n\n\n" + _baseUrl + "\n\n\n");
                // Build complete file path with extension
                return _baseUrl + $"/api/Misc/file-io-test/preview/{fullFileKey}".Replace("//", "/");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n\n" + ex.ToString() + "\n\n\n");
                string[] pathParts = filePath.Split('/');
                string rootFolderPath = pathParts.Length > 0 ? pathParts[0] : "uploads";
                return _baseUrl + $"/api/Misc/file-io-test/preview/{rootFolderPath}/unknown.jpg";
            }
        }

        public async Task UploadBinaryFileAsync(byte[] fileData, string folderPath, string fileName, string? mimeType = null)
        {
            try
            {
                // If it's an image, try to resize it first
                if (!string.IsNullOrEmpty(mimeType) && IsImageFile(mimeType) && fileData.Length > 1000) // Skip very small images
                {
                    try
                    {
                        fileData = ResizeBinaryImage(fileData, _awsS3Config.UploadImageMaxWidth, _awsS3Config.UploadImageMaxHeight);
                    }
                    catch (Exception resizeEx)
                    {
                        Console.WriteLine($"Could not resize image, uploading original: {resizeEx.Message}");
                        // Continue with original fileData if resize fails
                    }
                }

                // Determine extension from MIME type if provided, otherwise use .bin
                string extension = !string.IsNullOrEmpty(mimeType) ? _mediaTypeConfig.GetExtensionFromMimeType(mimeType) ?? ".bin" : ".bin";
                string originalFileName = fileName + extension;

                await SaveFile(fileData, folderPath, fileName, originalFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading binary file. Message: {ex.Message}");
                throw new HttpRequestException("Error uploading binary file. " + ex.Message);
            }
        }

        public async Task UploadBinaryFileWithStreamAsync(Stream fileStream, string folderPath, string fileName, string? mimeType = null)
        {
            try
            {
                // Convert stream to byte array
                using (var ms = new MemoryStream())
                {
                    await fileStream.CopyToAsync(ms);
                    byte[] fileData = ms.ToArray();

                    // If it's an image, try to resize it first
                    if (!string.IsNullOrEmpty(mimeType) && IsImageFile(mimeType) && fileData.Length > 1000) // Skip very small images
                    {
                        try
                        {
                            fileData = ResizeBinaryImage(fileData, _awsS3Config.UploadImageMaxWidth, _awsS3Config.UploadImageMaxHeight);
                        }
                        catch (Exception resizeEx)
                        {
                            Console.WriteLine($"Could not resize image, uploading original: {resizeEx.Message}");
                            // Continue with original fileData if resize fails
                        }
                    }

                    // Determine extension from MIME type if provided, otherwise use .bin
                    string extension = !string.IsNullOrEmpty(mimeType) ? _mediaTypeConfig.GetExtensionFromMimeType(mimeType) ?? ".bin" : ".bin";
                    string originalFileName = fileName + extension;

                    // Use existing SaveFile method
                    await SaveFile(fileData, folderPath, fileName, originalFileName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading binary file with stream. Message: {ex.Message}");
                throw new HttpRequestException("Error uploading binary file with stream. " + ex.Message);
            }
        }

        public async Task CopyFileToFileAsync(string sourceFilePath, string destinationFilePath)
        {
            try
            {
                string? fullSourceFileKey = await GetFullFileKeyAsync(sourceFilePath);
                if (fullSourceFileKey == null) return;

                // Build full source path
                string fullSourceFilePath = Path.Combine(_wwwRootPath, fullSourceFileKey.Replace('/', '\\'));

                // Get destination directory and filename
                string destinationDirectoryPath = Path.GetDirectoryName(destinationFilePath) ?? "";
                string destinationFileName = Path.GetFileNameWithoutExtension(destinationFilePath);
                string sourceExtension = Path.GetExtension(fullSourceFileKey);
                string fullDestinationDirectoryPath = Path.Combine(_wwwRootPath, destinationDirectoryPath.Replace('/', '\\'));
                string fullDestinationFilePath = Path.Combine(fullDestinationDirectoryPath, destinationFileName + sourceExtension);

                if (!Directory.Exists(fullDestinationDirectoryPath))
                {
                    Directory.CreateDirectory(fullDestinationDirectoryPath);
                }

                File.Copy(fullSourceFilePath, fullDestinationFilePath, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n\n CopyFile: " + ex.ToString() + "\n\n\n");
                throw new Exception($"Error copying file: {ex.Message}");
            }
        }

        public async Task CopyFolderToFolderAsync(string sourceFolderPath, string destinationFolderPath)
        {
            try
            {
                // Build full source path
                string fullSourceFolderPath = Path.Combine(_wwwRootPath, sourceFolderPath.Replace('/', '\\'));

                // Check if source folder exists
                if (!Directory.Exists(fullSourceFolderPath))
                {
                    Console.WriteLine($"\n\n\n Source folder not found: {fullSourceFolderPath} \n\n\n");
                    return;
                }

                // Build full destination path
                string fullDestinationFolderPath = Path.Combine(_wwwRootPath, destinationFolderPath.Replace('/', '\\'));

                // Create destination folder if it doesn't exist
                if (!Directory.Exists(fullDestinationFolderPath))
                {
                    Directory.CreateDirectory(fullDestinationFolderPath);
                }

                // Get all files in source folder (recursively)
                string[] sourceFiles = Directory.GetFiles(fullSourceFolderPath, "*", SearchOption.AllDirectories);

                // Copy each file to destination
                foreach (var sourceFile in sourceFiles)
                {
                    try
                    {
                        // Calculate relative path from source folder
                        string relativePath = Path.GetRelativePath(fullSourceFolderPath, sourceFile);

                        // Build destination file path
                        string destinationFile = Path.Combine(fullDestinationFolderPath, relativePath);

                        // Create destination directory if needed
                        string? destinationDirectory = Path.GetDirectoryName(destinationFile);
                        if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                        {
                            Directory.CreateDirectory(destinationDirectory);
                        }

                        // Copy file (overwrite if exists)
                        File.Copy(sourceFile, destinationFile, true);
                    }
                    catch (Exception fileEx)
                    {
                        Console.WriteLine($"\n\n\n Error copying file {sourceFile}: {fileEx.ToString()} \n\n\n");
                        // Continue with next file instead of throwing
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n\n CopyFolder: " + ex.ToString() + "\n\n\n");
                throw new Exception($"Error copying folder: {ex.Message}");
            }
        }

        public Task CreateFolderAsync(string folderPath)
        {
            try
            {
                string fullFolderPath = Path.Combine(_wwwRootPath, folderPath.Replace('/', '\\'));
                if (Directory.Exists(fullFolderPath))
                {
                    return Task.CompletedTask;
                }
                Directory.CreateDirectory(fullFolderPath);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating folder {folderPath}: {ex.Message}");
            }
        }
        public async Task DeleteFileAsync(string filePath)
        {
            try
            {
                string? fullFileKey = await GetFullFileKeyAsync(filePath);
                if (fullFileKey == null) return;

                string fullFilePath = Path.Combine(_wwwRootPath, fullFileKey.Replace('/', '\\'));

                if (!File.Exists(fullFilePath)) return;

                File.Delete(fullFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n\n DeleteFile:" + ex.ToString() + "\n\n\n");
                throw new Exception($"Error deleting file {filePath}: {ex.Message}");
            }
        }

        public Task DeleteFolderAsync(string folderPath)
        {
            try
            {
                string fullFolderPath = Path.Combine(_wwwRootPath, folderPath.Replace('/', '\\'));
                if (!Directory.Exists(fullFolderPath))
                {
                    return Task.CompletedTask;
                }
                Directory.Delete(fullFolderPath, true);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting folder {folderPath}: {ex.Message}");
            }
        }

        public async Task<byte[]?> GetFileBytesAsync(string filePath)
        {
            try
            {
                var fullFileKey = await GetFullFileKeyAsync(filePath);
                if (fullFileKey != null)
                {
                    string fullFilePath = Path.Combine(_wwwRootPath, fullFileKey.Replace('/', '\\'));
                    return await File.ReadAllBytesAsync(fullFilePath);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n\n" + ex.ToString() + "\n\n\n");
                return null;
            }
        }

        public async Task<Stream?> GetFileStreamAsync(string filePath)
        {
            try
            {
                var fullFileKey = await GetFullFileKeyAsync(filePath);
                if (fullFileKey != null)
                {
                    var fullPath = Path.Combine(_wwwRootPath, fullFileKey.Replace('/', '\\'));
                    return File.OpenRead(fullPath);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting file stream: {ex.Message}");
                return null;
            }
        }


        public Task<string?> GetFullFileKeyAsync(string filePath)
        {
            try
            {
                // Check if filePath already has extension
                string extension = Path.GetExtension(filePath);
                if (!string.IsNullOrEmpty(extension))
                {
                    // File path already has extension, return as is
                    return Task.FromResult<string?>(filePath);
                }

                // Extract folder path and filename from full path
                string directoryPath = Path.GetDirectoryName(filePath) ?? "";
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);

                string fullDirectoryPath = Path.Combine(_wwwRootPath, directoryPath.Replace('/', '\\'));
                if (!Directory.Exists(fullDirectoryPath))
                {
                    return Task.FromResult<string?>(null);
                }

                string[] files = Directory.GetFiles(fullDirectoryPath);
                foreach (var file in files)
                {
                    string fileWithoutExt = Path.GetFileNameWithoutExtension(file);
                    if (fileWithoutExt == fileNameWithoutExt)
                    {
                        // Return full path with extension
                        string foundFileName = Path.GetFileName(file);
                        string fullFileKey = string.IsNullOrEmpty(directoryPath) ? foundFileName : $"{directoryPath.Replace('\\', '/')}/{foundFileName}";
                        return Task.FromResult<string?>(fullFileKey);
                    }
                }
                return Task.FromResult<string?>(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n\n" + ex.ToString() + "\n\n\n");
                return Task.FromResult<string?>(null);
            }
        }

        public async Task<string?> GetImageBase64(string filePath)
        {
            try
            {
                byte[]? fileBytes = await GetFileBytesAsync(filePath);
                if (fileBytes == null)
                {
                    Console.WriteLine($"File not found: {filePath}");
                    return null;
                }

                // Convert to base64
                string base64String = Convert.ToBase64String(fileBytes);

                // Get file extension to determine MIME type
                string? fullFileKey = await GetFullFileKeyAsync(filePath);
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
                Console.WriteLine($"Error getting image base64: {ex.Message}");
                return null;
            }
        }


        // Helper method
        public bool IsImageFile(string mimeType)
        {
            return mimeType != null && mimeType.StartsWith("image/");
        }
        public async Task<bool> FileExistsAsync(string filePath)
        {
            try
            {
                var fullFileKey = await GetFullFileKeyAsync(filePath);
                if (fullFileKey != null)
                {
                    string fullFilePath = Path.Combine(_wwwRootPath, fullFileKey.Replace('/', '\\'));
                    return File.Exists(fullFilePath);
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\n\n" + ex.ToString() + "\n\n\n");
                return false;
            }
        }

    }
}
