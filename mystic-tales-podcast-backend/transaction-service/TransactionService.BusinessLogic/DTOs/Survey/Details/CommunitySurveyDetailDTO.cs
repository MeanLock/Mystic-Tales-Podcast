using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TransactionService.BusinessLogic.DTOs.Survey.ListItems;
using TransactionService.DataAccess.Entities;

namespace TransactionService.BusinessLogic.DTOs.Survey.Details
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
