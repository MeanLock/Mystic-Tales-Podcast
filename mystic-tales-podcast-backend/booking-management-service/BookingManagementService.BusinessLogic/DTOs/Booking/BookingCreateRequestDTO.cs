using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Booking
{
    public class BookingCreateRequestDTO
    {
        public BookingInfoDTO BookingInfo { get; set; } = new BookingInfoDTO();
    }
    public class BookingInfoDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PodcastBuddyId { get; set; }
    }
}
