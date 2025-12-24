namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddPodcasterListenCount
{
    public class AddPodcasterListenCountParameterDTO
    {
        public required int PodcasterId { get; set; }
        public required int ListenCountAmount { get; set; }
    }
}
