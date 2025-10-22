using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SubscriptionService.BusinessLogic.DTOs.Survey.Details
{
    public class MarketResearchSurveyDetailDTO : SurveyDetailDTO
    {
        public List<VersionTrackingListItemDTO>? VersionTrackings { get; set; }
        public JArray? Questions { get; set; }
    }
}
