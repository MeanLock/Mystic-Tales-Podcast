using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.Models.Mail
{
    public class DMCALawsuitProofPodcasterWinToAccuserMailViewModel
    {
        public string AccuserEmail { get; set; } = null!;
        public string AccuserFullName { get; set; } = null!;
        public string? PodcastShowName { get; set; }
        public string? PodcastEpisodeName { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
