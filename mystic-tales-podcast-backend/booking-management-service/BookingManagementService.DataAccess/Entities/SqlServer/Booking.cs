using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.SqlServer;

public partial class Booking
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public int AccountId { get; set; }

    public int PodcastBuddyId { get; set; }

    public decimal? Price { get; set; }

    public DateOnly? Deadline { get; set; }

    public string DemoAudioFileKey { get; set; }

    public string BookingManualCancelledReason { get; set; }

    public string BookingAutoCancelReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int? AssignedStaffId { get; set; }

    public double? CustomerBookingCancelDepositRefundRate { get; set; }

    public double? PodcastBuddyBookingCancelDepositRefundRate { get; set; }

    public int? DeadlineDays { get; set; }

    public virtual ICollection<BookingChatRoom> BookingChatRooms { get; set; } = new List<BookingChatRoom>();

    public virtual ICollection<BookingPodcastTrack> BookingPodcastTracks { get; set; } = new List<BookingPodcastTrack>();

    public virtual ICollection<BookingProducingRequest> BookingProducingRequests { get; set; } = new List<BookingProducingRequest>();

    public virtual ICollection<BookingRequirement> BookingRequirements { get; set; } = new List<BookingRequirement>();

    public virtual ICollection<BookingStatusTracking> BookingStatusTrackings { get; set; } = new List<BookingStatusTracking>();
}
