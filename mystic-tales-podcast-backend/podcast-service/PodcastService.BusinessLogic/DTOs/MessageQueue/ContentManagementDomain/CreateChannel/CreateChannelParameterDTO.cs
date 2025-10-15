namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateChannel
{
    public class CreateChannelParameterDTO
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? MainImageFileKey { get; set; }
        public string? BackgroundImageFileKey { get; set; }
        public int? PodcastCategoryId { get; set; }
        public int? PodcastSubCategoryId { get; set; }
        public List<int> HashtagIds { get; set; } = new List<int>();
        public required int PodcasterId { get; set; }
    }
}
