namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SubtractAccountBalanceAmountRollback
{
    public class SubtractAccountBalanceAmountRollbackParameterDTO
    {
        public required int AccountId { get; set; }
        public required decimal Amount { get; set; }
    }
}
