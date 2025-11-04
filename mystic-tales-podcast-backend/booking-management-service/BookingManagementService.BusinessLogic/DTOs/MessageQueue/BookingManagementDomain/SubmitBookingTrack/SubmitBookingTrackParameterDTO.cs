using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.SubmitBookingTrack
{
    public class SubmitBookingTrackParameterDTO
    {
        public Guid BookingProducingRequestId { get; set; }
        public List<TracksParameterDTO> Tracks { get; set; }
    }

    public class TracksParameterDTO
    {
        public Guid Id { get; set; }
        public string AudioFileKey { get; set; }
        public double AudioFileSize { get; set; }
        public int AudioLength { get; set; }
    }
}
