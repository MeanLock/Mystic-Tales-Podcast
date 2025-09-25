namespace SurveyTalkService.BusinessLogic.DTOs.Report
{
    public class AccountBalancePeriodicTransactionAmountListItemDTO
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal DepositTransactionAmount { get; set; }
        public decimal WithdrawalTransactionAmount { get; set; }
    }
}

