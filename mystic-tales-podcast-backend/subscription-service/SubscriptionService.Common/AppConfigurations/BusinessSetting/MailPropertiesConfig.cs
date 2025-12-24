using Microsoft.Extensions.Configuration;
using SubscriptionService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace SubscriptionService.Common.AppConfigurations.BusinessSetting
{
    public class MailPropertiesConfigModel
    {
        public MailProperty CustomerRegistrationVerification { get; set; } = new MailProperty();
        public MailProperty PodcasterRequestConfirmation { get; set; } = new MailProperty();
        public MailProperty PodcasterRequestResult { get; set; } = new MailProperty();
        public MailProperty PodcastSubscriptionRegistration { get; set; } = new MailProperty();
        public MailProperty PodcastSubscriptionNewVersion { get; set; } = new MailProperty();
        public MailProperty PodcastSubscriptionRegistrationRenewalSuccess { get; set; } = new MailProperty();
        public MailProperty PodcastSubscriptionRegistrationRenewalFailure { get; set; } = new MailProperty();
        public MailProperty PodcastSubscriptionRegistrationCancel { get; set; } = new MailProperty();
        public MailProperty PodcastSubscriptionCancel { get; set; } = new MailProperty();
        public MailProperty PodcastSubscriptionInactive { get; set; } = new MailProperty();
        public MailProperty PodcastSubscriptionDuplicate { get; set; } = new MailProperty();
    }
    public class MailPropertiesConfig : IMailPropertiesConfig
    {
        public MailProperty CustomerRegistrationVerification { get; set; } = new MailProperty();
        public MailProperty PodcasterRequestConfirmation { get; set; } = new MailProperty();
        public MailProperty PodcasterRequestResult { get; set; } = new MailProperty();
        public MailProperty PodcastSubscriptionRegistration { get; set; } = new MailProperty();
        public MailProperty PodcastSubscriptionNewVersion { get; set; } = new MailProperty();
        public MailProperty PodcastSubscriptionRegistrationRenewalSuccess { get; set; } = new MailProperty();
        public MailProperty PodcastSubscriptionRegistrationRenewalFailure { get; set; } = new MailProperty();
        public MailProperty PodcastSubscriptionRegistrationCancel { get; set; } = new MailProperty();
        public MailProperty PodcastSubscriptionCancel { get; set; } = new MailProperty();
        public MailProperty PodcastSubscriptionInactive { get; set; } = new MailProperty();
        public MailProperty PodcastSubscriptionDuplicate { get; set; } = new MailProperty();

        public MailPropertiesConfig(IConfiguration configuration)
        {
            var mailConfig = configuration.GetSection("BusinessSettings:MailProperties").Get<MailPropertiesConfigModel>();
            CustomerRegistrationVerification = mailConfig.CustomerRegistrationVerification;
            PodcasterRequestConfirmation = mailConfig.PodcasterRequestConfirmation;
            PodcasterRequestResult = mailConfig.PodcasterRequestResult;
            PodcastSubscriptionRegistration = mailConfig.PodcastSubscriptionRegistration;
            PodcastSubscriptionNewVersion = mailConfig.PodcastSubscriptionNewVersion;
            PodcastSubscriptionRegistrationRenewalSuccess = mailConfig.PodcastSubscriptionRegistrationRenewalSuccess;
            PodcastSubscriptionRegistrationRenewalFailure = mailConfig.PodcastSubscriptionRegistrationRenewalFailure;
            PodcastSubscriptionRegistrationCancel = mailConfig.PodcastSubscriptionRegistrationCancel;
            PodcastSubscriptionCancel = mailConfig.PodcastSubscriptionCancel;
            PodcastSubscriptionInactive = mailConfig.PodcastSubscriptionInactive;
            PodcastSubscriptionDuplicate = mailConfig.PodcastSubscriptionDuplicate;
        }


        public MailProperty GetMailPropertyByTypeName(string mailTypeName)
        {
            if (string.IsNullOrWhiteSpace(mailTypeName))
                throw new ArgumentException("Mail type name cannot be null or empty", nameof(mailTypeName));

            // Tìm property có tên match với mailTypeName (case-insensitive)
            var property = this.GetType()
                .GetProperties()
                .FirstOrDefault(p => p.PropertyType == typeof(MailProperty) &&
                                    string.Equals(p.Name, mailTypeName, StringComparison.OrdinalIgnoreCase));

            if (property == null)
                throw new ArgumentException($"Invalid mail type: {mailTypeName}");

            return (MailProperty)property.GetValue(this)!;
        }

    }
}
