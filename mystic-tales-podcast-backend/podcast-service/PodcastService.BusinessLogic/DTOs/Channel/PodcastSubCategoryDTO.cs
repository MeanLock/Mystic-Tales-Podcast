using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Channel
{
    public class PodcastSubCategoryDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int PodcastCategoryId { get; set; }
    }
}