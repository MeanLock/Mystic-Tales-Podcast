using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.ProducingRequest.ListItems
{
    public class BookingProducingRequestListItemResponseDTO
    {
        public Guid Id { get; set; }
        public int BookingId { get; set; }
        public string Note { get; set; } = null!;
        public DateOnly Deadline { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime FinishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
