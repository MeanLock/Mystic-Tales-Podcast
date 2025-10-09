namespace UserService.BusinessLogic.DTOs.Auth
{
    public class NewResetPasswordRequestDTO
    {
        public required ResetPasswordInfoDTO ResetPasswordInfo { get; set; }
    }
    public class ResetPasswordInfoDTO
    {
        public required string Email { get; set; }
        public required string ResetPasswordToken { get; set; }
        public required string NewPassword { get; set; }
    }
}
