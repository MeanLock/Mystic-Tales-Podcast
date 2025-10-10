using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateBooking
{
    public class CreateBookingParameterDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int AccountId { get; set; }
        public int PodcastBuddyId { get; set; }
    }
}
