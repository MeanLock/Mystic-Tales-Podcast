using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.ProcessingBookingTransaction
{
    public class ProcessingBookingTransactionParameterDTO
    {
        public Guid BookingTransactionId { get; set; }
    }
}
