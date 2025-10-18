// using Microsoft.Extensions.Logging;
// using AcoustID;
// using AcoustID.Chromaprint;
// using NAudio.Wave;
// using UserService.Infrastructure.Models.Audio;
// using NAudio.MediaFoundation;
// using System.Security.Cryptography;
// using UserService.Infrastructure.Models.Audio.AcoustID;
// using System.Numerics;

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

//                         // Tính toán tỷ lệ tương đồng với Chromaprint-specific algorithms
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
//                         // Thêm kết quả với 0% similarity cho candidate lỗi
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
//         /// Tính toán tỷ lệ tương đồng giữa hai audio fingerprints với Chromaprint-specific optimizations
//         /// </summary>
//         /// <param name="fingerprint1">Fingerprint đầu tiên (string)</param>
//         /// <param name="fingerprint2">Fingerprint thứ hai (string)</param>
//         /// <returns>Tỷ lệ tương đồng từ 0.0 đến 100.0</returns>
//         private float CalculateFingerprintSimilarity(string fingerprint1, string fingerprint2)
//         {
//             try
//             {
//                 if (string.IsNullOrEmpty(fingerprint1) || string.IsNullOrEmpty(fingerprint2))
//                     return 0f;

//                 if (fingerprint1.Equals(fingerprint2, StringComparison.Ordinal))
//                     return 100f;

//                 // Parse Chromaprint fingerprints thành raw data
//                 var chromaData1 = ParseChromaprintFingerprint(fingerprint1);
//                 var chromaData2 = ParseChromaprintFingerprint(fingerprint2);

//                 Console.WriteLine($"Parsed Chromaprint data lengths: {chromaData1?.Length}, {chromaData2?.Length}");

//                 if (chromaData1 != null && chromaData2 != null)
//                 {
//                     // Áp dụng Chromaprint-specific matching algorithms
//                     return CalculateChromaprintSimilarity(chromaData1, chromaData2);
//                 }

//                 // Fallback to byte comparison nếu parse thất bại
//                 var bytes1 = ParseFingerprintToBytes(fingerprint1);
//                 var bytes2 = ParseFingerprintToBytes(fingerprint2);
//                 return CalculateByteFingerprintSimilarity(bytes1, bytes2);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error calculating fingerprint similarity");
//                 return 0f;
//             }
//         }

//         /// <summary>
//         /// Parse Chromaprint fingerprint string thành uint32 array (raw format)
//         /// </summary>
//         private uint[]? ParseChromaprintFingerprint(string fingerprint)
//         {
//             try
//             {
//                 // Chromaprint fingerprints thường là base64 encoded
//                 if (!fingerprint.StartsWith("AQA"))
//                 {
//                     Console.WriteLine("Fingerprint does not appear to be Chromaprint format");
//                     return null;
//                 }

//                 var bytes = Convert.FromBase64String(fingerprint);

//                 // Chromaprint format: header + fingerprint data
//                 // Skip header (first 8 bytes) và convert sang uint32 array
//                 if (bytes.Length < 12)
//                 {
//                     Console.WriteLine("Chromaprint data too short: " + bytes.Length);
//                     return null;
//                 }
                    

//                 // bytes 8-11: number of fingerprints
//                 int numFingerprints = BitConverter.ToInt32(bytes, 8);
//                 if (numFingerprints <= 0 || bytes.Length < 12 + (numFingerprints * 4))
//                 {
//                     Console.WriteLine("Invalid number of Chromaprint fingerprints: " + numFingerprints);
//                     return null;
//                 }

//                 var fingerprints = new uint[numFingerprints];
//                 for (int i = 0; i < numFingerprints; i++)
//                 {
//                     int offset = 12 + (i * 4);
//                     fingerprints[i] = BitConverter.ToUInt32(bytes, offset);
//                 }
                
//                 _logger.LogDebug($"Parsed {numFingerprints} Chromaprint fingerprints from base64");
//                 return fingerprints;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogWarning(ex, "Failed to parse Chromaprint fingerprint");
//                 return null;
//             }
//         }

//         /// <summary>
//         /// Tính similarity cho Chromaprint fingerprints với fuzzy matching
//         /// </summary>
//         private float CalculateChromaprintSimilarity(uint[] fingerprint1, uint[] fingerprint2)
//         {
//             Console.WriteLine($"Calculating Chromaprint similarity between fingerprints of lengths {fingerprint1.Length} and {fingerprint2.Length}");
//             var similarities = new List<float>();

