namespace ApiGatewayService.Infrastructure.Configurations.Redis.interfaces
{
    public interface IRedisCacheConfig
    {
        string KeyPrefix { get; set; }
        int DefaultExpirationMinutes { get; set; }
        bool EnableCompression { get; set; }
        string SerializationFormat { get; set; }
        
        // YARP Configuration Cache Keys
        string YarpRoutesKey { get; set; }
        string YarpClustersKey { get; set; }
        string YarpConfigurationKey { get; set; }
        
        // Service Discovery Cache Keys
        string ServiceEndpointsKey { get; set; }
        string StaleKeyPrefix { get; set; }
        
        // Cache TTL Settings (in seconds)
        int ServiceEndpointsTtlSeconds { get; set; }
        int StaleCacheTtlSeconds { get; set; }
    }
}
