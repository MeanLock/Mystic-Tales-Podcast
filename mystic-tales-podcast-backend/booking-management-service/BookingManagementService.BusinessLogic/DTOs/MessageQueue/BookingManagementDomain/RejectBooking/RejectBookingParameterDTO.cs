using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.RejectBooking
{
    public class RejectBookingParameterDTO
    {
        public int BookingId { get; set; }
    }
}
