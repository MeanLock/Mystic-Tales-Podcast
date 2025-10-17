using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CreatePodcastSubscription
{
    public class CreatePodcastSubscriptionParameterDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? PodcastShowId { get; set; }
        public Guid? PodcastChannelId { get; set; }
        public List<PodcastSubscriptionCycleTypePriceParameterDTO> PodcastSubscriptionCycleTypePriceList { get; set; }
        public List<int> PodcastSubscriptionBenefitMappingList { get; set; }
    }
    public class PodcastSubscriptionCycleTypePriceParameterDTO
    {
        public int SubscriptionCycleTypeId { get; set; }
        public decimal Price { get; set; }
    }
}
