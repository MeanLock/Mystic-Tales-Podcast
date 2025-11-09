using Newtonsoft.Json.Linq;

namespace UserService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IBackgroundJobsConfig
    {
        BackgroundJob PodcasterQueryMetricUpdateJob { get; }
    }

    public class BackgroundJob
    {
        public string CronExpression { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public string ConsulLockKey { get; set; }
        public int ConsulLockTTLSeconds { get; set; }
        public int ConsulLockRenewalIntervalSeconds { get; set; }
    }
}
