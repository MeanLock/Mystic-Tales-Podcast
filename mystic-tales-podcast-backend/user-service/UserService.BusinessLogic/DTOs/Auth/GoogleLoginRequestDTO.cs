namespace UserService.BusinessLogic.DTOs.Auth
{
    public class GoogleLoginRequestDTO
    {
        public required GoogleAuthDTO GoogleAuth { get; set; }
        public DeviceInfoDTO DeviceInfo { get; set; }
    }

    public class GoogleAuthDTO
    {
        public required string AuthorizationCode { get; set; }
        public required string RedirectUri { get; set; }
    }
}
