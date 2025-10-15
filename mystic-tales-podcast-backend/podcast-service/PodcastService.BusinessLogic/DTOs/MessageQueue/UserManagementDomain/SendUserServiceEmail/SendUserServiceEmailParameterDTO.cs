using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PodcastService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SendPodcastServiceEmail
{
    public class SendPodcastServiceEmailParameterDTO
    {
        public SendPodcastServiceEmailMailInfoDTO SendPodcastServiceEmailMailInfo { get; set; }
    }

    public class SendPodcastServiceEmailMailInfoDTO
    {
        public string MailTypeName { get; set; }
        public JObject MailObject { get; set; }
        public string ToEmail { get; set; }
    }
}
