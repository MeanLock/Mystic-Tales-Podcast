namespace SystemConfigurationService.Infrastructure.Models.Consul.DistributedLock
{
    /// <summary>
    /// Information about a Consul session
    /// </summary>
    public class ConsulSessionInfo
    {
        /// <summary>
        /// Consul session ID
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Session TTL in seconds
        /// </summary>
        public int TTLSeconds { get; set; }

        /// <summary>
        /// Renewal interval in seconds
        /// </summary>
        public int RenewalIntervalSeconds { get; set; }

        /// <summary>
        /// When the session was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last renewal timestamp
        /// </summary>
        public DateTime LastRenewedAt { get; set; }

        /// <summary>
        /// Whether auto-renewal is active
        /// </summary>
        public bool IsAutoRenewalActive { get; set; }

        /// <summary>
        /// Number of successful renewals
        /// </summary>
        public int RenewalCount { get; set; }
    }
}