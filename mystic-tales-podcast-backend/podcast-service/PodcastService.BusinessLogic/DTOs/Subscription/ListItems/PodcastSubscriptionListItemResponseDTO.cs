using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Subscription.ListItems
{
    public class PodcastSubscriptionListItemResponseDTO : PodcastSubscriptionDTO
    {
        public List<PodcastSubscriptionCycleTypePriceListItemResponseDTO> PodcastSubscriptionCycleTypePriceList { get; set; } = new();
        public List<PodcastSubscriptionBenefitMappingListItemResponseDTO> PodcastSubscriptionBenefitMappingList { get; set; } = new();
    }
}