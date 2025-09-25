namespace SurveyTalkService.BusinessLogic.DTOs.Transaction
{
    public class AccountBalanceWithdrawalDTO
    {
        public decimal Amount { get; set; }
        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}

