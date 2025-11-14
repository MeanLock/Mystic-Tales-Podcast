using Newtonsoft.Json.Linq;

namespace ModerationService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IBackgroundJobsConfig
    {
        BackgroundJob SampleBackgroundJob { get; }
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
