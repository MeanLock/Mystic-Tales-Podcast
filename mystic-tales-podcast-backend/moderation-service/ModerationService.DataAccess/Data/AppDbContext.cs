using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ModerationService.DataAccess.Entities.SqlServer;

namespace ModerationService.DataAccess.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CounterNotice> CounterNotices { get; set; }

    public virtual DbSet<CounterNoticeAttachFile> CounterNoticeAttachFiles { get; set; }

    public virtual DbSet<Dmcaaccusation> Dmcaaccusations { get; set; }

    public virtual DbSet<DmcaaccusationConclusionReport> DmcaaccusationConclusionReports { get; set; }

    public virtual DbSet<DmcaaccusationConclusionReportType> DmcaaccusationConclusionReportTypes { get; set; }

    public virtual DbSet<DmcaaccusationStatus> DmcaaccusationStatuses { get; set; }

    public virtual DbSet<DmcaaccusationStatusTracking> DmcaaccusationStatusTrackings { get; set; }

    public virtual DbSet<Dmcanotice> Dmcanotices { get; set; }

    public virtual DbSet<DmcanoticeAttachFile> DmcanoticeAttachFiles { get; set; }

    public virtual DbSet<LawsuitProof> LawsuitProofs { get; set; }

    public virtual DbSet<LawsuitProofAttachFile> LawsuitProofAttachFiles { get; set; }

    public virtual DbSet<PodcastBuddyReport> PodcastBuddyReports { get; set; }

    public virtual DbSet<PodcastBuddyReportReviewSession> PodcastBuddyReportReviewSessions { get; set; }

    public virtual DbSet<PodcastBuddyReportType> PodcastBuddyReportTypes { get; set; }

    public virtual DbSet<PodcastEpisodeReport> PodcastEpisodeReports { get; set; }

    public virtual DbSet<PodcastEpisodeReportReviewSession> PodcastEpisodeReportReviewSessions { get; set; }

    public virtual DbSet<PodcastEpisodeReportType> PodcastEpisodeReportTypes { get; set; }

    public virtual DbSet<PodcastShowReport> PodcastShowReports { get; set; }

    public virtual DbSet<PodcastShowReportReviewSession> PodcastShowReportReviewSessions { get; set; }

    public virtual DbSet<PodcastShowReportType> PodcastShowReportTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CounterNotice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CounterN__3213E83FEEFF0929");

            entity.ToTable("CounterNotice", tb => tb.HasTrigger("TR_CounterNotice_UpdatedAt"));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DmcaAccusationId).HasColumnName("dmcaAccusationId");
            entity.Property(e => e.InvalidReason).HasColumnName("invalidReason");
            entity.Property(e => e.IsValid).HasColumnName("isValid");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
            entity.Property(e => e.ValidatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("validatedAt");
            entity.Property(e => e.ValidatedBy)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("validatedBy");

            entity.HasOne(d => d.DmcaAccusation).WithMany(p => p.CounterNotices)
                .HasForeignKey(d => d.DmcaAccusationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CounterNo__dmcaA__74AE54BC");
        });

        modelBuilder.Entity<CounterNoticeAttachFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CounterN__3213E83F0B1205A5");

            entity.ToTable("CounterNoticeAttachFile");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AttachFileKey).HasColumnName("attachFileKey");
            entity.Property(e => e.CounterNoticeId).HasColumnName("counterNoticeId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");

            entity.HasOne(d => d.CounterNotice).WithMany(p => p.CounterNoticeAttachFiles)
                .HasForeignKey(d => d.CounterNoticeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CounterNo__count__0E6E26BF");
        });

        modelBuilder.Entity<Dmcaaccusation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DMCAAccu__3213E83FE75D2251");

            entity.ToTable("DMCAAccusation", tb => tb.HasTrigger("TR_DMCAAccusation_UpdatedAt"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccuserEmail)
                .HasMaxLength(254)
                .HasColumnName("accuserEmail");
            entity.Property(e => e.AccuserFullName)
                .HasMaxLength(500)
                .HasColumnName("accuserFullName");
            entity.Property(e => e.AccuserPhone)
                .HasMaxLength(20)
                .HasColumnName("accuserPhone");
            entity.Property(e => e.AssignedStaff).HasColumnName("assignedStaff");
            entity.Property(e => e.CancelledAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("cancelledAt");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DismissReason).HasColumnName("dismissReason");
            entity.Property(e => e.PodcastEpisodeId).HasColumnName("podcastEpisodeId");
            entity.Property(e => e.PodcastShowId).HasColumnName("podcastShowId");
            entity.Property(e => e.ResolvedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("resolvedAt");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<DmcaaccusationConclusionReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DMCAAccu__3213E83F1DB404F7");

            entity.ToTable("DMCAAccusationConclusionReport");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CancelledAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("cancelledAt");
            entity.Property(e => e.CompletedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("completedAt");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DmcaAccusationConclusionReportTypeId).HasColumnName("dmcaAccusationConclusionReportTypeId");
            entity.Property(e => e.DmcaAccusationId).HasColumnName("dmcaAccusationId");
            entity.Property(e => e.InvalidReason).HasColumnName("invalidReason");
            entity.Property(e => e.IsRejected)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("isRejected");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.DmcaAccusationConclusionReportType).WithMany(p => p.DmcaaccusationConclusionReports)
                .HasForeignKey(d => d.DmcaAccusationConclusionReportTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DMCAAccus__dmcaA__46B27FE2");

            entity.HasOne(d => d.DmcaAccusation).WithMany(p => p.DmcaaccusationConclusionReports)
                .HasForeignKey(d => d.DmcaAccusationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DMCAAccus__dmcaA__45BE5BA9");
        });

        modelBuilder.Entity<DmcaaccusationConclusionReportType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DMCAAccu__3213E83F9D118DBC");

            entity.ToTable("DMCAAccusationConclusionReportType");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<DmcaaccusationStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DMCAAccu__3213E83F8DA6EBCD");

            entity.ToTable("DMCAAccusationStatus");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<DmcaaccusationStatusTracking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DMCAAccu__3213E83F445EC919");

            entity.ToTable("DMCAAccusationStatusTracking");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DmcaAccusationId).HasColumnName("dmcaAccusationId");
            entity.Property(e => e.DmcaAccusationStatusId).HasColumnName("dmcaAccusationStatusId");

            entity.HasOne(d => d.DmcaAccusation).WithMany(p => p.DmcaaccusationStatusTrackings)
                .HasForeignKey(d => d.DmcaAccusationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DMCAAccus__dmcaA__09A971A2");

            entity.HasOne(d => d.DmcaAccusationStatus).WithMany(p => p.DmcaaccusationStatusTrackings)
                .HasForeignKey(d => d.DmcaAccusationStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DMCAAccus__dmcaA__0A9D95DB");
        });

        modelBuilder.Entity<Dmcanotice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DMCANoti__3213E83F53978A01");

            entity.ToTable("DMCANotice", tb => tb.HasTrigger("TR_DMCANotice_UpdatedAt"));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DmcaAccusationId).HasColumnName("dmcaAccusationId");
            entity.Property(e => e.InvalidReason).HasColumnName("invalidReason");
            entity.Property(e => e.IsValid).HasColumnName("isValid");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
            entity.Property(e => e.ValidatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("validatedAt");
            entity.Property(e => e.ValidatedBy)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("validatedBy");

            entity.HasOne(d => d.DmcaAccusation).WithMany(p => p.Dmcanotices)
                .HasForeignKey(d => d.DmcaAccusationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DMCANotic__dmcaA__7B5B524B");
        });

        modelBuilder.Entity<DmcanoticeAttachFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DMCANoti__3213E83F70E62F7D");

            entity.ToTable("DMCANoticeAttachFile");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AttachFileKey).HasColumnName("attachFileKey");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DmcaNoticeId).HasColumnName("dmcaNoticeId");

            entity.HasOne(d => d.DmcaNotice).WithMany(p => p.DmcanoticeAttachFiles)
                .HasForeignKey(d => d.DmcaNoticeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DMCANotic__dmcaN__123EB7A3");
        });

        modelBuilder.Entity<LawsuitProof>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LawsuitP__3213E83FC469DDA6");

            entity.ToTable("LawsuitProof", tb => tb.HasTrigger("TR_LawsuitProof_UpdatedAt"));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DmcaAccusationId).HasColumnName("dmcaAccusationId");
            entity.Property(e => e.InValidReason).HasColumnName("inValidReason");
            entity.Property(e => e.IsValid).HasColumnName("isValid");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
            entity.Property(e => e.ValidatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("validatedAt");
            entity.Property(e => e.ValidatedBy)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("validatedBy");

            entity.HasOne(d => d.DmcaAccusation).WithMany(p => p.LawsuitProofs)
                .HasForeignKey(d => d.DmcaAccusationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LawsuitPr__dmcaA__05D8E0BE");
        });

        modelBuilder.Entity<LawsuitProofAttachFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LawsuitP__3213E83F2AA4CAF8");

            entity.ToTable("LawsuitProofAttachFile");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AttachFileKey).HasColumnName("attachFileKey");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.LawsuitProofId).HasColumnName("lawsuitProofId");

            entity.HasOne(d => d.LawsuitProof).WithMany(p => p.LawsuitProofAttachFiles)
                .HasForeignKey(d => d.LawsuitProofId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LawsuitPr__lawsu__160F4887");
        });

        modelBuilder.Entity<PodcastBuddyReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastB__3213E83FFAA968EC");

            entity.ToTable("PodcastBuddyReport");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.PodcastBuddyId).HasColumnName("podcastBuddyId");
            entity.Property(e => e.PodcastBuddyReportTypeId).HasColumnName("podcastBuddyReportTypeId");
            entity.Property(e => e.ResolvedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("resolvedAt");

            entity.HasOne(d => d.PodcastBuddyReportType).WithMany(p => p.PodcastBuddyReports)
                .HasForeignKey(d => d.PodcastBuddyReportTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastBu__podca__534D60F1");
        });

        modelBuilder.Entity<PodcastBuddyReportReviewSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastB__3213E83F52E0E079");

            entity.ToTable("PodcastBuddyReportReviewSession", tb => tb.HasTrigger("TR_PodcastBuddyReportReviewSession_UpdatedAt"));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AssignedStaff).HasColumnName("assignedStaff");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.IsResolved).HasColumnName("isResolved");
            entity.Property(e => e.PodcastBuddyId).HasColumnName("podcastBuddyId");
            entity.Property(e => e.ResolvedViolationPoint)
                .HasDefaultValue(1)
                .HasColumnName("resolvedViolationPoint");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<PodcastBuddyReportType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastB__3213E83F8112FC05");

            entity.ToTable("PodcastBuddyReportType");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PodcastEpisodeReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastE__3213E83F4439A767");

            entity.ToTable("PodcastEpisodeReport");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.PodcastEpisodeId).HasColumnName("podcastEpisodeId");
            entity.Property(e => e.PodcastEpisodeReportTypeId).HasColumnName("podcastEpisodeReportTypeId");
            entity.Property(e => e.ResolvedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("resolvedAt");

            entity.HasOne(d => d.PodcastEpisodeReportType).WithMany(p => p.PodcastEpisodeReports)
                .HasForeignKey(d => d.PodcastEpisodeReportTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastEp__podca__5CD6CB2B");
        });

        modelBuilder.Entity<PodcastEpisodeReportReviewSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastE__3213E83F90CA8D99");

            entity.ToTable("PodcastEpisodeReportReviewSession", tb => tb.HasTrigger("TR_PodcastEpisodeReportReviewSession_UpdatedAt"));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AssignedStaff).HasColumnName("assignedStaff");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.IsResolved).HasColumnName("isResolved");
            entity.Property(e => e.PodcastEpisodeId).HasColumnName("podcastEpisodeId");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<PodcastEpisodeReportType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastE__3213E83F0E348874");

            entity.ToTable("PodcastEpisodeReportType");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PodcastShowReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastS__3213E83F3ED81C64");

            entity.ToTable("PodcastShowReport");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.PodcastShowId).HasColumnName("podcastShowId");
            entity.Property(e => e.PodcastShowReportTypeId).HasColumnName("podcastShowReportTypeId");
            entity.Property(e => e.ResolvedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("resolvedAt");

            entity.HasOne(d => d.PodcastShowReportType).WithMany(p => p.PodcastShowReports)
                .HasForeignKey(d => d.PodcastShowReportTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSh__podca__5812160E");
        });

        modelBuilder.Entity<PodcastShowReportReviewSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastS__3213E83FCB68F40C");

            entity.ToTable("PodcastShowReportReviewSession", tb => tb.HasTrigger("TR_PodcastShowReportReviewSession_UpdatedAt"));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AssignedStaff).HasColumnName("assignedStaff");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.IsResolved).HasColumnName("isResolved");
            entity.Property(e => e.PodcastShowId).HasColumnName("podcastShowId");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<PodcastShowReportType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastS__3213E83F17C34367");

            entity.ToTable("PodcastShowReportType");

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
