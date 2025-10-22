using System.Globalization;
using System.IO.Compression;
using System.Net.Http.Headers;
using PodcastService.Common.AppConfigurations.App.interfaces;
using PodcastService.Infrastructure.Helpers.AudioHelpers;
using PodcastService.Infrastructure.Models.Audio.Tuning;

namespace PodcastService.Infrastructure.Services.Audio.Tuning
{
    public class AITuningService
    {
        private readonly IAppConfig _appConfig;
        public sealed class AITuningResult
        {
            public bool Applied { get; init; }          // Có tách/mix & áp filter
            public bool NotNeeded { get; init; }        // Không cần xử lý (gain == 0)
            public bool Failed { get; init; }           // Lỗi (separation fail)
            public string? Error { get; init; }
            public Stream? fileStream { get; init; }  // Kết quả cuối (nếu Applied)
            public static AITuningResult Skip() => new() { NotNeeded = true };
            public static AITuningResult Fail(string err) => new() { Failed = true, Error = err };
        }

        private readonly EqualizerTuningService _equalizerTuningService;
        public AITuningService(EqualizerTuningService equalizerTuningService, IAppConfig appConfig)
        {
            _equalizerTuningService = equalizerTuningService;
            _appConfig = appConfig;
        }

        public async Task<AITuningResult> ProcessAudioWithAI(Stream fileStream, AITuningProfile aITuningProfile, string? filterChain)
        {
            if (aITuningProfile.UvrMdxNetMainProfile != null)
                return await UvrMdxNetMain(fileStream, aITuningProfile.UvrMdxNetMainProfile, filterChain);
            return AITuningResult.Skip();
        }

        public async Task<AITuningResult> UvrMdxNetMain(Stream fileStream, UvrMdxNetMainProfile uvrMdxNetMainProfile, string? filterChain)
        {
            double voiceGain = uvrMdxNetMainProfile.VoiceGainDb ?? 0;
            double bgGain = uvrMdxNetMainProfile.BackgroundSoundGainDb ?? 0;
            bool needs = Math.Abs(voiceGain) >= 0.0001 || Math.Abs(bgGain) >= 0.0001;

            string apiUrl = _appConfig.AUDIO_SEPARATION_AI_API_URL ?? "http://localhost:8000/separate";

            if (!needs)
                return AITuningResult.Skip();

            var tempDir = Path.Combine(Path.GetTempPath(), "ai_tune_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            var tempInput = Path.Combine(tempDir, "input.mp3");
            try
            {
                using (var fs = new FileStream(tempInput, FileMode.Create, FileAccess.Write, FileShare.None))
                    await fileStream.CopyToAsync(fs);

                var mixedPath = await SeparateAndMixAsync(tempInput, voiceGain, bgGain, apiUrl);
                if (mixedPath == null)
                    return AITuningResult.Fail("Separation/mix failed");

                string finalPath = mixedPath;

                if (!string.IsNullOrWhiteSpace(filterChain))
                {
                    using var stream = new FileStream(mixedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var filteredStream = await _equalizerTuningService.ApplyFilterChain(stream, filterChain);
                    var tempFilteredPath = Path.Combine(Path.GetTempPath(), "filtered_" + Guid.NewGuid().ToString("N") + ".mp3");
                    using (var fs = new FileStream(tempFilteredPath, FileMode.Create, FileAccess.Write))
                    {
                        await filteredStream.CopyToAsync(fs);
                    }
                    finalPath = tempFilteredPath;
                }

    

                return new AITuningResult
                {
                    Applied = true,
                    fileStream = await FFmpegCoreHelper.LoadToMemory(finalPath),
                };
            }
            catch (Exception ex)
            {
                return AITuningResult.Fail(ex.Message);
            }
            finally
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }

        private async Task<string?> SeparateAndMixAsync(string inputPath, double voiceGainDb, double bgGainDb, string apiUrl)
        {

            var workDir = Path.Combine(Path.GetTempPath(), "stems_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(workDir);

            var zipPath = Path.Combine(workDir, "stems.zip");
            try
            {
                using var http = new HttpClient();
                http.Timeout = Timeout.InfiniteTimeSpan;
                
                using var form = new MultipartFormDataContent();
                var fileContent = new StreamContent(File.OpenRead(inputPath));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/mpeg");
                form.Add(fileContent, "file", Path.GetFileName(inputPath));

                using var resp = await http.PostAsync(apiUrl, form);
                if (!resp.IsSuccessStatusCode) return null;

                await using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
                    await resp.Content.CopyToAsync(fs);

                var extractDir = Path.Combine(workDir, "x");
                Directory.CreateDirectory(extractDir);
                ZipFile.ExtractToDirectory(zipPath, extractDir);

                var audioFiles = Directory.GetFiles(extractDir, "*.*", SearchOption.AllDirectories)
                    .Where(f => new[] { ".mp3", ".wav", ".flac", ".m4a", ".ogg" }
                        .Contains(Path.GetExtension(f).ToLowerInvariant())).ToList();
                if (audioFiles.Count < 2) return null;

                string? voice = audioFiles.FirstOrDefault(f => Path.GetFileName(f).ToLower().Contains("voice"))
                                ?? audioFiles[0];
                string? bg = audioFiles.FirstOrDefault(f => Path.GetFileName(f).ToLower().Contains("backgroundsound"))
                             ?? audioFiles.First(f => f != voice);

                var mixedOut = Path.Combine(workDir, "mixed.mp3");
                var vDb = voiceGainDb.ToString(CultureInfo.InvariantCulture) + "dB";
                var bDb = bgGainDb.ToString(CultureInfo.InvariantCulture) + "dB";

                var fc = $"[0:a]volume={vDb}[a0];[1:a]volume={bDb}[a1];[a0][a1]amix=inputs=2:duration=longest:dropout_transition=0[m]";

                await FFmpegCoreHelper.RunFfmpegAsync($"-hide_banner -loglevel error -nostdin -i \"{voice}\" -i \"{bg}\" -vn -filter_complex \"{fc}\" -map \"[m]\" -c:a libmp3lame -b:a 192k -y \"{mixedOut}\"");

                if (!File.Exists(mixedOut)) return null;

                // Copy ra temp file “sạch” rồi dọn
                var final = Path.Combine(Path.GetTempPath(), "mixed_" + Guid.NewGuid().ToString("N") + ".mp3");
                File.Copy(mixedOut, final, true);
                try { Directory.Delete(workDir, true); } catch { }
                return final;
            }
            catch
            {
                try { Directory.Delete(workDir, true); } catch { }
                return null;
            }
        }
    }
}
