using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.AccountBalanceTransaction
{
    public class AccountBalanceWithdrawalRequestDTO
    {
        public decimal Amount { get; set; }
        public string BankCode { get; set; }
        public string BankNumber { get; set; }
        public string BankName { get; set; }
    }
}
