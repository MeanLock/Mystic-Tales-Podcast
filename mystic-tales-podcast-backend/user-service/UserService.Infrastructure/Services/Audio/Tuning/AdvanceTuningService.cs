using System.Globalization;
using UserService.Infrastructure.Helpers.AudioHelpers;
using UserService.Infrastructure.Models.Audio.Tuning;

namespace UserService.Infrastructure.Services.Audio.Tuning
{
    public class AdvanceTuningService
    {
        private readonly EqualizerTuningService _equalizerTuningService;

        public AdvanceTuningService(EqualizerTuningService equalizerTuningService)
        {
            _equalizerTuningService = equalizerTuningService;
        }

        public async Task<Stream?> MergeBackground(Stream fileStream, BackgroundMergeProfile backgroundMergeProfile, string? filterChain)
        {
            if (backgroundMergeProfile?.FileStream == null) return null;

            var tempDir = Path.Combine(Path.GetTempPath(), "bg_merge_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            var mainInput = Path.Combine(tempDir, "main.mp3");
            var bgInput = Path.Combine(tempDir, "bg.mp3");
            var mergedOut = Path.Combine(tempDir, "merged.mp3");

            try
            {
                // Ghi 2 streams ra file tạm
                using (var fs = new FileStream(mainInput, FileMode.Create, FileAccess.Write))
                    await fileStream.CopyToAsync(fs);

                using (var fs = new FileStream(bgInput, FileMode.Create, FileAccess.Write))
                    await backgroundMergeProfile.FileStream.CopyToAsync(fs);

                // Lấy duration của file chính
                double durationSeconds = await FFmpegCoreHelper.GetAudioDurationSecondsAsync(mainInput);
                if (durationSeconds <= 0) durationSeconds = 300; // fallback 5 min max

                var durStr = durationSeconds.ToString(CultureInfo.InvariantCulture);
                var volStr = (backgroundMergeProfile.VolumeGainDb ?? 0).ToString(CultureInfo.InvariantCulture) + "dB";
                var filter = $"[1:a]aloop=loop=-1:size=2e9,atrim=0:{durStr},asetpts=N/SR/TB,volume={volStr}[fx];[0:a][fx]amix=inputs=2:duration=first:dropout_transition=0[m]";

                // Merge background với main audio
                var success = await FFmpegCoreHelper.RunFfmpegAsync($"-hide_banner -loglevel error -nostdin -i \"{mainInput}\" -i \"{bgInput}\" -filter_complex \"{filter}\" -map \"[m]\" -c:a libmp3lame -b:a 192k -y \"{mergedOut}\"");
                if (!success || !File.Exists(mergedOut)) return null;

                string finalPath = mergedOut;

                // Áp filterChain nếu có (EQ/mood)
                if (!string.IsNullOrWhiteSpace(filterChain))
                {
                    using var mergedStream = await FFmpegCoreHelper.LoadToMemory(mergedOut);
                    var filteredStream = await _equalizerTuningService.ApplyFilterChain(mergedStream, filterChain);
                    if (filteredStream != null) return filteredStream;
                }

      
                return await FFmpegCoreHelper.LoadToMemory(finalPath);
            }
            catch
            {
                return null;
            }
            finally
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }

    }
}
