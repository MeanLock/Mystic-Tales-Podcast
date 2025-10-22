namespace SurveyTalkService.BusinessLogic.DTOs.Account
{
    public class AccountListItemDTO
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
        public bool IsVerified { get; set; }
        public int Xp { get; set; }
        public int Level { get; set; }
        public int ProgressionSurveyCount { get; set; }
        public bool IsFilterSurveyRequired { get; set; }
        public string? LastFilterSurveyTakenAt { get; set; }
        public string? DeactivatedAt { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string? MainImageUrl { get; set; }
        public bool IsPlatformFeedbackGiven { get; set; }
    }


}