using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.MessageQueue.SubscriptionManagementDomain.SendSubscriptionServiceEmail
{
    public class SendSubscriptionServiceEmailParameterDTO
    {
        public SendSubscriptionServiceEmailInfoDTO SendSubscriptionServiceEmailInfo { get; set; }
    }
    public class SendSubscriptionServiceEmailInfoDTO
    {
        public string MailTypeName { get; set; }
        public JObject MailObject { get; set; }
        public string ToEmail { get; set; }
    }
}
