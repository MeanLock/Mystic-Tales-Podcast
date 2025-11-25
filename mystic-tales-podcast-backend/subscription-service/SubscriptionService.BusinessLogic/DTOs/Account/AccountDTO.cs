using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.DTOs.Account
{
    public class AccountDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Dob { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public decimal Balance { get; set; }
    }
}
