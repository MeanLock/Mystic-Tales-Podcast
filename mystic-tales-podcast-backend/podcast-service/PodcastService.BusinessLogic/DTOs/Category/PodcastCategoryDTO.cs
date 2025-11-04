using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Category
{
    public class PodcastCategoryDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
    }
}