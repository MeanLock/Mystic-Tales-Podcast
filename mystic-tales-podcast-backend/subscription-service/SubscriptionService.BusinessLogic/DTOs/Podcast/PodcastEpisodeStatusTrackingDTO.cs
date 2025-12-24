using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.Podcast
{
    public class PodcastEpisodeStatusTrackingDTO
    {
        public Guid Id { get; set; }
        public Guid PodcastEpisodeId { get; set; }
        public int PodcastEpisodeStatusId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
