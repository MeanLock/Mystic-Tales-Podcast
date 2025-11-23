using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.AgreeProducingRequest
{
    public class AgreeProducingRequestParameterDTO
    {
        public int AccountId { get; set; }
        public Guid BookingProducingRequestId { get; set; }
        public bool IsAccepted { get; set; }
        public string? RejectReason { get; set; }
    }
}
