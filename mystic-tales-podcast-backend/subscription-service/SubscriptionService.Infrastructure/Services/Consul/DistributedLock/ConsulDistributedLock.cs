using Consul;
using Microsoft.Extensions.Logging;
using SubscriptionService.Common.AppConfigurations.App.interfaces;
using SubscriptionService.Infrastructure.Models.Consul.DistributedLock;

namespace SubscriptionService.Infrastructure.Services.Consul.DistributedLock
{
    /// <summary>
    /// Represents an acquired distributed lock with automatic renewal
    /// </summary>
    public class ConsulDistributedLock : IAsyncDisposable
    {
        private IAppConfig _appConfig;
        private readonly IConsulClient _consulClient;
        private readonly ConsulSessionManager _sessionManager;
        private readonly ConsulLockOptions _options;
        private readonly ILogger _logger;
        private bool _isReleased;
        private bool _disposed;

        public string LockKey { get; }
        public string SessionId { get; }
        public DateTime AcquiredAt { get; }
        public bool IsHeld => !_isReleased && !_disposed;

        internal ConsulDistributedLock(
            IAppConfig appConfig,
            string lockKey,
            string sessionId,
            IConsulClient consulClient,
            ConsulSessionManager sessionManager,
            ConsulLockOptions options,
            ILogger logger)
        {
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            LockKey = lockKey ?? throw new ArgumentNullException(nameof(lockKey));
            SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            _consulClient = consulClient ?? throw new ArgumentNullException(nameof(consulClient));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            AcquiredAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(_appConfig.TIME_ZONE));
            _isReleased = false;
            _disposed = false;

            _logger.LogInformation(
                "Lock acquired: Key={LockKey}, SessionId={SessionId}",
                LockKey, SessionId);

            // Start auto-renewal if enabled
            if (_options.EnableAutoRenewal)
            {
                _sessionManager.StartAutoRenewal(
                    SessionId,
                    _options.RenewalIntervalSeconds ?? 40);
            }
        }

        /// <summary>
        /// Manually renew the lock session
        /// </summary>
        public async Task RenewAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ThrowIfReleased();

            try
            {
                var renewed = await _sessionManager.RenewSessionAsync(SessionId, cancellationToken);

                if (!renewed)
                {
                    throw new SessionExpiredException(
                        SessionId,
                        LockKey,
                        $"Failed to renew session for lock '{LockKey}'");
                }

                _logger.LogDebug("Lock renewed manually: Key={LockKey}, SessionId={SessionId}", LockKey, SessionId);
            }
            catch (Exception ex) when (ex is not SessionExpiredException)
            {
                _logger.LogError(ex, "Failed to renew lock: Key={LockKey}, SessionId={SessionId}", LockKey, SessionId);
                throw;
            }
        }

        /// <summary>
        /// Release the lock explicitly
        /// </summary>
        public async Task ReleaseAsync(CancellationToken cancellationToken = default)
        {
            if (_isReleased || _disposed)
            {
                _logger.LogDebug(
                    "Lock already released or disposed: Key={LockKey}, SessionId={SessionId}",
                    LockKey, SessionId);
                return;
            }

            try
            {
                _logger.LogDebug("Releasing lock: Key={LockKey}, SessionId={SessionId}", LockKey, SessionId);

                // Stop auto-renewal
                _sessionManager.StopAutoRenewal(SessionId);

                // Release the lock in Consul KV
                var kvPair = new KVPair(LockKey)
                {
                    Session = SessionId
                };

                var released = await _consulClient.KV.Release(kvPair, cancellationToken);

                if (!released.Response)
                {
                    _logger.LogWarning(
                        "Failed to release lock in Consul (may already be released): Key={LockKey}, SessionId={SessionId}",
                        LockKey, SessionId);
                }

                // Destroy the session
                await _sessionManager.DestroySessionAsync(SessionId, cancellationToken);

                _isReleased = true;

                _logger.LogInformation(
                    "Lock released successfully: Key={LockKey}, SessionId={SessionId}, Duration={Duration}ms",
                    LockKey, SessionId, (TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(_appConfig.TIME_ZONE)) - AcquiredAt).TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error releasing lock: Key={LockKey}, SessionId={SessionId}",
                    LockKey, SessionId);

                throw new LockReleaseException(
                    LockKey,
                    SessionId,
                    $"Failed to release lock '{LockKey}'",
                    ex);
            }
        }

        /// <summary>
        /// Extend the lock TTL (useful for long-running operations)
        /// </summary>
        public async Task ExtendTTLAsync(
            int additionalSeconds,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            ThrowIfReleased();

            if (additionalSeconds <= 0)
            {
                throw new ArgumentException("Additional seconds must be positive", nameof(additionalSeconds));
            }

            _logger.LogInformation(
                "Extending lock TTL: Key={LockKey}, SessionId={SessionId}, AdditionalSeconds={AdditionalSeconds}",
                LockKey, SessionId, additionalSeconds);

            // Note: Consul doesn't support directly extending TTL
            // The session will be renewed with the original TTL
            // This is more of a documentation method to indicate intent
            await RenewAsync(cancellationToken);

            _logger.LogDebug(
                "Lock TTL extended (via renewal): Key={LockKey}, SessionId={SessionId}",
                LockKey, SessionId);
        }

        /// <summary>
        /// Check if the lock is still valid in Consul
        /// </summary>
        public async Task<bool> IsValidAsync(CancellationToken cancellationToken = default)
        {
            if (_isReleased || _disposed)
                return false;

            try
            {
                var result = await _consulClient.KV.Get(LockKey, cancellationToken);

                if (result.Response == null)
                    return false;

                return result.Response.Session == SessionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to check lock validity: Key={LockKey}, SessionId={SessionId}",
                    LockKey, SessionId);
                return false;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            try
            {
                if (!_isReleased)
                {
                    _logger.LogDebug(
                        "Lock being disposed without explicit release, releasing now: Key={LockKey}",
                        LockKey);

                    await ReleaseAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error during lock disposal: Key={LockKey}, SessionId={SessionId}",
                    LockKey, SessionId);
            }
            finally
            {
                _disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(
                    nameof(ConsulDistributedLock),
                    $"Lock has been disposed: Key={LockKey}");
            }
        }

        private void ThrowIfReleased()
        {
            if (_isReleased)
            {
                throw new InvalidOperationException(
                    $"Lock has already been released: Key={LockKey}");
            }
        }
    }
}