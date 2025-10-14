using System.Text.Json.Serialization;

namespace BookingManagementService.BusinessLogic.DTOs.ProducingRequest
{
    public class BookingProducingRequestRequestDTO
    {
        public BookingProducingRequestInfoDTO BookingProducingRequestInfo { get; set; }
    }

    public class BookingProducingRequestInfoDTO
    {
        public string? Note { get; set; }
        public DateOnly Deadline { get; set; }
        public List<Guid> BookingPodcastTrackIds { get; set; } = new List<Guid>();
    }
}
