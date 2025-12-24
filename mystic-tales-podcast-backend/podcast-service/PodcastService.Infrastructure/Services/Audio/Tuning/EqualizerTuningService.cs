using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;
using PodcastService.Infrastructure.Configurations.Audio.Tuning;
using PodcastService.Infrastructure.Helpers.AudioHelpers;
using PodcastService.Infrastructure.Models.Audio;
using PodcastService.Infrastructure.Models.Audio.Tuning;
using PodcastService.Infrastructure.Services.Audio;

namespace PodcastService.Infrastructure.Services.Audio.Tuning
{
    public class EqualizerTuningService
    {
        private readonly ILogger<EqualizerTuningService>? _logger;
        private readonly IMoodConfig _moodConfig;
        private readonly IEqualizerConfig _equalizerConfig;
        private readonly AudioFormatDetectorHelper _audioFormatDetectorHelper; // ← NEW
        private readonly IWebHostEnvironment _environment;
        private readonly IFilePathConfig _filePathConfig;

        public EqualizerTuningService(
            ILogger<EqualizerTuningService> logger,
            IMoodConfig moodConfig,
            IEqualizerConfig equalizerConfig,
            IWebHostEnvironment environment,
            IFilePathConfig filePathConfig)
        {
            _logger = logger;
            _moodConfig = moodConfig;
            _equalizerConfig = equalizerConfig;
            _audioFormatDetectorHelper = new AudioFormatDetectorHelper(logger as ILogger<AudioFormatDetectorHelper>); // ← Initialize
            _environment = environment;
            _filePathConfig = filePathConfig;
        }

