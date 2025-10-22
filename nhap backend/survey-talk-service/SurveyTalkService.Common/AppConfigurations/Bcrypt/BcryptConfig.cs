using Microsoft.Extensions.Configuration;
using SurveyTalkService.Common.AppConfigurations.Bcrypt.interfaces;

namespace SurveyTalkService.Common.AppConfigurations.Bcrypt
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
