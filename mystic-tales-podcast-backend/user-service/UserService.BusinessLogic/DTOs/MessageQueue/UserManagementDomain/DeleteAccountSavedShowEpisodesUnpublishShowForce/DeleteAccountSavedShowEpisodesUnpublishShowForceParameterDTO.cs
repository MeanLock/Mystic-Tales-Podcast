namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedShowEpisodesUnpublishShowForce
{
    public class DeleteAccountSavedShowEpisodesUnpublishShowForceParameterDTO
    {
        public required Guid PodcastShowId { get; set; }
        public required List<Guid> DmcaDismissedEpisodeIds { get; set; }
    }
}
