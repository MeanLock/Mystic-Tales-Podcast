using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Booking
{
    public class BookingDealingRequestDTO
    {
        public BookingDealingInfoDTO BookingDealingInfo { get; set; }
    }
    public class BookingDealingInfoDTO
    {
        public List<BookingRequirementDealingRequestDTO> BookingRequirementInfoList { get; set; }
        public int DeadlineDayCount { get; set; }
    }
}
