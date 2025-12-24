using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.ProcessBookingPayTheRestPayment
{
    public class ProcessBookingPayTheRestPaymentParameterDTO
    {
        public int AccountId { get; set; }
        public int BookingId { get; set; }
    }
}
