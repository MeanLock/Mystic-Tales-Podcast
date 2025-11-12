using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.Common.AppConfigurations.FilePath;

namespace PodcastService.Common.AppConfigurations.BusinessSetting
{
    public class BackgroundJobsConfigModel
    {
        public BackgroundJob PodcasterAllTimeMaxQueryMetricUpdateJob { get; }
        public BackgroundJob PodcasterTemporal7dMaxQueryMetricUpdateJob { get; }
        public BackgroundJob ShowAllTimeMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob ChannelAllTimeMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob ShowTemporal7dMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob ChannelTemporal7dMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob SystemPreferencesTemporal30dQueryMetricUpdateJob { get; set; }
        public BackgroundJob UserPreferencesTemporal30dQueryMetricUpdateJob { get; set; }
    }
    public class BackgroundJobsConfig : IBackgroundJobsConfig
    {
        public BackgroundJob PodcasterAllTimeMaxQueryMetricUpdateJob { get; }
        public BackgroundJob PodcasterTemporal7dMaxQueryMetricUpdateJob { get; }
        public BackgroundJob ShowAllTimeMaxQueryMetricUpdateJob { get; }
        public BackgroundJob ChannelAllTimeMaxQueryMetricUpdateJob { get; }
        public BackgroundJob ShowTemporal7dMaxQueryMetricUpdateJob { get; }
        public BackgroundJob ChannelTemporal7dMaxQueryMetricUpdateJob { get; }
        public BackgroundJob SystemPreferencesTemporal30dQueryMetricUpdateJob { get; }
        public BackgroundJob UserPreferencesTemporal30dQueryMetricUpdateJob { get; }


        public BackgroundJobsConfig(IConfiguration configuration)
        {
            var backgroundJobsConfig = configuration.GetSection("BusinessSettings:BackgroundJobs").Get<BackgroundJobsConfigModel>();
            PodcasterAllTimeMaxQueryMetricUpdateJob = backgroundJobsConfig?.PodcasterAllTimeMaxQueryMetricUpdateJob;
            PodcasterTemporal7dMaxQueryMetricUpdateJob = backgroundJobsConfig?.PodcasterTemporal7dMaxQueryMetricUpdateJob;
            ShowAllTimeMaxQueryMetricUpdateJob = backgroundJobsConfig?.ShowAllTimeMaxQueryMetricUpdateJob;
            ChannelAllTimeMaxQueryMetricUpdateJob = backgroundJobsConfig?.ChannelAllTimeMaxQueryMetricUpdateJob;
            ShowTemporal7dMaxQueryMetricUpdateJob = backgroundJobsConfig?.ShowTemporal7dMaxQueryMetricUpdateJob;
            ChannelTemporal7dMaxQueryMetricUpdateJob = backgroundJobsConfig?.ChannelTemporal7dMaxQueryMetricUpdateJob;
            SystemPreferencesTemporal30dQueryMetricUpdateJob = backgroundJobsConfig?.SystemPreferencesTemporal30dQueryMetricUpdateJob;
            UserPreferencesTemporal30dQueryMetricUpdateJob = backgroundJobsConfig?.UserPreferencesTemporal30dQueryMetricUpdateJob;
        }
    }
}
