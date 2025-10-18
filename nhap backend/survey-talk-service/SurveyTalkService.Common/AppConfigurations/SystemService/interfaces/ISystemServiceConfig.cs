using System.Collections.Generic;

namespace SurveyTalkService.Common.AppConfigurations.SystemService.interfaces
{
    public interface ISystemServiceConfig
    {
        Dictionary<string, ServiceInfo> Services { get; set; }
        ServiceInfo GetServiceInfo(string serviceName);
    }

    public class ServiceInfo
    {
        public string Url { get; set; } = string.Empty;
    }
}