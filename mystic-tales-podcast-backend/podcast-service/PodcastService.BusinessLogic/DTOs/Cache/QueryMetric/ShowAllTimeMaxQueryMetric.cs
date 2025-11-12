using Newtonsoft.Json;
using PodcastService.BusinessLogic.Helpers.JsonHelpers;

namespace PodcastService.BusinessLogic.DTOs.Cache.QueryMetric
{
    public class ShowAllTimeMaxQueryMetric
    {
        public required int MaxTotalFollow { get; set; }
        public required int MaxListenCount { get; set; }
        public required double MaxRatingTerm { get; set; }
        public required DateTime LastUpdated { get; set; }
    }


}