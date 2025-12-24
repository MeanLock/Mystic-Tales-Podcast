namespace ModerationService.Infrastructure.Models.Consul.DistributedLock
{
    /// <summary>
    /// Information about current lock holder
    /// </summary>
    public class ConsulLockHolderInfo
    {
        /// <summary>
        /// Consul session ID holding the lock
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Instance identifier (machine name, pod name, etc.)
        /// </summary>
        public string? InstanceId { get; set; }

        /// <summary>
        /// When the lock was acquired
        /// </summary>
        public DateTime AcquiredAt { get; set; }

        /// <summary>
        /// Custom metadata attached to the lock
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// Lock value stored in Consul KV
        /// </summary>
        public string? LockValue { get; set; }
    }
}