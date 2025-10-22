namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.CreateEpisode
{
    public class CreateEpisodeParameterDTO
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool ExplicitContent { get; set; }
        public string? MainImageFileKey { get; set; }
        public int PodcastEpisodeSubscriptionTypeId { get; set; }
        public Guid PodcastShowId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeOrder { get; set; }
        public List<int> HashtagIds { get; set; } = new List<int>();
        public required int PodcasterId { get; set; }
    }
}
