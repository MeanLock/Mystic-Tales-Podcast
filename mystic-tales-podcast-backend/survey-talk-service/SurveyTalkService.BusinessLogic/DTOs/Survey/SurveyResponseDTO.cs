namespace SurveyTalkService.BusinessLogic.DTOs.Survey
{
    public class SurveyResponseDTO
    {
        public int Id { get; set; }
        public int SurveyTakenResultId { get; set; }
        // public int SurveyQuestionId { get; set; }
        // [INT --> GUID]
        public Guid SurveyQuestionId { get; set; }
        public bool IsValid { get; set; }
        public string ValueJsonString { get; set; } = string.Empty;
    }
}
