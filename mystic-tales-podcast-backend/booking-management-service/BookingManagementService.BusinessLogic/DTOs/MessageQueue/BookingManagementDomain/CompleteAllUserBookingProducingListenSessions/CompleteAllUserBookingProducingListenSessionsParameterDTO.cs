using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CompleteAllUserBookingProducingListenSessions
{
    public class CompleteAllUserBookingProducingListenSessionsParameterDTO
    {
        public int AccountId { get; set; }  
        public bool IsBookingProducingListenSessionCompleted { get; set; }
    }
}
