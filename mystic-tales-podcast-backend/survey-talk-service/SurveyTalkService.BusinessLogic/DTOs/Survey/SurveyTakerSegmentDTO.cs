namespace SurveyTalkService.BusinessLogic.DTOs.Survey
{
    public class SurveyTakerSegmentDTO
    {
        public string? CountryRegion { get; set; }

        public string? MaritalStatus { get; set; }

        public string? AverageIncome { get; set; }

        public string? EducationLevel { get; set; }

        public string? JobField { get; set; }

        public string? Prompt { get; set; }

        public double? TagFilterAccuracyRate { get; set; }
    }
}
