using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.CancelPodcastSubscriptionRegistrationChannelShows
{
    public class CancelPodcastSubscriptionRegistrationChannelShowsParameterDTO
    {
        public Guid PodcastShowId { get; set; }
        public Guid? PodcastChannelId { get; set; }
        public int PodcasterId { get; set; }
    }
}
