namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddAccountBalanceAmountRollback
{
    public class AddAccountBalanceAmountRollbackParameterDTO
    {
        public required int AccountId { get; set; }
        public required decimal Amount { get; set; }
    }
}
