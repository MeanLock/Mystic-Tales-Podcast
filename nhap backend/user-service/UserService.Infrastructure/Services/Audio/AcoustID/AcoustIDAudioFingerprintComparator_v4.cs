// using Microsoft.Extensions.Logging;
// using AcoustID;
// using AcoustID.Chromaprint;
// using NAudio.Wave;
// using UserService.Infrastructure.Models.Audio;
// using NAudio.MediaFoundation;
// using System.Security.Cryptography;
// using UserService.Infrastructure.Models.Audio.AcoustID;
// using System.Numerics;
// using System.Collections.Concurrent;
// using System.Runtime.CompilerServices;
// using System.Runtime.Intrinsics.X86;

// namespace UserService.Infrastructure.Services.Audio.AcoustID
// {
//     public class AcoustIDAudioFingerprintComparator : IDisposable
//     {
//         private readonly ILogger<AcoustIDAudioFingerprintComparator> _logger;
//         private readonly ConcurrentDictionary<string, uint[]> _fingerprintCache;
//         private readonly object _cacheLock = new object();
//         private const int MAX_CACHE_SIZE = 10000;

//         public AcoustIDAudioFingerprintComparator(ILogger<AcoustIDAudioFingerprintComparator> logger)
//         {
//             _logger = logger;
//             _fingerprintCache = new ConcurrentDictionary<string, uint[]>();
//         }

//         /// <summary>
//         /// Optimized comparison với caching và vectorization
//         /// </summary>
//         public AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparisonPercentageResult CompareTargetToCandidates(
//             AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparison comparison)
//         {
//             var result = new AcoustIDTargetToCandidatesAudioFingerprintSimilarityComparisonPercentageResult();

//             if (string.IsNullOrEmpty(comparison?.Target?.AudioFingerPrint))
//             {
//                 return result;
//             }

//             // Parse target fingerprint với caching
//             var targetFingerprint = GetOrParseFingerprint(comparison.Target.AudioFingerPrint);
//             if (targetFingerprint == null) return result;

//             // Parallel processing với optimized comparison
//             var candidates = comparison.Candidates?.Where(c => !string.IsNullOrEmpty(c?.AudioFingerPrint)).ToList();
//             if (candidates == null || !candidates.Any()) return result;

//             var results = new ConcurrentBag<AcoustIDAudioFingerprintSimilarityPercentageResult>();

//             // Parallel processing
//             Parallel.ForEach(candidates, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, candidate =>
//             {
//                 try
//                 {
//                     var candidateFingerprint = GetOrParseFingerprint(candidate.AudioFingerPrint);
//                     if (candidateFingerprint != null)
//                     {
//                         // Fast pre-check với basic metrics
//                         if (ShouldSkipDetailedComparison(targetFingerprint, candidateFingerprint))
//                         {
//                             results.Add(new AcoustIDAudioFingerprintSimilarityPercentageResult
//                             {
//                                 Id = candidate.Id,
//                                 SimilarityPercentage = 0f
//                             });
//                             return;
//                         }

//                         var similarity = CalculateOptimizedSimilarity(targetFingerprint, candidateFingerprint);
//                         results.Add(new AcoustIDAudioFingerprintSimilarityPercentageResult
//                         {
//                             Id = candidate.Id,
//                             SimilarityPercentage = similarity
//                         });
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError(ex, $"Error comparing candidate {candidate?.Id}");
//                     results.Add(new AcoustIDAudioFingerprintSimilarityPercentageResult
//                     {
//                         Id = candidate?.Id ?? "unknown",
//                         SimilarityPercentage = 0f
//                     });
//                 }
//             });

//             result.results = results.OrderByDescending(r => r.SimilarityPercentage).ToList();
//             return result;
//         }

//         /// <summary>
//         /// Cache fingerprint parsing để tránh re-parse
//         /// </summary>
//         private uint[]? GetOrParseFingerprint(string base64String)
//         {
//             if (string.IsNullOrEmpty(base64String)) return null;

//             // Check cache first
//             if (_fingerprintCache.TryGetValue(base64String, out var cached))
//             {
//                 return cached;
//             }

//             try
//             {
//                 var bytes = Convert.FromBase64String(base64String);
//                 var uint32Count = bytes.Length / 4;
//                 if (uint32Count == 0) return null;

//                 var result = new uint[uint32Count];

//                 // Optimized conversion using unsafe code
//                 unsafe
//                 {
//                     fixed (byte* bytePtr = bytes)
//                     fixed (uint* uintPtr = result)
//                     {
//                         Buffer.MemoryCopy(bytePtr, uintPtr, bytes.Length, bytes.Length);
//                     }
//                 }

//                 // Add to cache với size limit
//                 if (_fingerprintCache.Count < MAX_CACHE_SIZE)
//                 {
//                     _fingerprintCache.TryAdd(base64String, result);
//                 }

//                 return result;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogWarning(ex, "Failed to parse fingerprint");
//                 return null;
//             }
//         }

