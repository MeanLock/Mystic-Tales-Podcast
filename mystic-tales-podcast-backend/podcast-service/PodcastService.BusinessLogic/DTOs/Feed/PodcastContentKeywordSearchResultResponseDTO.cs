using PodcastService.BusinessLogic.DTOs.Show;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Episode;
using PodcastService.BusinessLogic.DTOs.Channel;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Show.ListItems;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.Category.ListItems;
using PodcastService.BusinessLogic.DTOs.Feed.ListItems;
using PodcastService.BusinessLogic.DTOs.Episode.ListItems;

namespace PodcastService.BusinessLogic.DTOs.Feed;

public class PodcastContentKeywordSearchResultResponseDTO
{
    public List<PodcastShowOrEpisodeKeywordSearchedListItemResponseDTO> TopSearchResults { get; set; } = new List<PodcastShowOrEpisodeKeywordSearchedListItemResponseDTO>();
    public List<ShowListItemResponseDTO> ShowList { get; set; } = new List<ShowListItemResponseDTO>();
    public List<EpisodeListItemResponseDTO> EpisodeList { get; set; } = new List<EpisodeListItemResponseDTO>();
    public List<ChannelListItemResponseDTO> ChannelList { get; set; } = new List<ChannelListItemResponseDTO>();
}
