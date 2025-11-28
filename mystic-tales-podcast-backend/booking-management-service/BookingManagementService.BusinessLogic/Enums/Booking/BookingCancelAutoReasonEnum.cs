using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.Enums.Booking
{
    public enum BookingCancelAutoReasonEnum
    {
        ExpiredPreview = 1,
        PodcastBuddyNoResponse = 2
    }
    public static class BookingCancelAutoReasonEnumExtensions
    {
        public static string GetDescription(this BookingCancelAutoReasonEnum reason)
        {
            return reason switch
            {
                BookingCancelAutoReasonEnum.ExpiredPreview => "ExpiredPreview (quá thời hạn preview và pay the rest)",
                BookingCancelAutoReasonEnum.PodcastBuddyNoResponse => "PodcastBuddyNoResponse (không phản hồi producing request",
                _ => "Unknown reason"
            };
        }
    }
}
