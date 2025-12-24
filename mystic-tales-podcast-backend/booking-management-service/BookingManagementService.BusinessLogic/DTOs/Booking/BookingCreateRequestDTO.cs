using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Booking
{
    public class BookingCreateRequestDTO
    {
        public string BookingCreateInfo { get; set; }
        public List<IFormFile> BookingRequirementFiles { get; set; } = new List<IFormFile>();
    }
    public class BookingCreateInfoDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PodcastBuddyId { get; set; }
        public int DeadlineDayCount { get; set; }
        public List<BookingRequirementCreateRequestDTO> BookingRequirementInfo { get; set; } = new List<BookingRequirementCreateRequestDTO>();
    }
}
