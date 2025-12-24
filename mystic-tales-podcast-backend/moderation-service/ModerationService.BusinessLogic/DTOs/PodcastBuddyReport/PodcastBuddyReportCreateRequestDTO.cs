using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.PodcastBuddyReport
{
    public class PodcastBuddyReportCreateRequestDTO
    {
        public PodcastBuddyReportCreateInfoDTO BuddyReportCreateInfo { get; set; }
    }
    public class PodcastBuddyReportCreateInfoDTO
    {
        public string Content { get; set; }
        public int PodcastBuddyReportTypeId { get; set; }
    }
}
