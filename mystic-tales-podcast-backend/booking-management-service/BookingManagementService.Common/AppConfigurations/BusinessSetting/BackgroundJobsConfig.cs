using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using BookingManagementService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace BookingManagementService.Common.AppConfigurations.BusinessSetting
{
    public class BackgroundJobsConfigModel
    {
        public BackgroundJob SampleBackgroundJob { get; set; }
        public BackgroundJob BookingProducingRequestedResponseTimeoutJob { get; set; }
        public BackgroundJob BookingTrackPreviewingResponseTimeoutJob { get; set; }
    }
    public class BackgroundJobsConfig : IBackgroundJobsConfig
    {
        public BackgroundJob SampleBackgroundJob { get; set; }

        public BackgroundJob BookingProducingRequestedResponseTimeoutJob { get; set; }

        public BackgroundJob BookingTrackPreviewingResponseTimeoutJob { get; set; }

        public BackgroundJobsConfig(IConfiguration configuration)
        {
            var backgroundJobsConfig = configuration.GetSection("BusinessSettings:BackgroundJobs").Get<BackgroundJobsConfigModel>();
            SampleBackgroundJob = backgroundJobsConfig?.SampleBackgroundJob;
            BookingProducingRequestedResponseTimeoutJob = backgroundJobsConfig?.BookingProducingRequestedResponseTimeoutJob;
            BookingTrackPreviewingResponseTimeoutJob = backgroundJobsConfig?.BookingTrackPreviewingResponseTimeoutJob;
        }
    }
}
