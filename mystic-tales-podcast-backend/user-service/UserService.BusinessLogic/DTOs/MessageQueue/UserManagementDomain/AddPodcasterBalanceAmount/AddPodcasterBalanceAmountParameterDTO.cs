namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddPodcasterBalanceAmount
{
    public class AddPodcasterBalanceAmountParameterDTO
    {
        public required int PodcasterId { get; set; }
        public required decimal Amount { get; set; }
    }
}
