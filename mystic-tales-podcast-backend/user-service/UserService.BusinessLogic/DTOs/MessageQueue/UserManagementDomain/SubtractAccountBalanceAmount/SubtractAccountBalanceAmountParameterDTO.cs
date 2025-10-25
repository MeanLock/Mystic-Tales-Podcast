namespace UserService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.SubtractAccountBalanceAmount
{
    public class SubtractAccountBalanceAmountParameterDTO
    {
        public required int AccountId { get; set; }
        public required decimal Amount { get; set; }
    }
}
