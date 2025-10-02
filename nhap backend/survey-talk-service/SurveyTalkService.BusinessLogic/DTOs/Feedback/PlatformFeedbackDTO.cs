namespace SurveyTalkService.BusinessLogic.DTOs.Feedback
{
    public class PlatformFeedbackDTO
    {
        public int AccountId { get; set; }

        public double RatingScore { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

}
