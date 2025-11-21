using SubscriptionService.BusinessLogic.DTOs.PodcastSubscription.ListItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.Models.Mail
{
    public class PodcastSubscriptionNewVersionMailViewModel
    {
        public string CustomerFullName { get; set; }
        public PodcastSubscriptionListItemResponseDTO PodcastSubscription { get; set; }
        public List<PodcastSubscriptionBenefitMappingListItemResponseDTO> PodcastSubscriptionBenefitList { get; set; }
        public List<PodcastSubscriptionCycleTypePriceListItemResponseDTO> PodcastSubscriptionCycleTypePriceList { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
