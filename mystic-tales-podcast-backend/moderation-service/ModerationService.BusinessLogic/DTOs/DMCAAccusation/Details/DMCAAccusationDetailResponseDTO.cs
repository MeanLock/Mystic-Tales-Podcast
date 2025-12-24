using ModerationService.BusinessLogic.DTOs.CounterNotice.Details;
using ModerationService.BusinessLogic.DTOs.DMCANotice.Details;
using ModerationService.BusinessLogic.DTOs.LawsuitProof.Details;
using ModerationService.BusinessLogic.DTOs.Snippet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.DTOs.DMCAAccusation.Details
{
    public class DMCAAccusationDetailResponseDTO
    {
        public int Id { get; set; }
        public string AccuserEmail { get; set; }
        public string AccuserPhone { get; set; }
        public string AccuserFullName { get; set; }
        public string? DismissReason { get; set; }
        public DMCAPodcastShowSnippetResponseDTO? PodcastShow { get; set; }
        public DMCAPodcastEpisodeSnippetResponseDTO? PodcastEpisode { get; set; }
        public AssignedStaffSnippetResponseDTO? AssignedStaff { get; set; }
        public DateTime? LastLawsuitCheckingAlertAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DMCAAccusationStatusDTO CurrentStatus { get; set; }
        public DMCANoticeDetailResponseDTO DMCANotice { get; set; }
        public CounterNoticeDetailResponseDTO? CounterNotice { get; set; }
        public LawsuitProofDetailResponseDTO? LawsuitProof { get; set; }
    }
}