//             // 1. PRIORITY: Exact subsequence matching (cho audio clips)
//             var subsequenceMatch = CalculateChromaprintSubsequenceMatch(fingerprint1, fingerprint2);
//             if (subsequenceMatch >= 95f)
//             {
//                 _logger.LogDebug($"Found exact Chromaprint subsequence match: {subsequenceMatch:F2}%");
//                 return Math.Min(100f, subsequenceMatch);
//             }
//             similarities.Add(subsequenceMatch);

//             // 2. Sliding window với bit tolerance
//             similarities.Add(CalculateChromaprintSlidingWindow(fingerprint1, fingerprint2));

//             // 3. Hamming distance với bit masking
//             similarities.Add(CalculateChromaprintHammingWithMask(fingerprint1, fingerprint2));

//             // 4. Cross-correlation analysis
//             similarities.Add(CalculateChromaprintCrossCorrelation(fingerprint1, fingerprint2));

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

//             if (shorter.Length == 0)
//                 return 0f;

//             // Tìm exact match với bit tolerance
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
//         private float CalculateChromaprintSlidingWindow(uint[] fingerprint1, uint[] fingerprint2)
//         {
//             var shorter = fingerprint1.Length <= fingerprint2.Length ? fingerprint1 : fingerprint2;
//             var longer = fingerprint1.Length > fingerprint2.Length ? fingerprint1 : fingerprint2;

//             if (shorter.Length < 4)
//                 return CalculateSimpleChromaprintHamming(fingerprint1, fingerprint2);

//             float maxSimilarity = 0f;
            
//             // Multiple window sizes: 100%, 75%, 50%, 25% của shorter array
//             var windowSizes = new[] { 
//                 shorter.Length, 
//                 shorter.Length * 3 / 4, 
//                 shorter.Length / 2, 
//                 shorter.Length / 4 
//             }.Where(s => s >= 4).ToArray();

//             foreach (int windowSize in windowSizes)
//             {
//                 // Slide qua longer array
//                 for (int i = 0; i <= longer.Length - windowSize; i++)
//                 {
//                     // So sánh với shorter array
//                     for (int j = 0; j <= shorter.Length - windowSize; j++)
//                     {
//                         float similarity = CalculateChromaprintWindowSimilarity(
//                             longer.Skip(i).Take(windowSize).ToArray(),
//                             shorter.Skip(j).Take(windowSize).ToArray()
//                         );

//                         // Adjust similarity dựa trên window size
//                         float coverage = (float)windowSize / shorter.Length;
//                         float adjustedSimilarity = similarity * coverage;
//                         maxSimilarity = Math.Max(maxSimilarity, adjustedSimilarity);

//                         if (similarity > 85f) // Early exit cho high similarity
//                         {
//                             float bonus = coverage > 0.7f ? 1.15f : 1.1f; // 15% bonus cho large windows
//                             return Math.Min(100f, adjustedSimilarity * bonus);
//                         }
//                     }
//                 }
//             }

//             return maxSimilarity;
//         }

//         /// <summary>
//         /// Tính similarity cho một window với bit tolerance
//         /// </summary>
//         private float CalculateChromaprintWindowSimilarity(uint[] window1, uint[] window2)
//         {
//             if (window1.Length != window2.Length)
//                 return 0f;

//             const int BIT_TOLERANCE = 3; // Slightly higher tolerance cho windows
//             float totalScore = 0f;

//             for (int i = 0; i < window1.Length; i++)
//             {
//                 if (window1[i] == window2[i])
//                 {
//                     totalScore += 1.0f; // Perfect match
//                 }
//                 else
//                 {
//                     uint xor = window1[i] ^ window2[i];
//                     int bitDiff = CountSetBits(xor);

//                     if (bitDiff <= BIT_TOLERANCE)
//                     {
//                         totalScore += 0.95f; // Near perfect
//                     }
//                     else if (bitDiff <= 6) // Extended tolerance
//                     {
//                         totalScore += Math.Max(0f, 0.8f - (bitDiff * 0.08f)); // Graduated scoring
//                     }
//                 }
//             }

//             return (totalScore / window1.Length) * 100f;
//         }

//         /// <summary>
//         /// Hamming distance với bit masking cho Chromaprint
//         /// </summary>
//         private float CalculateChromaprintHammingWithMask(uint[] fingerprint1, uint[] fingerprint2)
//         {
//             if (fingerprint1.Length != fingerprint2.Length)
//                 return CalculateCrossArrayChromaprintHamming(fingerprint1, fingerprint2);

//             // Chromaprint-specific: Ignore least significant bits (more prone to noise)
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

