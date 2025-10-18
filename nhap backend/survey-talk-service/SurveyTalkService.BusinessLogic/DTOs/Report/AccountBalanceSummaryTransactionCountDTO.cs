namespace SurveyTalkService.BusinessLogic.DTOs.Report
{
    public class AccountBalanceSummaryTransactionCountDTO
    {
        public int DepositTransactionCount { get; set; }
        public int WithdrawalTransactionCount { get; set; }
        public double DepositTransactionPercentChange { get; set; }
        public double WithdrawalTransactionPercentChange { get; set; }
    }
}

