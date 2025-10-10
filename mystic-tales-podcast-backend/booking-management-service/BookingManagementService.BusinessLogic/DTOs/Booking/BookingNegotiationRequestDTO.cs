using Microsoft.AspNetCore.Http;

namespace BookingManagementService.BusinessLogic.DTOs.Booking
{
    public class BookingNegotiationRequestDTO
    {
        public required string BookingNegotiationInfo { get; set; }
        public IFormFile? DemoAudioFile { get; set; }
    }
    public class BookingNegotiationInfoDTO
    {
        public string Note { get; set; }
        public decimal? Price { get; set; }
        public DateOnly? Deadline { get; set; }
        public bool DemoAudioRequired { get; set; }
    }
}