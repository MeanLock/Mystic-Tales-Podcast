using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.ProducingRequest
{
    public class BookingCancelRequestRequestDTO
    {
        public int AccountId { get; set; }
        public string BookingManualCancelledReason { get; set; }
    }
}
