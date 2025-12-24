namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.AddAccountBalanceAmount
{
    public class AddAccountBalanceAmountParameterDTO
    {
        public required int AccountId { get; set; }
        public required decimal Amount { get; set; }
    }
}
