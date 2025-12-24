namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.DeletePodcasterFollowed
{
    public class DeletePodcasterFollowedParameterDTO
    {
        public int? AccountId { get; set; }
        public required int PodcastBuddyId { get; set; }
    }
}
