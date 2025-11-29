namespace UserService.BusinessLogic.DTOs.Account.ListItems
{
    public class PodcastBuddyListItemResponseDTO
    {
        public PodcastBuddyProfileDTO PodcastBuddyProfile { get; set; } = null!;
        public required PodcastBuddyAccountDTO PodcastBuddyAccount { get; set; } = null!;
        public List<PodcastBuddyReviewListItemResponseDTO> ReviewList { get; set; } = new List<PodcastBuddyReviewListItemResponseDTO>();

    }

    public class PodcastBuddyAccountDTO
    {
        public string MainImageFileKey { get; set; } = null!;
    }


}