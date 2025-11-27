using ModerationService.BusinessLogic.DTOs.PodcastShowReport;
using ModerationService.BusinessLogic.DTOs.Snippet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.DMCAAccusation.ListItems
{
    public class DMCAAccusationListItemResponseDTO
    {
        public int Id { get; set; }
        public string AccuserEmail { get; set; }
        public string AccuserPhone { get; set; }
        public string AccuserFullName { get; set; }
        public string? DismissReason { get; set; }
        public DMCAPodcastShowSnippetResponseDTO? PodcastShow { get; set; }
        public DMCAPodcastEpisodeSnippetResponseDTO? PodcastEpisode { get; set; }
        public AssignedStaffSnippetResponseDTO? AssignedStaff { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DMCAAccusationStatusDTO CurrentStatus { get; set; }
    }
}
