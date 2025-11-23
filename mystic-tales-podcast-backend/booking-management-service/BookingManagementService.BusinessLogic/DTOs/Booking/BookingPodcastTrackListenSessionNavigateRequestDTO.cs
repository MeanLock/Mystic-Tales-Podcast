using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Booking
{
    public class BookingPodcastTrackListenSessionNavigateRequestDTO
    {
        public CurrentListenSessionDTO CurrentListenSession { get; set; }
    }
    public class CurrentListenSessionDTO
    {
        public Guid ListenSessionId { get; set; }
        public Guid ListenSessionProcedureId { get; set; }
    }
}
