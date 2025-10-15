using BookingManagementService.BusinessLogic.Helpers.DateHelpers;
using BookingManagementService.BusinessLogic.Services.DbServices.BookingServices;
using BookingManagementService.Common.AppConfigurations.FilePath.interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookingManagementService.BusinessLogic.Services.BackgroundServices.BookingScheduleServices
{
    public class HourlyDeadlineBookingCheckingServices : BackgroundService
    {
        private readonly ILogger<HourlyDeadlineBookingCheckingServices> _logger;

        // CONFIG
        public readonly IFilePathConfig _filePathConfig;

        // HELPERS
        private readonly DateHelper _dateHelper;

        // SERVICE PROVIDER
        private readonly IServiceProvider _serviceProvider;
        public HourlyDeadlineBookingCheckingServices(
            ILogger<HourlyDeadlineBookingCheckingServices> logger,
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
            _logger.LogInformation("HourlyDeadlineBookingCheckingServices is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking booking requests...");
                Console.WriteLine("\n\n----Checking booking requests----\n\n");
                try
                {
                    await BookingDayResponseAllowedChecking();

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the requests.");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
        private async Task BookingDayResponseAllowedChecking()
        {
            Console.WriteLine("Checking......");
            using (var scope = _serviceProvider.CreateScope())
            {
                var bookingRequestService = scope.ServiceProvider.GetRequiredService<BookingService>();
                await bookingRequestService.BookingDayResponseAllowedChecking();
            }
        }
    }
}
