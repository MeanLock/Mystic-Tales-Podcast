using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreateShowReport
{
    public class CreateShowReportParameterDTO
    {
        public int AccountId { get; set; }
        public Guid PodcastShowId { get; set; }
        public int PodcastShowReportTypeId { get; set; }
        public string Content { get; set; }
    }
}
