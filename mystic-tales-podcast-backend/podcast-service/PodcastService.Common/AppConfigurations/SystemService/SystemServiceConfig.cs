using Microsoft.Extensions.Configuration;
using PodcastService.Common.AppConfigurations.SystemService.interfaces;
using System.Collections.Generic;

namespace PodcastService.Common.AppConfigurations.SystemService
{
    public class SystemServiceConfigModel
    {
        public Dictionary<string, ServiceInfo> Services { get; set; } = new Dictionary<string, ServiceInfo>();
    }
    public class SystemServiceConfig : ISystemServiceConfig
    {
        public Dictionary<string, ServiceInfo> Services { get; set; } = new Dictionary<string, ServiceInfo>();

        public SystemServiceConfig(IConfiguration configuration)
        {
            var systemServiceConfig = configuration.GetSection("SystemService").Get<SystemServiceConfigModel>();
            if (systemServiceConfig != null)
            {
                Services = systemServiceConfig.Services;
            }
        }

        public ServiceInfo GetServiceInfo(string serviceName)
        {
            if (Services.TryGetValue(serviceName, out var serviceInfo))
            {
                return serviceInfo;
            }
            throw new KeyNotFoundException($"Service '{serviceName}' not found in configuration.");
        }

    }
}
