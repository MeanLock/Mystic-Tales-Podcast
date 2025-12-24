using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.Models.Mail
{
    public class DMCACounterNoticeValidToAccusedMailViewModel
    {
        public string PodcasterEmail { get; set; } = null!;
        public string PodcasterFullName { get; set; } = null!;
        public DateTime ValidatedAt { get; set; }
    }
}
