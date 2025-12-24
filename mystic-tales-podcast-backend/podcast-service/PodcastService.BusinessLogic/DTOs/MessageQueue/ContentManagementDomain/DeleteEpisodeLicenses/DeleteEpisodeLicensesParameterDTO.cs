namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.DeleteEpisodeLicenses
{
    public class DeleteEpisodeLicensesParameterDTO
    {
        public required Guid PodcastEpisodeId { get; set; }
        public required List<Guid> PodcastEpisodeLicenseIds { get; set; }
        public required int PodcasterId { get; set; }
    }
}
