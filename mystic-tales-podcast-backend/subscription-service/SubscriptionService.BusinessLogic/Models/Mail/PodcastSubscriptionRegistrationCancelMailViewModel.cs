using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.Models.Mail
{
    public class PodcastSubscriptionRegistrationCancelMailViewModel
    {
        public string CustomerFullName { get; set; }
        public decimal? RefundAmount { get; set; }
        public DateTime CancelledDate { get; set; }
    }
}
