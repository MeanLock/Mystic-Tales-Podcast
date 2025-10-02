using Microsoft.AspNetCore.StaticFiles;

namespace BookingManagementService.Common.AppConfigurations.Media.interfaces
{
    public interface IMediaTypeConfig
    {
        // FileExtensionContentTypeProvider GetContentTypeProvider(); // Removed invalid static method from interface

        /// <summary>
        /// Get file extension from MIME type
        /// </summary>
        /// <param name="mimeType">The MIME type to convert</param>
        /// <returns>File extension including the dot (e.g., ".jpg") or null if not found</returns>
        string? GetExtensionFromMimeType(string mimeType);

        /// <summary>
        /// Get MIME type from file extension
        /// </summary>
        /// <param name="extension">The file extension with or without dot</param>
        /// <returns>MIME type string (e.g., "image/jpeg") or "application/octet-stream" if not found</returns>
        string GetMimeTypeFromExtension(string extension);

        /// <summary>
        /// Dictionary mapping MIME types to their corresponding file extensions
        /// </summary>
        Dictionary<string, string> MimeTypeToExtensionMap { get; }

        /// <summary>
        /// Dictionary mapping file extensions to their corresponding MIME types
        /// </summary>
        Dictionary<string, string> ExtensionToMimeTypeMap { get; }
    }
}
