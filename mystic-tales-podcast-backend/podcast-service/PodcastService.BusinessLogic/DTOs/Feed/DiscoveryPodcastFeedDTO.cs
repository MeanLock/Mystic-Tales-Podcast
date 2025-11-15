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
    public required ContinueListeningDiscoveryPodcastFeedSection ContinueListening { get; set; }
    public required BasedOnYourTasteDiscoveryPodcastFeedSection BasedOnYourTaste { get; set; }
    public required NewReleasesDiscoveryPodcastFeedSection NewReleases { get; set; }
    public required HotThisWeekDiscoveryPodcastFeedSection HotThisWeek { get; set; }
    public required TopSubCategoryDiscoveryPodcastFeedSection TopSubCategory { get; set; }
    public required TopPodcastersDiscoveryPodcastFeedSection TopPodcasters { get; set; }
    public required RandomCategoryDiscoveryPodcastFeedSection RandomCategory { get; set; }
    public required TalentedRookiesDiscoveryPodcastFeedSection TalentedRookies { get; set; }

    public class ContinueListeningDiscoveryPodcastFeedSection
    {
        public required List<ListenSessionDiscoveryPodcastFeedListItem> ListenSessionList { get; set; }
    }

    public class ListenSessionDiscoveryPodcastFeedListItem
    {
        public PodcastEpisodeSnippetResponseDTO? Episode { get; set; }
        public AccountSnippetResponseDTO? Podcaster { get; set; }
        public required PodcastEpisodeListenSessionSnippetResponseDTO? PodcastEpisodeListenSession { get; set; }
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
        public required PodcastCategoryDTO PodcastCategory { get; set; }
        public List<ShowListItemResponseDTO>? ShowList { get; set; }
    }

    public class TalentedRookiesDiscoveryPodcastFeedSection
    {
        public List<AccountSnippetResponseDTO>? PodcasterList { get; set; }
    }


}
