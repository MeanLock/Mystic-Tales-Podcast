using BookingManagementService.BusinessLogic.DTOs.Snippet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Booking.ListItems
{
    public class BookingListItemResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public AccountSnippetResponseDTO Account { get; set; }
        public AccountSnippetResponseDTO PodcastBuddy { get; set; }
        public AccountSnippetResponseDTO? AssignedStaff { get; set; }
        public decimal? Price { get; set; }
        public DateOnly? Deadline { get; set; }
        public int? DeadlineDays { get; set; }
        public string? DemoAudioFileKey { get; set; }
        public string? BookingManualCancelledReason { get; set; }
        public string? BookingAutoCancelledReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public BookingStatusResponseDTO CurrentStatus { get; set; }
    }
}
