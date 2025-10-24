using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.Enums.Transaction
{
    public enum TransactionStatusEnum
    {
        Pending = 1,
        Success = 2,
        Cancelled = 3,
        Error = 4
    }
}
