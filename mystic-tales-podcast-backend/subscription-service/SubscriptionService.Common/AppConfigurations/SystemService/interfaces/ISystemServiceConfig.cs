using System.Collections.Generic;

namespace SubscriptionService.Common.AppConfigurations.SystemService.interfaces
{
    public interface ISystemServiceConfig
    {
        Dictionary<string, ServiceInfo> Services { get; set; }
        ServiceInfo GetServiceInfo(string serviceName);
    }

    public class ServiceInfo
    {
        public string Url { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
    }
}