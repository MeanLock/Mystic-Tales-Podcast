using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Report
{
    public class BookingIncomeStatisticReportListItemResponseDTO
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal Amount { get; set; }
    }
}
