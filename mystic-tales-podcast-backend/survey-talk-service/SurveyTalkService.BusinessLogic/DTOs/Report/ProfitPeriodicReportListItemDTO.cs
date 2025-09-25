namespace SurveyTalkService.BusinessLogic.DTOs.Report
{
    public class ProfitPeriodicReportListItemDTO
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal Revenue { get; set; }
    }
}

