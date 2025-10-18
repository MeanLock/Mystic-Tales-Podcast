using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModerationService.Common.AppConfigurations.FilePath.interfaces;

namespace ModerationService.BusinessLogic.Services.BackgroundServices.TimeRequiredRequestServices
{
    public class MinuteBaseRequestCancellationService : BackgroundService
    {
        // LOGGER
        private readonly ILogger<MinuteBaseRequestCancellationService> _logger;

        // CONFIG
        public readonly IFilePathConfig _filePathConfig;

        // HELPERS

        // SERVICE PROVIDER
        private readonly IServiceProvider _serviceProvider;

        public MinuteBaseRequestCancellationService(
            ILogger<MinuteBaseRequestCancellationService> logger,
            IFilePathConfig filePathConfig,
            IServiceProvider serviceProvider
            )
        {
            _logger = logger;
            _filePathConfig = filePathConfig;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("HourlyRequestCancellationService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking Time Required Service requests...");
                Console.WriteLine("\n\n----Checking Time Required Service requests----\n\n");
                try
                {
                    await CancelTimeRequiredServiceRequestByHour();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the requests.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task CancelTimeRequiredServiceRequestByHour()
        {
            //* [CHỈNH LẠI]
            // using (var scope = _serviceProvider.CreateScope())
            // {
            //     var serviceRequestService = scope.ServiceProvider.GetRequiredService<ServiceRequestService>();
            //     await serviceRequestService.CancelTimeRequiredServiceRequestByHour();
            // }
        }
    }
}