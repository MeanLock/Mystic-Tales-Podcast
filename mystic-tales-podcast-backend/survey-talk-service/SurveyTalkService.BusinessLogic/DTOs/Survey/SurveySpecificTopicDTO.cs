namespace SurveyTalkService.BusinessLogic.DTOs.Survey
{
    public class SurveySpecificTopicDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int SurveyTopicId { get; set; }
    }
}
