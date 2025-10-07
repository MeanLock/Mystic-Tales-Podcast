using System;
using System.Collections.Generic;

namespace TransactionService.DataAccess.Entities.sqlserver;

public partial class BookingTransaction
{
    public Guid Id { get; set; }

    public int BookingId { get; set; }

    public decimal Amount { get; set; }

    public decimal? Profit { get; set; }

    public int TransactionTypeId { get; set; }

    public int TransactionStatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual TransactionStatus TransactionStatus { get; set; } = null!;

    public virtual TransactionType TransactionType { get; set; } = null!;
}
