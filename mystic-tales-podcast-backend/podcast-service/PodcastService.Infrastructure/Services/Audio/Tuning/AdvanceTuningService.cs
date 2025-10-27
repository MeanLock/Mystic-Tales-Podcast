using System.Globalization;
using Microsoft.Extensions.Logging;
using PodcastService.Infrastructure.Helpers.AudioHelpers;
using PodcastService.Infrastructure.Models.Audio;
using PodcastService.Infrastructure.Models.Audio.Tuning;
using PodcastService.Infrastructure.Services.Audio;

namespace PodcastService.Infrastructure.Services.Audio.Tuning
{
    /// <summary>
    /// Advanced tuning service with dynamic audio format support
    /// Version 2.0 - No hardcoded formats
    /// </summary>
    public class AdvanceTuningService
    {
        private readonly EqualizerTuningService _equalizerTuningService;
        private readonly AudioFormatDetectorHelper _audioFormatDetectorHelper;
        private readonly ILogger<AdvanceTuningService>? _logger;

        public AdvanceTuningService(
            EqualizerTuningService equalizerTuningService,
            ILogger<AdvanceTuningService>? logger = null)
        {
            _equalizerTuningService = equalizerTuningService;
            _audioFormatDetectorHelper = new AudioFormatDetectorHelper(logger as ILogger<AudioFormatDetectorHelper>);
            _logger = logger;
        }

