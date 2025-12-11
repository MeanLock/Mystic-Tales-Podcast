using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using PodcastService.Common.AppConfigurations.FilePath.interfaces;
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
        private readonly IWebHostEnvironment _environment;
        private readonly IFilePathConfig _filePathConfig;
        private readonly ILogger<AdvanceTuningService>? _logger;

        public AdvanceTuningService(
            EqualizerTuningService equalizerTuningService,
            IWebHostEnvironment environment,
            IFilePathConfig filePathConfig,
            ILogger<AdvanceTuningService>? logger = null)
        {
            _equalizerTuningService = equalizerTuningService;
            _audioFormatDetectorHelper = new AudioFormatDetectorHelper(logger as ILogger<AudioFormatDetectorHelper>);
            _environment = environment;
            _filePathConfig = filePathConfig;
            _logger = logger;
        }

        #region Public Methods - Background Merging

        /// <summary>
        /// Merge background audio with main audio, preserving quality based on input format
        /// </summary>
        public async Task<Stream?> MergeBackground(
            Stream fileStream,
            BackgroundMergeProfile backgroundMergeProfile,
            string? filterChain)
        {
            if (backgroundMergeProfile?.FileStream == null) return null;

            // var tempDir = Path.Combine(Path.GetTempPath(), "bg_merge_" + Guid.NewGuid().ToString("N"));
            var tempDir = Path.Combine(_environment.ContentRootPath, _filePathConfig.AUDIO_TUNING_SINGLE_BACKGROUND_MERGE_LOCAL_TEMP_FILE_PATH, "bg_merge_" + Guid.NewGuid().ToString("N"));
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

                // ✅ STEP 4: Determine output format strategy (single background)
                var outputStrategy = DetermineOutputStrategy(mainFormat, new List<AudioFormatInfo> { bgFormat });
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
        /// Merge multiple time-ranged background audios with main audio
        /// </summary>
        public async Task<Stream?> MergeMultipleTimeRangeBackgrounds(
            Stream mainAudioStream,
            MultipleTimeRangeBackgroundMergeProfile profile,
            string? filterChain)
        {
            if (profile?.TimeRangeMergeBackgrounds == null || !profile.TimeRangeMergeBackgrounds.Any())
                return null;

            // var tempDir = Path.Combine(Path.GetTempPath(), "multi_bg_merge_" + Guid.NewGuid().ToString("N"));
            var tempDir = Path.Combine(_environment.ContentRootPath, _filePathConfig.AUDIO_TUNING_MULTI_BACKGROUND_MERGE_LOCAL_TEMP_FILE_PATH, "multi_bg_merge_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            Stream? mainWorkingStream = null;
            var bgWorkingStreams = new List<Stream>();
            AudioFormatInfo? mainFormat = null;
            var bgFormats = new List<AudioFormatInfo>();

            try
            {
                // ✅ STEP 1: Prepare main stream
                mainWorkingStream = await PrepareStreamAsync(mainAudioStream);
                if (mainWorkingStream == null)
                {
                    _logger?.LogError("Failed to prepare main stream");
                    return null;
                }

                // ✅ STEP 2: Detect main audio format and get duration
                Console.WriteLine("[FROM] MergeMultipleTimeRangeBackgrounds cho audio gốc");
                mainFormat = _audioFormatDetectorHelper.DetectFormatFromStream(mainWorkingStream);
                mainWorkingStream.Position = 0;

                var mainInput = Path.Combine(tempDir, $"main{mainFormat.Extension}");
                using (var fs = new FileStream(mainInput, FileMode.Create, FileAccess.Write))
                    await mainWorkingStream.CopyToAsync(fs);

                double mainDuration = await FFmpegCoreHelper.GetAudioDurationSecondsAsync(mainInput);
                if (mainDuration <= 0)
                {
                    _logger?.LogError("Could not determine main audio duration");
                    return null;
                }

                _logger?.LogInformation($"Main audio: {mainFormat.Format}, Duration: {mainDuration}s");

                // ✅ STEP 3: Validate time ranges
                ValidateTimeRangedItems(profile.TimeRangeMergeBackgrounds, mainDuration);

                // ✅ STEP 4: Prepare background streams
                for (int i = 0; i < profile.TimeRangeMergeBackgrounds.Count; i++)
                {
                    var item = profile.TimeRangeMergeBackgrounds[i];
                    if (item.FileStream == null)
                    {
                        throw new ArgumentException($"Background item {i} has null FileStream");
                    }

                    var bgStream = await PrepareStreamAsync(item.FileStream);
                    // var bgStreamDuration = (int)await FFmpegCoreHelper.GetAudioDurationSecondsFromStreamAsync(bgStream);
                    // Console.WriteLine($"[DEBUG] Item duration: {bgStreamDuration}s");
                    if (bgStream == null)
                    {
                        throw new Exception($"Failed to prepare background stream {i}");
                    }

                    Console.WriteLine($"[FROM] Detecting format for background {i}");
                    var bgFormat = _audioFormatDetectorHelper.DetectFormatFromStream(bgStream);
                    Console.WriteLine($"[DEBUG] Detected format: {bgFormat.Format}");
                    // in kết quả định dạng
                    Console.WriteLine($"Background {i} format detected: {bgFormat.Format}, start second: {item.BackgroundCutStartSecond}, end second: {item.BackgroundCutEndSecond}, merge start second: {item.OriginalMergeStartSecond}, merge end second: {item.OriginalMergeEndSecond}");
                    bgStream.Position = 0;

                    var bgInput = Path.Combine(tempDir, $"bg{i}{bgFormat.Extension}");
                    using (var fs = new FileStream(bgInput, FileMode.Create, FileAccess.Write))
                        await bgStream.CopyToAsync(fs);

                    bgWorkingStreams.Add(bgStream);
                    bgFormats.Add(bgFormat);

                    _logger?.LogDebug($"Background {i}: {bgFormat.Format}, File: {bgInput}");
                }

                // ✅ STEP 5: Determine output format strategy
                var outputStrategy = DetermineOutputStrategy(mainFormat, bgFormats);
                var mergedOut = Path.Combine(tempDir, $"merged{outputStrategy.Extension}");

                _logger?.LogInformation($"Output strategy: {outputStrategy.Description}");

                // ✅ STEP 6: Build complex FFmpeg filter
                var filter = await BuildMultipleTimeRangeFilter(profile.TimeRangeMergeBackgrounds, mainDuration);

                // ✅ STEP 7: Build FFmpeg command
                var codecArgs = BuildCodecArguments(outputStrategy);
                var inputArgs = new System.Text.StringBuilder();
                inputArgs.Append($"-i \"{mainInput}\" ");
                for (int i = 0; i < profile.TimeRangeMergeBackgrounds.Count; i++)
                {
                    inputArgs.Append($"-i \"{Path.Combine(tempDir, $"bg{i}{bgFormats[i].Extension}")}\" ");
                }

                var ffmpegCommand = $"-hide_banner -loglevel error -nostdin " +
                                   $"{inputArgs}" +
                                   $"-filter_complex \"{filter}\" " +
                                   $"-map \"[out]\" {codecArgs} -y \"{mergedOut}\"";

                _logger?.LogDebug($"FFmpeg command: {ffmpegCommand}");

                // ✅ STEP 8: Execute merge
                var success = await FFmpegCoreHelper.RunFfmpegAsync(ffmpegCommand);
                if (!success || !File.Exists(mergedOut))
                {
                    _logger?.LogError("FFmpeg multi-background merge failed");
                    return null;
                }

                string finalPath = mergedOut;

                // ✅ STEP 9: Apply filterChain if provided (EQ/mood)
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
                _logger?.LogError(ex, "Error merging multiple time-ranged backgrounds");
                throw;
            }
            finally
            {
                // Cleanup working streams
                if (mainWorkingStream != null && mainWorkingStream != mainAudioStream)
                {
                    try { mainWorkingStream.Dispose(); } catch { }
                }

                foreach (var bgStream in bgWorkingStreams)
                {
                    try { bgStream.Dispose(); } catch { }
                }

                // Cleanup temp directory
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }

        #endregion

        #region Validation Helpers - Time Range Backgrounds

        /// <summary>
        /// Validate time-ranged background items for overlaps and bounds
        /// </summary>
        private void ValidateTimeRangedItems(
            List<TimeRangeMergeBackground> items,
            double mainAudioDuration)
        {
            // Validate each item individually
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                if (!item.IsValid())
                {
                    throw new ArgumentException(
                        $"Item {i}: Invalid duration match. " +
                        $"Background: {item.BackgroundDuration:F2}s, Merge: {item.MergeDuration:F2}s");
                }

                // Check if merge end exceeds main audio duration
                if (item.OriginalMergeEndSecond > mainAudioDuration)
                {
                    throw new ArgumentException(
                        $"Item {i}: Merge end ({item.OriginalMergeEndSecond:F2}s) exceeds main audio duration ({mainAudioDuration:F2}s)");
                }
            }

            // Check for overlaps between items
            for (int i = 0; i < items.Count; i++)
            {
                for (int j = i + 1; j < items.Count; j++)
                {
                    var item1 = items[i];
                    var item2 = items[j];

                    bool overlaps = !(item1.OriginalMergeEndSecond <= item2.OriginalMergeStartSecond ||
                                     item2.OriginalMergeEndSecond <= item1.OriginalMergeStartSecond);

                    if (overlaps)
                    {
                        throw new ArgumentException(
                            $"Overlap detected between item {i} [{item1.OriginalMergeStartSecond:F2}s-{item1.OriginalMergeEndSecond:F2}s] " +
                            $"and item {j} [{item2.OriginalMergeStartSecond:F2}s-{item2.OriginalMergeEndSecond:F2}s]");
                    }
                }
            }

            _logger?.LogInformation($"Validation passed: {items.Count} background items, no overlaps");
        }

        #endregion

        #region FFmpeg Filter Builders - Time Range Backgrounds

        // /// <summary>
        // /// Build complex FFmpeg filter for multiple time-ranged backgrounds with fade effects
        // /// </summary>
        // private string BuildMultipleTimeRangeFilter(
        //     List<TimeRangeMergeBackground> items,
        //     double mainDuration)
        // {
        //     var filterParts = new List<string>();
        //     var mixInputs = new List<string> { "[0:a]" };

        //     for (int i = 0; i < items.Count; i++)
        //     {
        //         var item = items[i];
        //         var bgIndex = i + 1; // Background input index (0 is main)
        //         var outputLabel = $"bg{i}";

        //         // Calculate fade timings
        //         var clipDuration = item.BackgroundDuration;
        //         var fadeOutStart = Math.Max(0, clipDuration - 0.5);

        //         // Build filter for this background:
        //         // 1. atrim: Cut the segment
        //         // 2. afade in: 500ms fade in
        //         // 3. afade out: 500ms fade out
        //         // 4. volume: Apply gain
        //         // 5. adelay: Position at merge start time
        //         var filter = $"[{bgIndex}:a]" +
        //                     $"atrim=start={item.BackgroundCutStartSecond.ToString(CultureInfo.InvariantCulture)}:" +
        //                     $"end={item.BackgroundCutEndSecond.ToString(CultureInfo.InvariantCulture)}," +
        //                     $"afade=t=in:st=0:d=0.5," +
        //                     $"afade=t=out:st={fadeOutStart.ToString(CultureInfo.InvariantCulture)}:d=0.5," +
        //                     $"volume={(item.VolumeGainDb ?? 0).ToString(CultureInfo.InvariantCulture)}dB," +
        //                     $"adelay={((int)(item.OriginalMergeStartSecond * 1000)).ToString(CultureInfo.InvariantCulture)}|" +
        //                     $"{((int)(item.OriginalMergeStartSecond * 1000)).ToString(CultureInfo.InvariantCulture)}" +
        //                     $"[{outputLabel}]";

        //         filterParts.Add(filter);
        //         mixInputs.Add($"[{outputLabel}]");
        //     }

        //     // Final mix: Combine main + all backgrounds
        //     var mixFilter = $"{string.Join("", mixInputs)}amix=inputs={mixInputs.Count}:duration=first:dropout_transition=0[out]";
        //     filterParts.Add(mixFilter);

        //     var completeFilter = string.Join(";", filterParts);

        //     _logger?.LogDebug($"Generated filter: {completeFilter}");

        //     return completeFilter;
        // }

        /// <summary>
        /// Build complex FFmpeg filter for multiple time-ranged backgrounds with fade effects
        /// </summary>
        private async Task<string> BuildMultipleTimeRangeFilter(
            List<TimeRangeMergeBackground> items,
            double mainDuration)
        {
            var filterParts = new List<string>();

            // Start with main audio
            filterParts.Add("[0:a]aformat=sample_rates=48000:channel_layouts=stereo[main]");

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var bgIndex = i + 1;
                var bgLabel = $"bg{i}";
                var trimLabel = $"trim{i}";

                var clipDuration = item.BackgroundDuration;
                var fadeDurations = CalculateFadeDurations(items);
                var fadeInDuration = fadeDurations[i].FadeInDuration;
                var fadeOutDuration = fadeDurations[i].FadeOutDuration;
                var fadeOutStart = Math.Max(0, clipDuration - fadeOutDuration);

                // ✅ CÁCH TỐT NHẤT: Trim → Fade → Volume → SetPTS, sau đó overlay với enable timing
                var filter = $"[{bgIndex}:a]" +
                            $"aformat=sample_rates=48000:channel_layouts=stereo," +
                            $"atrim=start={item.BackgroundCutStartSecond.ToString(CultureInfo.InvariantCulture)}:" +
                            $"duration={clipDuration.ToString(CultureInfo.InvariantCulture)}," +
                            $"asetpts=PTS-STARTPTS," +
                            $"afade=t=in:st=0:d={fadeInDuration.ToString(CultureInfo.InvariantCulture)}," +
                            $"afade=t=out:st={fadeOutStart.ToString(CultureInfo.InvariantCulture)}:d={fadeOutDuration.ToString(CultureInfo.InvariantCulture)}," +
                            $"volume={(item.VolumeGainDb ?? 0).ToString(CultureInfo.InvariantCulture)}dB," +
                            // ✅ Add delay samples at the beginning
                            $"adelay={((int)(item.OriginalMergeStartSecond * 1000)).ToString(CultureInfo.InvariantCulture)}:all=1" +
                            $"[{bgLabel}]";

                filterParts.Add(filter);
            }

            // // ✅ Build amix with all inputs
            // var mixInputs = new System.Text.StringBuilder("[main]");
            // for (int i = 0; i < items.Count; i++)
            // {
            //     mixInputs.Append($"[bg{i}]");
            // }
            // mixInputs.Append($"amix=inputs={items.Count + 1}:duration=longest:dropout_transition=0,");
            // mixInputs.Append($"atrim=0:{mainDuration.ToString(CultureInfo.InvariantCulture)}[out]");

            // filterParts.Add(mixInputs.ToString());

            // ✅ Build amix with all inputs
            var mixInputs = new System.Text.StringBuilder("[main]");
            for (int i = 0; i < items.Count; i++)
            {
                mixInputs.Append($"[bg{i}]");
            }

            // ✅ KHUYẾN NGHỊ: Dùng weights="1.0 1.0 1.0 1.0"
            // Vì bạn ĐÃ có VolumeGainDb điều chỉnh background rồi!
            var weights = "1.0"; // Main
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                double bgWeight = 0.5; // Default

                // Điều chỉnh weight dựa trên VolumeGainDb
                if (item.VolumeGainDb <= -8)
                {
                    bgWeight = 0.6; // Đã nhỏ rồi, tăng weight
                }
                else if (item.VolumeGainDb >= -3)
                {
                    bgWeight = 0.4; // Còn to, giảm weight
                }

                weights += $" {bgWeight.ToString(CultureInfo.InvariantCulture)}";
            }

            mixInputs.Append($"amix=inputs={items.Count + 1}:duration=longest:dropout_transition=0:weights='{weights}':normalize=0,");
            mixInputs.Append($"dynaudnorm=f=150:g=15:p=0.9,"); // Tự động tránh clipping
            mixInputs.Append($"atrim=0:{mainDuration.ToString(CultureInfo.InvariantCulture)}[out]");

            filterParts.Add(mixInputs.ToString());

            var completeFilter = string.Join(";", filterParts);

            Console.WriteLine($"[DEBUG] Complete filter: {completeFilter}");

            return completeFilter;
        }
        /// <summary>
        /// Calculate fade durations for each item based on gaps between adjacent items
        /// </summary>
        private List<FadeDurationInfo> CalculateFadeDurations(List<TimeRangeMergeBackground> items)
        {
            const double NORMAL_FADE = 0.5;  // 500ms
            const double SHORT_FADE = 0.25;  // 250ms
            const double ADJACENT_THRESHOLD = 0.2; // 200ms - consider adjacent if gap <= this

            var result = new List<FadeDurationInfo>();

            for (int i = 0; i < items.Count; i++)
            {
                var currentItem = items[i];
                double fadeInDuration = NORMAL_FADE;
                double fadeOutDuration = NORMAL_FADE;
                double gapToPrevious = -1;
                double gapToNext = -1;

                // Check gap to PREVIOUS item
                if (i > 0)
                {
                    var prevItem = items[i - 1];
                    gapToPrevious = currentItem.OriginalMergeStartSecond - prevItem.OriginalMergeEndSecond;

                    if (gapToPrevious <= ADJACENT_THRESHOLD)
                    {
                        fadeInDuration = SHORT_FADE; // Shorten fade in
                        _logger?.LogDebug($"Item {i} adjacent to item {i - 1} (gap: {gapToPrevious:F3}s) → FadeIn: {SHORT_FADE}s");
                    }
                }

                // Check gap to NEXT item
                if (i < items.Count - 1)
                {
                    var nextItem = items[i + 1];
                    gapToNext = nextItem.OriginalMergeStartSecond - currentItem.OriginalMergeEndSecond;

                    if (gapToNext <= ADJACENT_THRESHOLD)
                    {
                        fadeOutDuration = SHORT_FADE; // Shorten fade out
                        _logger?.LogDebug($"Item {i} adjacent to item {i + 1} (gap: {gapToNext:F3}s) → FadeOut: {SHORT_FADE}s");
                    }
                }

                result.Add(new FadeDurationInfo
                {
                    ItemIndex = i,
                    FadeInDuration = fadeInDuration,
                    FadeOutDuration = fadeOutDuration,
                    GapToPrevious = gapToPrevious,
                    GapToNext = gapToNext
                });
            }

            return result;
        }

        /// <summary>
        /// Fade duration information for an item
        /// </summary>
        private class FadeDurationInfo
        {
            public int ItemIndex { get; set; }
            public double FadeInDuration { get; set; }
            public double FadeOutDuration { get; set; }
            public double GapToPrevious { get; set; }
            public double GapToNext { get; set; }
        }

        #endregion

        #region Stream Preparation Helpers

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

        #endregion

        #region Format Strategy Helpers

        /// <summary>
        /// Determine optimal output format strategy based on ALL inputs (main + multiple backgrounds)
        /// Uses "bottleneck principle" - output quality limited by lowest quality input
        /// </summary>
        private OutputFormatStrategy DetermineOutputStrategy(
            AudioFormatInfo mainFormat,
            List<AudioFormatInfo> bgFormats)
        {
            bool mainIsLossless = mainFormat.IsLossless;
            bool allBgLossless = bgFormats.All(bg => bg.IsLossless);
            bool anyBgLossless = bgFormats.Any(bg => bg.IsLossless);

            _logger?.LogDebug($"Format analysis - Main: {mainFormat.Format} (Lossless: {mainIsLossless}), " +
                             $"Backgrounds: {bgFormats.Count} (All lossless: {allBgLossless}, Any lossless: {anyBgLossless})");

            // ========================================
            // SCENARIO 1: All inputs are lossless
            // ========================================
            if (mainIsLossless && allBgLossless)
            {
                _logger?.LogInformation("Strategy: All inputs lossless → Output FLAC");
                return new OutputFormatStrategy
                {
                    Format = AudioFormat.FLAC,
                    Extension = ".flac",
                    Codec = "flac",
                    Bitrate = null,
                    Description = "Lossless output (all inputs lossless)"
                };
            }

            // ========================================
            // SCENARIO 2: Mixed lossless/lossy
            // → Output high-quality lossy (no fake upsample)
            // ========================================
            if (mainIsLossless || anyBgLossless)
            {
                _logger?.LogInformation("Strategy: Mixed lossless/lossy → Output high-quality lossy");

                // Find any AAC/M4A in backgrounds for format preference
                var hasAacBg = bgFormats.Any(bg => bg.Format is AudioFormat.AAC or AudioFormat.M4A);

                if (mainFormat.Format is AudioFormat.AAC or AudioFormat.M4A || hasAacBg)
                {
                    return new OutputFormatStrategy
                    {
                        Format = AudioFormat.AAC,
                        Extension = ".m4a",
                        Codec = "aac",
                        Bitrate = "256k",
                        Description = "AAC 256k (mixed quality inputs)"
                    };
                }
                else
                {
                    return new OutputFormatStrategy
                    {
                        Format = AudioFormat.MP3,
                        Extension = ".mp3",
                        Codec = "libmp3lame",
                        Bitrate = "256k",
                        Description = "MP3 256k (mixed quality inputs)"
                    };
                }
            }

            // ========================================
            // SCENARIO 3: All inputs are lossy
            // → Choose format based on compatibility
            // ========================================
            _logger?.LogInformation($"Strategy: All lossy (Main: {mainFormat.Format}, Backgrounds: {string.Join(", ", bgFormats.Select(bg => bg.Format))})");

            // Priority 1: Main is AAC/M4A
            if (mainFormat.Format is AudioFormat.AAC or AudioFormat.M4A)
            {
                return new OutputFormatStrategy
                {
                    Format = AudioFormat.AAC,
                    Extension = ".m4a",
                    Codec = "aac",
                    Bitrate = "256k",
                    Description = "AAC 256k (main AAC/M4A)"
                };
            }

            // Priority 2: Any background is AAC/M4A
            if (bgFormats.Any(bg => bg.Format is AudioFormat.AAC or AudioFormat.M4A))
            {
                return new OutputFormatStrategy
                {
                    Format = AudioFormat.AAC,
                    Extension = ".m4a",
                    Codec = "aac",
                    Bitrate = "256k",
                    Description = "AAC 256k (background AAC/M4A)"
                };
            }

            // Priority 3: Default to MP3 (universal compatibility)
            return new OutputFormatStrategy
            {
                Format = AudioFormat.MP3,
                Extension = ".mp3",
                Codec = "libmp3lame",
                Bitrate = "256k",
                Description = "MP3 256k (universal compatibility)"
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

        #endregion

        #region Internal Models

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

        #endregion
    }
}