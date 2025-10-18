namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.SubtractChannelTotalFavorite
{
    public class SubtractChannelTotalFavoriteParameterDTO
    {
        public required Guid PodcastChannelId { get; set; }
        public List<int> AffectedAccountIds { get; set; } = new List<int>();
    }
}
