using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Report
{
    public class BookingIncomeTotalStatisticReportResponseDTO
    {
        public decimal TotalBookingIncomeAmount { get; set; }
        public double TotalBookingIncomePercentChange { get; set; }
    }
}
