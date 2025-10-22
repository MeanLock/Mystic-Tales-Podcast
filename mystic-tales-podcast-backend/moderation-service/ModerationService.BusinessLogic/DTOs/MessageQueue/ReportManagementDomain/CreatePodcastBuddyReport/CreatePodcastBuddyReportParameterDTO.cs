using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreatePodcastBuddyReport
{
    public class CreatePodcastBuddyReportParameterDTO
    {
        public int AccountId { get; set; }
        public int PodcastBuddyId { get; set; }
        public int PodcastBuddyReportTypeId { get; set; }
        public string Content { get; set; }
    }
}
