namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateBackgroundSoundTrack
{
    public class UpdateBackgroundSoundTrackParameterDTO
    {
        public required Guid PodcastBackgroundSoundTrackId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? MainImageFileKey { get; set; }
        public string? AudioFileKey { get; set; }
    }
}
