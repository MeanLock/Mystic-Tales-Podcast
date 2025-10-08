using Newtonsoft.Json.Linq;

namespace UserService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IMailPropertiesConfig
    {
        MailProperty CustomerRegistrationVerification { get; }
        MailProperty CustomerPasswordReset { get; }
        MailProperty PodcasterRequestConfirmation { get; }
        MailProperty PodcasterRequestResult { get; }

        MailProperty GetMailPropertyByTypeName(string mailTypeName);
    }
    
    public class MailProperty
    {
        public string Subject { get; set; } = string.Empty;
        public string TemplateFilePath { get; set; } = string.Empty;
    }
}
