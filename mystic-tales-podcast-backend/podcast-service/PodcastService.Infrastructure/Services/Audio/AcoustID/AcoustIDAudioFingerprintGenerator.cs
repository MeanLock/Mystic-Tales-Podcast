using Microsoft.Extensions.Logging;
using AcoustID;
using AcoustID.Chromaprint;
using NAudio.Wave;
using PodcastService.Infrastructure.Models.Audio;
using NAudio.MediaFoundation;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using PodcastService.Infrastructure.Models.Audio.AcoustID;
using PodcastService.Infrastructure.Helpers.AudioHelpers;

namespace PodcastService.Infrastructure.Services.Audio.AcoustID
{
    public class AcoustIDAudioFingerprintGenerator : IDisposable
    {
        private readonly ILogger<AcoustIDAudioFingerprintGenerator> _logger;
        private readonly AudioFormatDetectorHelper _audioFormatDetectorHelper;
        private bool _isMediaFoundationInitialized;
        private readonly bool _isWindows;

        public AcoustIDAudioFingerprintGenerator(ILogger<AcoustIDAudioFingerprintGenerator> logger)
        {
            _logger = logger;
            _audioFormatDetectorHelper = new AudioFormatDetectorHelper(logger as ILogger<AudioFormatDetectorHelper>);

            // ✅ Detect OS
            _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            // ✅ Initialize MediaFoundation ONLY on Windows
            if (_isWindows)
            {
                try
                {
                    MediaFoundationApi.Startup();
                    _isMediaFoundationInitialized = true;
                    _logger.LogInformation("MediaFoundation initialized successfully (Windows)");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to initialize MediaFoundation on Windows. Some audio formats may not be supported.");
                    _isMediaFoundationInitialized = false;
                }
            }
            else
            {
                _logger.LogInformation($"Running on {RuntimeInformation.OSDescription}. MediaFoundation is not available (Windows-only). Supported formats: MP3, WAV");
                _isMediaFoundationInitialized = false;
            }
        }

        /// <summary>
        /// Create appropriate wave reader based on audio format with platform awareness
        /// </summary>
        // private WaveStream? CreateWaveReader(Stream audioStream)
        // {
        //     try
        //     {
        //         audioStream.Position = 0;

        //         // STEP 1: Detect format using magic bytes
        //         var formatInfo = _audioFormatDetectorHelper.DetectFormatFromStream(audioStream);
        //         audioStream.Position = 0; // Reset after detection

        //         _logger.LogDebug($"Detected audio format: {formatInfo.Format}");

        //         // STEP 2: Use appropriate reader based on format and platform
        //         return formatInfo.Format switch
        //         {
        //             AudioFormat.MP3 => CreateMp3Reader(audioStream),
        //             AudioFormat.WAV => CreateWavReader(audioStream),
        //             AudioFormat.FLAC => CreateMediaFoundationReader(audioStream),  // Windows only
        //             AudioFormat.M4A => CreateMediaFoundationReader(audioStream),   // Windows only
        //             AudioFormat.AAC => CreateMediaFoundationReader(audioStream),   // Windows only
        //             _ => TryAllReaders(audioStream) // Fallback for unknown formats
        //         };
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error creating wave reader");
        //         return null;
        //     }
        // }

        private WaveStream? CreateWaveReader(Stream audioStream)
        {
            try
            {
                audioStream.Position = 0;

                // Detect format using magic bytes
                var formatInfo = _audioFormatDetectorHelper.DetectFormatFromStream(audioStream);
                audioStream.Position = 0;

                _logger.LogDebug($"Detected audio format: {formatInfo.Format}");

                // On Linux, convert everything to WAV using FFmpeg
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (formatInfo.Format == AudioFormat.WAV)
                    {
                        // WAV can be read directly
                        return new WaveFileReader(audioStream);
                    }
                    else
                    {
                        // Convert to WAV using FFmpeg
                        _logger.LogInformation($"Converting {formatInfo.Format} to WAV using FFmpeg (Linux)");
                        return ConvertToWavUsingFFmpeg(audioStream, formatInfo);
                    }
                }

                // Windows - use native readers
                return formatInfo.Format switch
                {
                    AudioFormat.MP3 => CreateMp3Reader(audioStream),
                    AudioFormat.WAV => CreateWavReader(audioStream),
                    AudioFormat.FLAC => CreateMediaFoundationReader(audioStream),
                    AudioFormat.M4A => CreateMediaFoundationReader(audioStream),
                    AudioFormat.AAC => CreateMediaFoundationReader(audioStream),
                    _ => TryAllReaders(audioStream)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating wave reader");
                return null;
            }
        }

