namespace PodcastService.BusinessLogic.DTOs.Show
{
    public class ShowPublishRequestDTO
    {
        public required ShowPublishInfoDTO ShowPublishInfo { get; set; }
    }

    public class ShowPublishInfoDTO
    {
        public DateOnly? ReleaseDate { get; set; }
    }
}