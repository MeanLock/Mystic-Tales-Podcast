namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateBackgroundSoundTrack
{
    public class CreateBackgroundSoundTrackParameterDTO
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? MainImageFileKey { get; set; }
        public string? AudioFileKey { get; set; }
    }
}
