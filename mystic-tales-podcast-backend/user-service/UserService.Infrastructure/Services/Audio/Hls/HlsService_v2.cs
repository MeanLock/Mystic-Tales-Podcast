// using Microsoft.Extensions.Logging;
// using System.Diagnostics;
// using System.Security.Cryptography;
// using System.Text;
// using UserService.Infrastructure.Configurations.Audio.Hls.interfaces;

// namespace UserService.Infrastructure.Services.Audio.Hls
// {
//     public class HlsService : IDisposable
//     {
//         private readonly ILogger<HlsService> _logger;
//         private readonly IHlsConfig _hlsConfig;

//         public HlsService(ILogger<HlsService> logger, IHlsConfig hlsConfig)
//         {
//             _logger = logger;
//             _hlsConfig = hlsConfig;
//         }

//         /// <summary>
//         /// Process audio stream and convert to HLS format
//         /// Returns list of generated files for caller to save to storage
//         /// </summary>
//         /// <param name="audioStream">Input audio stream</param>
//         /// <param name="cancellationToken">Cancellation token</param>
//         /// <returns>HLS processing result with generated files</returns>
//         public async Task<HlsProcessingResult> ProcessAudioToHlsAsync(
//             Stream audioStream,
//             CancellationToken cancellationToken = default)
//         {
//             string? workingDir = null;

//             try
//             {
//                 _logger.LogInformation("Starting HLS processing");

//                 // Create temporary working directory
//                 workingDir = Path.Combine(Path.GetTempPath(), "hls_processing", Guid.NewGuid().ToString());
//                 Directory.CreateDirectory(workingDir);

//                 // Save stream to temporary audio file
//                 var tempAudioFile = Path.Combine(workingDir, "audio.mp3");
//                 await SaveStreamToFileAsync(audioStream, tempAudioFile, cancellationToken);

//                 // Get audio duration using FFmpeg
//                 var audioDurationSeconds = await GetAudioDurationAsync(tempAudioFile, cancellationToken);
//                 _logger.LogInformation($"Audio duration: {audioDurationSeconds} seconds");

//                 // Determine segment duration based on audio length
//                 var segmentDuration = audioDurationSeconds <= _hlsConfig.ShortAudioThresholdSeconds
//                     ? _hlsConfig.DefaultShortSegmentSeconds
//                     : _hlsConfig.DefaultLongSegmentSeconds;

//                 _logger.LogInformation($"Using segment duration: {segmentDuration} seconds for audio of {audioDurationSeconds} seconds");

//                 // Create HLS segments
//                 var hlsResult = await CreateHlsSegmentsAsync(tempAudioFile, workingDir, segmentDuration, cancellationToken);

//                 _logger.LogInformation("HLS processing completed");
//                 return hlsResult;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error processing audio to HLS");
//                 return new HlsProcessingResult
//                 {
//                     Success = false,
//                     ErrorMessage = ex.Message,
//                     GeneratedFiles = new List<HlsFile>()
//                 };
//             }
//             finally
//             {
//                 // Cleanup working directory after caller reads the files
//                 if (!string.IsNullOrEmpty(workingDir) && Directory.Exists(workingDir))
//                 {
//                     try
//                     {
//                         // Delay cleanup to allow caller to read files
//                         _ = Task.Run(async () =>
//                         {
//                             await Task.Delay(TimeSpan.FromMinutes(5), CancellationToken.None);
//                             try { Directory.Delete(workingDir, true); } catch { }
//                         });
//                     }
//                     catch { }
//                 }
//             }
//         }

//         /// <summary>
//         /// Create HLS segments from audio file using FFmpeg
//         /// </summary>
//         private async Task<HlsProcessingResult> CreateHlsSegmentsAsync(
//             string audioFilePath,
//             string workingDir,
//             int segmentDuration,
//             CancellationToken cancellationToken)
//         {
//             var hlsDir = Path.Combine(workingDir, "hls");
//             Directory.CreateDirectory(hlsDir);

//             var playlistPath = Path.Combine(hlsDir, _hlsConfig.PlaylistName);

//             try
//             {
//                 // Skip encryption for better performance (as requested)
//                 string? keyInfoPath = null;
//                 string? keyFilePath = null;
//                 // Encryption disabled for performance optimization

