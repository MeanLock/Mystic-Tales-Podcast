using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.AccountBalanceTransaction
{
    public class PaymentResultResponseDTO
    {
        public decimal Amount { get; set; } 
        public DateTime CompletedAt { get; set; }
    }
}
