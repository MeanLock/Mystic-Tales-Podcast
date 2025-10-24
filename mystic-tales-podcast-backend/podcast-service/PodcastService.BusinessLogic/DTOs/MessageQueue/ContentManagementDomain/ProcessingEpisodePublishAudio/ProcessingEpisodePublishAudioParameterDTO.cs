namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.ProcessingEpisodePublishAudio
{
    public class ProcessingEpisodePublishAudioParameterDTO
    {
        public required Guid PodcastEpisodeId { get; set; }
        public required int PodcasterId { get; set; }
    }
}
