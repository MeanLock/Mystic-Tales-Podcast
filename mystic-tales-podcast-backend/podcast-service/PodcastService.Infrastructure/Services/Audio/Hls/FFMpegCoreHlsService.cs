using Microsoft.Extensions.Logging;
using PodcastService.Infrastructure.Configurations.Audio.Hls.interfaces;
using PodcastService.Infrastructure.Models.Audio.Hls;
using FFMpegCore;
using FFMpegCore.Enums;

namespace PodcastService.Infrastructure.Services.Audio.Hls
{
    public class FFMpegCoreHlsService : IDisposable
    {
        private readonly ILogger<FFMpegCoreHlsService> _logger;
        private readonly IHlsConfig _hlsConfig;

        public FFMpegCoreHlsService(ILogger<FFMpegCoreHlsService> logger, IHlsConfig hlsConfig)
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
                Console.WriteLine($"path nè: {_hlsConfig.FfmpegPath}");

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
                // Generate encryption key and key info file
                var (keyFilePath, keyInfoPath, keyId) = await CreateEncryptionKeyAsync(hlsDir, cancellationToken);
                
                _logger.LogInformation($"Encryption key generated successfully with ID: {keyId}");

                // Generate HLS segments using FFmpeg with encryption
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

                // Get encryption key file
                var encryptionKeyFile = generatedFiles.FirstOrDefault(f => f.FileType == HlsFileType.EncryptionKey);

