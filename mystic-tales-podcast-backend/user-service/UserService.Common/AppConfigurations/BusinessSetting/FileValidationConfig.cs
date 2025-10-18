using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UserService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace UserService.Common.AppConfigurations.BusinessSetting
{
    public class FileValidationConfigModel
    {
        public Dictionary<string, FileValidationRuleModel> FileValidation { get; set; } = new();
    }

    public class FileValidationRuleModel
    {
        public string FieldName { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long MaxSizeBytes { get; set; }
        public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
        public string[] AllowedMimeTypes { get; set; } = Array.Empty<string>();
        public bool IsLegalDocument { get; set; } = false;
    }

    public class FileValidationConfig : IFileValidationConfig
    {
        public Dictionary<string, FileValidationRule> ValidationRules { get; private set; } = new();

        public FileValidationConfig(IConfiguration configuration)
        {
            LoadConfiguration(configuration);
        }

        private void LoadConfiguration(IConfiguration configuration)
        {
            var fileValidationSection = configuration.GetSection("BusinessSettings:FileValidation");
            
            if (fileValidationSection.Exists())
            {
                var configData = fileValidationSection.Get<Dictionary<string, FileValidationRuleModel>>();
                
                if (configData != null)
                {
                    foreach (var kvp in configData)
                    {
                        ValidationRules[kvp.Key] = new FileValidationRule
                        {
                            FieldName = kvp.Value.FieldName,
                            Purpose = kvp.Value.Purpose,
                            FileType = kvp.Value.FileType,
                            MaxSizeBytes = kvp.Value.MaxSizeBytes,
                            AllowedExtensions = kvp.Value.AllowedExtensions,
                            AllowedMimeTypes = kvp.Value.AllowedMimeTypes,
                            IsLegalDocument = kvp.Value.IsLegalDocument
                        };
                    }
                }
            }
        }

        public FileValidationRule? GetValidationRule(string fieldName)
        {
            return ValidationRules.TryGetValue(fieldName, out var rule) ? rule : null;
        }

        public bool IsValidFile(string fieldName, string fileName, long fileSizeBytes, string mimeType)
        {
            var rule = GetValidationRule(fieldName);
            if (rule == null)
                return false;

            // Check file size
            if (fileSizeBytes > rule.MaxSizeBytes)
                return false;

            // Check file extension
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!rule.AllowedExtensions.Contains(extension))
                return false;

            // Check MIME type
            if (!rule.AllowedMimeTypes.Contains(mimeType.ToLowerInvariant()))
                return false;

            return true;
        }
    }
}