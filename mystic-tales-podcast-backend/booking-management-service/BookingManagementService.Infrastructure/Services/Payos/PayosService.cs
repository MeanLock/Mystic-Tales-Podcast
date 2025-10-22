using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingManagementService.Infrastructure.Services.Payos
{
    public class PayosService
    {
        // LOGGER
        private readonly ILogger<PayosService> _logger;

        // CONFIG


        public PayosService(
            ILogger<PayosService> logger
        )
        {
            _logger = logger;
        }

        


    }
}
