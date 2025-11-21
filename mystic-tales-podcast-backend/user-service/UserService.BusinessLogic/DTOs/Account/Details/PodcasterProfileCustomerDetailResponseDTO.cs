using UserService.BusinessLogic.DTOs.Account.ListItems;

namespace UserService.BusinessLogic.DTOs.Account.Details
{
    public class PodcasterProfileCustomerDetailResponseDTO
    {
        public int AccountId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public int TotalFollow { get; set; }
        public int ListenCount { get; set; }
        public bool? IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? MainImageFileKey { get; set; }
        public bool IsBuddy { get; set; }
        public bool IsFollowedByCurrentUser { get; set; }
        public List<ReviewListItemDTO> ReviewList { get; set; } = new List<ReviewListItemDTO>();
    }
}