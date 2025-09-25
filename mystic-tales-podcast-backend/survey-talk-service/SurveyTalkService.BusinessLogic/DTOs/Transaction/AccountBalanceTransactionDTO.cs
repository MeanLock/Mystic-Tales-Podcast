using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.BusinessLogic.DTOs.Transaction
{
    public class AccountBalanceTransactionDTO
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; }
        public AccountBalanceTransactionDTOAccountDTO Account { get; set; }

        public virtual TransactionStatusDTO TransactionStatus { get; set; } = null!;

        public virtual TransactionTypeDTO TransactionType { get; set; } = null!;
    }
    
    public class AccountBalanceTransactionDTOAccountDTO
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; } 
        public string? Phone { get; set; }
        public string? MainImageUrl { get; set; }
    }
}

