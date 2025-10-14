using BookingManagementService.DataAccess.DTOs.Booking.ListItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.DataAccess.DTOs.Booking
{
    public class BookingResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int AccountId { get; set; }
        public int PodcastBuddyId { get; set; }
        public decimal Price { get; set; }
        public DateOnly Deadline { get; set; }
        public string? DemoAudioFileKey { get; set; }
        public string? BookingManualCancelledReason { get; set; }
        public List<BookingNegotiationListItemResponseDTO> BookingNegotiationList { get; set; } = new List<BookingNegotiationListItemResponseDTO>();
        public List<BookingProducingRequestListItemResponseDTO> BookingProducingRequestList { get; set; } = new List<BookingProducingRequestListItemResponseDTO>();
        public string? BookingAutoCancelledReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
