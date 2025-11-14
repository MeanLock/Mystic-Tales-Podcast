using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.BusinessLogic.DTOs.Transaction;

namespace TransactionService.BusinessLogic.DTOs.AccountBalanceTransaction.ListItems
{
    public class AccountBalanceTransactionListItemResponseDTO
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public TransactionTypeResponseDTO TransactionType { get; set; }
        public TransactionStatusResponseDTO TransactionStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ChangedAt { get; set; }
    }
}
