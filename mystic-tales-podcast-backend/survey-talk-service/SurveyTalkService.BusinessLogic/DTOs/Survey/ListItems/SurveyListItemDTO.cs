namespace SurveyTalkService.BusinessLogic.DTOs.Survey.ListItems
{
    public class SurveyListItemDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? SurveyTopicId { get; set; }
        public int? SurveySpecificTopicId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? SecurityModeId { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime? PublishedAt { get; set; }
        public double TakerBaseRewardPrice { get; set; }
        public string? ConfigJsonString { get; set; }
        public SurveyPrivateDataDTO? SurveyPrivateData { get; set; }
        public int SurveyStatusId { get; set; }
        public string? MainImageUrl { get; set; }
        public string? BackgroundImageUrl { get; set; }
        public int QuestionCount { get; set; }
    }
}