        private WaveStream? ConvertToWavUsingFFmpeg(Stream audioStream, AudioFormatInfo formatInfo)
        {
            string? tempDir = null;

            try
            {
                tempDir = Path.Combine(Path.GetTempPath(), $"acoustid_{Guid.NewGuid():N}");
                Directory.CreateDirectory(tempDir);

                var inputFile = Path.Combine(tempDir, $"input{formatInfo.Extension}");
                var outputFile = Path.Combine(tempDir, "output.wav");

                // Write input stream to temp file
                using (var fs = new FileStream(inputFile, FileMode.Create, FileAccess.Write))
                {
                    audioStream.CopyTo(fs);
                }

                // ✅ CRITICAL: Don't force sample rate or channels
                // Let FFmpeg preserve the original audio properties
                var ffmpegArgs = $"-i \"{inputFile}\" -acodec pcm_s16le -y \"{outputFile}\"";

                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = ffmpegArgs,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                _logger.LogDebug($"Converting to WAV (preserving original format): {ffmpegArgs}");

                process.Start();
                var stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    _logger.LogError($"FFmpeg conversion failed (exit {process.ExitCode}): {stderr}");
                    return null;
                }

                if (!File.Exists(outputFile))
                {
                    _logger.LogError("FFmpeg completed but output file not found");
                    return null;
                }

                // Open WAV file
                var wavFileStream = new FileStream(outputFile, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
                return new WaveFileReader(wavFileStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting audio to WAV using FFmpeg");

                if (tempDir != null && Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, true); } catch { }
                }

                return null;
            }
        }
        /// <summary>
        /// Create MP3 reader (cross-platform)
        /// </summary>
        private WaveStream? CreateMp3Reader(Stream audioStream)
        {
            try
            {
                var reader = new Mp3FileReader(audioStream);
                _logger.LogDebug("Successfully created Mp3FileReader");
                return reader;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create MP3 reader");
                return null;
            }
        }

        /// <summary>
        /// Create WAV reader (cross-platform)
        /// </summary>
        private WaveStream? CreateWavReader(Stream audioStream)
        {
            try
            {
                var reader = new WaveFileReader(audioStream);
                _logger.LogDebug("Successfully created WaveFileReader");
                return reader;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create WAV reader");
                return null;
            }
        }

        /// <summary>
        /// Create MediaFoundation reader (Windows only)
        /// </summary>
        private WaveStream? CreateMediaFoundationReader(Stream audioStream)
        {
            // Platform check
            if (!_isWindows)
            {
                _logger.LogWarning($"MediaFoundation reader requested but not available on {RuntimeInformation.OSDescription}. Format not supported.");
                return null;
            }

            if (!_isMediaFoundationInitialized)
            {
                _logger.LogWarning("MediaFoundation not initialized on Windows");
                return null;
            }

            try
            {
                var reader = new StreamMediaFoundationReader(audioStream);
                _logger.LogDebug("Successfully created MediaFoundationReader (Windows)");
                return reader;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create MediaFoundation reader");
                return null;
            }
        }

