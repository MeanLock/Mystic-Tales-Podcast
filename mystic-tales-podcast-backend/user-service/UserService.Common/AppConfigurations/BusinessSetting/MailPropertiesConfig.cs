using Microsoft.Extensions.Configuration;
using UserService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace UserService.Common.AppConfigurations.BusinessSetting
{
    public class MailPropertiesConfigModel
    {
        public MailProperty CustomerRegistrationVerification { get; set; } = new MailProperty();
        public MailProperty CustomerPasswordReset { get; set; } = new MailProperty();
        public MailProperty PodcasterRequestConfirmation { get; set; } = new MailProperty();
        public MailProperty PodcasterRequestResult { get; set; } = new MailProperty();
    }
    public class MailPropertiesConfig : IMailPropertiesConfig
    {
        public MailProperty CustomerPasswordReset { get; set; } = new MailProperty();
        public MailProperty CustomerRegistrationVerification { get; set; } = new MailProperty();
        public MailProperty PodcasterRequestConfirmation { get; set; } = new MailProperty();
        public MailProperty PodcasterRequestResult { get; set; } = new MailProperty();

        public MailPropertiesConfig(IConfiguration configuration)
        {
            var mailConfig = configuration.GetSection("BusinessSettings:MailProperties").Get<MailPropertiesConfigModel>();
            CustomerRegistrationVerification = mailConfig.CustomerRegistrationVerification;
            PodcasterRequestConfirmation = mailConfig.PodcasterRequestConfirmation;
            PodcasterRequestResult = mailConfig.PodcasterRequestResult;
            CustomerPasswordReset = mailConfig.CustomerPasswordReset;
        }

        // hàm nhận vào string mailType, trả về MailProperty tương ứng
        public MailProperty GetMailPropertyByType(string mailType)
        {
            return mailType.ToLower() switch
            {
                "customerregistrationverification" => CustomerRegistrationVerification,
                "customerpasswordreset" => CustomerPasswordReset,
                "podcasterrequestconfirmation" => PodcasterRequestConfirmation,
                "podcasterrequestresult" => PodcasterRequestResult,
                _ => throw new ArgumentException($"Invalid mail type: {mailType}")
            };
        }

    }
}
