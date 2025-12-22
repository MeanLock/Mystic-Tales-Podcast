using Microsoft.Extensions.Logging;
using BookingManagementService.Infrastructure.Configurations.Audio.Hls.interfaces;
using BookingManagementService.Infrastructure.Models.Audio;
using BookingManagementService.Infrastructure.Models.Audio.Hls;
using FFMpegCore;
using FFMpegCore.Enums;
using System.Text;
using BookingManagementService.Infrastructure.Helpers.AudioHelpers;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using BookingManagementService.Common.AppConfigurations.FilePath.interfaces;

namespace BookingManagementService.Infrastructure.Services.Audio.Hls
{
    public class FFMpegCoreHlsService : IDisposable
    {
        private readonly ILogger<FFMpegCoreHlsService> _logger;
        private readonly IHlsConfig _hlsConfig;
        private readonly AudioFormatDetectorHelper _audioFormatDetectorHelper;
        private readonly IWebHostEnvironment _environment;
        private readonly IFilePathConfig _filePathConfig;

        public FFMpegCoreHlsService(ILogger<FFMpegCoreHlsService> logger, IHlsConfig hlsConfig, IWebHostEnvironment environment, IFilePathConfig filePathConfig)
        {
            _logger = logger;
            _hlsConfig = hlsConfig;
            _audioFormatDetectorHelper = new AudioFormatDetectorHelper(logger as ILogger<AudioFormatDetectorHelper>);
            _environment = environment;
            _filePathConfig = filePathConfig;
        }

