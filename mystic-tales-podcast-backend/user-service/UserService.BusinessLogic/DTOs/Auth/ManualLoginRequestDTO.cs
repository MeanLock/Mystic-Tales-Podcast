using Microsoft.AspNetCore.Http;

namespace UserService.BusinessLogic.DTOs.Auth
{
    public class ManualLoginRequestDTO
    {
        public required ManualLoginInfoDTO ManualLoginInfo { get; set; }
        public DeviceInfoDTO DeviceInfo { get; set; }
    }
    public class ManualLoginInfoDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}