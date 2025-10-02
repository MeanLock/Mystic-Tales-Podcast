using System.Collections.Generic;
using SystemConfigurationService.BusinessLogic.DTOs.Survey.TakenResult.V1;

namespace SystemConfigurationService.BusinessLogic.DTOs.Survey.TakenResult
{
    public class SurveyTakingResponseRequestDTO
    {
        public string? InvalidReason { get; set; }
        public List<SurveyTakingResponseDTO>? SurveyResponses { get; set; }
    }

}
