using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.ValidateBookingCancellation
{
    public class ValidateBookingCancellationParameterDTO
    {
        public int AccountId { get; set; }
        public int BookingId { get; set; }
        public bool IsAccepted { get; set; }
        public decimal? CustomerBookingCancelDepositRefundRate { get; set; }
        public decimal? PodcastBuddyBookingCancelDepositRefundRate { get; set; }
    }
}
