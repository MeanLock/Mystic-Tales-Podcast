using System.Text;
using System.Text.Json;
using Consul;
using Microsoft.Extensions.Logging;
using TransactionService.Common.AppConfigurations.App.interfaces;
using TransactionService.Common.Configurations.Consul.interfaces;
using TransactionService.Infrastructure.Models.Consul.DistributedLock;

namespace TransactionService.Infrastructure.Services.Consul.DistributedLock
{
    /// <summary>
    /// Service for acquiring and managing distributed locks using Consul
    /// </summary>
    public class ConsulDistributedLockService
    {
        private readonly IAppConfig _appConfig;
        private readonly IConsulClient _consulClient;
        private readonly ConsulSessionManager _sessionManager;
        private readonly IConsulDistributedLockConfig _config;
        private readonly ILogger<ConsulDistributedLockService> _logger;

        public ConsulDistributedLockService(
            IAppConfig appConfig,
            IConsulClient consulClient,
            ConsulSessionManager sessionManager,
            IConsulDistributedLockConfig config,
            ILogger<ConsulDistributedLockService> logger)
        {
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _consulClient = consulClient ?? throw new ArgumentNullException(nameof(consulClient));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Acquire a distributed lock with automatic renewal (blocking until acquired or timeout)
        /// </summary>
        public async Task<ConsulDistributedLock> AcquireLockAsync(
            string lockKey,
            ConsulLockOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var fullKey = BuildFullLockKey(lockKey);
            var lockOptions = MergeLockOptions(options);

            _logger.LogDebug(
                "Attempting to acquire lock: Key={LockKey}, TTL={TTL}s, WaitTimeout={WaitTimeout}s",
                fullKey, lockOptions.TTLSeconds, lockOptions.WaitTimeout.TotalSeconds);

            // Create Consul session
            var sessionId = await _sessionManager.CreateSessionAsync(
                lockOptions.TTLSeconds!.Value,
                lockOptions.RenewalIntervalSeconds!.Value,
                cancellationToken);

            try
            {
                // Try to acquire lock with retry
                var acquired = await TryAcquireWithRetryAsync(
                    fullKey,
                    sessionId,
                    lockOptions,
                    cancellationToken);

                if (!acquired)
                {
                    await _sessionManager.DestroySessionAsync(sessionId, cancellationToken);
                    throw new LockAcquisitionException(
                        fullKey,
                        $"Failed to acquire lock within timeout of {lockOptions.WaitTimeout.TotalSeconds}s",
                        lockOptions.WaitTimeout);
                }

                // Create and return lock instance
                var distributedLock = new ConsulDistributedLock(
                    _appConfig,
                    fullKey,
                    sessionId,
                    _consulClient,
                    _sessionManager,
                    lockOptions,
                    _logger);

                _logger.LogInformation(
                    "Lock acquired successfully: Key={LockKey}, SessionId={SessionId}",
                    fullKey, sessionId);

                return distributedLock;
            }
            catch
            {
                // Clean up session if lock acquisition fails
                try
                {
                    await _sessionManager.DestroySessionAsync(sessionId, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to clean up session {SessionId} after lock acquisition failure", sessionId);
                }

                throw;
            }
        }

        /// <summary>
        /// Try to acquire lock without blocking (returns immediately with result)
        /// </summary>
        public async Task<LockAcquisitionResult> TryAcquireLockAsync(
            string lockKey,
            ConsulLockOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var fullKey = BuildFullLockKey(lockKey);
            var lockOptions = MergeLockOptions(options);

            try
            {
                // Create session
                var sessionId = await _sessionManager.CreateSessionAsync(
                    lockOptions.TTLSeconds!.Value,
                    lockOptions.RenewalIntervalSeconds!.Value,
                    cancellationToken);

                try
                {
                    // Single attempt to acquire
                    var acquired = await TryAcquireOnceAsync(fullKey, sessionId, lockOptions, cancellationToken);

                    if (!acquired)
                    {
                        await _sessionManager.DestroySessionAsync(sessionId, cancellationToken);

                        var currentHolder = await GetLockHolderAsync(fullKey, cancellationToken);

                        return new LockAcquisitionResult
                        {
                            Success = false,
                            FailureReason = "Lock is currently held by another instance",
                            RetryAfter = TimeSpan.FromSeconds(5),
                            CurrentHolder = currentHolder
                        };
                    }

                    var distributedLock = new ConsulDistributedLock(
                        _appConfig,
                        fullKey,
                        sessionId,
                        _consulClient,
                        _sessionManager,
                        lockOptions,
                        _logger);

                    return new LockAcquisitionResult
                    {
                        Success = true,
                        Lock = distributedLock
                    };
                }
                catch
                {
                    await _sessionManager.DestroySessionAsync(sessionId, cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trying to acquire lock: Key={LockKey}", fullKey);

                return new LockAcquisitionResult
                {
                    Success = false,
                    FailureReason = ex.Message,
                    RetryAfter = TimeSpan.FromSeconds(5)
                };
            }
        }

        /// <summary>
        /// Execute action with lock (automatically acquires and releases lock)
        /// </summary>
        public async Task ExecuteWithLockAsync(
            string lockKey,
            Func<CancellationToken, Task> action,
            ConsulLockOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            await using var distributedLock = await AcquireLockAsync(lockKey, options, cancellationToken);

            await action(cancellationToken);

            // Lock automatically released via Dispose
        }

        /// <summary>
        /// Execute action with lock and return result
        /// </summary>
        public async Task<T> ExecuteWithLockAsync<T>(
            string lockKey,
            Func<CancellationToken, Task<T>> action,
            ConsulLockOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            await using var distributedLock = await AcquireLockAsync(lockKey, options, cancellationToken);

            return await action(cancellationToken);
        }

        /// <summary>
        /// Check if a lock is currently held by any instance
        /// </summary>
        public async Task<bool> IsLockHeldAsync(
            string lockKey,
            CancellationToken cancellationToken = default)
        {
            var fullKey = BuildFullLockKey(lockKey);

            try
            {
                var result = await _consulClient.KV.Get(fullKey, cancellationToken);
                return result.Response?.Session != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if lock is held: Key={LockKey}", fullKey);
                throw;
            }
        }

        /// <summary>
        /// Get current lock holder information
        /// </summary>
        public async Task<ConsulLockHolderInfo?> GetLockHolderAsync(
            string lockKey,
            CancellationToken cancellationToken = default)
        {
            var fullKey = BuildFullLockKey(lockKey);

            try
            {
                var result = await _consulClient.KV.Get(fullKey, cancellationToken);

                if (result.Response?.Session == null)
                    return null;

                // Try to parse metadata from lock value
                var metadata = ParseLockValue(result.Response.Value);

                return new ConsulLockHolderInfo
                {
                    SessionId = result.Response.Session,
                    AcquiredAt = metadata.TryGetValue("AcquiredAt", out var acquiredAt)
                        ? DateTime.Parse(acquiredAt)
                        : TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(_appConfig.TIME_ZONE)),
                    InstanceId = metadata.TryGetValue("InstanceId", out var instanceId)
                        ? instanceId
                        : null,
                    Metadata = metadata.Count > 0 ? metadata : null,
                    LockValue = result.Response.Value != null
                        ? Encoding.UTF8.GetString(result.Response.Value)
                        : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get lock holder info: Key={LockKey}", fullKey);
                throw;
            }
        }

        /// <summary>
        /// Force release a lock (use with caution - for admin/cleanup purposes only)
        /// </summary>
        public async Task ForceReleaseLockAsync(
            string lockKey,
            CancellationToken cancellationToken = default)
        {
            var fullKey = BuildFullLockKey(lockKey);

            _logger.LogWarning("Force releasing lock: Key={LockKey}", fullKey);

            try
            {
                var result = await _consulClient.KV.Get(fullKey, cancellationToken);

                if (result.Response?.Session != null)
                {
                    // Destroy the session holding the lock
                    await _consulClient.Session.Destroy(result.Response.Session, cancellationToken);
                    _logger.LogInformation(
                        "Lock force released: Key={LockKey}, SessionId={SessionId}",
                        fullKey, result.Response.Session);
                }
                else
                {
                    _logger.LogInformation("No active lock found to release: Key={LockKey}", fullKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to force release lock: Key={LockKey}", fullKey);
                throw;
            }
        }

        // ========== PRIVATE HELPER METHODS ==========

        private string BuildFullLockKey(string lockKey)
        {
            if (string.IsNullOrWhiteSpace(lockKey))
                throw new ArgumentException("Lock key cannot be empty", nameof(lockKey));

            return $"{_config.LockKeyPrefix}{lockKey}";
        }

        private ConsulLockOptions MergeLockOptions(ConsulLockOptions? options)
        {
            return new ConsulLockOptions
            {
                TTLSeconds = options?.TTLSeconds ?? _config.DefaultLockShortTTLSeconds,
                RenewalIntervalSeconds = options?.RenewalIntervalSeconds ?? _config.DefaultLockShortRenewalIntervalSeconds,
                WaitTimeout = options?.WaitTimeout ?? TimeSpan.FromSeconds(30),
                RetryDelay = options?.RetryDelay ?? TimeSpan.FromSeconds(1),
                EnableAutoRenewal = options?.EnableAutoRenewal ?? true,
                Metadata = options?.Metadata,
                LockValue = options?.LockValue
            };
        }

        private async Task<bool> TryAcquireWithRetryAsync(
            string fullKey,
            string sessionId,
            ConsulLockOptions options,
            CancellationToken cancellationToken)
        {
            var startTime = DateTime.UtcNow; // Đơn giản hơn
            var attemptCount = 0;

            while (DateTime.UtcNow - startTime < options.WaitTimeout)
            {
                attemptCount++;

                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogDebug("Lock acquisition cancelled: Key={LockKey}", fullKey);
                    return false;
                }

                var acquired = await TryAcquireOnceAsync(fullKey, sessionId, options, cancellationToken);

                if (acquired)
                {
                    _logger.LogDebug(
                        "Lock acquired after {Attempts} attempts: Key={LockKey}, Duration={Duration}ms",
                        attemptCount, fullKey, (TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(_appConfig.TIME_ZONE)) - startTime).TotalMilliseconds);
                    return true;
                }

                // Wait before retry
                await Task.Delay(options.RetryDelay, cancellationToken);
            }

            _logger.LogWarning(
                "Failed to acquire lock after {Attempts} attempts and {Duration}ms: Key={LockKey}",
                attemptCount, (TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(_appConfig.TIME_ZONE)) - startTime).TotalMilliseconds, fullKey);

            return false;
        }

        private async Task<bool> TryAcquireOnceAsync(
            string fullKey,
            string sessionId,
            ConsulLockOptions options,
            CancellationToken cancellationToken)
        {
            try
            {
                // Build lock value with metadata
                var lockValue = BuildLockValue(options);

                var kvPair = new KVPair(fullKey)
                {
                    Value = Encoding.UTF8.GetBytes(lockValue),
                    Session = sessionId
                };

                var result = await _consulClient.KV.Acquire(kvPair, cancellationToken);

                return result.Response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during lock acquisition attempt: Key={LockKey}, SessionId={SessionId}",
                    fullKey, sessionId);
                return false;
            }
        }

        private string BuildLockValue(ConsulLockOptions options)
        {
            var lockData = new Dictionary<string, string>
            {
                ["AcquiredAt"] = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(_appConfig.TIME_ZONE)).ToString("O"),
                ["InstanceId"] = Environment.MachineName,
                ["ProcessId"] = Environment.ProcessId.ToString()
            };

            if (options.Metadata != null)
            {
                foreach (var kvp in options.Metadata)
                {
                    lockData[kvp.Key] = kvp.Value;
                }
            }

            if (!string.IsNullOrEmpty(options.LockValue))
            {
                lockData["CustomValue"] = options.LockValue;
            }

            return JsonSerializer.Serialize(lockData);
        }

        private Dictionary<string, string> ParseLockValue(byte[]? valueBytes)
        {
            if (valueBytes == null || valueBytes.Length == 0)
                return new Dictionary<string, string>();

            try
            {
                var valueString = Encoding.UTF8.GetString(valueBytes);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(valueString)
                       ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse lock value as JSON, returning empty metadata");
                return new Dictionary<string, string>();
            }
        }
    }
}