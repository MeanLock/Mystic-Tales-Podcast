using Newtonsoft.Json.Linq;

namespace PodcastService.Common.AppConfigurations.BusinessSetting.interfaces
{
    public interface IPodcastPublishReviewSessionConfig
    {
        float MinDuplicateSimilarityRate { get; set; }
        int HighTranscriptionMinRestrictedTermCount { get; set; }
        int MediumTranscriptionMinRestrictedTermCount { get; set; }
    }
}
