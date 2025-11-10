using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SubscriptionService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace SubscriptionService.Common.AppConfigurations.BusinessSetting
{
    public class BackgroundJobsConfigModel
    {
        public BackgroundJob SampleBackgroundJob { get; set; }
    }
    public class BackgroundJobsConfig : IBackgroundJobsConfig
    {
        public BackgroundJob SampleBackgroundJob { get; set; }

        public BackgroundJobsConfig(IConfiguration configuration)
        {
            var backgroundJobsConfig = configuration.GetSection("BusinessSettings:BackgroundJobs").Get<BackgroundJobsConfigModel>();
            SampleBackgroundJob = backgroundJobsConfig?.SampleBackgroundJob;
        }
    }
}
