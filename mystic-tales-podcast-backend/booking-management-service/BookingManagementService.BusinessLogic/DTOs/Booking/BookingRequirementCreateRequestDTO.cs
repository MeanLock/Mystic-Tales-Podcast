using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Booking
{
    public class BookingRequirementCreateRequestDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public Guid PodcastBookingToneId { get; set; }
    }
}
