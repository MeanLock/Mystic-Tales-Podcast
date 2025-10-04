using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using UserService.Infrastructure.Configurations.Audio.Tuning;
using UserService.Infrastructure.Helpers.AudioHelpers;
using UserService.Infrastructure.Models.Audio.Tuning;

namespace UserService.Infrastructure.Services.Audio.Tuning
{
    public class EqualizerTuningService
    {
        private readonly IMoodConfig _moodConfig;
        private readonly IEqualizerConfig _equalizerConfig;
        public EqualizerTuningService(IMoodConfig moodConfig, IEqualizerConfig equalizerConfig)
        {
            _moodConfig = moodConfig;
            _equalizerConfig = equalizerConfig;
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

            string workDir = Path.Combine(Path.GetTempPath(), "eqseg_" + Guid.NewGuid().ToString("N"));
            Console.WriteLine($"[DEBUG] WorkDir: {workDir}");
            Directory.CreateDirectory(workDir);
            // kiểm tra folder tạo được chưa
            if (!Directory.Exists(workDir))
                Console.WriteLine($"[DEBUG] Failed to create workDir: {workDir}");

            string inputPath = Path.Combine(workDir, "input.mp3");

            try
            {
                //  Ghi stream vào file tạm
                using (var fs = new FileStream(inputPath, FileMode.Create, FileAccess.Write))
                    await fileStream.CopyToAsync(fs);

                double durationSeconds = await FFmpegCoreHelper.GetAudioDurationSecondsAsync(inputPath);
                Console.WriteLine($"[DEBUG] Audio duration seconds (initial): {durationSeconds}");

                int segmentTime = ComputeSegmentTime(durationSeconds);

                Console.WriteLine($"[DEBUG] Audio duration seconds: {durationSeconds}");
                Console.WriteLine($"[DEBUG] Computed segment time: {segmentTime}");
                // int segmentTime = 1000; // Cố định 1 phút

                if (durationSeconds > 0 && (durationSeconds <= 90 || durationSeconds <= segmentTime))
                {
                    var outFile = Path.Combine(workDir, "single.mp3");
                    Console.WriteLine($"[DEBUG] Single pass processing, outFile: {outFile}");
                    var success = await FFmpegCoreHelper.RunFfmpegAsync($"-hide_banner -loglevel error -nostdin -i \"{inputPath}\" -vn -af \"{filterChain}\" -c:a libmp3lame -b:a 192k -threads 1 -y \"{outFile}\"");
                    Console.WriteLine($"[DEBUG] Single pass processing success: {success}");
                    if (!success || !File.Exists(outFile))
                    {
                        Console.WriteLine($"[DEBUG] Single pass processing failed.");
                        return null;
                    }

                    var memoryStream = await FFmpegCoreHelper.LoadToMemory(outFile);

                    return memoryStream;
                }

                // Split
                var segmentDir = Path.Combine(workDir, "parts");
                var segments = await SplitIntoSegmentsAsync(inputPath, segmentDir, segmentTime);
                if (segments == null || segments.Length == 0) return null;

                // song song từng segment
                var processed = await ProcessSegmentsAsync(segments, filterChain);
                if (processed == null || processed.Count == 0) return null;

                //  Combine
                var finalPath = await CombineAsync(processed, workDir);
                if (finalPath == null) return null;


                //  Load vào MemoryStream
                Console.WriteLine($"[DEBUG] Final output path: {finalPath}");
                var result = await FFmpegCoreHelper.LoadToMemory(finalPath);
                Console.WriteLine($"[DEBUG] Final output size: {result.Length}");
                return result;
            }
            catch
            {
                return null;
            }
            finally
            {
                try { Directory.Delete(workDir, true); } catch { }
            }
        }

        // hàm này tính segment time dựa trên duration
        // ý tưởng là audio càng dài thì segment càng dài để giảm số segment, tránh overhead
        // private static int ComputeSegmentTime(double durationSeconds)
        // {
        //     if (durationSeconds <= 0) return 30;
        //     int parallelism = Math.Max(1, Environment.ProcessorCount - 1);
        //     int targetSegments = Math.Max(3, parallelism * 3);
        //     int seg = (int)Math.Ceiling(durationSeconds / targetSegments);
        //     return Math.Clamp(seg, 20, 120);
        // }

        // hàm này tính segment time dựa trên duration
        private int ComputeSegmentTime(double durationSeconds)
        {
            if (durationSeconds <= 0)
                return _equalizerConfig.DefaultShortSegmentSeconds;

            if (durationSeconds <= _equalizerConfig.ShortAudioThresholdSeconds)
                return _equalizerConfig.DefaultShortSegmentSeconds;

            return _equalizerConfig.DefaultLongSegmentSeconds;
        }

