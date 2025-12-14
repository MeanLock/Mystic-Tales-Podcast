using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CreatePodcastSubscription
{
    public class CreatePodcastSubscriptionParameterDTO
    {
        public int AccountId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public Guid? PodcastShowId { get; set; }
        public Guid? PodcastChannelId { get; set; }
        [Required]
        public List<PodcastSubscriptionCycleTypePriceParameterDTO> PodcastSubscriptionCycleTypePriceList { get; set; }
        [Required]
        public List<int> PodcastSubscriptionBenefitMappingList { get; set; }
    }
    public class PodcastSubscriptionCycleTypePriceParameterDTO
    {
        public int SubscriptionCycleTypeId { get; set; }
        public decimal Price { get; set; }
    }
}
