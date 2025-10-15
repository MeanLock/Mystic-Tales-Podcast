using Microsoft.AspNetCore.Http;

namespace UserService.BusinessLogic.DTOs.Account
{
    public class PodcasterProfileCreateRequestDTO
    {
        public required string PodcasterProfileCreateInfo { get; set; }
        public required IFormFile CommitmentDocumentFile { get; set; } 
    }

    public class PodcasterProfileCreateInfoDTO
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
    }
}