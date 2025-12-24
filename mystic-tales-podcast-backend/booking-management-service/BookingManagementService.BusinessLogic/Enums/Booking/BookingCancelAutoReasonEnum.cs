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
                BookingCancelAutoReasonEnum.ExpiredPreview => "Track Previewing duration has expired",
                BookingCancelAutoReasonEnum.PodcastBuddyNoResponse => "Podcaster has not response in time",
                BookingCancelAutoReasonEnum.TerminatePodcasterGiven => "Podcaster has been terminate",
                BookingCancelAutoReasonEnum.TerminatePodcasterTaken => "Customer has been terminate",
                _ => "Unknown reason"
            };
        }
    }
}
