using Microsoft.AspNetCore.Mvc;
using Architecture_1.BusinessLogic.Services.DbServices.FacilityMajorServices;
using Newtonsoft.Json.Linq;
using Architecture_1.Infrastructure.Services.Redis;
using Architecture_1.Infrastructure.Configurations.Redis.interfaces;

namespace Architecture_1.API.Controllers.TestingControllers
{
    [ApiController]
    [Route("api/test/[controller]")]
    public class RedisController : ControllerBase
    {
        private readonly ILogger<RedisController> _logger;

        // Configs
        private readonly IRedisCacheConfig _redisCacheConfig;

        // Services
        private readonly RedisCacheService _redisCacheService;
        private readonly MajorService _majorService;

        public RedisController(IRedisCacheConfig redisCacheConfig, ILogger<RedisController> logger, RedisCacheService redisCacheService, MajorService majorService)
        {
            _redisCacheConfig = redisCacheConfig;
            _logger = logger;
            _redisCacheService = redisCacheService;
            _majorService = majorService;
        }

        // [GET] /api/test/redis/getAllCacheKeyValues
        [HttpGet("getAllCacheKeyValues")]
        public async Task<IActionResult> GetAllCacheKeyValues()
        {
            var cacheKeyValues = await _redisCacheService.GetAllKeyValuesAsync();
            return Ok(new
            {
                CacheKeyValues = cacheKeyValues
            });
        }
        // [GET] /api/test/redis/getCacheValueByKey
        [HttpPost("getCacheValueByKey")]
        public async Task<IActionResult> GetCacheValueByKey([FromBody] JToken jsonData)
        {
            var key = jsonData["key"].ToString();
            var cacheValue = await _redisCacheService.KeyStringGetAsync(key);
            return Ok(new
            {
                CacheValue = cacheValue
            });
        }

        // [GET] /api/test/redis/set-demo-key-values
        [HttpGet("SetDemoKeyValues")]
        public async Task<IActionResult> SetDemoKeyValues()
        {
            await _redisCacheService.KeySetAsync("string-key", "alo 112344");
            await _redisCacheService.KeyHashFieldsOverrideSetAsync("hash-key", new Dictionary<string, string> { { "alo 1", "alo 1" }, { "alo 2", "alo 2" } });
            await _redisCacheService.KeyListOverrideSetAsync("list-key", new List<string> { "alo 1", "alo 2" });
            await _redisCacheService.KeySetOverrideSetAsync("set-key", new List<string> { "alo 1", "alo 2" });
            await _redisCacheService.KeySortedSetOverrideSetAsync("sorted-set-key", new Dictionary<string, double> { { "alo 1", 1 }, { "alo 2", 2 } });

            return Ok(new
            {
                Message = "Demo key values set successfully"
            });
        }

        // [GET] /api/test/redis/no-cache/major-head/{accountId}/majors
        [HttpGet("no-cache/major-head/{accountId:int}/majors")]
        public async Task<IActionResult> GetMajorHeadMajorsNoCache(int accountId)
        {
            var majors = await _majorService.GetMajorHeadMajors(accountId);
            
            return Ok(new
            {
                Majors = majors
            });
        }

        // [GET] /api/test/redis/major-head/{accountId}/majors
        [HttpGet("cache/major-head/{accountId:int}/majors")]
        public async Task<IActionResult> GetMajorHeadMajors(int accountId)
        {
            var cacheValue = await _redisCacheService.KeyGetAsync<JArray>("major-head-majors");
            if (cacheValue != null)
            {
                return Ok(new
                {
                    Source = "Cache",
                    Majors = cacheValue,
                    Expiry = await _redisCacheService.KeyExpiryGetAsync("major-head-majors")
                });
            }
            var majors = await _majorService.GetMajorHeadMajors(accountId);
            await _redisCacheService.KeySetAsync("major-head-majors", majors, TimeSpan.FromSeconds(10));

            return Ok(new
            {
                Source = "Database",
                Majors = majors
            });
        }



    }


}