using System;
using System.Collections.Generic;

namespace BookingManagementService.DataAccess.Entities.SqlServer;

public partial class BookingProducingRequestPodcastTrackToEdit
{
    public Guid Id { get; set; }

    public Guid BookingProducingRequestId { get; set; }

    public Guid BookingPodcastTrackId { get; set; }

    public virtual BookingPodcastTrack BookingPodcastTrack { get; set; } = null!;

    public virtual BookingProducingRequest BookingProducingRequest { get; set; } = null!;
}
