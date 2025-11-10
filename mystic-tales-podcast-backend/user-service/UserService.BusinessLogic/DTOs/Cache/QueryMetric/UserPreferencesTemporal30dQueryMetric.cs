using Newtonsoft.Json;
using UserService.BusinessLogic.Helpers.JsonHelpers;

namespace UserService.BusinessLogic.DTOs.Cache.QueryMetric
{
    public class UserPreferencesTemporal30dQueryMetric
    {
        public required List<UserListenedPodcastCategory> ListenedPodcastCategories { get; set; }
        public required List<UserListenedPodcaster> ListenedPodcasters { get; set; }
        public required DateTime LastUpdated { get; set; }
    }

    public class UserListenedPodcastCategory
    {
        public required int PodcastCategoryId { get; set; }
        public required List<UserPodcastSubCategoryListenCount> PodcastSubCategoryIds { get; set; }
        public required int ListenCount { get; set; }
    }
    public class UserPodcastSubCategoryListenCount
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