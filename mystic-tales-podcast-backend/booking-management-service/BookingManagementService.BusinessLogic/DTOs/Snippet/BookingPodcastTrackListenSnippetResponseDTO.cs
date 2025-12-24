using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Snippet
{
    public class BookingPodcastTrackListenSnippetResponseDTO
    {
        public Guid Id { get; set; }
        public string BookingRequirementName { get; set; }
        public string BookingRequirementDescription { get; set; }
    }
}
