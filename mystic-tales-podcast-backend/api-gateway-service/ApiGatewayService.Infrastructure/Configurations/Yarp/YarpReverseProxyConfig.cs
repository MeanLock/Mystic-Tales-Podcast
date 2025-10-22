using Microsoft.Extensions.Configuration;
using ApiGatewayService.Infrastructure.Configurations.Yarp.interfaces;

namespace ApiGatewayService.Infrastructure.Configurations.Yarp
{
    public class YarpReverseProxyConfigModel
    {
        public bool EnableCaching { get; set; } = true;
        public int DefaultCacheTtlMinutes { get; set; } = 5;
        public bool EnableCompression { get; set; } = true;
        public int TimeoutSeconds { get; set; } = 30;
        public long MaxRequestBodySize { get; set; } = 104857600; // 100MB
        public bool EnableRequestBuffering { get; set; } = false;
        
        public YarpLoadBalancingData LoadBalancing { get; set; } = new();
        public YarpHealthCheckData HealthCheck { get; set; } = new();
    }

    public class YarpReverseProxyConfig : IYarpReverseProxyConfig
    {
        public bool EnableCaching { get; set; } = true;
        public int DefaultCacheTtlMinutes { get; set; } = 5;
        public bool EnableCompression { get; set; } = true;
        public int TimeoutSeconds { get; set; } = 30;
        public long MaxRequestBodySize { get; set; } = 104857600; // 100MB
        public bool EnableRequestBuffering { get; set; } = false;
        
        public YarpLoadBalancingData LoadBalancing { get; set; } = new();
        public YarpHealthCheckData HealthCheck { get; set; } = new();

        public YarpReverseProxyConfig(IConfiguration configuration)
        {
            var yarpConfig = configuration.GetSection("Infrastructure:Yarp:ReverseProxy")
                .Get<YarpReverseProxyConfigModel>();
            if (yarpConfig != null)
            {
                EnableCaching = yarpConfig.EnableCaching;
                DefaultCacheTtlMinutes = yarpConfig.DefaultCacheTtlMinutes;
                EnableCompression = yarpConfig.EnableCompression;
                TimeoutSeconds = yarpConfig.TimeoutSeconds;
                MaxRequestBodySize = yarpConfig.MaxRequestBodySize;
                EnableRequestBuffering = yarpConfig.EnableRequestBuffering;
                
                LoadBalancing = yarpConfig.LoadBalancing ?? new YarpLoadBalancingData();
                HealthCheck = yarpConfig.HealthCheck ?? new YarpHealthCheckData();
            }
        }
    }
}
