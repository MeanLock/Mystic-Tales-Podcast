using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.DataAccess.DTOs.Booking.ListItems
{
    public class BookingNegotiationListItemResponseDTO
    {
        public Guid Id { get; set; }
        public int BookingId { get; set; }
        public string Note { get; set; }
        public DateOnly Deadline { get; set; }
        public decimal Price { get; set; }
        public bool DemoAudioRequired { get; set; }
        public string? DemoAudioFileKey { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsFromCustomer { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
