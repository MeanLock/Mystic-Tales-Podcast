using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.sqlserver;

public partial class BookingProducingRequest
{
    public Guid Id { get; set; }

    public int BookingId { get; set; }

    public string Note { get; set; } = null!;

    public DateOnly Deadline { get; set; }

    public bool? IsAccepted { get; set; }

    public DateTime? FinishedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual ICollection<BookingPodcastTrack> BookingPodcastTracks { get; set; } = new List<BookingPodcastTrack>();

    public virtual ICollection<BookingProducingRequestPodcastTrackToEdit> BookingProducingRequestPodcastTrackToEdits { get; set; } = new List<BookingProducingRequestPodcastTrackToEdit>();
}
