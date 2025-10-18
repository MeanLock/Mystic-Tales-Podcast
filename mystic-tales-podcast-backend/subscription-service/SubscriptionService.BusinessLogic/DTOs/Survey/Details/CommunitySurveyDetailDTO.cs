using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SubscriptionService.BusinessLogic.DTOs.Survey.ListItems;
using SubscriptionService.DataAccess.Entities;

namespace SubscriptionService.BusinessLogic.DTOs.Survey.Details
{
    public class CommunitySurveyDetailDTO : SurveyDetailDTO
    {
        public int CurrentTakenResultCount { get; set; }
        public int AvailableTakenResultSlot { get; set; }
        public JArray? Questions { get; set; }
        public List<SurveyTakenResultListItemDTO>? SurveyTakenResults { get; set; }
        public List<SurveyRewardTracking>? SurveyRewardTrackings { get; set; }
    }
}
