using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SurveyTalkService.BusinessLogic.DTOs.Survey.Filters
{
    public class SurveyFilterObject
    {
        public int? RequesterId { get; set; }
        public int? SurveyTypeId { get; set; }
        public bool? IsAvailable { get; set; }
        public bool? IsDeletedContain { get; set; }
        // public bool? IsInvalidTakenResultContain { get; set; }
        public bool? IsEndDateExceededContain { get; set; } 
        public List<int>? SurveyStatusIds { get; set; } = new(); 
        public List<int>? SurveyMarketVersionStatusIds { get; set; } = new();
        public int? Version { get; set; }

        public override string ToString()
        {
            return JToken.FromObject(this).ToString();
        }
    }
}
