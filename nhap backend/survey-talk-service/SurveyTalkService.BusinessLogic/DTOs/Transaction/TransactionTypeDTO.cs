namespace SurveyTalkService.BusinessLogic.DTOs.Transaction
{
    public partial class TransactionTypeDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string OperationType { get; set; } = null!;

    }
}