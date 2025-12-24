using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.BusinessLogic.DTOs.PodcastSubscriptionTransaction.ListItems
{
    public class PodcastSubscriptionLast30DashboardListItemResponseDTO
    {
        public DateOnly Date { get; set; }
        public decimal Amount { get; set; }
    }
}
