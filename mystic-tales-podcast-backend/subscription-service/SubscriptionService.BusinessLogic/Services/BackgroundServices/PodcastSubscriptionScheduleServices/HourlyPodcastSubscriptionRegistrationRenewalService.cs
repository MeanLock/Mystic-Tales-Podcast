using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SubscriptionService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.Services.BackgroundServices.PodcastSubscriptionScheduleServices
{
    public class HourlyPodcastSubscriptionRegistrationRenewalService : BackgroundService
    {
        private readonly ILogger<HourlyPodcastSubscriptionRegistrationRenewalService> _logger;
        public HourlyPodcastSubscriptionRegistrationRenewalService(
            ILogger<HourlyPodcastSubscriptionRegistrationRenewalService> logger
            )
        {
            _logger = logger;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
