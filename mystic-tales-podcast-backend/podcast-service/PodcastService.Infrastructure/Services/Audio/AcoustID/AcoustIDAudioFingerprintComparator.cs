using Microsoft.Extensions.Logging;
using NAudio.Wave;
using PodcastService.Infrastructure.Configurations.Audio.AcoustID.interfaces;
using PodcastService.Infrastructure.Models.Audio.AcoustID;
using System.Diagnostics;

namespace PodcastService.Infrastructure.Services.Audio.AcoustID
{
    public class AcoustIDAudioFingerprintComparator : IDisposable
    {
        private readonly ILogger<AcoustIDAudioFingerprintComparator> _logger;
        private readonly IAcoustIDFingerprintComparisonConfig _acoustIDFingerprintComparisonConfig;

        public AcoustIDAudioFingerprintComparator(ILogger<AcoustIDAudioFingerprintComparator> logger, IAcoustIDFingerprintComparisonConfig acoustIDFingerprintComparisonConfig)
        {
            _logger = logger;
            _acoustIDFingerprintComparisonConfig = acoustIDFingerprintComparisonConfig;
        }

        /// <summary>
        /// So sánh một target fingerprint với nhiều candidate fingerprints
        /// </summary>
        /// <param name="comparison">Đối tượng chứa target và danh sách candidates</param>
        /// <returns>Kết quả so sánh với tỷ lệ phần trăm tương đồng</returns>
        public AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparisonPercentageResult CompareTargetToCandidates(
            AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparison comparison)
        {
            var result = new AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparisonPercentageResult();

            try
            {
                if (string.IsNullOrEmpty(comparison?.Target?.AudioFingerPrint))
                {
                    _logger.LogWarning("Target fingerprint is null or empty");
                    return result;
                }

                if (comparison.Candidates == null || !comparison.Candidates.Any())
                {
                    _logger.LogWarning("No candidates provided for comparison");
                    return result;
                }

                var targetFingerprint = comparison.Target.AudioFingerPrint;

                foreach (var candidate in comparison.Candidates)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(candidate?.AudioFingerPrint))
                        {
                            _logger.LogWarning($"Candidate {candidate?.Id} has null or empty fingerprint");
                            continue;
                        }

                        // Tính toán tỷ lệ tương đồng với Chromaprint Base64
                        Console.WriteLine($"\n\nComparing Target {comparison.Target.Id} with Candidate {candidate.Id}");
                        var similarityPercentage = CalculateFingerprintSimilarity(targetFingerprint, candidate.AudioFingerPrint);

                        result.results.Add(new AcoustIDAudioFingerprintSimilarityPercentageResult
                        {
                            Id = candidate.Id,
                            SimilarityPercentage = similarityPercentage
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error comparing candidate {candidate?.Id}");
                        result.results.Add(new AcoustIDAudioFingerprintSimilarityPercentageResult
                        {
                            Id = candidate?.Id ?? "unknown",
                            SimilarityPercentage = 0f
                        });
                    }
                }

                // Sắp xếp kết quả theo tỷ lệ tương đồng giảm dần
                result.results = result.results.OrderByDescending(r => r.SimilarityPercentage).ToList();

                _logger.LogInformation($"Completed comparison for target {comparison.Target.Id} with {result.results.Count} candidates");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during fingerprint comparison");
            }

            return result;
        }

