using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.PodcastSubscription.ListItems
{
    public class PodcastSubscriptionMoneyFlowListItemResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? PodcastShowName { get; set; }
        public string? PodcastChannelName { get; set; }
        public bool IsActive { get; set; }
        public int CurrentVersion { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PodcastSubscriptionRegistrationMoneyFlowListItemResponseDTO> PodcastSubscriptionRegistrationList { get; set; }
    }
}
