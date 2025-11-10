using Newtonsoft.Json;
using UserService.BusinessLogic.Helpers.JsonHelpers;

namespace UserService.BusinessLogic.DTOs.Cache.QueryMetric
{
    public class SystemPreferencesTemporal30dQueryMetric
    {
        public required List<ListenedPodcastCategory> ListenedPodcastCategories { get; set; }
        public required List<ListenedPodcaster> ListenedPodcasters { get; set; }
        public required DateTime LastUpdated { get; set; }
    }

    public class ListenedPodcastCategory
    {
        public required int PodcastCategoryId { get; set; }
        public required List<PodcastSubCategoryListenCount> PodcastSubCategoryIds { get; set; }
        public required int ListenCount { get; set; }
    }
    
    public class PodcastSubCategoryListenCount
    {
        public required int PodcastSubCategoryId { get; set; }
        public required int ListenCount { get; set; }
    }

    public class ListenedPodcaster
    {
        public required int PodcasterId { get; set; }
        public required int ListenCount { get; set; }
    }


}