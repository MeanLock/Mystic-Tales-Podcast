using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateBooking
{
    public class CreateBookingParameterDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int AccountId { get; set; }
        public int PodcastBuddyId { get; set; }
        public int DeadlineDayCount { get; set; }
        public List<BookingRequirementInfoObjectParameterDTO> BookingRequirementInfoList { get; set; }
    }
    public class BookingRequirementInfoObjectParameterDTO {         
        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public Guid PodcastBookingToneId { get; set; }
        public string? RequirementDocumentFileKey { get; set; }
    }
}
