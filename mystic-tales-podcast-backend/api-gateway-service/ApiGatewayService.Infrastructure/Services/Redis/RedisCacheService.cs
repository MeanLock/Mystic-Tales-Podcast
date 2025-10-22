using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;

namespace ApiGatewayService.Infrastructure.Services.Redis
{
    public class RedisCacheService
    {
        private readonly IDatabase _database;
        private readonly IConnectionMultiplexer _redis;
        private readonly IRedisCacheConfig _redisCacheConfig;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheService(
            IConnectionMultiplexer redis,
            IRedisCacheConfig redisCacheConfig,
            IRedisDefaultConfig redisDefaultConfig,
            ILogger<RedisCacheService> logger)
        {
            _redis = redis;
            _database = redis.GetDatabase(redisDefaultConfig.DefaultDatabase);
            _redisCacheConfig = redisCacheConfig;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = GetCacheKey(key);
                var value = await _database.StringGetAsync(cacheKey);
                
                if (!value.HasValue)
                {
                    _logger.LogDebug("Cache miss for key: {Key}", cacheKey);
                    return default;
                }

                _logger.LogDebug("Cache hit for key: {Key}", cacheKey);
                return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
                return default;
            }
        }

        public async Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = GetCacheKey(key);
                var value = await _database.StringGetAsync(cacheKey);
                
                if (!value.HasValue)
                {
                    _logger.LogDebug("Cache miss for key: {Key}", cacheKey);
                    return null;
                }

                _logger.LogDebug("Cache hit for key: {Key}", cacheKey);
                return value!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache string for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = GetCacheKey(key);
                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                var expiryTime = expiry ?? TimeSpan.FromMinutes(_redisCacheConfig.DefaultExpirationMinutes);
                

                await _database.StringSetAsync(cacheKey, serializedValue, expiryTime);
                _logger.LogDebug("Cache set for key: {Key} with expiry: {Expiry}", cacheKey, expiryTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
            }
        }

        public async Task SetStringAsync(string key, string value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = GetCacheKey(key);
                var expiryTime = expiry ?? TimeSpan.FromMinutes(_redisCacheConfig.DefaultExpirationMinutes);
                
                await _database.StringSetAsync(cacheKey, value, expiryTime);
                _logger.LogDebug("Cache string set for key: {Key} with expiry: {Expiry}", cacheKey, expiryTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache string for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = GetCacheKey(key);
                await _database.KeyDeleteAsync(cacheKey);
                _logger.LogDebug("Cache removed for key: {Key}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache for key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var keys = server.Keys(pattern: GetCacheKey(pattern));
                
                foreach (var key in keys)
                {
                    await _database.KeyDeleteAsync(key);
                }
                
                _logger.LogDebug("Cache removed for pattern: {Pattern}", pattern);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache by pattern: {Pattern}", pattern);
            }
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = GetCacheKey(key);
                return await _database.KeyExistsAsync(cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
                return false;
            }
        }

        public async Task<long> IncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = GetCacheKey(key);
                return await _database.StringIncrementAsync(cacheKey, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing cache for key: {Key}", key);
                return 0;
            }
        }

        public async Task<long> DecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = GetCacheKey(key);
                return await _database.StringDecrementAsync(cacheKey, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrementing cache for key: {Key}", key);
                return 0;
            }
        }

        public async Task<TimeSpan?> GetTtlAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = GetCacheKey(key);
                return await _database.KeyTimeToLiveAsync(cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TTL for key: {Key}", key);
                return null;
            }
        }

        public async Task<bool> ExpireAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = GetCacheKey(key);
                return await _database.KeyExpireAsync(cacheKey, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting expiry for key: {Key}", key);
                return false;
            }
        }

        private string GetCacheKey(string key)
        {
            return $"{_redisCacheConfig.KeyPrefix}{key}";
        }
    }
}
