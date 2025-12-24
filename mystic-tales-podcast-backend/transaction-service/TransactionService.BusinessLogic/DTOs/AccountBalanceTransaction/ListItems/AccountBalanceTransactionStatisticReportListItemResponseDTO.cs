using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.Booking.ListItems
{
    public class AccountBalanceTransactionStatisticReportListItemResponseDTO
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal DepositTransactionAmount { get; set; }
        public decimal WithdrawalTransactionAmount { get; set; }
    }
}
