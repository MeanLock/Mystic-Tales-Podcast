using Microsoft.AspNetCore.Http;

namespace BookingManagementService.BusinessLogic.DTOs.Auth
{
    public class DeviceInfoDTO
    {
        public Guid DeviceId { get; set; }
        public string Platform { get; set; }
        public string OSName { get; set; } 
    }
}