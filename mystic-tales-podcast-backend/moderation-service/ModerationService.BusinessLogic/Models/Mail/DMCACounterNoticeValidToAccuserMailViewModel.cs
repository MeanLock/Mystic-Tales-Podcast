using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.Models.Mail
{
    public class DMCACounterNoticeValidToAccuserMailViewModel
    {
        public string AccuserEmail { get; set; } = null!;
        public string AccuserFullName { get; set; } = null!;
        public DateTime ValidatedAt { get; set; }
        public int TimeToResponse { get; set; }
        public List<string> AttachmentFileUrls { get; set; } = new List<string>();
    }
}
