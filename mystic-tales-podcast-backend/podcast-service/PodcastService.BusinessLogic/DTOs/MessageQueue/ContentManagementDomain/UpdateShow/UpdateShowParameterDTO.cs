namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateShow
{
    public class UpdateShowParameterDTO
    {
        public required Guid PodcastShowId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Language { get; set; } = null!;
        public string Copyright { get; set; } = null!;
        public string? UploadFrequency { get; set; }
        public int PodcasterId { get; set; }
        public int? PodcastCategoryId { get; set; }
        public int? PodcastSubCategoryId { get; set; }
        public int PodcastShowSubscriptionTypeId { get; set; }
        // public Guid? PodcastChannelId { get; set; }
        public List<int>? HashtagIds { get; set; }
        public string? MainImageFileKey { get; set; }
    }
}