//                 // Generate HLS segments using FFmpeg
//                 var segmentPattern = Path.Combine(hlsDir, _hlsConfig.SegmentFilePattern);
//                 var success = await RunFfmpegHlsConversion(
//                     audioFilePath,
//                     playlistPath,
//                     segmentPattern,
//                     keyInfoPath,
//                     segmentDuration,
//                     cancellationToken);

//                 if (!success || !File.Exists(playlistPath))
//                 {
//                     return new HlsProcessingResult
//                     {
//                         Success = false,
//                         ErrorMessage = "FFmpeg HLS conversion failed",
//                         GeneratedFiles = new List<HlsFile>()
//                     };
//                 }

//                 // Collect all generated files
//                 var generatedFiles = await CollectGeneratedFiles(hlsDir, keyFilePath);

//                 return new HlsProcessingResult
//                 {
//                     Success = true,
//                     PlaylistPath = playlistPath,
//                     GeneratedFiles = generatedFiles,
//                     IsReused = false
//                 };
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error creating HLS segments");
//                 return new HlsProcessingResult
//                 {
//                     Success = false,
//                     ErrorMessage = ex.Message,
//                     GeneratedFiles = new List<HlsFile>()
//                 };
//             }
//         }


//         private async Task<bool> RunFfmpegHlsConversion(
//     string inputFile,
//     string playlistPath,
//     string segmentPattern,
//     string? keyInfoPath,
//     int segmentDuration,
//     CancellationToken cancellationToken)
//         {
//             try
//             {
//                 var arguments = new StringBuilder();
                
//                 // Performance optimizations
//                 arguments.Append($"-hide_banner -loglevel error -nostdin");
//                 arguments.Append($" -threads 0"); // Use all available CPU cores
//                 arguments.Append($" -fflags +genpts+discardcorrupt"); // Generate PTS and handle corrupt data
//                 arguments.Append($" -avoid_negative_ts make_zero"); // Handle timestamp issues
//                 arguments.Append($" -i \"{inputFile}\"");

//                 // Fast audio processing settings - optimized for speed
//                 arguments.Append($" -c:a aac -b:a 128k -ar 44100 -ac 2"); // Reduced bitrate for faster processing
//                 arguments.Append($" -profile:a aac_low"); // Use AAC-LC profile (faster)
//                 arguments.Append($" -preset ultrafast"); // Fastest encoding preset
//                 arguments.Append($" -tune zerolatency"); // Optimize for low latency

//                 // HLS specific optimizations
//                 arguments.Append($" -hls_time {segmentDuration}");
//                 arguments.Append($" -hls_list_size 0"); // Keep all segments in playlist
//                 arguments.Append($" -hls_playlist_type vod");
//                 arguments.Append($" -hls_segment_type mpegts");
//                 arguments.Append($" -hls_flags independent_segments+temp_file"); // Use temp files for atomic writes
                
//                 // Optimized keyframe settings - faster than force_key_frames
//                 arguments.Append($" -g {segmentDuration * 2}"); // GOP size based on segment duration
//                 arguments.Append($" -keyint_min {segmentDuration}"); // Minimum keyframe interval
//                 arguments.Append($" -sc_threshold 0"); // Disable scene change detection for consistent segments
                
//                 arguments.Append($" -hls_segment_filename \"{segmentPattern}\"");

//                 // Memory and I/O optimizations
//                 arguments.Append($" -hls_allow_cache 1");
//                 arguments.Append($" -hls_base_url \"\""); // Empty base URL for relative paths
//                 arguments.Append($" -bufsize 1M -maxrate 192k"); // Buffer optimizations
//                 arguments.Append($" -f hls"); // Explicitly specify HLS format

//                 // Skip encryption key for performance (as requested)
//                 // No encryption key handling

//                 arguments.Append($" -y \"{playlistPath}\"");

//                 var processStartInfo = new ProcessStartInfo
//                 {
//                     FileName = _hlsConfig.FfmpegPath,
//                     Arguments = arguments.ToString(),
//                     RedirectStandardError = true,
//                     RedirectStandardOutput = true,
//                     UseShellExecute = false,
//                     CreateNoWindow = true,
//                     WindowStyle = ProcessWindowStyle.Hidden
//                 };

