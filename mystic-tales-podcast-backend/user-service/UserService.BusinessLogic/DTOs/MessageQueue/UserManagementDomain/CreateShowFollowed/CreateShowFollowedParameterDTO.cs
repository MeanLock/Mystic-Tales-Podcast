namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreateShowFollowed
{
    public class CreateShowFollowedParameterDTO
    {
        public required int AccountId { get; set; }
        public required Guid PodcastShowId { get; set; }
    }
}
