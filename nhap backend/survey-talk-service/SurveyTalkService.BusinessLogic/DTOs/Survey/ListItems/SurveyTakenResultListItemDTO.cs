using System;

namespace SurveyTalkService.BusinessLogic.DTOs.Survey.ListItems
{
    public class SurveyTakenResultListItemDTO
    {
        public int Id { get; set; }

        public bool IsValid { get; set; }

        public string? InvalidReason { get; set; }

        public decimal? MoneyEarned { get; set; }

        public int? XpEarned { get; set; }

        public DateTime CompletedAt { get; set; }

        public TakerListItemDTO? Taker { get; set; }
    }
}
