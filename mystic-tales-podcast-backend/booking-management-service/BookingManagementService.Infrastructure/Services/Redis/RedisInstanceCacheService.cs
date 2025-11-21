using Microsoft.Extensions.Logging;
using BookingManagementService.Infrastructure.Configurations.Redis.interfaces;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace BookingManagementService.Infrastructure.Services.Redis
{
    public class RedisInstanceCacheService
    {
        // LOGGER
        private readonly ILogger<RedisInstanceCacheService> _logger;

        // CONFIG
        private readonly IRedisDefaultConfig _redisDefaultConfig;
        private readonly IRedisCacheConfig _redisCacheConfig;
        private readonly IRedisSessionConfig _redisSessionConfig;
        private readonly IRedisRateLimitConfig _redisRateLimitConfig;
        private readonly IRedisMessageQueueConfig _redisMessageQueueConfig;
        private readonly IRedisLockConfig _redisLockConfig;
        private readonly IRedisAnalyticsConfig _redisAnalyticsConfig;

        // REDIS
        private readonly IDatabase _redisDb;
        private readonly IConnectionMultiplexer _redis;
        private readonly IServer _redisServer;
        private readonly string _redisPrefix;

        public RedisInstanceCacheService(
            IRedisDefaultConfig redisDefaultConfig,
            IRedisCacheConfig redisCacheConfig,
            IRedisSessionConfig redisSessionConfig,
            IRedisRateLimitConfig redisRateLimitConfig,
            IRedisMessageQueueConfig redisMessageQueueConfig,
            IRedisLockConfig redisLockConfig,
            IRedisAnalyticsConfig redisAnalyticsConfig,
            IConnectionMultiplexer redis,
            ILogger<RedisInstanceCacheService> logger)
        {
            _logger = logger;

            _redisDefaultConfig = redisDefaultConfig;
            _redisCacheConfig = redisCacheConfig;
            _redisSessionConfig = redisSessionConfig;
            _redisRateLimitConfig = redisRateLimitConfig;
            _redisMessageQueueConfig = redisMessageQueueConfig;
            _redisLockConfig = redisLockConfig;
            _redisAnalyticsConfig = redisAnalyticsConfig;

            _redis = redis;
            _redisDb = redis.GetDatabase();
            _redisPrefix = $"{_redisDefaultConfig.InstanceKeyName}:{_redisDefaultConfig.DefaultDatabase}:{_redisCacheConfig.KeyPrefix}";
            _redisServer = redis.GetServer(_redisDefaultConfig.ConnectionString);
        }

        public string GetRedisKey(string key)
        {
            return $"{_redisPrefix}:{key}";
        }

        public string ValueToString<T>(T value)
        {
            if (value is string strValue)
            {
                return strValue;
            }
            else if (value is int intValue)
            {
                return intValue.ToString();
            }
            else if (value is long longValue)
            {
                return longValue.ToString();
            }
            else if (value is double doubleValue)
            {
                return doubleValue.ToString();
            }
            else if (value is bool boolValue)
            {
                return boolValue.ToString();
            }
            else
            {
                return JsonConvert.SerializeObject(value);
            }
        }

        public TimeSpan GetExpirySeconds(TimeSpan? expiry)
        {
            return expiry ?? TimeSpan.FromSeconds(_redisCacheConfig.ExpirySeconds);
        }

        // Class để chứa thông tin về key và giá trị trong Redis
        public class RedisValueInfo
        {
            public string Key { get; set; }
            public string Type { get; set; }
            public object Value { get; set; }
            public TimeSpan? Expiry { get; set; }
            public string Prefix { get; set; }
            public string OriginalKey { get; set; }
        }

        #region Generic Cache Operations
        // Get tất cả các key-value trong Redis cache với thông tin đầy đủ
        public async Task<List<RedisValueInfo>> GetAllKeyValuesAsync()
        {
            var result = new List<RedisValueInfo>();

            await foreach (var key in _redisServer.KeysAsync())
            {
                try
                {
                    var keyString = key.ToString();
                    var type = await _redisDb.KeyTypeAsync(key);
                    var expiry = await _redisDb.KeyTimeToLiveAsync(key);
                    object value = null;

                    // Tách prefix và original key
                    string prefix = null;
                    string originalKey = keyString;
                    if (keyString.Contains(":"))
                    {
                        var parts = keyString.Split(':');
                        prefix = parts[0];
                        originalKey = string.Join(":", parts.Skip(1));
                    }

                    switch (type)
                    {
                        case RedisType.String:
                            value = await _redisDb.StringGetAsync(key);
                            break;
                        case RedisType.Hash:
                            var hashEntries = await _redisDb.HashGetAllAsync(key);
                            value = hashEntries.ToDictionary(
                                entry => entry.Name.ToString(),
                                entry => entry.Value.ToString()
                            );
                            break;
                        case RedisType.List:
                            var listValues = await _redisDb.ListRangeAsync(key);
                            value = listValues.Select(v => v.ToString()).ToList();
                            break;
                        case RedisType.Set:
                            var setValues = await _redisDb.SetMembersAsync(key);
                            value = setValues.Select(v => v.ToString()).ToList();
                            break;
                        case RedisType.SortedSet:
                            var sortedSetEntries = await _redisDb.SortedSetRangeByScoreWithScoresAsync(key);
                            value = sortedSetEntries.ToDictionary(
                                entry => entry.Element.ToString(),
                                entry => entry.Score
                            );
                            break;
                    }

                    result.Add(new RedisValueInfo
                    {
                        Key = keyString,
                        Type = type.ToString(),
                        Value = value,
                        Expiry = expiry,
                        Prefix = prefix,
                        OriginalKey = originalKey
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting value for key: {key}");
                }
            }

            return result;
        }

        // Set giá trị vào Redis cache với thời gian hết hạn tùy chọn
        public async Task<bool> KeySetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var serializedValue = ValueToString(value);
                var expirySeconds = GetExpirySeconds(expiry);
                return await _redisDb.StringSetAsync(redisKey, serializedValue, expirySeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting cache for key: {key}");
                return false;
            }
        }

        // Set giá trị vào Redis cache KHÔNG có thời gian hết hạn (persistent key)
        public async Task<bool> KeySetWithoutExpiryAsync<T>(string key, T value)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var serializedValue = ValueToString(value);
                return await _redisDb.StringSetAsync(redisKey, serializedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting cache without expiry for key: {key}");
                return false;
            }
        }

        // Get giá trị từ Redis cache theo key
        public async Task<T> KeyGetAsync<T>(string key)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var value = await _redisDb.StringGetAsync(redisKey);
                return value.HasValue ? JsonConvert.DeserializeObject<T>(value) : default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting cache for key: {key}");
                return default;
            }
        }

        // Get nhiều giá trị từ Redis cache theo danh sách key
        public async Task<IDictionary<string, T>> MultipleKeysGetAsync<T>(IEnumerable<string> keys)
        {
            try
            {
                var redisKeys = keys.Select(k => (RedisKey)GetRedisKey(k)).ToArray();
                var values = await _redisDb.StringGetAsync(redisKeys);
                var result = new Dictionary<string, T>();

                for (int i = 0; i < keys.Count(); i++)
                {
                    if (values[i].HasValue)
                    {
                        result[keys.ElementAt(i)] = JsonConvert.DeserializeObject<T>(values[i]);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting multiple cache entries");
                return new Dictionary<string, T>();
            }
        }

        // Set nhiều cặp key-value vào Redis cache với thời gian hết hạn tùy chọn
        public async Task<bool> KeyPairsSetAsync<T>(IDictionary<string, T> keyValuePairs, TimeSpan? expiry = null)
        {
            try
            {
                var batch = _redisDb.CreateBatch();
                var expirySeconds = GetExpirySeconds(expiry);

                foreach (var kvp in keyValuePairs)
                {
                    var redisKey = GetRedisKey(kvp.Key);
                    var serializedValue = ValueToString(kvp.Value);
                    batch.StringSetAsync(redisKey, serializedValue, expirySeconds);
                }

                await batch.ExecuteAsync("BATCH");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting multiple cache entries");
                return false;
            }
        }
        #endregion

        #region Cache Management
        // Delete một key khỏi Redis cache
        public async Task<bool> KeyDeleteAsync(string key)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                return await _redisDb.KeyDeleteAsync(redisKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing cache for key: {key}");
                return false;
            }
        }

        // Delete nhiều key khỏi Redis cache
        public async Task<bool> MultipleKeysDeleteAsync(IEnumerable<string> keys)
        {
            try
            {
                var redisKeys = keys.Select(k => (RedisKey)GetRedisKey(k)).ToArray();
                return await _redisDb.KeyDeleteAsync(redisKeys) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing multiple cache entries");
                return false;
            }
        }

        // Kiểm tra xem một key có tồn tại trong Redis cache không
        public async Task<bool> IsKeyExistsAsync(string key)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                return await _redisDb.KeyExistsAsync(redisKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking cache existence for key: {key}");
                return false;
            }
        }

        // Set thời gian hết hạn cho một key trong Redis cache
        public async Task<bool> KeyExpirySetAsync(string key, TimeSpan? expiry)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var expirySeconds = GetExpirySeconds(expiry);
                return await _redisDb.KeyExpireAsync(redisKey, expirySeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting expiry for key: {key}");
                return false;
            }
        }

        // Get thời gian hết hạn còn lại của một key trong Redis cache
        public async Task<TimeSpan?> KeyExpiryGetAsync(string key)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                return await _redisDb.KeyTimeToLiveAsync(redisKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting expiry for key: {key}");
                return null;
            }
        }
        #endregion

        #region String Operations
        // Set một giá trị string vào Redis cache với thời gian hết hạn tùy chọn
        public async Task<bool> KeyStringSetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var jsonString = ValueToString(value);
                var expirySeconds = GetExpirySeconds(expiry);
                return await _redisDb.StringSetAsync(redisKey, jsonString, expirySeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting string value for key: {key}");
                return false;
            }
        }

        // Get một giá trị string từ Redis cache theo key
        public async Task<string> KeyStringGetAsync(string key)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var value = await _redisDb.StringGetAsync(redisKey);
                return value.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting string value for key: {key}");
                return null;
            }
        }

        // Delete một key string khỏi Redis cache
        public async Task<bool> KeyStringDeleteAsync(string key)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                return await _redisDb.KeyDeleteAsync(redisKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting string key: {key}");
                return false;
            }
        }
        #endregion

        #region Hash Operations
        // Set một cặp field-value vào Redis Hash
        public async Task<bool> KeyHashFieldSetAsync<T>(string key, string hashField, T value)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                string jsonString = ValueToString(value);
                return await _redisDb.HashSetAsync(redisKey, hashField, jsonString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting hash field {hashField} for key: {key}");
                return false;
            }
        }

        // Get giá trị của một field từ Redis Hash
        public async Task<string> KeyHashFieldGetAsync(string key, string hashField)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var value = await _redisDb.HashGetAsync(redisKey, hashField);
                return value.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting hash field {hashField} for key: {key}");
                return null;
            }
        }

        // Get tất cả các cặp field-value từ Redis Hash
        public async Task<Dictionary<string, string>> KeyAllHashFieldsGetAsync(string key)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var hashEntries = await _redisDb.HashGetAllAsync(redisKey);
                var result = new Dictionary<string, string>();
                foreach (var entry in hashEntries)
                {
                    result.Add(entry.Name.ToString(), entry.Value.ToString());
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting all hash fields for key: {key}");
                return null;
            }
        }

        // Set nhiều cặp field-value vào Redis Hash với thời gian hết hạn tùy chọn
        public async Task<bool> KeyHashFieldsSetAsync<T>(string key, IDictionary<string, T> hashFields, TimeSpan? expiry = null)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var tasks = new List<Task>();

                // Tạo các task riêng lẻ thay vì sử dụng batch
                foreach (var field in hashFields)
                {
                    string jsonString = ValueToString(field.Value);
                    tasks.Add(_redisDb.HashSetAsync(redisKey, field.Key, jsonString));
                }

                // Thực thi tất cả các task
                await Task.WhenAll(tasks);


                var expirySeconds = GetExpirySeconds(expiry);
                await _redisDb.KeyExpireAsync(redisKey, expirySeconds);


                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting multiple hash fields for key: {key}");
                return false;
            }
        }

        // Delete nhiều field khỏi Redis Hash
        public async Task<bool> KeyHashFieldsDeleteAsync(string key, IEnumerable<string> hashFields)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var tasks = new List<Task>();

                // Tạo các task riêng lẻ thay vì sử dụng batch
                foreach (var field in hashFields)
                {
                    tasks.Add(_redisDb.HashDeleteAsync(redisKey, field));
                }

                // Thực thi tất cả các task
                await Task.WhenAll(tasks);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting hash fields for key: {key}");
                return false;
            }
        }

        // Override tất cả các field trong Redis Hash với các cặp field-value mới
        public async Task<bool> KeyHashFieldsOverrideSetAsync(string key, IDictionary<string, string> newHashFields)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                // Xóa tất cả các field cũ
                await _redisDb.KeyDeleteAsync(redisKey);
                // Thêm các field mới
                return await KeyHashFieldsSetAsync(key, newHashFields);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error overriding hash for key: {key}");
                return false;
            }
        }
        #endregion

        #region List Operations
        // Get một dải giá trị từ Redis List
        public async Task<string[]> KeyListRangeGetAsync(string key, long start = 0, long stop = -1)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var values = await _redisDb.ListRangeAsync(redisKey, start, stop);
                return Array.ConvertAll(values, x => x.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting range from list: {key}");
                return null;
            }
        }

        // Push một giá trị vào bên trái của Redis List
        public async Task<long> KeyListLeftPushAsync<T>(string key, T value)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                string jsonString = ValueToString(value);
                return await _redisDb.ListLeftPushAsync(redisKey, jsonString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error pushing value to list: {key}");
                return -1;
            }
        }

        // Pop một giá trị từ bên trái của Redis List
        public async Task<string> KeyListLeftPopAsync(string key)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var value = await _redisDb.ListLeftPopAsync(redisKey);
                return value.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error popping value from list: {key}");
                return null;
            }
        }

        // Push nhiều giá trị vào bên trái của Redis List
        public async Task<long> KeyListLeftPushAsync<T>(string key, IEnumerable<T> values)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var tasks = new List<Task<long>>();

                // Tạo các task riêng lẻ thay vì sử dụng batch
                foreach (var value in values)
                {
                    string jsonString = ValueToString(value);
                    tasks.Add(_redisDb.ListLeftPushAsync(redisKey, jsonString));
                }

                // Thực thi tất cả các task
                var results = await Task.WhenAll(tasks);
                return results.Sum();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error pushing multiple values to list: {key}");
                return -1;
            }
        }

        // Push một giá trị vào bên phải của Redis List
        public async Task<long> KeyListRightPushAsync<T>(string key, T value)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                string jsonString = ValueToString(value);
                return await _redisDb.ListRightPushAsync(redisKey, jsonString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error pushing value to list: {key}");
                return -1;
            }
        }

        // Pop một giá trị từ bên phải của Redis List
        public async Task<string> KeyListRightPopAsync(string key)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var value = await _redisDb.ListRightPopAsync(redisKey);
                return value.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error popping value from list: {key}");
                return null;
            }
        }

        // Push nhiều giá trị vào bên phải của Redis List
        public async Task<long> KeyListRightPushAsync<T>(string key, IEnumerable<T> values)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var tasks = new List<Task<long>>();

                // Tạo các task riêng lẻ thay vì sử dụng batch
                foreach (var value in values)
                {
                    string jsonString = ValueToString(value);
                    tasks.Add(_redisDb.ListRightPushAsync(redisKey, jsonString));
                }

                // Thực thi tất cả các task
                var results = await Task.WhenAll(tasks);
                return results.Sum();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error pushing multiple values to list: {key}");
                return -1;
            }
        }

        // Override tất cả các giá trị trong Redis List với các giá trị mới
        public async Task<bool> KeyListOverrideSetAsync<T>(string key, IEnumerable<T> newValues)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                // Xóa list cũ
                await _redisDb.KeyDeleteAsync(redisKey);
                // Thêm các giá trị mới
                var values = newValues.Select(v => ValueToString(v));
                var result = await KeyListLeftPushAsync(key, values);
                return result >= 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error overriding list for key: {key}");
                return false;
            }
        }

        // Delete một giá trị khỏi Redis List
        public async Task<long> KeyListRemoveAsync<T>(string key, T value)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                string jsonString = ValueToString(value);
                return await _redisDb.ListRemoveAsync(redisKey, jsonString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing value from list: {key}");
                return -1;
            }
        }

        // Delete nhiều giá trị khỏi Redis List
        public async Task<long> KeyListRemoveAsync<T>(string key, IEnumerable<T> values)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var batch = _redisDb.CreateBatch();
                var count = 0;
                foreach (var value in values)
                {
                    string jsonString = ValueToString(value);
                    batch.ListRemoveAsync(redisKey, jsonString);
                    count++;
                }
                await batch.ExecuteAsync("BATCH");
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing multiple values from list: {key}");
                return -1;
            }
        }
        #endregion

        #region Set Operations
        // Add một giá trị vào Redis Set
        public async Task<bool> KeySetAddAsync<T>(string key, T value)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                string jsonString = ValueToString(value);
                return await _redisDb.SetAddAsync(redisKey, jsonString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding value to set: {key}");
                return false;
            }
        }

        // Get tất cả các thành viên của Redis Set
        public async Task<string[]> KeySetGetAsync(string key)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var values = await _redisDb.SetMembersAsync(redisKey);
                return Array.ConvertAll(values, x => x.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting members from set: {key}");
                return null;
            }
        }

        // Add nhiều giá trị vào Redis Set
        public async Task<long> KeySetAddAsync<T>(string key, IEnumerable<T> values)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var tasks = new List<Task<bool>>();

                // Tạo các task riêng lẻ thay vì sử dụng batch
                foreach (var value in values)
                {
                    string jsonString = ValueToString(value);
                    tasks.Add(_redisDb.SetAddAsync(redisKey, jsonString));
                }

                // Thực thi tất cả các task
                var results = await Task.WhenAll(tasks);
                return results.Count(x => x);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding multiple values to set: {key}");
                return -1;
            }
        }

        // Remove nhiều giá trị khỏi Redis Set
        public async Task<long> KeySetRemoveAsync<T>(string key, IEnumerable<T> values)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var tasks = new List<Task<bool>>();

                // Tạo các task riêng lẻ thay vì sử dụng batch
                foreach (var value in values)
                {
                    string jsonString = ValueToString(value);
                    tasks.Add(_redisDb.SetRemoveAsync(redisKey, jsonString));
                }

                // Thực thi tất cả các task
                var results = await Task.WhenAll(tasks);
                return results.Count(x => x);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing multiple values from set: {key}");
                return -1;
            }
        }

        // Remove một thành viên khỏi Redis Set
        public async Task<bool> KeySetRemoveAsync<T>(string key, T value)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                string jsonString = ValueToString(value);
                return await _redisDb.SetRemoveAsync(redisKey, jsonString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing member from set: {key}");
                return false;
            }
        }

        // Override tất cả các giá trị trong Redis Set với các giá trị mới
        public async Task<bool> KeySetOverrideSetAsync<T>(string key, IEnumerable<T> newValues)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                // Xóa set cũ
                await _redisDb.KeyDeleteAsync(redisKey);
                // Thêm các giá trị mới
                var values = newValues.Select(v => ValueToString(v));
                var result = await KeySetAddAsync(key, values);
                return result >= 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error overriding set for key: {key}");
                return false;
            }
        }
        #endregion

        #region Sorted Set Operations
        // Add một thành viên với điểm số vào Redis Sorted Set
        public async Task<bool> KeySortedSetAddAsync<T>(string key, T member, double score)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                string jsonString = ValueToString(member);
                return await _redisDb.SortedSetAddAsync(redisKey, jsonString, score);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding member to sorted set: {key}");
                return false;
            }
        }

        // Get điểm số của một thành viên trong Redis Sorted Set
        public async Task<double?> KeySortedSetScoreGetAsync<T>(string key, T member)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                string jsonString = ValueToString(member);
                return await _redisDb.SortedSetScoreAsync(redisKey, jsonString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting score from sorted set: {key}");
                return null;
            }
        }

        // Get range of members by score from a Redis Sorted Set
        public async Task<string[]> KeySortedSetRangeByScoreGetAsync(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var values = await _redisDb.SortedSetRangeByScoreAsync(redisKey, start, stop);
                return Array.ConvertAll(values, x => x.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting range from sorted set: {key}");
                return null;
            }
        }

        // Add multiple members with scores to a Redis Sorted Set
        public async Task<long> KeySortedSetAddAsync<T>(string key, IDictionary<T, double> members)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var tasks = new List<Task<bool>>();

                // Tạo các task riêng lẻ thay vì sử dụng batch
                foreach (var member in members)
                {
                    string jsonString = ValueToString(member.Key);
                    tasks.Add(_redisDb.SortedSetAddAsync(redisKey, jsonString, member.Value));
                }

                // Thực thi tất cả các task
                var results = await Task.WhenAll(tasks);
                return results.Count(x => x);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding multiple members to sorted set: {key}");
                return -1;
            }
        }

        // Remove multiple members from a Redis Sorted Set
        public async Task<long> KeySortedSetRemoveAsync<T>(string key, IEnumerable<T> members)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                var tasks = new List<Task<bool>>();

                // Tạo các task riêng lẻ thay vì sử dụng batch
                foreach (var member in members)
                {
                    string jsonString = ValueToString(member);
                    tasks.Add(_redisDb.SortedSetRemoveAsync(redisKey, jsonString));
                }

                // Thực thi tất cả các task
                var results = await Task.WhenAll(tasks);
                return results.Count(x => x);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing multiple members from sorted set: {key}");
                return -1;
            }
        }

        // Override tất cả các thành viên trong Redis Sorted Set với các thành viên và điểm số mới
        public async Task<bool> KeySortedSetOverrideSetAsync<T>(string key, IDictionary<T, double> newMembers)
        {
            try
            {
                var redisKey = GetRedisKey(key);
                // Xóa sorted set cũ
                await _redisDb.KeyDeleteAsync(redisKey);
                // Thêm các member mới
                var result = await KeySortedSetAddAsync(key, newMembers);
                return result >= 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error overriding sorted set for key: {key}");
                return false;
            }
        }
        #endregion

    }
}
