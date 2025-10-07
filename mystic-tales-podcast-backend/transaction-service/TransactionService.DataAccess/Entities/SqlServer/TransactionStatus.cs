using System;
using System.Collections.Generic;

namespace TransactionService.DataAccess.Entities.sqlserver;

public partial class TransactionStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<AccountBalanceTransaction> AccountBalanceTransactions { get; set; } = new List<AccountBalanceTransaction>();

    public virtual ICollection<BookingStorageTransaction> BookingStorageTransactions { get; set; } = new List<BookingStorageTransaction>();

    public virtual ICollection<BookingTransaction> BookingTransactions { get; set; } = new List<BookingTransaction>();

    public virtual ICollection<MemberSubscriptionTransaction> MemberSubscriptionTransactions { get; set; } = new List<MemberSubscriptionTransaction>();

    public virtual ICollection<PodcastSubscriptionTransaction> PodcastSubscriptionTransactions { get; set; } = new List<PodcastSubscriptionTransaction>();
}
