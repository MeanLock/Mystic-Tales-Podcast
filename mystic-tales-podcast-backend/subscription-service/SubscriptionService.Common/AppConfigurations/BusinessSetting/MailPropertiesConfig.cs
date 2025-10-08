using Microsoft.Extensions.Configuration;
using SubscriptionService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace SubscriptionService.Common.AppConfigurations.BusinessSetting
{
    public class MailPropertiesConfigModel
    {
        public MailProperty CustomerRegistrationVerification { get; set; } = new MailProperty();
        public MailProperty PodcasterRequestConfirmation { get; set; } = new MailProperty();
        public MailProperty PodcasterRequestResult { get; set; } = new MailProperty();
    }
    public class MailPropertiesConfig : IMailPropertiesConfig
    {
        public MailProperty CustomerRegistrationVerification { get; set; } = new MailProperty();
        public MailProperty PodcasterRequestConfirmation { get; set; } = new MailProperty();
        public MailProperty PodcasterRequestResult { get; set; } = new MailProperty();

        public MailPropertiesConfig(IConfiguration configuration)
        {
            var mailConfig = configuration.GetSection("BusinessSettings:MailProperties").Get<MailPropertiesConfigModel>();
            CustomerRegistrationVerification = mailConfig.CustomerRegistrationVerification;
            PodcasterRequestConfirmation = mailConfig.PodcasterRequestConfirmation;
            PodcasterRequestResult = mailConfig.PodcasterRequestResult;
        }


        public MailProperty GetMailPropertyByTypeName(string mailTypeName)
        {
            return mailTypeName.ToLower() switch
            {
                "customerregistrationverification" => CustomerRegistrationVerification,
                "podcasterrequestconfirmation" => PodcasterRequestConfirmation,
                "podcasterrequestresult" => PodcasterRequestResult,
                _ => throw new ArgumentException($"Invalid mail type: {mailTypeName}")
            };
        }

    }
}
