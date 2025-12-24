namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.TakedownContentDmca
{
    public class TakedownContentDmcaParameterDTO
    {
        public Guid? PodcastEpisodeId { get; set; }
        public Guid? PodcastShowId { get; set; }
        public string TakenDownReason { get; set; } = string.Empty;
    }
}
