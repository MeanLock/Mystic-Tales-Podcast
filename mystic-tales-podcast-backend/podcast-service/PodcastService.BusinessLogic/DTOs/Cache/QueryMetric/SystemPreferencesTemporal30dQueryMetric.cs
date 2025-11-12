using Newtonsoft.Json;
using PodcastService.BusinessLogic.Helpers.JsonHelpers;

namespace PodcastService.BusinessLogic.DTOs.Cache.QueryMetric
{
    public class SystemPreferencesTemporal30dQueryMetric
    {
        public required List<SystemListenedPodcastCategory> ListenedPodcastCategories { get; set; }
        public required List<SystemListenedPodcaster> ListenedPodcasters { get; set; }
        public required DateTime LastUpdated { get; set; }
    }

    public class SystemListenedPodcastCategory
    {
        public required int PodcastCategoryId { get; set; }
        public required List<SystemListenedPodcastSubCategory> PodcastSubCategories { get; set; }
        public required int ListenCount { get; set; }
    }
    
    public class SystemListenedPodcastSubCategory
    {
        public required int PodcastSubCategoryId { get; set; }
        public required int ListenCount { get; set; }
    }

    public class SystemListenedPodcaster
    {
        public required int PodcasterId { get; set; }
        public required int ListenCount { get; set; }
    }


}