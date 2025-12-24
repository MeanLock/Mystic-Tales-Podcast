namespace UserService.BusinessLogic.DTOs.Account
{
    public class AccountMeResponseDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Dob { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public decimal Balance { get; set; }
        public string? MainImageFileKey { get; set; }
        public int? PodcastListenSlot { get; set; }
        public string? DeactivatedAt { get; set; }
        public bool IsPodcaster { get; set; }
        public bool IsPodcasterApplying { get; set; }
    }


}