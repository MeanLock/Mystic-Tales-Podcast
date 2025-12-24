using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.BusinessLogic.DTOs.Snippet;

namespace TransactionService.BusinessLogic.DTOs.AccountBalanceTransaction.ListItems
{
    public class AccountBalanceWithdrawalRequestListItemResponseDTO
    {
        public Guid Id { get; set; }
        public AccountSnippetResponseDTO Account { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? TransferReceiptImageFileKey { get; set; }
        public string? RejectReason { get; set; }
        public bool? IsRejected { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
