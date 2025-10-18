using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;

namespace ApiGatewayService.Infrastructure.Services.Redis
{
    public class RedisLockService
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisLockService> _logger;
        private readonly IRedisLockConfig _redisLockConfig;
        private readonly string _keyPrefix;

        public RedisLockService(
            IConnectionMultiplexer connectionMultiplexer,
            ILogger<RedisLockService> logger,
            IRedisLockConfig redisLockConfig,
            IRedisDefaultConfig redisDefaultConfig)
        {
            _database = connectionMultiplexer.GetDatabase(redisDefaultConfig.DefaultDatabase);
            _logger = logger;
            _redisLockConfig = redisLockConfig;
            _keyPrefix = $"{redisLockConfig.LockKeyPrefix}:lock:";
        }

        public async Task<RedisLock?> AcquireLockAsync(string lockKey, TimeSpan expiry, CancellationToken cancellationToken = default)
        {
            return await AcquireLockAsync(lockKey, expiry, TimeSpan.FromSeconds(_redisLockConfig.DefaultLockTimeoutSeconds), cancellationToken);
        }

        public async Task<RedisLock?> AcquireLockAsync(string lockKey, TimeSpan expiry, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(lockKey))
                throw new ArgumentException("Lock key cannot be null or empty", nameof(lockKey));

            var lockValue = Guid.NewGuid().ToString();
            var fullKey = $"{_keyPrefix}{lockKey}";
            var deadline = DateTime.UtcNow.Add(timeout);

            try
            {
                while (DateTime.UtcNow < deadline && !cancellationToken.IsCancellationRequested)
                {
                    var acquired = await _database.StringSetAsync(fullKey, lockValue, expiry, When.NotExists);
                    
                    if (acquired)
                    {
                        _logger.LogDebug("Acquired lock {LockKey} with value {LockValue}, expiry {Expiry}", 
                            lockKey, lockValue, expiry);
                        
                        return new RedisLock(this, lockKey, lockValue, expiry, _logger);
                    }

                    // Wait before retry
                    await Task.Delay(TimeSpan.FromMilliseconds(_redisLockConfig.LockRetryDelayMs), cancellationToken);
                }

                _logger.LogDebug("Failed to acquire lock {LockKey} within timeout {Timeout}", lockKey, timeout);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acquiring lock {LockKey}", lockKey);
                throw;
            }
        }

        public async Task<bool> ReleaseLockAsync(string lockKey, string lockValue, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(lockKey))
                throw new ArgumentException("Lock key cannot be null or empty", nameof(lockKey));
            if (string.IsNullOrWhiteSpace(lockValue))
                throw new ArgumentException("Lock value cannot be null or empty", nameof(lockValue));

            try
            {
                var fullKey = $"{_keyPrefix}{lockKey}";
                
                // Lua script to ensure we only delete if the value matches (atomic operation)
                const string script = @"
                    if redis.call('GET', KEYS[1]) == ARGV[1] then
                        return redis.call('DEL', KEYS[1])
                    else
                        return 0
                    end";

                var result = await _database.ScriptEvaluateAsync(script, new RedisKey[] { fullKey }, new RedisValue[] { lockValue });
                var released = (int)result == 1;

                if (released)
                {
                    _logger.LogDebug("Released lock {LockKey} with value {LockValue}", lockKey, lockValue);
                }
                else
                {
                    _logger.LogWarning("Failed to release lock {LockKey} - value mismatch or already expired", lockKey);
                }

                return released;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing lock {LockKey}", lockKey);
                throw;
            }
        }

        public async Task<bool> IsLockedAsync(string lockKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(lockKey))
                throw new ArgumentException("Lock key cannot be null or empty", nameof(lockKey));

            try
            {
                var fullKey = $"{_keyPrefix}{lockKey}";
                var exists = await _database.KeyExistsAsync(fullKey);
                
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking lock status for {LockKey}", lockKey);
                return false;
            }
        }

        public async Task<bool> ExtendLockAsync(string lockKey, string lockValue, TimeSpan extension, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(lockKey))
                throw new ArgumentException("Lock key cannot be null or empty", nameof(lockKey));
            if (string.IsNullOrWhiteSpace(lockValue))
                throw new ArgumentException("Lock value cannot be null or empty", nameof(lockValue));

            try
            {
                var fullKey = $"{_keyPrefix}{lockKey}";
                
                // Lua script to extend lock only if value matches
                const string script = @"
                    if redis.call('GET', KEYS[1]) == ARGV[1] then
                        return redis.call('EXPIRE', KEYS[1], ARGV[2])
                    else
                        return 0
                    end";

                var result = await _database.ScriptEvaluateAsync(script, 
                    new RedisKey[] { fullKey }, 
                    new RedisValue[] { lockValue, (int)extension.TotalSeconds });
                
                var extended = (int)result == 1;

                if (extended)
                {
                    _logger.LogDebug("Extended lock {LockKey} by {Extension}", lockKey, extension);
                }
                else
                {
                    _logger.LogWarning("Failed to extend lock {LockKey} - value mismatch or already expired", lockKey);
                }

                return extended;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending lock {LockKey}", lockKey);
                throw;
            }
        }
    }

    public class RedisLock : IAsyncDisposable
    {
        private readonly RedisLockService _lockService;
        private readonly ILogger _logger;
        private bool _disposed;

        public string LockKey { get; }
        public string LockValue { get; }
        public TimeSpan Expiry { get; }
        public DateTime AcquiredAt { get; }
        public bool IsAcquired { get; private set; }

        public RedisLock(RedisLockService lockService, string lockKey, string lockValue, TimeSpan expiry, ILogger logger)
        {
            _lockService = lockService;
            _logger = logger;
            LockKey = lockKey;
            LockValue = lockValue;
            Expiry = expiry;
            AcquiredAt = DateTime.UtcNow;
            IsAcquired = true;
        }

        public async Task<bool> ExtendAsync(TimeSpan extension, CancellationToken cancellationToken = default)
        {
            if (_disposed || !IsAcquired)
                return false;

            var result = await _lockService.ExtendLockAsync(LockKey, LockValue, extension, cancellationToken);
            return result;
        }

        public async Task<bool> ReleaseAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed || !IsAcquired)
                return false;

            var result = await _lockService.ReleaseLockAsync(LockKey, LockValue, cancellationToken);
            if (result)
            {
                IsAcquired = false;
            }
            return result;
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                if (IsAcquired)
                {
                    try
                    {
                        await ReleaseAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error releasing lock {LockKey} during disposal", LockKey);
                    }
                }
                _disposed = true;
            }
        }
    }
}
