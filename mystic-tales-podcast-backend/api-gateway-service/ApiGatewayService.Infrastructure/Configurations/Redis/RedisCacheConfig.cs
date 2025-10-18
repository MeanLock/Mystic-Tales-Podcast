using Microsoft.Extensions.Configuration;
using ApiGatewayService.Infrastructure.Configurations.Redis.interfaces;

namespace ApiGatewayService.Infrastructure.Configurations.Redis
{
    public class RedisCacheConfigModel
    {
        public string KeyPrefix { get; set; } = "gateway:cache:";
        public int DefaultExpirationMinutes { get; set; } = 30;
        public bool EnableCompression { get; set; } = false;
        public string SerializationFormat { get; set; } = "JSON";
        
        // YARP Configuration Cache Keys
        public string YarpRoutesKey { get; set; } = "yarp:routes";
        public string YarpClustersKey { get; set; } = "yarp:clusters";
        public string YarpConfigurationKey { get; set; } = "yarp:configuration";
        
        // Service Discovery Cache Keys
        public string ServiceEndpointsKey { get; set; } = "consul:service-endpoints";
        public string StaleKeyPrefix { get; set; } = "stale:";
        
        // Cache TTL Settings (in seconds)
        public int ServiceEndpointsTtlSeconds { get; set; } = 60;
        public int StaleCacheTtlSeconds { get; set; } = 3600; // 1 hour for emergency fallback
    }

    public class RedisCacheConfig : IRedisCacheConfig
    {
        public string KeyPrefix { get; set; } = "gateway:cache:";
        public int DefaultExpirationMinutes { get; set; } = 30;
        public bool EnableCompression { get; set; } = false;
        public string SerializationFormat { get; set; } = "JSON";
        
        // YARP Configuration Cache Keys
        public string YarpRoutesKey { get; set; } = "yarp:routes";
        public string YarpClustersKey { get; set; } = "yarp:clusters";
        public string YarpConfigurationKey { get; set; } = "yarp:configuration";
        
        // Service Discovery Cache Keys
        public string ServiceEndpointsKey { get; set; } = "consul:service-endpoints";
        public string StaleKeyPrefix { get; set; } = "stale:";
        
        // Cache TTL Settings (in seconds)
        public int ServiceEndpointsTtlSeconds { get; set; } = 60;
        public int StaleCacheTtlSeconds { get; set; } = 3600; // 1 hour for emergency fallback

        public RedisCacheConfig(IConfiguration configuration)
        {
            var cacheConfig = configuration.GetSection("Infrastructure:Redis:Cache")
                .Get<RedisCacheConfigModel>();
            if (cacheConfig != null)
            {
                KeyPrefix = cacheConfig.KeyPrefix;
                DefaultExpirationMinutes = cacheConfig.DefaultExpirationMinutes;
                EnableCompression = cacheConfig.EnableCompression;
                SerializationFormat = cacheConfig.SerializationFormat;
                
                // YARP Configuration Cache Keys
                YarpRoutesKey = cacheConfig.YarpRoutesKey;
                YarpClustersKey = cacheConfig.YarpClustersKey;
                YarpConfigurationKey = cacheConfig.YarpConfigurationKey;
                
                // Service Discovery Cache Keys
                ServiceEndpointsKey = cacheConfig.ServiceEndpointsKey;
                StaleKeyPrefix = cacheConfig.StaleKeyPrefix;
                
                // Cache TTL Settings
                ServiceEndpointsTtlSeconds = cacheConfig.ServiceEndpointsTtlSeconds;
                StaleCacheTtlSeconds = cacheConfig.StaleCacheTtlSeconds;
            }
        }
    }
}