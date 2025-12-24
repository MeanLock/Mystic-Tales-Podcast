namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateChannelFavoritedRollback
{
    public class CreateChannelFavoritedRollbackParameterDTO
    {
        public required int AccountId { get; set; }
        public required Guid PodcastChannelId { get; set; }
    }
}
