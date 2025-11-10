using Newtonsoft.Json;
using UserService.BusinessLogic.Helpers.JsonHelpers;

namespace UserService.BusinessLogic.DTOs.Cache.QueryMetric
{
    public class ShowTemporal7dMaxQueryMetric
    {
        public required int MaxNewListenSession { get; set; }
        public required int MaxNewFollow { get; set; }
        public required DateTime LastUpdated { get; set; }
    }


}