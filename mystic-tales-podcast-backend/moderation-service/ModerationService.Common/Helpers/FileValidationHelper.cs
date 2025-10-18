using ModerationService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace ModerationService.Common.Helpers
{
    public static class FileValidationHelper
    {
        /// <summary>
        /// Validate file based on field name and file properties
        /// </summary>
        /// <param name="config">File validation config</param>
        /// <param name="fieldName">Field name (e.g., "Account.mainImageFileUrl")</param>
        /// <param name="fileName">Original file name with extension</param>
        /// <param name="fileSizeBytes">File size in bytes</param>
        /// <param name="mimeType">File MIME type</param>
        /// <returns>Validation result</returns>
        public static FileValidationResult ValidateFile(
            IFileValidationConfig config,
            string fieldName,
            string fileName,
            long fileSizeBytes,
            string mimeType)
        {
            var rule = config.GetValidationRule(fieldName);
            if (rule == null)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"No validation rule found for field: {fieldName}"
                };
            }

            // Check file size
            if (fileSizeBytes > rule.MaxSizeBytes)
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"File size ({FormatFileSize(fileSizeBytes)}) exceeds maximum allowed size ({FormatFileSize(rule.MaxSizeBytes)}) for {rule.Purpose}"
                };
            }

            // Check file extension
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!rule.AllowedExtensions.Contains(extension))
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"File extension '{extension}' is not allowed for {rule.Purpose}. Allowed extensions: {string.Join(", ", rule.AllowedExtensions)}"
                };
            }

            // Check MIME type
            if (!rule.AllowedMimeTypes.Contains(mimeType.ToLowerInvariant()))
            {
                return new FileValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"File type '{mimeType}' is not allowed for {rule.Purpose}. Allowed types: {string.Join(", ", rule.AllowedMimeTypes)}"
                };
            }

            return new FileValidationResult
            {
                IsValid = true,
                Rule = rule
            };
        }

        /// <summary>
        /// Get validation rule for a specific field
        /// </summary>
        public static FileValidationRule? GetValidationRule(IFileValidationConfig config, string fieldName)
        {
            return config.GetValidationRule(fieldName);
        }

        /// <summary>
        /// Format file size to human readable string
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Get MIME type from file extension
        /// </summary>
        public static string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                // Images
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".svg" => "image/svg+xml",
                ".bmp" => "image/bmp",
                
                // Audio
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".flac" => "audio/flac",
                ".m4a" => "audio/mp4",
                ".aac" => "audio/aac",
                
                // Documents
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                
                // Archives
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                
                _ => "application/octet-stream"
            };
        }
    }

    public class FileValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public FileValidationRule? Rule { get; set; }
    }
}