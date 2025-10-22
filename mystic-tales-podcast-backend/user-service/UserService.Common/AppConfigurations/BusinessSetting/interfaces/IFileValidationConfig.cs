using Newtonsoft.Json.Linq;

namespace UserService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IFileValidationConfig
    {
        Dictionary<string, FileValidationRule> ValidationRules { get; }
        FileValidationRule? GetValidationRule(string fieldName);
        bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType);
    }

    public class FileValidationRule
    {
        public string FieldName { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long MaxSizeBytes { get; set; }
        public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
        public string[] AllowedMimeTypes { get; set; } = Array.Empty<string>();
        public bool IsLegalDocument { get; set; } = false;
    }

    public enum FileType
    {
        Image,
        Audio,
        Document,
        DocumentPDF,
        Mixed
    }
}