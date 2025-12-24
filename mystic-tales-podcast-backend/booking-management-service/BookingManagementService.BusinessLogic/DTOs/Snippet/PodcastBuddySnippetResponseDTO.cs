using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Snippet
{
    public class PodcastBuddySnippetResponseDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string? MainImageFileKey { get; set; }
        public double? AverageRating { get; set; }
        public int RatingCount { get; set; }
        public int TotalFollow { get; set; }
        public decimal? PriceBookingPerWord { get; set; }
        public int TotalBookingCompleted { get; set; }
    }
}
