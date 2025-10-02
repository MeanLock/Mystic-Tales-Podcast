using Microsoft.Extensions.Configuration;
using ModerationService.Common.Configurations.Consul.interfaces;

namespace ModerationService.Common.Configurations.Consul
{
    public class ConsulHealthCheckConfigModel
    {
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public int Timeout { get; set; } = 10;
        public int Interval { get; set; } = 30;
        public int DeregisterCriticalServiceAfter { get; set; } = 300;
    }

    public class ConsulHealthCheckConfig : IConsulHealthCheckConfig
    {
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public int Timeout { get; set; } = 10;
        public int Interval { get; set; } = 30;
        public int DeregisterCriticalServiceAfter { get; set; } = 300;

        public ConsulHealthCheckConfig(IConfiguration configuration)
        {
            var consulHealthCheckConfig = configuration.GetSection("Infrastructure:Consul:HealthCheck").Get<ConsulHealthCheckConfigModel>();
            if (consulHealthCheckConfig != null)
            {
                Enabled = consulHealthCheckConfig.Enabled;
                Name = consulHealthCheckConfig.Name;
                Notes = consulHealthCheckConfig.Notes;
                BaseUrl = consulHealthCheckConfig.BaseUrl;
                Timeout = consulHealthCheckConfig.Timeout;
                Interval = consulHealthCheckConfig.Interval;
                DeregisterCriticalServiceAfter = consulHealthCheckConfig.DeregisterCriticalServiceAfter;
            }
        }
    }
}