                return new HlsProcessingResult
                {
                    Success = true,
                    PlaylistPath = playlistPath,
                    GeneratedFiles = generatedFiles,
                    EncryptionKeyFile = encryptionKeyFile,
                    EncryptionKeyId = keyId,
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
        /// Create encryption key file and key info file for HLS encryption (AES-128)
        /// </summary>
        /// <returns>Tuple of (keyFilePath, keyInfoPath, keyId)</returns>
        private async Task<(string keyFilePath, string keyInfoPath, Guid keyId)> CreateEncryptionKeyAsync(
            string hlsDir, 
            CancellationToken cancellationToken)
        {
            try
            {
                // Generate unique ID for encryption key (will be used as URI and stored in database)
                var keyId = Guid.NewGuid();
                
                _logger.LogDebug($"Generated encryption key ID: {keyId}");

                // Generate random 16-byte (128-bit) encryption key for AES-128
                var encryptionKey = new byte[16];
                using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                {
                    rng.GetBytes(encryptionKey);
                }

                // Save encryption key to file
                var keyFileName = _hlsConfig.Encryption.KeyFileName; //"enc.key"
                var keyFilePath = Path.Combine(hlsDir, keyFileName);
                await File.WriteAllBytesAsync(keyFilePath, encryptionKey, cancellationToken);

                _logger.LogDebug($"Encryption key created: {keyFilePath}");

                // Create key info file for FFmpeg
                // Format:
                // Line 1: Key URI (using GUID - client will use this to request key from API)
                // Line 2: Path to key file (for FFmpeg to read during encoding)
                // Line 3: IV (initialization vector) - optional, using default
                var keyInfoPath = Path.Combine(hlsDir, _hlsConfig.Encryption.KeyInfoFileName);
                var keyInfoContent = $"{keyId}\n{keyFilePath}\n";
                
                await File.WriteAllTextAsync(keyInfoPath, keyInfoContent, cancellationToken);

                _logger.LogDebug($"Key info file created: {keyInfoPath}");

                return (keyFilePath, keyInfoPath, keyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating encryption key");
                throw;
            }
        }

        /// <summary>
        /// Run FFmpeg command with AudioController optimizations using FFMpegCore
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
                _logger.LogDebug($"Running FFMpegCore HLS conversion from {inputFile} to {playlistPath}");

                var ffOptions = new FFOptions 
                { 
                    BinaryFolder = Path.GetDirectoryName(_hlsConfig.FfmpegPath) ?? "",
                    TemporaryFilesFolder = Path.GetTempPath()
                };

                var ffmpegArgs = FFMpegArguments
                    .FromFileInput(inputFile, false, options => options
                        // Input optimizations (from AudioController patterns)
                        .WithCustomArgument("-analyzeduration 10000000") // Analyze more data for better format detection
                        .WithCustomArgument("-probesize 10000000")       // Larger probe size for complex files
                        .WithCustomArgument("-fflags +discardcorrupt+genpts") // Handle corrupt data and generate PTS
                        .WithCustomArgument("-err_detect ignore_err")    // Ignore minor errors
                        .WithCustomArgument("-avoid_negative_ts make_zero")) // Handle timestamp issues
                    .OutputToFile(playlistPath, true, options => options
                        // Audio stream selection and processing (AudioController approach)
                        .WithCustomArgument("-map 0:a:0") // Map first audio stream explicitly
                        .WithAudioCodec(AudioCodec.Aac)
                        .WithAudioBitrate(128)
                        .WithAudioSamplingRate(44100)
                        .WithCustomArgument("-ac 2") // 2 audio channels
                        .WithCustomArgument("-profile:a aac_low") // Use AAC-LC profile
                        
                        // Performance presets (from AudioController)
                        .WithSpeedPreset(Speed.UltraFast) // Fastest encoding preset
                        .WithCustomArgument("-tune zerolatency") // Optimize for low latency
                        .WithCustomArgument("-threads 0") // Use all available CPU cores
                        
                        // HLS specific optimizations
                        .WithCustomArgument($"-hls_time {segmentDuration}")
                        .WithCustomArgument("-hls_list_size 0") // Keep all segments in playlist
                        .WithCustomArgument("-hls_playlist_type vod")
                        .WithCustomArgument("-hls_segment_type mpegts")
                        .WithCustomArgument("-hls_flags independent_segments+temp_file") // Atomic writes
                        
                        // Encryption settings (if keyInfoPath is provided)
                        .WithCustomArgument(!string.IsNullOrEmpty(keyInfoPath) 
                            ? $"-hls_key_info_file \"{keyInfoPath}\"" 
                            : "")
                        
                        // Keyframe settings optimized for speed
                        .WithCustomArgument($"-g {segmentDuration * 2}") // GOP size
                        .WithCustomArgument($"-keyint_min {segmentDuration}") // Minimum keyframe interval
                        .WithCustomArgument("-sc_threshold 0") // Disable scene change detection
                        
                        .WithCustomArgument($"-hls_segment_filename \"{segmentPattern}\"")
                        
                        // Memory and I/O optimizations
                        .WithCustomArgument("-hls_allow_cache 1")
                        .WithCustomArgument("-hls_base_url \"\"") // Empty base URL for relative paths
                        .WithCustomArgument("-bufsize 1M -maxrate 192k") // Buffer optimizations
                        .WithCustomArgument("-movflags +faststart") // Enable fast start for web streaming
                        .WithCustomArgument("-f hls")); // Explicitly specify HLS format

                var success = await ffmpegArgs
                    .CancellableThrough(cancellationToken)
                    .ProcessAsynchronously(throwOnError: false, ffMpegOptions: ffOptions);

                if (success)
                {
                    _logger.LogInformation("FFMpegCore HLS conversion completed successfully");
                    return true;
                }
                else
                {
                    _logger.LogError("FFMpegCore HLS conversion failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running FFMpegCore HLS conversion");
                return false;
            }
        }

        /// <summary>
        /// Get audio duration using FFMpegCore (AudioController approach)
        /// </summary>
        private async Task<double> GetAudioDurationAsync(string audioFilePath, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug($"Getting audio duration for: {audioFilePath}");
                
                var ffOptions = new FFOptions 
                { 
                    BinaryFolder = Path.GetDirectoryName(_hlsConfig.FfmpegPath) ?? "",
                    TemporaryFilesFolder = Path.GetTempPath()
                };
                
                // Use FFMpegCore to analyze the media file
                var mediaInfo = await FFProbe.AnalyseAsync(audioFilePath, ffOptions);
                var duration = mediaInfo.Duration.TotalSeconds;
                
                _logger.LogInformation($"Audio duration detected: {duration} seconds");
                return duration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audio duration with FFMpegCore, using default fallback");
                return 300; // Default fallback (5 minutes)
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

                    // Skip keyinfo.txt (internal file, not needed for storage)
                    if (fileName.Equals("keyinfo.txt", StringComparison.OrdinalIgnoreCase))
                        continue;

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

                _logger.LogDebug($"Collected {files.Count} HLS files: " +
                    $"{files.Count(f => f.FileType == HlsFileType.Playlist)} playlist(s), " +
                    $"{files.Count(f => f.FileType == HlsFileType.Segment)} segment(s), " +
                    $"{files.Count(f => f.FileType == HlsFileType.EncryptionKey)} key(s)");
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
}