        /// <summary>
        /// Fallback: Try all available readers based on platform
        /// </summary>
        private WaveStream? TryAllReaders(Stream audioStream)
        {
            _logger.LogWarning("Unknown format, trying all available readers...");

            // Try MP3 (cross-platform)
            try
            {
                audioStream.Position = 0;
                var reader = new Mp3FileReader(audioStream);
                _logger.LogInformation("Successfully opened with Mp3FileReader");
                return reader;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Mp3FileReader failed");
            }

            // Try WAV (cross-platform)
            try
            {
                audioStream.Position = 0;
                var reader = new WaveFileReader(audioStream);
                _logger.LogInformation("Successfully opened with WaveFileReader");
                return reader;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "WaveFileReader failed");
            }

            // Try MediaFoundation (Windows only)
            if (_isWindows && _isMediaFoundationInitialized)
            {
                try
                {
                    audioStream.Position = 0;
                    var reader = new StreamMediaFoundationReader(audioStream);
                    _logger.LogInformation("Successfully opened with MediaFoundationReader (Windows)");
                    return reader;
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "MediaFoundationReader failed");
                }
            }

            // Log supported formats based on platform
            var supportedFormats = _isWindows
                ? "MP3, WAV, FLAC, M4A, AAC (MediaFoundation)"
                : "MP3, WAV";

            _logger.LogError($"All available readers failed. Supported formats on this platform: {supportedFormats}");
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
                        result.FileSizeMB = Math.Round(fileSizeBytes / (1024.0 * 1024.0), 2);
                    }
                }
                catch (NotSupportedException)
                {
                    _logger.LogDebug("Stream does not support Length property");
                    result.FileSizeMB = 0;
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

        /// <summary>
        /// Extract audio data with platform-aware resampling
        /// </summary>
        // private async Task<AcoustIDAudioData?> ExtractAudioDataAsync(Stream audioStream)
        // {
        //     WaveStream? reader = null;
        //     WaveStream? resampler = null;

        //     try
        //     {
        //         // Create appropriate reader based on stream content
        //         reader = CreateWaveReader(audioStream);
        //         if (reader == null)
        //         {
        //             _logger.LogError("Unable to create wave reader for the audio stream");
        //             return null;
        //         }

        //         // Convert to standard format for Chromaprint (11025Hz, 16-bit, Mono)
        //         // AcoustID.NET works best with lower sample rates
        //         var targetFormat = new WaveFormat(11025, 16, 1);

        //         // ✅ Platform-aware resampling
        //         if (_isWindows && _isMediaFoundationInitialized)
        //         {
        //             // Windows: Use MediaFoundationResampler (best quality)
        //             try
        //             {
        //                 resampler = new MediaFoundationResampler(reader, targetFormat);
        //                 _logger.LogDebug($"Using MediaFoundationResampler: {reader.WaveFormat.SampleRate}Hz → {targetFormat.SampleRate}Hz");
        //             }
        //             catch (Exception ex)
        //             {
        //                 _logger.LogWarning(ex, "MediaFoundationResampler failed, using fallback");
        //                 resampler = TryFallbackResampling(reader, targetFormat);
        //             }
        //         }
        //         else
        //         {
        //             // Linux: Use fallback resampling
        //             resampler = TryFallbackResampling(reader, targetFormat);
        //         }

        //         // If resampler is null, use original reader
        //         var sourceStream = resampler ?? reader;

        //         // Read all audio data
        //         var samples = await ReadAllSamplesAsync(sourceStream);

        //         var audioData = new AcoustIDAudioData
        //         {
        //             Samples = samples,
        //             SampleRate = sourceStream.WaveFormat.SampleRate,
        //             Channels = sourceStream.WaveFormat.Channels,
        //             Duration = reader.TotalTime.TotalSeconds
        //         };

        //         return audioData;
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error extracting audio data");
        //         return null;
        //     }
        //     finally
        //     {
        //         // Dispose resampler if it's different from reader
        //         if (resampler != null && resampler != reader)
        //         {
        //             resampler.Dispose();
        //         }

        //         // Dispose reader
        //         reader?.Dispose();
        //     }
        // }

        private async Task<AcoustIDAudioData?> ExtractAudioDataAsync(Stream audioStream)
        {
            try
            {
                using WaveStream? reader = CreateWaveReader(audioStream);
                if (reader == null)
                {
                    _logger.LogError("Unable to create wave reader for the audio stream");
                    return null;
                }

                var targetFormat = new WaveFormat(11025, 16, 1);

                // ✅ Dùng IWaveProvider và using
                IWaveProvider sourceStream;

                if (_isWindows && _isMediaFoundationInitialized)
                {
                    using var resampler = new MediaFoundationResampler(reader, targetFormat);
                    sourceStream = resampler;
                    var samples = await ReadAllSamplesAsync(sourceStream);

                    return new AcoustIDAudioData
                    {
                        Samples = samples,
                        SampleRate = sourceStream.WaveFormat.SampleRate,
                        Channels = sourceStream.WaveFormat.Channels,
                        Duration = reader.TotalTime.TotalSeconds
                    };
                }
                else
                {
                    sourceStream = TryFallbackResampling(reader, targetFormat) ?? reader;
                    var samples = await ReadAllSamplesAsync(sourceStream);

                    return new AcoustIDAudioData
                    {
                        Samples = samples,
                        SampleRate = sourceStream.WaveFormat.SampleRate,
                        Channels = sourceStream.WaveFormat.Channels,
                        Duration = reader.TotalTime.TotalSeconds
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting audio data");
                return null;
            }
        }

        /// <summary>
        /// Try fallback resampling for Linux or when MediaFoundation fails
        /// </summary>
        private WaveStream? TryFallbackResampling(WaveStream reader, WaveFormat targetFormat)
        {
            // Check if conversion is needed
            if (reader.WaveFormat.SampleRate == targetFormat.SampleRate &&
                reader.WaveFormat.Channels == targetFormat.Channels &&
                reader.WaveFormat.BitsPerSample == targetFormat.BitsPerSample)
            {
                _logger.LogDebug("No resampling needed - format already matches target");
                return reader; // No conversion needed
            }

            // Try WaveFormatConversionStream
            try
            {
                var converted = new WaveFormatConversionStream(targetFormat, reader);
                _logger.LogDebug($"Using WaveFormatConversionStream (fallback): {reader.WaveFormat.SampleRate}Hz → {targetFormat.SampleRate}Hz");
                return converted;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"WaveFormatConversionStream not supported. Using original format: {reader.WaveFormat.SampleRate}Hz, {reader.WaveFormat.Channels}ch, {reader.WaveFormat.BitsPerSample}bit");
                return reader; // Use original format
            }
        }

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
                    // Create ChromaContext using AcoustID.NET
                    var context = new ChromaContext();

                    // Start fingerprinting with audio parameters
                    context.Start(audioData.SampleRate, audioData.Channels);

                    // Feed audio data
                    context.Feed(audioData.Samples, audioData.Samples.Length);

                    // Finish fingerprinting
                    context.Finish();

                    // Get raw fingerprint and convert to Base64
                    int[] raw = context.GetRawFingerprint();
                    byte[] rawBytes = raw.SelectMany(i => BitConverter.GetBytes(i)).ToArray();
                    var fingerprintString = Convert.ToBase64String(rawBytes);

                    if (string.IsNullOrEmpty(fingerprintString))
                    {
                        _logger.LogWarning("Empty fingerprint returned from ChromaContext");
                        return string.Empty;
                    }

                    _logger.LogDebug($"Generated fingerprint: {fingerprintString.Substring(0, Math.Min(50, fingerprintString.Length))}... (length: {fingerprintString.Length})");
                    return fingerprintString;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating AcoustID fingerprint");
                    return string.Empty;
                }
            });
        }

        private string GenerateHash(byte[] data)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(data);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        public void Dispose()
        {
            // Only shutdown MediaFoundation if it was initialized (Windows only)
            if (_isWindows && _isMediaFoundationInitialized)
            {
                try
                {
                    MediaFoundationApi.Shutdown();
                    _logger.LogInformation("MediaFoundation shutdown successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error shutting down MediaFoundation");
                }
            }
        }
    }
}