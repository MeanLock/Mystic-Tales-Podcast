using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Transaction
{
    public class BookingTransactionDTO
    {
        public Guid Id { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public decimal? Profit { get; set; }
        public int TransactionTypeId { get; set; }
        public int TransactionStatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
