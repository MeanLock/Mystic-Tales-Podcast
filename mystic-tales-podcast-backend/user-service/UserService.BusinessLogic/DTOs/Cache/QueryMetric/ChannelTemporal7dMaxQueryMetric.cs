using Newtonsoft.Json;
using UserService.BusinessLogic.Helpers.JsonHelpers;

namespace UserService.BusinessLogic.DTOs.Cache.QueryMetric
{
    public class ChannelTemporal7dMaxQueryMetric
    {
        public required int MaxNewListenSession { get; set; }
        public required int MaxNewFavorite { get; set; }
        public required DateTime LastUpdated { get; set; }
    }


}