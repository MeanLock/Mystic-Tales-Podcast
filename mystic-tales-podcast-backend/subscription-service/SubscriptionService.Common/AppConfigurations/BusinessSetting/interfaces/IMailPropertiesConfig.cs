using Newtonsoft.Json.Linq;

namespace SubscriptionService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IMailPropertiesConfig
    {
        MailProperty CustomerRegistrationVerification { get; }
        MailProperty PodcasterRequestConfirmation { get; }
        MailProperty PodcasterRequestResult { get; }
        MailProperty PodcastSubscriptionRegistration { get; }
        MailProperty PodcastSubscriptionNewVersion { get; }
        MailProperty PodcastSubscriptionRegistrationRenewalSuccess { get; }
        MailProperty PodcastSubscriptionRegistrationRenewalFailure { get; }
        MailProperty PodcastSubscriptionRegistrationCancel { get; }
        MailProperty PodcastSubscriptionCancel { get; }
        MailProperty PodcastSubscriptionInactive { get; }
        MailProperty PodcastSubscriptionDuplicate { get; }
        MailProperty GetMailPropertyByTypeName(string mailTypeName);
    }
    
    public class MailProperty
    {
        public string Subject { get; set; } = string.Empty;
        public string TemplateFilePath { get; set; } = string.Empty;
    }
}
