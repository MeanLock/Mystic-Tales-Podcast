using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Channel
{
    public class PodcastCategoryDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
    }
}