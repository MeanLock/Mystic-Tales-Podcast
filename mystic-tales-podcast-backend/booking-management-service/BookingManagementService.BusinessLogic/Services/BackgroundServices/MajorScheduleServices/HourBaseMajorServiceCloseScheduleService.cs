using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BookingManagementService.BusinessLogic.Helpers.DateHelpers;
using BookingManagementService.BusinessLogic.Helpers.FileHelpers;
using BookingManagementService.Common.AppConfigurations.FilePath.interfaces;

namespace BookingManagementService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices
{
    public class HourBaseMajorServiceCloseScheduleService : BackgroundService
    {
        // LOGGER
        private readonly ILogger<HourBaseMajorServiceCloseScheduleService> _logger;

        // CONFIG
        public readonly IFilePathConfig _filePathConfig;

        // HELPERS

        // SERVICE PROVIDER
        private readonly IServiceProvider _serviceProvider;

        public HourBaseMajorServiceCloseScheduleService(
            ILogger<HourBaseMajorServiceCloseScheduleService> logger,
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
            _logger.LogInformation("HourBaseMajorServiceCloseScheduleService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking majors services close schedule requests...");
                Console.WriteLine("\n\n----Checking majors services close schedule requests----\n\n");
                try
                {
                    await HandleMajorServicesCloseSchedule();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the requests.");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task HandleMajorServicesCloseSchedule()
        {
            //* [CHỈNH LẠI]
            // using (var scope = _serviceProvider.CreateScope())
            // {
            //     var serviceRequestService = scope.ServiceProvider.GetRequiredService<MajorServicesService>();
            //     await serviceRequestService.HandleMajorServicesSchedule();
            // }
        }
    }
}