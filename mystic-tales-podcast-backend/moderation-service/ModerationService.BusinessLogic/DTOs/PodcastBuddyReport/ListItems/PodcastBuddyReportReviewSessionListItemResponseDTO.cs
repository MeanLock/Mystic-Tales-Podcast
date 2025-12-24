using Microsoft.EntityFrameworkCore.Proxies.Internal;
using ModerationService.BusinessLogic.DTOs.Snippet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.PodcastBuddyReport.ListItems
{
    public class PodcastBuddyReportReviewSessionListItemResponseDTO
    {
        public Guid Id { get; set; }
        public PodcastBuddySnippetResponseDTO PodcastBuddy { get; set; }
        public AssignedStaffSnippetResponseDTO AssignedStaff { get; set; }
        public int ResolvedViolationPoint { get; set; }
        public bool? IsResolved { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
