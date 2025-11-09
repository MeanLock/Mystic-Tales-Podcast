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
}
