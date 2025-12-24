using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.Snippet
{
    public class PodcastBuddySnippetResponseDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? MainImageFileKey { get; set; }
        public decimal? PriceBookingPerWord { get; set; }
    }
}
