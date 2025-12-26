using System;
using System.Collections.Generic;

namespace TransactionService.DataAccess.Entities.SqlServer;

public partial class AccountBalanceWithdrawalRequest
{
    public Guid Id { get; set; }

    public int AccountId { get; set; }

    public decimal Amount { get; set; }

    public string? TransferReceiptImageFileKey { get; set; }

    public string? RejectReason { get; set; }

    public bool? IsRejected { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string BankCode { get; set; } = null!;

    public string BankNumber { get; set; } = null!;

    public string BankName { get; set; } = null!;
}
