namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountSavedChannelEpisodesUnpublishChannelForce
{
    public class DeleteAccountSavedChannelEpisodesUnpublishChannelForceParameterDTO
    {
        public required Guid PodcastChannelId { get; set; }
        public required List<Guid> DmcaDismissedEpisodeIds { get; set; }
    }
}
