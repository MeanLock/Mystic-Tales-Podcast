using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.Enums.Booking
{
    public enum BookingStatusEnum
    {
        QuotationUnderNegotiation = 1,
        QuotationDealing = 2,
        QuotationRejected = 3,
        QuotationCancelled = 4,
        Producing = 5,
        TrackPreviewing = 6,
        ProducingRequested = 7,
        Completed = 8,
        CancelledAutomatically = 9,
        CancelledManually = 10
    }
}
