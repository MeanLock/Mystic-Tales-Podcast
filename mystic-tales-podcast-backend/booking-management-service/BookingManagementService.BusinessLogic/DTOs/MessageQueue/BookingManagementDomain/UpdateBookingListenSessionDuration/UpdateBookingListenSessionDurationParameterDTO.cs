using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.UpdateBookingListenSessionDuration
{
    public class UpdateBookingListenSessionDurationParameterDTO
    {
        public required Guid BookingPodcastTrackListenSessionId { get; set; }
        public required int ListenerId { get; set; }
        public required int LastListenDurationSeconds { get; set; }
    }
}
