namespace ModerationService.BusinessLogic.Models.Mail
{
    public class CustomerPasswordResetMailViewModel
    {
        public required string Email { get; set; }
        public required string PasswordResetToken { get; set; }
        public required string ResetPasswordUrl { get; set; }
        public required string ExpiredAt { get; set; }

    }

}