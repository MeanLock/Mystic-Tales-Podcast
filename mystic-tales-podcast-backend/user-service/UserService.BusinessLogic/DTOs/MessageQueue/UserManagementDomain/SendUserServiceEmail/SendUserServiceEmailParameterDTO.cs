using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SendUserServiceEmail
{
    public class SendUserServiceEmailParameterDTO
    {
        public SendUserServiceEmailMailInfoDTO SendUserServiceEmailMailInfo { get; set; }
    }

    public class SendUserServiceEmailMailInfoDTO
    {
        public string MailTypeName { get; set; }
        public JObject MailObject { get; set; }
        public string ToEmail { get; set; }
    }
}
