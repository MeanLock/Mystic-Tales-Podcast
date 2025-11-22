using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Snippet
{
    public class BookingPodcastTrackListenSessionSnippetResponseDTO
    {
        public Guid Id { get; set; }
        public int LastListenDurationSeconds { get; set; }
    }
}
