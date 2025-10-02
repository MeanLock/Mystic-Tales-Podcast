using Microsoft.Extensions.Configuration;
using ApiGatewayService.Infrastructure.Configurations.Consul.interfaces;

namespace ApiGatewayService.Infrastructure.Configurations.Consul
{
    public class ConsulServiceConfigModel
    {
        public string Host { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceScheme { get; set; } = "http";
        public List<string> Tags { get; set; } = new List<string>();
        public string Version { get; set; } = "1.0.0";  // Added for service versioning
        
        // YARP Destination Instance Naming for Consul Services
        public string DestinationInstancePattern { get; set; } = "{serviceName}-{environment}-{instanceNumber:D3}";
        public string DestinationEnvironment { get; set; } = "dev";
    }

    public class ConsulServiceConfig : IConsulServiceConfig
    {
        public string Host { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceScheme { get; set; } = "http";
        public List<string> Tags { get; set; } = new List<string>();
        public string Version { get; set; } = "1.0.0";  // Added for service versioning
        
        // YARP Destination Instance Naming for Consul Services
        public string DestinationInstancePattern { get; set; } = "{serviceName}-{environment}-{instanceNumber:D3}";
        public string DestinationEnvironment { get; set; } = "dev";

        public ConsulServiceConfig(IConfiguration configuration)
        {
            var consulServiceConfig = configuration.GetSection("Infrastructure:Consul:Service")
                .Get<ConsulServiceConfigModel>();
            if (consulServiceConfig != null)
            {
                Host = consulServiceConfig.Host;
                Id = consulServiceConfig.Id;
                ServiceName = consulServiceConfig.ServiceName;
                ServiceScheme = consulServiceConfig.ServiceScheme;
                Tags = consulServiceConfig.Tags;
                Version = consulServiceConfig.Version;  // Added for service versioning
                
                // YARP Destination Instance Naming
                DestinationInstancePattern = consulServiceConfig.DestinationInstancePattern;
                DestinationEnvironment = consulServiceConfig.DestinationEnvironment;
            }
        }
    }
}
