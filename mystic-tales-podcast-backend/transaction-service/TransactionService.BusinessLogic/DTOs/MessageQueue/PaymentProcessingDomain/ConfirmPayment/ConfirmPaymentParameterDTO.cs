using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.MessageQueue.PaymentProcessingDomain.ConfirmPayment
{
    public class ConfirmPaymentParameterDTO
    {
        public WebhookType WebHookBody { get; set; }
    }
}
