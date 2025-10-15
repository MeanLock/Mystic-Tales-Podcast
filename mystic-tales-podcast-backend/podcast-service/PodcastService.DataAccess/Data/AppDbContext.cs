using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PodcastService.DataAccess.Entities.SqlServer;

namespace PodcastService.DataAccess.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Hashtag> Hashtags { get; set; }

    public virtual DbSet<PodcastCategory> PodcastCategories { get; set; }

    public virtual DbSet<PodcastChannel> PodcastChannels { get; set; }

    public virtual DbSet<PodcastChannelHashtag> PodcastChannelHashtags { get; set; }

    public virtual DbSet<PodcastChannelStatus> PodcastChannelStatuses { get; set; }

    public virtual DbSet<PodcastChannelStatusTracking> PodcastChannelStatusTrackings { get; set; }

    public virtual DbSet<PodcastEpisode> PodcastEpisodes { get; set; }

    public virtual DbSet<PodcastEpisodeHashtag> PodcastEpisodeHashtags { get; set; }

    public virtual DbSet<PodcastEpisodeIllegalContentTypeMarking> PodcastEpisodeIllegalContentTypeMarkings { get; set; }

    public virtual DbSet<PodcastEpisodeLicense> PodcastEpisodeLicenses { get; set; }

    public virtual DbSet<PodcastEpisodeLicenseType> PodcastEpisodeLicenseTypes { get; set; }

    public virtual DbSet<PodcastEpisodePublishReviewSession> PodcastEpisodePublishReviewSessions { get; set; }

    public virtual DbSet<PodcastEpisodePublishReviewSessionStatus> PodcastEpisodePublishReviewSessionStatuses { get; set; }

    public virtual DbSet<PodcastEpisodePublishReviewSessionStatusTracking> PodcastEpisodePublishReviewSessionStatusTrackings { get; set; }

    public virtual DbSet<PodcastEpisodeStatus> PodcastEpisodeStatuses { get; set; }

    public virtual DbSet<PodcastEpisodeStatusTracking> PodcastEpisodeStatusTrackings { get; set; }

    public virtual DbSet<PodcastEpisodeSubscriptionType> PodcastEpisodeSubscriptionTypes { get; set; }

    public virtual DbSet<PodcastIllegalContentType> PodcastIllegalContentTypes { get; set; }

    public virtual DbSet<PodcastShow> PodcastShows { get; set; }

    public virtual DbSet<PodcastShowHashtag> PodcastShowHashtags { get; set; }

    public virtual DbSet<PodcastShowReview> PodcastShowReviews { get; set; }

    public virtual DbSet<PodcastShowStatus> PodcastShowStatuses { get; set; }

    public virtual DbSet<PodcastShowStatusTracking> PodcastShowStatusTrackings { get; set; }

    public virtual DbSet<PodcastShowsSubscriptionType> PodcastShowsSubscriptionTypes { get; set; }

    public virtual DbSet<PodcastSubCategory> PodcastSubCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hashtag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Hashtag__3213E83FE4B51E39");

            entity.ToTable("Hashtag");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PodcastCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastC__3213E83F38A97BE5");

            entity.ToTable("PodcastCategory");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PodcastChannel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastC__3213E83F0AAD7CF8");

            entity.ToTable("PodcastChannel", tb => tb.HasTrigger("TR_PodcastChannel_UpdatedAt"));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.BackgroundImageFileKey).HasColumnName("backgroundImageFileKey");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("deletedAt");
            entity.Property(e => e.Description)
                .HasDefaultValue("")
                .HasColumnName("description");
            entity.Property(e => e.ListenCount).HasColumnName("listenCount");
            entity.Property(e => e.MainImageFileKey).HasColumnName("mainImageFileKey");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
            entity.Property(e => e.PodcastCategoryId).HasColumnName("podcastCategoryId");
            entity.Property(e => e.PodcastSubCategoryId).HasColumnName("podcastSubCategoryId");
            entity.Property(e => e.PodcasterId).HasColumnName("podcasterId");
            entity.Property(e => e.TotalFavorite).HasColumnName("totalFavorite");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.PodcastCategory).WithMany(p => p.PodcastChannels)
                .HasForeignKey(d => d.PodcastCategoryId)
                .HasConstraintName("FK__PodcastCh__podca__6383C8BA");

            entity.HasOne(d => d.PodcastSubCategory).WithMany(p => p.PodcastChannels)
                .HasForeignKey(d => d.PodcastSubCategoryId)
                .HasConstraintName("FK__PodcastCh__podca__6477ECF3");
        });

        modelBuilder.Entity<PodcastChannelHashtag>(entity =>
        {
            entity.HasKey(e => new { e.PodcastChannelId, e.HashtagId }).HasName("PK__PodcastC__166218237826B4F5");

            entity.ToTable("PodcastChannelHashtag");

            entity.Property(e => e.PodcastChannelId).HasColumnName("podcastChannelId");
            entity.Property(e => e.HashtagId).HasColumnName("hashtagId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");

            entity.HasOne(d => d.Hashtag).WithMany(p => p.PodcastChannelHashtags)
                .HasForeignKey(d => d.HashtagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastCh__hasht__2DE6D218");

            entity.HasOne(d => d.PodcastChannel).WithMany(p => p.PodcastChannelHashtags)
                .HasForeignKey(d => d.PodcastChannelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastCh__podca__2CF2ADDF");
        });

        modelBuilder.Entity<PodcastChannelStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastC__3213E83F8A7F931C");

            entity.ToTable("PodcastChannelStatus");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PodcastChannelStatusTracking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastC__3213E83FB83D4FAB");

            entity.ToTable("PodcastChannelStatusTracking");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.PodcastChannelId).HasColumnName("podcastChannelId");
            entity.Property(e => e.PodcastChannelStatusId).HasColumnName("podcastChannelStatusId");

            entity.HasOne(d => d.PodcastChannel).WithMany(p => p.PodcastChannelStatusTrackings)
                .HasForeignKey(d => d.PodcastChannelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastCh__podca__17F790F9");

            entity.HasOne(d => d.PodcastChannelStatus).WithMany(p => p.PodcastChannelStatusTrackings)
                .HasForeignKey(d => d.PodcastChannelStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastCh__podca__18EBB532");
        });

        modelBuilder.Entity<PodcastEpisode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastE__3213E83F03FE8EE8");

            entity.ToTable("PodcastEpisode", tb => tb.HasTrigger("TR_PodcastEpisode_UpdatedAt"));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AudioFileKey).HasColumnName("audioFileKey");
            entity.Property(e => e.AudioFileSize).HasColumnName("audioFileSize");
            entity.Property(e => e.AudioFingerPrint)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("audioFingerPrint");
            entity.Property(e => e.AudioLength).HasColumnName("audioLength");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("deletedAt");
            entity.Property(e => e.Description)
                .HasDefaultValue("")
                .HasColumnName("description");
            entity.Property(e => e.EpisodeOrder)
                .HasDefaultValue(1)
                .HasColumnName("episodeOrder");
            entity.Property(e => e.ExplicitContent).HasColumnName("explicitContent");
            entity.Property(e => e.IsAudioPublishable).HasColumnName("isAudioPublishable");
            entity.Property(e => e.IsReleased).HasColumnName("isReleased");
            entity.Property(e => e.ListenCount).HasColumnName("listenCount");
            entity.Property(e => e.MainImageFileKey).HasColumnName("mainImageFileKey");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
            entity.Property(e => e.PodcastEpisodeSubscriptionTypeId)
                .HasDefaultValue(1)
                .HasColumnName("podcastEpisodeSubscriptionTypeId");
            entity.Property(e => e.PodcastShowId).HasColumnName("podcastShowId");
            entity.Property(e => e.ReleaseDate).HasColumnName("releaseDate");
            entity.Property(e => e.SeasonNumber).HasColumnName("seasonNumber");
            entity.Property(e => e.TakenDownReason).HasColumnName("takenDownReason");
            entity.Property(e => e.TotalSave).HasColumnName("totalSave");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.PodcastEpisodeSubscriptionType).WithMany(p => p.PodcastEpisodes)
                .HasForeignKey(d => d.PodcastEpisodeSubscriptionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastEp__podca__7F2BE32F");

            entity.HasOne(d => d.PodcastShow).WithMany(p => p.PodcastEpisodes)
                .HasForeignKey(d => d.PodcastShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastEp__podca__00200768");
        });

        modelBuilder.Entity<PodcastEpisodeHashtag>(entity =>
        {
            entity.HasKey(e => new { e.PodcastEpisodeId, e.HashtagId }).HasName("PK__PodcastE__5D89B8B2AA76DEF0");

            entity.ToTable("PodcastEpisodeHashtag");

            entity.Property(e => e.PodcastEpisodeId).HasColumnName("podcastEpisodeId");
            entity.Property(e => e.HashtagId).HasColumnName("hashtagId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");

            entity.HasOne(d => d.Hashtag).WithMany(p => p.PodcastEpisodeHashtags)
                .HasForeignKey(d => d.HashtagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastEp__hasht__3587F3E0");

            entity.HasOne(d => d.PodcastEpisode).WithMany(p => p.PodcastEpisodeHashtags)
                .HasForeignKey(d => d.PodcastEpisodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastEp__podca__3493CFA7");
        });

        modelBuilder.Entity<PodcastEpisodeIllegalContentTypeMarking>(entity =>
        {
            entity.HasKey(e => new { e.PodcastEpisodeId, e.PodcastIllegalContentTypeId }).HasName("PK__PodcastE__355A44AE7101C8F7");

            entity.ToTable("PodcastEpisodeIllegalContentTypeMarking");

            entity.Property(e => e.PodcastEpisodeId).HasColumnName("podcastEpisodeId");
            entity.Property(e => e.PodcastIllegalContentTypeId).HasColumnName("podcastIllegalContentTypeId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.MarkerId).HasColumnName("markerId");

            entity.HasOne(d => d.PodcastEpisode).WithMany(p => p.PodcastEpisodeIllegalContentTypeMarkings)
                .HasForeignKey(d => d.PodcastEpisodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastEp__podca__08B54D69");

            entity.HasOne(d => d.PodcastIllegalContentType).WithMany(p => p.PodcastEpisodeIllegalContentTypeMarkings)
                .HasForeignKey(d => d.PodcastIllegalContentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastEp__podca__09A971A2");
        });

        modelBuilder.Entity<PodcastEpisodeLicense>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastE__3213E83FC1873768");

            entity.ToTable("PodcastEpisodeLicense");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.LicenseDocumentFileKey).HasColumnName("licenseDocumentFileKey");
            entity.Property(e => e.PodcastEpisodeId).HasColumnName("podcastEpisodeId");
            entity.Property(e => e.PodcastEpisodeLicenseTypeId).HasColumnName("podcastEpisodeLicenseTypeId");

            entity.HasOne(d => d.PodcastEpisode).WithMany(p => p.PodcastEpisodeLicenses)
                .HasForeignKey(d => d.PodcastEpisodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastEp__podca__03F0984C");

            entity.HasOne(d => d.PodcastEpisodeLicenseType).WithMany(p => p.PodcastEpisodeLicenses)
                .HasForeignKey(d => d.PodcastEpisodeLicenseTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastEp__podca__04E4BC85");
        });

        modelBuilder.Entity<PodcastEpisodeLicenseType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastE__3213E83F2941DEB4");

            entity.ToTable("PodcastEpisodeLicenseType");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PodcastEpisodePublishReviewSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastE__3213E83F3D190B53");

            entity.ToTable("PodcastEpisodePublishReviewSession", tb => tb.HasTrigger("TR_PodcastEpisodePublishReviewSession_UpdatedAt"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssignedStaff).HasColumnName("assignedStaff");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Deadline)
                .HasColumnType("datetime")
                .HasColumnName("deadline");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.PodcastEpisodeId).HasColumnName("podcastEpisodeId");
            entity.Property(e => e.ReReviewCount).HasColumnName("reReviewCount");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.PodcastEpisode).WithMany(p => p.PodcastEpisodePublishReviewSessions)
                .HasForeignKey(d => d.PodcastEpisodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastEp__podca__0F624AF8");
        });

        modelBuilder.Entity<PodcastEpisodePublishReviewSessionStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastE__3213E83FEA046330");

            entity.ToTable("PodcastEpisodePublishReviewSessionStatus");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PodcastEpisodePublishReviewSessionStatusTracking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastE__3213E83F50BD1580");

            entity.ToTable("PodcastEpisodePublishReviewSessionStatusTracking");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.PodcastEpisodePublishReviewSessionId).HasColumnName("podcastEpisodePublishReviewSessionId");
            entity.Property(e => e.PodcastEpisodePublishReviewSessionStatusId).HasColumnName("podcastEpisodePublishReviewSessionStatusId");

            entity.HasOne(d => d.PodcastEpisodePublishReviewSession).WithMany(p => p.PodcastEpisodePublishReviewSessionStatusTrackings)
                .HasForeignKey(d => d.PodcastEpisodePublishReviewSessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastEp__podca__1332DBDC");

            entity.HasOne(d => d.PodcastEpisodePublishReviewSessionStatus).WithMany(p => p.PodcastEpisodePublishReviewSessionStatusTrackings)
                .HasForeignKey(d => d.PodcastEpisodePublishReviewSessionStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastEp__podca__14270015");
        });

        modelBuilder.Entity<PodcastEpisodeStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastE__3213E83FD99E30A5");

            entity.ToTable("PodcastEpisodeStatus");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PodcastEpisodeStatusTracking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastE__3213E83FE547BD48");

            entity.ToTable("PodcastEpisodeStatusTracking");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.PodcastEpisodeId).HasColumnName("podcastEpisodeId");
            entity.Property(e => e.PodcastEpisodeStatusId).HasColumnName("podcastEpisodeStatusId");

            entity.HasOne(d => d.PodcastEpisode).WithMany(p => p.PodcastEpisodeStatusTrackings)
                .HasForeignKey(d => d.PodcastEpisodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastEp__podca__2180FB33");

            entity.HasOne(d => d.PodcastEpisodeStatus).WithMany(p => p.PodcastEpisodeStatusTrackings)
                .HasForeignKey(d => d.PodcastEpisodeStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastEp__podca__22751F6C");
        });

        modelBuilder.Entity<PodcastEpisodeSubscriptionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastE__3213E83FD00551AB");

            entity.ToTable("PodcastEpisodeSubscriptionType");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PodcastIllegalContentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastI__3213E83FE1046ABF");

            entity.ToTable("PodcastIllegalContentType");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PodcastShow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastS__3213E83F710474E7");

            entity.ToTable("PodcastShow", tb => tb.HasTrigger("TR_PodcastShow_UpdatedAt"));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AverageRating).HasColumnName("averageRating");
            entity.Property(e => e.Copyright)
                .HasMaxLength(250)
                .HasColumnName("copyright");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("deletedAt");
            entity.Property(e => e.Description)
                .HasDefaultValue("")
                .HasColumnName("description");
            entity.Property(e => e.IsReleased).HasColumnName("isReleased");
            entity.Property(e => e.Language)
                .HasMaxLength(50)
                .HasColumnName("language");
            entity.Property(e => e.ListenCount).HasColumnName("listenCount");
            entity.Property(e => e.MainImageFileKey).HasColumnName("mainImageFileKey");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
            entity.Property(e => e.PodcastCategoryId).HasColumnName("podcastCategoryId");
            entity.Property(e => e.PodcastChannelId).HasColumnName("podcastChannelId");
            entity.Property(e => e.PodcastShowsSubscriptionTypeId)
                .HasDefaultValue(1)
                .HasColumnName("podcastShowsSubscriptionTypeId");
            entity.Property(e => e.PodcastSubCategoryId).HasColumnName("podcastSubCategoryId");
            entity.Property(e => e.PodcasterId).HasColumnName("podcasterId");
            entity.Property(e => e.RatingCount).HasColumnName("ratingCount");
            entity.Property(e => e.ReleaseDate).HasColumnName("releaseDate");
            entity.Property(e => e.TakenDownReason).HasColumnName("takenDownReason");
            entity.Property(e => e.TotalFollow).HasColumnName("totalFollow");
            entity.Property(e => e.TrailerAudioFileKey).HasColumnName("trailerAudioFileKey");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
            entity.Property(e => e.UploadFrequency).HasColumnName("uploadFrequency");

            entity.HasOne(d => d.PodcastCategory).WithMany(p => p.PodcastShows)
                .HasForeignKey(d => d.PodcastCategoryId)
                .HasConstraintName("FK__PodcastSh__podca__6FE99F9F");

            entity.HasOne(d => d.PodcastChannel).WithMany(p => p.PodcastShows)
                .HasForeignKey(d => d.PodcastChannelId)
                .HasConstraintName("FK__PodcastSh__podca__72C60C4A");

            entity.HasOne(d => d.PodcastShowsSubscriptionType).WithMany(p => p.PodcastShows)
                .HasForeignKey(d => d.PodcastShowsSubscriptionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSh__podca__71D1E811");

            entity.HasOne(d => d.PodcastSubCategory).WithMany(p => p.PodcastShows)
                .HasForeignKey(d => d.PodcastSubCategoryId)
                .HasConstraintName("FK__PodcastSh__podca__70DDC3D8");
        });

        modelBuilder.Entity<PodcastShowHashtag>(entity =>
        {
            entity.HasKey(e => new { e.PodcastShowId, e.HashtagId }).HasName("PK__PodcastS__67B6F557AE2EEB71");

            entity.ToTable("PodcastShowHashtag");

            entity.Property(e => e.PodcastShowId).HasColumnName("podcastShowId");
            entity.Property(e => e.HashtagId).HasColumnName("hashtagId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");

            entity.HasOne(d => d.Hashtag).WithMany(p => p.PodcastShowHashtags)
                .HasForeignKey(d => d.HashtagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSh__hasht__31B762FC");

            entity.HasOne(d => d.PodcastShow).WithMany(p => p.PodcastShowHashtags)
                .HasForeignKey(d => d.PodcastShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSh__podca__30C33EC3");
        });

        modelBuilder.Entity<PodcastShowReview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastS__3213E83FCFC1C553");

            entity.ToTable("PodcastShowReview", tb => tb.HasTrigger("TR_PodcastShowReview_UpdatedAt"));

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
            entity.Property(e => e.PodcastShowId).HasColumnName("podcastShowId");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Title)
                .HasMaxLength(250)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.PodcastShow).WithMany(p => p.PodcastShowReviews)
                .HasForeignKey(d => d.PodcastShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSh__podca__282DF8C2");
        });

        modelBuilder.Entity<PodcastShowStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastS__3213E83F136DC555");

            entity.ToTable("PodcastShowStatus");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PodcastShowStatusTracking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastS__3213E83F6D02E683");

            entity.ToTable("PodcastShowStatusTracking");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.PodcastShowId).HasColumnName("podcastShowId");
            entity.Property(e => e.PodcastShowStatusId).HasColumnName("podcastShowStatusId");

            entity.HasOne(d => d.PodcastShow).WithMany(p => p.PodcastShowStatusTrackings)
                .HasForeignKey(d => d.PodcastShowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSh__podca__1CBC4616");

            entity.HasOne(d => d.PodcastShowStatus).WithMany(p => p.PodcastShowStatusTrackings)
                .HasForeignKey(d => d.PodcastShowStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSh__podca__1DB06A4F");
        });

        modelBuilder.Entity<PodcastShowsSubscriptionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastS__3213E83F594D8BE4");

            entity.ToTable("PodcastShowsSubscriptionType");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PodcastSubCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastS__3213E83FC3874177");

            entity.ToTable("PodcastSubCategory");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.PodcastCategoryId).HasColumnName("podcastCategoryId");

            entity.HasOne(d => d.PodcastCategory).WithMany(p => p.PodcastSubCategories)
                .HasForeignKey(d => d.PodcastCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastSu__podca__4BAC3F29");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
