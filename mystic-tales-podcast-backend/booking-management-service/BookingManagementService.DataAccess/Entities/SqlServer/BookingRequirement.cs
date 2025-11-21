using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.SqlServer;

public partial class BookingRequirement
{
    public Guid Id { get; set; }

    public int BookingId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string RequirementDocumentFileKey { get; set; }

    public int Order { get; set; }

    public int WordCount { get; set; }

    public Guid PodcastBookingToneId { get; set; }

    public virtual Booking Booking { get; set; }

    public virtual ICollection<BookingPodcastTrack> BookingPodcastTracks { get; set; } = new List<BookingPodcastTrack>();

    public virtual PodcastBookingTone PodcastBookingTone { get; set; }
}
