using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using PodcastService.Infrastructure.Configurations.Audio.Hls.interfaces;
using PodcastService.Infrastructure.Models.Audio.Hls;

namespace PodcastService.Infrastructure.Services.Audio.Hls
{
    public class FFMegLocalHlsService : IDisposable
    {
        private readonly ILogger<FFMegLocalHlsService> _logger;
        private readonly IHlsConfig _hlsConfig;

        public FFMegLocalHlsService(ILogger<FFMegLocalHlsService> logger, IHlsConfig hlsConfig)
        {
            _logger = logger;
            _hlsConfig = hlsConfig;
        }

        /// <summary>
        /// Process audio stream and convert to HLS format
        /// Returns list of generated files for caller to save to storage
        /// </summary>
        /// <param name="audioStream">Input audio stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>HLS processing result with generated files</returns>
        public async Task<HlsProcessingResult> ProcessAudioToHlsAsync(
            Stream audioStream,
            CancellationToken cancellationToken = default)
        {
            string? workingDir = null;

            try
            {
                _logger.LogInformation("Starting HLS processing");

                // Create temporary working directory
                workingDir = Path.Combine(Path.GetTempPath(), "hls_processing", Guid.NewGuid().ToString());
                Directory.CreateDirectory(workingDir);

                // Save stream to temporary audio file
                var tempAudioFile = Path.Combine(workingDir, "audio.mp3");
                await SaveStreamToFileAsync(audioStream, tempAudioFile, cancellationToken);

                // Get audio duration using optimized FFmpeg/FFprobe
                var audioDurationSeconds = await GetAudioDurationAsync(tempAudioFile, cancellationToken);
                _logger.LogInformation($"Audio duration: {audioDurationSeconds} seconds");

                // Determine segment duration based on audio length
                var segmentDuration = audioDurationSeconds <= _hlsConfig.ShortAudioThresholdSeconds
                    ? _hlsConfig.DefaultShortSegmentSeconds
                    : _hlsConfig.DefaultLongSegmentSeconds;

                _logger.LogInformation($"Using segment duration: {segmentDuration} seconds for audio of {audioDurationSeconds} seconds");

                // Create HLS segments
                var hlsResult = await CreateHlsSegmentsAsync(tempAudioFile, workingDir, segmentDuration, cancellationToken);

                _logger.LogInformation("HLS processing completed");
                return hlsResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing audio to HLS");
                return new HlsProcessingResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    GeneratedFiles = new List<HlsFile>()
                };
            }
            finally
            {
                // Cleanup working directory after caller reads the files
                if (!string.IsNullOrEmpty(workingDir) && Directory.Exists(workingDir))
                {
                    try
                    {
                        // Delay cleanup to allow caller to read files
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromMinutes(5), CancellationToken.None);
                            try { Directory.Delete(workingDir, true); } catch { }
                        });
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Create HLS segments from audio file using FFmpeg
        /// </summary>
        private async Task<HlsProcessingResult> CreateHlsSegmentsAsync(
            string audioFilePath,
            string workingDir,
            int segmentDuration,
            CancellationToken cancellationToken)
        {
            var hlsDir = Path.Combine(workingDir, "hls");
            Directory.CreateDirectory(hlsDir);

            var playlistPath = Path.Combine(hlsDir, _hlsConfig.PlaylistFileName);

            try
            {
                // Skip encryption for better performance (as requested)
                string? keyInfoPath = null;
                string? keyFilePath = null;
                // Encryption disabled for performance optimization

                // Generate HLS segments using FFmpeg
                var segmentPattern = Path.Combine(hlsDir, _hlsConfig.SegmentFileNamePattern);
                var success = await RunFfmpegHlsConversion(
                    audioFilePath,
                    playlistPath,
                    segmentPattern,
                    keyInfoPath,
                    segmentDuration,
                    cancellationToken);

                if (!success || !File.Exists(playlistPath))
                {
                    return new HlsProcessingResult
                    {
                        Success = false,
                        ErrorMessage = "FFmpeg HLS conversion failed",
                        GeneratedFiles = new List<HlsFile>()
                    };
                }

                // Collect all generated files
                var generatedFiles = await CollectGeneratedFiles(hlsDir, keyFilePath);

                return new HlsProcessingResult
                {
                    Success = true,
                    PlaylistPath = playlistPath,
                    GeneratedFiles = generatedFiles,
                    IsReused = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating HLS segments");
                return new HlsProcessingResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    GeneratedFiles = new List<HlsFile>()
                };
            }
        }

