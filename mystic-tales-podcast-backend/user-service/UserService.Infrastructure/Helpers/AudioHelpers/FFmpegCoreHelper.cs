using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using FFMpegCore;
using FFMpegCore.Arguments;

namespace UserService.Infrastructure.Helpers.AudioHelpers
{
    public class FFmpegCoreHelper
    {
        public static async Task<bool> RunFfmpegAsync(string args)
        {
            try
            {
                // Configure FFOptions tương tự FFMpegCoreHlsService
                var ffOptions = new FFOptions 
                { 
                    BinaryFolder = "", // Let FFMpegCore auto-detect
                    TemporaryFilesFolder = Path.GetTempPath()
                };

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg", // FFMpegCore sẽ tự tìm trong PATH
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processStartInfo };
                process.Start();
                Console.WriteLine($"[DEBUG] FFmpeg started with args: {args}");
                await process.WaitForExitAsync();
                Console.WriteLine($"[DEBUG] FFmpeg exited with code {process.ExitCode}");

                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<string> GetStdOutByFfprobeAsync(string args)
        {
            try
            {
                // Use Process approach for raw FFProbe args with auto-detection
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "ffprobe", // Let FFMpegCore find the path
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processStartInfo };
                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                return output;
            }
            catch
            {
                return "";
            }
        }

        public static async Task<double> GetAudioDurationSecondsAsync(string inputPath)
        {
            try
            {
                // Sử dụng FFMpegCore FFProbe để get duration
                var mediaInfo = await FFProbe.AnalyseAsync(inputPath);
                var seconds = mediaInfo.Duration.TotalSeconds;

                return seconds;
            }
            catch
            {
                return 0d;
            }
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