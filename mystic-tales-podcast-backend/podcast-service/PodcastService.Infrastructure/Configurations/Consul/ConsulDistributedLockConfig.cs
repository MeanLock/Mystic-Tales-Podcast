using Microsoft.Extensions.Configuration;
using PodcastService.Common.Configurations.Consul.interfaces;

namespace PodcastService.Common.Configurations.Consul
{
    public class ConsulDistributedLockConfigModel
    {
        public string LockKeyPrefix { get; set; } = string.Empty;
        public int DefaultLockShortTTLSeconds { get; set; }
        public int DefaultLockLongTTLSeconds { get; set; }
        public int DefaultLockShortRenewalIntervalSeconds { get; set; }
        public int DefaultLockLongRenewalIntervalSeconds { get; set; }
        public double DefaultLockWaitTimeoutSeconds { get; set; }
        public double DefaultLockRetryDelaySeconds { get; set; }
    }
    public class ConsulDistributedLockConfig : IConsulDistributedLockConfig
    {
        public string LockKeyPrefix { get; set; } = string.Empty;
        public int DefaultLockShortTTLSeconds { get; set; }
        public int DefaultLockLongTTLSeconds { get; set; }
        public int DefaultLockShortRenewalIntervalSeconds { get; set; }
        public int DefaultLockLongRenewalIntervalSeconds { get; set; }
        public double DefaultLockWaitTimeoutSeconds { get; set; }
        public double DefaultLockRetryDelaySeconds { get; set; }

        public ConsulDistributedLockConfig(IConfiguration configuration)
        {
            var consulDistributedLockConfig = configuration.GetSection("Infrastructure:Consul:DistributedLock").Get<ConsulDistributedLockConfigModel>();
            if (consulDistributedLockConfig != null)
            {
                LockKeyPrefix = consulDistributedLockConfig.LockKeyPrefix;
                DefaultLockShortTTLSeconds = consulDistributedLockConfig.DefaultLockShortTTLSeconds;
                DefaultLockLongTTLSeconds = consulDistributedLockConfig.DefaultLockLongTTLSeconds;
                DefaultLockShortRenewalIntervalSeconds = consulDistributedLockConfig.DefaultLockShortRenewalIntervalSeconds;
                DefaultLockLongRenewalIntervalSeconds = consulDistributedLockConfig.DefaultLockLongRenewalIntervalSeconds;
                DefaultLockWaitTimeoutSeconds = consulDistributedLockConfig.DefaultLockWaitTimeoutSeconds;
                DefaultLockRetryDelaySeconds = consulDistributedLockConfig.DefaultLockRetryDelaySeconds;
            }
        }
    }

    
}
