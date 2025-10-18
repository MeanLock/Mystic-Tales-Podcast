using Microsoft.Extensions.Configuration;
using TransactionService.Common.Configurations.Consul.interfaces;

namespace TransactionService.Common.Configurations.Consul
{
    public class ConsulServiceConfigModel
    {
        public string Host { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceScheme { get; set; } = "http";
        public List<string> Tags { get; set; } = new List<string>();
    }
    public class ConsulServiceConfig : IConsulServiceConfig
    {
        public string Host { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceScheme { get; set; } = "http";
        public List<string> Tags { get; set; } = new List<string>();

        public ConsulServiceConfig(IConfiguration configuration)
        {
            var consulServiceConfig = configuration.GetSection("Infrastructure:Consul:Service").Get<ConsulServiceConfigModel>();
            if (consulServiceConfig != null)
            {
                Host = consulServiceConfig.Host;
                Id = consulServiceConfig.Id;
                ServiceName = consulServiceConfig.ServiceName;
                ServiceScheme = consulServiceConfig.ServiceScheme;
                Tags = consulServiceConfig.Tags ?? new List<string>();
            }
        }
    }

    
}
