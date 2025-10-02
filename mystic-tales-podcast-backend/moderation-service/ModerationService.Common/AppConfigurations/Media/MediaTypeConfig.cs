using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using ModerationService.Common.AppConfigurations.Media.interfaces;

namespace ModerationService.Common.AppConfigurations.Media
{
    public class MediaTypeConfig : IMediaTypeConfig
    {
        public Dictionary<string, string> MimeTypeToExtensionMap { get; private set; } = null!;
        public Dictionary<string, string> ExtensionToMimeTypeMap { get; private set; } = null!;

        public MediaTypeConfig()
        {
            InitializeMappings();
        }

        private void InitializeMappings()
        {
            // Initialize Extension to MIME Type mapping
            ExtensionToMimeTypeMap = new Dictionary<string, string>
            {
                // Images
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" },
                { ".gif", "image/gif" },
                { ".bmp", "image/bmp" },
                { ".webp", "image/webp" },
                { ".svg", "image/svg+xml" },
                { ".ico", "image/x-icon" },
                { ".tiff", "image/tiff" },
                { ".tif", "image/tiff" },
                { ".tga", "image/x-tga" },
                { ".psd", "image/vnd.adobe.photoshop" },

                // Documents
                { ".pdf", "application/pdf" },
                { ".doc", "application/msword" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".ppt", "application/vnd.ms-powerpoint" },
                { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                { ".rtf", "application/rtf" },
                { ".odt", "application/vnd.oasis.opendocument.text" },
                { ".ods", "application/vnd.oasis.opendocument.spreadsheet" },
                { ".odp", "application/vnd.oasis.opendocument.presentation" },

                // Text files
                { ".txt", "text/plain" },
                { ".csv", "text/csv" },
                { ".html", "text/html" },
                { ".htm", "text/html" },
                { ".css", "text/css" },
                { ".js", "application/javascript" },
                { ".json", "application/json" },
                { ".xml", "application/xml" },
                { ".md", "text/markdown" },
                { ".yml", "text/yaml" },
                { ".yaml", "application/x-yaml" },

                // Programming files
                { ".py", "text/x-python" },
                { ".java", "text/x-java-source" },
                { ".cs", "text/x-csharp" },
                { ".c", "text/x-c" },
                { ".cpp", "text/x-c++src" },
                { ".cxx", "text/x-c++src" },
                { ".cc", "text/x-c++src" },
                { ".php", "text/x-php" },
                { ".rb", "text/x-ruby" },
                { ".go", "text/x-go" },
                { ".rs", "text/x-rust" },
                { ".ts", "application/typescript" },

                // Audio
                { ".mp3", "audio/mpeg" },
                { ".wav", "audio/wav" },
                { ".ogg", "audio/ogg" },
                { ".m4a", "audio/mp4" },
                { ".aac", "audio/aac" },
                { ".flac", "audio/flac" },
                { ".wma", "audio/x-ms-wma" },
                { ".weba", "audio/webm" },

                // Video
                { ".mp4", "video/mp4" },
                { ".webm", "video/webm" },
                { ".ogv", "video/ogg" },
                { ".avi", "video/x-msvideo" },
                { ".mov", "video/quicktime" },
                { ".wmv", "video/x-ms-wmv" },
                { ".flv", "video/x-flv" },
                { ".3gp", "video/3gpp" },
                { ".mkv", "video/x-matroska" },

                // Archives
                { ".zip", "application/zip" },
                { ".rar", "application/vnd.rar" },
                { ".7z", "application/x-7z-compressed" },
                { ".tar", "application/x-tar" },
                { ".gz", "application/gzip" },
                { ".bz2", "application/x-bzip2" },
                { ".xz", "application/x-xz" },

                // Executables
                { ".exe", "application/x-msdownload" },
                { ".deb", "application/x-deb" },
                { ".rpm", "application/x-rpm" },
                { ".pkg", "application/vnd.apple.installer+xml" },
                { ".dmg", "application/x-apple-diskimage" },

                // Fonts
                { ".ttf", "font/ttf" },
                { ".otf", "font/otf" },
                { ".woff", "font/woff" },
                { ".woff2", "font/woff2" },
                { ".eot", "application/vnd.ms-fontobject" },

                // 3D Models
                { ".obj", "model/obj" },
                { ".gltf", "model/gltf+json" },
                { ".glb", "model/gltf-binary" }
            };

            // Initialize MIME Type to Extension mapping (reverse mapping)
            MimeTypeToExtensionMap = new Dictionary<string, string>
            {
                // Images
                { "image/jpeg", ".jpg" },
                { "image/png", ".png" },
                { "image/gif", ".gif" },
                { "image/bmp", ".bmp" },
                { "image/webp", ".webp" },
                { "image/svg+xml", ".svg" },
                { "image/x-icon", ".ico" },
                { "image/tiff", ".tiff" },
                { "image/x-tga", ".tga" },
                { "image/vnd.adobe.photoshop", ".psd" },

                // Documents
                { "application/pdf", ".pdf" },
                { "application/msword", ".doc" },
                { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx" },
                { "application/vnd.ms-excel", ".xls" },
                { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx" },
                { "application/vnd.ms-powerpoint", ".ppt" },
                { "application/vnd.openxmlformats-officedocument.presentationml.presentation", ".pptx" },
                { "application/rtf", ".rtf" },
                { "application/vnd.oasis.opendocument.text", ".odt" },
                { "application/vnd.oasis.opendocument.spreadsheet", ".ods" },
                { "application/vnd.oasis.opendocument.presentation", ".odp" },

                // Text files
                { "text/plain", ".txt" },
                { "text/csv", ".csv" },
                { "text/html", ".html" },
                { "text/css", ".css" },
                { "application/javascript", ".js" },
                { "application/json", ".json" },
                { "application/xml", ".xml" },
                { "text/markdown", ".md" },
                { "text/yaml", ".yml" },
                { "application/x-yaml", ".yaml" },

                // Programming files
                { "text/x-python", ".py" },
                { "text/x-java-source", ".java" },
                { "text/x-csharp", ".cs" },
                { "text/x-c", ".c" },
                { "text/x-c++src", ".cpp" },
                { "text/x-php", ".php" },
                { "text/x-ruby", ".rb" },
                { "text/x-go", ".go" },
                { "text/x-rust", ".rs" },
                { "application/typescript", ".ts" },

                // Audio
                { "audio/mpeg", ".mp3" },
                { "audio/wav", ".wav" },
                { "audio/ogg", ".ogg" },
                { "audio/mp4", ".m4a" },
                { "audio/aac", ".aac" },
                { "audio/flac", ".flac" },
                { "audio/x-ms-wma", ".wma" },
                { "audio/webm", ".weba" },

                // Video
                { "video/mp4", ".mp4" },
                { "video/webm", ".webm" },
                { "video/ogg", ".ogv" },
                { "video/x-msvideo", ".avi" },
                { "video/quicktime", ".mov" },
                { "video/x-ms-wmv", ".wmv" },
                { "video/x-flv", ".flv" },
                { "video/3gpp", ".3gp" },
                { "video/x-matroska", ".mkv" },

                // Archives
                { "application/zip", ".zip" },
                { "application/vnd.rar", ".rar" },
                { "application/x-7z-compressed", ".7z" },
                { "application/x-tar", ".tar" },
                { "application/gzip", ".gz" },
                { "application/x-bzip2", ".bz2" },
                { "application/x-xz", ".xz" },

                // Executables
                { "application/x-msdownload", ".exe" },
                { "application/x-deb", ".deb" },
                { "application/x-rpm", ".rpm" },
                { "application/vnd.apple.installer+xml", ".pkg" },
                { "application/x-apple-diskimage", ".dmg" },

                // Fonts
                { "font/ttf", ".ttf" },
                { "font/otf", ".otf" },
                { "font/woff", ".woff" },
                { "font/woff2", ".woff2" },
                { "application/vnd.ms-fontobject", ".eot" },

                // 3D Models
                { "model/obj", ".obj" },
                { "model/gltf+json", ".gltf" },
                { "model/gltf-binary", ".glb" }
            };
        }

        public static FileExtensionContentTypeProvider GetFileExtensionContentTypeProvider()
        {
            var provider = new FileExtensionContentTypeProvider();

            foreach (var mapping in new MediaTypeConfig().ExtensionToMimeTypeMap)
            {
                // Add custom mappings, overriding existing ones if necessary
                provider.Mappings[mapping.Key] = mapping.Value;
            }

            return provider;
        }

        public string? GetExtensionFromMimeType(string mimeType)
        {
            if (string.IsNullOrEmpty(mimeType))
                return null;

            return MimeTypeToExtensionMap.GetValueOrDefault(mimeType.ToLower());
        }

        public string GetMimeTypeFromExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return "application/octet-stream";

            // Ensure extension starts with a dot
            if (!extension.StartsWith("."))
                extension = "." + extension;

            return ExtensionToMimeTypeMap.GetValueOrDefault(extension.ToLower(), "application/octet-stream");
        }
    }
}
