using BookingManagementService.Common.AppConfigurations.BusinessSetting.interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.Common.AppConfigurations.BusinessSetting
{
    public class BookingListenSessionConfigModel
    {
        public int SessionExpirationMinutes { get; set; }
        public int SessionAdditionalUpdateBufferExpirationMinutes { get; set; }
        public int SessionAudioUrlExpirationSeconds { get; set; }
    }
    public class BookingListenSessionConfig : IBookingListenSessionConfig
    {
        public int SessionExpirationMinutes { get; set; }
        public int SessionAdditionalUpdateBufferExpirationMinutes { get; set; }
        public int SessionAudioUrlExpirationSeconds { get; set; }
        public BookingListenSessionConfig(IConfiguration configuration)
        {
            var bookingListenSessionConfig = configuration.GetSection("BusinessSettings:BookingListenSession").Get<BookingListenSessionConfigModel>();
            SessionExpirationMinutes = bookingListenSessionConfig.SessionExpirationMinutes;
            SessionAdditionalUpdateBufferExpirationMinutes = bookingListenSessionConfig.SessionAdditionalUpdateBufferExpirationMinutes;
            SessionAudioUrlExpirationSeconds = bookingListenSessionConfig.SessionAudioUrlExpirationSeconds;
        }
    }
}
