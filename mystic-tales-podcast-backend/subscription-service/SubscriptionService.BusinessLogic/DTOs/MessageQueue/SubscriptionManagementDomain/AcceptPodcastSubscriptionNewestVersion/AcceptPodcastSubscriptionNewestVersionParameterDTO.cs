using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CAcceptPodcastSubscriptionNewestVersion
{
    public class AcceptPodcastSubscriptionNewestVersionParameterDTO
    {
        public int AccountId { get; set; }
        public Guid PodcastSubscriptionRegistrationId { get; set; }
        public bool IsAccepted { get; set; }
    }
}
