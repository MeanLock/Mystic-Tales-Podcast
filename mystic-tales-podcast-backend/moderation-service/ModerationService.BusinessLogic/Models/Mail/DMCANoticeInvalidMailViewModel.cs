using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.Models.Mail
{
    public class DMCANoticeInvalidMailViewModel
    {
        public string AccuserEmail { get; set; } = null!;
        public string AccuserFullName { get; set; } = null!;
        public string InvalidReason { get; set; } = null!;
        public DateTime CompletedAt { get; set; }
    }
}
