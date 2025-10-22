using Microsoft.Extensions.Configuration;
using SystemConfigurationService.Common.AppConfigurations.Bcrypt.interfaces;

namespace SystemConfigurationService.Common.AppConfigurations.Bcrypt
{
    public class BcryptConfigModel
    {
        public int SALT_ROUNDS { get; set; }
    }
    public class BcryptConfig : IBcryptConfig
    {
        public int SALT_ROUNDS { get; set; }


        public BcryptConfig(IConfiguration configuration)
        {
            var bcryptConfig = configuration.GetSection("Bcrypt").Get<BcryptConfigModel>();
            SALT_ROUNDS = bcryptConfig.SALT_ROUNDS;
        }

        
    }
}
