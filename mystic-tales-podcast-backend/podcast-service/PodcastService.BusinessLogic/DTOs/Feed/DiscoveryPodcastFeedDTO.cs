using PodcastService.BusinessLogic.DTOs.Show;
using PodcastService.BusinessLogic.DTOs.Account;
using PodcastService.BusinessLogic.DTOs.Episode;
using PodcastService.BusinessLogic.DTOs.Channel;
using PodcastService.BusinessLogic.DTOs.Category;
using PodcastService.BusinessLogic.DTOs.Show.ListItems;
using PodcastService.BusinessLogic.DTOs.Channel.ListItems;
using PodcastService.BusinessLogic.DTOs.Category.ListItems;

namespace PodcastService.BusinessLogic.DTOs.Feed;

public class DiscoveryPodcastFeedDTO
{
    public ContinueListeningDiscoveryPodcastFeedSection? ContinueListening { get; set; }
    public BasedOnYourTasteDiscoveryPodcastFeedSection? BasedOnYourTaste { get; set; }
    public NewReleasesDiscoveryPodcastFeedSection? NewReleases { get; set; }
    public HotThisWeekDiscoveryPodcastFeedSection? HotThisWeek { get; set; }
    public TopSubCategoryDiscoveryPodcastFeedSection? TopSubCategory { get; set; }
    public TopPodcastersDiscoveryPodcastFeedSection? TopPodcasters { get; set; }
    public RandomCategoryDiscoveryPodcastFeedSection? RandomCategory { get; set; }
    public TalentedRookiesDiscoveryPodcastFeedSection? TalentedRookies { get; set; }

    public class ContinueListeningDiscoveryPodcastFeedSection
    {
        public List<ListenSessionDiscoveryPodcastFeedListItem>? ListenSessionList { get; set; }
    }

    public class ListenSessionDiscoveryPodcastFeedListItem
    {
        public PodcastEpisodeSnippetResponseDTO? Episode { get; set; }
        public AccountSnippetResponseDTO? Podcaster { get; set; }
    }

    public class BasedOnYourTasteDiscoveryPodcastFeedSection
    {
        public List<ShowListItemResponseDTO>? ShowList { get; set; }
    }

    public class NewReleasesDiscoveryPodcastFeedSection
    {
        public List<ShowListItemResponseDTO>? ShowList { get; set; }
    }

    public class HotThisWeekDiscoveryPodcastFeedSection
    {
        public List<ShowListItemResponseDTO>? ShowList { get; set; }
        public List<ChannelListItemResponseDTO>? ChannelList { get; set; }
    }

    public class TopSubCategoryDiscoveryPodcastFeedSection
    {
        public PodcastSubCategoryListItemResponseDTO? PodcastSubCategory { get; set; }
        public List<ShowListItemResponseDTO>? ShowList { get; set; }
    }

    public class TopPodcastersDiscoveryPodcastFeedSection
    {
        public List<AccountSnippetResponseDTO>? PodcasterList { get; set; }
    }

    public class RandomCategoryDiscoveryPodcastFeedSection
    {
        public List<ShowListItemResponseDTO>? ShowList { get; set; }
    }

    public class TalentedRookiesDiscoveryPodcastFeedSection
    {
        public List<AccountSnippetResponseDTO>? PodcasterList { get; set; }
    }


}
