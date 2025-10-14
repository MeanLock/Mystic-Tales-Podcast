using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TransactionService.DataAccess.Entities.SqlServer;

namespace TransactionService.DataAccess.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccountBalanceTransaction> AccountBalanceTransactions { get; set; }

    public virtual DbSet<BookingStorageTransaction> BookingStorageTransactions { get; set; }

    public virtual DbSet<BookingTransaction> BookingTransactions { get; set; }

    public virtual DbSet<MemberSubscriptionTransaction> MemberSubscriptionTransactions { get; set; }

    public virtual DbSet<PodcastSubscriptionTransaction> PodcastSubscriptionTransactions { get; set; }

    public virtual DbSet<TransactionStatus> TransactionStatuses { get; set; }

    public virtual DbSet<TransactionType> TransactionTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost,1434;Database=MTP_TransactionDb_dev;User Id=sa;Password=Banana100;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountBalanceTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AccountB__3213E83F534726D0");

            entity.ToTable("AccountBalanceTransaction", tb => tb.HasTrigger("TR_AccountBalanceTransaction_UpdatedAt"));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.OrderCode).HasColumnName("orderCode");
            entity.Property(e => e.TransactionStatusId).HasColumnName("transactionStatusId");
            entity.Property(e => e.TransactionTypeId).HasColumnName("transactionTypeId");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.TransactionStatus).WithMany(p => p.AccountBalanceTransactions)
                .HasForeignKey(d => d.TransactionStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccountBa__trans__5070F446");

            entity.HasOne(d => d.TransactionType).WithMany(p => p.AccountBalanceTransactions)
                .HasForeignKey(d => d.TransactionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccountBa__trans__4F7CD00D");
        });

        modelBuilder.Entity<BookingStorageTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingS__3213E83F07961E64");

            entity.ToTable("BookingStorageTransaction", tb => tb.HasTrigger("TR_BookingStorageTransaction_UpdatedAt"));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.StorageSize).HasColumnName("storageSize");
            entity.Property(e => e.TransactionStatusId).HasColumnName("transactionStatusId");
            entity.Property(e => e.TransactionTypeId).HasColumnName("transactionTypeId");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.TransactionStatus).WithMany(p => p.BookingStorageTransactions)
                .HasForeignKey(d => d.TransactionStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingSt__trans__6754599E");

            entity.HasOne(d => d.TransactionType).WithMany(p => p.BookingStorageTransactions)
                .HasForeignKey(d => d.TransactionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingSt__trans__66603565");
        });

        modelBuilder.Entity<BookingTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingT__3213E83F0FE5CC14");

            entity.ToTable("BookingTransaction", tb => tb.HasTrigger("TR_BookingTransaction_UpdatedAt"));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Profit)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("profit");
            entity.Property(e => e.TransactionStatusId).HasColumnName("transactionStatusId");
            entity.Property(e => e.TransactionTypeId).HasColumnName("transactionTypeId");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.TransactionStatus).WithMany(p => p.BookingTransactions)
                .HasForeignKey(d => d.TransactionStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingTr__trans__619B8048");

            entity.HasOne(d => d.TransactionType).WithMany(p => p.BookingTransactions)
                .HasForeignKey(d => d.TransactionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingTr__trans__60A75C0F");
        });

        modelBuilder.Entity<MemberSubscriptionTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MemberSu__3213E83FB7CA02F9");

            entity.ToTable("MemberSubscriptionTransaction", tb => tb.HasTrigger("TR_MemberSubscriptionTransaction_UpdatedAt"));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.MemberSubscriptionRegistrationId).HasColumnName("memberSubscriptionRegistrationId");
            entity.Property(e => e.TransactionStatusId).HasColumnName("transactionStatusId");
            entity.Property(e => e.TransactionTypeId).HasColumnName("transactionTypeId");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.TransactionStatus).WithMany(p => p.MemberSubscriptionTransactions)
                .HasForeignKey(d => d.TransactionStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MemberSub__trans__5BE2A6F2");

            entity.HasOne(d => d.TransactionType).WithMany(p => p.MemberSubscriptionTransactions)
                .HasForeignKey(d => d.TransactionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MemberSub__trans__5AEE82B9");
        });

        modelBuilder.Entity<PodcastSubscriptionTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastS__3213E83F679EC528");

            entity.ToTable("PodcastSubscriptionTransaction", tb => tb.HasTrigger("TR_PodcastSubscriptionTransaction_UpdatedAt"));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.PodcastSubscriptionRegistrationId).HasColumnName("podcastSubscriptionRegistrationId");
            entity.Property(e => e.Profit)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("profit");
            entity.Property(e => e.TransactionStatusId).HasColumnName("transactionStatusId");
            entity.Property(e => e.TransactionTypeId).HasColumnName("transactionTypeId");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.TransactionStatus).WithMany(p => p.PodcastSubscriptionTransactions)
                .HasForeignKey(d => d.TransactionStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSu__trans__5629CD9C");

            entity.HasOne(d => d.TransactionType).WithMany(p => p.PodcastSubscriptionTransactions)
                .HasForeignKey(d => d.TransactionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSu__trans__5535A963");
        });

        modelBuilder.Entity<TransactionStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3213E83F3D3DA149");

            entity.ToTable("TransactionStatus");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TransactionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3213E83FFEF671EE");

            entity.ToTable("TransactionType");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
