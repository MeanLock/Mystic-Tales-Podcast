using Microsoft.Extensions.Logging;
using AcoustID;
using AcoustID.Chromaprint;
using NAudio.Wave;
using PodcastService.Infrastructure.Models.Audio;
using NAudio.MediaFoundation;
using System.Security.Cryptography;
using PodcastService.Infrastructure.Models.Audio.AcoustID;
using PodcastService.Infrastructure.Helpers.AudioHelpers;

namespace PodcastService.Infrastructure.Services.Audio.AcoustID
{
    public class AcoustIDAudioFingerprintGenerator : IDisposable
    {
        private readonly ILogger<AcoustIDAudioFingerprintGenerator> _logger;
        private readonly AudioFormatDetectorHelper _formatDetector;
        private bool _isMediaFoundationInitialized;

        public AcoustIDAudioFingerprintGenerator(ILogger<AcoustIDAudioFingerprintGenerator> logger)
        {
            _logger = logger;
            _formatDetector = new AudioFormatDetectorHelper();
            // Initialize MediaFoundation for MP3/other format support
            try
            {
                MediaFoundationApi.Startup();
                _isMediaFoundationInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize MediaFoundation. Some audio formats may not be supported.");
                _isMediaFoundationInitialized = false;
            }
        }

        private WaveStream? CreateWaveReader(Stream audioStream)
        {
            try
            {
                audioStream.Position = 0;

                // ✅ STEP 1: Detect format using magic bytes
                var formatInfo = _formatDetector.DetectFormat(audioStream);
                audioStream.Position = 0; // Reset after detection

                _logger.LogDebug($"Detected audio format: {formatInfo.Format}");

                // ✅ STEP 2: Use appropriate reader based on format
                return formatInfo.Format switch
                {
                    AudioFormat.MP3 => CreateMp3Reader(audioStream),
                    AudioFormat.WAV => CreateWavReader(audioStream),
                    AudioFormat.FLAC => CreateMediaFoundationReader(audioStream),
                    AudioFormat.M4A => CreateMediaFoundationReader(audioStream),
                    AudioFormat.AAC => CreateMediaFoundationReader(audioStream),
                    _ => TryAllReaders(audioStream) // Fallback for unknown
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating wave reader");
                return null;
            }
        }

        private WaveStream? CreateMp3Reader(Stream audioStream)
        {
            try
            {
                return new Mp3FileReader(audioStream);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create MP3 reader");
                return null;
            }
        }

        private WaveStream? CreateWavReader(Stream audioStream)
        {
            try
            {
                return new WaveFileReader(audioStream);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create WAV reader");
                return null;
            }
        }

        private WaveStream? CreateMediaFoundationReader(Stream audioStream)
        {
            if (!_isMediaFoundationInitialized)
            {
                _logger.LogWarning("MediaFoundation not initialized");
                return null;
            }

            try
            {
                return new StreamMediaFoundationReader(audioStream);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create MediaFoundation reader");
                return null;
            }
        }

        // ✅ Fallback: Try all readers (existing logic)
        private WaveStream? TryAllReaders(Stream audioStream)
        {
            _logger.LogWarning("Unknown format, trying all readers...");

            // Try MP3
            try
            {
                audioStream.Position = 0;
                return new Mp3FileReader(audioStream);
            }
            catch { }

            // Try WAV
            try
            {
                audioStream.Position = 0;
                return new WaveFileReader(audioStream);
            }
            catch { }

            // Try MediaFoundation
            if (_isMediaFoundationInitialized)
            {
                try
                {
                    audioStream.Position = 0;
                    return new StreamMediaFoundationReader(audioStream);
                }
                catch { }
            }

            return null;
        }

        public async Task<AcoustIDAudioFingerprintGeneratedResult> GenerateFingerprintAsync(Stream audioStream)
        {
            var result = new AcoustIDAudioFingerprintGeneratedResult
            {
                Algorithm = "Chromaprint",
                GeneratedAt = DateTime.UtcNow
            };

            Stream? workingStream = null;
            try
            {
                if (audioStream == null)
                {
                    _logger.LogError("Audio stream is null");
                    return result;
                }

                // Handle streams that don't support Length or Position (like HttpBaseStream)
                workingStream = await PrepareStreamForProcessingAsync(audioStream);
                if (workingStream == null)
                {
                    _logger.LogError("Failed to prepare stream for processing");
                    return result;
                }

                // Get file size if available
                try
                {
                    if (workingStream.CanSeek)
                    {
                        var fileSizeBytes = workingStream.Length;
                        result.FileSizeMB = Math.Round(fileSizeBytes / (1024.0 * 1024.0), 2); // Convert bytes to MB với 2 chữ số thập phân
                    }
                }
                catch (NotSupportedException)
                {
                    _logger.LogDebug("Stream does not support Length property");
                    result.FileSizeMB = 0; // Set to 0 when Length is not available
                }

                // Extract audio data using NAudio
                var audioData = await ExtractAudioDataAsync(workingStream);
                if (audioData == null)
                {
                    _logger.LogError("Failed to extract audio data from stream");
                    return result;
                }

                // Set audio metadata
                result.Duration = audioData.Duration;
                result.SampleRate = audioData.SampleRate;
                result.Channels = audioData.Channels;

                // Generate fingerprint using AcoustID.NET
                var fingerprintData = await GenerateAcoustIDFingerprintAsync(audioData);
                if (!string.IsNullOrEmpty(fingerprintData))
                {
                    result.FingerprintData = fingerprintData;
                    result.FingerprintHash = GenerateHash(System.Text.Encoding.UTF8.GetBytes(fingerprintData));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating audio fingerprint");
                return result;
            }
            finally
            {
                // Clean up working stream if it's different from original
                if (workingStream != null && workingStream != audioStream)
                {
                    workingStream.Dispose();
                }
            }
        }

        private async Task<Stream?> PrepareStreamForProcessingAsync(Stream originalStream)
        {
            try
            {
                // If stream supports seeking and has accessible Length, use it directly
                if (originalStream.CanSeek)
                {
                    try
                    {
                        // Test if Length is accessible
                        var _ = originalStream.Length;
                        originalStream.Position = 0;
                        return originalStream;
                    }
                    catch (NotSupportedException)
                    {
                        // Length not supported, but CanSeek is true, still try to use it
                        originalStream.Position = 0;
                        return originalStream;
                    }
                }

                // For streams that don't support seeking (like HttpBaseStream), copy to MemoryStream
                _logger.LogDebug("Stream doesn't support seeking, copying to MemoryStream");
                var memoryStream = new MemoryStream();
                await originalStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing stream for processing");
                return null;
            }
        }

        private async Task<AcoustIDAudioData?> ExtractAudioDataAsync(Stream audioStream)
        {
            try
            {
                // Create appropriate reader based on stream content
                using WaveStream? reader = CreateWaveReader(audioStream);
                if (reader == null)
                {
                    _logger.LogError("Unable to create wave reader for the audio stream");
                    return null;
                }

                // Convert to standard format for Chromaprint (11025Hz, 16-bit, Mono)
                // AcoustID.NET works best with lower sample rates
                var targetFormat = new WaveFormat(11025, 16, 1);
                using var resampler = new MediaFoundationResampler(reader, targetFormat);

                // Read all audio data
                var samples = await ReadAllSamplesAsync(resampler);

                return new AcoustIDAudioData
                {
                    Samples = samples,
                    SampleRate = targetFormat.SampleRate,
                    Channels = targetFormat.Channels,
                    Duration = reader.TotalTime.TotalSeconds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting audio data");
                return null;
            }
        }

        // private WaveStream? CreateWaveReader(Stream audioStream)
        // {
        //     try
        //     {
        //         // Try different readers based on stream content
        //         audioStream.Position = 0;

        //         // Try MP3 first (most common)
        //         try
        //         {
        //             return new Mp3FileReader(audioStream);
        //         }
        //         catch
        //         {
        //             audioStream.Position = 0;
        //         }

        //         // Try WAV
        //         try
        //         {
        //             return new WaveFileReader(audioStream);
        //         }
        //         catch
        //         {
        //             audioStream.Position = 0;
        //         }

        //         // Try MediaFoundation reader (supports multiple formats)
        //         if (_isMediaFoundationInitialized)
        //         {
        //             try
        //             {
        //                 return new StreamMediaFoundationReader(audioStream);
        //             }
        //             catch
        //             {
        //                 audioStream.Position = 0;
        //             }
        //         }

        //         return null;
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error creating wave reader");
        //         return null;
        //     }
        // }

        private async Task<short[]> ReadAllSamplesAsync(IWaveProvider waveProvider)
        {
            var samples = new List<short>();
            var buffer = new byte[4096];
            int bytesRead;

            await Task.Run(() =>
            {
                while ((bytesRead = waveProvider.Read(buffer, 0, buffer.Length)) > 0)
                {
                    // Convert bytes to 16-bit samples
                    for (int i = 0; i < bytesRead; i += 2)
                    {
                        if (i + 1 < bytesRead)
                        {
                            short sample = (short)(buffer[i] | (buffer[i + 1] << 8));
                            samples.Add(sample);
                        }
                    }
                }
            });

            return samples.ToArray();
        }

        private async Task<string> GenerateAcoustIDFingerprintAsync(AcoustIDAudioData audioData)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Create ChromaContext using AcoustID.NET - không dùng using vì không implement IDisposable
                    var context = new ChromaContext();

                    // Start fingerprinting with audio parameters
                    context.Start(audioData.SampleRate, audioData.Channels);

                    // AcoustID.NET expects short[] data, không cần convert sang float
                    context.Feed(audioData.Samples, audioData.Samples.Length);

                    // Finish fingerprinting
                    context.Finish();

                    // Get fingerprint as string - trả về trực tiếp string
                    // var fingerprintString = context.GetFingerprint();
                    int[] raw = context.GetRawFingerprint();
                    // hoặc nếu là byte[] rawBytes = ...
                    byte[] rawBytes = raw.SelectMany(i => BitConverter.GetBytes(i)).ToArray();

                    var fingerprintString = Convert.ToBase64String(rawBytes);
                    if (string.IsNullOrEmpty(fingerprintString))
                    {
                        _logger.LogWarning("Empty fingerprint returned from ChromaContext");
                        return string.Empty;
                    }

                    // Trả về trực tiếp fingerprint string
                    _logger.LogDebug($"Generated fingerprint: {fingerprintString.Substring(0, Math.Min(50, fingerprintString.Length))}...");
                    return fingerprintString;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating AcoustID fingerprint");
                    return string.Empty;
                }
            });
        }

