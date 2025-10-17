using BookingManagementService.BusinessLogic.DTOs.Booking.ListItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingManagementService.BusinessLogic.DTOs.ProducingRequest.ListItems;

namespace BookingManagementService.BusinessLogic.DTOs.ProducingRequest
{
    public class BookingProducingRequestDetailResponseDTO
    {
        public Guid Id { get; set; }
        public int BookingId { get; set; }
        public string Note { get; set; } = string.Empty;
        public DateOnly Deadline { get; set; }
        public bool? IsAccepted { get; set; }
        public DateTime? FinishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<BookingPodcastTrackListItemResponseDTO> BookingPodcastTracks { get; set; } = new List<BookingPodcastTrackListItemResponseDTO>();
    }
}
