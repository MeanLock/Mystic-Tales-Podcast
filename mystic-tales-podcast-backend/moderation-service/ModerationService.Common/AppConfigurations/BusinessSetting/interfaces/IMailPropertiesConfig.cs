using Newtonsoft.Json.Linq;

namespace ModerationService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IMailPropertiesConfig
    {
        MailProperty CustomerRegistrationVerification { get; }
        MailProperty PodcasterRequestConfirmation { get; }
        MailProperty PodcasterRequestResult { get; }
    }
    
    public class MailProperty
    {
        public string Subject { get; set; } = string.Empty;
        public string TemplateFilePath { get; set; } = string.Empty;
    }
}
