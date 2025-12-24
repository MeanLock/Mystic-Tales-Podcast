using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.Report
{
    public class PodcastSubscriptionDashboardResponseDTO
    {
        public List<PodcastSubscriptionLast30DashboardListItemResponseDTO> Last30DayList { get; set; } = null!;
        public decimal LastMonthTotalAmount { get; set; }
        public decimal Last3MonthTotalAmount { get; set; }
    }
}
