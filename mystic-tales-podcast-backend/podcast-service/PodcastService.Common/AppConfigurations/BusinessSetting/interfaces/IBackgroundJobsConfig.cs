using Newtonsoft.Json.Linq;

namespace PodcastService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IBackgroundJobsConfig
    {
        BackgroundJob PodcasterAllTimeMaxQueryMetricUpdateJob { get; }
        BackgroundJob PodcasterTemporal7dMaxQueryMetricUpdateJob { get; }
        BackgroundJob ShowAllTimeMaxQueryMetricUpdateJob { get; }
        BackgroundJob ChannelAllTimeMaxQueryMetricUpdateJob { get; }
        BackgroundJob ShowTemporal7dMaxQueryMetricUpdateJob { get; }
        BackgroundJob ChannelTemporal7dMaxQueryMetricUpdateJob { get; }
        BackgroundJob SystemPreferencesTemporal30dQueryMetricUpdateJob { get; }
        BackgroundJob UserPreferencesTemporal30dQueryMetricUpdateJob { get; }

    }

    public class BackgroundJob
    {
        public string CronExpression { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public string ConsulLockKey { get; set; }
        public int ConsulLockTTLSeconds { get; set; }
        public int ConsulLockRenewalIntervalSeconds { get; set; }
        public string? RedisKeyName { get; set; }
        public int? RedisKeyTTLSeconds { get; set; }
    }
}
