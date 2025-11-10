using Newtonsoft.Json;
using UserService.BusinessLogic.Helpers.JsonHelpers;

namespace UserService.BusinessLogic.DTOs.Cache.QueryMetric
{
    public class PodcasterAllTimeMaxQueryMetric
    {
        public required int MaxTotalFollow { get; set; }
        public required int MaxListenCount { get; set; }
        public required int MaxRatingTerm { get; set; }
        public required int MaxAge { get; set; } // in days
        public required DateTime LastUpdated { get; set; }
    }


}