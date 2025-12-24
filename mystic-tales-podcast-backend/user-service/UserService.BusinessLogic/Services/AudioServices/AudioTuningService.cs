using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;
using UserService.Infrastructure.Models.Audio.Tuning;
using UserService.Infrastructure.Services.Audio.Tuning;

namespace UserService.BusinessLogic.Services.AudioServices
{
    public class AudioTuningService
    {
        private readonly EqualizerTuningService _equalizerTuningService;
        private readonly AITuningService _aiTuningService;
        private readonly AdvanceTuningService _advanceTuningService;
        public AudioTuningService(EqualizerTuningService equalizerTuningService, AITuningService aiTuningService, AdvanceTuningService advanceTuningService)
        {
            _equalizerTuningService = equalizerTuningService;
            _aiTuningService = aiTuningService;
            _advanceTuningService = advanceTuningService;
        }

        public async Task<IActionResult> ProcessTuning(Stream fileStream, GeneralTuningProfile generalTuningProfile)
        {
            if (fileStream == null) return new BadRequestObjectResult("File stream null");

            string? filterChain = null;

            if (generalTuningProfile.EqualizerProfile != null)
            {

                filterChain = _equalizerTuningService.CreateFilterChain(generalTuningProfile.EqualizerProfile);
                Console.WriteLine($"[DEBUG] Filter chain: {filterChain}");
            }

            if (generalTuningProfile.AITuningProfile != null)
            {
                var aiResult = await _aiTuningService.ProcessAudioWithAI(fileStream, generalTuningProfile.AITuningProfile, filterChain);

                if (aiResult.Failed)
                    return new ObjectResult(new { error = aiResult.Error }) { StatusCode = 502 };

                if (aiResult.Applied && aiResult.fileStream != null)
                {
                    if (generalTuningProfile.BackgroundMergeProfile?.FileStream != null)
                    {
                        var bgMerged = await _advanceTuningService.MergeBackground(aiResult.fileStream, generalTuningProfile.BackgroundMergeProfile, null);
                        if (bgMerged != null)
                            return new FileStreamResult(bgMerged, "audio/mpeg") { FileDownloadName = "tuned_audio.mp3" };
                    }
                    return new FileStreamResult(aiResult.fileStream, "audio/mpeg") { FileDownloadName = "tuned_audio.mp3" };
                }

                if (aiResult.NotNeeded)
                    fileStream.Position = 0;
            }

            if (generalTuningProfile.BackgroundMergeProfile != null)
            {
                var bgMerged = await _advanceTuningService.MergeBackground(fileStream, generalTuningProfile.BackgroundMergeProfile, filterChain);

                if (bgMerged != null)
                    return new FileStreamResult(bgMerged, "audio/mpeg") { FileDownloadName = "merge.mp3" };
            }


            if (!string.IsNullOrWhiteSpace(filterChain))
            {
                var processed = await _equalizerTuningService.ApplyFilterChain(fileStream, filterChain);
                if (processed != null)
                    return new FileStreamResult(processed, "audio/mpeg") { FileDownloadName = "tuned_audio.mp3" };
            }
            var original = new MemoryStream();
            await fileStream.CopyToAsync(original);
            original.Position = 0;
            return new FileStreamResult(original, "audio/mpeg") { FileDownloadName = "original_audio.mp3" };

        }


    }
}
