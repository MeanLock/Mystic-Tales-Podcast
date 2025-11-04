namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateEpisodeListenSessionDuration
{
    public class UpdateEpisodeListenSessionDurationParameterDTO
    {
        public required Guid PodcastEpisodeListenSessionId { get; set; }
        public required int ListenerId { get; set; }
        public required int LastListenDurationSeconds { get; set; }
    }
}
