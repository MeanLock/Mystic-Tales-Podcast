namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubtractShowTotalFollow
{
    public class SubtractShowTotalFollowParameterDTO
    {
        public required Guid PodcastShowId { get; set; }
        public List<int> AffectedAccountIds { get; set; } = new List<int>();
    }
}
