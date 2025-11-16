using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.SubmitBookingTrack
{
    public class SubmitBookingTrackParameterDTO
    {
        public int AccountId { get; set; }
        public Guid BookingProducingRequestId { get; set; }
        [Required]
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
