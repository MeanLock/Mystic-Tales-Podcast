namespace UserService.BusinessLogic.DTOs.ViewModels.Mail
{
    public class CustomerRegistrationVerificationMailViewModel
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string VerifyCode { get; set; }
        public string? ExpiredAt { get; set; } 

    }

}