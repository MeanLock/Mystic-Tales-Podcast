using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModerationService.BusinessLogic.DTOs.DMCAAccusation.Details;
using ModerationService.BusinessLogic.Helpers.DateHelpers;
using ModerationService.BusinessLogic.Services.DbServices.DMCAServices;
using ModerationService.Common.AppConfigurations.FilePath.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.Services.BackgroundServices.DMCAScheduleServices
{
    public class HourlyResponseTimeDMCAAccusationCheckingService : BackgroundService
    {
        // LOGGER
        private readonly ILogger<HourlyResponseTimeDMCAAccusationCheckingService> _logger;

        // CONFIG
        public readonly IFilePathConfig _filePathConfig;

        // HELPERS
        private readonly DateHelper _dateHelper;

        // SERVICE PROVIDER
        private readonly IServiceProvider _serviceProvider;
        public HourlyResponseTimeDMCAAccusationCheckingService(
            ILogger<HourlyResponseTimeDMCAAccusationCheckingService> logger,
            IFilePathConfig filePathConfig,
            DateHelper dateHelper,
            IServiceProvider serviceProvider
            )
        {
            _logger = logger;
            _filePathConfig = filePathConfig;
            _dateHelper = dateHelper;
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HourlyResponseTimeDMCAAccusationCheckingService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking Dmca Accusation...");
                Console.WriteLine("\n\n----Checking Dmca Accusation----\n\n");
                try
                {
                    await DMCAAccusationResponseTimeAllowedChecking();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the requests.");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
        private async Task DMCAAccusationResponseTimeAllowedChecking()
        {
            Console.WriteLine("Checking......");
            using (var scope = _serviceProvider.CreateScope())
            {
                var dmcaAccusationService = scope.ServiceProvider.GetRequiredService<DMCAAccusationService>();
                await dmcaAccusationService.DMCAAccusationResponseTimeAllowedChecking();
            }
        }
    }
}
