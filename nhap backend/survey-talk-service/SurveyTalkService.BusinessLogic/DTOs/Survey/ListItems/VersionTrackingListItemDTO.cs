namespace SurveyTalkService.BusinessLogic.DTOs.Survey
{
    public class VersionTrackingListItemDTO
    {
        public int Version { get; set; }
        public int SurveyVersionStatusId { get; set; }
        public int ContributorCount { get; set; }
        public decimal CurrentVersionPrice { get; set; }
        public string? ExpiredAt { get; set; }
        public string? PublishAt { get; set; }
    }
}