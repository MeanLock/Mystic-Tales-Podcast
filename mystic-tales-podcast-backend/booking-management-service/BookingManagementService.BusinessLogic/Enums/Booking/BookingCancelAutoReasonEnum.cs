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
                BookingCancelAutoReasonEnum.ExpiredPreview => "Customer đã vượt quá thời hạn xem preview",
                BookingCancelAutoReasonEnum.PodcastBuddyNoResponse => "Podcaster không phản hồi kịp trong thời gian quy định",
                BookingCancelAutoReasonEnum.TerminatePodcasterGiven => "Podcaster đã bị terminate",
                BookingCancelAutoReasonEnum.TerminatePodcasterTaken => "Customer đã bị terminate",
                _ => "Unknown reason"
            };
        }
    }
}
