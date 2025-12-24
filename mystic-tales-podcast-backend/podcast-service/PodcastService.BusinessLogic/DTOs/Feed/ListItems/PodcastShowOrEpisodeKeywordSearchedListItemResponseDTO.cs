using PodcastService.BusinessLogic.DTOs.Show;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Episode;
using PodcastService.BusinessLogic.DTOs.Channel;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Show.ListItems;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.Category.ListItems;

namespace PodcastService.BusinessLogic.DTOs.Feed.ListItems;

public class PodcastShowOrEpisodeKeywordSearchedListItemResponseDTO
{
    public PodcastEpisodeSnippetResponseDTO? Episode { get; set; }
    public PodcastShowSnippetResponseDTO? Show { get; set; }
}