        public string GetProjectRootPath()
        {
            return AppContext.BaseDirectory;
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
            AudioFormatInfo? formatInfo = null;
            Stream? processStream = null;

            try
            {
                _logger.LogInformation("Starting HLS processing");

                // ✅ STEP 1: Handle non-seekable streams (S3 HashStream, network streams, etc.)
                // Check if stream is seekable
                if (!audioStream.CanSeek)
                {
                    _logger.LogInformation("Input stream is not seekable (likely S3 HashStream or network stream). Copying to MemoryStream...");

                    // Copy to MemoryStream for seekable operations
                    var memoryStream = new MemoryStream();
                    await audioStream.CopyToAsync(memoryStream, cancellationToken);
                    memoryStream.Position = 0; // Reset to beginning

                    processStream = memoryStream;
                    _logger.LogInformation($"Copied {memoryStream.Length} bytes to MemoryStream");
                }
                else
                {
                    _logger.LogInformation("Input stream is seekable, using directly");
                    processStream = audioStream;

                    // Ensure stream is at beginning
                    if (processStream.Position != 0)
                    {
                        processStream.Position = 0;
                    }
                }

                // ✅ STEP 2: Detect audio format from stream (reads first 12 bytes)
                formatInfo = _audioFormatDetectorHelper.DetectFormatFromStream(processStream);

                // After detection, reset position for subsequent reads
                processStream.Position = 0;

                _logger.LogInformation($"Detected audio format: {formatInfo.Format} " +
                                      $"(Extension: {formatInfo.Extension}, " +
                                      $"Lossless: {formatInfo.IsLossless}, " +
                                      $"Codec: {formatInfo.FfmpegCodec})");

                // ✅ STEP 3: Validate format support
                if (!formatInfo.IsSupported)
                {
                    _logger.LogError($"Unsupported audio format detected");
                    return new HlsProcessingResult
                    {
                        Success = false,
                        ErrorMessage = "Unsupported audio format. Supported formats: MP3, AAC, M4A, FLAC, WAV",
                        GeneratedFiles = new List<HlsFile>()
                    };
                }

                // ✅ STEP 4: Log codec strategy
                var strategyDescription = HlsCodecStrategy.GetStrategyDescription(formatInfo);
                var qualityImpact = HlsCodecStrategy.GetQualityImpactDescription(formatInfo);
                _logger.LogInformation($"HLS Codec Strategy: {strategyDescription}");
                _logger.LogInformation($"Quality Impact: {qualityImpact}");

                // Create temporary working directory
                // workingDir = Path.Combine(Path.GetTempPath(), "hls_processing", Guid.NewGuid().ToString());
                workingDir = Path.Combine(_environment.ContentRootPath, _filePathConfig.HLS_PROCESSING_LOCAL_TEMP_FILE_PATH, Guid.NewGuid().ToString());
                Directory.CreateDirectory(workingDir);

                // ✅ STEP 5: Save stream with correct extension (DYNAMIC)
                var tempAudioFile = Path.Combine(workingDir, $"audio{formatInfo.Extension}");
                await SaveStreamToFileAsync(processStream, tempAudioFile, cancellationToken);

                // Get audio duration using optimized FFmpeg/FFprobe
                var audioDurationSeconds = await GetAudioDurationAsync(tempAudioFile, cancellationToken);
                _logger.LogInformation($"Audio duration: {audioDurationSeconds} seconds");

                // Determine segment duration based on audio length
                var segmentDuration = audioDurationSeconds <= _hlsConfig.ShortAudioThresholdSeconds
                    ? _hlsConfig.DefaultShortSegmentSeconds
                    : _hlsConfig.DefaultLongSegmentSeconds;

                _logger.LogInformation($"Using segment duration: {segmentDuration} seconds for audio of {audioDurationSeconds} seconds");

                // ✅ STEP 6: Create HLS segments with format-aware strategy
                var hlsResult = await CreateHlsSegmentsAsync(
                    tempAudioFile,
                    workingDir,
                    segmentDuration,
                    formatInfo,  // ← Pass format info
                    cancellationToken);

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
                // Cleanup copied MemoryStream if created
                if (processStream != null && processStream != audioStream)
                {
                    try
                    {
                        processStream.Dispose();
                        _logger.LogDebug("Disposed copied MemoryStream");
                    }
                    catch { }
                }

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
            AudioFormatInfo inputFormat,  // ← NEW PARAMETER
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

                // Generate unique UID for this batch of segments
                var batchUid = Guid.NewGuid().ToString("N").Substring(0, 8); // Short UID (8 chars)

                // Replace {uid} placeholder with actual UID
                var segmentFileNamePattern = _hlsConfig.SegmentFileNamePattern.Replace("{uid}", batchUid);
                var segmentPattern = Path.Combine(hlsDir, segmentFileNamePattern);

                _logger.LogInformation($"Using segment pattern: {segmentFileNamePattern}");

                // ✅ Run FFmpeg with format-aware codec strategy
                var success = await RunFfmpegHlsConversion(
                    audioFilePath,
                    playlistPath,
                    segmentPattern,
                    keyInfoPath,
                    segmentDuration,
                    inputFormat,  // ← Pass format info
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
                var keyFileName = _hlsConfig.Encryption.KeyFileName;
                var keyFilePath = Path.Combine(hlsDir, keyFileName);
                await File.WriteAllBytesAsync(keyFilePath, encryptionKey, cancellationToken);

                _logger.LogDebug($"Encryption key saved to: {keyFilePath}");

                // Create key info file for FFmpeg
                // Format:
                // Line 1: Key URI (will be in m3u8 playlist)
                // Line 2: Path to key file
                // Line 3: IV (optional, we'll let FFmpeg generate it)
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
        /// Run FFmpeg to convert audio to HLS format with encryption
        /// ✅ FIXED: Use Process directly + skip video streams (-vn flag)
        /// Root cause: FFMpegCore wrapper bug + album art in MP3 causing timestamp issues
        /// </summary>
        private async Task<bool> RunFfmpegHlsConversion(
            string audioFilePath,
            string playlistPath,
            string segmentPattern,
            string keyInfoPath,
            int segmentDuration,
            AudioFormatInfo inputFormat,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Starting FFmpeg HLS conversion for input: {audioFilePath}");

                var (codecArg, bitrateArg, shouldCopyCodec) = HlsCodecStrategy.GetCodecStrategy(inputFormat);
                _logger.LogInformation($"FFmpeg codec strategy: codec={codecArg}, bitrate={bitrateArg ?? "N/A"}");

                // ⭐ Build FFmpeg command manually (bypass FFMpegCore wrapper bug)
                var arguments = new List<string>
        {
            "-i", $"\"{audioFilePath}\"",
            
            // ⭐⭐⭐ CRITICAL FIX: Skip video streams
            // Album art in MP3/M4A files causes FFmpeg to encode video,
            // which makes HLS calculate timestamps based on video (1 frame = 0.000011s)
            // instead of audio duration. This causes #EXTINF:0.000011 bug.
            "-vn",
            
            // Audio codec settings
            "-c:a", codecArg,
            "-b:a", bitrateArg ?? "192k",  // Fallback if null
            "-ar", "44100",
            "-ac", "2",
            
            // HLS format
            "-f", "hls",
            "-hls_time", segmentDuration.ToString(),
            "-hls_list_size", "0",
            "-hls_key_info_file", $"\"{keyInfoPath}\"",
            "-hls_playlist_type", "vod",
            "-hls_flags", "independent_segments",
            "-hls_segment_filename", $"\"{segmentPattern}\"",
            
            // Output
            $"\"{playlistPath}\"",
            "-y"
        };

                var argumentString = string.Join(" ", arguments);
                _logger.LogDebug($"FFmpeg command: ffmpeg {argumentString}");

                // ⭐ Use Process directly instead of FFMpegCore (wrapper has bugs)
                var processStartInfo = new ProcessStartInfo
                {
                    // FileName = string.IsNullOrEmpty(_hlsConfig.FfmpegPath) ? "ffmpeg" : _hlsConfig.FfmpegPath,
                    FileName = "ffmpeg",
                    Arguments = argumentString,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(audioFilePath)
                };

                using var process = new Process { StartInfo = processStartInfo };

                var errorBuilder = new StringBuilder();

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        errorBuilder.AppendLine(e.Data);
                        _logger.LogTrace($"FFmpeg: {e.Data}");
                    }
                };

                process.Start();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(cancellationToken);

                if (process.ExitCode == 0)
                {
                    _logger.LogInformation("FFmpeg HLS conversion completed successfully");
                    return true;
                }
                else
                {
                    _logger.LogError($"FFmpeg failed with exit code {process.ExitCode}");
                    _logger.LogError($"FFmpeg error output:\n{errorBuilder}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running FFmpeg HLS conversion");
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
                    // TemporaryFilesFolder = Path.GetTempPath()
                    TemporaryFilesFolder = Path.Combine(_environment.ContentRootPath, _filePathConfig.HLS_PROCESSING_LOCAL_TEMP_FILE_PATH)
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

        public string GetPlaylistContentAsync(byte[] playlistBytes, string segmentRootPath)
        {
            try
            {
                // Convert byte[] to string using UTF-8 encoding
                var playlistContent = Encoding.UTF8.GetString(playlistBytes);

                // Normalize segmentRootPath (remove trailing slash if exists)
                segmentRootPath = segmentRootPath.TrimEnd('/');

                // Split content into lines
                var lines = playlistContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                var modifiedLines = new List<string>(lines.Length);

                foreach (var line in lines)
                {
                    // Keep empty lines as-is
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        modifiedLines.Add(line);
                        continue;
                    }

                    var trimmedLine = line.Trim();

                    // Check if this is a segment file line (not starting with # and ends with .ts)
                    if (!trimmedLine.StartsWith("#") &&
                        trimmedLine.EndsWith(".ts", StringComparison.OrdinalIgnoreCase))
                    {
                        // Replace segment name with full path
                        var fullSegmentPath = $"{segmentRootPath}/{trimmedLine}";
                        modifiedLines.Add(fullSegmentPath);
                    }
                    else
                    {
                        // Keep metadata lines unchanged (including #EXT-X-KEY, #EXTINF, etc.)
                        modifiedLines.Add(line);
                    }
                }

                // Join lines back with original line endings (Unix-style \n for HLS compatibility)
                return string.Join("\n", modifiedLines);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse and modify playlist content", ex);
            }
        }

        public static string CombinePaths(params string[] pathParts)
        {
            if (pathParts == null || pathParts.Length == 0)
                return string.Empty;

            var result = string.Empty;
            foreach (var part in pathParts)
            {
                if (!string.IsNullOrWhiteSpace(part))
                {
                    var normalizedPart = part.Replace("\\", "/").Trim('/', ' ');
                    if (!string.IsNullOrEmpty(normalizedPart))
                    {
                        result = string.IsNullOrEmpty(result)
                            ? normalizedPart
                            : $"{result}/{normalizedPart}";
                    }
                }
            }
            return result;
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