//         /// <summary>
//         /// Fast pre-check để skip expensive comparisons
//         /// </summary>
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         private bool ShouldSkipDetailedComparison(uint[] fp1, uint[] fp2)
//         {
//             // Skip nếu length difference quá lớn
//             float lengthRatio = (float)Math.Min(fp1.Length, fp2.Length) / Math.Max(fp1.Length, fp2.Length);
//             if (lengthRatio < 0.3f) return true;

//             // Quick hash check for obvious mismatches
//             var hash1 = CalculateQuickHash(fp1);
//             var hash2 = CalculateQuickHash(fp2);

//             // If hashes are too different, skip detailed comparison
//             uint hashXor = hash1 ^ hash2;
//             int hashDiff = CountSetBitsOptimized(hashXor);
//             return hashDiff > 20; // More than 20 bit differences in hash
//         }

//         /// <summary>
//         /// Quick hash cho pre-filtering
//         /// </summary>
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         private uint CalculateQuickHash(uint[] fingerprint)
//         {
//             uint hash = 0;
//             int step = Math.Max(1, fingerprint.Length / 8); // Sample every N elements

//             for (int i = 0; i < fingerprint.Length; i += step)
//             {
//                 hash ^= fingerprint[i];
//             }

//             return hash;
//         }

//         /// <summary>
//         /// Optimized similarity calculation
//         /// </summary>
//         private float CalculateOptimizedSimilarity(uint[] fp1, uint[] fp2)
//         {
//             // Exact match check
//             if (fp1.Length == fp2.Length && fp1.SequenceEqual(fp2))
//             {
//                 return 100f;
//             }

//             // Prioritize efficient methods
//             var methods = new (Func<uint[], uint[], float> method, float weight)[]
//             {
//             (CalculateOptimizedHamming, 0.4f),
//             (CalculateOptimizedSubsequenceMatch, 0.4f),
//             (CalculateOptimizedSlidingWindow, 0.2f)
//             };

//             float maxSimilarity = 0f;

//             foreach (var (method, weight) in methods)
//             {
//                 try
//                 {
//                     var similarity = method(fp1, fp2) * weight;
//                     maxSimilarity = Math.Max(maxSimilarity, similarity);

//                     // Early exit for high similarity
//                     if (similarity > 90f)
//                     {
//                         return Math.Min(100f, similarity);
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogWarning(ex, "Error in similarity method");
//                 }
//             }

//             return maxSimilarity;
//         }

//         /// <summary>
//         /// Vectorized Hamming distance calculation
//         /// </summary>
//         private float CalculateOptimizedHamming(uint[] fp1, uint[] fp2)
//         {
//             if (fp1.Length != fp2.Length)
//             {
//                 return CalculateAlignedHamming(fp1, fp2);
//             }

//             int matchingBits = 0;
//             int totalBits = fp1.Length * 32;

//             // Use SIMD for faster XOR và bit counting nếu available
//             if (Avx2.IsSupported && fp1.Length >= 8)
//             {
//                 matchingBits = CalculateHammingSIMD(fp1, fp2);
//             }
//             else
//             {
//                 // Fallback to optimized scalar version
//                 for (int i = 0; i < fp1.Length; i++)
//                 {
//                     uint xor = fp1[i] ^ fp2[i];
//                     matchingBits += 32 - CountSetBitsOptimized(xor);
//                 }
//             }

//             return ((float)matchingBits / totalBits) * 100f;
//         }

//         /// <summary>
//         /// SIMD-accelerated Hamming distance
//         /// </summary>
//         private unsafe int CalculateHammingSIMD(uint[] fp1, uint[] fp2)
//         {
//             int matchingBits = 0;
//             int vectorCount = fp1.Length / 8;
//             int remainder = fp1.Length % 8;

//             fixed (uint* ptr1 = fp1, ptr2 = fp2)
//             {
//                 for (int i = 0; i < vectorCount; i++)
//                 {
//                     var vec1 = Avx2.LoadVector256((uint*)(ptr1 + i * 8));
//                     var vec2 = Avx2.LoadVector256((uint*)(ptr2 + i * 8));
//                     var xorVec = Avx2.Xor(vec1, vec2);

//                     // Count bits for each element
//                     for (int j = 0; j < 8; j++)
//                     {
//                         uint xorValue = *((uint*)&xorVec + j);
//                         matchingBits += 32 - CountSetBitsOptimized(xorValue);
//                     }
//                 }

//                 // Handle remainder
//                 for (int i = vectorCount * 8; i < fp1.Length; i++)
//                 {
//                     uint xor = ptr1[i] ^ ptr2[i];
//                     matchingBits += 32 - CountSetBitsOptimized(xor);
//                 }
//             }

//             return matchingBits;
//         }

//         /// <summary>
//         /// Optimized bit counting using built-in instruction
//         /// </summary>
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         private int CountSetBitsOptimized(uint value)
//         {
//             // Use hardware bit count instruction if available
//             if (Popcnt.IsSupported)
//             {
//                 return (int)Popcnt.PopCount(value);
//             }

//             // Fallback to Brian Kernighan's algorithm
//             int count = 0;
//             while (value != 0)
//             {
//                 value &= value - 1;
//                 count++;
//             }
//             return count;
//         }

