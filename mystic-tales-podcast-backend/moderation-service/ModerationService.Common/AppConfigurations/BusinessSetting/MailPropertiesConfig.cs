using Microsoft.Extensions.Configuration;
using ModerationService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace ModerationService.Common.AppConfigurations.BusinessSetting
{
    public class MailPropertiesConfigModel
    {
        public MailProperty CustomerRegistrationVerification { get; set; } = new MailProperty();
        public MailProperty PodcasterRequestConfirmation { get; set; } = new MailProperty();
        public MailProperty PodcasterRequestResult { get; set; } = new MailProperty();
        public MailProperty DMCANoticePending { get; } = new MailProperty();
        public MailProperty DMCACounterNoticePending { get; } = new MailProperty();
        public MailProperty DMCALawsuitProofPending { get; } = new MailProperty();
        public MailProperty DMCANoticeInvalid { get; } = new MailProperty();
        public MailProperty DMCACounterNoticeInvalidToAccused { get; } = new MailProperty();
        public MailProperty DMCACounterNoticeInvalidToAccuser { get; } = new MailProperty();
        public MailProperty DMCALawsuitProofInvalidToAccused { get; } = new MailProperty();
        public MailProperty DMCALawsuitProofInvalidToAccuser { get; } = new MailProperty();
        public MailProperty DMCALawsuitProofPodcasterWinToAccused { get; } = new MailProperty();
        public MailProperty DMCALawsuitProofPodcasterWinToAccuser { get; } = new MailProperty();
        public MailProperty DMCALawsuitProofAccuserWinToAccused { get; } = new MailProperty();
        public MailProperty DMCALawsuitProofAccuserWinToAccuser { get; } = new MailProperty();
        public MailProperty DMCANoticeValidToAccuser { get; } = new MailProperty();
        public MailProperty DMCANoticeValidToAccused { get; } = new MailProperty();
        public MailProperty DMCANoticeValidNotResponseInTimeToAccused { get; } = new MailProperty();
        public MailProperty DMCANoticeValidNotResponseInTimeToAccuser { get; } = new MailProperty();
        public MailProperty DMCANoticeValidAgreeTakenDownToAccused { get; } = new MailProperty();
        public MailProperty DMCANoticeValidAgreeTakenDownToAccuser { get; } = new MailProperty();
        public MailProperty DMCACounterNoticeConfirmation { get; } = new MailProperty();
        public MailProperty DMCACounterNoticeValidToAccused { get; } = new MailProperty();
        public MailProperty DMCACounterNoticeValidToAccuser { get; } = new MailProperty();
        public MailProperty DMCACounterNoticeValidNotResponseInTimeToAccused { get; } = new MailProperty();
        public MailProperty DMCACounterNoticeValidNotResponseInTimeToAccuser { get; } = new MailProperty();
        public MailProperty DMCALawsuitProofValidToAccused { get; } = new MailProperty();
        public MailProperty DMCALawsuitProofValidToAccuser { get; } = new MailProperty();
    }
    public class MailPropertiesConfig : IMailPropertiesConfig
    {
        public MailProperty CustomerRegistrationVerification { get; set; } = new MailProperty();
        public MailProperty PodcasterRequestConfirmation { get; set; } = new MailProperty();
        public MailProperty PodcasterRequestResult { get; set; } = new MailProperty();
        public MailProperty DMCANoticePending { get; set; } = new MailProperty();
        public MailProperty DMCACounterNoticePending { get; set; } = new MailProperty();
        public MailProperty DMCALawsuitProofPending { get; set; } = new MailProperty();
        public MailProperty DMCANoticeInvalid { get; set; } = new MailProperty();
        public MailProperty DMCACounterNoticeInvalidToAccused { get; set; } = new MailProperty();
        public MailProperty DMCACounterNoticeInvalidToAccuser { get; set; } = new MailProperty();
        public MailProperty DMCALawsuitProofInvalidToAccused { get; set; } = new MailProperty();
        public MailProperty DMCALawsuitProofInvalidToAccuser { get; set; } = new MailProperty();
        public MailProperty DMCALawsuitProofPodcasterWinToAccused { get; set; } = new MailProperty();
        public MailProperty DMCALawsuitProofPodcasterWinToAccuser { get; set; } = new MailProperty();
        public MailProperty DMCALawsuitProofAccuserWinToAccused { get; set; } = new MailProperty();
        public MailProperty DMCALawsuitProofAccuserWinToAccuser { get; set; } = new MailProperty();
        public MailProperty DMCANoticeValidToAccuser { get; set; } = new MailProperty();
        public MailProperty DMCANoticeValidToAccused { get; set; } = new MailProperty();
        public MailProperty DMCANoticeValidNotResponseInTimeToAccused { get; set; } = new MailProperty();
        public MailProperty DMCANoticeValidNotResponseInTimeToAccuser { get; set; } = new MailProperty();
        public MailProperty DMCANoticeValidAgreeTakenDownToAccused { get; set; } = new MailProperty();
        public MailProperty DMCANoticeValidAgreeTakenDownToAccuser { get; set; } = new MailProperty();
        public MailProperty DMCACounterNoticeConfirmation { get; set; } = new MailProperty();
        public MailProperty DMCACounterNoticeValidToAccused { get; set; } = new MailProperty();
        public MailProperty DMCACounterNoticeValidToAccuser { get; set; } = new MailProperty();
        public MailProperty DMCACounterNoticeValidNotResponseInTimeToAccused { get; set; } = new MailProperty();
        public MailProperty DMCACounterNoticeValidNotResponseInTimeToAccuser { get; set; } = new MailProperty();
        public MailProperty DMCALawsuitProofValidToAccused { get; set; } = new MailProperty();
        public MailProperty DMCALawsuitProofValidToAccuser { get; set; } = new MailProperty();

        public MailPropertiesConfig(IConfiguration configuration)
        {
            var mailConfig = configuration.GetSection("BusinessSettings:MailProperties").Get<MailPropertiesConfigModel>();
            CustomerRegistrationVerification = mailConfig.CustomerRegistrationVerification;
            PodcasterRequestConfirmation = mailConfig.PodcasterRequestConfirmation;
            PodcasterRequestResult = mailConfig.PodcasterRequestResult;
            DMCANoticePending = mailConfig.DMCANoticePending;
            DMCACounterNoticePending = mailConfig.DMCACounterNoticePending;
            DMCALawsuitProofPending = mailConfig.DMCALawsuitProofPending;
            DMCANoticeInvalid = mailConfig.DMCANoticeInvalid;
            DMCACounterNoticeInvalidToAccused = mailConfig.DMCACounterNoticeInvalidToAccused;
            DMCACounterNoticeInvalidToAccuser = mailConfig.DMCACounterNoticeInvalidToAccuser;
            DMCALawsuitProofInvalidToAccused = mailConfig.DMCALawsuitProofInvalidToAccused;
            DMCALawsuitProofInvalidToAccuser = mailConfig.DMCALawsuitProofInvalidToAccuser;
            DMCALawsuitProofPodcasterWinToAccused = mailConfig.DMCALawsuitProofPodcasterWinToAccused;
            DMCALawsuitProofPodcasterWinToAccuser = mailConfig.DMCALawsuitProofPodcasterWinToAccuser;
            DMCALawsuitProofAccuserWinToAccused = mailConfig.DMCALawsuitProofAccuserWinToAccused;
            DMCALawsuitProofAccuserWinToAccuser = mailConfig.DMCALawsuitProofAccuserWinToAccuser;
            DMCANoticeValidToAccuser = mailConfig.DMCANoticeValidToAccuser;
            DMCANoticeValidToAccused = mailConfig.DMCANoticeValidToAccused;
            DMCANoticeValidNotResponseInTimeToAccused = mailConfig.DMCANoticeValidNotResponseInTimeToAccused;
            DMCANoticeValidNotResponseInTimeToAccuser = mailConfig.DMCANoticeValidNotResponseInTimeToAccuser;
            DMCANoticeValidAgreeTakenDownToAccused = mailConfig.DMCANoticeValidAgreeTakenDownToAccused;
            DMCANoticeValidAgreeTakenDownToAccuser = mailConfig.DMCANoticeValidAgreeTakenDownToAccuser;
            DMCACounterNoticeConfirmation = mailConfig.DMCACounterNoticeConfirmation;
            DMCACounterNoticeValidToAccused = mailConfig.DMCACounterNoticeValidToAccused;
            DMCACounterNoticeValidToAccuser = mailConfig.DMCACounterNoticeValidToAccuser;
            DMCACounterNoticeValidNotResponseInTimeToAccused = mailConfig.DMCACounterNoticeValidNotResponseInTimeToAccused;
            DMCACounterNoticeValidNotResponseInTimeToAccuser = mailConfig.DMCACounterNoticeValidNotResponseInTimeToAccuser;
            DMCALawsuitProofValidToAccused = mailConfig.DMCALawsuitProofValidToAccused;
            DMCALawsuitProofValidToAccuser = mailConfig.DMCALawsuitProofValidToAccuser;
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
