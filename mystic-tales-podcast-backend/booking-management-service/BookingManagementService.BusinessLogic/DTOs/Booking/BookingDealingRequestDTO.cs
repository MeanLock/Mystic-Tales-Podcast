using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Booking
{
    public class BookingDealingRequestDTO
    {
        public List<BookingRequirementDealingRequestDTO> BookingRequirementInfoList { get; set; }
        public decimal Price { get; set; }
        public DateTime? Deadline { get; set; }
    }
}