//         /// <summary>
//         /// Optimized subsequence matching với early termination
//         /// </summary>
//         private float CalculateOptimizedSubsequenceMatch(uint[] fp1, uint[] fp2)
//         {
//             var shorter = fp1.Length <= fp2.Length ? fp1 : fp2;
//             var longer = fp1.Length > fp2.Length ? fp1 : fp2;

//             if (shorter.Length == 0) return 0f;

//             const int BIT_TOLERANCE = 2;
//             float bestScore = 0f;
//             int searchLimit = Math.Min(longer.Length - shorter.Length + 1, 100); // Limit search

//             for (int i = 0; i < searchLimit; i++)
//             {
//                 int tolerantMatches = 0;
//                 bool earlyExit = false;

//                 for (int j = 0; j < shorter.Length; j++)
//                 {
//                     uint val1 = shorter[j];
//                     uint val2 = longer[i + j];

//                     if (val1 == val2)
//                     {
//                         tolerantMatches++;
//                     }
//                     else
//                     {
//                         uint xor = val1 ^ val2;
//                         int bitDiff = CountSetBitsOptimized(xor);

//                         if (bitDiff <= BIT_TOLERANCE)
//                         {
//                             tolerantMatches++;
//                         }
//                         else if (bitDiff > 10 && j < shorter.Length / 4)
//                         {
//                             // Early exit nếu quá nhiều differences ở đầu
//                             earlyExit = true;
//                             break;
//                         }
//                     }
//                 }

//                 if (!earlyExit)
//                 {
//                     float score = ((float)tolerantMatches / shorter.Length) * 100f;
//                     bestScore = Math.Max(bestScore, score);

//                     // Early termination for high scores
//                     if (score > 95f) return score;
//                 }
//             }

//             return bestScore;
//         }

//         /// <summary>
//         /// Optimized sliding window với reduced window sizes
//         /// </summary>
//         private float CalculateOptimizedSlidingWindow(uint[] fp1, uint[] fp2)
//         {
//             var shorter = fp1.Length <= fp2.Length ? fp1 : fp2;
//             var longer = fp1.Length > fp2.Length ? fp1 : fp2;

//             if (shorter.Length < 4) return 0f;

//             float maxSimilarity = 0f;

//             // Reduced window sizes cho performance
//             var windowSizes = new[] { shorter.Length, shorter.Length / 2 }
//                 .Where(s => s >= 4).ToArray();

//             foreach (int windowSize in windowSizes)
//             {
//                 int maxOffset = Math.Min(longer.Length - windowSize, 20); // Limit search range

//                 for (int i = 0; i <= maxOffset; i++)
//                 {
//                     float similarity = CalculateWindowSimilarityFast(
//                         longer, i, shorter, 0, windowSize);

//                     maxSimilarity = Math.Max(maxSimilarity, similarity);

//                     if (similarity > 85f)
//                     {
//                         return Math.Min(100f, similarity);
//                     }
//                 }
//             }

//             return maxSimilarity;
//         }

//         /// <summary>
//         /// Fast window similarity với reduced precision
//         /// </summary>
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         private float CalculateWindowSimilarityFast(uint[] arr1, int start1, uint[] arr2, int start2, int length)
//         {
//             int matches = 0;
//             const int BIT_TOLERANCE = 3;

//             for (int i = 0; i < length; i++)
//             {
//                 uint val1 = arr1[start1 + i];
//                 uint val2 = arr2[start2 + i];

//                 if (val1 == val2)
//                 {
//                     matches++;
//                 }
//                 else
//                 {
//                     uint xor = val1 ^ val2;
//                     int bitDiff = CountSetBitsOptimized(xor);
//                     if (bitDiff <= BIT_TOLERANCE)
//                     {
//                         matches++;
//                     }
//                 }
//             }

//             return ((float)matches / length) * 100f;
//         }

//         /// <summary>
//         /// Aligned Hamming cho arrays khác length
//         /// </summary>
//         private float CalculateAlignedHamming(uint[] fp1, uint[] fp2)
//         {
//             var shorter = fp1.Length <= fp2.Length ? fp1 : fp2;
//             var longer = fp1.Length > fp2.Length ? fp1 : fp2;

//             float maxSimilarity = 0f;
//             int searchLimit = Math.Min(longer.Length - shorter.Length + 1, 50);

//             for (int offset = 0; offset < searchLimit; offset++)
//             {
//                 int matchingBits = 0;
//                 int totalBits = shorter.Length * 32;

//                 for (int i = 0; i < shorter.Length; i++)
//                 {
//                     uint xor = shorter[i] ^ longer[offset + i];
//                     matchingBits += 32 - CountSetBitsOptimized(xor);
//                 }

//                 float similarity = ((float)matchingBits / totalBits) * 100f;
//                 maxSimilarity = Math.Max(maxSimilarity, similarity);

//                 if (similarity > 90f) break; // Early exit
//             }

//             return maxSimilarity;
//         }


//         public void Dispose()
//         {
//             // Cleanup if needed
//         }
//     }
// }