namespace SubscriptionService.Common.Configurations.Consul.interfaces
{
    public interface IConsulDistributedLockConfig
    {
        string LockKeyPrefix { get; set; }
        int DefaultLockShortTTLSeconds { get; set; }
        int DefaultLockLongTTLSeconds { get; set; }
        int DefaultLockShortRenewalIntervalSeconds { get; set; }
        int DefaultLockLongRenewalIntervalSeconds { get; set; }
        double DefaultLockWaitTimeoutSeconds { get; set; }
        double DefaultLockRetryDelaySeconds { get; set; }
    }
}
