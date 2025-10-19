namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubtractEpisodeTotalSaved
{
    public class SubtractEpisodeTotalSavedParameterDTO
    {
        public required Guid PodcastEpisodeId { get; set; }
        public List<int> AffectedAccountIds { get; set; } = new List<int>();
    }
}
