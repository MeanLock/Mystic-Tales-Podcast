using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SurveyTalkService.BusinessLogic.DTOs.Survey.Details
{
    public class FilterSurveyDetailDTO : SurveyDetailDTO
    {
        public int CurrentTakenResultCount { get; set; }
        public JArray? Questions { get; set; }
    }
}
