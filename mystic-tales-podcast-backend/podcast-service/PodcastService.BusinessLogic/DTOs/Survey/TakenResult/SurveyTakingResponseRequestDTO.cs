using System.Collections.Generic;
using PodcastService.BusinessLogic.DTOs.Survey.TakenResult.V1;

namespace PodcastService.BusinessLogic.DTOs.Survey.TakenResult
{
    public class SurveyTakingResponseRequestDTO
    {
        public string? InvalidReason { get; set; }
        public List<SurveyTakingResponseDTO>? SurveyResponses { get; set; }
    }

}
