using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.Enums.DMCA
{
    public enum DMCAAccusationConclusionReportTypeEnum
    {
        InvalidDMCANotice = 1,
        InvalidCounterNotice = 2,
        InvalidLawsuitProof = 3,
        PodcasterLawsuitWin = 4,
        AccuserLawsuitWin = 5
    }
}
