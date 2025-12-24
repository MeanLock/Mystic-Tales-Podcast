using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ModerationService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace ModerationService.Common.AppConfigurations.BusinessSetting
{
    public class BackgroundJobsConfigModel
    {
        public BackgroundJob SampleBackgroundJob { get; set; }
        public BackgroundJob DMCANoticeResponseTimeoutJob { get; set; }
        public BackgroundJob CounterNoticeResponseTimeoutJob { get; set; }
    }
    public class BackgroundJobsConfig : IBackgroundJobsConfig
    {
        public BackgroundJob SampleBackgroundJob { get; set; }
        public BackgroundJob DMCANoticeResponseTimeoutJob { get; set; }
        public BackgroundJob CounterNoticeResponseTimeoutJob { get; set; }


        public BackgroundJobsConfig(IConfiguration configuration)
        {
            var backgroundJobsConfig = configuration.GetSection("BusinessSettings:BackgroundJobs").Get<BackgroundJobsConfigModel>();
            SampleBackgroundJob = backgroundJobsConfig?.SampleBackgroundJob;
            DMCANoticeResponseTimeoutJob = backgroundJobsConfig?.DMCANoticeResponseTimeoutJob;
            CounterNoticeResponseTimeoutJob = backgroundJobsConfig?.CounterNoticeResponseTimeoutJob;
        }
    }
}
