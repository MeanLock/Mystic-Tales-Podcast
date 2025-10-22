using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PodcastService.Common.AppConfigurations.BusinessSetting.interfaces;

namespace PodcastService.Common.AppConfigurations.BusinessSetting
{
    public class PodcastPublishReviewSessionConfigModel
    {
        public float MinDuplicateSimilarityRate { get; set; }
        public int TranscriptionMinRestrictedTermCount { get; set; }
    }
    public class PodcastPublishReviewSessionConfig : IPodcastPublishReviewSessionConfig
    {
        public float MinDuplicateSimilarityRate { get; set; }
        public int TranscriptionMinRestrictedTermCount { get; set; }


        public PodcastPublishReviewSessionConfig(IConfiguration configuration)
        {
            var podcastPublishReviewSessionConfig = configuration.GetSection("BusinessSettings:PodcastPublishReviewSession").Get<PodcastPublishReviewSessionConfigModel>();
            MinDuplicateSimilarityRate = podcastPublishReviewSessionConfig.MinDuplicateSimilarityRate;
            TranscriptionMinRestrictedTermCount = podcastPublishReviewSessionConfig.TranscriptionMinRestrictedTermCount;
        }
    }
}
