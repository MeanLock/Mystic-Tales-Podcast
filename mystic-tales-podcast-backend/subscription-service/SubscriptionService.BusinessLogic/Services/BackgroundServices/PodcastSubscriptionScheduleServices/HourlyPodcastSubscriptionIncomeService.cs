using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SubscriptionService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices;
using SubscriptionService.BusinessLogic.Services.DbServices.SubscriptionServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.Services.BackgroundServices.PodcastSubscriptionScheduleServices
{
    public class HourlyPodcastSubscriptionIncomeService : BackgroundService
    {
        // LOGGER
        private readonly ILogger<HourlyPodcastSubscriptionIncomeService> _logger;

        // SERVICE PROVIDER
        private readonly IServiceProvider _serviceProvider;
        public HourlyPodcastSubscriptionIncomeService(
            ILogger<HourlyPodcastSubscriptionIncomeService> logger,
            IServiceProvider serviceProvider
            )
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HourlyPodcastSubscriptionIncomeService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking Podcast Subscription Income Service requests...");
                Console.WriteLine("\n\n----Checking Podcast Subscription Income Service requests----\n\n");
                try
                {
                    await PodcastSubscriptionIncomeCheckingByHour();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the requests.");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
        public async Task PodcastSubscriptionIncomeCheckingByHour()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var podcastSubscriptionService = scope.ServiceProvider.GetRequiredService<PodcastSubscriptionService>();
                await podcastSubscriptionService.PodcastSubscriptionIncomeCheckingByHourAsync();
            }
        }
    }
}
