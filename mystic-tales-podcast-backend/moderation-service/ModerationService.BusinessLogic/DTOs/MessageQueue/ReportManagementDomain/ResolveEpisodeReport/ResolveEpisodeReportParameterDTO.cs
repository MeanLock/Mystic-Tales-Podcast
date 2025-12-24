using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolveEpisodeReport
{
    public class ResolveEpisodeReportParameterDTO
    {
        public int AccountId { get; set; }
        public bool IsResolved { get; set; }
        public bool IsTakenEffect { get; set; }
        public Guid PodcastEpisodeReportReviewSessionId { get; set; }
    }
}
