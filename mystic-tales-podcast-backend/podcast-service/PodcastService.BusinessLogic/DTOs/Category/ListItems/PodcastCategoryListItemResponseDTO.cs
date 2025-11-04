using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Category.ListItems
{
    public class PodcastCategoryListItemResponseDTO : PodcastCategoryDTO
    {
        public List<PodcastSubCategoryDTO> PodcastSubCategoryList { get; set; } = new List<PodcastSubCategoryDTO>();
    }
}