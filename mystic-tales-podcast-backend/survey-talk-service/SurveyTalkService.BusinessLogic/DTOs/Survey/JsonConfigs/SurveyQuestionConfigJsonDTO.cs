namespace SurveyTalkService.BusinessLogic.DTOs.Survey.JsonConfigs
{
    public class SurveyQuestionConfigJsonDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? SurveyTopicId { get; set; }
        public int? SurveySpecificTopicId { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public int? SecurityModeId { get; set; }
        public bool IsAvailable { get; set; }
        public string? PublishedAt { get; set; }
        public SurveyPrivateDataDTO? SurveyPrivateData { get; set; }
        public int SurveyStatusId { get; set; }
        public string? MainImageUrl { get; set; }
        public string? BackgroundImageUrl { get; set; }
        public int QuestionCount { get; set; }
    }
}
