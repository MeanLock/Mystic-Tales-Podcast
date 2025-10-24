using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.Enums.DMCA
{
    public enum DMCAAccusationStatusEnum
    {
        Pending = 1,
        Reviewing = 2,
        Rejected = 3,
        TakeDownPermanent = 4,
        CloseWithdrawn = 5,
        CounterReviewing = 6,
        LawsuitPending = 7,
        LawsuitFiled = 8,
        LawsuitVerified = 9,
        DMCAWins = 10,
        CounterWins = 11
    }
}
