using Microsoft.AspNetCore.Http;

namespace PodcastService.BusinessLogic.DTOs.Subscription
{
    public class PodcastSubscriptionBenefitDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
    }
}