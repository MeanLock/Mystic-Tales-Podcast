using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SystemConfigurationService.DataAccess.Entities.SqlServer;

namespace SystemConfigurationService.DataAccess.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccountConfig> AccountConfigs { get; set; }

    public virtual DbSet<AccountViolationLevelConfig> AccountViolationLevelConfigs { get; set; }

    public virtual DbSet<BookingConfig> BookingConfigs { get; set; }

    public virtual DbSet<PodcastRestrictedTerm> PodcastRestrictedTerms { get; set; }

    public virtual DbSet<PodcastSubscriptionConfig> PodcastSubscriptionConfigs { get; set; }

    public virtual DbSet<PodcastSuggestionConfig> PodcastSuggestionConfigs { get; set; }

    public virtual DbSet<ReviewSessionConfig> ReviewSessionConfigs { get; set; }

    public virtual DbSet<SystemConfigProfile> SystemConfigProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountConfig>(entity =>
        {
            entity.HasKey(e => e.ConfigProfileId).HasName("PK__AccountC__D540A124F04023D1");

            entity.ToTable("AccountConfig", tb => tb.HasTrigger("TR_AccountConfig_UpdatedAt"));

            entity.Property(e => e.ConfigProfileId)
                .ValueGeneratedNever()
                .HasColumnName("configProfileId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.PodcastListenSlotRecoverySeconds).HasColumnName("podcastListenSlotRecoverySeconds");
            entity.Property(e => e.PodcastListenSlotThreshold).HasColumnName("podcastListenSlotThreshold");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
            entity.Property(e => e.ViolationPointDecayHours).HasColumnName("violationPointDecayHours");

            entity.HasOne(d => d.ConfigProfile).WithOne(p => p.AccountConfig)
                .HasForeignKey<AccountConfig>(d => d.ConfigProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccountCo__confi__5FB337D6");
        });

        modelBuilder.Entity<AccountViolationLevelConfig>(entity =>
        {
            entity.HasKey(e => new { e.ConfigProfileId, e.ViolationLevel }).HasName("PK__AccountV__FF0A1D2853FBF691");

            entity.ToTable("AccountViolationLevelConfig", tb => tb.HasTrigger("TR_AccountViolationLevelConfig_UpdatedAt"));

            entity.Property(e => e.ConfigProfileId).HasColumnName("configProfileId");
            entity.Property(e => e.ViolationLevel).HasColumnName("violationLevel");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.PunishmentDays).HasColumnName("punishmentDays");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
            entity.Property(e => e.ViolationPointThreshold).HasColumnName("violationPointThreshold");

            entity.HasOne(d => d.ConfigProfile).WithMany(p => p.AccountViolationLevelConfigs)
                .HasForeignKey(d => d.ConfigProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AccountVi__confi__6477ECF3");
        });

        modelBuilder.Entity<BookingConfig>(entity =>
        {
            entity.HasKey(e => e.ConfigProfileId).HasName("PK__BookingC__D540A1240E105BE4");

            entity.ToTable("BookingConfig", tb => tb.HasTrigger("TR_BookingConfig_UpdatedAt"));

            entity.Property(e => e.ConfigProfileId)
                .ValueGeneratedNever()
                .HasColumnName("configProfileId");
            entity.Property(e => e.ChatRoomExpiredHours).HasColumnName("chatRoomExpiredHours");
            entity.Property(e => e.ChatRoomFileMessageExpiredHours).HasColumnName("chatRoomFileMessageExpiredHours");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DepositRate).HasColumnName("depositRate");
            entity.Property(e => e.FreeInitialBookingStorageSize).HasColumnName("freeInitialBookingStorageSize");
            entity.Property(e => e.PodcastTrackPreviewListenSlot).HasColumnName("podcastTrackPreviewListenSlot");
            entity.Property(e => e.PreviewResponseAllowedDays).HasColumnName("previewResponseAllowedDays");
            entity.Property(e => e.ProducingRequestResponseAllowedDays).HasColumnName("producingRequestResponseAllowedDays");
            entity.Property(e => e.ProfitRate).HasColumnName("profitRate");
            entity.Property(e => e.SingleStorageUnitPurchasePrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("singleStorageUnitPurchasePrice");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.ConfigProfile).WithOne(p => p.BookingConfig)
                .HasForeignKey<BookingConfig>(d => d.ConfigProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingCo__confi__5AEE82B9");
        });

        modelBuilder.Entity<PodcastRestrictedTerm>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastR__3213E83FCC19F137");

            entity.ToTable("PodcastRestrictedTerm");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Term)
                .HasMaxLength(100)
                .HasColumnName("term");
        });

        modelBuilder.Entity<PodcastSubscriptionConfig>(entity =>
        {
            entity.HasKey(e => new { e.ConfigProfileId, e.SubscriptionCycleTypeId }).HasName("PK__PodcastS__7ECB22A81BCB646F");

            entity.ToTable("PodcastSubscriptionConfig", tb => tb.HasTrigger("TR_PodcastSubscriptionConfig_UpdatedAt"));

            entity.Property(e => e.ConfigProfileId).HasColumnName("configProfileId");
            entity.Property(e => e.SubscriptionCycleTypeId).HasColumnName("subscriptionCycleTypeId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.IncomeTakenDelayDays).HasColumnName("incomeTakenDelayDays");
            entity.Property(e => e.ProfitRate).HasColumnName("profitRate");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.ConfigProfile).WithMany(p => p.PodcastSubscriptionConfigs)
                .HasForeignKey(d => d.ConfigProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSu__confi__5165187F");
        });

        modelBuilder.Entity<PodcastSuggestionConfig>(entity =>
        {
            entity.HasKey(e => e.ConfigProfileId).HasName("PK__PodcastS__D540A124D3AD4D32");

            entity.ToTable("PodcastSuggestionConfig", tb => tb.HasTrigger("TR_PodcastSuggestionConfig_UpdatedAt"));

            entity.Property(e => e.ConfigProfileId)
                .ValueGeneratedNever()
                .HasColumnName("configProfileId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.MinChannelQuery).HasColumnName("minChannelQuery");
            entity.Property(e => e.MinLongRangeContentBehaviorLookbackDayCount).HasColumnName("minLongRangeContentBehaviorLookbackDayCount");
            entity.Property(e => e.MinLongRangeUserBehaviorLookbackDayCount).HasColumnName("minLongRangeUserBehaviorLookbackDayCount");
            entity.Property(e => e.MinMediumRangeContentBehaviorLookbackDayCount).HasColumnName("minMediumRangeContentBehaviorLookbackDayCount");
            entity.Property(e => e.MinMediumRangeUserBehaviorLookbackDayCount).HasColumnName("minMediumRangeUserBehaviorLookbackDayCount");
            entity.Property(e => e.MinShortRangeContentBehaviorLookbackDayCount).HasColumnName("minShortRangeContentBehaviorLookbackDayCount");
            entity.Property(e => e.MinShortRangeUserBehaviorLookbackDayCount).HasColumnName("minShortRangeUserBehaviorLookbackDayCount");
            entity.Property(e => e.MinShowQuery).HasColumnName("minShowQuery");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.ConfigProfile).WithOne(p => p.PodcastSuggestionConfig)
                .HasForeignKey<PodcastSuggestionConfig>(d => d.ConfigProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSu__confi__5629CD9C");
        });

        modelBuilder.Entity<ReviewSessionConfig>(entity =>
        {
            entity.HasKey(e => e.ConfigProfileId).HasName("PK__ReviewSe__D540A1242D18E194");

            entity.ToTable("ReviewSessionConfig", tb => tb.HasTrigger("TR_ReviewSessionConfig_UpdatedAt"));

            entity.Property(e => e.ConfigProfileId)
                .ValueGeneratedNever()
                .HasColumnName("configProfileId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.PodcastBuddyUnResolvedReportStreak).HasColumnName("podcastBuddyUnResolvedReportStreak");
            entity.Property(e => e.PodcastEpisodePublishEditRequirementExpiredHours).HasColumnName("podcastEpisodePublishEditRequirementExpiredHours");
            entity.Property(e => e.PodcastEpisodeUnResolvedReportStreak).HasColumnName("podcastEpisodeUnResolvedReportStreak");
            entity.Property(e => e.PodcastShowUnResolvedReportStreak).HasColumnName("podcastShowUnResolvedReportStreak");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.ConfigProfile).WithOne(p => p.ReviewSessionConfig)
                .HasForeignKey<ReviewSessionConfig>(d => d.ConfigProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReviewSes__confi__693CA210");
        });

        modelBuilder.Entity<SystemConfigProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SystemCo__3213E83FF798D966");

            entity.ToTable("SystemConfigProfile", tb => tb.HasTrigger("TR_SystemConfigProfile_UpdatedAt"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("deletedAt");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
