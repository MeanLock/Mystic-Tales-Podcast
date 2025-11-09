using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.ProducingRequest
{
    public class BookingCancelValidationRequestDTO
    {
        public BookingCancelValidationInfoDTO BookingCancelValidationInfo { get; set; }
    }
    public class BookingCancelValidationInfoDTO
    {
        public decimal? CustomerBookingCancelDepositRefundRate { get; set; }
        public decimal? PodcastBuddyBookingCancelDepositRefundRate { get; set; }
    }   
}