        /// <summary>
        /// Run FFmpeg command with AudioController optimizations
        /// </summary>
        private async Task<bool> RunFfmpegHlsConversion(
            string inputFile,
            string playlistPath,
            string segmentPattern,
            string? keyInfoPath,
            int segmentDuration,
            CancellationToken cancellationToken)
        {
            try
            {
                var arguments = new StringBuilder();
                
                // Performance optimizations from AudioController
                arguments.Append($"-hide_banner -loglevel error -nostdin");
                arguments.Append($" -threads 0"); // Use all available CPU cores
                
                // Input optimizations (from AudioController patterns)
                arguments.Append($" -analyzeduration 10000000"); // Analyze more data for better format detection
                arguments.Append($" -probesize 10000000");       // Larger probe size for complex files
                arguments.Append($" -fflags +discardcorrupt+genpts"); // Handle corrupt data and generate PTS
                arguments.Append($" -err_detect ignore_err");    // Ignore minor errors
                arguments.Append($" -avoid_negative_ts make_zero"); // Handle timestamp issues
                
                arguments.Append($" -i \"{inputFile}\"");

                // Audio stream selection and processing (AudioController approach)
                arguments.Append($" -map 0:a:0"); // Map first audio stream explicitly
                arguments.Append($" -c:a aac -b:a 128k -ar 44100 -ac 2"); // Consistent audio settings
                arguments.Append($" -profile:a aac_low"); // Use AAC-LC profile
                
                // Performance presets (from AudioController)
                arguments.Append($" -preset ultrafast"); // Fastest encoding preset
                arguments.Append($" -tune zerolatency"); // Optimize for low latency

                // HLS specific optimizations
                arguments.Append($" -hls_time {segmentDuration}");
                arguments.Append($" -hls_list_size 0"); // Keep all segments in playlist
                arguments.Append($" -hls_playlist_type vod");
                arguments.Append($" -hls_segment_type mpegts");
                arguments.Append($" -hls_flags independent_segments+temp_file"); // Atomic writes
                
                // Keyframe settings optimized for speed
                arguments.Append($" -g {segmentDuration * 2}"); // GOP size
                arguments.Append($" -keyint_min {segmentDuration}"); // Minimum keyframe interval
                arguments.Append($" -sc_threshold 0"); // Disable scene change detection
                
                arguments.Append($" -hls_segment_filename \"{segmentPattern}\"");

                // Memory and I/O optimizations
                arguments.Append($" -hls_allow_cache 1");
                arguments.Append($" -hls_base_url \"\""); // Empty base URL for relative paths
                arguments.Append($" -bufsize 1M -maxrate 192k"); // Buffer optimizations
                arguments.Append($" -movflags +faststart"); // Enable fast start for web streaming
                arguments.Append($" -f hls"); // Explicitly specify HLS format

                arguments.Append($" -y \"{playlistPath}\"");

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = _hlsConfig.FfmpegPath,
                    Arguments = arguments.ToString(),
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                _logger.LogDebug($"Running optimized FFmpeg: {processStartInfo.Arguments}");

                using var process = Process.Start(processStartInfo);
                if (process != null)
                {
                    // Set priority like AudioController
                    try
                    {
                        process.PriorityClass = ProcessPriorityClass.AboveNormal;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Could not set process priority: {ex.Message}");
                    }

                    // Timeout handling (from AudioController pattern)
                    var timeout = TimeSpan.FromMinutes(10);
                    var processTask = process.WaitForExitAsync(cancellationToken);
                    var timeoutTask = Task.Delay(timeout, cancellationToken);
                    
                    var completedTask = await Task.WhenAny(processTask, timeoutTask);
                    
                    if (completedTask == timeoutTask)
                    {
                        _logger.LogError("FFmpeg process timed out");
                        try { process.Kill(); } catch { }
                        return false;
                    }

                    var stderr = await process.StandardError.ReadToEndAsync();
                    var stdout = await process.StandardOutput.ReadToEndAsync();

                    if (!string.IsNullOrEmpty(stderr))
                    {
                        _logger.LogWarning($"FFmpeg stderr: {stderr}");
                    }

                    if (process.ExitCode == 0)
                    {
                        _logger.LogInformation("FFmpeg HLS conversion completed successfully");
                        return true;
                    }
                    else
                    {
                        _logger.LogError($"FFmpeg failed with exit code: {process.ExitCode}");
                        return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running FFmpeg HLS conversion");
                return false;
            }
        }

        /// <summary>
        /// Get audio duration with FFprobe/FFmpeg fallback (AudioController approach)
        /// </summary>
        private async Task<double> GetAudioDurationAsync(string audioFilePath, CancellationToken cancellationToken)
        {
            try
            {
                // Try ffprobe first (fastest method like AudioController)
                var ffprobePath = _hlsConfig.FfmpegPath.Replace("ffmpeg.exe", "ffprobe.exe");
                if (File.Exists(ffprobePath))
                {
                    var arguments = $"-v quiet -show_entries format=duration -of csv=p=0 \"{audioFilePath}\"";
                    
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = ffprobePath,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(processStartInfo);
                    if (process != null)
                    {
                        var timeoutTask = Task.Delay(5000, cancellationToken);
                        var processTask = process.WaitForExitAsync(cancellationToken);
                        
                        var completedTask = await Task.WhenAny(processTask, timeoutTask);
                        
                        if (completedTask != timeoutTask)
                        {
                            var stdout = await process.StandardOutput.ReadToEndAsync();
                            
                            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(stdout))
                            {
                                var durationStr = stdout.Trim();
                                if (double.TryParse(durationStr, System.Globalization.NumberStyles.Float,
                                    System.Globalization.CultureInfo.InvariantCulture, out var duration))
                                {
                                    _logger.LogInformation($"Audio duration detected: {duration} seconds");
                                    return duration;
                                }
                            }
                        }
                        else
                        {
                            try { process.Kill(); } catch { }
                        }
                    }
                }

                // Fallback to FFmpeg method (AudioController fallback pattern)
                var ffmpegArgs = $"-hide_banner -nostdin -i \"{audioFilePath}\" -f null -t 1 -";
                var ffmpegStartInfo = new ProcessStartInfo
                {
                    FileName = _hlsConfig.FfmpegPath,
                    Arguments = ffmpegArgs,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var ffmpegProcess = Process.Start(ffmpegStartInfo);
                if (ffmpegProcess != null)
                {
                    var timeoutTask = Task.Delay(5000, cancellationToken);
                    var processTask = ffmpegProcess.WaitForExitAsync(cancellationToken);
                    
                    var completedTask = await Task.WhenAny(processTask, timeoutTask);
                    
                    if (completedTask != timeoutTask)
                    {
                        var stderr = await ffmpegProcess.StandardError.ReadToEndAsync();
                        var durationMatch = System.Text.RegularExpressions.Regex.Match(
                            stderr, @"Duration:\s*(\d{2}):(\d{2}):(\d{2}\.?\d*)");
                            
                        if (durationMatch.Success)
                        {
                            var hours = int.Parse(durationMatch.Groups[1].Value);
                            var minutes = int.Parse(durationMatch.Groups[2].Value);
                            var seconds = double.Parse(durationMatch.Groups[3].Value, 
                                System.Globalization.CultureInfo.InvariantCulture);
                                
                            var totalSeconds = hours * 3600 + minutes * 60 + seconds;
                            _logger.LogInformation($"Audio duration detected (FFmpeg): {totalSeconds} seconds");
                            return totalSeconds;
                        }
                    }
                    else
                    {
                        try { ffmpegProcess.Kill(); } catch { }
                    }
                }

                // Default fallback (AudioController approach)
                _logger.LogWarning("Could not detect audio duration, using default");
                return 300; // Default to 5 minutes
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audio duration");
                return 300; // Default fallback
            }
        }

        /// <summary>
        /// Collect all generated HLS files for caller to save
        /// </summary>
        private async Task<List<HlsFile>> CollectGeneratedFiles(string hlsDir, string? keyFilePath)
        {
            var files = new List<HlsFile>();

            try
            {
                var allFiles = Directory.GetFiles(hlsDir, "*.*", SearchOption.TopDirectoryOnly);

                foreach (var filePath in allFiles)
                {
                    var fileName = Path.GetFileName(filePath);
                    var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
                    var fileInfo = new FileInfo(filePath);

                    var fileType = fileExtension switch
                    {
                        ".m3u8" => HlsFileType.Playlist,
                        ".ts" => HlsFileType.Segment,
                        ".key" => HlsFileType.EncryptionKey,
                        _ => HlsFileType.Segment
                    };

                    var fileContent = await File.ReadAllBytesAsync(filePath);

                    files.Add(new HlsFile
                    {
                        FileName = fileName,
                        FilePath = filePath,
                        FileType = fileType,
                        FileSize = fileInfo.Length,
                        FileContent = fileContent
                    });
                }

                _logger.LogDebug($"Collected {files.Count} HLS files: {files.Count(f => f.FileType == HlsFileType.Playlist)} playlist(s), {files.Count(f => f.FileType == HlsFileType.Segment)} segment(s)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting generated HLS files");
            }

            return files;
        }

        /// <summary>
        /// Save stream content to file
        /// </summary>
        private async Task SaveStreamToFileAsync(Stream stream, string filePath, CancellationToken cancellationToken)
        {
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fileStream, cancellationToken);
        }

        public void Dispose()
        {
            // Cleanup resources if needed
        }
    }

    // /// <summary>
    // /// Result object for HLS processing operations
    // /// </summary>
    // public class HlsProcessingResult
    // {
    //     public bool Success { get; set; }
    //     public string? PlaylistPath { get; set; }
    //     public string? EncodedPath { get; set; }
    //     public bool IsReused { get; set; }
    //     public string? ErrorMessage { get; set; }
    //     public List<HlsFile> GeneratedFiles { get; set; } = new List<HlsFile>();
    // }

    // /// <summary>
    // /// Represents a single HLS file (playlist or segment)
    // /// </summary>
    // public class HlsFile
    // {
    //     public string FileName { get; set; } = string.Empty;
    //     public string FilePath { get; set; } = string.Empty;
    //     public HlsFileType FileType { get; set; }
    //     public long FileSize { get; set; }
    //     public byte[] FileContent { get; set; } = Array.Empty<byte>();
    // }

    // /// <summary>
    // /// Types of HLS files
    // /// </summary>
    // public enum HlsFileType
    // {
    //     Playlist,       // .m3u8 file
    //     Segment,        // .ts file
    //     EncryptionKey   // .key file
    // }
}