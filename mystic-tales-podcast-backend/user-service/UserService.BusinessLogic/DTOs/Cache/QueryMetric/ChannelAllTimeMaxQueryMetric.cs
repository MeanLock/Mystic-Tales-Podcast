using Newtonsoft.Json;
using UserService.BusinessLogic.Helpers.JsonHelpers;

namespace UserService.BusinessLogic.DTOs.Cache.QueryMetric
{
    public class ChannelAllTimeMaxQueryMetric
    {
        public required int MaxTotalListenSession { get; set; }
        public required int MaxTotalFavorite { get; set; }
        public required DateTime LastUpdated { get; set; }
    }


}