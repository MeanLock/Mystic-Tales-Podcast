using Newtonsoft.Json;
using UserService.BusinessLogic.Helpers.JsonHelpers;

namespace UserService.BusinessLogic.DTOs.Cache.QueryMetric
{
    public class ShowAllTimeMaxQueryMetric
    {
        public required int MaxTotalFollow { get; set; }
        public required int MaxListenCount { get; set; }
        public required int MaxRatingTerm { get; set; }
        public required DateTime LastUpdated { get; set; }
    }


}