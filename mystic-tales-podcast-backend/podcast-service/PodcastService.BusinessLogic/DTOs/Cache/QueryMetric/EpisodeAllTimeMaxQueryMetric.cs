using Newtonsoft.Json;
using PodcastService.BusinessLogic.Helpers.JsonHelpers;

namespace PodcastService.BusinessLogic.DTOs.Cache.QueryMetric
{
    public class EpisodeAllTimeMaxQueryMetric
    {
        public required int MaxListenCount { get; set; }
        public required int MaxTotalSave { get; set; }
        public required DateTime LastUpdated { get; set; }
    }


}