//             if (shorter.Length < 8)
//                 return 0f;

//             float maxCorrelation = 0f;
//             int searchRange = Math.Min(longer.Length - shorter.Length + 1, 50); // Limit search range

//             for (int offset = 0; offset < searchRange; offset++)
//             {
//                 float correlation = 0f;
//                 float weightSum = 0f;
                
//                 for (int i = 0; i < shorter.Length; i++)
//                 {
//                     // Weighted correlation - center samples có weight cao hơn
//                     float weight = 1.0f;
//                     if (shorter.Length > 16)
//                     {
//                         float centerDistance = Math.Abs(i - shorter.Length / 2f) / (shorter.Length / 2f);
//                         weight = 1.0f - 0.3f * centerDistance; // Center weight 1.0, edge weight 0.7
//                     }

//                     uint val1 = shorter[i];
//                     uint val2 = longer[offset + i];

//                     // Bit-level correlation
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
//         /// Cross-array Hamming cho Chromaprint arrays khác độ dài
//         /// </summary>
//         private float CalculateCrossArrayChromaprintHamming(uint[] fingerprint1, uint[] fingerprint2)
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
//         private float CalculateSimpleChromaprintHamming(uint[] fingerprint1, uint[] fingerprint2)
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
//         /// Đếm số bit được set trong uint32 - optimized version
//         /// </summary>
//         private int CountSetBits(uint value)
//         {
//             // Brian Kernighan's algorithm - faster than bit shifting
//             int count = 0;
//             while (value != 0)
//             {
//                 value &= value - 1;
//                 count++;
//             }
//             return count;
//         }

//         #region Fallback Methods (giữ nguyên từ code cũ)
        
//         /// <summary>
//         /// Tính toán tỷ lệ tương đồng giữa hai audio fingerprints dưới dạng byte arrays
//         /// Ưu tiên exact substring matching: nếu một audio giống hoàn toàn một đoạn trong audio khác -> 100%
//         /// </summary>
//         /// <param name="fingerprint1">Fingerprint đầu tiên (bytes)</param>
//         /// <param name="fingerprint2">Fingerprint thứ hai (bytes)</param>
//         /// <returns>Tỷ lệ tương đồng từ 0.0 đến 100.0</returns>
//         private float CalculateByteFingerprintSimilarity(byte[] fingerprint1, byte[] fingerprint2)
//         {
//             try
//             {
//                 if (fingerprint1.Length == 0 || fingerprint2.Length == 0)
//                 {
//                     return 0f;
//                 }

//                 // 1. PRIORITY: Exact substring matching (nếu tìm thấy -> return 100% ngay)
//                 var substringMatch = CalculateByteSubstringMatchingSimilarity(fingerprint1, fingerprint2);
//                 if (substringMatch >= 100f)
//                 {
//                     _logger.LogDebug($"Found exact substring match - returning 100% similarity");
//                     return 100f;
//                 }

//                 // 2. Nếu không có exact substring match, thử các algorithms khác
//                 var similarities = new List<float> { substringMatch };

//                 // Sliding window matching với byte chunks
//                 similarities.Add(CalculateByteSlidingWindowSimilarity(fingerprint1, fingerprint2));

//                 // Hamming distance cho cùng độ dài
//                 if (fingerprint1.Length == fingerprint2.Length)
//                 {
//                     similarities.Add(CalculateHammingDistanceSimilarity(fingerprint1, fingerprint2));
//                 }

//                 // Jaccard similarity cho khác độ dài
//                 similarities.Add(CalculateJaccardSimilarity(fingerprint1, fingerprint2));

//                 // Longest Common Byte Subsequence
//                 similarities.Add(CalculateByteLCSSimilarity(fingerprint1, fingerprint2));

//                 // Trả về similarity cao nhất
//                 var maxSimilarity = similarities.Max();
//                 _logger.LogDebug($"Byte similarity results: Substring={similarities[0]:F2}%, Sliding={similarities[1]:F2}%, " +
//                                 $"Hamming={(similarities.Count > 2 && fingerprint1.Length == fingerprint2.Length ? similarities[2] : 0):F2}%, " +
//                                 $"Jaccard={similarities[similarities.Count-2]:F2}%, LCS={similarities.Last():F2}%, Max={maxSimilarity:F2}%");
                
//                 return maxSimilarity;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error calculating byte fingerprint similarity");
//                 return 0f;
//             }
//         }

//         /// <summary>
//         /// Convert fingerprint string thành byte array để xử lý
//         /// </summary>
//         private byte[] ParseFingerprintToBytes(string fingerprint)
//         {
//             try
//             {
//                 var cleanString = fingerprint.Trim();

