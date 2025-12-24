using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.DeletePodcastSubscription
{
    public class DeletePodcastSubscriptionParameterDTO
    {
        public int AccountId { get; set; }
        public int PodcastSubscriptionId { get; set; }
    }
}
