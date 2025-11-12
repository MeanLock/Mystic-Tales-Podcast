using Newtonsoft.Json;
using PodcastService.BusinessLogic.Helpers.JsonHelpers;

namespace PodcastService.BusinessLogic.DTOs.Cache.QueryMetric
{
    public class ShowTemporal7dMaxQueryMetric
    {
        public required int MaxNewListenSession { get; set; }
        public required int MaxNewFollow { get; set; }
        public required DateTime LastUpdated { get; set; }
    }


}