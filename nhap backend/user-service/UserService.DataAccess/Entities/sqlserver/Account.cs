using System;
using System.Collections.Generic;

namespace UserService.DataAccess.Entities.sqlserver;

public partial class Account
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int RoleId { get; set; }

    public string FullName { get; set; } = null!;

    public DateOnly? Dob { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public decimal Balance { get; set; }

    public string? MainImageFileKey { get; set; }

    public bool IsVerified { get; set; }

    public string? GoogleId { get; set; }

    public string? VerifyCode { get; set; }

    public int? PodcastListenSlot { get; set; }

    public int ViolationPoint { get; set; }

    public int ViolationLevel { get; set; }

    public DateTime? LastViolationPointChanged { get; set; }

    public DateTime? LastViolationLevelChanged { get; set; }

    public DateTime? LastPodcastListenSlotChanged { get; set; }

    public DateTime? DeactivatedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AccountFavoritedPodcastChannel> AccountFavoritedPodcastChannels { get; set; } = new List<AccountFavoritedPodcastChannel>();

    public virtual ICollection<AccountFollowedPodcastShow> AccountFollowedPodcastShows { get; set; } = new List<AccountFollowedPodcastShow>();

    public virtual ICollection<AccountFollowedPodcaster> AccountFollowedPodcasterAccounts { get; set; } = new List<AccountFollowedPodcaster>();

    public virtual ICollection<AccountFollowedPodcaster> AccountFollowedPodcasterPodcasters { get; set; } = new List<AccountFollowedPodcaster>();

    public virtual ICollection<AccountNotification> AccountNotifications { get; set; } = new List<AccountNotification>();

    public virtual ICollection<AccountPodcastListenHistory> AccountPodcastListenHistories { get; set; } = new List<AccountPodcastListenHistory>();

    public virtual ICollection<AccountSavedPodcastEpisode> AccountSavedPodcastEpisodes { get; set; } = new List<AccountSavedPodcastEpisode>();

    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

    public virtual ICollection<PodcastBuddyReview> PodcastBuddyReviewAccounts { get; set; } = new List<PodcastBuddyReview>();

    public virtual ICollection<PodcastBuddyReview> PodcastBuddyReviewPodcastBuddies { get; set; } = new List<PodcastBuddyReview>();

    public virtual PodcasterProfile? PodcasterProfile { get; set; }

    public virtual Role Role { get; set; } = null!;
}
