using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.ProcessBookingDepositPayment
{
    public class ProcessBookingDepositPaymentParameterDTO
    {
        public int AccountId { get; set; }
        public int BookingId { get; set; }
    }
}
