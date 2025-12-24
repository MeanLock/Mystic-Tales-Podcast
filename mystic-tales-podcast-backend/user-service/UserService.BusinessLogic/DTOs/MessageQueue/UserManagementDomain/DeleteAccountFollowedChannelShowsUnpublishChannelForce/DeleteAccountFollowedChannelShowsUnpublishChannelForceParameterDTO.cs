namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeleteAccountFollowedChannelShowsUnpublishChannelForce
{
    public class DeleteAccountFollowedChannelShowsUnpublishChannelForceParameterDTO
    {
        public required Guid PodcastChannelId { get; set; }
        public required List<Guid> DmcaDismissedShowIds { get; set; }
    }
}
