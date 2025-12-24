using PodcastService.BusinessLogic.DTOs.Show.ListItems;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Episode;

namespace PodcastService.BusinessLogic.DTOs.Feed;

public class TrendingPodcastFeedDTO
{
    public required PopularPodcastersTrendingPodcastFeedSection PopularPodcasters { get; set; }
    public required CategoryTrendingPodcastFeedSection Category1 { get; set; }
    public required HotPodcastersTrendingPodcastFeedSection HotPodcasters { get; set; }
    public required CategoryTrendingPodcastFeedSection Category2 { get; set; }
    public required PopularChannelsTrendingPodcastFeedSection PopularChannels { get; set; }
    public required CategoryTrendingPodcastFeedSection Category3 { get; set; }
    public required HotChannelsTrendingPodcastFeedSection HotChannels { get; set; }
    public required CategoryTrendingPodcastFeedSection Category4 { get; set; }
    public required PopularShowsTrendingPodcastFeedSection PopularShows { get; set; }
    public required CategoryTrendingPodcastFeedSection Category5 { get; set; }
    public required HotShowsTrendingPodcastFeedSection HotShows { get; set; }
    public required CategoryTrendingPodcastFeedSection Category6 { get; set; }
    public required NewEpisodesTrendingPodcastFeedSection NewEpisodes { get; set; }
    public required PopularEpisodesTrendingPodcastFeedSection PopularEpisodes { get; set; }

    public class PopularPodcastersTrendingPodcastFeedSection
    {
        public List<AccountSnippetResponseDTO>? PodcasterList { get; set; }
    }

    public class CategoryTrendingPodcastFeedSection
    {
        public PodcastCategoryDTO? PodcastCategory { get; set; }
        public List<ShowListItemResponseDTO>? ShowList { get; set; }
    }

    public class HotPodcastersTrendingPodcastFeedSection
    {
        public List<AccountSnippetResponseDTO>? PodcasterList { get; set; }
    }

    public class PopularChannelsTrendingPodcastFeedSection
    {
        public List<ChannelListItemResponseDTO>? ChannelList { get; set; }
    }

    public class HotChannelsTrendingPodcastFeedSection
    {
        public List<ChannelListItemResponseDTO>? ChannelList { get; set; }
    }

    public class PopularShowsTrendingPodcastFeedSection
    {
        public List<ShowListItemResponseDTO>? ShowList { get; set; }
    }

    public class HotShowsTrendingPodcastFeedSection
    {
        public List<ShowListItemResponseDTO>? ShowList { get; set; }
    }

    public class NewEpisodesTrendingPodcastFeedSection
    {
        public List<PodcastEpisodeSnippetResponseDTO>? EpisodeList { get; set; }
    }

    public class PopularEpisodesTrendingPodcastFeedSection
    {
        public List<PodcastEpisodeSnippetResponseDTO>? EpisodeList { get; set; }
    }
}
