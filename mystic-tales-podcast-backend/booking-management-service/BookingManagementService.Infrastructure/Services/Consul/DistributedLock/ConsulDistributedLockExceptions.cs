namespace BookingManagementService.Infrastructure.Services.Consul.DistributedLock
{
    /// <summary>
    /// Exception thrown when lock acquisition fails
    /// </summary>
    public class LockAcquisitionException : Exception
    {
        public string LockKey { get; }
        public TimeSpan? WaitedDuration { get; }

        public LockAcquisitionException(string lockKey, string message)
            : base(message)
        {
            LockKey = lockKey;
        }

        public LockAcquisitionException(string lockKey, string message, TimeSpan waitedDuration)
            : base(message)
        {
            LockKey = lockKey;
            WaitedDuration = waitedDuration;
        }

        public LockAcquisitionException(string lockKey, string message, Exception innerException)
            : base(message, innerException)
        {
            LockKey = lockKey;
        }
    }

    /// <summary>
    /// Exception thrown when session expires unexpectedly
    /// </summary>
    public class SessionExpiredException : Exception
    {
        public string SessionId { get; }
        public string LockKey { get; }

        public SessionExpiredException(string sessionId, string lockKey, string message)
            : base(message)
        {
            SessionId = sessionId;
            LockKey = lockKey;
        }

        public SessionExpiredException(string sessionId, string lockKey, string message, Exception innerException)
            : base(message, innerException)
        {
            SessionId = sessionId;
            LockKey = lockKey;
        }
    }

    /// <summary>
    /// Exception thrown when session renewal fails
    /// </summary>
    public class SessionRenewalException : Exception
    {
        public string SessionId { get; }

        public SessionRenewalException(string sessionId, string message)
            : base(message)
        {
            SessionId = sessionId;
        }

        public SessionRenewalException(string sessionId, string message, Exception innerException)
            : base(message, innerException)
        {
            SessionId = sessionId;
        }
    }

    /// <summary>
    /// Exception thrown when lock release fails
    /// </summary>
    public class LockReleaseException : Exception
    {
        public string LockKey { get; }
        public string SessionId { get; }

        public LockReleaseException(string lockKey, string sessionId, string message)
            : base(message)
        {
            LockKey = lockKey;
            SessionId = sessionId;
        }

        public LockReleaseException(string lockKey, string sessionId, string message, Exception innerException)
            : base(message, innerException)
        {
            LockKey = lockKey;
            SessionId = sessionId;
        }
    }
}