using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserService.BusinessLogic.Helpers.DateHelpers;
using UserService.BusinessLogic.Helpers.FileHelpers;
using UserService.Common.AppConfigurations.FilePath.interfaces;

namespace UserService.BusinessLogic.Services.BackgroundServices.NonTimeRequiredRequestServices
{
    public class HourBaseRequestCancellationService : BackgroundService
    {
        // LOGGER
        private readonly ILogger<HourBaseRequestCancellationService> _logger;

        // CONFIG
        public readonly IFilePathConfig _filePathConfig;

        // HELPERS

        // SERVICE PROVIDER
        private readonly IServiceProvider _serviceProvider;

        public HourBaseRequestCancellationService(
            ILogger<HourBaseRequestCancellationService> logger,
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
                _logger.LogInformation("Checking Non Time Required Service requests...");
                Console.WriteLine("\n\n----Checking Non Time Required Service requests----\n\n");
                try
                {
                    await CancelNonTimeRequiredServiceRequestByHour();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the requests.");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task CancelNonTimeRequiredServiceRequestByHour()
        {
            //* [CHỈNH LẠI]
            // using (var scope = _serviceProvider.CreateScope())
            // {
            //     var serviceRequestService = scope.ServiceProvider.GetRequiredService<ServiceRequestService>();
            //     await serviceRequestService.CancelNonTimeRequiredServiceRequestByHour();
            // }
        }
    }
}