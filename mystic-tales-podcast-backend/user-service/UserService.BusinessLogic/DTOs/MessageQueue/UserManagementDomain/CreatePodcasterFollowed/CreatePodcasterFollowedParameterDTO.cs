namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.CreatePodcasterFollowed
{
    public class CreatePodcasterFollowedParameterDTO
    {
        public required int AccountId { get; set; }
        public required int PodcastBuddyId { get; set; }
    }
}
