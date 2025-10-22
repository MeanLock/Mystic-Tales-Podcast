namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.ProcessingEpisodeDraftAudio
{
    public class ProcessingEpisodeDraftAudioParameterDTO
    {
        public required Guid PodcastEpisodeId { get; set; }
        public required int PodcasterId { get; set; }
    }
}
