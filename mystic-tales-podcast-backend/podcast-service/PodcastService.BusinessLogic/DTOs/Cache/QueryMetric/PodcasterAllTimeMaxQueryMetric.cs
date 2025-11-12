using Newtonsoft.Json;
using PodcastService.BusinessLogic.Helpers.JsonHelpers;

namespace PodcastService.BusinessLogic.DTOs.Cache.QueryMetric
{
    public class PodcasterAllTimeMaxQueryMetric
    {
        public required int MaxTotalFollow { get; set; }
        public required int MaxListenCount { get; set; }
        public required double MaxRatingTerm { get; set; }
        public required int MaxAge { get; set; } // in days
        public required DateTime LastUpdated { get; set; }
    }


}