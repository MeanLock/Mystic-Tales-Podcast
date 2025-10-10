using System;
using System.Collections.Generic;

namespace UserService.DataAccess.Entities.SqlServer;

public partial class PodcasterProfile
{
    public int AccountId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public double AverageRating { get; set; }

    public int RatingCount { get; set; }

    public string CommitmentDocumentFileKey { get; set; } = null!;

    public string? BuddyAudioFileKey { get; set; }

    public double OwnedBookingStorageSize { get; set; }

    public double UsedBookingStorageSize { get; set; }

    public bool? IsVerified { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;
}
