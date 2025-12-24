using ModerationService.BusinessLogic.DTOs.PodcastBuddyReport;
using ModerationService.BusinessLogic.DTOs.PodcastShowReport;
using ModerationService.BusinessLogic.DTOs.Snippet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.PodcastEpisodeReport.ListItems
{
    public class PodcastEpisodeReportListItemResponseDTO
    {
        public Guid Id { get; set; }
        public string? Content { get; set; }
        public AccountSnippetResponseDTO Account { get; set; }
        public PodcastEpisodeSnippetResponseDTO PodcastEpisode { get; set; }
        public PodcastEpisodeReportTypeDTO PodcastEpisodeReportType { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
