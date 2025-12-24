using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Category.ListItems
{
    public class PodcastSubCategoryListItemResponseDTO : PodcastSubCategoryDTO
    {
        public PodcastCategoryDTO PodcastCategory { get; set; } = null!;
    }
}