        private static async Task<string[]?> SplitIntoSegmentsAsync(string inputPath, string segmentDir, int segmentTime)
        {
            Directory.CreateDirectory(segmentDir);
            var pattern = Path.Combine(segmentDir, "part_%03d.mp3");
            Console.WriteLine($"[DEBUG] Splitting into segments with pattern: {pattern}");
            bool ok = await FFmpegCoreHelper.RunFfmpegAsync(
                $"-hide_banner -loglevel error -nostdin -i \"{inputPath}\" -vn -f segment -segment_time {segmentTime} -c copy \"{pattern}\"");
            Console.WriteLine($"[DEBUG] Splitting success: {ok}");
            if (!ok) return null;
            return Directory.GetFiles(segmentDir, "part_*.mp3");
        }

        private static async Task<List<string>?> ProcessSegmentsAsync(string[] segments, string filterChain)
        {
            if (segments.Length == 0) return null;
            var processed = new ConcurrentBag<string>();
            int maxPar = Math.Max(1, Environment.ProcessorCount - 1);

            await Parallel.ForEachAsync(
                segments,
                new ParallelOptions { MaxDegreeOfParallelism = maxPar },
                async (seg, _) =>
                {
                    string outSeg = seg.Replace(".mp3", "_proc.mp3");
                    Console.WriteLine($"[DEBUG] Processing segment: {seg} to {outSeg}");
                    await FFmpegCoreHelper.RunFfmpegAsync(
                        $"-hide_banner -loglevel error -nostdin -i \"{seg}\" -vn -af \"{filterChain}\" -c:a libmp3lame -b:a 192k -threads 1 -y \"{outSeg}\"");
                    Console.WriteLine($"[DEBUG] Finished processing segment: {seg}");
                    if (File.Exists(outSeg))
                        processed.Add(outSeg);
                });

            var ordered = processed
                .OrderBy(f => Path.GetFileName(f))
                .ToList();

            return ordered.Count == 0 ? null : ordered;
        }

        // private static async Task<string?> CombineAsync(List<string> orderedProcessed, string workDir)
        // {
        //     string listFile = Path.Combine(workDir, "concat.txt");
        //     Console.WriteLine($"[DEBUG] Combining segments, list file: {listFile}");
        //     await File.WriteAllLinesAsync(
        //         listFile,
        //         orderedProcessed.Select(p => $"file '{p.Replace("\\", "/")}'"));

        //     Console.WriteLine($"[DEBUG] Written concat list file.");

        //     string finalPath = Path.Combine(workDir, "final.mp3");
        //     Console.WriteLine($"[DEBUG] Final output path will be: {finalPath}");
        //     bool ok = await FFmpegCoreHelper.RunFfmpegAsync(
        //         $"-hide_banner -loglevel error -nostdin -f concat -safe 0 -i \"{listFile}\" -c copy -y \"{finalPath}\"");
        //     Console.WriteLine($"[DEBUG] Combining segments success: {ok}");
        //     return ok && File.Exists(finalPath) ? finalPath : null;
        // }

        private static async Task<string?> CombineAsync(List<string> orderedProcessed, string workDir)
        {
            if (orderedProcessed.Count == 0) return null;

            // Nếu chỉ có 1 segment, copy trực tiếp
            if (orderedProcessed.Count == 1)
            {
                string singleFinalPath = Path.Combine(workDir, "final.mp3");
                File.Copy(orderedProcessed[0], singleFinalPath, true);
                Console.WriteLine($"[DEBUG] Single segment, copied directly to: {singleFinalPath}");
                return singleFinalPath;
            }

            string finalPath = Path.Combine(workDir, "final.mp3");
            Console.WriteLine($"[DEBUG] Binary combining {orderedProcessed.Count} segments to: {finalPath}");

            try
            {
                // Binary concat - nhanh nhất cho MP3
                using var finalStream = new FileStream(finalPath, FileMode.Create, FileAccess.Write, FileShare.None,
                    bufferSize: 1024 * 1024); // 1MB buffer cho performance

                foreach (var segment in orderedProcessed)
                {
                    Console.WriteLine($"[DEBUG] Appending segment: {Path.GetFileName(segment)}");

                    using var segmentStream = new FileStream(segment, FileMode.Open, FileAccess.Read, FileShare.Read,
                        bufferSize: 1024 * 1024); // 1MB buffer

                    await segmentStream.CopyToAsync(finalStream);
                }

                Console.WriteLine($"[DEBUG] Binary concat completed, final size: {finalStream.Length} bytes");
                return finalPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Binary concat failed: {ex.Message}");
                return null;
            }
        }



        #endregion
    }
}


