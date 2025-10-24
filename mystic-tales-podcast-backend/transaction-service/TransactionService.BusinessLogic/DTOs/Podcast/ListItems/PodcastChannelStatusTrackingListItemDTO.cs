using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.Podcast.ListItems
{
    public class PodcastChannelStatusTrackingListItemDTO
    {
        public Guid Id { get; set; }

        public Guid PodcastChannelId { get; set; }

        public int PodcastChannelStatusId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
