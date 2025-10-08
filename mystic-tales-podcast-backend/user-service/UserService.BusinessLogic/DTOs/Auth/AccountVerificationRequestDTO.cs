namespace UserService.BusinessLogic.DTOs.Auth
{
    public class AccountVerificationRequestDTO
    {
        public required string AccountVerificationInfo { get; set; }
    }
    public class AccountVerificationInfoDTO
    {
        public required string Email { get; set; }
        public required string VerifyCode { get; set; }
    }

}