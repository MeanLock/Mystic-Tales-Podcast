namespace UserService.BusinessLogic.DTOs.Account
{
    public class PodcasterListItemResponseDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public RoleDTO Role { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Dob { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public decimal Balance { get; set; }
        public string? MainImageFileKey { get; set; }
        public bool IsVerified { get; set; }
        public int? PodcastListenSlot { get; set; }
        public int ViolationPoint { get; set; }
        public int ViolationLevel { get; set; }
        public string? LastViolationPointChanged { get; set; }
        public string? LastViolationLevelChanged { get; set; }
        public string? LastPodcastListenSlotChanged { get; set; }
        public string? DeactivatedAt { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public PodcasterProfileDTO PodcasterProfile { get; set; } = null!;
        public List<ReviewListItemDTO> ReviewList { get; set; } = new List<ReviewListItemDTO>();

    }


}