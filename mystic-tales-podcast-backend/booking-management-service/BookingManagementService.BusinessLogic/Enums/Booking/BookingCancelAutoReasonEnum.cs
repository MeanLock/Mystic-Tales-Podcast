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
        PodcastBuddyNoResponse = 2,
        TerminatePodcasterGiven = 3,
        TerminatePodcasterTaken = 4
    }
    public static class BookingCancelAutoReasonEnumExtensions
    {
        public static string GetDescription(this BookingCancelAutoReasonEnum reason)
        {
            return reason switch
            {
                BookingCancelAutoReasonEnum.ExpiredPreview => "ExpiredPreview (quá thời hạn preview và pay the rest)",
                BookingCancelAutoReasonEnum.PodcastBuddyNoResponse => "PodcastBuddyNoResponse (không phản hồi producing request",
                BookingCancelAutoReasonEnum.TerminatePodcasterGiven => "TerminatePodcaster (podcaster bị terminate)",
                BookingCancelAutoReasonEnum.TerminatePodcasterTaken => "TerminatePodcaster (account là podcaster bị terminate)",
                _ => "Unknown reason"
            };
        }
    }
}
