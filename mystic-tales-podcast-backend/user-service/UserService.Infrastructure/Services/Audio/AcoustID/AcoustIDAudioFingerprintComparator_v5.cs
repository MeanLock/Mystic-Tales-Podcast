// using Microsoft.Extensions.Logging;
// using AcoustID;
// using AcoustID.Chromaprint;
// using NAudio.Wave;
// using UserService.Infrastructure.Models.Audio;
// using NAudio.MediaFoundation;
// using System.Security.Cryptography;
// using UserService.Infrastructure.Models.Audio.AcoustID;
// using System.Numerics;
// using System.Diagnostics;

// namespace UserService.Infrastructure.Services.Audio.AcoustID
// {
//     public class AcoustIDAudioFingerprintComparator : IDisposable
//     {
//         private readonly ILogger<AcoustIDAudioFingerprintComparator> _logger;

//         public AcoustIDAudioFingerprintComparator(ILogger<AcoustIDAudioFingerprintComparator> logger)
//         {
//             _logger = logger;
//         }

//         /// <summary>
//         /// So sánh một target fingerprint với nhiều candidate fingerprints
//         /// </summary>
//         /// <param name="comparison">Đối tượng chứa target và danh sách candidates</param>
//         /// <returns>Kết quả so sánh với tỷ lệ phần trăm tương đồng</returns>
//         public AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparisonPercentageResult CompareTargetToCandidates(
//             AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparison comparison)
//         {
//             var result = new AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparisonPercentageResult();

//             try
//             {
//                 if (string.IsNullOrEmpty(comparison?.Target?.AudioFingerPrint))
//                 {
//                     _logger.LogWarning("Target fingerprint is null or empty");
//                     return result;
//                 }

//                 if (comparison.Candidates == null || !comparison.Candidates.Any())
//                 {
//                     _logger.LogWarning("No candidates provided for comparison");
//                     return result;
//                 }

//                 var targetFingerprint = comparison.Target.AudioFingerPrint;

//                 foreach (var candidate in comparison.Candidates)
//                 {
//                     try
//                     {
//                         if (string.IsNullOrEmpty(candidate?.AudioFingerPrint))
//                         {
//                             _logger.LogWarning($"Candidate {candidate?.Id} has null or empty fingerprint");
//                             continue;
//                         }

//                         // Tính toán tỷ lệ tương đồng với Chromaprint Base64
//                         var similarityPercentage = CalculateFingerprintSimilarity(targetFingerprint, candidate.AudioFingerPrint);

//                         result.results.Add(new AcoustIDAudioFingerprintSimilarityPercentageResult
//                         {
//                             Id = candidate.Id,
//                             SimilarityPercentage = similarityPercentage
//                         });
//                     }
//                     catch (Exception ex)
//                     {
//                         _logger.LogError(ex, $"Error comparing candidate {candidate?.Id}");
//                         result.results.Add(new AcoustIDAudioFingerprintSimilarityPercentageResult
//                         {
//                             Id = candidate?.Id ?? "unknown",
//                             SimilarityPercentage = 0f
//                         });
//                     }
//                 }

//                 // Sắp xếp kết quả theo tỷ lệ tương đồng giảm dần
//                 result.results = result.results.OrderByDescending(r => r.SimilarityPercentage).ToList();

//                 _logger.LogInformation($"Completed comparison for target {comparison.Target.Id} with {result.results.Count} candidates");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error during fingerprint comparison");
//             }

//             return result;
//         }

//         /// <summary>
//         /// Tính toán tỷ lệ tương đồng giữa hai audio fingerprints Base64
//         /// </summary>
//         /// <param name="fingerprint1">Fingerprint đầu tiên (Base64 string)</param>
//         /// <param name="fingerprint2">Fingerprint thứ hai (Base64 string)</param>
//         /// <returns>Tỷ lệ tương đồng từ 0.0 đến 100.0</returns>
//         private float CalculateFingerprintSimilarity(string fingerprint1, string fingerprint2)
//         {
//             try
//             {
//                 if (string.IsNullOrEmpty(fingerprint1) || string.IsNullOrEmpty(fingerprint2))
//                     return 0f;

