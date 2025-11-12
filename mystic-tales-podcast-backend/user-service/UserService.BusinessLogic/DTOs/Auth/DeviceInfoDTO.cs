using Microsoft.AspNetCore.Http;

namespace UserService.BusinessLogic.DTOs.Auth
{
    public class DeviceInfoDTO
    {
        public string DeviceId { get; set; }
        public string Platform { get; set; }
        public string OSName { get; set; } 
    }
}