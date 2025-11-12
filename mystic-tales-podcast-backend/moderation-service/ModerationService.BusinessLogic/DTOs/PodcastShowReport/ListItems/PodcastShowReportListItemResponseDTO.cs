using ModerationService.BusinessLogic.DTOs.PodcastBuddyReport;
using ModerationService.BusinessLogic.DTOs.PodcastEpisodeReport;
using ModerationService.BusinessLogic.DTOs.Snippet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.PodcastShowReport.ListItems
{
    public class PodcastShowReportListItemResponseDTO
    {
        public Guid Id { get; set; }
        public string? Content { get; set; }
        public AccountSnippetResponseDTO Account { get; set; }
        public PodcastShowSnippetResponseDTO PodcastShow { get; set; }
        public PodcastShowReportTypeDTO PodcastShowReportType { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
