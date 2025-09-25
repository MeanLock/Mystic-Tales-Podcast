namespace SurveyTalkService.BusinessLogic.DTOs.Survey
{
    public class SurveyQuestionTypeDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public decimal Price { get; set; }

        public DateTime? DeactivatedAt { get; set; }
    }
}