        /// <summary>
        /// Merge background audio with main audio, preserving quality based on input format
        /// </summary>
        public async Task<Stream?> MergeBackground(
            Stream fileStream,
            BackgroundMergeProfile backgroundMergeProfile,
            string? filterChain)
        {
            if (backgroundMergeProfile?.FileStream == null) return null;

            var tempDir = Path.Combine(Path.GetTempPath(), "bg_merge_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            Stream? mainWorkingStream = null;
            Stream? bgWorkingStream = null;
            AudioFormatInfo? mainFormat = null;
            AudioFormatInfo? bgFormat = null;

            try
            {
                // ✅ STEP 1: Handle non-seekable streams (S3, HTTP, etc.)
                mainWorkingStream = await PrepareStreamAsync(fileStream);
                bgWorkingStream = await PrepareStreamAsync(backgroundMergeProfile.FileStream);

                if (mainWorkingStream == null || bgWorkingStream == null)
                {
                    _logger?.LogError("Failed to prepare streams for processing");
                    return null;
                }

                // ✅ STEP 2: Detect audio formats
                mainFormat = _audioFormatDetectorHelper.DetectFormatFromStream(mainWorkingStream);
                mainWorkingStream.Position = 0;

                bgFormat = _audioFormatDetectorHelper.DetectFormatFromStream(bgWorkingStream);
                bgWorkingStream.Position = 0;

                _logger?.LogInformation($"Detected formats - Main: {mainFormat.Format}, Background: {bgFormat.Format}");

                // ✅ STEP 3: Save streams with correct extensions (DYNAMIC)
                var mainInput = Path.Combine(tempDir, $"main{mainFormat.Extension}");
                var bgInput = Path.Combine(tempDir, $"bg{bgFormat.Extension}");

                using (var fs = new FileStream(mainInput, FileMode.Create, FileAccess.Write))
                    await mainWorkingStream.CopyToAsync(fs);

                using (var fs = new FileStream(bgInput, FileMode.Create, FileAccess.Write))
                    await bgWorkingStream.CopyToAsync(fs);

                _logger?.LogDebug($"Saved temp files - Main: {mainInput}, BG: {bgInput}");

                // ✅ STEP 4: Determine output format strategy
                var outputStrategy = DetermineOutputStrategy(mainFormat, bgFormat);
                var mergedOut = Path.Combine(tempDir, $"merged{outputStrategy.Extension}");

                _logger?.LogInformation($"Output strategy: {outputStrategy.Description}");

                // Get duration of main audio
                double durationSeconds = await FFmpegCoreHelper.GetAudioDurationSecondsAsync(mainInput);
                if (durationSeconds <= 0) durationSeconds = 300; // fallback 5 min max

                var durStr = durationSeconds.ToString(CultureInfo.InvariantCulture);
                var volStr = (backgroundMergeProfile.VolumeGainDb ?? 0).ToString(CultureInfo.InvariantCulture) + "dB";

                // Build FFmpeg filter
                var filter = $"[1:a]aloop=loop=-1:size=2e9,atrim=0:{durStr},asetpts=N/SR/TB,volume={volStr}[fx];[0:a][fx]amix=inputs=2:duration=first:dropout_transition=0[m]";

                // ✅ STEP 5: Build FFmpeg command with dynamic codec
                var codecArgs = BuildCodecArguments(outputStrategy);
                var ffmpegCommand = $"-hide_banner -loglevel error -nostdin " +
                                   $"-i \"{mainInput}\" -i \"{bgInput}\" " +
                                   $"-filter_complex \"{filter}\" " +
                                   $"-map \"[m]\" {codecArgs} -y \"{mergedOut}\"";

                _logger?.LogDebug($"FFmpeg command: {ffmpegCommand}");

                // Merge background with main audio
                var success = await FFmpegCoreHelper.RunFfmpegAsync(ffmpegCommand);
                if (!success || !File.Exists(mergedOut))
                {
                    _logger?.LogError("FFmpeg merge failed");
                    return null;
                }

                string finalPath = mergedOut;

                // ✅ STEP 6: Apply filterChain if provided (EQ/mood)
                if (!string.IsNullOrWhiteSpace(filterChain))
                {
                    _logger?.LogInformation($"Applying filter chain: {filterChain}");

                    using var mergedStream = await FFmpegCoreHelper.LoadToMemory(mergedOut);
                    var filteredStream = await _equalizerTuningService.ApplyFilterChain(mergedStream, filterChain);

                    if (filteredStream != null)
                    {
                        _logger?.LogInformation("Filter chain applied successfully");
                        return filteredStream;
                    }
                    else
                    {
                        _logger?.LogWarning("Filter chain failed, returning unfiltered result");
                    }
                }

                // Return final result
                return await FFmpegCoreHelper.LoadToMemory(finalPath);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error merging background audio");
                return null;
            }
            finally
            {
                // Cleanup working streams if they're copies
                if (mainWorkingStream != null && mainWorkingStream != fileStream)
                {
                    try { mainWorkingStream.Dispose(); } catch { }
                }
                if (bgWorkingStream != null && bgWorkingStream != backgroundMergeProfile.FileStream)
                {
                    try { bgWorkingStream.Dispose(); } catch { }
                }

                // Cleanup temp directory
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }

        /// <summary>
        /// Prepare stream for processing (handle non-seekable streams)
        /// </summary>
        private async Task<Stream?> PrepareStreamAsync(Stream stream)
        {
            try
            {
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                    return stream;
                }

                // Copy non-seekable stream to MemoryStream
                _logger?.LogDebug("Non-seekable stream detected, copying to MemoryStream");
                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error preparing stream");
                return null;
            }
        }

        /// <summary>
        /// Determine optimal output format strategy based on BOTH inputs
        /// Uses "bottleneck principle" - output quality limited by lowest quality input
        /// </summary>
        private OutputFormatStrategy DetermineOutputStrategy(
            AudioFormatInfo mainFormat,
            AudioFormatInfo bgFormat)
        {
            bool mainIsLossless = mainFormat.IsLossless;
            bool bgIsLossless = bgFormat.IsLossless;

            _logger?.LogDebug($"Format analysis - Main: {mainFormat.Format} (Lossless: {mainIsLossless}), " +
                             $"BG: {bgFormat.Format} (Lossless: {bgIsLossless})");

            // ========================================
            // SCENARIO 1: Both are lossless
            // ========================================
            if (mainIsLossless && bgIsLossless)
            {
                _logger?.LogInformation("Strategy: Both inputs lossless → Output FLAC");
                return new OutputFormatStrategy
                {
                    Format = AudioFormat.FLAC,
                    Extension = ".flac",
                    Codec = "flac",
                    Bitrate = null,
                    Description = "Lossless output (both inputs lossless)"
                };
            }

            // ========================================
            // SCENARIO 2: One lossless, one lossy
            // → Output high-quality lossy (no fake upsample)
            // ========================================
            if (mainIsLossless && !bgIsLossless)
            {
                _logger?.LogInformation($"Strategy: Main lossless, BG lossy ({bgFormat.Format}) → Output high-quality lossy");

                // Use background's format family for compatibility
                if (bgFormat.Format is AudioFormat.AAC or AudioFormat.M4A)
                {
                    return new OutputFormatStrategy
                    {
                        Format = AudioFormat.AAC,
                        Extension = ".m4a",
                        Codec = "aac",
                        Bitrate = "256k",
                        Description = $"AAC 256k (main lossless + BG {bgFormat.Format})"
                    };
                }
                else
                {
                    // MP3 or others → MP3 256k
                    return new OutputFormatStrategy
                    {
                        Format = AudioFormat.MP3,
                        Extension = ".mp3",
                        Codec = "libmp3lame",
                        Bitrate = "256k",
                        Description = $"MP3 256k (main lossless + BG {bgFormat.Format})"
                    };
                }
            }

            if (!mainIsLossless && bgIsLossless)
            {
                _logger?.LogInformation($"Strategy: Main lossy ({mainFormat.Format}), BG lossless → Output high-quality lossy");

                // Use main's format family for compatibility
                if (mainFormat.Format is AudioFormat.AAC or AudioFormat.M4A)
                {
                    return new OutputFormatStrategy
                    {
                        Format = AudioFormat.AAC,
                        Extension = ".m4a",
                        Codec = "aac",
                        Bitrate = "256k",
                        Description = $"AAC 256k (main {mainFormat.Format} + BG lossless)"
                    };
                }
                else
                {
                    // MP3 or others → MP3 256k
                    return new OutputFormatStrategy
                    {
                        Format = AudioFormat.MP3,
                        Extension = ".mp3",
                        Codec = "libmp3lame",
                        Bitrate = "256k",
                        Description = $"MP3 256k (main {mainFormat.Format} + BG lossless)"
                    };
                }
            }

            // ========================================
            // SCENARIO 3: Both are lossy
            // → Choose format based on compatibility
            // ========================================
            _logger?.LogInformation($"Strategy: Both lossy (Main: {mainFormat.Format}, BG: {bgFormat.Format})");

            // Priority 1: If main is AAC/M4A → AAC
            if (mainFormat.Format is AudioFormat.AAC or AudioFormat.M4A)
            {
                return new OutputFormatStrategy
                {
                    Format = AudioFormat.AAC,
                    Extension = ".m4a",
                    Codec = "aac",
                    Bitrate = "256k",
                    Description = $"AAC 256k (main AAC/M4A + BG {bgFormat.Format})"
                };
            }

            // Priority 2: If background is AAC/M4A → AAC
            if (bgFormat.Format is AudioFormat.AAC or AudioFormat.M4A)
            {
                return new OutputFormatStrategy
                {
                    Format = AudioFormat.AAC,
                    Extension = ".m4a",
                    Codec = "aac",
                    Bitrate = "256k",
                    Description = $"AAC 256k (main {mainFormat.Format} + BG AAC/M4A)"
                };
            }

            // Priority 3: Default to MP3 (universal compatibility)
            return new OutputFormatStrategy
            {
                Format = AudioFormat.MP3,
                Extension = ".mp3",
                Codec = "libmp3lame",
                Bitrate = "256k",
                Description = $"MP3 256k (main {mainFormat.Format} + BG {bgFormat.Format})"
            };
        }

        /// <summary>
        /// Build FFmpeg codec arguments based on strategy
        /// </summary>
        private string BuildCodecArguments(OutputFormatStrategy strategy)
        {
            if (strategy.Bitrate != null)
            {
                return $"-c:a {strategy.Codec} -b:a {strategy.Bitrate}";
            }
            else
            {
                // Lossless (no bitrate)
                return $"-c:a {strategy.Codec}";
            }
        }

        /// <summary>
        /// Output format strategy
        /// </summary>
        private class OutputFormatStrategy
        {
            public AudioFormat Format { get; set; }
            public string Extension { get; set; } = string.Empty;
            public string Codec { get; set; } = string.Empty;
            public string? Bitrate { get; set; }
            public string Description { get; set; } = string.Empty;
        }
    }
}