using BookingManagementService.BusinessLogic.DTOs.Booking.ListItems;
using BookingManagementService.BusinessLogic.DTOs.ProducingRequest.ListItems;
using BookingManagementService.BusinessLogic.DTOs.Snippet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Booking.Details
{
    public class BookingResultDetailResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public AccountSnippetResponseDTO Account { get; set; }
        public PodcastBuddySnippetResponseDTO PodcastBuddy { get; set; }
        public AccountSnippetResponseDTO? AssignedStaff { get; set; }
        public decimal? Price { get; set; }
        public DateOnly? Deadline { get; set; }
        public int? DeadlineDays { get; set; }
        public string? DemoAudioFileKey { get; set; }
        public string? BookingManualCancelledReason { get; set; }
        public List<BookingRequirementListItemResponseDTO>? BookingRequirementFileList { get; set; } = new List<BookingRequirementListItemResponseDTO>();
        public List<BookingPodcastTrackListItemResponseDTO>? BookingPodcastTrackList { get; set; } = new List<BookingPodcastTrackListItemResponseDTO>();
        public string? BookingAutoCancelledReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public BookingStatusResponseDTO CurrentStatus { get; set; }
    }
}
