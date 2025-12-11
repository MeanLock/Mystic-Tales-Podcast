using Microsoft.Extensions.Logging;
using PodcastService.Infrastructure.Helpers.AudioHelpers;
using PodcastService.Infrastructure.Models.Audio;
using PodcastService.Infrastructure.Models.Audio.Tuning;
using PodcastService.Infrastructure.Services.Audio;
using PodcastService.Infrastructure.Services.Audio.Tuning;

namespace PodcastService.BusinessLogic.Services.AudioServices
{
    /// <summary>
    /// Audio tuning pipeline: EQ → AI → Background Merge
    /// Each layer transforms stream → returns new stream
    /// </summary>
    public class AudioTuningService
    {
        private readonly ILogger<AudioTuningService> _logger;
        private readonly EqualizerTuningService _equalizerTuningService;
        private readonly AITuningService _aiTuningService;
        private readonly AdvanceTuningService _advanceTuningService;
        private readonly AudioFormatDetectorHelper _audioFormatDetectorHelper;

        public AudioTuningService(
            ILogger<AudioTuningService> logger,
            EqualizerTuningService equalizerTuningService,
            AITuningService aiTuningService,
            AdvanceTuningService advanceTuningService)
        {
            _logger = logger;
            _equalizerTuningService = equalizerTuningService;
            _aiTuningService = aiTuningService;
            _advanceTuningService = advanceTuningService;
            _audioFormatDetectorHelper = new AudioFormatDetectorHelper();
        }

        /// <summary>
        /// Process tuning pipeline - returns tuned stream
        /// Caller must dispose returned stream
        /// </summary>
        public async Task<Stream?> ProcessTuningAsync(
            Stream inputStream,
            GeneralTuningProfile profile)
        {
            Stream? workingStream = null;
            AudioFormatInfo? formatInfo = null;

            try
            {
                // Validate
                if (inputStream == null)
                    throw new ArgumentNullException(nameof(inputStream), "Input stream cannot be null");

                // Handle non-seekable (S3, network)
                workingStream = await PrepareStreamAsync(inputStream);

                // Detect format
                Console.WriteLine("[FROM] ProcessTuningAsync cho audio gốc");
                formatInfo = _audioFormatDetectorHelper.DetectFormatFromStream(workingStream);
                workingStream.Position = 0;

                _logger.LogInformation($"Tuning: {formatInfo.Format} ({formatInfo.Extension})");

                // PIPELINE: Each layer processes stream

                // Layer 1: EQ
                if (profile.EqualizerProfile != null)
                {
                    workingStream = await ApplyEqualizerAsync(workingStream, profile.EqualizerProfile);
                    if (workingStream == null)
                        throw new Exception("EQ failed, cannot continue tuning");
                }

                // Layer 2: AI
                if (profile.AITuningProfile != null)
                {
                    // var aiResult = await ApplyAITuningAsync(workingStream, profile.AITuningProfile);

                    // if (aiResult.Failed)
                    //     return AudioTuningResult.Failure($"AI failed: {aiResult.Error}");

                    // if (aiResult.Applied && aiResult.Stream != null)
                    // {
                    //     // Dispose old stream, use AI-tuned
                    //     if (workingStream != inputStream)
                    //         workingStream.Dispose();

                    //     workingStream = aiResult.Stream;
                    // }
                }

                // Layer 3: Background
                if (profile.BackgroundMergeProfile != null)
                {
                    workingStream = await ApplyBackgroundMergeAsync(
                        workingStream,
                        profile.BackgroundMergeProfile);

                    if (workingStream == null)
                        throw new Exception("Background merge failed");
                }

                if (profile.MultipleTimeRangeBackgroundMergeProfile != null)
                {
                    _logger.LogInformation("Layer 4: Multiple Time Range Background Merge");
                    workingStream = await ApplyMultipleTimeRangeBackgroundMergeAsync(
                        workingStream,
                        profile.MultipleTimeRangeBackgroundMergeProfile);
                    if (workingStream == null)
                        throw new Exception("Multiple time range background merge failed");
                }


                if (workingStream.CanSeek)
                {
                    workingStream.Position = 0;
                    return workingStream;
                }
                else
                {
                    // Nếu stream không seekable, copy sang MemoryStream
                    var memoryStream = new MemoryStream();
                    await workingStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    return memoryStream;
                }
                // return workingStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pipeline error");

                // Cleanup
                if (workingStream != null && workingStream != inputStream)
                {
                    try { workingStream.Dispose(); } catch { }
                }

                // return AudioTuningResult.Failure(ex.Message);
                throw new Exception("Audio tuning pipeline error, error: " + ex.Message);
            }
        }

