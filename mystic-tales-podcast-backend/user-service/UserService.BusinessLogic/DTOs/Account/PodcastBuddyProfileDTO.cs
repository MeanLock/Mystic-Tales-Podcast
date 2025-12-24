namespace UserService.BusinessLogic.DTOs.Account
{
    public class PodcastBuddyProfileDTO
    {
        public int AccountId { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public double AverageRating { get; set; }

        public int RatingCount { get; set; }
        public int TotalFollow { get; set; }
        public int ListenCount { get; set; }
        public decimal? PricePerBookingWord { get; set; }

        public string CommitmentDocumentFileKey { get; set; } = null!;

        public string? BuddyAudioFileKey { get; set; }

        public bool? IsVerified { get; set; }
        public required bool IsFollowedByCurrentUser { get; set; }
    }
}