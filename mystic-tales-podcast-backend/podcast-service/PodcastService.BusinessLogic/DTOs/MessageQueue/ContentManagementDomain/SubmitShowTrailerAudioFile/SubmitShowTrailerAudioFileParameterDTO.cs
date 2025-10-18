namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubmitShowTrailerAudioFile
{
    public class SubmitShowTrailerAudioFileParameterDTO
    {
        public required Guid PodcastShowId { get; set; }
        public required int PodcasterId { get; set; }
        public required string TrailerAudioFileKey { get; set; }
    }
}
