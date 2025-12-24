namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddPodcasterBalanceAmountRollback
{
    public class AddPodcasterBalanceAmountRollbackParameterDTO
    {
        public required int PodcasterId { get; set; }
        public required decimal Amount { get; set; }
    }
}
