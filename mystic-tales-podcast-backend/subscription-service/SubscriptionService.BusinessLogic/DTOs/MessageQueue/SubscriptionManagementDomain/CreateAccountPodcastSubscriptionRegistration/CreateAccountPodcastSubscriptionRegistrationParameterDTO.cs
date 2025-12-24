using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CreateAccountPodcastSubscriptionRegistration
{
    public class CreateAccountPodcastSubscriptionRegistrationParameterDTO
    {
        public int AccountId { get; set; }
        public int PodcastSubscriptionId { get; set; }
        public int SubscriptionCycleTypeId { get; set; }
    }
}
