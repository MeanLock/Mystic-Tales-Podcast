using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.Podcast.ListItems
{
    public class PodcastChannelStatusTrackingDTO
    {
        public Guid Id { get; set; }

        public Guid PodcastChannelId { get; set; }

        public int PodcastChannelStatusId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
