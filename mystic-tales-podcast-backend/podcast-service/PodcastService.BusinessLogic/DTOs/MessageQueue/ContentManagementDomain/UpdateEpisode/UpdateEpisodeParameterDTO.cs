namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UpdateEpisode
{
    public class UpdateEpisodeParameterDTO
    {
        public required Guid PodcastEpisodeId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool ExplicitContent { get; set; }
        public string? MainImageFileKey { get; set; }
        public int PodcastEpisodeSubscriptionTypeId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeOrder { get; set; }
        public List<int> HashtagIds { get; set; } = new List<int>();
        public required int PodcasterId { get; set; }
    }
}
