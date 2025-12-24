using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.Booking
{
    public class BookingDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int AccountId { get; set; }
        public int PodcastBuddyId { get; set; }
        public decimal? Price { get; set; }
        public DateOnly? Deadline { get; set; }
        public string? DemoAudioFileKey { get; set; }
        public string? BookingManualCancelledReason { get; set; }
        public string? BookingAutoCancelReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? AssignedStaffId { get; set; }
        public double? CustomerBookingCancelDepositRefundRate { get; set; }
        public double? PodcastBuddyBookingCancelDepositRefundRate { get; set; }
        public int? DeadlineDays { get; set; }
    }
}
