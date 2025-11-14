using Newtonsoft.Json.Linq;

namespace PodcastService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IBackgroundJobsConfig
    {
        BackgroundJob PodcasterAllTimeMaxQueryMetricUpdateJob { get; set; }
        BackgroundJob PodcasterTemporal7dMaxQueryMetricUpdateJob { get; set; }
        BackgroundJob ShowAllTimeMaxQueryMetricUpdateJob { get; set; }
        BackgroundJob ChannelAllTimeMaxQueryMetricUpdateJob { get; set; }
        BackgroundJob ShowTemporal7dMaxQueryMetricUpdateJob { get; set; }
        BackgroundJob ChannelTemporal7dMaxQueryMetricUpdateJob { get; set; }
        BackgroundJob SystemPreferencesTemporal30dQueryMetricUpdateJob { get; set; }
        BackgroundJob UserPreferencesTemporal30dQueryMetricUpdateJob { get; set; }

    }

    public class BackgroundJob
    {
        public string? CronExpression { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public string ConsulLockKey { get; set; }
        public int ConsulLockTTLSeconds { get; set; }
        public int ConsulLockRenewalIntervalSeconds { get; set; }
        public string? RedisKeyName { get; set; }
        public int? RedisKeyTTLSeconds { get; set; }
    }
}
