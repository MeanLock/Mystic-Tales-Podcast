using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelPodcastSubscriptionRegistration
{
    public class CancelPodcastSubscriptionRegistrationParameterDTO
    {
        public int AccountId { get; set; }
        public Guid PodcastSubscriptionRegistrationId { get; set; }
    }
}
