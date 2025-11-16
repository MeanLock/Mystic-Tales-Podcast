using Newtonsoft.Json;
using PodcastService.BusinessLogic.Helpers.JsonHelpers;

namespace PodcastService.BusinessLogic.DTOs.Cache
{
    public class CustomerRecordedPodcastContentSearchKeywordCache
    {
        public required List<CustomerRecordedPodcastContentSearchKeywordCacheItem> KeywordList { get; set; }
    }
    
    public class CustomerRecordedPodcastContentSearchKeywordCacheItem
    {
        public required string Keyword { get; set; }
        public required int SearchCount { get; set; }
    }

}