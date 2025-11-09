using BookingManagementService.BusinessLogic.DTOs.Booking.ListItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.ProducingRequest.ListItems
{
    public class BookingEditRequirementListItemResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public BookingPodcastTrackListItemResponseDTO BookingPodcastTrack { get; set; }
    }
}
