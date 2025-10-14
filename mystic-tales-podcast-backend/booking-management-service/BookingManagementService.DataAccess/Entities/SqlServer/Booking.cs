using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.SqlServer;

public partial class Booking
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int AccountId { get; set; }

    public int PodcastBuddyId { get; set; }

    public decimal? Price { get; set; }

    public DateOnly? Deadline { get; set; }

    public string? DemoAudioFileKey { get; set; }

    public string? BookingManualCancelledReason { get; set; }

    public string? BookingAutoCancelReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<BookingChatRoom> BookingChatRooms { get; set; } = new List<BookingChatRoom>();

    public virtual ICollection<BookingNegotiation> BookingNegotiations { get; set; } = new List<BookingNegotiation>();

    public virtual ICollection<BookingPodcastTrack> BookingPodcastTracks { get; set; } = new List<BookingPodcastTrack>();

    public virtual ICollection<BookingProducingRequest> BookingProducingRequests { get; set; } = new List<BookingProducingRequest>();

    public virtual ICollection<BookingRequirementAttachFile> BookingRequirementAttachFiles { get; set; } = new List<BookingRequirementAttachFile>();

    public virtual ICollection<BookingStatusTracking> BookingStatusTrackings { get; set; } = new List<BookingStatusTracking>();
}
