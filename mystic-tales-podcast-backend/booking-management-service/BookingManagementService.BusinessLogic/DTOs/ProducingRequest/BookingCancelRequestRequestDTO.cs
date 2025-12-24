using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.ProducingRequest
{
    public class BookingCancelRequestRequestDTO
    {
        public BookingCancelInfoDTO BookingCancelInfo { get; set; }
    }
    public class BookingCancelInfoDTO
    {
        public string BookingManualCancelledReason { get; set; }
    }
}
