using Microsoft.AspNetCore.Http;

namespace UserService.BusinessLogic.DTOs.Account
{
    public class AccountUpdateRequestDTO
    {
        public required string AccountUpdateInfo { get; set; }
        public IFormFile? MainImageFile { get; set; }
    }
    public class AccountUpdateInfoDTO
    {
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public DateTime? Dob { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
    }
}