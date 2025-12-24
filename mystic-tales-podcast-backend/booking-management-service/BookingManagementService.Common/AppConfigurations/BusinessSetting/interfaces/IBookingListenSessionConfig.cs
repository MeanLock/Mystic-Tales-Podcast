using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IBookingListenSessionConfig
    {
        int SessionExpirationMinutes { get; set; }
        int SessionAdditionalUpdateBufferExpirationMinutes { get; set; }
        int SessionAudioUrlExpirationSeconds { get; set; }
    }
}
