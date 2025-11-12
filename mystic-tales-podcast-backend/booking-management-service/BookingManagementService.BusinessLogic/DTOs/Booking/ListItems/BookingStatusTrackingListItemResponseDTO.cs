using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Booking.ListItems
{
    public class BookingStatusTrackingListItemResponseDTO
    {
        public Guid Id { get; set; }
        public int BookingId { get; set; }
        public int BookingStatusId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