        #region CreateFilterChain
        public string CreateFilterChain(EqualizerProfile? equalizerProfile)
        {
            if (equalizerProfile == null)
                return string.Empty;

            var baseEqualizer = equalizerProfile.BaseEqualizer ?? new BaseEqualizerProfile();

            BaseEqualizerProfile moodEqualizer = new();
            string moodFilters = string.Empty;

            var moodEnum = equalizerProfile.ExpandEqualizer?.Mood;
            if (moodEnum.HasValue)
            {
                var moodData = _moodConfig.Get(moodEnum.Value);
                if (moodData.HasValue)
                {
                    moodEqualizer = moodData.Value.Eq;
                    moodFilters = moodData.Value.Filters;
                }
            }

            var merged = AddBands(baseEqualizer, moodEqualizer);

            var parts = new List<string>();
            if (HasAnyEq(merged))
                parts.Add(BuildEqFilter(merged));

            if (!string.IsNullOrWhiteSpace(moodFilters))
                parts.Add(moodFilters);

            return string.Join(",", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        private static BaseEqualizerProfile AddBands(BaseEqualizerProfile baseEqualizerProfile, BaseEqualizerProfile moodEqualizer) => new BaseEqualizerProfile
        {
            SubBass = (baseEqualizerProfile.SubBass ?? 0) + (moodEqualizer.SubBass ?? 0),
            Bass = (baseEqualizerProfile.Bass ?? 0) + (moodEqualizer.Bass ?? 0),
            Low = (baseEqualizerProfile.Low ?? 0) + (moodEqualizer.Low ?? 0),
            LowMid = (baseEqualizerProfile.LowMid ?? 0) + (moodEqualizer.LowMid ?? 0),
            Mid = (baseEqualizerProfile.Mid ?? 0) + (moodEqualizer.Mid ?? 0),
            Presence = (baseEqualizerProfile.Presence ?? 0) + (moodEqualizer.Presence ?? 0),
            HighMid = (baseEqualizerProfile.HighMid ?? 0) + (moodEqualizer.HighMid ?? 0),
            Treble = (baseEqualizerProfile.Treble ?? 0) + (moodEqualizer.Treble ?? 0),
            Air = (baseEqualizerProfile.Air ?? 0) + (moodEqualizer.Air ?? 0)
        };

        private static bool HasAnyEq(BaseEqualizerProfile b) =>
            !((b.SubBass ?? 0) == 0 && (b.Bass ?? 0) == 0 && (b.Low ?? 0) == 0 && (b.LowMid ?? 0) == 0 &&
              (b.Mid ?? 0) == 0 && (b.Presence ?? 0) == 0 && (b.HighMid ?? 0) == 0 && (b.Treble ?? 0) == 0 && (b.Air ?? 0) == 0);

        private static string BuildEqFilter(BaseEqualizerProfile b) =>
            "firequalizer=gain_entry='" +
            $"entry(32,{b.SubBass ?? 0});" +
            $"entry(64,{b.Bass ?? 0});" +
            $"entry(125,{b.Low ?? 0});" +
            $"entry(250,{b.LowMid ?? 0});" +
            $"entry(1000,{b.Mid ?? 0});" +
            $"entry(2000,{b.Presence ?? 0});" +
            $"entry(4000,{b.HighMid ?? 0});" +
            $"entry(8000,{b.Treble ?? 0});" +
            $"entry(16000,{b.Air ?? 0})'";
        #endregion

        #region ApplyFilterChain
        public async Task<Stream?> ApplyFilterChain(Stream fileStream, string filterChain)
        {
            if (string.IsNullOrWhiteSpace(filterChain))
            {
                var direct = new MemoryStream();
                await fileStream.CopyToAsync(direct);
                direct.Position = 0;
                return direct;
            }

            // ✅ STEP 1: Handle non-seekable streams
            Stream processStream;
            if (!fileStream.CanSeek)
            {
                Console.WriteLine("[DEBUG] Non-seekable stream, copying to MemoryStream");
                var memStream = new MemoryStream();
                await fileStream.CopyToAsync(memStream);
                memStream.Position = 0;
                processStream = memStream;
            }
            else
            {
                processStream = fileStream;
                processStream.Position = 0;
            }

            // ✅ STEP 2: Detect audio format
            var formatInfo = _audioFormatDetectorHelper.DetectFormatFromStream(processStream);
            processStream.Position = 0;

            Console.WriteLine($"[DEBUG] Detected format: {formatInfo.Format} ({formatInfo.Extension})");
            Console.WriteLine($"[DEBUG] Lossless: {formatInfo.IsLossless}, Codec: {formatInfo.FfmpegCodec}");

            // ✅ STEP 3: Determine encoding strategy
            var encodingStrategy = DetermineEncodingStrategy(formatInfo);
            Console.WriteLine($"[DEBUG] Encoding strategy: {encodingStrategy.Description}");

            // string workDir = Path.Combine(Path.GetTempPath(), "eqseg_" + Guid.NewGuid().ToString("N"));
            string workDir = Path.Combine(_environment.ContentRootPath, _filePathConfig.AUDIO_TUNING_EQUALIZER_LOCAL_TEMP_FILE_PATH, "eqseg_" + Guid.NewGuid().ToString("N"));
            Console.WriteLine($"[DEBUG] WorkDir: {workDir}");
            Directory.CreateDirectory(workDir);

            // ✅ STEP 4: Use dynamic extension
            string inputPath = Path.Combine(workDir, $"input{formatInfo.Extension}");

            try
            {
                // Ghi stream vào file tạm
                using (var fs = new FileStream(inputPath, FileMode.Create, FileAccess.Write))
                    await processStream.CopyToAsync(fs);

                double durationSeconds = await FFmpegCoreHelper.GetAudioDurationSecondsAsync(inputPath);
                Console.WriteLine($"[DEBUG] Audio duration: {durationSeconds}s");

                int segmentTime = ComputeSegmentTime(durationSeconds);
                Console.WriteLine($"[DEBUG] Segment time: {segmentTime}s");

                // Short audio or single segment processing
                if (durationSeconds > 0 && (durationSeconds <= 90 || durationSeconds <= segmentTime))
                {
                    var outFile = Path.Combine(workDir, $"single{formatInfo.Extension}");
                    Console.WriteLine($"[DEBUG] Single pass processing: {outFile}");

                    // ✅ Use dynamic encoding
                    var success = await FFmpegCoreHelper.RunFfmpegAsync(
                        $"-hide_banner -loglevel error -nostdin " +
                        $"-i \"{inputPath}\" -vn -af \"{filterChain}\" " +
                        $"{encodingStrategy.FfmpegArgs} -threads 1 -y \"{outFile}\"");

                    Console.WriteLine($"[DEBUG] Success: {success}");

                    if (!success || !File.Exists(outFile))
                    {
                        Console.WriteLine($"[DEBUG] Single pass failed");
                        return null;
                    }

                    return await FFmpegCoreHelper.LoadToMemory(outFile);
                }

                // ✅ Multi-segment processing with format awareness
                var segmentDir = Path.Combine(workDir, "parts");
                var segments = await SplitIntoSegmentsAsync(inputPath, segmentDir, segmentTime, formatInfo);
                if (segments == null || segments.Length == 0) return null;

                var processed = await ProcessSegmentsAsync(segments, filterChain, encodingStrategy, formatInfo);
                if (processed == null || processed.Count == 0) return null;

                var finalPath = await CombineAsync(processed, workDir, formatInfo);
                if (finalPath == null) return null;

                Console.WriteLine($"[DEBUG] Final output: {finalPath}");
                var result = await FFmpegCoreHelper.LoadToMemory(finalPath);
                Console.WriteLine($"[DEBUG] Final size: {result.Length} bytes");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                return null;
            }
            finally
            {
                // Cleanup copied stream if needed
                if (processStream != fileStream)
                {
                    processStream.Dispose();
                }

                try { Directory.Delete(workDir, true); } catch { }
            }
        }

        // ✅ NEW: Determine encoding strategy based on format
        private static AudioEncodingStrategy DetermineEncodingStrategy(AudioFormatInfo formatInfo)
        {
            return formatInfo.Format switch
            {
                // Lossless → High quality lossy encoding
                AudioFormat.FLAC or AudioFormat.WAV => new AudioEncodingStrategy
                {
                    Codec = "aac",
                    Bitrate = "256k",
                    FfmpegArgs = "-c:a aac -b:a 256k",
                    Description = "Lossless → AAC 256k (preserve quality)"
                },

                // AAC/M4A → Keep AAC with same/better quality
                AudioFormat.AAC or AudioFormat.M4A => new AudioEncodingStrategy
                {
                    Codec = "aac",
                    Bitrate = "256k",
                    FfmpegArgs = "-c:a aac -b:a 256k",
                    Description = "AAC → AAC 256k (maintain quality)"
                },

                // MP3 → Keep MP3 with good quality
                AudioFormat.MP3 => new AudioEncodingStrategy
                {
                    Codec = "libmp3lame",
                    Bitrate = "192k",
                    FfmpegArgs = "-c:a libmp3lame -b:a 192k",
                    Description = "MP3 → MP3 192k (standard)"
                },

                // Unknown → Safe fallback
                _ => new AudioEncodingStrategy
                {
                    Codec = "libmp3lame",
                    Bitrate = "192k",
                    FfmpegArgs = "-c:a libmp3lame -b:a 192k",
                    Description = "Unknown → MP3 192k (fallback)"
                }
            };
        }

        private int ComputeSegmentTime(double durationSeconds)
        {
            if (durationSeconds <= 0)
                return _equalizerConfig.DefaultShortSegmentSeconds;

            if (durationSeconds <= _equalizerConfig.ShortAudioThresholdSeconds)
                return _equalizerConfig.DefaultShortSegmentSeconds;

            return _equalizerConfig.DefaultLongSegmentSeconds;
        }

        // ✅ REFACTORED: Dynamic extension support
        private static async Task<string[]?> SplitIntoSegmentsAsync(
            string inputPath,
            string segmentDir,
            int segmentTime,
            AudioFormatInfo formatInfo)
        {
            Directory.CreateDirectory(segmentDir);

            // ✅ Dynamic pattern based on format
            var pattern = Path.Combine(segmentDir, $"part_%03d{formatInfo.Extension}");
            Console.WriteLine($"[DEBUG] Split pattern: {pattern}");

            bool ok = await FFmpegCoreHelper.RunFfmpegAsync(
                $"-hide_banner -loglevel error -nostdin " +
                $"-i \"{inputPath}\" -vn -f segment -segment_time {segmentTime} " +
                $"-c copy \"{pattern}\"");

            Console.WriteLine($"[DEBUG] Split success: {ok}");

            if (!ok) return null;

            // ✅ Dynamic file search
            return Directory.GetFiles(segmentDir, $"part_*{formatInfo.Extension}");
        }

        // ✅ REFACTORED: Format-aware processing
        private static async Task<List<string>?> ProcessSegmentsAsync(
            string[] segments,
            string filterChain,
            AudioEncodingStrategy encodingStrategy,
            AudioFormatInfo formatInfo)
        {
            if (segments.Length == 0) return null;

            var processed = new ConcurrentBag<string>();
            int maxPar = Math.Max(1, Environment.ProcessorCount - 1);

            await Parallel.ForEachAsync(
                segments,
                new ParallelOptions { MaxDegreeOfParallelism = maxPar },
                async (seg, _) =>
                {
                    // ✅ Dynamic output extension
                    string outSeg = seg.Replace(formatInfo.Extension, $"_proc{formatInfo.Extension}");
                    Console.WriteLine($"[DEBUG] Processing: {Path.GetFileName(seg)}");

                    // ✅ Use encoding strategy
                    await FFmpegCoreHelper.RunFfmpegAsync(
                        $"-hide_banner -loglevel error -nostdin " +
                        $"-i \"{seg}\" -vn -af \"{filterChain}\" " +
                        $"{encodingStrategy.FfmpegArgs} -threads 1 -y \"{outSeg}\"");

                    if (File.Exists(outSeg))
                        processed.Add(outSeg);
                });

            var ordered = processed
                .OrderBy(f => Path.GetFileName(f))
                .ToList();

            return ordered.Count == 0 ? null : ordered;
        }

        // ✅ REFACTORED: Dynamic final extension
        private static async Task<string?> CombineAsync(
            List<string> orderedProcessed,
            string workDir,
            AudioFormatInfo formatInfo)
        {
            if (orderedProcessed.Count == 0) return null;

            // ✅ Dynamic final path
            string finalPath = Path.Combine(workDir, $"final{formatInfo.Extension}");

            // Single segment optimization
            if (orderedProcessed.Count == 1)
            {
                File.Copy(orderedProcessed[0], finalPath, true);
                Console.WriteLine($"[DEBUG] Single segment copied to: {finalPath}");
                return finalPath;
            }

            Console.WriteLine($"[DEBUG] Combining {orderedProcessed.Count} segments to: {finalPath}");

            try
            {
                // Binary concat for efficiency
                using var finalStream = new FileStream(
                    finalPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 1024 * 1024); // 1MB buffer

                foreach (var segment in orderedProcessed)
                {
                    Console.WriteLine($"[DEBUG] Appending: {Path.GetFileName(segment)}");

                    using var segmentStream = new FileStream(
                        segment,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read,
                        bufferSize: 1024 * 1024);

                    await segmentStream.CopyToAsync(finalStream);
                }

                Console.WriteLine($"[DEBUG] Combine complete, size: {finalStream.Length} bytes");
                return finalPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Combine failed: {ex.Message}");
                return null;
            }
        }
        #endregion
    }

    // ✅ NEW: Encoding strategy model
    public class AudioEncodingStrategy
    {
        public string Codec { get; set; } = string.Empty;
        public string Bitrate { get; set; } = string.Empty;
        public string FfmpegArgs { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}