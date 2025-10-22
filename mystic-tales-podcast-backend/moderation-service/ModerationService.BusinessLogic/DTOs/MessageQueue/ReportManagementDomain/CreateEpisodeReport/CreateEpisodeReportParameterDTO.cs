using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreateEpisodeReport
{
    public class CreateEpisodeReportParameterDTO
    {
        public int AccountId { get; set; }
        public Guid PodcastEpisodeId { get; set; }
        public int PodcastEpisodeReportTypeId { get; set; }
        public string Content { get; set; }
    }
}
