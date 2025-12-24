using BookingManagementService.BusinessLogic.DTOs.Cache.ListesnSessionProcedure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Booking
{
    public class BookingListenSessionResponseDTO
    {
        public BookingTrackListenResponseDTO? ListenSession { get; set; }
        public CustomerListenSessionProcedure? ListenSessionProcedure { get; set; }
    }
}
