namespace SurveyTalkService.BusinessLogic.DTOs.Feedback
{
    public class PlatformFeedbackRequestDTO
    {
        public PlatformFeedbackRequestFeedbackDTO Feedback { get; set; } = null!;
    }

    public class PlatformFeedbackRequestFeedbackDTO
    {
        public double RatingScore { get; set; }
        public string? Comment { get; set; }
    }
}
