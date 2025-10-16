using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Cachegory
{
    public class PodcastSubCategoryDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int PodcastCategoryId { get; set; }
    }
}