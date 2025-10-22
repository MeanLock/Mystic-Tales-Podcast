namespace SurveyTalkService.BusinessLogic.DTOs.Account
{
    public class AccountUpdateDTO
    {
        public string? FullName { get; set; }
        public DateOnly? Dob { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? ImageBase64 { get; set; }
    }
}