        private byte[] ParseFingerprintString(string fingerprintString)
        {
            try
            {
                // Remove any whitespace and handle base64-like encoding
                var cleanString = fingerprintString.Trim();

                // If it starts with "AQAA", it's likely base64 encoded
                if (cleanString.StartsWith("AQAA"))
                {
                    try
                    {
                        return Convert.FromBase64String(cleanString);
                    }
                    catch
                    {
                        // Fallback to UTF8 bytes if base64 decode fails
                        return System.Text.Encoding.UTF8.GetBytes(cleanString);
                    }
                }

                // Fallback: convert string to bytes
                return System.Text.Encoding.UTF8.GetBytes(cleanString);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse fingerprint string, using UTF8 encoding");
                return System.Text.Encoding.UTF8.GetBytes(fingerprintString);
            }
        }

        private string GenerateHash(byte[] data)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(data);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
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
                if (string.IsNullOrEmpty(comparison.Target?.AudioFingerPrint))
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

                        // Tính toán tỷ lệ tương đồng
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
                        // Thêm kết quả với 0% similarity cho candidate lỗi
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
        /// Tính toán tỷ lệ tương đồng giữa hai audio fingerprints
        /// </summary>
        /// <param name="fingerprint1">Fingerprint đầu tiên</param>
        /// <param name="fingerprint2">Fingerprint thứ hai</param>
        /// <returns>Tỷ lệ tương đồng từ 0.0 đến 100.0</returns>
        private float CalculateFingerprintSimilarity(string fingerprint1, string fingerprint2)
        {
            try
            {
                // Nếu một trong hai fingerprint rỗng
                if (string.IsNullOrEmpty(fingerprint1) || string.IsNullOrEmpty(fingerprint2))
                {
                    return 0f;
                }

                // Nếu hai fingerprint giống hệt nhau
                if (fingerprint1.Equals(fingerprint2, StringComparison.Ordinal))
                {
                    return 100f;
                }

                // Convert string sang byte array để tính toán
                var bytes1 = System.Text.Encoding.UTF8.GetBytes(fingerprint1);
                var bytes2 = System.Text.Encoding.UTF8.GetBytes(fingerprint2);

                // Tính Hamming distance cho các fingerprint có cùng độ dài
                if (bytes1.Length == bytes2.Length)
                {
                    return CalculateHammingDistanceSimilarity(bytes1, bytes2);
                }

                // Nếu độ dài khác nhau, sử dụng string similarity
                return CalculateStringSimilarity(fingerprint1, fingerprint2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating fingerprint similarity");
                return 0f;
            }
        }

        /// <summary>
        /// Tính tỷ lệ tương đồng dựa trên Hamming distance (cho fingerprints cùng độ dài)
        /// </summary>
        private float CalculateHammingDistanceSimilarity(byte[] fingerprint1, byte[] fingerprint2)
        {
            int differences = 0;
            int totalBits = fingerprint1.Length * 8; // Tổng số bits

            for (int i = 0; i < fingerprint1.Length; i++)
            {
                // XOR để tìm các bit khác nhau
                byte xorResult = (byte)(fingerprint1[i] ^ fingerprint2[i]);

                // Đếm số bit khác nhau trong byte
                while (xorResult != 0)
                {
                    differences += xorResult & 1;
                    xorResult >>= 1;
                }
            }

            // Tính tỷ lệ tương đồng: (tổng bits - bits khác nhau) / tổng bits * 100
            float similarity = ((float)(totalBits - differences) / totalBits) * 100f;
            return Math.Max(0f, Math.Min(100f, similarity)); // Đảm bảo trong khoảng [0, 100]
        }

        /// <summary>
        /// Tính tỷ lệ tương đồng dựa trên Jaccard similarity (cho fingerprints khác độ dài)
        /// </summary>
        private float CalculateJaccardSimilarity(byte[] fingerprint1, byte[] fingerprint2)
        {
            // Convert byte arrays to sets of bytes
            var set1 = new HashSet<byte>(fingerprint1);
            var set2 = new HashSet<byte>(fingerprint2);

            // Tính intersection và union
            var intersection = set1.Intersect(set2).Count();
            var union = set1.Union(set2).Count();

            if (union == 0)
            {
                return 0f;
            }

            // Jaccard similarity = |intersection| / |union| * 100
            return ((float)intersection / union) * 100f;
        }

        /// <summary>
        /// Tính tỷ lệ tương đồng dựa trên string similarity (cho fingerprints string khác độ dài)
        /// </summary>
        private float CalculateStringSimilarity(string fingerprint1, string fingerprint2)
        {
            // Sử dụng Levenshtein distance để tính tương đồng
            int distance = CalculateLevenshteinDistance(fingerprint1, fingerprint2);
            int maxLength = Math.Max(fingerprint1.Length, fingerprint2.Length);

            if (maxLength == 0)
            {
                return 100f; // Cả hai string đều rỗng
            }

            // Similarity = (1 - distance/maxLength) * 100
            float similarity = (1f - (float)distance / maxLength) * 100f;
            return Math.Max(0f, Math.Min(100f, similarity));
        }

        /// <summary>
        /// Tính Levenshtein distance giữa hai string
        /// </summary>
        private int CalculateLevenshteinDistance(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1))
                return s2?.Length ?? 0;

            if (string.IsNullOrEmpty(s2))
                return s1.Length;

            int[,] matrix = new int[s1.Length + 1, s2.Length + 1];

            // Initialize first row and column
            for (int i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;

            // Fill matrix
            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost
                    );
                }
            }

            return matrix[s1.Length, s2.Length];
        }

        public void Dispose()
        {
            if (_isMediaFoundationInitialized)
            {
                try
                {
                    MediaFoundationApi.Shutdown();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error shutting down MediaFoundation");
                }
            }
        }

    }

}
