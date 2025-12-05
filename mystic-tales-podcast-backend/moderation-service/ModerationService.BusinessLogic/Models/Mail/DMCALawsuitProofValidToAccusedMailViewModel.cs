using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.Models.Mail
{
    public class DMCALawsuitProofValidToAccusedMailViewModel
    {
        public string PodcasterEmail { get; set; } = null!;
        public string PodcasterFullName { get; set; } = null!;
        public List<string> AttachmentFileUrls { get; set; } = new List<string>();
    }
}
