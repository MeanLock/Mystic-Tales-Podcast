using Newtonsoft.Json.Linq;

namespace ModerationService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IMailPropertiesConfig
    {
        MailProperty CustomerRegistrationVerification { get; }
        MailProperty PodcasterRequestConfirmation { get; }
        MailProperty PodcasterRequestResult { get; }
        MailProperty DMCANoticePending { get; }
        MailProperty DMCACounterNoticePending { get; }
        MailProperty DMCALawsuitProofPending { get; }
        MailProperty DMCANoticeInvalid { get; }
        MailProperty DMCACounterNoticeInvalidToAccused { get; }
        MailProperty DMCACounterNoticeInvalidToAccuser { get; }
        MailProperty DMCALawsuitProofInvalidToAccused { get; }
        MailProperty DMCALawsuitProofInvalidToAccuser { get; }
        MailProperty DMCALawsuitProofPodcasterWinToAccused { get; }
        MailProperty DMCALawsuitProofPodcasterWinToAccuser { get; }
        MailProperty DMCALawsuitProofAccuserWinToAccused { get; }
        MailProperty DMCALawsuitProofAccuserWinToAccuser { get; }
        MailProperty DMCANoticeValidToAccuser { get; }
        MailProperty DMCANoticeValidToAccused { get; }
        MailProperty DMCANoticeValidNotResponseInTimeToAccused { get; }
        MailProperty DMCANoticeValidNotResponseInTimeToAccuser { get; }
        MailProperty DMCANoticeValidAgreeTakenDownToAccused { get; }
        MailProperty DMCANoticeValidAgreeTakenDownToAccuser { get; }
        MailProperty DMCACounterNoticeConfirmation { get; }
        MailProperty DMCACounterNoticeValidToAccused { get; }
        MailProperty DMCACounterNoticeValidToAccuser { get; }
        MailProperty DMCACounterNoticeValidNotResponseInTimeToAccused { get; }
        MailProperty DMCACounterNoticeValidNotResponseInTimeToAccuser { get; }
        MailProperty DMCALawsuitProofValidToAccused { get; }
        MailProperty DMCALawsuitProofValidToAccuser { get; }

        MailProperty GetMailPropertyByTypeName(string mailTypeName);
    }
    
    public class MailProperty
    {
        public string Subject { get; set; } = string.Empty;
        public string TemplateFilePath { get; set; } = string.Empty;
    }
}
