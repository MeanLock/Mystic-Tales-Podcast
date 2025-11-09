using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.SendModerationServiceEmail
{
    public class SendModerationServiceEmailParameterDTO
    {
        public SendModerationServiceEmailInfoDTO SendModerationServiceEmailInfo { get; set; }
    }
    public class SendModerationServiceEmailInfoDTO
    {
        public string MailTypeName { get; set; }
        public JObject MailObject { get; set; }
        public string ToEmail { get; set; }
    }
}
