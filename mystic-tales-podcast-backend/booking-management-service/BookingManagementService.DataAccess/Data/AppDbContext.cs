using System;
using System.Collections.Generic;
using BookingManagementService.DataAccess.Entities.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace BookingManagementService.DataAccess.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingChatMember> BookingChatMembers { get; set; }

    public virtual DbSet<BookingChatMessage> BookingChatMessages { get; set; }

    public virtual DbSet<BookingChatRoom> BookingChatRooms { get; set; }

    public virtual DbSet<BookingOptionalManualCancelReason> BookingOptionalManualCancelReasons { get; set; }

    public virtual DbSet<BookingPodcastTrack> BookingPodcastTracks { get; set; }

    public virtual DbSet<BookingProducingRequest> BookingProducingRequests { get; set; }

    public virtual DbSet<BookingProducingRequestPodcastTrackToEdit> BookingProducingRequestPodcastTrackToEdits { get; set; }

    public virtual DbSet<BookingRequirement> BookingRequirements { get; set; }

    public virtual DbSet<BookingStatus> BookingStatuses { get; set; }

    public virtual DbSet<BookingStatusTracking> BookingStatusTrackings { get; set; }

    public virtual DbSet<PodcastBookingTone> PodcastBookingTones { get; set; }

    public virtual DbSet<PodcastBookingToneCategory> PodcastBookingToneCategories { get; set; }

    public virtual DbSet<PodcastBuddyBookingTone> PodcastBuddyBookingTones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Booking__3213E83F3C4DBC0D");

            entity.ToTable("Booking", tb => tb.HasTrigger("TR_Booking_UpdatedAt"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.AssignedStaffId).HasColumnName("assignedStaffId");
            entity.Property(e => e.BookingAutoCancelReason).HasColumnName("bookingAutoCancelReason");
            entity.Property(e => e.BookingManualCancelledReason).HasColumnName("bookingManualCancelledReason");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.CustomerBookingCancelDepositRefundRate)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("customerBookingCancelDepositRefundRate");
            entity.Property(e => e.Deadline).HasColumnName("deadline");
            entity.Property(e => e.DeadlineDays).HasColumnName("deadlineDays");
            entity.Property(e => e.DemoAudioFileKey).HasColumnName("demoAudioFileKey");
            entity.Property(e => e.Description)
                .HasDefaultValue("")
                .HasColumnName("description");
            entity.Property(e => e.PodcastBuddyBookingCancelDepositRefundRate)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("podcastBuddyBookingCancelDepositRefundRate");
            entity.Property(e => e.PodcastBuddyId).HasColumnName("podcastBuddyId");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Title)
                .HasMaxLength(250)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("updatedAt");
        });

        modelBuilder.Entity<BookingChatMember>(entity =>
        {
            entity.HasKey(e => new { e.ChatRoomId, e.AccountId }).HasName("PK__BookingC__347EC6C34424824E");

            entity.ToTable("BookingChatMember");

            entity.Property(e => e.ChatRoomId).HasColumnName("chatRoomId");
            entity.Property(e => e.AccountId).HasColumnName("accountId");

            entity.HasOne(d => d.ChatRoom).WithMany(p => p.BookingChatMembers)
                .HasForeignKey(d => d.ChatRoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingCh__chatR__778AC167");
        });

        modelBuilder.Entity<BookingChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingC__3213E83FB506977F");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AudioFileKey).HasColumnName("audioFileKey");
            entity.Property(e => e.ChatRoomId).HasColumnName("chatRoomId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.SenderId).HasColumnName("senderId");
            entity.Property(e => e.Text).HasColumnName("text");

            entity.HasOne(d => d.ChatRoom).WithMany(p => p.BookingChatMessages)
                .HasForeignKey(d => d.ChatRoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingCh__chatR__74AE54BC");
        });

        modelBuilder.Entity<BookingChatRoom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingC__3213E83FB174F737");

            entity.ToTable("BookingChatRoom");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingChatRooms)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingCh__booki__70DDC3D8");
        });

        modelBuilder.Entity<BookingOptionalManualCancelReason>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingO__3213E83F779B281A");

            entity.ToTable("BookingOptionalManualCancelReason");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .HasColumnName("name");
        });

        modelBuilder.Entity<BookingPodcastTrack>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingP__3213E83F338988F1");

            entity.ToTable("BookingPodcastTrack");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.AudioEncryptionKeyFileKey)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("audioEncryptionKeyFileKey");
            entity.Property(e => e.AudioEncryptionKeyId)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("audioEncryptionKeyId");
            entity.Property(e => e.AudioFileKey).HasColumnName("audioFileKey");
            entity.Property(e => e.AudioFileSize).HasColumnName("audioFileSize");
            entity.Property(e => e.AudioLength).HasColumnName("audioLength");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.BookingProducingRequestId).HasColumnName("bookingProducingRequestId");
            entity.Property(e => e.BookingRequirementId).HasColumnName("bookingRequirementId");
            entity.Property(e => e.RemainingPreviewListenSlot).HasColumnName("remainingPreviewListenSlot");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingPodcastTracks)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingPo__booki__68487DD7");

            entity.HasOne(d => d.BookingProducingRequest).WithMany(p => p.BookingPodcastTracks)
                .HasForeignKey(d => d.BookingProducingRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingPo__booki__693CA210");

            entity.HasOne(d => d.BookingRequirement).WithMany(p => p.BookingPodcastTracks)
                .HasForeignKey(d => d.BookingRequirementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingPo__booki__3864608B");
        });

        modelBuilder.Entity<BookingProducingRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingP__3213E83F1C41137B");

            entity.ToTable("BookingProducingRequest");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Deadline)
                .HasColumnType("datetime")
                .HasColumnName("deadline");
            entity.Property(e => e.DeadlineDays).HasColumnName("deadlineDays");
            entity.Property(e => e.FinishedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("finishedAt");
            entity.Property(e => e.IsAccepted).HasColumnName("isAccepted");
            entity.Property(e => e.Note)
                .HasDefaultValue("")
                .HasColumnName("note");
            entity.Property(e => e.RejectReason)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("rejectReason");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingProducingRequests)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingPr__booki__656C112C");
        });

        modelBuilder.Entity<BookingProducingRequestPodcastTrackToEdit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingP__3213E83FFC47A118");

            entity.ToTable("BookingProducingRequestPodcastTrackToEdit");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.BookingPodcastTrackId).HasColumnName("bookingPodcastTrackId");
            entity.Property(e => e.BookingProducingRequestId).HasColumnName("bookingProducingRequestId");

            entity.HasOne(d => d.BookingPodcastTrack).WithMany(p => p.BookingProducingRequestPodcastTrackToEdits)
                .HasForeignKey(d => d.BookingPodcastTrackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingPr__booki__6D0D32F4");

            entity.HasOne(d => d.BookingProducingRequest).WithMany(p => p.BookingProducingRequestPodcastTrackToEdits)
                .HasForeignKey(d => d.BookingProducingRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingPr__booki__6C190EBB");
        });

        modelBuilder.Entity<BookingRequirement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingR__3213E83FFEFFA280");

            entity.ToTable("BookingRequirement");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.Description)
                .HasDefaultValue("")
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasDefaultValue("")
                .HasColumnName("name");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.PodcastBookingToneId).HasColumnName("podcastBookingToneId");
            entity.Property(e => e.RequirementDocumentFileKey).HasColumnName("requirementDocumentFileKey");
            entity.Property(e => e.WordCount).HasColumnName("wordCount");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingRequirements)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingRe__booki__3493CFA7");

            entity.HasOne(d => d.PodcastBookingTone).WithMany(p => p.BookingRequirements)
                .HasForeignKey(d => d.PodcastBookingToneId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingRe__podca__3587F3E0");
        });

        modelBuilder.Entity<BookingStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingS__3213E83FD852AACA");

            entity.ToTable("BookingStatus");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<BookingStatusTracking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingS__3213E83F94A470A9");

            entity.ToTable("BookingStatusTracking");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.BookingStatusId).HasColumnName("bookingStatusId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingStatusTrackings)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingSt__booki__5EBF139D");

            entity.HasOne(d => d.BookingStatus).WithMany(p => p.BookingStatusTrackings)
                .HasForeignKey(d => d.BookingStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingSt__booki__5FB337D6");
        });

        modelBuilder.Entity<PodcastBookingTone>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastB__3213E83F74170A1C");

            entity.ToTable("PodcastBookingTone");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.DeletedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("deletedAt");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PodcastBookingToneCategoryId).HasColumnName("podcastBookingToneCategoryId");

            entity.HasOne(d => d.PodcastBookingToneCategory).WithMany(p => p.PodcastBookingTones)
                .HasForeignKey(d => d.PodcastBookingToneCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastBo__podca__2B0A656D");
        });

        modelBuilder.Entity<PodcastBookingToneCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PodcastB__3213E83F8B8CD0C3");

            entity.ToTable("PodcastBookingToneCategory");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<PodcastBuddyBookingTone>(entity =>
        {
            entity.HasKey(e => new { e.PodcasterId, e.PodcastBookingToneId }).HasName("PK__PodcastB__04D9E9B7A241A40A");

            entity.ToTable("PodcastBuddyBookingTone");

            entity.Property(e => e.PodcasterId).HasColumnName("podcasterId");
            entity.Property(e => e.PodcastBookingToneId).HasColumnName("podcastBookingToneId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");

            entity.HasOne(d => d.PodcastBookingTone).WithMany(p => p.PodcastBuddyBookingTones)
                .HasForeignKey(d => d.PodcastBookingToneId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PodcastBu__podca__2EDAF651");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