//                 if (fingerprint1.Equals(fingerprint2, StringComparison.Ordinal))
//                     return 100f;

//                 // Parse Base64 thành uint32 arrays
//                 var chromaData1 = ParseBase64ToUint32Array(fingerprint1);
//                 var chromaData2 = ParseBase64ToUint32Array(fingerprint2);
//                 Console.WriteLine($"Parsed chromaData1 length: {chromaData1?.Length}, chromaData2 length: {chromaData2?.Length}");

//                 if (chromaData1 != null && chromaData2 != null)
//                 {
//                     return CalculateChromaprintSimilarity(chromaData1, chromaData2);
//                 }

//                 // Fallback to byte comparison
//                 var bytes1 = Convert.FromBase64String(fingerprint1);
//                 var bytes2 = Convert.FromBase64String(fingerprint2);
//                 return CalculateByteSimilarity(bytes1, bytes2);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error calculating fingerprint similarity");
//                 return 0f;
//             }
//         }

//         /// <summary>
//         /// Parse Base64 string thành uint32 array cho Chromaprint
//         /// </summary>
//         private uint[]? ParseBase64ToUint32Array(string base64String)
//         {
//             try
//             {
//                 var bytes = Convert.FromBase64String(base64String);

//                 // Convert bytes thành uint32 array
//                 var uint32Count = bytes.Length / 4;
//                 if (uint32Count == 0) return null;

//                 var result = new uint[uint32Count];
//                 for (int i = 0; i < uint32Count; i++)
//                 {
//                     int offset = i * 4;
//                     if (offset + 3 < bytes.Length)
//                     {
//                         result[i] = BitConverter.ToUInt32(bytes, offset);
//                     }
//                 }

//                 _logger.LogDebug($"Parsed {uint32Count} uint32 values from Base64");
//                 return result;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogWarning(ex, "Failed to parse Base64 to uint32 array");
//                 return null;
//             }
//         }

//         /// <summary>
//         /// Tính similarity cho Chromaprint fingerprints với advanced matching
//         /// </summary>
//         private float CalculateChromaprintSimilarity(uint[] fingerprint1, uint[] fingerprint2)
//         {
//             var similarities = new List<float>();

//             // 1. PRIORITY: Exact subsequence matching (cho audio clips)
//             var subsequenceMatch = CalculateChromaprintSubsequenceMatch(fingerprint1, fingerprint2);
//             if (subsequenceMatch >= 95f)
//             {
//                 _logger.LogDebug($"Found exact Chromaprint subsequence match: {subsequenceMatch:F2}%");
//                 return Math.Min(100f, subsequenceMatch);
//             }
//             similarities.Add(subsequenceMatch);

//             // // 2. Sliding window với bit tolerance
//             // similarities.Add(CalculateChromaprintSlidingWindowOriginal(fingerprint1, fingerprint2));

//             // // 3. Hamming distance với bit masking
//             // similarities.Add(CalculateChromaprintHamming(fingerprint1, fingerprint2));

//             // // 4. Cross-correlation analysis
//             // similarities.Add(CalculateChromaprintCrossCorrelation(fingerprint1, fingerprint2));

//             var stopwatch = Stopwatch.StartNew();
//             stopwatch.Restart();
//             similarities.Add(CalculateChromaprintSlidingWindowOriginal(fingerprint1, fingerprint2));
//             Console.WriteLine($"\n\n ==> SlidingWindow completed in {stopwatch.ElapsedMilliseconds}ms");

//             // 3. Hamming distance với bit masking
//             stopwatch.Restart();
//             similarities.Add(CalculateChromaprintHamming(fingerprint1, fingerprint2));
//             Console.WriteLine($"\n\n ==> Hamming completed in {stopwatch.ElapsedMilliseconds}ms");

