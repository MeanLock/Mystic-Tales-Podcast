using BookingManagementService.BusinessLogic.DTOs.Snippet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Booking.ListItems
{
    public class BookingPodcastTrackWithRequirementListItemResponseDTO
    {
        public Guid Id { get; set; }
        public int BookingId { get; set; }
        public BookingRequirementSnippetResponseDTO BookingRequirement { get; set; }
        public Guid BookingProducingRequestId { get; set; }
        public string AudioFileKey { get; set; }
        public double AudioFileSize { get; set; }
        public int AudioLength { get; set; }
        public int RemainingPreviewListenSlot { get; set; }
    }
}
