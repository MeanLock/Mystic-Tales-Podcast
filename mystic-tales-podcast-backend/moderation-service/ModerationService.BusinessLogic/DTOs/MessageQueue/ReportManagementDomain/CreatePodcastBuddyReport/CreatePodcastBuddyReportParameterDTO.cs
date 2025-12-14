using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreatePodcastBuddyReport
{
    public class CreatePodcastBuddyReportParameterDTO
    {
        public int AccountId { get; set; }
        [Required]
        public int PodcastBuddyId { get; set; }
        [Required]
        public int PodcastBuddyReportTypeId { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
