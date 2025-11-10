using System.Collections.Concurrent;
using System.Text.Json;
using Consul;
using Microsoft.Extensions.Logging;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.Infrastructure.Models.Consul.DistributedLock;

namespace PodcastService.Infrastructure.Services.Consul.DistributedLock
{
    /// <summary>
    /// Manages Consul session lifecycle including creation, renewal, and cleanup
    /// </summary>
    public class ConsulSessionManager : IDisposable
    {
        private readonly IAppConfig _appConfig;
        private readonly IConsulClient _consulClient;
        private readonly ILogger<ConsulSessionManager> _logger;
        private readonly ConcurrentDictionary<string, ConsulSessionInfo> _activeSessions;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _renewalTasks;
        private readonly SemaphoreSlim _sessionCreationLock;
        private bool _disposed;

        public ConsulSessionManager(
            IAppConfig appConfig,
            IConsulClient consulClient,
            ILogger<ConsulSessionManager> logger)
        {
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _consulClient = consulClient ?? throw new ArgumentNullException(nameof(consulClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _activeSessions = new ConcurrentDictionary<string, ConsulSessionInfo>();
            _renewalTasks = new ConcurrentDictionary<string, CancellationTokenSource>();
            _sessionCreationLock = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Create a new Consul session with specified TTL
        /// </summary>
        public async Task<string> CreateSessionAsync(
            int ttlSeconds,
            int renewalIntervalSeconds,
            CancellationToken cancellationToken = default)
        {
            await _sessionCreationLock.WaitAsync(cancellationToken);
            try
            {
                var sessionEntry = new SessionEntry
                {
                    Name = $"distributed-lock-session-{Guid.NewGuid():N}",
                    TTL = TimeSpan.FromSeconds(ttlSeconds),
                    Behavior = SessionBehavior.Delete, // Delete locks when session expires
                    LockDelay = TimeSpan.Zero // No lock delay for immediate re-acquisition
                };

                _logger.LogDebug(
                    "Creating Consul session with TTL={TTLSeconds}s, Name={SessionName}",
                    ttlSeconds, sessionEntry.Name);

                var sessionId = await _consulClient.Session.Create(sessionEntry, cancellationToken);

                if (string.IsNullOrEmpty(sessionId.Response))
                {
                    throw new InvalidOperationException("Failed to create Consul session: empty session ID returned");
                }

                var sessionInfo = new ConsulSessionInfo
                {
                    SessionId = sessionId.Response,
                    TTLSeconds = ttlSeconds,
                    RenewalIntervalSeconds = renewalIntervalSeconds,
                    CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(_appConfig.TIME_ZONE)),
                    LastRenewedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(_appConfig.TIME_ZONE)),
                    IsAutoRenewalActive = false,
                    RenewalCount = 0
                };

                _activeSessions.TryAdd(sessionId.Response, sessionInfo);

                _logger.LogInformation(
                    "Consul session created successfully: SessionId={SessionId}, TTL={TTLSeconds}s",
                    sessionId.Response, ttlSeconds);

                return sessionId.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Consul session with TTL={TTLSeconds}s", ttlSeconds);
                throw;
            }
            finally
            {
                _sessionCreationLock.Release();
            }
        }

        /// <summary>
        /// Start automatic renewal for a session
        /// </summary>
        public void StartAutoRenewal(
            string sessionId,
            int renewalIntervalSeconds,
            CancellationToken cancellationToken = default)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var sessionInfo))
            {
                throw new InvalidOperationException($"Session {sessionId} not found in active sessions");
            }

            if (sessionInfo.IsAutoRenewalActive)
            {
                _logger.LogWarning("Auto-renewal already active for session {SessionId}", sessionId);
                return;
            }

            var renewalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _renewalTasks.TryAdd(sessionId, renewalCts);

            sessionInfo.IsAutoRenewalActive = true;

            // Start renewal task
            _ = Task.Run(async () => await RenewalLoopAsync(sessionId, renewalIntervalSeconds, renewalCts.Token), renewalCts.Token);