//                 _logger.LogDebug($"Running optimized FFmpeg: {processStartInfo.FileName} {processStartInfo.Arguments}");

//                 using var process = Process.Start(processStartInfo);
//                 if (process != null)
//                 {
//                     // Set high priority for faster processing
//                     try
//                     {
//                         process.PriorityClass = ProcessPriorityClass.AboveNormal;
//                     }
//                     catch (Exception ex)
//                     {
//                         _logger.LogWarning($"Could not set process priority: {ex.Message}");
//                     }

//                     // Use timeout to avoid hanging processes
//                     var timeout = TimeSpan.FromMinutes(10);
//                     var processTask = process.WaitForExitAsync(cancellationToken);
//                     var timeoutTask = Task.Delay(timeout, cancellationToken);
                    
//                     var completedTask = await Task.WhenAny(processTask, timeoutTask);
                    
//                     if (completedTask == timeoutTask)
//                     {
//                         _logger.LogError("FFmpeg process timed out");
//                         try { process.Kill(); } catch { }
//                         return false;
//                     }

//                     var stderr = await process.StandardError.ReadToEndAsync();
//                     var stdout = await process.StandardOutput.ReadToEndAsync();

//                     if (!string.IsNullOrEmpty(stderr))
//                     {
//                         _logger.LogWarning($"FFmpeg stderr: {stderr}");
//                     }

//                     if (!string.IsNullOrEmpty(stdout))
//                     {
//                         _logger.LogDebug($"FFmpeg stdout: {stdout}");
//                     }

//                     if (process.ExitCode == 0)
//                     {
//                         // Skip playlist validation for better performance
//                         // await ValidateAndFixPlaylist(playlistPath, cancellationToken);
//                         _logger.LogInformation("FFmpeg HLS conversion completed successfully");
//                         return true;
//                     }
//                     else
//                     {
//                         _logger.LogError($"FFmpeg failed with exit code: {process.ExitCode}");
//                         return false;
//                     }
//                 }

//                 return false;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error running FFmpeg HLS conversion");
//                 return false;
//             }
//         }

//         /// <summary>
//         /// Validate and fix playlist EXTINF tags if necessary
//         /// </summary>
//         private async Task ValidateAndFixPlaylist(string playlistPath, CancellationToken cancellationToken)
//         {
//             try
//             {
//                 if (!File.Exists(playlistPath))
//                     return;

//                 var lines = await File.ReadAllLinesAsync(playlistPath, cancellationToken);
//                 Console.WriteLine($"Playlist file pathhhhhhhhhhhh: {lines.ToString()}");
//                 var modifiedLines = new List<string>();
//                 var needsModification = false;

//                 for (int i = 0; i < lines.Length; i++)
//                 {
//                     var line = lines[i];

//                     if (line.StartsWith("#EXTINF:"))
//                     {
//                         // Parse EXTINF line: #EXTINF:6.000000,
//                         var match = System.Text.RegularExpressions.Regex.Match(line, @"#EXTINF:(\d+\.?\d*),(.*)");
//                         if (match.Success)
//                         {
//                             var duration = match.Groups[1].Value;
//                             var title = match.Groups[2].Value;

//                             // Ensure proper formatting with 6 decimal places
//                             if (double.TryParse(duration, System.Globalization.NumberStyles.Float,
//                                 System.Globalization.CultureInfo.InvariantCulture, out var durationValue))
//                             {
//                                 var formattedDuration = durationValue.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
//                                 var newLine = $"#EXTINF:{formattedDuration},{title}";

//                                 if (newLine != line)
//                                 {
//                                     needsModification = true;
//                                     modifiedLines.Add(newLine);
//                                     _logger.LogDebug($"Fixed EXTINF: {line} -> {newLine}");
//                                 }
//                                 else
//                                 {
//                                     modifiedLines.Add(line);
//                                 }
//                             }
//                             else
//                             {
//                                 modifiedLines.Add(line);
//                             }
//                         }
//                         else
//                         {
//                             modifiedLines.Add(line);
//                         }
//                     }
//                     else
//                     {
//                         modifiedLines.Add(line);
//                     }
//                 }

