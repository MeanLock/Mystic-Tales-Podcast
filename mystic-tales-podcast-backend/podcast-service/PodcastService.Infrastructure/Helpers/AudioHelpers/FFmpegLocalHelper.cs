using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace PodcastService.Infrastructure.Helpers.AudioHelpers
{
    public class FFmpegLocalHelper
    {
        public static async Task<bool> RunFfmpegAsync(string args)
        {
            string ffmpegPath = @"D:\ffmpeg\ffmpeg-2025-09-10-git-c1dc2e2b7c-full_build\bin\ffmpeg.exe";

            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            if (p == null) return false;
            await p.WaitForExitAsync();
            return p.ExitCode == 0;
        }
        
        public static async Task<string> GetStdOutByFfprobeAsync(string args)
        {
            string ffprobePath = @"D:\ffmpeg\ffmpeg-2025-09-10-git-c1dc2e2b7c-full_build\bin\ffprobe.exe";

            var psi = new ProcessStartInfo
            {
                FileName = ffprobePath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            if (p == null) return null;
            string? output = await p.StandardOutput.ReadToEndAsync();
            await p.WaitForExitAsync();
            return output;
        }
        public static async Task<double> GetAudioDurationSecondsAsync(string inputPath)
        {
            try
            {
                var output = await GetStdOutByFfprobeAsync($"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{inputPath}\"");
                if (double.TryParse(output?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
                    return seconds;
                return 0d;
            }
            catch { return 0d; }
        }
        public static async Task<MemoryStream> LoadToMemory(string path)
        {
            var ms = new MemoryStream();
            await using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                await fs.CopyToAsync(ms);
            ms.Position = 0;
            return ms;
        }
        public static async Task WriteStreamToTempfile(Stream input, string path)
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            await input.CopyToAsync(fs);
        }
    }
}
