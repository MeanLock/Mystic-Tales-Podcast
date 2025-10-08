namespace UserService.BusinessLogic.DTOs.ViewModels.Mail
{
    public class CustomerRegistrationVerificationMailViewModel
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string VerificationCode { get; set; }
        public required string ExpiredAt { get; set; }

    }

}