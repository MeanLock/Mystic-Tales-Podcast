using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CancelBookingManual
{
    public class CancelBookingManualParameterDTO
    {
        public int AccountId { get; set; }
        public int BookingId { get; set; }
        public string BookingManualCancelledReason { get; set; }
    }
}
