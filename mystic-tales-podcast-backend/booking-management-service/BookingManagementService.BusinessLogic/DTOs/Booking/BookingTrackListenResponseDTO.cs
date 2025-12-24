using BookingManagementService.BusinessLogic.DTOs.Snippet;

namespace BookingManagementService.BusinessLogic.DTOs.Booking
{
    public class BookingTrackListenResponseDTO
    {
        public BookingListenSnippetResponseDTO Booking { get; set; }
        public BookingPodcastTrackListenSnippetResponseDTO BookingPodcastTrack { get; set; }
        public BookingPodcastTrackListenSessionSnippetResponseDTO BookingPodcastTrackListenSession { get; set; }
        public required string PlaylistFileKey { get; set; } = null!;
        public required string? AudioFileUrl { get; set; }
    }
}