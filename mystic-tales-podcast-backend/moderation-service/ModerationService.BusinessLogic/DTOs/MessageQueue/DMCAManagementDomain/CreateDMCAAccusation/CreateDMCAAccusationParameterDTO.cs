using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.DMCAManagementDomain.CreateDMCAAccusation
{
    public class CreateDMCAAccusationParameterDTO
    {
        public int AccountId { get; set; }
        public string AccuserEmail { get; set; }
        public string AccuserPhone { get; set; }
        public Guid? PodcastShowId { get; set; }
        public Guid? PodcastEpisodeId { get; set; }
        public string GoodFaithStatement { get; set; }
        public string WorkClaimed { get; set; }
        public string Signature { get; set; }
        public List<string> DMCANoticeFileKeys { get; set; } = new List<string>();
    }
}
