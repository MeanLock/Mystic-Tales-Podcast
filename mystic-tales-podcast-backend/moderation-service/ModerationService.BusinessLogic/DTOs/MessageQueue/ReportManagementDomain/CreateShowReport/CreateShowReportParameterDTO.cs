using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreateShowReport
{
    public class CreateShowReportParameterDTO
    {
        public int AccountId { get; set; }
        [Required]
        public Guid PodcastShowId { get; set; }
        [Required]
        public int PodcastShowReportTypeId { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
