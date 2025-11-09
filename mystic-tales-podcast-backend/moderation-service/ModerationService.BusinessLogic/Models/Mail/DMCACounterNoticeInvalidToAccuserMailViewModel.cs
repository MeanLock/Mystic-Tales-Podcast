using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.Models.Mail
{
    public class DMCACounterNoticeInvalidToAccuserMailViewModel
    {
        public string AccuserEmail { get; set; }
        public string AccuserFullName { get; set; }
        public string? PodcastShowName { get; set; }
        public string? PodcastEpisodeName { get; set; }
        public string InvalidReason { get; set; }
	    public DateTime CompletedAt { get; set; }
    }
}