        private async Task<Stream> PrepareStreamAsync(Stream inputStream)
        {
            if (!inputStream.CanSeek)
            {
                _logger.LogInformation("Non-seekable → MemoryStream");
                var ms = new MemoryStream();
                await inputStream.CopyToAsync(ms);
                ms.Position = 0;
                return ms;
            }

            if (inputStream.Position != 0)
                inputStream.Position = 0;

            return inputStream;
        }

        private async Task<Stream?> ApplyEqualizerAsync(Stream input, EqualizerProfile profile)
        {
            try
            {
                _logger.LogInformation("Layer 1: EQ");
                var filterChain = _equalizerTuningService.CreateFilterChain(profile);

                if (string.IsNullOrWhiteSpace(filterChain))
                    return input;

                _logger.LogDebug($"Filter: {filterChain}");
                var processed = await _equalizerTuningService.ApplyFilterChain(input, filterChain);

                return processed ?? input;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EQ error");
                input.Position = 0;
                return input;
            }
        }

        // private async Task<AITuningLayerResult> ApplyAITuningAsync(Stream input, AITuningProfile profile)
        // {
        //     try
        //     {
        //         _logger.LogInformation("Layer 2: AI");
        //         var aiResult = await _aiTuningService.ProcessAudioWithAI(input, profile, null);

        //         if (aiResult.Failed)
        //             return AITuningLayerResult.Failed(aiResult.Error);

        //         if (aiResult.Applied && aiResult.fileStream != null)
        //             return AITuningLayerResult.Applied(aiResult.fileStream);

        //         input.Position = 0;
        //         return AITuningLayerResult.NotNeeded();
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "AI error");
        //         return AITuningLayerResult.Failed(ex.Message);
        //     }
        // }

        private async Task<Stream?> ApplyBackgroundMergeAsync(Stream input, BackgroundMergeProfile profile)
        {
            try
            {
                _logger.LogInformation("Layer 3: Background");
                var merged = await _advanceTuningService.MergeBackground(input, profile, null);

                return merged ?? input;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background error");
                input.Position = 0;
                return input;
            }
        }

        private async Task<Stream?> ApplyMultipleTimeRangeBackgroundMergeAsync(Stream input, MultipleTimeRangeBackgroundMergeProfile profile)
        {
            try
            {
                _logger.LogInformation("Layer 4: Multiple Time Range Background Merge");
                var merged = await _advanceTuningService.MergeMultipleTimeRangeBackgrounds(input, profile, null);

                return merged ?? input;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Multiple Time Range Background Merge error");
                input.Position = 0;
                return input;
            }
        }   
    }

    // Result model
    // public class AudioTuningResult
    // {
    //     public bool Success { get; set; }
    //     public string? ErrorMessage { get; set; }
    //     public Stream? Stream { get; set; }
    //     public AudioFormatInfo? Format { get; set; }
    //     public string MimeType => Format?.MimeType ?? "audio/mpeg";
    //     public string Extension => Format?.Extension ?? ".mp3";

    //     public static AudioTuningResult Success(Stream stream, AudioFormatInfo format)
    //         => new() { Success = true, Stream = stream, Format = format };

    //     public static AudioTuningResult Failure(string error)
    //         => new() { Success = false, ErrorMessage = error };
    // }

    // // Internal AI result
    // internal class AITuningLayerResult
    // {
    //     public bool Applied { get; set; }
    //     public bool Failed { get; set; }
    //     public string? Error { get; set; }
    //     public Stream? Stream { get; set; }

    //     public static AITuningLayerResult Applied(Stream s) => new() { Applied = true, Stream = s };
    //     public static AITuningLayerResult Failed(string e) => new() { Failed = true, Error = e };
    //     public static AITuningLayerResult NotNeeded() => new();
    // }
}