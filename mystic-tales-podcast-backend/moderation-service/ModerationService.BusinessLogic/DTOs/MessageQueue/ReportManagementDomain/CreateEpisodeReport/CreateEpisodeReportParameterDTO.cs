using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.MessageQueue.ReportManagementDomain.CreateEpisodeReport
{
    public class CreateEpisodeReportParameterDTO
    {
        public int AccountId { get; set; }
        [Required]
        public Guid PodcastEpisodeId { get; set; }
        [Required]
        public int PodcastEpisodeReportTypeId { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
