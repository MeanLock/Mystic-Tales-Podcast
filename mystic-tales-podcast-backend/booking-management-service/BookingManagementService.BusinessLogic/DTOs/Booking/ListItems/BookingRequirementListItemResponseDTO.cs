using BookingManagementService.BusinessLogic.DTOs.Booking.Details;
using BookingManagementService.DataAccess.Entities.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Booking.ListItems
{
    public class BookingRequirementListItemResponseDTO
    {
        public Guid Id { get; set; }
        public int BookingId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string RequirementDocumentFileKey { get; set; }
        public int Order { get; set; }
        public int WordCount { get; set; }
        public PodcastBookingToneDetailResponseDTO PodcastBookingTone { get; set; }
    }
}
