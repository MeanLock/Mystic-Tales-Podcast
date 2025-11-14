using PodcastService.BusinessLogic.DTOs.Show.ListItems;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Episode;

namespace PodcastService.BusinessLogic.DTOs.Feed;

public class TrendingPodcastFeedDTO
{
    public PopularPodcastersTrendingPodcastFeedSection? PopularPodcasters { get; set; }
    public CategoryTrendingPodcastFeedSection? Category1 { get; set; }
    public HotPodcastersTrendingPodcastFeedSection? HotPodcasters { get; set; }
    public CategoryTrendingPodcastFeedSection? Category2 { get; set; }
    public PopularChannelsTrendingPodcastFeedSection? PopularChannels { get; set; }
    public CategoryTrendingPodcastFeedSection? Category3 { get; set; }
    public HotChannelsTrendingPodcastFeedSection? HotChannels { get; set; }
    public CategoryTrendingPodcastFeedSection? Category4 { get; set; }
    public PopularShowsTrendingPodcastFeedSection? PopularShows { get; set; }
    public CategoryTrendingPodcastFeedSection? Category5 { get; set; }
    public HotShowsTrendingPodcastFeedSection? HotShows { get; set; }
    public CategoryTrendingPodcastFeedSection? Category6 { get; set; }
    public NewEpisodesTrendingPodcastFeedSection? NewEpisodes { get; set; }
    public PopularEpisodesTrendingPodcastFeedSection? PopularEpisodes { get; set; }

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
