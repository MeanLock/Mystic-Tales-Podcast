namespace BookingManagementService.BusinessLogic.DTOs.Booking
{
    public class BookingTrackListenResponseDTO
    {
        public required string PlaylistFileKey { get; set; } = null!;
         public required string? AudioFileUrl { get; set; }
    }
}