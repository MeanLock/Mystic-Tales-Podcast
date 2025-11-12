using Newtonsoft.Json;
using PodcastService.BusinessLogic.Helpers.JsonHelpers;

namespace PodcastService.BusinessLogic.DTOs.Cache.QueryMetric
{
    public class UserPreferencesTemporal30dQueryMetric
    {
        public required int UserId { get; set; }
        public required List<UserListenedPodcastCategory> ListenedPodcastCategories { get; set; }
        public required List<UserListenedPodcaster> ListenedPodcasters { get; set; }
        public required DateTime LastUpdated { get; set; }
    }

    public class UserListenedPodcastCategory
    {
        public required int PodcastCategoryId { get; set; }
        public required List<UserListenedPodcastSubCategory> PodcastSubCategories { get; set; }
        public required int ListenCount { get; set; }
    }
    public class UserListenedPodcastSubCategory
    {
        public required int PodcastSubCategoryId { get; set; }
        public required int ListenCount { get; set; }
    }
    public class UserListenedPodcaster
    {
        public required int PodcasterId { get; set; }
        public required int ListenCount { get; set; }
    }





}