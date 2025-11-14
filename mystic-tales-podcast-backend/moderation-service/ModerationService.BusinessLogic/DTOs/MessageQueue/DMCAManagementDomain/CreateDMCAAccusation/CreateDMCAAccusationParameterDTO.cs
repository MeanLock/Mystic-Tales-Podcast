using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CreateDMCAAccusation
{
    public class CreateDMCAAccusationParameterDTO
    {
        public string AccuserEmail { get; set; }
        public string AccuserPhone { get; set; }
        public string AccuserFullName { get; set; }
        public Guid? PodcastShowId { get; set; }
        public Guid? PodcastEpisodeId { get; set; }
        public List<string> DMCANoticeAttachFileKeys { get; set; } = new List<string>();
    }
}