            _logger.LogInformation(
                "Auto-renewal started for session {SessionId} with interval {IntervalSeconds}s",
                sessionId, renewalIntervalSeconds);
        }

        /// <summary>
        /// Manually renew a session
        /// </summary>
        public async Task<bool> RenewSessionAsync(
            string sessionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Renewing session {SessionId}", sessionId);

                var result = await _consulClient.Session.Renew(sessionId, cancellationToken);

                if (result.Response == null)
                {
                    _logger.LogWarning("Session {SessionId} renewal failed: session not found or expired", sessionId);
                    return false;
                }

                if (_activeSessions.TryGetValue(sessionId, out var sessionInfo))
                {
                    // sessionInfo.LastRenewedAt = DateTime.UtcNow;
                    sessionInfo.LastRenewedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(_appConfig.TIME_ZONE));
                    sessionInfo.RenewalCount++;
                }

                _logger.LogDebug("Session {SessionId} renewed successfully", sessionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to renew session {SessionId}", sessionId);
                throw new SessionRenewalException(sessionId, "Failed to renew Consul session", ex);
            }
        }

        /// <summary>
        /// Stop auto-renewal for a session
        /// </summary>
        public void StopAutoRenewal(string sessionId)
        {
            if (_renewalTasks.TryRemove(sessionId, out var cts))
            {
                cts.Cancel();
                cts.Dispose();

                if (_activeSessions.TryGetValue(sessionId, out var sessionInfo))
                {
                    sessionInfo.IsAutoRenewalActive = false;
                }

                _logger.LogInformation("Auto-renewal stopped for session {SessionId}", sessionId);
            }
        }

        /// <summary>
        /// Destroy a Consul session
        /// </summary>
        public async Task DestroySessionAsync(
            string sessionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Stop auto-renewal first
                StopAutoRenewal(sessionId);

                _logger.LogDebug("Destroying session {SessionId}", sessionId);

                await _consulClient.Session.Destroy(sessionId, cancellationToken);

                _activeSessions.TryRemove(sessionId, out _);

                _logger.LogInformation("Session {SessionId} destroyed successfully", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to destroy session {SessionId}", sessionId);
                throw;
            }
        }

        /// <summary>
        /// Get information about an active session
        /// </summary>
        public ConsulSessionInfo? GetSessionInfo(string sessionId)
        {
            return _activeSessions.TryGetValue(sessionId, out var info) ? info : null;
        }

        /// <summary>
        /// Check if a session is still valid in Consul
        /// </summary>
        public async Task<bool> IsSessionValidAsync(
            string sessionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _consulClient.Session.Info(sessionId, cancellationToken);
                return result.Response != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check session validity for {SessionId}", sessionId);
                return false;
            }
        }

        /// <summary>
        /// Renewal loop that runs in background
        /// </summary>
        private async Task RenewalLoopAsync(
            string sessionId,
            int renewalIntervalSeconds,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("Starting renewal loop for session {SessionId}", sessionId);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(renewalIntervalSeconds), cancellationToken);

                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var renewed = await RenewSessionAsync(sessionId, cancellationToken);

                    if (!renewed)
                    {
                        _logger.LogWarning(
                            "Session {SessionId} could not be renewed, stopping auto-renewal",
                            sessionId);
                        break;
                    }

                    _logger.LogTrace(
                        "Session {SessionId} renewed successfully in background loop",
                        sessionId);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("Renewal loop cancelled for session {SessionId}", sessionId);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error in renewal loop for session {SessionId}, will retry",
                        sessionId);

                    // Continue loop to retry renewal
                }
            }

            _logger.LogDebug("Renewal loop stopped for session {SessionId}", sessionId);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _logger.LogInformation("Disposing ConsulSessionManager, cleaning up {Count} active sessions",
                _activeSessions.Count);

            // Stop all renewal tasks
            foreach (var kvp in _renewalTasks)
            {
                kvp.Value.Cancel();
                kvp.Value.Dispose();
            }

            _renewalTasks.Clear();

            // Destroy all active sessions
            foreach (var sessionId in _activeSessions.Keys.ToList())
            {
                try
                {
                    DestroySessionAsync(sessionId).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to destroy session {SessionId} during disposal", sessionId);
                }
            }

            _activeSessions.Clear();
            _sessionCreationLock.Dispose();

            _disposed = true;
            _logger.LogInformation("ConsulSessionManager disposed successfully");
        }
    }
}