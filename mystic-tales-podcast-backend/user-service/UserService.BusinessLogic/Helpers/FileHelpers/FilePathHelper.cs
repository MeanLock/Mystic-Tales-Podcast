using System;
using System.IO;

namespace UserService.BusinessLogic.Helpers.FileHelpers
{
    public class FilePathHelper
    {
        /// <summary>
        /// Normalizes a folder path to S3 format (xxx/xxx/xxx/xxx) without leading/trailing slashes
        /// </summary>
        /// <param name="folderPath">The folder path to normalize</param>
        /// <returns>Normalized folder path</returns>
        public static string NormalizeFolderPath(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return string.Empty;

            return folderPath
                .Replace("\\", "/")          // Convert backslashes to forward slashes
                .Trim('/', ' ')              // Remove leading/trailing slashes and spaces
                .Replace("//", "/");         // Remove double slashes
        }

        /// <summary>
        /// Normalizes a file path to S3 format (xxx/xxx/xxx/xxx) without leading slash
        /// </summary>
        /// <param name="filePath">The file path to normalize</param>
        /// <returns>Normalized file path</returns>
        public static string NormalizeFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return string.Empty;

            return filePath
                .Replace("\\", "/")          // Convert backslashes to forward slashes
                .TrimStart('/', ' ')         // Remove leading slashes and spaces
                .Replace("//", "/");         // Remove double slashes
        }

        /// <summary>
        /// Checks if a file path has an extension
        /// </summary>
        /// <param name="filePath">The file path to check</param>
        /// <returns>True if the file path has an extension, false otherwise</returns>
        public static bool HasExtension(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            var fileName = Path.GetFileName(filePath);
            return !string.IsNullOrEmpty(Path.GetExtension(fileName));
        }

        /// <summary>
        /// Extracts the filename (without extension) from a file path
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns>The filename without extension</returns>
        public static string GetFileNameWithoutExtension(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return string.Empty;

            var normalizedPath = NormalizeFilePath(filePath);
            var fileName = Path.GetFileName(normalizedPath);
            return Path.GetFileNameWithoutExtension(fileName);
        }

        /// <summary>
        /// Extracts the folder path from a full file path
        /// </summary>
        /// <param name="filePath">The full file path</param>
        /// <returns>The folder path portion</returns>
        public static string GetFolderPathFromFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return string.Empty;

            var normalizedPath = NormalizeFilePath(filePath);
            var directoryPath = Path.GetDirectoryName(normalizedPath);
            
            return string.IsNullOrEmpty(directoryPath) 
                ? string.Empty 
                : NormalizeFolderPath(directoryPath);
        }

        /// <summary>
        /// Combines multiple path parts to create a full S3 key
        /// </summary>
        /// <param name="pathParts">The path parts to combine</param>
        /// <returns>Combined S3 key path</returns>
        public static string CombinePaths(params string[] pathParts)
        {
            if (pathParts == null || pathParts.Length == 0)
                return string.Empty;

            var result = string.Empty;
            foreach (var part in pathParts)
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    var normalizedPart = part.Replace("\\", "/").Trim('/', ' ');
                    if (!string.IsNullOrEmpty(normalizedPart))
                    {
                        result = string.IsNullOrEmpty(result) 
                            ? normalizedPart 
                            : $"{result}/{normalizedPart}";
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Combines a folder path and filename to create a full S3 key
        /// </summary>
        /// <param name="folderPath">The folder path</param>
        /// <param name="fileName">The filename</param>
        /// <returns>Combined S3 key path</returns>
        public static string CombinePath(string folderPath, string fileName)
        {
            var normalizedFolderPath = NormalizeFolderPath(folderPath);
            
            if (string.IsNullOrEmpty(normalizedFolderPath))
                return fileName ?? string.Empty;

            if (string.IsNullOrEmpty(fileName))
                return normalizedFolderPath;

            return $"{normalizedFolderPath}/{fileName}";
        }

        /// <summary>
        /// Validates if a path is properly formatted for S3
        /// </summary>
        /// <param name="path">The path to validate</param>
        /// <returns>True if the path is valid, false otherwise</returns>
        public static bool IsValidS3Path(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            // Check for invalid characters
            var invalidChars = new[] { '\\', '<', '>', ':', '"', '|', '?', '*' };
            foreach (var invalidChar in invalidChars)
            {
                if (path.Contains(invalidChar))
                    return false;
            }

            // Check if path starts with slash (should not for S3)
            if (path.StartsWith("/"))
                return false;

            // Check for double slashes
            if (path.Contains("//"))
                return false;

            return true;
        }

        /// <summary>
        /// Extracts the filename with extension from a file path
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns>The filename with extension</returns>
        public static string GetFileName(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return string.Empty;

            var normalizedPath = NormalizeFilePath(filePath);
            return Path.GetFileName(normalizedPath);
        }

        /// <summary>
        /// Gets the file extension from a file path
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns>The file extension including the dot (e.g., ".jpg")</returns>
        public static string GetExtension(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return string.Empty;

            var fileName = GetFileName(filePath);
            return Path.GetExtension(fileName);
        }
    }
}