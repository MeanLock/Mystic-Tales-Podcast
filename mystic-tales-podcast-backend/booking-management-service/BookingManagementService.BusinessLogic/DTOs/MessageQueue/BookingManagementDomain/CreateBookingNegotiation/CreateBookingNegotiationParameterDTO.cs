using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.MessageQueue.BookingManagementDomain.CreateBookingNegotiation
{
    public class CreateBookingNegotiationParameterDTO
    {
        public int BookingId { get; set; }
        public int AccountId { get; set; }
        public decimal? Price { get; set; }
        public DateOnly? Deadline { get; set; }
        public string? Note { get; set; }
        public bool? DemoAudioRequired { get; set; }
        public string? DemoAudioFileKey { get; set; }
    }
}
