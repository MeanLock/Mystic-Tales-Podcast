namespace UserService.BusinessLogic.DTOs.Account.ListItems
{
    public class PodcastBuddyListItemResponseDTO
    {
        public PodcastBuddyProfileDTO PodcastBuddyProfile { get; set; } = null!;
        public List<PodcastBuddyReviewListItemResponseDTO> ReviewList { get; set; } = new List<PodcastBuddyReviewListItemResponseDTO>();

    }


}