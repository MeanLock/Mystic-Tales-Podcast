using Microsoft.AspNetCore.Http;

namespace UserService.BusinessLogic.DTOs.Account
{
    public class PodcasterProfileUpdateRequestDTO
    {
        public required string PodcasterProfileUpdateInfo { get; set; }
        public IFormFile? BuddyAudioFile { get; set; }
    }

    public class PodcasterProfileUpdateInfoDTO
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal? PricePerBookingWord { get; set; }
    }
}