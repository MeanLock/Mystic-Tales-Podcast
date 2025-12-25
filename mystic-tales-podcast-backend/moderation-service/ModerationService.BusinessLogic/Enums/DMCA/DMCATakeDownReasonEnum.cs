using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.Enums.DMCA
{
    public enum DMCATakeDownReasonEnum
    {
        DuplicateContent = 1,
        RestrictedTermsViolation = 2,
        ExplicitOrAdultContent = 3,
        HateSpeech = 4,
        HarassmentAbuse = 5,
        PrivacyViolation = 6,
        Impersonation = 7,
        MisinformationFalseClaims = 8,
        PromotingIllegalActivity = 9,
        LawsuitDMCA = 10
    }
    public static class DMCATakeDownReasonEnumExtensions
    {
        public static string GetDescription(this DMCATakeDownReasonEnum reason)
        {
            return reason switch
            {
                DMCATakeDownReasonEnum.DuplicateContent => "Duplicate content",
                DMCATakeDownReasonEnum.RestrictedTermsViolation => "Violation of restricted terms",
                DMCATakeDownReasonEnum.ExplicitOrAdultContent => "Explicit or adult content",
                DMCATakeDownReasonEnum.HateSpeech => "Hate speech",
                DMCATakeDownReasonEnum.HarassmentAbuse => "Harassment or abuse",
                DMCATakeDownReasonEnum.PrivacyViolation => "Privacy violation",
                DMCATakeDownReasonEnum.Impersonation => "Impersonation",
                DMCATakeDownReasonEnum.MisinformationFalseClaims => "Misinformation or false claims",
                DMCATakeDownReasonEnum.PromotingIllegalActivity => "Promoting illegal activity",
                DMCATakeDownReasonEnum.LawsuitDMCA => "DMCA lawsuit",
                _ => "Unknown reason",
            };
        }
    }
}
