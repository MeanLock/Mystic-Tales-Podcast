namespace UserService.BusinessLogic.DTOs.Account
{
    public class PodcasterProfileDTO
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
    }
}