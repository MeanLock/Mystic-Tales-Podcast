using PodcastService.Infrastructure.Configurations.Audio.AcoustID.interfaces;
using PodcastService.Infrastructure.Configurations.Audio.Hls.interfaces;

namespace PodcastService.Infrastructure.Configurations.Audio.AcoustID
{
    public class AcoustIDFingerprintComparisonConfig : IAcoustIDFingerprintComparisonConfig
    {
        // ========== Bit Tolerance ==========
        public int SubsequenceBitTolerance { get; set; } = 12; // 5 - 6 - 7 - 8 - 10 - 12
        public int WindowBitTolerance { get; set; } = 8; // 3 - 4 - 5 - 6 - 8
        public int WindowSecondaryBitDiffThreshold { get; set; } = 12; // 6 - 7 - 8 - 10 - 12

        // ========== Thresholds ==========
        public float SubsequenceMatchThreshold { get; set; } = 95f;
        public float HighQualityMatchThreshold { get; set; } = 85f;
        public float EnhancedValidationMinThreshold { get; set; } = 50f;
        public float EnhancedValidationMaxThreshold { get; set; } = 95f;
        public float MaxLengthRatioThreshold { get; set; } = 1.4f;

        // ========== Window Configuration ==========
        public float[] WindowSizeRatios { get; set; } = { 1.0f, 0.8f, 0.6f, 0.4f, 0.2f };
        public int MinimumWindowSize { get; set; } = 4;

        // ========== Step Size Optimization ==========
        public int LongerStepDivisor { get; set; } = 20; // 50 - 25
        public int ShorterStepDivisor { get; set; } = 10; // 25 - 10

        // ========== Weight Coefficients ==========
        // Subsequence Match Enhanced Validation
        public float SubsequenceBaseWeight { get; set; } = 0.6f;
        public float SubsequenceHammingWeight { get; set; } = 0.25f;
        public float SubsequenceCrossCorrelationWeight { get; set; } = 0.15f;

        // Sliding Window Enhanced Validation
        public float SlidingWindowBaseWeight { get; set; } = 0.65f;
        public float SlidingWindowHammingWeight { get; set; } = 0.20f;
        public float SlidingWindowCrossCorrelationWeight { get; set; } = 0.15f;

        // Window Similarity Scoring
        public float TolerantMatchScore { get; set; } = 0.95f;
        public float SecondaryMatchBaseScore { get; set; } = 0.8f;
        public float SecondaryMatchDecayRate { get; set; } = 0.04f; // 0.08f - 0.06f - 0.04f

        // ========== Bonus Multipliers ==========
        public float CoverageBonusMultiplier { get; set; } = 10f;

        // ========== Bit Masking ==========
        public uint HammingDistanceMask { get; set; } = 0xFFFFFFF0; // Ignore 4 LSBs
        public int MaskedBitsPerUint { get; set; } = 28;            // 32 - 4 ignored bits

        // ========== Minimum Lengths ==========
        public int MinimumCorrelationLength { get; set; } = 8;
        public int CenterWeightThresholdLength { get; set; } = 16;

        // ========== Search Range ==========
        public int MaxCrossCorrelationSearchRange { get; set; } = 50;

        // ========== Cross-Correlation Weight Calculation ==========
        public float DefaultCorrelationWeight { get; set; } = 1.0f;
        public float CenterDistanceWeightFactor { get; set; } = 0.3f;

        // ========== Bit Calculations ==========
        public int BitsPerUint { get; set; } = 32;
    }
}