namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentModerationDomain.RequestEpisodeAudioExamination
{
    public class RequestEpisodeAudioExaminationParameterDTO
    {
        public required Guid PodcastEpisodeId { get; set; }
        public required int PodcasterId { get; set; }
    }
}
