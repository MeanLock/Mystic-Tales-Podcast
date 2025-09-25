using SurveyTalkService.DataAccess.Entities;

namespace SurveyTalkService.BusinessLogic.DTOs.Transaction
{
    public class SurveyCommunityTransactionDTO
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }
        public decimal? Profit { get; set; }

        public DateTime CreatedAt { get; set; }
        public SurveyCommunityTransactionAccountDTO Account { get; set; } 
        public SurveyCommunityTransactionSurveyDTO Survey { get; set; } 

        public TransactionStatusDTO TransactionStatus { get; set; } = null!;

        public TransactionTypeDTO TransactionType { get; set; } = null!;
    }

    public class SurveyCommunityTransactionAccountDTO
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? MainImageUrl { get; set; }
    }

    public class SurveyCommunityTransactionSurveyDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? MainImageUrl { get; set; }

    }
    

}

