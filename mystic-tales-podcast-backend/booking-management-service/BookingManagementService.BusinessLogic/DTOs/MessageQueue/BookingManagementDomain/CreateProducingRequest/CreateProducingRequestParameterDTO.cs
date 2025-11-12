using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateProducingRequest
{
    public class CreateProducingRequestParameterDTO
    {
        public int AccountId { get; set; }
        public int BookingId { get; set; }
        public string Note { get; set; }
        public int DeadlineDayCount { get; set; }
        public List<Guid> BookingPodcastTrackIds { get; set; }
    }
}
