namespace UserService.BusinessLogic.DTOs.Account.ListItems
{
    public class PodcasterProfileCustomerListItemResponseDTO
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
    }
}