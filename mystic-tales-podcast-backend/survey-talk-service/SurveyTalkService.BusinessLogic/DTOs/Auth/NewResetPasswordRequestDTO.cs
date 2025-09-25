namespace SurveyTalkService.BusinessLogic.DTOs.Auth
{
    public class NewResetPasswordRequestDTO
    {
        public required string Email { get; set; }
        public required string NewPassword { get; set; }
        public required string PasswordResetToken { get; set; }
    }
}
