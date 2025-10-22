using Microsoft.Extensions.Configuration;
using Architecture_1.Infrastructure.Configurations.Payos.interfaces;

namespace Architecture_1.Infrastructure.Configurations.Payos
{
    public class PayosConfigModel
    {
        public string ClientID { get; set; }
        public string APIKey { get; set; }
        public string ChecksumKey { get; set; }
        
    }
    public class PayosConfig : IPayosConfig
    {
        public string ClientID { get; set; }
        public string APIKey { get; set; }
        public string ChecksumKey { get; set; }
        public PayosConfig(IConfiguration configuration)
        {

            var filePaths = configuration.GetSection("Infrastructure:PayOS").Get<PayosConfigModel>();
            ClientID = filePaths?.ClientID;
            APIKey = filePaths?.APIKey;
            ChecksumKey = filePaths?.ChecksumKey;
        }

    }
}
