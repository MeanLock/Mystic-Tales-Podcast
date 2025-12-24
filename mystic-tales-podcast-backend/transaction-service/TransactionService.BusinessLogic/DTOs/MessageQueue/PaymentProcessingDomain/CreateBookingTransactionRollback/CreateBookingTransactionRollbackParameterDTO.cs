using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.CreateBookingTransactionRollback
{
    public class CreateBookingTransactionRollbackParameterDTO
    {
        public Guid BookingTransactionId { get; set; }
    }
}
