using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SubscriptionService.DataAccess.Entities.SqlServer;

namespace SubscriptionService.DataAccess.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MemberSubscription> MemberSubscriptions { get; set; }

    public virtual DbSet<MemberSubscriptionBenefit> MemberSubscriptionBenefits { get; set; }

    public virtual DbSet<MemberSubscriptionBenefitMapping> MemberSubscriptionBenefitMappings { get; set; }

    public virtual DbSet<MemberSubscriptionCycleTypePrice> MemberSubscriptionCycleTypePrices { get; set; }

    public virtual DbSet<MemberSubscriptionRegistration> MemberSubscriptionRegistrations { get; set; }

    public virtual DbSet<PodcastSubscription> PodcastSubscriptions { get; set; }

    public virtual DbSet<PodcastSubscriptionBenefit> PodcastSubscriptionBenefits { get; set; }

    public virtual DbSet<PodcastSubscriptionBenefitMapping> PodcastSubscriptionBenefitMappings { get; set; }

    public virtual DbSet<PodcastSubscriptionCycleTypePrice> PodcastSubscriptionCycleTypePrices { get; set; }

    public virtual DbSet<PodcastSubscriptionRegistration> PodcastSubscriptionRegistrations { get; set; }

    public virtual DbSet<SubscriptionCycleType> SubscriptionCycleTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost,1434;Database=MTP_SubscriptionDb_dev;User Id=sa;Password=Banana100;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MemberSubscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MemberSu__3213E83FE7E9BBB5");

            entity.ToTable("MemberSubscription", tb => tb.HasTrigger("TR_MemberSubscription_UpdatedAt"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.CurrentVersion).HasColumnName("currentVersion");
            entity.Property(e => e.DeletedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("deletedAt");
            entity.Property(e => e.Description)
                .HasDefaultValue("")
                .HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.IsSubscribable).HasColumnName("isSubscribable");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<MemberSubscriptionBenefit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MemberSu__3213E83F75A94C21");

            entity.ToTable("MemberSubscriptionBenefit");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
        });

        modelBuilder.Entity<MemberSubscriptionBenefitMapping>(entity =>
        {
            entity.HasKey(e => new { e.MemberSubscriptionId, e.MemberSubscriptionBenefitId, e.Version }).HasName("PK__MemberSu__565A8F7584FFE5F8");

            entity.ToTable("MemberSubscriptionBenefitMapping", tb => tb.HasTrigger("TR_MemberSubscriptionBenefitMapping_UpdatedAt"));

            entity.Property(e => e.MemberSubscriptionId).HasColumnName("memberSubscriptionId");
            entity.Property(e => e.MemberSubscriptionBenefitId).HasColumnName("memberSubscriptionBenefitId");
            entity.Property(e => e.Version).HasColumnName("version");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.MemberSubscriptionBenefit).WithMany(p => p.MemberSubscriptionBenefitMappings)
                .HasForeignKey(d => d.MemberSubscriptionBenefitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MemberSub__membe__7F2BE32F");

            entity.HasOne(d => d.MemberSubscription).WithMany(p => p.MemberSubscriptionBenefitMappings)
                .HasForeignKey(d => d.MemberSubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MemberSub__membe__7E37BEF6");
        });

        modelBuilder.Entity<MemberSubscriptionCycleTypePrice>(entity =>
        {
            entity.HasKey(e => new { e.MemberSubscriptionId, e.SubscriptionCycleTypeId, e.Version }).HasName("PK__MemberSu__0FCD154CD99968FF");

            entity.ToTable("MemberSubscriptionCycleTypePrice", tb => tb.HasTrigger("TR_MemberSubscriptionCycleTypePrice_UpdatedAt"));

            entity.Property(e => e.MemberSubscriptionId).HasColumnName("memberSubscriptionId");
            entity.Property(e => e.SubscriptionCycleTypeId).HasColumnName("subscriptionCycleTypeId");
            entity.Property(e => e.Version).HasColumnName("version");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.MemberSubscription).WithMany(p => p.MemberSubscriptionCycleTypePrices)
                .HasForeignKey(d => d.MemberSubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MemberSub__membe__787EE5A0");

            entity.HasOne(d => d.SubscriptionCycleType).WithMany(p => p.MemberSubscriptionCycleTypePrices)
                .HasForeignKey(d => d.SubscriptionCycleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MemberSub__subsc__797309D9");
        });

        modelBuilder.Entity<MemberSubscriptionRegistration>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__MemberSu__F267251EF869FEE5");

            entity.ToTable("MemberSubscriptionRegistration", tb => tb.HasTrigger("TR_MemberSubscriptionRegistration_UpdatedAt"));

            entity.Property(e => e.AccountId)
                .ValueGeneratedNever()
                .HasColumnName("accountId");
            entity.Property(e => e.CancelledAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("cancelledAt");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.CurrentVersion).HasColumnName("currentVersion");
            entity.Property(e => e.IsAcceptNewestVersionSwitch).HasColumnName("isAcceptNewestVersionSwitch");
            entity.Property(e => e.LastPaidAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("lastPaidAt");
            entity.Property(e => e.MemberSubscriptionId).HasColumnName("memberSubscriptionId");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.MemberSubscription).WithMany(p => p.MemberSubscriptionRegistrations)
                .HasForeignKey(d => d.MemberSubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MemberSub__membe__05D8E0BE");
        });

        modelBuilder.Entity<PodcastSubscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastS__3213E83F867647BC");

            entity.ToTable("PodcastSubscription", tb => tb.HasTrigger("TR_PodcastSubscription_UpdatedAt"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.CurrentVersion).HasColumnName("currentVersion");
            entity.Property(e => e.DeletedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("deletedAt");
            entity.Property(e => e.Description)
                .HasDefaultValue("")
                .HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
            entity.Property(e => e.PodcastChannelId).HasColumnName("podcastChannelId");
            entity.Property(e => e.PodcastShowId).HasColumnName("podcastShowId");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<PodcastSubscriptionBenefit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastS__3213E83F163F6B42");

            entity.ToTable("PodcastSubscriptionBenefit");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PodcastSubscriptionBenefitMapping>(entity =>
        {
            entity.HasKey(e => new { e.PodcastSubscriptionId, e.PodcastSubscriptionBenefitId, e.Version }).HasName("PK__PodcastS__E507FB98F672EDC2");

            entity.ToTable("PodcastSubscriptionBenefitMapping", tb => tb.HasTrigger("TR_PodcastSubscriptionBenefitMapping_UpdatedAt"));

            entity.Property(e => e.PodcastSubscriptionId).HasColumnName("podcastSubscriptionId");
            entity.Property(e => e.PodcastSubscriptionBenefitId).HasColumnName("podcastSubscriptionBenefitId");
            entity.Property(e => e.Version).HasColumnName("version");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.PodcastSubscriptionBenefit).WithMany(p => p.PodcastSubscriptionBenefitMappings)
                .HasForeignKey(d => d.PodcastSubscriptionBenefitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSu__podca__5EBF139D");

            entity.HasOne(d => d.PodcastSubscription).WithMany(p => p.PodcastSubscriptionBenefitMappings)
                .HasForeignKey(d => d.PodcastSubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSu__podca__5DCAEF64");
        });

        modelBuilder.Entity<PodcastSubscriptionCycleTypePrice>(entity =>
        {
            entity.HasKey(e => new { e.PodcastSubscriptionId, e.SubscriptionCycleTypeId, e.Version }).HasName("PK__PodcastS__A497A63E9E1121C1");

            entity.ToTable("PodcastSubscriptionCycleTypePrice", tb => tb.HasTrigger("TR_PodcastSubscriptionCycleTypePrice_UpdatedAt"));

            entity.Property(e => e.PodcastSubscriptionId).HasColumnName("podcastSubscriptionId");
            entity.Property(e => e.SubscriptionCycleTypeId).HasColumnName("subscriptionCycleTypeId");
            entity.Property(e => e.Version).HasColumnName("version");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.PodcastSubscription).WithMany(p => p.PodcastSubscriptionCycleTypePrices)
                .HasForeignKey(d => d.PodcastSubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSu__podca__5812160E");

            entity.HasOne(d => d.SubscriptionCycleType).WithMany(p => p.PodcastSubscriptionCycleTypePrices)
                .HasForeignKey(d => d.SubscriptionCycleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSu__subsc__59063A47");
        });

        modelBuilder.Entity<PodcastSubscriptionRegistration>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__PodcastS__F267251E0F7C81CC");

            entity.ToTable("PodcastSubscriptionRegistration", tb => tb.HasTrigger("TR_PodcastSubscriptionRegistration_UpdatedAt"));

            entity.Property(e => e.AccountId)
                .ValueGeneratedNever()
                .HasColumnName("accountId");
            entity.Property(e => e.CancelledAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("cancelledAt");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.CurrentVersion).HasColumnName("currentVersion");
            entity.Property(e => e.IsAcceptNewestVersionSwitch)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("isAcceptNewestVersionSwitch");
            entity.Property(e => e.IsIncomeTaken).HasColumnName("isIncomeTaken");
            entity.Property(e => e.LastPaidAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("lastPaidAt");
            entity.Property(e => e.PodcastSubscriptionId).HasColumnName("podcastSubscriptionId");
            entity.Property(e => e.SubscriptionCycleTypeId).HasColumnName("subscriptionCycleTypeId");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.PodcastSubscription).WithMany(p => p.PodcastSubscriptionRegistrations)
                .HasForeignKey(d => d.PodcastSubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSu__podca__6754599E");

            entity.HasOne(d => d.SubscriptionCycleType).WithMany(p => p.PodcastSubscriptionRegistrations)
                .HasForeignKey(d => d.SubscriptionCycleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSu__subsc__68487DD7");

            entity.HasMany(d => d.PodcastSubscriptionBenefits).WithMany(p => p.PodcastSubscriptionRegistrations)
                .UsingEntity<Dictionary<string, object>>(
                    "PodcastSubscriptionRegistrationBenefit",
                    r => r.HasOne<PodcastSubscriptionBenefit>().WithMany()
                        .HasForeignKey("PodcastSubscriptionBenefitId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PodcastSu__podca__6C190EBB"),
                    l => l.HasOne<PodcastSubscriptionRegistration>().WithMany()
                        .HasForeignKey("PodcastSubscriptionRegistrationId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PodcastSu__podca__6B24EA82"),
                    j =>
                    {
                        j.HasKey("PodcastSubscriptionRegistrationId", "PodcastSubscriptionBenefitId").HasName("PK__PodcastS__CCA820513C00438D");
                        j.ToTable("PodcastSubscriptionRegistrationBenefit");
                        j.IndexerProperty<int>("PodcastSubscriptionRegistrationId").HasColumnName("podcastSubscriptionRegistrationId");
                        j.IndexerProperty<int>("PodcastSubscriptionBenefitId").HasColumnName("podcastSubscriptionBenefitId");
                    });
        });

        modelBuilder.Entity<SubscriptionCycleType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Subscrip__3213E83F27AC690D");

            entity.ToTable("SubscriptionCycleType");

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
