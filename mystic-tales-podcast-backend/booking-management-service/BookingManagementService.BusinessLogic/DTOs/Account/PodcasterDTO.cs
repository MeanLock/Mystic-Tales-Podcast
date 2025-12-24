using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Account
{
    public class PodcasterDTO
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
        public PodcasterProfileDTO PodcasterProfile { get; set; } = null!;

    }
    public class PodcasterProfileDTO
    {
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
        public int TotalFollow { get; set; }
        public int ListenCount { get; set; }
        public decimal? PricePerBookingWord { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
