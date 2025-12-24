using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.BusinessLogic.DTOs.Transaction;

namespace TransactionService.BusinessLogic.DTOs.AccountBalanceTransaction.ListItems
{
    public class BalanceChangeHistoryListItemResponseDTO
    {
        public decimal Amount { get; set; }
        public TransactionTypeResponseDTO TransactionType { get; set; }
        public TransactionStatusResponseDTO TransactionStatus { get; set; }
        public bool IsReceived { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
