namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.UploadEpisodeLicenses
{
    public class UploadEpisodeLicensesParameterDTO
    {
        public required Guid PodcastEpisodeId { get; set; }
        public required List<string> LicenseDocumentFileKeys { get; set; }
        public required int PodcasterId { get; set; }
    }
}
