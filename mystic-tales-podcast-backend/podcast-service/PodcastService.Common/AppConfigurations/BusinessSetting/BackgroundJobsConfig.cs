using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace PodcastService.Common.AppConfigurations.BusinessSetting
{
    public class BackgroundJobsConfigModel
    {
        public BackgroundJob PodcasterQueryMetricUpdateJob { get; set; }
    }
    public class BackgroundJobsConfig : IBackgroundJobsConfig
    {
        public BackgroundJob PodcasterQueryMetricUpdateJob { get; set; }


        public BackgroundJobsConfig(IConfiguration configuration)
        {
            var backgroundJobsConfig = configuration.GetSection("BusinessSettings:BackgroundJobs").Get<BackgroundJobsConfigModel>();
            PodcasterQueryMetricUpdateJob = backgroundJobsConfig?.PodcasterQueryMetricUpdateJob;
        }
    }
}
