using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;

namespace ApiGatewayService.Infrastructure.Services.Redis
{
    public class RedisSessionService
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisSessionService> _logger;
        private readonly IRedisSessionConfig _redisSessionConfig;
        private readonly string _keyPrefix;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisSessionService(
            IConnectionMultiplexer connectionMultiplexer,
            ILogger<RedisSessionService> logger,
            IRedisSessionConfig redisSessionConfig,
            IRedisDefaultConfig redisDefaultConfig)
        {
            _database = connectionMultiplexer.GetDatabase(redisDefaultConfig.DefaultDatabase);
            _logger = logger;
            _redisSessionConfig = redisSessionConfig;
            _keyPrefix = $"{redisSessionConfig.SessionKeyPrefix}:session:";
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<T?> GetSessionDataAsync<T>(string sessionId, string key, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            try
            {
                var sessionKey = $"{_keyPrefix}{sessionId}";
                var value = await _database.HashGetAsync(sessionKey, key);

                if (!value.HasValue)
                {
                    _logger.LogDebug("Session data not found for session {SessionId}, key {Key}", sessionId, key);
                    return null;
                }

                var result = JsonSerializer.Deserialize<T>(value!, _jsonOptions);
                _logger.LogDebug("Retrieved session data for session {SessionId}, key {Key}", sessionId, key);
                
                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing session data for session {SessionId}, key {Key}", sessionId, key);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session data for session {SessionId}, key {Key}", sessionId, key);
                throw;
            }
        }

        public async Task SetSessionDataAsync<T>(string sessionId, string key, T value, CancellationToken cancellationToken = default) where T : class
        {
            await SetSessionDataAsync(sessionId, key, value, TimeSpan.FromMinutes(_redisSessionConfig.SessionTimeoutMinutes), cancellationToken);
        }

        public async Task SetSessionDataAsync<T>(string sessionId, string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            try
            {
                var sessionKey = $"{_keyPrefix}{sessionId}";
                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);

                var transaction = _database.CreateTransaction();
                var setTask = transaction.HashSetAsync(sessionKey, key, serializedValue);
                var expireTask = transaction.KeyExpireAsync(sessionKey, expiry);

                await transaction.ExecuteAsync();
                await Task.WhenAll(setTask, expireTask);

                _logger.LogDebug("Set session data for session {SessionId}, key {Key}, expiry {Expiry}", 
                    sessionId, key, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting session data for session {SessionId}, key {Key}", sessionId, key);
                throw;
            }
        }

        public async Task RemoveSessionDataAsync(string sessionId, string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            try
            {
                var sessionKey = $"{_keyPrefix}{sessionId}";
                await _database.HashDeleteAsync(sessionKey, key);

                _logger.LogDebug("Removed session data for session {SessionId}, key {Key}", sessionId, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing session data for session {SessionId}, key {Key}", sessionId, key);
                throw;
            }
        }

        public async Task RemoveSessionAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));

            try
            {
                var sessionKey = $"{_keyPrefix}{sessionId}";
                await _database.KeyDeleteAsync(sessionKey);

                _logger.LogInformation("Removed session {SessionId}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing session {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<bool> SessionExistsAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));

            try
            {
                var sessionKey = $"{_keyPrefix}{sessionId}";
                var exists = await _database.KeyExistsAsync(sessionKey);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking session existence for {SessionId}", sessionId);
                return false;
            }
        }

        public async Task RefreshSessionAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            await RefreshSessionAsync(sessionId, TimeSpan.FromMinutes(_redisSessionConfig.SessionTimeoutMinutes), cancellationToken);
        }

        public async Task RefreshSessionAsync(string sessionId, TimeSpan expiry, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));

            try
            {
                var sessionKey = $"{_keyPrefix}{sessionId}";
                await _database.KeyExpireAsync(sessionKey, expiry);

                _logger.LogDebug("Refreshed session {SessionId} with expiry {Expiry}", sessionId, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing session {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<Dictionary<string, string>> GetAllSessionDataAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));

            try
            {
                var sessionKey = $"{_keyPrefix}{sessionId}";
                var data = await _database.HashGetAllAsync(sessionKey);

                var result = new Dictionary<string, string>();
                foreach (var item in data)
                {
                    result[item.Name!] = item.Value!;
                }

                _logger.LogDebug("Retrieved all session data for session {SessionId}, {Count} items", 
                    sessionId, result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all session data for session {SessionId}", sessionId);
                throw;
            }
        }

        public async Task<string[]> GetSessionKeysAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be null or empty", nameof(sessionId));

            try
            {
                var sessionKey = $"{_keyPrefix}{sessionId}";
                var keys = await _database.HashKeysAsync(sessionKey);

                var result = keys.Select(k => k.ToString()).ToArray();
                
                _logger.LogDebug("Retrieved session keys for session {SessionId}, {Count} keys", 
                    sessionId, result.Length);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session keys for session {SessionId}", sessionId);
                throw;
            }
        }
    }
}
