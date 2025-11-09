using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.ProcessBookingDealing
{
    public class ProcessBookingDealingParameterDTO
    {
        public int BookingId { get; set; }
        public int AccountId { get; set; }
        public List<BookingRequirementDealingInfoObjectParameterDTO> BookingRequirementInfoList { get; set; }
        public int? DeadlineDayCount { get; set; }
    }
    public class BookingRequirementDealingInfoObjectParameterDTO
    {
        public Guid Id { get; set; }
        public int WordCount { get; set; }
    }
}
