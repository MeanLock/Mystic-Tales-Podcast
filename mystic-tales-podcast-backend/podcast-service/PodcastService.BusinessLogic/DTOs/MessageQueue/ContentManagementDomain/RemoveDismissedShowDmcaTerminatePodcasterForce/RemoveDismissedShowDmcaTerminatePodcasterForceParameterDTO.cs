namespace PodcastService.BusinessLogic.DTOs.MessageQueue.ContentManagementDomain.RemoveDismissedShowDmcaTerminatePodcasterForce
{
    public class RemoveDismissedShowDmcaTerminatePodcasterForceParameterDTO
    {
        public required int PodcasterId { get; set; }
        public required List<Guid> DmcaDismissedShowIds { get; set; }
    }
}
