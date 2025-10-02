namespace SurveyTalkService.BusinessLogic.DTOs.Auth
{
    public class AccountVerificationDTO
    {
        public required string Email { get; set; }
        public required string VerifyCode { get; set; }
    }

}