//                 // Nếu là base64 encoded fingerprint (AcoustID format)
//                 if (cleanString.StartsWith("AQAA"))
//                 {
//                     try
//                     {
//                         return Convert.FromBase64String(cleanString);
//                     }
//                     catch
//                     {
//                         // Fallback nếu base64 decode thất bại
//                     }
//                 }

//                 // Nếu là hex string
//                 if (cleanString.Length % 2 == 0 && cleanString.All(c => char.IsDigit(c) || "ABCDEFabcdef".Contains(c)))
//                 {
//                     try
//                     {
//                         return Convert.FromHexString(cleanString);
//                     }
//                     catch
//                     {
//                         // Fallback nếu hex decode thất bại
//                     }
//                 }

//                 // Fallback: UTF8 encoding
//                 return System.Text.Encoding.UTF8.GetBytes(cleanString);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogWarning(ex, "Failed to parse fingerprint, using UTF8 encoding");
//                 return System.Text.Encoding.UTF8.GetBytes(fingerprint);
//             }
//         }

//         /// <summary>
//         /// Tính similarity dựa trên byte substring matching
//         /// Nếu một audio giống hoàn toàn một đoạn trong audio khác -> 100% match
//         /// </summary>
//         private float CalculateByteSubstringMatchingSimilarity(byte[] fingerprint1, byte[] fingerprint2)
//         {
//             // Tìm array ngắn hơn và dài hơn
//             var shorter = fingerprint1.Length <= fingerprint2.Length ? fingerprint1 : fingerprint2;
//             var longer = fingerprint1.Length > fingerprint2.Length ? fingerprint1 : fingerprint2;

//             // Nếu độ dài bằng nhau và giống hệt -> 100%
//             if (shorter.Length == longer.Length)
//             {
//                 return shorter.SequenceEqual(longer) ? 100f : 0f;
//             }

//             // Kiểm tra nếu array ngắn là exact substring của array dài
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

//                 if (isMatch)
//                 {
//                     // EXACT SUBSTRING MATCH = 100% similarity
//                     // Vì một audio giống hoàn toàn một đoạn trong audio khác
//                     _logger.LogDebug($"Found exact substring match: shorter length={shorter.Length}, longer length={longer.Length}, match position={i}");
//                     return 100f;
//                 }
//             }

//             return 0f;
//         }

//         /// <summary>
//         /// Tính similarity sử dụng sliding window với byte arrays
//         /// Tìm kiếm best matching segment, nếu tìm thấy perfect segment -> gần 100%
//         /// </summary>
//         private float CalculateByteSlidingWindowSimilarity(byte[] fingerprint1, byte[] fingerprint2)
//         {
//             var shorter = fingerprint1.Length <= fingerprint2.Length ? fingerprint1 : fingerprint2;
//             var longer = fingerprint1.Length > fingerprint2.Length ? fingerprint1 : fingerprint2;

//             if (shorter.Length == 0 || longer.Length == 0)
//                 return 0f;

//             // Thử nhiều window sizes để tìm best match
//             var windowSizes = new[]
//             {
//                 shorter.Length, // Full length first
//                 Math.Max(4, shorter.Length * 3 / 4), // 75%
//                 Math.Max(4, shorter.Length / 2), // 50%
//                 Math.Max(4, shorter.Length / 4)  // 25%
//             }.Distinct().OrderByDescending(x => x).ToArray();
            
//             float maxSimilarity = 0f;

//             foreach (var windowSize in windowSizes)
//             {
//                 if (windowSize > longer.Length)
//                     continue;

//                 // Slide window qua array dài hơn
//                 for (int i = 0; i <= longer.Length - windowSize; i++)
//                 {
//                     // So sánh với đoạn tương ứng trong shorter array
//                     for (int j = 0; j <= shorter.Length - windowSize; j++)
//                     {
//                         int matches = 0;
//                         for (int k = 0; k < windowSize; k++)
//                         {
//                             if (longer[i + k] == shorter[j + k])
//                                 matches++;
//                         }
                        
//                         float similarity = ((float)matches / windowSize) * 100f;
                        
//                         // Nếu tìm thấy perfect window match với kích thước lớn
//                         if (similarity >= 100f && windowSize >= shorter.Length * 0.8f)
//                         {
//                             _logger.LogDebug($"Found perfect window match: size={windowSize}, shorter_length={shorter.Length}, coverage={((float)windowSize/shorter.Length*100):F1}%");
//                             return 98f; // Near perfect cho large window match
//                         }
                        
