using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.Enums.DMCA
{
    public enum DMCAAccusationQueryEnum
    {
        TAKEDOWN_ACTIVE = 1,
        REJECTED = 2,
        CLOSED_WITHDRAWN = 3,
        TAKEDOWN_PERMANENT = 4,
        COUNTER_ACCEPTED = 5,
        COUNTER_REJECTED = 6,
        LAWSUIT_VERIFIED = 7,
        LAWSUIT_REJECTED = 8,
        DMCA_WINS = 9,
        COUNTER_WINS = 10
    }
}