        /// <summary>
        /// Tính toán tỷ lệ tương đồng giữa hai audio fingerprints Base64
        /// </summary>
        /// <param name="fingerprint1">Fingerprint đầu tiên (Base64 string)</param>
        /// <param name="fingerprint2">Fingerprint thứ hai (Base64 string)</param>
        /// <returns>Tỷ lệ tương đồng từ 0.0 đến 100.0</returns>
        private float CalculateFingerprintSimilarity(string fingerprint1, string fingerprint2)
        {
            try
            {
                if (string.IsNullOrEmpty(fingerprint1) || string.IsNullOrEmpty(fingerprint2))
                    return 0f;

                if (fingerprint1.Equals(fingerprint2, StringComparison.Ordinal))
                    return 100f;

                // Parse Base64 thành uint32 arrays
                var chromaData1 = ParseBase64ToUint32Array(fingerprint1);
                var chromaData2 = ParseBase64ToUint32Array(fingerprint2);
                Console.WriteLine($"Parsed chromaData1 length: {chromaData1?.Length}, chromaData2 length: {chromaData2?.Length}");

                if (chromaData1 != null && chromaData2 != null)
                {
                    return CalculateChromaprintSimilarity(chromaData1, chromaData2);
                }

                // Fallback to byte comparison
                var bytes1 = Convert.FromBase64String(fingerprint1);
                var bytes2 = Convert.FromBase64String(fingerprint2);
                return CalculateByteSimilarity(bytes1, bytes2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating fingerprint similarity");
                return 0f;
            }
        }

        /// <summary>
        /// Parse Base64 string thành uint32 array cho Chromaprint
        /// </summary>
        private uint[]? ParseBase64ToUint32Array(string base64String)
        {
            try
            {
                var bytes = Convert.FromBase64String(base64String);

                // Convert bytes thành uint32 array
                var uint32Count = bytes.Length / 4;
                if (uint32Count == 0) return null;

                var result = new uint[uint32Count];
                for (int i = 0; i < uint32Count; i++)
                {
                    int offset = i * 4;
                    if (offset + 3 < bytes.Length)
                    {
                        result[i] = BitConverter.ToUInt32(bytes, offset);
                    }
                }

                _logger.LogDebug($"Parsed {uint32Count} uint32 values from Base64");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse Base64 to uint32 array");
                return null;
            }
        }

        /// <summary>
        /// Tính similarity cho Chromaprint fingerprints với advanced matching
        /// </summary>
        private float CalculateChromaprintSimilarity(uint[] fingerprint1, uint[] fingerprint2)
        {
            // OPTIMIZATION 1: Target dài hơn 30% → pass ngay
            // var longer = fingerprint1.Length > fingerprint2.Length ? fingerprint1 : fingerprint2;
            // var shorter = fingerprint1.Length <= fingerprint2.Length ? fingerprint1 : fingerprint2;
            // float lengthRatio = (float)longer.Length / shorter.Length;
            // Console.WriteLine($"Fingerprint lengths: longer={longer.Length}, shorter={shorter.Length}, ratio={lengthRatio:F2}");
            // if (lengthRatio > 1.3f) // Target dài hơn 30%
            // {
            //     _logger.LogDebug($"OPTIMIZATION 1: Target too long ({longer.Length} vs {shorter.Length}, ratio={lengthRatio:F2}), returning 0%");
            //     return 0f; // Pass ngay
            // }

            var targetFingerprint = fingerprint1;
            var candidateFingerprint = fingerprint2;

            var longer = candidateFingerprint.Length > targetFingerprint.Length ? candidateFingerprint : targetFingerprint;
            var shorter = candidateFingerprint.Length <= targetFingerprint.Length ? candidateFingerprint : targetFingerprint;

            float lengthRatio = (float)longer.Length / shorter.Length;
            Console.WriteLine($"Fingerprint lengths: target={targetFingerprint.Length}, candidate={candidateFingerprint.Length}, ratio={lengthRatio:F2}");

            // OPTIMIZATION 1: Chỉ skip khi candidate ngắn hơn target quá nhiều (không hợp lý)
            if (candidateFingerprint.Length < targetFingerprint.Length && lengthRatio > _acoustIDFingerprintComparisonConfig.MaxLengthRatioThreshold)
            {
                _logger.LogDebug($"OPTIMIZATION 1: Candidate too short ({candidateFingerprint.Length} vs {targetFingerprint.Length}, ratio={lengthRatio:F2}), returning 0%");
                return 0f;
            }

            var similarities = new List<float>();

            // 1. PRIORITY: Exact subsequence matching (cho audio clips)
            var subsequenceMatch = CalculateChromaprintSubsequenceMatch(fingerprint1, fingerprint2);
            Console.WriteLine($"\n\n ==> Final SubsequenceMatch: {subsequenceMatch:F2}%");
            if (subsequenceMatch >= _acoustIDFingerprintComparisonConfig.SubsequenceMatchThreshold)
            {
                _logger.LogDebug($"Found exact Chromaprint subsequence match: {subsequenceMatch:F2}%");
                return Math.Min(100f, subsequenceMatch);
            }
            similarities.Add(subsequenceMatch);

            var stopwatch = Stopwatch.StartNew();
            stopwatch.Restart();
            // 2. Sliding window với bit tolerance
            var slidingWindow = CalculateChromaprintSlidingWindow(fingerprint1, fingerprint2);
            Console.WriteLine($"\n\n ==> Final Sliding windows: {slidingWindow:F2}%");
            similarities.Add(slidingWindow);
            // Console.WriteLine($"\n\nSlidingWindow completed [{stopwatch.ElapsedMilliseconds}ms]");
            stopwatch.Restart();

            // // 3. Hamming distance với bit masking
            // var hammingDistance = CalculateChromaprintHamming(fingerprint1, fingerprint2);
            // Console.WriteLine($"\n\n ==> Final Hamming distance: {hammingDistance:F2}%");
            // similarities.Add(hammingDistance);
            // // Console.WriteLine($"\n\nHamming completed [{stopwatch.ElapsedMilliseconds}ms]");

            // stopwatch.Restart();
            // // 4. Cross-correlation analysis 
            // var crossCorrelation = CalculateChromaprintCrossCorrelation(fingerprint1, fingerprint2);
            // Console.WriteLine($"\n\n ==> Final CrossCorrelation: {crossCorrelation:F2}%");
            // similarities.Add(crossCorrelation);
            // Console.WriteLine($"\n\nCrossCorrelation completed [{stopwatch.ElapsedMilliseconds}ms]");

            var maxSimilarity = similarities.Max();

            _logger.LogDebug($"Chromaprint similarities: Subseq={similarities[0]:F2}%, Sliding={similarities[1]:F2}%, Max={maxSimilarity:F2}%");

            return maxSimilarity;
        }

        /// <summary>
        /// Tìm exact subsequence match trong Chromaprint fingerprints
        /// Giải thích: kiểm tra từng vị trí trong mảng longer để tìm dãy item được sắp xếp (tính từ item đang loop) giống hệt hoặc gần giống với mảng shorter
        /// Shorter luôn lấy 100% window size, Longer lấy loop toàn bộ item để duyệt với 100% window size của Shorter
        /// Lưu ý dễ bị hiểu lầm : so sánh này là exactly subsequence matching nghĩa là đoạn shorter phải xuất hiện toàn vẹn trong 1 trong số các đoạn của longer với độ lệch BIT_TOLERANCE cho phép
        /// </summary>
        private float CalculateChromaprintSubsequenceMatch(uint[] fingerprint1, uint[] fingerprint2)
        {
            var shorter = fingerprint1.Length <= fingerprint2.Length ? fingerprint1 : fingerprint2;
            var longer = fingerprint1.Length > fingerprint2.Length ? fingerprint1 : fingerprint2;

            if (shorter.Length == 0) return 0f;

            int BIT_TOLERANCE = _acoustIDFingerprintComparisonConfig.SubsequenceBitTolerance; // Allow n-bit differences per uint32, BIT_TOLERANCE càng thấp càng chặt (0=exact match, 1=very strict, 3=moderate, 5=loose)
            float bestMatchScore = 0f;
            int bestMatchPosition = -1;

            for (int i = 0; i <= longer.Length - shorter.Length; i++)
            {
                int perfectMatches = 0;
                int tolerantMatches = 0;

                for (int j = 0; j < shorter.Length; j++)
                {
                    uint val1 = shorter[j];
                    uint val2 = longer[i + j];

                    if (val1 == val2)
                    {
                        perfectMatches++;
                        tolerantMatches++;
                    }
                    else
                    {
                        uint xor = val1 ^ val2;
                        int bitDiff = CountSetBits(xor);

                        if (bitDiff <= BIT_TOLERANCE)
                        {
                            tolerantMatches++;
                        }
                    }

                    // OPTIMIZATION 2: Early exit khi tỉ lệ giống nhau > 80%
                    // if (j >= shorter.Length / 4) // Đã check ít nhất 25%
                    // {
                    //     float matchRatio = (float)tolerantMatches / (j + 1);
                    //     if (matchRatio > 0.8f) // Tỉ lệ giống > 80%
                    //     {
                    //         _logger.LogDebug($"OPTIMIZATION 2: High similarity detected at position {i}, segment {j}/{shorter.Length}, matchRatio={matchRatio:F2}");
                    //         // Extrapolate score và return ngay 
                    //         float extrapolatedScore = matchRatio * 100f;
                    //         return Math.Min(100f, extrapolatedScore * 1.1f); // Bonus 10% cho early detection
                    //     }
                    // }
                }

                float matchScore = ((float)tolerantMatches / shorter.Length) * 100f;

                // Enhanced: Apply integrated validation for promising matches
                if (matchScore >= _acoustIDFingerprintComparisonConfig.EnhancedValidationMinThreshold && matchScore < _acoustIDFingerprintComparisonConfig.EnhancedValidationMaxThreshold)
                {
                    // Extract matching segments for validation
                    var shorterSegment = shorter;
                    var longerSegment = longer.Skip(i).Take(shorter.Length).ToArray();

                    // Apply Hamming validation for bit-level refinement
                    float hammingScore = CalculateChromaprintHamming(shorterSegment, longerSegment);

                    // Apply CrossCorrelation for statistical confidence
                    float crossCorrScore = CalculateChromaprintCrossCorrelation(shorterSegment, longerSegment);

                    // Weighted combination: base (60%), hamming (25%), crosscorr (15%)
                    float enhancedScore = (matchScore * _acoustIDFingerprintComparisonConfig.SubsequenceBaseWeight) + (hammingScore * _acoustIDFingerprintComparisonConfig.SubsequenceHammingWeight) + (crossCorrScore * _acoustIDFingerprintComparisonConfig.SubsequenceCrossCorrelationWeight);

                    _logger.LogDebug($"Enhanced validation at pos {i}: Base={matchScore:F2}%, Hamming={hammingScore:F2}%, CrossCorr={crossCorrScore:F2}%, Enhanced={enhancedScore:F2}%");

                    matchScore = enhancedScore;
                }

                if (matchScore > bestMatchScore)
                {
                    bestMatchScore = matchScore;
                    bestMatchPosition = i;
                }

                // Early exit cho perfect match
                if (perfectMatches == shorter.Length)
                {
                    _logger.LogDebug($"Found PERFECT Chromaprint subsequence match at position {i}");
                    return 100f;
                }
            }

            // Nếu tìm thấy match > 85% với tolerance
            if (bestMatchScore >= _acoustIDFingerprintComparisonConfig.HighQualityMatchThreshold)
            {
                _logger.LogDebug($"Found high-quality Chromaprint subsequence match: {bestMatchScore:F2}% at position {bestMatchPosition}");

                // Bonus cho high-coverage matches
                float coverage = ((float)shorter.Length / longer.Length);
                float bonus = coverage * _acoustIDFingerprintComparisonConfig.CoverageBonusMultiplier; // Up to 10% bonus

                return Math.Min(100f, bestMatchScore + bonus);
            }

            return bestMatchScore;
        }

        /// <summary>
        /// Sliding window với adaptive window size cho Chromaprint
        /// Giải thích: tạo nhiều window size khác nhau (100%, 80%, 60%, 40%, 20% của shorter)
        /// Lặp qua từng kích thước window size, Shoter và Longer đều sẽ loop qua từng item tuân và cùng tuân sao window size đang loop để so sánh
        /// Tính similarity cho từng window pair và lấy max similarity
        /// </summary>
        private float CalculateChromaprintSlidingWindow(uint[] fingerprint1, uint[] fingerprint2)
        {
            var shorter = fingerprint1.Length <= fingerprint2.Length ? fingerprint1 : fingerprint2;
            var longer = fingerprint1.Length > fingerprint2.Length ? fingerprint1 : fingerprint2;

            if (shorter.Length < _acoustIDFingerprintComparisonConfig.MinimumWindowSize) return CalculateSimpleHamming(fingerprint1, fingerprint2);

            float maxSimilarity = 0f;

            // Multiple window sizes: 100%, 75%, 50%, 25%
            // var windowSizes = new[] {
            //     shorter.Length,
            //     shorter.Length * 3 / 4,
            //     shorter.Length / 2,
            //     shorter.Length / 4
            // }.Where(s => s >= 4).ToArray();

            // Multiple window sizes: 100%, 80%, 60%, 40%, 20%
            var windowSizes = _acoustIDFingerprintComparisonConfig.WindowSizeRatios
                .Select(ratio => (int)(shorter.Length * ratio))
                .Where(s => s >= _acoustIDFingerprintComparisonConfig.MinimumWindowSize)
                .ToArray();


            // Multiple window sizes: 100%, 95%, 90%, 85%, 80%
            // var windowSizes = new[] {
            //     shorter.Length,             // 100%
            //     shorter.Length * 19/20,     // 95%
            //     shorter.Length * 9/10,      // 90%
            //     shorter.Length * 17/20,     // 85%
            //     shorter.Length * 4/5        // 80%
            // }.Where(s => s >= 4).ToArray();

            // for (int i = 0; i < windowSizes.Length; i++)
            // {
            //     Console.WriteLine($"Window size option {i}: {windowSizes[i]} (ratio={(float)windowSizes[i] / shorter.Length:F2})");
            // }

            foreach (int windowSize in windowSizes)
            {
                // Console.WriteLine($"\n--- Sliding Window Size: {(float)windowSize / shorter.Length:F2} ---");

                // STEP OPTIMIZATION: Tính step size dựa trên độ dài
                int maxPositionsLonger = longer.Length - windowSize + 1;
                int maxPositionsShorter = shorter.Length - windowSize + 1;

                // Giảm số positions cần check
                // các mức độ bước nhảy và độ chính xác :
                // /10, /5 → 60% accuracy
                // /20, /10 → 70% accuracy
                // /50, /25 → 80% accuracy
                // /100, /50 → 85% accuracy
                // /200, /100 → 90% accuracy

                int stepLonger = Math.Max(1, maxPositionsLonger / _acoustIDFingerprintComparisonConfig.LongerStepDivisor);   // Max loop positions trên longer
                int stepShorter = Math.Max(1, maxPositionsShorter / _acoustIDFingerprintComparisonConfig.ShorterStepDivisor); // Max loop positions trên shorter

                // Console.WriteLine($"Step sizes: longer={stepLonger} (checking {maxPositionsLonger / stepLonger} positions), shorter={stepShorter} (checking {maxPositionsShorter / stepShorter} positions)");

                for (int i = 0; i < maxPositionsLonger; i += stepLonger)
                {
                    for (int j = 0; j < maxPositionsShorter; j += stepShorter)
                    {
                        float similarity = CalculateWindowSimilarity(
                            longer.Skip(i).Take(windowSize).ToArray(),
                            shorter.Skip(j).Take(windowSize).ToArray()
                        );

                        float coverage = (float)windowSize / shorter.Length;
                        float adjustedSimilarity = similarity * coverage;

                        // Enhanced: Apply integrated validation for promising window matches
                        // if (adjustedSimilarity >= 50f && adjustedSimilarity < 90f && windowSize >= 8)
                        // {
                        var longerWindow = longer.Skip(i).Take(windowSize).ToArray();
                        var shorterWindow = shorter.Skip(j).Take(windowSize).ToArray();

                        // Apply Hamming validation for bit-level refinement
                        float hammingScore = CalculateChromaprintHamming(longerWindow, shorterWindow);

                        // Apply CrossCorrelation for statistical confidence  
                        float crossCorrScore = CalculateChromaprintCrossCorrelation(longerWindow, shorterWindow);

                        // Weighted combination: base (65%), hamming (20%), crosscorr (15%)
                        float enhancedSimilarity = (adjustedSimilarity * _acoustIDFingerprintComparisonConfig.SlidingWindowBaseWeight) + (hammingScore * coverage * _acoustIDFingerprintComparisonConfig.SlidingWindowHammingWeight) + (crossCorrScore * coverage * _acoustIDFingerprintComparisonConfig.SlidingWindowCrossCorrelationWeight);

                        // Console.WriteLine($"Enhanced sliding validation at [{i},{j}] win={windowSize}: Base={adjustedSimilarity:F2}%, Hamming={hammingScore:F2}%, CrossCorr={crossCorrScore:F2}%, Enhanced={enhancedSimilarity:F2}%");

                        adjustedSimilarity = enhancedSimilarity;
                        // }

                        maxSimilarity = Math.Max(maxSimilarity, adjustedSimilarity);

                        // if (similarity > 85f)
                        // {
                        //     float bonus = coverage > 0.7f ? 1.15f : 1.1f;
                        //     return Math.Min(100f, adjustedSimilarity * bonus);
                        // }
                    }
                }
            }


            return maxSimilarity;
        }

        /// <summary>
        /// Tính similarity cho một window với bit tolerance
        /// </summary>
        private float CalculateWindowSimilarity(uint[] window1, uint[] window2)
        {
            if (window1.Length != window2.Length) return 0f;

            int BIT_TOLERANCE = _acoustIDFingerprintComparisonConfig.WindowBitTolerance;
            float totalScore = 0f;

            for (int i = 0; i < window1.Length; i++)
            {
                if (window1[i] == window2[i])
                {
                    totalScore += 1.0f;
                }
                else
                {
                    uint xor = window1[i] ^ window2[i];
                    int bitDiff = CountSetBits(xor);

                    if (bitDiff <= BIT_TOLERANCE)
                    {
                        totalScore += _acoustIDFingerprintComparisonConfig.TolerantMatchScore;
                    }
                    else if (bitDiff <= _acoustIDFingerprintComparisonConfig.WindowSecondaryBitDiffThreshold)
                    {
                        totalScore += Math.Max(0f, _acoustIDFingerprintComparisonConfig.SecondaryMatchBaseScore - (bitDiff * _acoustIDFingerprintComparisonConfig.SecondaryMatchDecayRate));
                    }
                }
            }

            return (totalScore / window1.Length) * 100f;
        }

        /// <summary>
        /// Hamming distance với bit masking cho Chromaprint
        /// Giải thích: áp dụng mask để bỏ qua các bit ít quan trọng (LSB) trước khi tính Hamming distance
        /// </summary>
        private float CalculateChromaprintHamming(uint[] fingerprint1, uint[] fingerprint2)
        {
            if (fingerprint1.Length != fingerprint2.Length)
                return CalculateCrossArrayHamming(fingerprint1, fingerprint2);

            // Ignore least significant bits (noise-prone)
            uint MASK = _acoustIDFingerprintComparisonConfig.HammingDistanceMask; // Ignore 4 LSBs (Lọc noise)

            int totalBits = fingerprint1.Length * _acoustIDFingerprintComparisonConfig.MaskedBitsPerUint; // 28 bits per uint (after masking)
            int matchingBits = 0;

            for (int i = 0; i < fingerprint1.Length; i++)
            {
                uint masked1 = fingerprint1[i] & MASK;
                uint masked2 = fingerprint2[i] & MASK;
                uint xorResult = masked1 ^ masked2;

                matchingBits += _acoustIDFingerprintComparisonConfig.MaskedBitsPerUint - CountSetBits(xorResult);
            }
            return ((float)matchingBits / totalBits) * 100f;
        }

        /// <summary>
        /// Cross-correlation analysis cho Chromaprint fingerprints
        /// </summary>
        private float CalculateChromaprintCrossCorrelation(uint[] fingerprint1, uint[] fingerprint2)
        {
            var shorter = fingerprint1.Length <= fingerprint2.Length ? fingerprint1 : fingerprint2;
            var longer = fingerprint1.Length > fingerprint2.Length ? fingerprint1 : fingerprint2;

            if (shorter.Length < _acoustIDFingerprintComparisonConfig.MinimumCorrelationLength) return 0f;

            float maxCorrelation = 0f;
            int searchRange = Math.Min(longer.Length - shorter.Length + 1,  _acoustIDFingerprintComparisonConfig.MaxCrossCorrelationSearchRange);

            for (int offset = 0; offset < searchRange; offset++)
            {
                float correlation = 0f;
                float weightSum = 0f;

                for (int i = 0; i < shorter.Length; i++)
                {
                    // Weighted correlation - center samples have higher weight
                    float weight = _acoustIDFingerprintComparisonConfig.DefaultCorrelationWeight;
                    if (shorter.Length > _acoustIDFingerprintComparisonConfig.CenterWeightThresholdLength)
                    {
                        float centerDistance = Math.Abs(i - shorter.Length / 2f) / (shorter.Length / 2f);
                        weight = _acoustIDFingerprintComparisonConfig.DefaultCorrelationWeight - _acoustIDFingerprintComparisonConfig.CenterDistanceWeightFactor * centerDistance;
                    }

                    uint val1 = shorter[i];
                    uint val2 = longer[offset + i];

                    if (val1 == val2)
                    {
                        correlation += weight;
                    }
                    else
                    {
                        uint xor = val1 ^ val2;
                        int bitDiff = CountSetBits(xor);
                        float bitSimilarity = Math.Max(0f, 1f - (bitDiff / (float)_acoustIDFingerprintComparisonConfig.BitsPerUint));
                        correlation += weight * bitSimilarity;
                    }

                    weightSum += weight;
                }

                float normalizedCorrelation = (correlation / weightSum) * 100f;
                maxCorrelation = Math.Max(maxCorrelation, normalizedCorrelation);
            }

            return maxCorrelation;
        }

        /// <summary>
        /// Cross-array Hamming cho arrays khác độ dài
        /// </summary>
        private float CalculateCrossArrayHamming(uint[] fingerprint1, uint[] fingerprint2)
        {
            var shorter = fingerprint1.Length <= fingerprint2.Length ? fingerprint1 : fingerprint2;
            var longer = fingerprint1.Length > fingerprint2.Length ? fingerprint1 : fingerprint2;

            float maxSimilarity = 0f;

            for (int offset = 0; offset <= longer.Length - shorter.Length; offset++)
            {
                int matchingBits = 0;
                int totalBits = shorter.Length * _acoustIDFingerprintComparisonConfig.BitsPerUint;

                for (int i = 0; i < shorter.Length; i++)
                {
                    uint xorResult = shorter[i] ^ longer[offset + i];
                    matchingBits += _acoustIDFingerprintComparisonConfig.BitsPerUint - CountSetBits(xorResult);
                }

                float similarity = ((float)matchingBits / totalBits) * 100f;
                maxSimilarity = Math.Max(maxSimilarity, similarity);
            }

            return maxSimilarity;
        }

        /// <summary>
        /// Simple Hamming cho trường hợp fallback
        /// </summary>
        private float CalculateSimpleHamming(uint[] fingerprint1, uint[] fingerprint2)
        {
            if (fingerprint1.Length == 0 && fingerprint2.Length == 0)
                return 100f;

            if (fingerprint1.Length == 0 || fingerprint2.Length == 0)
                return 0f;

            int minLength = Math.Min(fingerprint1.Length, fingerprint2.Length);
            int matchingBits = 0;
            int totalBits = minLength * _acoustIDFingerprintComparisonConfig.BitsPerUint;

            for (int i = 0; i < minLength; i++)
            {
                uint xorResult = fingerprint1[i] ^ fingerprint2[i];
                matchingBits += _acoustIDFingerprintComparisonConfig.BitsPerUint - CountSetBits(xorResult);
            }

            return ((float)matchingBits / totalBits) * 100f;
        }

        /// <summary>
        /// Fallback byte similarity cho khi uint32 parsing fails
        /// </summary>
        private float CalculateByteSimilarity(byte[] bytes1, byte[] bytes2)
        {
            if (bytes1.Length == 0 || bytes2.Length == 0) return 0f;
            if (bytes1.SequenceEqual(bytes2)) return 100f;

            // Simple byte-level comparison
            var shorter = bytes1.Length <= bytes2.Length ? bytes1 : bytes2;
            var longer = bytes1.Length > bytes2.Length ? bytes1 : bytes2;

            // Check for exact substring match
            for (int i = 0; i <= longer.Length - shorter.Length; i++)
            {
                bool isMatch = true;
                for (int j = 0; j < shorter.Length; j++)
                {
                    if (longer[i + j] != shorter[j])
                    {
                        isMatch = false;
                        break;
                    }
                }
                if (isMatch) return 100f;
            }

            // Hamming distance for same length
            if (bytes1.Length == bytes2.Length)
            {
                int matches = 0;
                for (int i = 0; i < bytes1.Length; i++)
                {
                    if (bytes1[i] == bytes2[i]) matches++;
                }
                return ((float)matches / bytes1.Length) * 100f;
            }

            // Jaccard similarity for different lengths
            var set1 = new HashSet<byte>(bytes1);
            var set2 = new HashSet<byte>(bytes2);
            var intersection = set1.Intersect(set2).Count();
            var union = set1.Union(set2).Count();

            return union > 0 ? ((float)intersection / union) * 100f : 0f;
        }

        /// <summary>
        /// Đếm số bit được set trong uint32
        /// </summary>
        private int CountSetBits(uint value)
        {
            int count = 0;
            while (value != 0)
            {
                value &= value - 1;
                count++;
            }
            return count;
        }




        public void Dispose()
        {
            // Cleanup if needed
        }
    }


}