//                         maxSimilarity = Math.Max(maxSimilarity, similarity * (windowSize / (float)shorter.Length));
                        
//                         if (similarity >= 100f && windowSize >= shorter.Length / 2) // Perfect match trên 50% độ dài
//                         {
//                             return Math.Min(95f, 70f + (windowSize / (float)shorter.Length * 25f));
//                         }
//                     }
//                 }
                
//                 // Early exit nếu đã tìm thấy similarity cao với window lớn
//                 if (maxSimilarity > 90f)
//                     break;
//             }

//             return maxSimilarity;
//         }

//         /// <summary>
//         /// Improved Hamming distance với bit-level comparison
//         /// </summary>
//         private float CalculateHammingDistanceSimilarity(byte[] fingerprint1, byte[] fingerprint2)
//         {
//             if (fingerprint1.Length != fingerprint2.Length)
//                 return 0f;

//             int differences = 0;
//             int totalBits = fingerprint1.Length * 8;

//             // Vectorized comparison cho performance tốt hơn
//             for (int i = 0; i < fingerprint1.Length; i++)
//             {
//                 byte xorResult = (byte)(fingerprint1[i] ^ fingerprint2[i]);
                
//                 // Sử dụng bit manipulation để đếm nhanh hơn
//                 differences += BitOperations.PopCount(xorResult);
//             }

//             float similarity = ((float)(totalBits - differences) / totalBits) * 100f;
//             return Math.Max(0f, Math.Min(100f, similarity));
//         }

//         /// <summary>
//         /// Improved Jaccard similarity với byte frequency
//         /// </summary>
//         private float CalculateJaccardSimilarity(byte[] fingerprint1, byte[] fingerprint2)
//         {
//             // Sử dụng Dictionary để đếm frequency thay vì HashSet đơn giản
//             var freq1 = new Dictionary<byte, int>();
//             var freq2 = new Dictionary<byte, int>();

//             // Count frequencies
//             foreach (byte b in fingerprint1)
//                 freq1[b] = freq1.GetValueOrDefault(b, 0) + 1;

//             foreach (byte b in fingerprint2)
//                 freq2[b] = freq2.GetValueOrDefault(b, 0) + 1;

//             // Calculate intersection và union dựa trên frequencies
//             int intersection = 0;
//             var allBytes = freq1.Keys.Union(freq2.Keys);
            
//             foreach (byte b in allBytes)
//             {
//                 int count1 = freq1.GetValueOrDefault(b, 0);
//                 int count2 = freq2.GetValueOrDefault(b, 0);
//                 intersection += Math.Min(count1, count2);
//             }

//             int union = fingerprint1.Length + fingerprint2.Length - intersection;

//             if (union == 0)
//                 return 0f;

//             return ((float)intersection / union) * 100f;
//         }

//         /// <summary>
//         /// Tính similarity sử dụng Longest Common Byte Subsequence
//         /// </summary>
//         private float CalculateByteLCSSimilarity(byte[] fingerprint1, byte[] fingerprint2)
//         {
//             int lcsLength = CalculateByteLCS(fingerprint1, fingerprint2);
//             int maxLength = Math.Max(fingerprint1.Length, fingerprint2.Length);
            
//             if (maxLength == 0)
//                 return 100f;

//             return ((float)lcsLength / maxLength) * 100f;
//         }

//         /// <summary>
//         /// Tính Longest Common Subsequence length cho byte arrays
//         /// </summary>
//         private int CalculateByteLCS(byte[] arr1, byte[] arr2)
//         {
//             // Optimize cho arrays lớn bằng cách chỉ sử dụng 2 rows thay vì full matrix
//             if (arr1.Length == 0 || arr2.Length == 0)
//                 return 0;

//             var prev = new int[arr2.Length + 1];
//             var curr = new int[arr2.Length + 1];

//             for (int i = 1; i <= arr1.Length; i++)
//             {
//                 for (int j = 1; j <= arr2.Length; j++)
//                 {
//                     if (arr1[i - 1] == arr2[j - 1])
//                     {
//                         curr[j] = prev[j - 1] + 1;
//                     }
//                     else
//                     {
//                         curr[j] = Math.Max(prev[j], curr[j - 1]);
//                     }
//                 }
                
//                 // Swap arrays
//                 (prev, curr) = (curr, prev);
//                 Array.Clear(curr, 0, curr.Length);
//             }

//             return prev[arr2.Length];
//         }

//         #endregion

//         public void Dispose()
//         {
//             // Cleanup if needed
//         }
//     }
// }