namespace ModerationService.Infrastructure.Models.Audio.AcoustID
{
    public class AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparison
    {
        public AcoustIDAudioFingerprintComparisonObject Target { get; set; } = new AcoustIDAudioFingerprintComparisonObject();
        public List<AcoustIDAudioFingerprintComparisonObject> Candidates { get; set; } = new List<AcoustIDAudioFingerprintComparisonObject>();
    }


    public class AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparisonPercentageResult
    {
        public List<AcoustIDAudioFingerprintSimilarityPercentageResult> results { get; set; } = new List<AcoustIDAudioFingerprintSimilarityPercentageResult>();
    }

    public class AcoustIDAudioFingerprintComparisonObject
    {
        public object Id { get; set; }
        public string AudioFingerPrint { get; set; } = string.Empty;
    }

    public class AcoustIDAudioFingerprintSimilarityPercentageResult
    {
        public object Id { get; set; }
        public float SimilarityPercentage { get; set; } // điểm tương đồng
    }


}