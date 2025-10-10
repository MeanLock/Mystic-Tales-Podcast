using Microsoft.AspNetCore.Http;

namespace UserService.BusinessLogic.DTOs.Account
{
    public class PodcasterProfileRequestDTO
    {
        public required string PodcasterProfileRequestInfo { get; set; }
        public IFormFile? CommitmentDocumentFile { get; set; } 
    }

    public class PodcasterProfileRequestInfoDTO
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
    }
}