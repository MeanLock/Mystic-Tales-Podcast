using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UserService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace UserService.Common.AppConfigurations.BusinessSetting
{
    public class BackgroundJobsConfigModel
    {
        // public BackgroundJob PodcasterAllTimeMaxQueryMetricUpdateJob { get; set; }
        // public BackgroundJob PodcasterTemporal7dMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob AccountPodcastListenSlotRecoveryJob { get; set; }
    }
    public class BackgroundJobsConfig : IBackgroundJobsConfig
    {
        // public BackgroundJob PodcasterAllTimeMaxQueryMetricUpdateJob { get; set; }
        // public BackgroundJob PodcasterTemporal7dMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob AccountPodcastListenSlotRecoveryJob { get; set; }

        public BackgroundJobsConfig(IConfiguration configuration)
        {
            var backgroundJobsConfig = configuration.GetSection("BusinessSettings:BackgroundJobs").Get<BackgroundJobsConfigModel>();
            // PodcasterAllTimeMaxQueryMetricUpdateJob = backgroundJobsConfig?.PodcasterAllTimeMaxQueryMetricUpdateJob;
            // PodcasterTemporal7dMaxQueryMetricUpdateJob = backgroundJobsConfig?.PodcasterTemporal7dMaxQueryMetricUpdateJob;
            AccountPodcastListenSlotRecoveryJob = backgroundJobsConfig?.AccountPodcastListenSlotRecoveryJob;
        }
    }
}
