namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.TakedownContentDmca
{
    public class TakedownContentDmcaParameterDTO
    {
        public required Guid PodcastEpisodeId { get; set; }
        public required Guid PodcastShowId { get; set; }
        public string TakenDownReason { get; set; } = string.Empty;
    }
}
