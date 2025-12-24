namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RestoreContentDmca
{
    public class RestoreContentDmcaParameterDTO
    {
        public Guid? PodcastEpisodeId { get; set; }
        public Guid? PodcastShowId { get; set; }
    }
}
