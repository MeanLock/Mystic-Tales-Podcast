using PodcastService.BusinessLogic.DTOs.Account;

namespace PodcastService.BusinessLogic.DTOs.Show.ListItems
{
    public class PodcastShowReviewListItemResponseDTO
    {
        public Guid Id { get; set; }

        public string? Title { get; set; }

        public string? Content { get; set; }

        public double Rating { get; set; }

        public AccountSnippetResponseDTO Account { get; set; }

        public Guid PodcastShowId { get; set; }

        public DateTime? DeletedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }


}