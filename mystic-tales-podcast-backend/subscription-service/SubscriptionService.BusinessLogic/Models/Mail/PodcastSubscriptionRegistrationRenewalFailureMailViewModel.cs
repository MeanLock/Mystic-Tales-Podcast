using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.Models.Mail
{
    public class PodcastSubscriptionRegistrationRenewalFailureMailViewModel
    {
        public string CustomerFullName { get; set; }
        public decimal Price { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