//             // 4. Cross-correlation analysis
//             stopwatch.Restart();
//             similarities.Add(CalculateChromaprintCrossCorrelation(fingerprint1, fingerprint2));
//             Console.WriteLine($"\n\n ==> CrossCorrelation completed in {stopwatch.ElapsedMilliseconds}ms");

//             stopwatch.Stop();

//             var maxSimilarity = similarities.Max();
//             _logger.LogDebug($"Chromaprint similarities: Subseq={similarities[0]:F2}%, Sliding={similarities[1]:F2}%, " +
//                             $"Hamming={similarities[2]:F2}%, CrossCorr={similarities[3]:F2}%, Max={maxSimilarity:F2}%");

//             return maxSimilarity;
//         }

//         /// <summary>
//         /// Tìm exact subsequence match trong Chromaprint fingerprints
//         /// </summary>
//         private float CalculateChromaprintSubsequenceMatch(uint[] fingerprint1, uint[] fingerprint2)
//         {
//             var shorter = fingerprint1.Length <= fingerprint2.Length ? fingerprint1 : fingerprint2;
//             var longer = fingerprint1.Length > fingerprint2.Length ? fingerprint1 : fingerprint2;

//             if (shorter.Length == 0) return 0f;

//             const int BIT_TOLERANCE = 2; // Allow 2-bit differences per uint32
//             float bestMatchScore = 0f;
//             int bestMatchPosition = -1;

//             for (int i = 0; i <= longer.Length - shorter.Length; i++)
//             {
//                 int perfectMatches = 0;
//                 int tolerantMatches = 0;

//                 for (int j = 0; j < shorter.Length; j++)
//                 {
//                     uint val1 = shorter[j];
//                     uint val2 = longer[i + j];

//                     if (val1 == val2)
//                     {
//                         perfectMatches++;
//                         tolerantMatches++;
//                     }
//                     else
//                     {
//                         uint xor = val1 ^ val2;
//                         int bitDiff = CountSetBits(xor);

//                         if (bitDiff <= BIT_TOLERANCE)
//                         {
//                             tolerantMatches++;
//                         }
//                     }
//                 }

//                 float matchScore = ((float)tolerantMatches / shorter.Length) * 100f;
//                 if (matchScore > bestMatchScore)
//                 {
//                     bestMatchScore = matchScore;
//                     bestMatchPosition = i;
//                 }

//                 // Early exit cho perfect match
//                 if (perfectMatches == shorter.Length)
//                 {
//                     _logger.LogDebug($"Found PERFECT Chromaprint subsequence match at position {i}");
//                     return 100f;
//                 }
//             }

//             // Nếu tìm thấy match > 85% với tolerance
//             if (bestMatchScore >= 85f)
//             {
//                 _logger.LogDebug($"Found high-quality Chromaprint subsequence match: {bestMatchScore:F2}% at position {bestMatchPosition}");

//                 // Bonus cho high-coverage matches
//                 float coverage = ((float)shorter.Length / longer.Length);
//                 float bonus = coverage * 10f; // Up to 10% bonus

//                 return Math.Min(100f, bestMatchScore + bonus);
//             }

//             return bestMatchScore;
//         }

//         /// <summary>
//         /// Sliding window với adaptive window size cho Chromaprint
//         /// </summary>
//         private float CalculateChromaprintSlidingWindowOriginal(uint[] fingerprint1, uint[] fingerprint2)
//         {
//             var shorter = fingerprint1.Length <= fingerprint2.Length ? fingerprint1 : fingerprint2;
//             var longer = fingerprint1.Length > fingerprint2.Length ? fingerprint1 : fingerprint2;

//             if (shorter.Length < 4) return CalculateSimpleHamming(fingerprint1, fingerprint2);

//             float maxSimilarity = 0f;

//             // Multiple window sizes: 100%, 75%, 50%, 25%
//             var windowSizes = new[] {
//                 shorter.Length,
//                 shorter.Length * 3 / 4,
//                 shorter.Length / 2,
//                 shorter.Length / 4
//             }.Where(s => s >= 4).ToArray();

