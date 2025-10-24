using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.Podcast
{
    public class PodcastShowStatusTrackingDTO
    {
        public Guid Id { get; set; }

        public Guid PodcastShowId { get; set; }

        public int PodcastShowStatusId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
