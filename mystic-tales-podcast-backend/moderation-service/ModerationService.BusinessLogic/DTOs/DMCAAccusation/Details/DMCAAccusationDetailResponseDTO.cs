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
        public PodcastShowSnippetDTO? PodcastShow { get; set; }
        public PodcastEpisodeSnippetDTO? PodcastEpisode { get; set; }
        public AssignedStaffSnippetDTO? AssignedStaff { get; set; }
        public DateTime? LastLawsuitCheckingAlertAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DMCAAccusationStatusDTO CurrentStatus { get; set; }
        public DMCANoticeDetailResponseDTO DMCANotice { get; set; }
        public CounterNoticeDetailResponsDTO? CounterNotice { get; set; }
        public LawsuitProofDetailResponseDTO? LawsuitProof { get; set; }
    }
}
