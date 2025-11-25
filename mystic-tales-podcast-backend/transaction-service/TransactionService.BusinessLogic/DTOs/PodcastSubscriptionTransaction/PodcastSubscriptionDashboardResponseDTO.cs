using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.BusinessLogic.DTOs.PodcastSubscriptionTransaction.ListItems;

namespace TransactionService.BusinessLogic.DTOs.PodcastSubscriptionTransaction
{
    public class PodcastSubscriptionDashboardResponseDTO
    {
        public List<PodcastSubscriptionLast30DashboardListItemResponseDTO> Last30DayList { get; set; } = null!;
        public decimal LastMonthTotalAmount { get; set; }
        public decimal Last3MonthTotalAmount { get; set; }
    }
}
