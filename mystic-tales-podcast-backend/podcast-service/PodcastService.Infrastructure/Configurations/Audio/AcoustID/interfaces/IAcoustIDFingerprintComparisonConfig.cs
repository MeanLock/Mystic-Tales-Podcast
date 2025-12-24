namespace PodcastService.Infrastructure.Configurations.Audio.AcoustID.interfaces
{
    public interface IAcoustIDFingerprintComparisonConfig
    {
        // ========== Bit Tolerance ==========
        int SubsequenceBitTolerance { get; set; }
        int WindowBitTolerance { get; set; }
        int WindowSecondaryBitDiffThreshold { get; set; }
        // ========== Thresholds ==========
        float SubsequenceMatchThreshold { get; set; }
        float HighQualityMatchThreshold { get; set; }
        float EnhancedValidationMinThreshold { get; set; }
        float EnhancedValidationMaxThreshold { get; set; }
        float MaxLengthRatioThreshold { get; set; }
        // ========== Window Configuration ==========
        float[] WindowSizeRatios { get; set; }
        int MinimumWindowSize { get; set; }
        // ========== Step Size Optimization ==========
        int LongerStepDivisor { get; set; }
        int ShorterStepDivisor { get; set; }
        // ========== Weight Coefficients ==========
        // Subsequence Match Enhanced Validation
        float SubsequenceBaseWeight { get; set; }
        float SubsequenceHammingWeight { get; set; }
        float SubsequenceCrossCorrelationWeight { get; set; }
        // Sliding Window Enhanced Validation
        float SlidingWindowBaseWeight { get; set; }
        float SlidingWindowHammingWeight { get; set; }
        float SlidingWindowCrossCorrelationWeight { get; set; }
        // Window Similarity Scoring
        float TolerantMatchScore { get; set; }
        float SecondaryMatchBaseScore { get; set; }
        float SecondaryMatchDecayRate { get; set; }
        // ========== Bonus Multipliers ==========
        float CoverageBonusMultiplier { get; set; }
        // ========== Bit Masking ==========
        uint HammingDistanceMask { get; set; }
        int MaskedBitsPerUint { get; set; }
        // ========== Minimum Lengths ==========
        int MinimumCorrelationLength { get; set; }
        int CenterWeightThresholdLength { get; set; }
        // ========== Search Range ==========
        int MaxCrossCorrelationSearchRange { get; set; }
        // ========== Cross-Correlation Weight Calculation ==========
        float DefaultCorrelationWeight { get; set; }
        float CenterDistanceWeightFactor { get; set; }
        // ========== Bit Calculations ==========
        int BitsPerUint { get; set; }

    }
}
