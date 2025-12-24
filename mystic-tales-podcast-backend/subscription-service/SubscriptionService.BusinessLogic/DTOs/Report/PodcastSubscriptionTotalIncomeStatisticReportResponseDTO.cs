using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.Report
{
    public class PodcastSubscriptionTotalIncomeStatisticReportResponseDTO
    {
        public decimal TotalPodcastSubscriptionIncomeAmount { get; set; }
        public double TotalPodcastSubscriptionIncomePercentChange { get; set; }
    }
}