//                 if (needsModification)
//                 {
//                     await File.WriteAllLinesAsync(playlistPath, modifiedLines, cancellationToken);
//                     _logger.LogInformation("Fixed EXTINF tags in HLS playlist");
//                 }

//                 // Log playlist content for debugging
//                 _logger.LogDebug($"HLS Playlist content:\n{string.Join("\n", modifiedLines)}");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error validating/fixing HLS playlist");
//             }
//         }

//         /// <summary>
//         /// Create encryption key and key info file for HLS encryption
//         /// </summary>
//         private async Task<(string keyInfoPath, string keyFilePath)> CreateEncryptionKeyAsync(string hlsDir, CancellationToken cancellationToken)
//         {
//             // Generate 16-byte AES key
//             var keyBytes = RandomNumberGenerator.GetBytes(16);
//             var keyFile = Path.Combine(hlsDir, _hlsConfig.Encryption.KeyFile);
//             await File.WriteAllBytesAsync(keyFile, keyBytes, cancellationToken);

//             // Create key info file (FFmpeg format)
//             var keyInfoFile = Path.Combine(hlsDir, _hlsConfig.Encryption.KeyInfoFile);
//             var keyUrl = $"file://{keyFile.Replace('\\', '/')}"; // Use file:// URL for local key
//             var keyInfoContent = $"{keyUrl}\n{keyFile.Replace('\\', '/')}\n";
//             await File.WriteAllTextAsync(keyInfoFile, keyInfoContent, cancellationToken);

//             return (keyInfoFile, keyFile);
//         }

//         /// <summary>
//         /// Collect all generated HLS files for caller to save
//         /// </summary>
//         private async Task<List<HlsFile>> CollectGeneratedFiles(string hlsDir, string? keyFilePath)
//         {
//             var files = new List<HlsFile>();

//             try
//             {
//                 // Get all files in HLS directory
//                 var allFiles = Directory.GetFiles(hlsDir, "*.*", SearchOption.TopDirectoryOnly);

//                 foreach (var filePath in allFiles)
//                 {
//                     var fileName = Path.GetFileName(filePath);
//                     var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
//                     var fileInfo = new FileInfo(filePath);

//                     // Determine file type
//                     var fileType = fileExtension switch
//                     {
//                         ".m3u8" => HlsFileType.Playlist,
//                         ".ts" => HlsFileType.Segment,
//                         ".key" => HlsFileType.EncryptionKey,
//                         _ => HlsFileType.Segment // Default fallback
//                     };

//                     // Read file content
//                     var fileContent = await File.ReadAllBytesAsync(filePath);

//                     files.Add(new HlsFile
//                     {
//                         FileName = fileName,
//                         FilePath = filePath,
//                         FileType = fileType,
//                         FileSize = fileInfo.Length,
//                         FileContent = fileContent
//                     });
//                 }

//                 _logger.LogDebug($"Collected {files.Count} HLS files: {files.Count(f => f.FileType == HlsFileType.Playlist)} playlist(s), {files.Count(f => f.FileType == HlsFileType.Segment)} segment(s), {files.Count(f => f.FileType == HlsFileType.EncryptionKey)} key(s)");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error collecting generated HLS files");
//             }

//             return files;
//         }

//         /// <summary>
//         /// Save stream content to file
//         /// </summary>
//         private async Task SaveStreamToFileAsync(Stream stream, string filePath, CancellationToken cancellationToken)
//         {
//             using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
//             await stream.CopyToAsync(fileStream, cancellationToken);
//         }

//         /// <summary>
//         /// Get audio duration in seconds using FFmpeg
//         /// </summary>
//         // private async Task<double> GetAudioDurationAsync(string audioFilePath, CancellationToken cancellationToken)
//         // {
//         //     try
//         //     {
//         //         // Use ffprobe instead of ffmpeg for better duration detection
//         //         var arguments = $"-v quiet -show_entries format=duration -of csv=p=0 \"{audioFilePath}\"";

//         //         var processStartInfo = new ProcessStartInfo
//         //         {
//         //             FileName = _hlsConfig.FfmpegPath.Replace("ffmpeg.exe", "ffprobe.exe"), // Use ffprobe
//         //             Arguments = arguments,
//         //             RedirectStandardOutput = true,
//         //             RedirectStandardError = true,
//         //             UseShellExecute = false,
//         //             CreateNoWindow = true
//         //         };

