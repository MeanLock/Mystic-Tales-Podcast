namespace UserService.BusinessLogic.DTOs.Auth
{
    public class PasswordUpdateRequestDTO
    {
        public required PasswordUpdateInfoDTO PasswordUpdateInfo { get; set; }
    }
    public class PasswordUpdateInfoDTO
    {
        public required string OldPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}
