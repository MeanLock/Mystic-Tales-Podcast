using ModerationService.BusinessLogic.DTOs.Snippet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.PodcastBuddyReport.ListItems
{
    public class PodcastBuddyReportListItemResponseDTO
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public AccountSnippetResponseDTO Account { get; set; }
        public PodcastBuddySnippetResponseDTO PodcastBuddy { get; set; }
        public PodcastBuddyReportTypeDTO PodcastBuddyReportType { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
