namespace SurveyTalkService.BusinessLogic.DTOs.Survey
{
    public class SurveyPrivateDataDTO
    {
        public int? RequesterId { get; set; }
        public int? SurveyTypeId { get; set; }
        public int Kpi { get; set; }
        public double? TheoryPrice { get; set; }
        public double ExtraPrice { get; set; }
        public double? ProfitPrice { get; set; }
        public double? AllocBaseAmount { get; set; }
        public double? AllocTimeAmount { get; set; }
        public double? AllocLevelAmount { get; set; }
        public int? MaxXp { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
