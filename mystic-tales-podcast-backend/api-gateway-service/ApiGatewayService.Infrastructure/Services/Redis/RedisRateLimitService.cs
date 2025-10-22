using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;

namespace ApiGatewayService.Infrastructure.Services.Redis
{
    public class RedisRateLimitService
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisRateLimitService> _logger;
        private readonly IRedisRateLimitConfig _redisRateLimitConfig;
        private readonly string _keyPrefix;

        public RedisRateLimitService(
            IConnectionMultiplexer connectionMultiplexer,
            ILogger<RedisRateLimitService> logger,
            IRedisRateLimitConfig redisRateLimitConfig,
            IRedisDefaultConfig redisDefaultConfig)
        {
            _database = connectionMultiplexer.GetDatabase(redisDefaultConfig.DefaultDatabase);
            _logger = logger;
            _redisRateLimitConfig = redisRateLimitConfig;
            _keyPrefix = $"{redisRateLimitConfig.RateLimitKeyPrefix}:ratelimit:";
        }

        public async Task<(bool IsAllowed, long RemainingRequests, TimeSpan ResetTime)> IsRequestAllowedAsync(
            string identifier, 
            int limit, 
            TimeSpan window, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("Identifier cannot be null or empty", nameof(identifier));

            try
            {
                var key = $"{_keyPrefix}{identifier}";
                var windowSeconds = (long)window.TotalSeconds;
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var windowStart = now - windowSeconds;

                // Use sorted set to track requests in time window
                var transaction = _database.CreateTransaction();
                
                // Remove old entries outside the window
                var removeOldTask = transaction.SortedSetRemoveRangeByScoreAsync(key, 0, windowStart);
                
                // Add current request
                var addRequestTask = transaction.SortedSetAddAsync(key, now, now);
                
                // Get current count
                var countTask = transaction.SortedSetLengthAsync(key);
                
                // Set expiry
                var expireTask = transaction.KeyExpireAsync(key, window);

                await transaction.ExecuteAsync();

                await Task.WhenAll(removeOldTask, addRequestTask, expireTask);
                var currentCount = await countTask;

                var isAllowed = currentCount <= limit;
                var remainingRequests = Math.Max(0, limit - currentCount);
                var resetTime = TimeSpan.FromSeconds(windowSeconds - (now % windowSeconds));

                _logger.LogDebug(
                    "Rate limit check for {Identifier}: {CurrentCount}/{Limit}, Allowed: {IsAllowed}, Reset in: {ResetTime}",
                    identifier, currentCount, limit, isAllowed, resetTime);

                return (isAllowed, remainingRequests, resetTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking rate limit for identifier: {Identifier}", identifier);
                // Fail open - allow request if Redis is unavailable
                return (true, 0, TimeSpan.Zero);
            }
        }

        public async Task<(bool IsAllowed, long RemainingRequests, TimeSpan ResetTime)> IsRequestAllowedAsync(
            string identifier, 
            CancellationToken cancellationToken = default)
        {
            return await IsRequestAllowedAsync(
                identifier,
                _redisRateLimitConfig.DefaultLimit,
                TimeSpan.FromMinutes(_redisRateLimitConfig.WindowMinutes),
                cancellationToken);
        }

        public async Task ResetLimitAsync(string identifier, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("Identifier cannot be null or empty", nameof(identifier));

            try
            {
                var key = $"{_keyPrefix}{identifier}";
                await _database.KeyDeleteAsync(key);
                
                _logger.LogInformation("Rate limit reset for identifier: {Identifier}", identifier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting rate limit for identifier: {Identifier}", identifier);
                throw;
            }
        }

        public async Task<long> GetCurrentCountAsync(string identifier, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("Identifier cannot be null or empty", nameof(identifier));

            try
            {
                var key = $"{_keyPrefix}{identifier}";
                var windowSeconds = _redisRateLimitConfig.WindowMinutes * 60;
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var windowStart = now - windowSeconds;

                // Remove old entries and get count
                await _database.SortedSetRemoveRangeByScoreAsync(key, 0, windowStart);
                var count = await _database.SortedSetLengthAsync(key);

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current count for identifier: {Identifier}", identifier);
                return 0;
            }
        }

        public async Task<TimeSpan?> GetResetTimeAsync(string identifier, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentException("Identifier cannot be null or empty", nameof(identifier));

            try
            {
                var key = $"{_keyPrefix}{identifier}";
                var ttl = await _database.KeyTimeToLiveAsync(key);
                
                return ttl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reset time for identifier: {Identifier}", identifier);
                return null;
            }
        }
    }
}
