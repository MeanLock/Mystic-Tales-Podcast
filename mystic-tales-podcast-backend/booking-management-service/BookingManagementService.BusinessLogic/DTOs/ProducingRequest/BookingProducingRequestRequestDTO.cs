using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.ProducingRequest
{
    public class BookingProducingRequestRequestDTO
    {
        public BookingProducingRequestInfoDTO BookingProducingRequestInfo { get; set; }
    }
    public class BookingProducingRequestInfoDTO
    {
        public string Note { get; set; }
        public DateOnly Deadline { get; set; }
        public List<Guid> BookingPodcastTrackIds { get; set; } = new List<Guid>();
    }
}
