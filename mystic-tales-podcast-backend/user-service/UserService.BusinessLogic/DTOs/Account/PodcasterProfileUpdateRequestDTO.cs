using Microsoft.AspNetCore.Http;

namespace UserService.BusinessLogic.DTOs.Account
{
    public class PodcasterProfileUpdateRequestDTO
    {
        public required string PodcasterProfileRequestInfo { get; set; }
        public IFormFile? BuddyAudioFile { get; set; }
    }
}