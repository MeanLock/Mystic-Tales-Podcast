namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedEpisodeDmcaTerminatePodcasterForce
{
    public class RemoveDismissedEpisodeDmcaTerminatePodcasterForceParameterDTO
    {
        public required int PodcasterId { get; set; }
        public required List<Guid> DmcaDismissedEpisodeIds { get; set; }
    }
}
