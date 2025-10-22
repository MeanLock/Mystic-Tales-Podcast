using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.ResolvePodcastBuddyReport
{
    public class ResolvePodcastBuddyReportParameterDTO
    {
        public int AccountId { get; set; }
        public bool IsResolved { get; set; }
        public bool IsTakenEffect { get; set; }
        public int ResolvedViolationPoint { get; set; }
        public Guid PodcastBuddyReportReviewSessionId { get; set; }
    }
}
