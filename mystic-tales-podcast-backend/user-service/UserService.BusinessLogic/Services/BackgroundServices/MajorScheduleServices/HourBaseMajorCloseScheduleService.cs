using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserService.BusinessLogic.Helpers.DateHelpers;
using UserService.BusinessLogic.Helpers.FileHelpers;
using UserService.Common.AppConfigurations.FilePath.interfaces;

namespace UserService.BusinessLogic.Services.BackgroundServices.MajorScheduleServices
{
    public class HourBaseMajorCloseScheduleService : BackgroundService
    {
        // LOGGER
        private readonly ILogger<HourBaseMajorCloseScheduleService> _logger;

        // CONFIG
        public readonly IFilePathConfig _filePathConfig;

        // HELPERS

        // SERVICE PROVIDER
        private readonly IServiceProvider _serviceProvider;

        public HourBaseMajorCloseScheduleService(
            ILogger<HourBaseMajorCloseScheduleService> logger,
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
            _logger.LogInformation("HourBaseMajorCloseScheduleService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking majors close schedule requests...");
                Console.WriteLine("\n\n----Checking majors close schedule requests----\n\n");
                try
                {
                    await HandleMajorCloseSchedule();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the requests.");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task HandleMajorCloseSchedule()
        {
            //* [CHỈNH LẠI]:
            // using (var scope = _serviceProvider.CreateScope())
            // {
            //     var serviceRequestService = scope.ServiceProvider.GetRequiredService<MajorService>();
            //     await serviceRequestService.HandleMajorsSchedule();
            // }
        }
    }
}