using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.BusinessLogic.DTOs.Survey.ListItems
{

    public class CommunitySurveyListItemDTO : SurveyListItemDTO
    {
        public int CurrentTakenResultCount { get; set; }
        public int AvailableTakenResultSlot { get; set; }
        public SurveyRewardTracking CurrentSurveyRewardTracking { get; set; }
    }
}
