using Microsoft.EntityFrameworkCore.Proxies.Internal;
using ModerationService.BusinessLogic.DTOs.PodcastBuddyReport;
using ModerationService.BusinessLogic.DTOs.Snippet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.PodcastEpisodeReport.ListItems
{
    public class PodcastEpisodeReportReviewSessionListItemResponseDTO
    {
        public Guid Id { get; set; }
        public PodcastEpisodeSnippetDTO PodcastEpisode { get; set; }
        public AssignedStaffSnippetDTO AssignedStaff { get; set; }
        public bool? IsResolved { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
