using TransactionService.Infrastructure.Services.Consul.DistributedLock;

namespace TransactionService.Infrastructure.Models.Consul.DistributedLock
{
    /// <summary>
    /// Result of a lock acquisition attempt
    /// </summary>
    public class LockAcquisitionResult
    {
        /// <summary>
        /// Whether the lock was successfully acquired
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The acquired lock (null if not successful)
        /// </summary>
        public ConsulDistributedLock? Lock { get; set; }

        /// <summary>
        /// Reason for failure (if not successful)
        /// </summary>
        public string? FailureReason { get; set; }

        /// <summary>
        /// Suggested time to wait before retrying
        /// </summary>
        public TimeSpan? RetryAfter { get; set; }

        /// <summary>
        /// Current lock holder information (if lock is held by another instance)
        /// </summary>
        public ConsulLockHolderInfo? CurrentHolder { get; set; }
    }
}