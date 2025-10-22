using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.PodcastEpisodeReport
{
    public class PodcastEpisodeReportCreateRequestDTO
    {
        public PodcastEpisodeReportCreateInfoDTO EpisodeReportCreateInfo { get; set; }
    }
    public class PodcastEpisodeReportCreateInfoDTO
    {
        public string Content { get; set; }
        public int PodcastEpisodeReportTypeId { get; set; }
    }
}