//         //         _logger.LogDebug($"Getting audio duration with FFprobe: {processStartInfo.FileName} {arguments}");

//         //         using var process = Process.Start(processStartInfo);
//         //         if (process != null)
//         //         {
//         //             var stdout = await process.StandardOutput.ReadToEndAsync();
//         //             var stderr = await process.StandardError.ReadToEndAsync();
//         //             await process.WaitForExitAsync(cancellationToken);

//         //             _logger.LogDebug($"FFprobe stdout: {stdout}");
//         //             if (!string.IsNullOrEmpty(stderr))
//         //             {
//         //                 _logger.LogWarning($"FFprobe stderr: {stderr}");
//         //             }

//         //             if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(stdout))
//         //             {
//         //                 var durationStr = stdout.Trim();
//         //                 if (double.TryParse(durationStr, System.Globalization.NumberStyles.Float, 
//         //                     System.Globalization.CultureInfo.InvariantCulture, out var duration))
//         //                 {
//         //                     _logger.LogInformation($"Parsed audio duration: {duration} seconds");
//         //                     return duration;
//         //                 }
//         //             }

//         //             // Fallback to FFmpeg method if ffprobe fails
//         //             _logger.LogWarning("FFprobe failed, falling back to FFmpeg method");
//         //             return await GetAudioDurationWithFfmpegFallback(audioFilePath, cancellationToken);
//         //         }

//         //         _logger.LogWarning("Could not start FFprobe process");
//         //         return await GetAudioDurationWithFfmpegFallback(audioFilePath, cancellationToken);
//         //     }
//         //     catch (Exception ex)
//         //     {
//         //         _logger.LogError(ex, "Error getting audio duration with FFprobe");
//         //         return await GetAudioDurationWithFfmpegFallback(audioFilePath, cancellationToken);
//         //     }
//         // }

//         /// <summary>
//         /// Optimized audio duration detection - faster method
//         /// </summary>
//         private async Task<double> GetAudioDurationAsync(string audioFilePath, CancellationToken cancellationToken)
//         {
//             try
//             {
//                 // Try ffprobe first (fastest method)
//                 var ffprobePath = _hlsConfig.FfmpegPath.Replace("ffmpeg.exe", "ffprobe.exe");
//                 if (File.Exists(ffprobePath))
//                 {
//                     var duration = await GetDurationWithFfprobe(ffprobePath, audioFilePath, cancellationToken);
//                     if (duration > 0)
//                     {
//                         return duration;
//                     }
//                 }

//                 // Fallback to fast FFmpeg method
//                 return await GetDurationWithFfmpeg(audioFilePath, cancellationToken);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error getting audio duration");
//                 return 300; // Default to 5 minutes if detection fails
//             }
//         }

//         private async Task<double> GetDurationWithFfprobe(string ffprobePath, string audioFilePath, CancellationToken cancellationToken)
//         {
//             try
//             {
//                 var arguments = $"-v quiet -show_entries format=duration -of csv=p=0 \"{audioFilePath}\"";
                
//                 var processStartInfo = new ProcessStartInfo
//                 {
//                     FileName = ffprobePath,
//                     Arguments = arguments,
//                     RedirectStandardOutput = true,
//                     RedirectStandardError = true,
//                     UseShellExecute = false,
//                     CreateNoWindow = true
//                 };

//                 using var process = Process.Start(processStartInfo);
//                 if (process != null)
//                 {
//                     // Quick timeout for duration detection
//                     var timeoutTask = Task.Delay(5000, cancellationToken); // 5 second timeout
//                     var processTask = process.WaitForExitAsync(cancellationToken);
                    
//                     var completedTask = await Task.WhenAny(processTask, timeoutTask);
                    
//                     if (completedTask == timeoutTask)
//                     {
//                         try { process.Kill(); } catch { }
//                         return 0;
//                     }

//                     var stdout = await process.StandardOutput.ReadToEndAsync();
                    
