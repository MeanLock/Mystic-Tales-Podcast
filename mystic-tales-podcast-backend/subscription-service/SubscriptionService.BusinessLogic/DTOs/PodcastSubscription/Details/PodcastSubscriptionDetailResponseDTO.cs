using SubscriptionService.BusinessLogic.DTOs.PodcastSubscription.ListItems;

namespace SubscriptionService.BusinessLogic.DTOs.PodcastSubscription.Details
{
    public class PodcastSubscriptionDetailResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? PodcastShowId { get; set; }
        public Guid? PodcastChannelId { get; set; }
        public bool IsActive { get; set; }
        public int CurrentVersion { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PodcastSubscriptionCycleTypePriceListItemResponseDTO>? PodcastSubscriptionCycleTypePriceList { get; set; }
        public List<PodcastSubscriptionBenefitMappingListItemResponseDTO>? PodcastSubscriptionBenefitMappingList { get; set; }
        public List<PodcastSubscriptionRegistrationListItemResponseDTO>? PodcastSubscriptionRegistrationList { get; set; }
    }
}
