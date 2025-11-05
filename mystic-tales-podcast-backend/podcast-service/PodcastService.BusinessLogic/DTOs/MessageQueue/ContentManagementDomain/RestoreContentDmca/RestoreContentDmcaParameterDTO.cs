namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RestoreContentDmca
{
    public class RestoreContentDmcaParameterDTO
    {
        public required Guid PodcastEpisodeId { get; set; }
        public required Guid PodcastShowId { get; set; }
    }
}
