using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.Models.Mail
{
    public class DMCALawsuitProofInvalidToAccusedMailViewModel
    {
        public string PodcasterEmail { get; set; }
        public string PodcasterFullName { get; set; }
        public string? PodcastShowName { get; set; }
        public string? PodcastEpisodeName { get; set; }
    }
}