//                     if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(stdout))
//                     {
//                         var durationStr = stdout.Trim();
//                         if (double.TryParse(durationStr, System.Globalization.NumberStyles.Float,
//                             System.Globalization.CultureInfo.InvariantCulture, out var duration))
//                         {
//                             _logger.LogInformation($"Audio duration detected: {duration} seconds");
//                             return duration;
//                         }
//                     }
//                 }
                
//                 return 0;
//             }
//             catch
//             {
//                 return 0;
//             }
//         }

//         private async Task<double> GetDurationWithFfmpeg(string audioFilePath, CancellationToken cancellationToken)
//         {
//             try
//             {
//                 // Faster FFmpeg method - only get format info
//                 var arguments = $"-hide_banner -nostdin -i \"{audioFilePath}\" -f null -t 1 -";

//                 var processStartInfo = new ProcessStartInfo
//                 {
//                     FileName = _hlsConfig.FfmpegPath,
//                     Arguments = arguments,
//                     RedirectStandardOutput = false, // Don't need stdout
//                     RedirectStandardError = true,   // Duration info is in stderr
//                     UseShellExecute = false,
//                     CreateNoWindow = true
//                 };

//                 using var process = Process.Start(processStartInfo);
//                 if (process != null)
//                 {
//                     var timeoutTask = Task.Delay(5000, cancellationToken);
//                     var processTask = process.WaitForExitAsync(cancellationToken);
                    
//                     var completedTask = await Task.WhenAny(processTask, timeoutTask);
                    
//                     if (completedTask == timeoutTask)
//                     {
//                         try { process.Kill(); } catch { }
//                         return 0;
//                     }

//                     var stderr = await process.StandardError.ReadToEndAsync();

//                     // Parse duration from stderr
//                     var durationMatch = System.Text.RegularExpressions.Regex.Match(
//                         stderr, @"Duration:\s*(\d{2}):(\d{2}):(\d{2}\.?\d*)");
                        
//                     if (durationMatch.Success)
//                     {
//                         var hours = int.Parse(durationMatch.Groups[1].Value);
//                         var minutes = int.Parse(durationMatch.Groups[2].Value);
//                         var seconds = double.Parse(durationMatch.Groups[3].Value, 
//                             System.Globalization.CultureInfo.InvariantCulture);
                            
//                         var totalSeconds = hours * 3600 + minutes * 60 + seconds;
//                         _logger.LogInformation($"Audio duration detected (FFmpeg): {totalSeconds} seconds");
//                         return totalSeconds;
//                     }
//                 }

//                 return 0;
//             }
//             catch
//             {
//                 return 0;
//             }
//         }

//         /// <summary>
//         /// Create success result object
//         /// </summary>
//         private HlsProcessingResult CreateSuccessResult(string playlistPath, bool reused)
//         {
//             var encodedPath = Convert.ToBase64String(Encoding.UTF8.GetBytes(playlistPath));

//             return new HlsProcessingResult
//             {
//                 Success = true,
//                 PlaylistPath = playlistPath,
//                 EncodedPath = encodedPath,
//                 IsReused = reused
//             };
//         }

//         public void Dispose()
//         {
//             // Cleanup resources if needed
//         }
//     }

//     /// <summary>
//     /// Result object for HLS processing operations
//     /// </summary>
//     public class HlsProcessingResult
//     {
//         public bool Success { get; set; }
//         public string? PlaylistPath { get; set; }
//         public string? EncodedPath { get; set; }
//         public bool IsReused { get; set; }
//         public string? ErrorMessage { get; set; }
//         public List<HlsFile> GeneratedFiles { get; set; } = new List<HlsFile>();
//     }

//     /// <summary>
//     /// Represents a single HLS file (playlist or segment)
//     /// </summary>
//     public class HlsFile
//     {
//         public string FileName { get; set; } = string.Empty;
//         public string FilePath { get; set; } = string.Empty;
//         public HlsFileType FileType { get; set; }
//         public long FileSize { get; set; }
//         public byte[] FileContent { get; set; } = Array.Empty<byte>();
//     }

//     /// <summary>
//     /// Types of HLS files
//     /// </summary>
//     public enum HlsFileType
//     {
//         Playlist,       // .m3u8 file
//         Segment,        // .ts file
//         EncryptionKey   // .key file
//     }
// }
