namespace UserService.BusinessLogic.DTOs.Account
{
    public class ReviewListItemDTO
    {
        public Guid Id { get; set; }

        public string? Title { get; set; }

        public string? Content { get; set; }

        public double Rating { get; set; }

        public AccountSnippetDTO Account { get; set; }

        public int PodcastBuddyId { get; set; }

        public DateTime? DeletedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }


}