//             foreach (int windowSize in windowSizes)
//             {
//                 for (int i = 0; i <= longer.Length - windowSize; i++)
//                 {
//                     for (int j = 0; j <= shorter.Length - windowSize; j++)
//                     {
//                         float similarity = CalculateWindowSimilarity(
//                             longer.Skip(i).Take(windowSize).ToArray(),
//                             shorter.Skip(j).Take(windowSize).ToArray()
//                         );

//                         float coverage = (float)windowSize / shorter.Length;
//                         float adjustedSimilarity = similarity * coverage;
//                         maxSimilarity = Math.Max(maxSimilarity, adjustedSimilarity);

//                         if (similarity > 85f)
//                         {
//                             float bonus = coverage > 0.7f ? 1.15f : 1.1f;
//                             return Math.Min(100f, adjustedSimilarity * bonus);
//                         }
//                     }
//                 }
//             }

//             return maxSimilarity;
//         }


//         private float CalculateChromaprintSlidingWindow(uint[] fingerprint1, uint[] fingerprint2)
//         {
//             var shorter = fingerprint1.Length <= fingerprint2.Length ? fingerprint1 : fingerprint2;
//             var longer = fingerprint1.Length > fingerprint2.Length ? fingerprint1 : fingerprint2;

//             if (shorter.Length < 4) return CalculateSimpleHamming(fingerprint1, fingerprint2);

//             float maxSimilarity = 0f;

//             // GIỮ NGUYÊN window sizes
//             var windowSizes = new[] {
//         shorter.Length,
//         shorter.Length * 3 / 4,
//         shorter.Length / 2,
//         shorter.Length / 4
//     }.Where(s => s >= 4).ToArray();

//             foreach (int windowSize in windowSizes)
//             {
//                 for (int i = 0; i <= longer.Length - windowSize; i++)
//                 {
//                     for (int j = 0; j <= shorter.Length - windowSize; j++)
//                     {
//                         // OPTION 1: Sử dụng ArraySegment thay vì Skip().Take().ToArray()
//                         var longerSegment = new ArraySegment<uint>(longer, i, windowSize);
//                         var shorterSegment = new ArraySegment<uint>(shorter, j, windowSize);

//                         float similarity = CalculateWindowSimilaritySegment(longerSegment, shorterSegment);

//                         float coverage = (float)windowSize / shorter.Length;
//                         float adjustedSimilarity = similarity * coverage;
//                         maxSimilarity = Math.Max(maxSimilarity, adjustedSimilarity);

//                         if (similarity > 85f)
//                         {
//                             float bonus = coverage > 0.7f ? 1.15f : 1.1f;
//                             return Math.Min(100f, adjustedSimilarity * bonus);
//                         }
//                     }
//                 }
//             }

//             return maxSimilarity;
//         }

//         /// <summary>
//         /// Window similarity using ArraySegment - exactly same logic as original
//         /// </summary>
//         private float CalculateWindowSimilaritySegment(ArraySegment<uint> window1, ArraySegment<uint> window2)
//         {
//             if (window1.Count != window2.Count) return 0f;

//             const int BIT_TOLERANCE = 3;
//             float totalScore = 0f;

//             for (int i = 0; i < window1.Count; i++)
//             {
//                 uint val1 = window1.Array![window1.Offset + i];
//                 uint val2 = window2.Array![window2.Offset + i];

//                 if (val1 == val2)
//                 {
//                     totalScore += 1.0f;
//                 }
//                 else
//                 {
//                     uint xor = val1 ^ val2;
//                     int bitDiff = CountSetBits(xor);

//                     if (bitDiff <= BIT_TOLERANCE)
//                     {
//                         totalScore += 0.95f;
//                     }
//                     else if (bitDiff <= 6)
//                     {
//                         totalScore += Math.Max(0f, 0.8f - (bitDiff * 0.08f));
//                     }
//                 }
//             }

//             return (totalScore / window1.Count) * 100f;
//         }

