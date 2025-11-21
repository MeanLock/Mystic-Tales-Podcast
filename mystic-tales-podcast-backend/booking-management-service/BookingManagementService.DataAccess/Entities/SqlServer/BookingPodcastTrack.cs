using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.SqlServer;

public partial class BookingPodcastTrack
{
    public Guid Id { get; set; }

    public int BookingId { get; set; }

    public Guid BookingProducingRequestId { get; set; }

    public string AudioFileKey { get; set; } = null!;

    public double AudioFileSize { get; set; }

    public int AudioLength { get; set; }

    public int RemainingPreviewListenSlot { get; set; }

    public Guid BookingRequirementId { get; set; }

    public Guid? AudioEncryptionKeyId { get; set; }

    public string? AudioEncryptionKeyFileKey { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual ICollection<BookingPodcastTrackListenSession> BookingPodcastTrackListenSessions { get; set; } = new List<BookingPodcastTrackListenSession>();

    public virtual BookingProducingRequest BookingProducingRequest { get; set; } = null!;

    public virtual ICollection<BookingProducingRequestPodcastTrackToEdit> BookingProducingRequestPodcastTrackToEdits { get; set; } = new List<BookingProducingRequestPodcastTrackToEdit>();

    public virtual BookingRequirement BookingRequirement { get; set; } = null!;
}
