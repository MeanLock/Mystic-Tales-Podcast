using ModerationService.BusinessLogic.DTOs.PodcastBuddyReport.ListItems;
using ModerationService.BusinessLogic.DTOs.PodcastEpisodeReport.ListItems;
using ModerationService.BusinessLogic.DTOs.PodcastShowReport.ListItems;
using ModerationService.BusinessLogic.DTOs.Snippet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.PodcastShowReport.Details
{
    public class PodcastShowReportReviewSessionDetailResponseDTO
    {
        public Guid Id { get; set; }
        public PodcastShowSnippetResponseDTO PodcastShow { get; set; }
        public AssignedStaffSnippetResponseDTO AssignedStaff { get; set; }
        public int ResolvedViolationPoint { get; set; }
        public bool? IsResolved { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PodcastShowReportListItemResponseDTO>? ShowReportList { get; set; }
    }
}