//         //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//         /// <summary>
//         /// Tính similarity cho một window với bit tolerance
//         /// </summary>
//         private float CalculateWindowSimilarity(uint[] window1, uint[] window2)
//         {
//             if (window1.Length != window2.Length) return 0f;

//             const int BIT_TOLERANCE = 3;
//             float totalScore = 0f;

//             for (int i = 0; i < window1.Length; i++)
//             {
//                 if (window1[i] == window2[i])
//                 {
//                     totalScore += 1.0f;
//                 }
//                 else
//                 {
//                     uint xor = window1[i] ^ window2[i];
//                     int bitDiff = CountSetBits(xor);

//                     if (bitDiff <= BIT_TOLERANCE)
//                     {
//                         totalScore += 0.95f;
//                     }
//                     else if (bitDiff <= 6)
//                     {
//                         totalScore += Math.Max(0f, 0.8f - (bitDiff * 0.08f));
//                     }
//                 }
//             }

//             return (totalScore / window1.Length) * 100f;
//         }

//         /// <summary>
//         /// Hamming distance với bit masking cho Chromaprint
//         /// </summary>
//         private float CalculateChromaprintHamming(uint[] fingerprint1, uint[] fingerprint2)
//         {
//             if (fingerprint1.Length != fingerprint2.Length)
//                 return CalculateCrossArrayHamming(fingerprint1, fingerprint2);

//             // Ignore least significant bits (noise-prone)
//             const uint MASK = 0xFFFFFFF0; // Ignore 4 LSBs

//             int totalBits = fingerprint1.Length * 28; // 28 bits per uint (after masking)
//             int matchingBits = 0;

//             for (int i = 0; i < fingerprint1.Length; i++)
//             {
//                 uint masked1 = fingerprint1[i] & MASK;
//                 uint masked2 = fingerprint2[i] & MASK;
//                 uint xorResult = masked1 ^ masked2;

//                 matchingBits += 28 - CountSetBits(xorResult);
//             }

//             return ((float)matchingBits / totalBits) * 100f;
//         }

//         /// <summary>
//         /// Cross-correlation analysis cho Chromaprint fingerprints
//         /// </summary>
//         private float CalculateChromaprintCrossCorrelation(uint[] fingerprint1, uint[] fingerprint2)
//         {
//             var shorter = fingerprint1.Length <= fingerprint2.Length ? fingerprint1 : fingerprint2;
//             var longer = fingerprint1.Length > fingerprint2.Length ? fingerprint1 : fingerprint2;

//             if (shorter.Length < 8) return 0f;

//             float maxCorrelation = 0f;
//             int searchRange = Math.Min(longer.Length - shorter.Length + 1, 50);

//             for (int offset = 0; offset < searchRange; offset++)
//             {
//                 float correlation = 0f;
//                 float weightSum = 0f;

//                 for (int i = 0; i < shorter.Length; i++)
//                 {
//                     // Weighted correlation - center samples have higher weight
//                     float weight = 1.0f;
//                     if (shorter.Length > 16)
//                     {
//                         float centerDistance = Math.Abs(i - shorter.Length / 2f) / (shorter.Length / 2f);
//                         weight = 1.0f - 0.3f * centerDistance;
//                     }

//                     uint val1 = shorter[i];
//                     uint val2 = longer[offset + i];

//                     if (val1 == val2)
//                     {
//                         correlation += weight;
//                     }
//                     else
//                     {
//                         uint xor = val1 ^ val2;
//                         int bitDiff = CountSetBits(xor);
//                         float bitSimilarity = Math.Max(0f, 1f - (bitDiff / 32f));
//                         correlation += weight * bitSimilarity;
//                     }

//                     weightSum += weight;
//                 }

//                 float normalizedCorrelation = (correlation / weightSum) * 100f;
//                 maxCorrelation = Math.Max(maxCorrelation, normalizedCorrelation);
//             }

//             return maxCorrelation;
//         }

