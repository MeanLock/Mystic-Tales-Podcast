using System.Collections.Generic;
using ModerationService.BusinessLogic.DTOs.Survey.TakenResult.V1;

namespace ModerationService.BusinessLogic.DTOs.Survey.TakenResult
{
    public class SurveyTakingResponseRequestDTO
    {
        public string? InvalidReason { get; set; }
        public List<SurveyTakingResponseDTO>? SurveyResponses { get; set; }
    }

}
