namespace UserService.BusinessLogic.DTOs.Account.ListItems
{
    public class PodcastBuddyListItemResponseDTO
    {
        public PodcastBuddyProfileDTO PodcastBuddyProfile { get; set; } = null!;
        public List<ReviewListItemDTO> ReviewList { get; set; } = new List<ReviewListItemDTO>();

    }


}