//         /// <summary>
//         /// Cross-array Hamming cho arrays khác độ dài
//         /// </summary>
//         private float CalculateCrossArrayHamming(uint[] fingerprint1, uint[] fingerprint2)
//         {
//             var shorter = fingerprint1.Length <= fingerprint2.Length ? fingerprint1 : fingerprint2;
//             var longer = fingerprint1.Length > fingerprint2.Length ? fingerprint1 : fingerprint2;

//             float maxSimilarity = 0f;

//             for (int offset = 0; offset <= longer.Length - shorter.Length; offset++)
//             {
//                 int matchingBits = 0;
//                 int totalBits = shorter.Length * 32;

//                 for (int i = 0; i < shorter.Length; i++)
//                 {
//                     uint xorResult = shorter[i] ^ longer[offset + i];
//                     matchingBits += 32 - CountSetBits(xorResult);
//                 }

//                 float similarity = ((float)matchingBits / totalBits) * 100f;
//                 maxSimilarity = Math.Max(maxSimilarity, similarity);
//             }

//             return maxSimilarity;
//         }

//         /// <summary>
//         /// Simple Hamming cho trường hợp fallback
//         /// </summary>
//         private float CalculateSimpleHamming(uint[] fingerprint1, uint[] fingerprint2)
//         {
//             if (fingerprint1.Length == 0 && fingerprint2.Length == 0)
//                 return 100f;

//             if (fingerprint1.Length == 0 || fingerprint2.Length == 0)
//                 return 0f;

//             int minLength = Math.Min(fingerprint1.Length, fingerprint2.Length);
//             int matchingBits = 0;
//             int totalBits = minLength * 32;

//             for (int i = 0; i < minLength; i++)
//             {
//                 uint xorResult = fingerprint1[i] ^ fingerprint2[i];
//                 matchingBits += 32 - CountSetBits(xorResult);
//             }

//             return ((float)matchingBits / totalBits) * 100f;
//         }

//         /// <summary>
//         /// Fallback byte similarity cho khi uint32 parsing fails
//         /// </summary>
//         private float CalculateByteSimilarity(byte[] bytes1, byte[] bytes2)
//         {
//             if (bytes1.Length == 0 || bytes2.Length == 0) return 0f;
//             if (bytes1.SequenceEqual(bytes2)) return 100f;

//             // Simple byte-level comparison
//             var shorter = bytes1.Length <= bytes2.Length ? bytes1 : bytes2;
//             var longer = bytes1.Length > bytes2.Length ? bytes1 : bytes2;

//             // Check for exact substring match
//             for (int i = 0; i <= longer.Length - shorter.Length; i++)
//             {
//                 bool isMatch = true;
//                 for (int j = 0; j < shorter.Length; j++)
//                 {
//                     if (longer[i + j] != shorter[j])
//                     {
//                         isMatch = false;
//                         break;
//                     }
//                 }
//                 if (isMatch) return 100f;
//             }

//             // Hamming distance for same length
//             if (bytes1.Length == bytes2.Length)
//             {
//                 int matches = 0;
//                 for (int i = 0; i < bytes1.Length; i++)
//                 {
//                     if (bytes1[i] == bytes2[i]) matches++;
//                 }
//                 return ((float)matches / bytes1.Length) * 100f;
//             }

//             // Jaccard similarity for different lengths
//             var set1 = new HashSet<byte>(bytes1);
//             var set2 = new HashSet<byte>(bytes2);
//             var intersection = set1.Intersect(set2).Count();
//             var union = set1.Union(set2).Count();

//             return union > 0 ? ((float)intersection / union) * 100f : 0f;
//         }

//         /// <summary>
//         /// Đếm số bit được set trong uint32
//         /// </summary>
//         private int CountSetBits(uint value)
//         {
//             int count = 0;
//             while (value != 0)
//             {
//                 value &= value - 1;
//                 count++;
//             }
//             return count;
//         }

//         public void Dispose()
//         {
//             // Cleanup if needed
//         }
//     }
// }