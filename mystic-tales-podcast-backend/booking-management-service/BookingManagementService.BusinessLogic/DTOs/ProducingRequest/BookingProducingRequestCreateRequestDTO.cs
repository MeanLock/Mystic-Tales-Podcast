using System.Text.Json.Serialization;

namespace BookingManagementService.BusinessLogic.DTOs.ProducingRequest
{
    public class BookingProducingRequestCreateRequestDTO
    {
        public BookingProducingRequestInfoDTO BookingProducingRequestInfo { get; set; }
    }

    public class BookingProducingRequestInfoDTO
    {
        public string? Note { get; set; }
        public int DeadlineDayCount { get; set; }
        public List<Guid> BookingPodcastTrackIds { get; set; } = new List<Guid>();
    }
}
