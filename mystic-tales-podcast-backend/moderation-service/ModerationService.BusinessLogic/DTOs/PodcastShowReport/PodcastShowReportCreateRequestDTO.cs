using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.PodcastShowReport
{
    public class PodcastShowReportCreateRequestDTO
    {
        public PodcastShowReportCreateInfoDTO ShowReportCreateInfo { get; set; }
    }
    public class PodcastShowReportCreateInfoDTO
    {
        public string Content { get; set; }
        public int PodcastShowReportTypeId { get; set; }
    }
}
