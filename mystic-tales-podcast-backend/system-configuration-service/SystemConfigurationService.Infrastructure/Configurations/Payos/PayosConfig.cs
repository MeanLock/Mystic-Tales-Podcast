using Microsoft.Extensions.Configuration;
using SystemConfigurationService.Infrastructure.Configurations.Payos.interfaces;

namespace SystemConfigurationService.Infrastructure.Configurations.Payos
{
    public class PayosConfigModel
    {
        public string ClientID { get; set; } = string.Empty;
        public string APIKey { get; set; } = string.Empty;
        public string ChecksumKey { get; set; } = string.Empty;
        
    }
    public class PayosConfig : IPayosConfig
    {
        public string ClientID { get; set; } = string.Empty;
        public string APIKey { get; set; } = string.Empty;
        public string ChecksumKey { get; set; } = string.Empty;
        public PayosConfig(IConfiguration configuration)
        {

            var filePaths = configuration.GetSection("Infrastructure:PayOS").Get<PayosConfigModel>();
            ClientID = filePaths?.ClientID ?? string.Empty;
            APIKey = filePaths?.APIKey ?? string.Empty;
            ChecksumKey = filePaths?.ChecksumKey ?? string.Empty;
        }

    }
}
