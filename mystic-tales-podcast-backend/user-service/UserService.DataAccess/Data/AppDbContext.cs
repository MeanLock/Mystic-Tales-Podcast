using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using UserService.DataAccess.Entities.SqlServer;

namespace UserService.DataAccess.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AccountFavoritedPodcastChannel> AccountFavoritedPodcastChannels { get; set; }

    public virtual DbSet<AccountFollowedPodcastShow> AccountFollowedPodcastShows { get; set; }

    public virtual DbSet<AccountFollowedPodcaster> AccountFollowedPodcasters { get; set; }

    public virtual DbSet<AccountNotification> AccountNotifications { get; set; }

    public virtual DbSet<AccountSavedPodcastEpisode> AccountSavedPodcastEpisodes { get; set; }

    public virtual DbSet<NotificationType> NotificationTypes { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<PodcastBuddyReview> PodcastBuddyReviews { get; set; }

    public virtual DbSet<PodcasterProfile> PodcasterProfiles { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3213E83FA1EEC62E");

            entity.ToTable("Account", tb => tb.HasTrigger("TR_Account_UpdatedAt"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(250)
                .HasColumnName("address");
            entity.Property(e => e.Balance)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("balance");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DeactivatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("deactivatedAt");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.Email)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(250)
                .HasColumnName("fullName");
            entity.Property(e => e.Gender)
                .HasMaxLength(250)
                .HasColumnName("gender");
            entity.Property(e => e.GoogleId)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("googleId");
            entity.Property(e => e.IsVerified).HasColumnName("isVerified");
            entity.Property(e => e.LastPodcastListenSlotChanged)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("lastPodcastListenSlotChanged");
            entity.Property(e => e.LastViolationLevelChanged)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("lastViolationLevelChanged");
            entity.Property(e => e.LastViolationPointChanged)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("lastViolationPointChanged");
            entity.Property(e => e.MainImageFileKey).HasColumnName("mainImageFileKey");
            entity.Property(e => e.Password)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.PodcastListenSlot).HasColumnName("podcastListenSlot");
            entity.Property(e => e.RoleId).HasColumnName("roleId");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
            entity.Property(e => e.VerifyCode)
                .HasMaxLength(250)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("verifyCode");
            entity.Property(e => e.ViolationLevel).HasColumnName("violationLevel");
            entity.Property(e => e.ViolationPoint).HasColumnName("violationPoint");

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Account__roleId__6A30C649");
        });

        modelBuilder.Entity<AccountFavoritedPodcastChannel>(entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.PodcastChannelId }).HasName("PK__AccountF__B1C1078DC1F4A581");

            entity.ToTable("AccountFavoritedPodcastChannel");

            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.PodcastChannelId).HasColumnName("podcastChannelId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountFavoritedPodcastChannels)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccountFa__accou__03F0984C");
        });

        modelBuilder.Entity<AccountFollowedPodcastShow>(entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.PodcastShowId }).HasName("PK__AccountF__F6DC495AB82F56FC");

            entity.ToTable("AccountFollowedPodcastShow");

            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.PodcastShowId).HasColumnName("podcastShowId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountFollowedPodcastShows)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccountFo__accou__07C12930");
        });

        modelBuilder.Entity<AccountFollowedPodcaster>(entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.PodcasterId }).HasName("PK__AccountF__78AA75BC65AA5FDA");

            entity.ToTable("AccountFollowedPodcaster");

            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.PodcasterId).HasColumnName("podcasterId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountFollowedPodcasterAccounts)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccountFo__accou__7F2BE32F");

            entity.HasOne(d => d.Podcaster).WithMany(p => p.AccountFollowedPodcasterPodcasters)
                .HasForeignKey(d => d.PodcasterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccountFo__podca__00200768");
        });

        modelBuilder.Entity<AccountNotification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AccountN__3213E83F70CE85E0");

            entity.ToTable("AccountNotification");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.IsSeen).HasColumnName("isSeen");
            entity.Property(e => e.NotificationTypeId).HasColumnName("notificationTypeId");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountNotifications)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccountNo__accou__17036CC0");

            entity.HasOne(d => d.NotificationType).WithMany(p => p.AccountNotifications)
                .HasForeignKey(d => d.NotificationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccountNotification_NotificationType");
        });

        modelBuilder.Entity<AccountSavedPodcastEpisode>(entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.PodcastEpisodeId }).HasName("PK__AccountS__A57FBD84E38A498A");

            entity.ToTable("AccountSavedPodcastEpisode");

            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.PodcastEpisodeId).HasColumnName("podcastEpisodeId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountSavedPodcastEpisodes)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccountSa__accou__0B91BA14");
        });

        modelBuilder.Entity<NotificationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3213E83F2A5EEBE7");

            entity.ToTable("NotificationType");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Password__3213E83FB75D0ADE");

            entity.ToTable("PasswordResetToken");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.ExpiredAt)
                .HasColumnType("datetime")
                .HasColumnName("expiredAt");
            entity.Property(e => e.IsUsed).HasColumnName("isUsed");
            entity.Property(e => e.Token)
                .HasMaxLength(250)
                .HasColumnName("token");

            entity.HasOne(d => d.Account).WithMany(p => p.PasswordResetTokens)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PasswordR__accou__72C60C4A");
        });

        modelBuilder.Entity<PodcastBuddyReview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastB__3213E83F6FAA8D0A");

            entity.ToTable("PodcastBuddyReview", tb => tb.HasTrigger("TR_PodcastBuddyReview_UpdatedAt"));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("deletedAt");
            entity.Property(e => e.PodcastBuddyId).HasColumnName("podcastBuddyId");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Title)
                .HasMaxLength(250)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Account).WithMany(p => p.PodcastBuddyReviewAccounts)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastBu__accou__114A936A");

            entity.HasOne(d => d.PodcastBuddy).WithMany(p => p.PodcastBuddyReviewPodcastBuddies)
                .HasForeignKey(d => d.PodcastBuddyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastBu__podca__123EB7A3");
        });

        modelBuilder.Entity<PodcasterProfile>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Podcaste__F267251E8D9E38E9");

            entity.ToTable("PodcasterProfile", tb => tb.HasTrigger("TR_PodcasterProfile_UpdatedAt"));

            entity.Property(e => e.AccountId)
                .ValueGeneratedNever()
                .HasColumnName("accountId");
            entity.Property(e => e.AverageRating).HasColumnName("averageRating");
            entity.Property(e => e.BuddyAudioFileKey).HasColumnName("buddyAudioFileKey");
            entity.Property(e => e.CommitmentDocumentFileKey).HasColumnName("commitmentDocumentFileKey");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Description)
                .HasDefaultValue("")
                .HasColumnName("description");
            entity.Property(e => e.IsVerified)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("isVerified");
            entity.Property(e => e.ListenCount).HasColumnName("listenCount");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
            entity.Property(e => e.OwnedBookingStorageSize).HasColumnName("ownedBookingStorageSize");
            entity.Property(e => e.PricePerBookingWord)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("pricePerBookingWord");
            entity.Property(e => e.RatingCount).HasColumnName("ratingCount");
            entity.Property(e => e.TotalFollow).HasColumnName("totalFollow");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
            entity.Property(e => e.UsedBookingStorageSize).HasColumnName("usedBookingStorageSize");
            entity.Property(e => e.VerifiedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("verifiedAt");

            entity.HasOne(d => d.Account).WithOne(p => p.PodcasterProfile)
                .HasForeignKey<PodcasterProfile>(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Podcaster__accou__7B5B524B");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Role__3213E83FA305110E");

            entity.ToTable("Role");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
