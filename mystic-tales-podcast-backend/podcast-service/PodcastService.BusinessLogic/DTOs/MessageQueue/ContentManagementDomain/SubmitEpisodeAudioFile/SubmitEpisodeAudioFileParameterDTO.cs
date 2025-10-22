namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubmitEpisodeAudioFile
{
    public class SubmitEpisodeAudioFileParameterDTO
    {
        public required Guid PodcastEpisodeId { get; set; }
        public required int PodcasterId { get; set; }
        public required string AudioFileKey { get; set; }
        public required int AudioFileSize { get; set; }
        public required int AudioLength { get; set; }
    }
}
