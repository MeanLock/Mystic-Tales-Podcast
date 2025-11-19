using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Booking
{
    public class PodcasterBookingToneApplyRequestDTO
    {
        public PodcasterBookingToneApplyDTO PodcasterBookingToneApplyInfo { get; set; }
    }
    public class PodcasterBookingToneApplyDTO
    {
        public List<Guid> PodcastBookingToneIds { get; set; }
    }
}
