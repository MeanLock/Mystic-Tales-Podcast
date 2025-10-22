using Microsoft.Extensions.Configuration;
using ApiGatewayService.Infrastructure.Configurations.Consul.interfaces;

namespace ApiGatewayService.Infrastructure.Configurations.Consul
{
    public class ConsulHealthCheckConfigModel
    {
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;  // Will be set from APP_BASE_URL or config
        public int IntervalSeconds { get; set; } = 10;
        public int TimeoutSeconds { get; set; } = 5;
        public int DeregisterCriticalServiceAfter { get; set; } = 300;
    }

    public class ConsulHealthCheckConfig : IConsulHealthCheckConfig
    {
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;  // Will be set from APP_BASE_URL or config
        public int IntervalSeconds { get; set; } = 10;
        public int TimeoutSeconds { get; set; } = 5;
        public int DeregisterCriticalServiceAfter { get; set; } = 300;

        public ConsulHealthCheckConfig(IConfiguration configuration)
        {
            var consulHealthCheckConfig = configuration.GetSection("Infrastructure:Consul:HealthCheck")
                .Get<ConsulHealthCheckConfigModel>();
            if (consulHealthCheckConfig != null)
            {
                Enabled = consulHealthCheckConfig.Enabled;
                Name = consulHealthCheckConfig.Name;
                Notes = consulHealthCheckConfig.Notes;
                BaseUrl = consulHealthCheckConfig.BaseUrl;  // Changed from Host to BaseUrl
                IntervalSeconds = consulHealthCheckConfig.IntervalSeconds;
                TimeoutSeconds = consulHealthCheckConfig.TimeoutSeconds;
                DeregisterCriticalServiceAfter = consulHealthCheckConfig.DeregisterCriticalServiceAfter;
            }
        }
    }
}
