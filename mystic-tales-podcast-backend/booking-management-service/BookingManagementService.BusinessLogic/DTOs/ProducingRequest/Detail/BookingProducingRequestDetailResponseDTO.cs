using BookingManagementService.BusinessLogic.DTOs.Booking.ListItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingManagementService.BusinessLogic.DTOs.ProducingRequest.ListItems;

namespace BookingManagementService.BusinessLogic.DTOs.ProducingRequest.Detail
{
    public class BookingProducingRequestDetailResponseDTO
    {
        public Guid Id { get; set; }
        public int BookingId { get; set; }
        public string Note { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public int? DeadlineDays { get; set; }
        public bool? IsAccepted { get; set; }
        public string RejectReason { get; set; }
        public DateTime? FinishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<BookingPodcastTrackWithRequirementListItemResponseDTO>? BookingPodcastTracks { get; set; } = new List<BookingPodcastTrackWithRequirementListItemResponseDTO>();
        public List<BookingEditRequirementListItemResponseDTO> EditRequirementList { get; set; } = new List<BookingEditRequirementListItemResponseDTO>();
    }
}
