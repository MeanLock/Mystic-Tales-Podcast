namespace TransactionService.Infrastructure.Models.Consul.DistributedLock
{
    /// <summary>
    /// Options for acquiring a distributed lock
    /// </summary>
    public class ConsulLockOptions
    {
        /// <summary>
        /// Lock TTL in seconds (if null, uses default from config)
        /// </summary>
        public int? TTLSeconds { get; set; }

        /// <summary>
        /// Renewal interval in seconds (if null, uses default from config)
        /// </summary>
        public int? RenewalIntervalSeconds { get; set; }

        /// <summary>
        /// Max time to wait for lock acquisition (default: 30s)
        /// </summary>
        public TimeSpan WaitTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Delay between acquisition retries (default: 1s)
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Enable automatic renewal (default: true)
        /// </summary>
        public bool EnableAutoRenewal { get; set; } = true;

        /// <summary>
        /// Metadata to attach to lock (for debugging and monitoring)
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// Custom lock value to store in Consul KV
        /// </summary>
        public string? LockValue { get; set; }
    }
};

