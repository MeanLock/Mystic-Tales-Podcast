using PodcastService.BusinessLogic.DTOs.Show.ListItems;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Episode;
using PodcastService.BusinessLogic.DTOs.Episode.ListItems;

namespace PodcastService.BusinessLogic.DTOs.Feed;

public class CategoryBasePodcastFeedDTO
{
    public required PodcastCategoryDTO PodcastCategory { get; set; } = new PodcastCategoryDTO();
    public List<ChannelListItemResponseDTO> TopChannels { get; set; } = new List<ChannelListItemResponseDTO>();
    public List<ShowListItemResponseDTO> TopShows { get; set; } = new List<ShowListItemResponseDTO>();
    public List<EpisodeListItemResponseDTO> TopEpisodes { get; set; } = new List<EpisodeListItemResponseDTO>();
    public List<ShowListItemResponseDTO> HotShows { get; set; } = new List<ShowListItemResponseDTO>();
    public List<SubCategoryCategoryBasePodcastFeedSection> SubCategorySections { get; set; } = new List<SubCategoryCategoryBasePodcastFeedSection>();

    public class SubCategoryCategoryBasePodcastFeedSection()
    {
        public PodcastSubCategoryDTO PodcastSubCategory { get; set; } = new PodcastSubCategoryDTO();
        public List<ShowListItemResponseDTO> ShowList { get; set; } = new List<ShowListItemResponseDTO>();
    }
}
