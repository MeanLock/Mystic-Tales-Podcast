namespace UserService.BusinessLogic.DTOs.Account
{
    public class AccountSnippetDTO
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public string? MainImageFileKey { get; set; }
    }
}