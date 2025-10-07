using System;
using System.Collections.Generic;
using BookingManagementService.DataAccess.Entities.sqlserver;
using Microsoft.EntityFrameworkCore;

namespace BookingManagementService.DataAccess.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingChatMember> BookingChatMembers { get; set; }

    public virtual DbSet<BookingChatMessage> BookingChatMessages { get; set; }

    public virtual DbSet<BookingChatRoom> BookingChatRooms { get; set; }

    public virtual DbSet<BookingNegotiation> BookingNegotiations { get; set; }

    public virtual DbSet<BookingOptionalManualCancelReason> BookingOptionalManualCancelReasons { get; set; }

    public virtual DbSet<BookingPodcastTrack> BookingPodcastTracks { get; set; }

    public virtual DbSet<BookingProducingRequest> BookingProducingRequests { get; set; }

    public virtual DbSet<BookingProducingRequestPodcastTrackToEdit> BookingProducingRequestPodcastTrackToEdits { get; set; }

    public virtual DbSet<BookingRequirementAttachFile> BookingRequirementAttachFiles { get; set; }

    public virtual DbSet<BookingStatus> BookingStatuses { get; set; }

    public virtual DbSet<BookingStatusTracking> BookingStatusTrackings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost,1434;Database=MTP_BookingManagementDb_dev;User Id=sa;Password=Banana100;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Booking__3213E83F3C4DBC0D");

            entity.ToTable("Booking", tb => tb.HasTrigger("TR_Booking_UpdatedAt"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.BookingAutoCancelReason).HasColumnName("bookingAutoCancelReason");
            entity.Property(e => e.BookingManualCancelledReason).HasColumnName("bookingManualCancelledReason");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Deadline).HasColumnName("deadline");
            entity.Property(e => e.DemoAudioFileKey).HasColumnName("demoAudioFileKey");
            entity.Property(e => e.Description)
                .HasDefaultValue("")
                .HasColumnName("description");
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
                .ValueGeneratedNever()
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
                .ValueGeneratedNever()
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

        modelBuilder.Entity<BookingNegotiation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingN__3213E83F5C16071E");

            entity.ToTable("BookingNegotiation");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Deadline).HasColumnName("deadline");
            entity.Property(e => e.DemoAudioFileKey).HasColumnName("demoAudioFileKey");
            entity.Property(e => e.DemoAudioRequired).HasColumnName("demoAudioRequired");
            entity.Property(e => e.IsCompleted).HasColumnName("isCompleted");
            entity.Property(e => e.IsFromCustomer)
                .HasDefaultValue(true)
                .HasColumnName("isFromCustomer");
            entity.Property(e => e.Note)
                .HasDefaultValue("")
                .HasColumnName("note");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingNegotiations)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingNe__booki__5AEE82B9");
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
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AudioFileKey).HasColumnName("audioFileKey");
            entity.Property(e => e.AudioFileSize).HasColumnName("audioFileSize");
            entity.Property(e => e.AudioLength).HasColumnName("audioLength");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.BookingProducingRequestId).HasColumnName("bookingProducingRequestId");
            entity.Property(e => e.RemainingPreviewListenSlot).HasColumnName("remainingPreviewListenSlot");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingPodcastTracks)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingPo__booki__68487DD7");

            entity.HasOne(d => d.BookingProducingRequest).WithMany(p => p.BookingPodcastTracks)
                .HasForeignKey(d => d.BookingProducingRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingPo__booki__693CA210");
        });

        modelBuilder.Entity<BookingProducingRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingP__3213E83F1C41137B");

            entity.ToTable("BookingProducingRequest");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(CONVERT([datetime],(sysdatetimeoffset() AT TIME ZONE 'N. Central Asia Standard Time')))")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Deadline).HasColumnName("deadline");
            entity.Property(e => e.FinishedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime")
                .HasColumnName("finishedAt");
            entity.Property(e => e.IsAccepted).HasColumnName("isAccepted");
            entity.Property(e => e.Note)
                .HasDefaultValue("")
                .HasColumnName("note");

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
                .ValueGeneratedNever()
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

        modelBuilder.Entity<BookingRequirementAttachFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingR__3213E83F08C914B5");

            entity.ToTable("BookingRequirementAttachFile");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AttachFileKey).HasColumnName("attachFileKey");
            entity.Property(e => e.BookingId).HasColumnName("bookingId");
            entity.Property(e => e.Description)
                .HasDefaultValue("")
                .HasColumnName("description");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingRequirementAttachFiles)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BookingRe__booki__534D60F1");
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
                .ValueGeneratedNever()
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
