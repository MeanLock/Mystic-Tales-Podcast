using PodcastService.DataAccess.Entities;

namespace PodcastService.BusinessLogic.DTOs.Survey.ListItems
{

    public class CommunitySurveyListItemDTO : SurveyListItemDTO
    {
        public int CurrentTakenResultCount { get; set; }
        public int AvailableTakenResultSlot { get; set; }
        public SurveyRewardTracking CurrentSurveyRewardTracking { get; set; }
    }
}
