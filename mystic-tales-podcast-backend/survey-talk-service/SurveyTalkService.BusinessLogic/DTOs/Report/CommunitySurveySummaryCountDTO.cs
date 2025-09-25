namespace SurveyTalkService.BusinessLogic.DTOs.Report
{
    public class CommunitySurveySummaryCountDTO
    {
        public int Published { get; set; }
        public int OnDeadline { get; set; }
        public int NearDeadline { get; set; }
        public int LateForDeadline { get; set; }
        public int Achieved { get; set; }
    }
}

