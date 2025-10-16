using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Cachegory
{
    public class PodcastCategoryDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
    }
}