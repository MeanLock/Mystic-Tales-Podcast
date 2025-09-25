namespace SurveyTalkService.BusinessLogic.DTOs.Transaction
{
    public class AccountBalanceDepositDTO
    {
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }
        public string? CancelUrl { get; set; }
    }
}

