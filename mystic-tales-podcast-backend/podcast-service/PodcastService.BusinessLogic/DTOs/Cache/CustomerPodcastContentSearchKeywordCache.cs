using Newtonsoft.Json;
using PodcastService.BusinessLogic.Helpers.JsonHelpers;

namespace PodcastService.BusinessLogic.DTOs.Cache
{
    public class CustomerPodcastContentSearchKeywordCache
    {
        public required List<CustomerPodcastContentSearchKeywordCacheItem> KeywordList { get; set; }
    }
    
    public class CustomerPodcastContentSearchKeywordCacheItem
    {
        public required string Keyword { get; set; }
        public required int SearchCount { get; set; }
    }

}