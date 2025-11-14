using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;
using PodcastService.Common.AppConfigurations.FilePath;

namespace PodcastService.Common.AppConfigurations.BusinessSetting
{
    public class BackgroundJobsConfigModel
    {
        public BackgroundJob PodcasterAllTimeMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob PodcasterTemporal7dMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob ShowAllTimeMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob ChannelAllTimeMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob ShowTemporal7dMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob ChannelTemporal7dMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob SystemPreferencesTemporal30dQueryMetricUpdateJob { get; set; }
        public BackgroundJob UserPreferencesTemporal30dQueryMetricUpdateJob { get; set; }
        public BackgroundJob ShowPublishReleaseJob { get; set; }
        public BackgroundJob EpisodePublishReleaseJob { get; set; }
        public BackgroundJob EpisodeEditRequirementPendingTimeoutJob { get; set; }
        public BackgroundJob EpisodeListenSessionExpiryJob { get; set; }
    }
    public class BackgroundJobsConfig : IBackgroundJobsConfig
    {
        public BackgroundJob PodcasterAllTimeMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob PodcasterTemporal7dMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob ShowAllTimeMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob ChannelAllTimeMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob ShowTemporal7dMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob ChannelTemporal7dMaxQueryMetricUpdateJob { get; set; }
        public BackgroundJob SystemPreferencesTemporal30dQueryMetricUpdateJob { get; set; }
        public BackgroundJob UserPreferencesTemporal30dQueryMetricUpdateJob { get; set; }
        public BackgroundJob ShowPublishReleaseJob { get; set; }
        public BackgroundJob EpisodePublishReleaseJob { get; set; }
        public BackgroundJob EpisodeEditRequirementPendingTimeoutJob { get; set; }
        public BackgroundJob EpisodeListenSessionExpiryJob { get; set; }


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
            ShowPublishReleaseJob = backgroundJobsConfig?.ShowPublishReleaseJob;
            EpisodePublishReleaseJob = backgroundJobsConfig?.EpisodePublishReleaseJob;
            EpisodeEditRequirementPendingTimeoutJob = backgroundJobsConfig?.EpisodeEditRequirementPendingTimeoutJob;
            EpisodeListenSessionExpiryJob = backgroundJobsConfig?.EpisodeListenSessionExpiryJob;
        }
    }
}
