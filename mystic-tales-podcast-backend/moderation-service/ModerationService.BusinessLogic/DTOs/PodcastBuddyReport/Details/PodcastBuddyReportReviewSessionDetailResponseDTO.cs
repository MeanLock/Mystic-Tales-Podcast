using ModerationService.BusinessLogic.DTOs.PodcastBuddyReport.ListItems;
using ModerationService.BusinessLogic.DTOs.Snippet;

namespace ModerationService.BusinessLogic.DTOs.PodcastBuddyReport.Details
{
    public class PodcastBuddyReportReviewSessionDetailResponseDTO
    {
        public Guid Id { get; set; }
        public PodcastBuddySnippetResponseDTO PodcastBuddy { get; set; }
        public AssignedStaffSnippetResponseDTO AssignedStaff { get; set; }
        public int ResolvedViolationPoint { get; set; }
        public bool? IsResolved { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PodcastBuddyReportListItemResponseDTO>? BuddyReportList { get; set; }
    }
}
