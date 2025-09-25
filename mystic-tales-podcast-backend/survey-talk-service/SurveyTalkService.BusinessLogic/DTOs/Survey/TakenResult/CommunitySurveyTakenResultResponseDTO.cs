using System.Collections.Generic;

namespace SurveyTalkService.BusinessLogic.DTOs.Survey.TakenResult
{
    public class CommunitySurveyTakenResultResponseDTO
    {
        public bool IsValid { get; set; }
        public decimal? MoneyEarned { get; set; }
        public int? XpEarned { get; set; }
    }

}
