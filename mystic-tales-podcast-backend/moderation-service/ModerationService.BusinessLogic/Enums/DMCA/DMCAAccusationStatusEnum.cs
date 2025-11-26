using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.Enums.DMCA
{
    public enum DMCAAccusationStatusEnum
    {
        PendingDMCANoticeReview = 1,
        InvalidDMCANotice = 2,
        ValidDMCANotice = 3,
        InvalidCounterNotice = 4,
        ValidCounterNotice = 5,
        InvalidLawsuitProof = 6,
        ValidLawsuitProof = 7,
        PodcasterLawsuitWin = 8,
        AccuserLawsuitWin = 9,
        UnresolvedDismissed = 10,
        DirectResolveDismissed = 11
    }
}
