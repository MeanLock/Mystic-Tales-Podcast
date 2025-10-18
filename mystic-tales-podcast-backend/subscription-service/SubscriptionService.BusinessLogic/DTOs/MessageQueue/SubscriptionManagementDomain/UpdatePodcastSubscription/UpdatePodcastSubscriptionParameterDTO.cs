using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.UpdatePodcastSubscription
{
    public class UpdatePodcastSubscriptionParameterDTO
    {
        public int AccountId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PodcastSubscriptionId { get; set; }
        public List<PodcastSubscriptionCycleTypePriceParameterDTO> PodcastSubscriptionCycleTypePriceList { get; set; }
        public List<int> PodcastSubscriptionBenefitMappingList { get; set; }
    }
    public class PodcastSubscriptionCycleTypePriceParameterDTO
    {
        public int SubscriptionCycleTypeId { get; set; }
        public decimal Price { get; set; }
    }
}
