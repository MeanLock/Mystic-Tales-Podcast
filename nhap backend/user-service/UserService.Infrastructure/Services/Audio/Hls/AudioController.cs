using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;

namespace AudioEQUploader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AudioController : ControllerBase
    {
        [HttpPost("export")]
        [Consumes("multipart/form-data")]
        [RequestFormLimits(MultipartBodyLengthLimit = 200_000_000)]
        [RequestSizeLimit(200_000_000)]
        public async Task<IActionResult> Export([FromForm] AudioExportRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("File not found");

            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            Directory.CreateDirectory(uploads);

            var inputPath = Path.Combine(uploads, request.File.FileName);
            using (var stream = new FileStream(inputPath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            // Determine session directory for per-session cache
            var sessionId = request.SessionId;
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                sessionId = Guid.NewGuid().ToString("N"); // Sinh SessionId ngẫu nhiên
                HttpContext.Response.Headers.Add("X-Session-Id", sessionId); // Trả về cho client
            }
            Console.WriteLine($"[DEBUG] Using session ID: {sessionId}");
            var sessionDir = Path.Combine(uploads, "sessions", sessionId);
            if (!Directory.Exists(sessionDir))
            {
                Directory.CreateDirectory(sessionDir);
                Console.WriteLine($"[DEBUG] Created session directory: {sessionDir}");
            }
            else
            {
                Console.WriteLine($"[DEBUG] Session directory already exists: {sessionDir}");
            }
            // Build filter chain theo thứ tự: EQ band -> Mood processing
            string filterChain = BuildAudioFilterChain(request);
            Console.WriteLine($"[DEBUG] Filter chain: {filterChain}");
            string ffmpegPath = @"D:\ffmpeg\ffmpeg-2025-09-10-git-c1dc2e2b7c-full_build\bin\ffmpeg.exe";
            string? ffprobePath = Path.Combine(Path.GetDirectoryName(ffmpegPath) ?? "", "ffprobe.exe");

            // Load previous cache for this session
            var previousPath = Path.Combine(sessionDir, "previous.json");
            var previous = await LoadPreviousAsync(previousPath);

            // Compute hash of current original file
            string fileHash = ComputeSha1(inputPath);

            // If session folder exists but file changed -> reset folder to overwrite cache
            if (previous != null && previous.fileHash != fileHash)
            {
                Console.WriteLine($"[DEBUG] Session '{sessionId}' fileHash changed (prev={previous.fileHash}, new={fileHash}). Clearing session folder to overwrite.");
                TryDeleteDirectory(sessionDir);
                Directory.CreateDirectory(sessionDir);
                previous = null; // drop previous to avoid stale reuse
            }
            // 0. Nếu cần tách giọng/nhạc bằng Python API thì đi nhánh này
            bool wantsVoiceBgAdjust = Math.Abs(request.VoiceGainDb) >= 0.0001 || Math.Abs(request.BackgroundSoundGainDb) >= 0.0001;

            // Determine if we need to overlay a sound effect (applies after mixing stems or directly on original before EQ)
            bool wantsSoundEffect = !string.IsNullOrWhiteSpace(request.SoundEffectType);

            // If no stem mix branch, and we will later apply filters (or even none), but effect requested, prepare base with effect now (for non-stem path)
            // We'll do this early ONLY for the non separation branch; for separation branch we'll overlay right after mixing.
            string? preProcessedBaseForNonStem = null;
            if (!wantsVoiceBgAdjust && wantsSoundEffect)
            {
                // Overlay effect onto inputPath (loop/truncate to match duration) producing a new base file
                var durationForEffect = 0d;
                try { durationForEffect = await GetAudioDurationSecondsAsync(inputPath, ffprobePath, HttpContext.RequestAborted); } catch { }
                var withFx = Path.Combine(sessionDir, $"withfx_{Guid.NewGuid()}.mp3");
                var effectApplied = await TryApplySoundEffectAsync(inputPath, withFx, request.SoundEffectType!, request.SoundEffectVolumeDb, durationForEffect, ffmpegPath, HttpContext.RequestAborted);
                if (effectApplied)
                {
                    preProcessedBaseForNonStem = withFx;
                    inputPath = withFx; // From now on pipeline uses this as original
                }
                else
                {
                    TryDeleteFile(withFx);
                }
            }
            if (wantsVoiceBgAdjust)
            {
                // Try reuse cache: same file, existing stems or mix
                if (previous != null && previous.fileHash == fileHash)
                {
                    // If gains unchanged and we have a mixed file, just re-apply filters if changed
                    bool sameGains = Math.Abs(previous.config.audioSeparation.VoiceGainDb - request.VoiceGainDb) < 0.0001 &&
                                     Math.Abs(previous.config.audioSeparation.BackgroundSoundGainDb - request.BackgroundSoundGainDb) < 0.0001;
                    if (sameGains && !string.IsNullOrWhiteSpace(previous.paths?.mix) && System.IO.File.Exists(previous.paths.mix))
                    {
                        // If filterChain unchanged and we already have output, return it
                        if (previous.config != null && ConfigEquals(previous.config, request) &&
                            !string.IsNullOrWhiteSpace(previous.paths?.output) && System.IO.File.Exists(previous.paths.output))
                        {
                            return await ReturnOutputAsync(sessionId, previous.paths.output, request.UseHls, ffmpegPath, HttpContext.RequestAborted);
                        }

                        // Prepare base (mix) possibly overlaying sound effect if requested or effect settings changed
                        string baseForFilters = previous.paths.mix;
                        if (wantsSoundEffect && SoundEffectConfigChanged(previous.config, request))
                        {
                            var baseWithFx = Path.Combine(sessionDir, $"withfx_{Guid.NewGuid()}.mp3");
                            var dur = await GetAudioDurationSecondsAsync(baseForFilters, ffprobePath, HttpContext.RequestAborted);
                            var okFx = await TryApplySoundEffectAsync(baseForFilters, baseWithFx, request.SoundEffectType!, request.SoundEffectVolumeDb, dur, ffmpegPath, HttpContext.RequestAborted);
                            if (okFx)
                                baseForFilters = baseWithFx;
                        }

                        // Apply new filters on (maybe effect-augmented) cached mix
                        var finalOutCached = Path.Combine(sessionDir, $"tuned_{Guid.NewGuid()}.mp3");
                        await ApplyFiltersAsync(baseForFilters, finalOutCached, filterChain, ffmpegPath, HttpContext.RequestAborted);

                        if (!string.IsNullOrWhiteSpace(previous.paths?.output) &&
    !string.Equals(previous.paths.output, finalOutCached, StringComparison.OrdinalIgnoreCase))
                        {
                            TryDeleteFile(previous.paths.output);
                        }

                        previous.paths.output = finalOutCached;
                        previous.config = BuildPreviousConfigFromRequest(request);
                        previous.fileHash = fileHash;
                        await SavePreviousAsync(previousPath, previous);
                        return await ReturnOutputAsync(sessionId, finalOutCached, request.UseHls, ffmpegPath, HttpContext.RequestAborted);
                    }
                }

                // Gains changed or no cache: if we already have stems, reuse them to mix
                if (previous != null && previous.fileHash == fileHash &&
                    !string.IsNullOrWhiteSpace(previous.paths?.voiceStem) && System.IO.File.Exists(previous.paths.voiceStem) &&
                    !string.IsNullOrWhiteSpace(previous.paths?.bgStem) && System.IO.File.Exists(previous.paths.bgStem))
                {
                    var mixedPath = Path.Combine(sessionDir, $"mixed_{Guid.NewGuid()}.mp3");
                    await MixExistingStemsAsync(previous.paths.voiceStem, previous.paths.bgStem, request, ffmpegPath, mixedPath, HttpContext.RequestAborted);

                    // xóa mix cũ nếu khác
                    if (!string.IsNullOrWhiteSpace(previous.paths?.mix) &&
                        System.IO.File.Exists(previous.paths.mix) &&
                        !string.Equals(previous.paths.mix, mixedPath, StringComparison.OrdinalIgnoreCase))
                    {
                        TryDeleteFile(previous.paths.mix);
                    }

                    // Overlay sound effect if requested
                    string baseAfterFx = mixedPath;
                    if (wantsSoundEffect)
                    {
                        var withFx = Path.Combine(sessionDir, $"withfx_{Guid.NewGuid()}.mp3");
                        var dur = await GetAudioDurationSecondsAsync(mixedPath, ffprobePath, HttpContext.RequestAborted);
                        var okFx = await TryApplySoundEffectAsync(mixedPath, withFx, request.SoundEffectType!, request.SoundEffectVolumeDb, dur, ffmpegPath, HttpContext.RequestAborted);
                        if (okFx)
                            baseAfterFx = withFx;
                        else
                            TryDeleteFile(withFx);
                    }

                    // Apply filters if any
                    string finalOut = baseAfterFx;
                    if (!string.IsNullOrWhiteSpace(filterChain))
                    {
                        finalOut = Path.Combine(sessionDir, $"tuned_{Guid.NewGuid()}.mp3");
                        await ApplyFiltersAsync(baseAfterFx, finalOut, filterChain, ffmpegPath, HttpContext.RequestAborted);
                    }
                    if (!string.IsNullOrWhiteSpace(previous.paths?.output) &&
                            System.IO.File.Exists(previous.paths.output) &&
                            !string.Equals(previous.paths.output, finalOut, StringComparison.OrdinalIgnoreCase))
                    {
                        TryDeleteFile(previous.paths.output);
                    }
                    previous.paths.mix = mixedPath;
                    previous.paths.output = finalOut;
                    previous.config = BuildPreviousConfigFromRequest(request);
                    previous.fileHash = fileHash;
                    await SavePreviousAsync(previousPath, previous);
                    return await ReturnOutputAsync(sessionId, finalOut, request.UseHls, ffmpegPath, HttpContext.RequestAborted);
                }

                // No usable cache: call separator, also persist stems for session
                var separatedMixed = await SeparateAndMixAsync(inputPath, request, ffmpegPath, HttpContext.RequestAborted, sessionDir);
                if (separatedMixed == null)
                    return StatusCode(502, "Separation service failed or returned invalid result");

                if (previous?.paths?.mix != null &&
                    System.IO.File.Exists(previous.paths.mix) &&
                    !string.Equals(previous.paths.mix, separatedMixed, StringComparison.OrdinalIgnoreCase))
                {
                    TryDeleteFile(previous.paths.mix);
                }


                // Overlay effect if requested (before filters)
                string baseAfterFx2 = separatedMixed;
                if (wantsSoundEffect)
                {
                    var withFx2 = Path.Combine(sessionDir, $"withfx_{Guid.NewGuid()}.mp3");
                    var dur2 = await GetAudioDurationSecondsAsync(separatedMixed, ffprobePath, HttpContext.RequestAborted);
                    var okFx2 = await TryApplySoundEffectAsync(separatedMixed, withFx2, request.SoundEffectType!, request.SoundEffectVolumeDb, dur2, ffmpegPath, HttpContext.RequestAborted);
                    if (okFx2)
                        baseAfterFx2 = withFx2;
                    else
                        TryDeleteFile(withFx2);
                }

                string finalOut2 = baseAfterFx2;
                if (!string.IsNullOrWhiteSpace(filterChain))
                {
                    var tunedPath = Path.Combine(sessionDir, $"tuned_{Guid.NewGuid()}.mp3");
                    await ApplyFiltersAsync(baseAfterFx2, tunedPath, filterChain, ffmpegPath, HttpContext.RequestAborted);
                    finalOut2 = tunedPath;
                }

                // xóa output cũ nếu khác (áp dụng cho cả trường hợp có/không có filter)
                if (previous?.paths?.output != null &&
                    System.IO.File.Exists(previous.paths.output) &&
                    !string.Equals(previous.paths.output, finalOut2, StringComparison.OrdinalIgnoreCase))
                {
                    TryDeleteFile(previous.paths.output);
                }

                var newPrev = previous ?? new PreviousRoot();
                newPrev.fileHash = fileHash;
                newPrev.createdAt = DateTime.UtcNow;
                newPrev.paths ??= new PreviousPaths();
                newPrev.paths.original = inputPath;
                if (System.IO.File.Exists(Path.Combine(sessionDir, "stems_voice.wav")))
                    newPrev.paths.voiceStem = Path.Combine(sessionDir, "stems_voice.wav");
                if (System.IO.File.Exists(Path.Combine(sessionDir, "stems_bg.wav")))
                    newPrev.paths.bgStem = Path.Combine(sessionDir, "stems_bg.wav");
                newPrev.paths.mix = separatedMixed;
                newPrev.paths.output = finalOut2;
                newPrev.config = BuildPreviousConfigFromRequest(request);
                await SavePreviousAsync(previousPath, newPrev);
                return await ReturnOutputAsync(sessionId, finalOut2, request.UseHls, ffmpegPath, HttpContext.RequestAborted);
            }

            // 0b. Nhánh nhanh: không EQ, không chỉnh giọng/nhạc => copy stream
            bool noEq = !HasAnyEq(request);
            bool noVoiceMusic = Math.Abs(request.VoiceGainDb) < 0.0001 && Math.Abs(request.BackgroundSoundGainDb) < 0.0001;
            bool noProcessing = noEq && noVoiceMusic;

            if (noProcessing)
            {
                var copied = Path.Combine(uploads, $"tuned_{Guid.NewGuid()}.mp3");
                var copyPsi = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = $"-hide_banner -loglevel error -nostdin -i \"{inputPath}\" -vn -c copy -y \"{copied}\"",
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var p = Process.Start(copyPsi))
                {
                    if (p != null)
                        await p.WaitForExitAsync(HttpContext.RequestAborted);
                }
                return await ReturnOutputAsync(sessionId, copied, request.UseHls, ffmpegPath, HttpContext.RequestAborted);
            }

            // 0.1 Lấy độ dài để quyết định chiến lược cắt
            double durationSeconds = await GetAudioDurationSecondsAsync(inputPath, ffprobePath, HttpContext.RequestAborted);
            var parallelism = Math.Max(1, Environment.ProcessorCount - 1);
            int segmentTime = 30; // mặc định
            if (durationSeconds > 0)
            {
                int targetSegments = Math.Max(3, parallelism * 3);
                segmentTime = (int)Math.Ceiling(durationSeconds / targetSegments);
                segmentTime = Math.Clamp(segmentTime, 20, 120);
            }

            // Nếu file ngắn (<= 90s) hoặc chia chỉ ra 1 phần, xử lý 1 pass
            if (durationSeconds > 0 && (durationSeconds <= 90 || durationSeconds <= segmentTime))
            {
                // Try filter-only cache reuse: if same file and config, return previous
                if (previous != null && previous.fileHash == fileHash && previous.config != null &&
                    ConfigEquals(previous.config, request) && !string.IsNullOrWhiteSpace(previous.paths?.output) && System.IO.File.Exists(previous.paths.output))
                {
                    return await ReturnOutputAsync(sessionId, previous.paths.output, request.UseHls, ffmpegPath, HttpContext.RequestAborted);
                }

                // If same file and we have a mixed file (from prior gains but now gains are zero), apply only filters to input
                // If effect requested and not previously applied (or config changed) we already handled preProcessedBaseForNonStem at top
                var onePassOut = Path.Combine(sessionDir, $"tuned_{Guid.NewGuid()}.mp3");
                var onePass = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = $"-hide_banner -loglevel error -nostdin -i \"{inputPath}\" -vn -af \"{filterChain}\" -c:a libmp3lame -b:a 192k -threads 1 -y \"{onePassOut}\"",
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var p = Process.Start(onePass))
                {
                    if (p != null)
                        await p.WaitForExitAsync(HttpContext.RequestAborted);
                }

                // Save/Update previous
                var prev = previous ?? new PreviousRoot();
                prev.fileHash = fileHash;
                prev.createdAt = DateTime.UtcNow;
                prev.paths ??= new PreviousPaths();
                prev.paths.original = inputPath;
                if (!string.IsNullOrWhiteSpace(prev.paths.output) && !string.Equals(prev.paths.output, onePassOut, StringComparison.OrdinalIgnoreCase))
                {
                    TryDeleteFile(prev.paths.output);
                }

                prev.paths.output = onePassOut;
                prev.config = BuildPreviousConfigFromRequest(request);
                await SavePreviousAsync(previousPath, prev);
                return await ReturnOutputAsync(sessionId, onePassOut, request.UseHls, ffmpegPath, HttpContext.RequestAborted);
            }

            // 1. Cắt file thành segment
            var segmentDir = Path.Combine(uploads, "segments_" + Guid.NewGuid());
            Directory.CreateDirectory(segmentDir);
            var segmentPattern = Path.Combine(segmentDir, "part_%03d.mp3");

            var split = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-hide_banner -loglevel error -nostdin -i \"{inputPath}\" -vn -f segment -segment_time {segmentTime} -c copy \"{segmentPattern}\"",
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var proc = Process.Start(split))
            {
                if (proc != null)
                    await proc.WaitForExitAsync(HttpContext.RequestAborted);
            }

            // 2. Xử lý song song với filter chain
            var segmentFiles = Directory.GetFiles(segmentDir, "part_*.mp3");
            var processedFiles = new List<string>();
            await Parallel.ForEachAsync(
                segmentFiles,
                new ParallelOptions { MaxDegreeOfParallelism = parallelism, CancellationToken = HttpContext.RequestAborted },
                async (file, token) =>
                {
                    var outFile = file.Replace(".mp3", "_proc.mp3");
                    var eq = new ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = $"-hide_banner -loglevel error -nostdin -i \"{file}\" -vn -af \"{filterChain}\" -c:a libmp3lame -b:a 192k -threads 1 -y \"{outFile}\"",
                        RedirectStandardError = false,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using (var p = Process.Start(eq))
                    {
                        if (p != null)
                            await p.WaitForExitAsync(token);
                    }
                    lock (processedFiles)
                    {
                        processedFiles.Add(outFile);
                    }
                });

            // 3. Ghép lại
            var listFile = Path.Combine(segmentDir, "concat.txt");
            var ordered = processedFiles
                .OrderBy(f => Path.GetFileName(f))
                .Select(f => $"file '{f.Replace("\\", "/")}'");
            await System.IO.File.WriteAllLinesAsync(listFile, ordered);

            var outputPath = Path.Combine(sessionDir, $"tuned_{Guid.NewGuid()}.mp3");
            var concat = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-hide_banner -loglevel error -nostdin -f concat -safe 0 -i \"{listFile}\" -c copy -y \"{outputPath}\"",
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var proc = Process.Start(concat))
            {
                if (proc != null)
                    await proc.WaitForExitAsync(HttpContext.RequestAborted);
            }

            // 4. Dọn tệp tạm
            if (Directory.Exists(segmentDir))
            {
                try { Directory.Delete(segmentDir, true); } catch { /* ignore cleanup errors */ }
            }

            var prevSeg = previous ?? new PreviousRoot();
            prevSeg.fileHash = fileHash;
            prevSeg.createdAt = DateTime.UtcNow;
            prevSeg.paths ??= new PreviousPaths();
            prevSeg.paths.original = inputPath;

            if (!string.IsNullOrWhiteSpace(prevSeg.paths.output) &&
                System.IO.File.Exists(prevSeg.paths.output) &&
                !string.Equals(prevSeg.paths.output, outputPath, StringComparison.OrdinalIgnoreCase))
            {
                TryDeleteFile(prevSeg.paths.output);
            }

            prevSeg.paths.output = outputPath;
            prevSeg.config = BuildPreviousConfigFromRequest(request);
            await SavePreviousAsync(previousPath, prevSeg);

            return await ReturnOutputAsync(sessionId, outputPath, request.UseHls, ffmpegPath, HttpContext.RequestAborted);
        }

        // Centralized return helper (MP3 or HLS)
        private async Task<IActionResult> ReturnOutputAsync(string sessionId, string audioPath, bool useHls, string ffmpegPath, CancellationToken token)
        {
            if (!useHls)
            {
                return PhysicalFile(audioPath, "audio/mpeg", Path.GetFileName(audioPath));
            }

            // Generate (or reuse) HLS
            var hlsInfo = await CreateHlsAsync(sessionId, audioPath, ffmpegPath, token);
            return Ok(hlsInfo);
        }

        private async Task<object> CreateHlsAsync(string sessionId, string sourceFile, string ffmpegPath, CancellationToken ct)
        {
            var sessionDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "sessions", sessionId);
            var hlsDir = Path.Combine(sessionDir, "hls");
            Directory.CreateDirectory(hlsDir);

            // Reuse existing playlist if source unchanged (simple heuristic)
            var existingPlaylist = Path.Combine(hlsDir, "playlist.m3u8");
            if (System.IO.File.Exists(existingPlaylist))
            {
                // Return existing without regenerating (optionally could validate timestamp)
                var pEncodedOld = Convert.ToBase64String(Encoding.UTF8.GetBytes(existingPlaylist));
                return new
                {
                    hls = true,
                    reused = true,
                    p = existingPlaylist,
                    e = pEncodedOld,
                    playlist = $"/api/hls/playlist?sid={sessionId}&p={Uri.EscapeDataString(pEncodedOld)}",
                    session = sessionId
                };
            }

            // Create encryption key
            var keyBytes = RandomNumberGenerator.GetBytes(16);
            var keyFile = Path.Combine(hlsDir, "enc.key");
            await System.IO.File.WriteAllBytesAsync(keyFile, keyBytes, ct);
            var keyId = HlsKeyStore.Add(sessionId, keyFile);
            var keyUrl = $"{Request.Scheme}://{Request.Host}/api/hls/key/{keyId}";

            // keyinfo file (two lines: URL, local key path)
            var keyInfoPath = Path.Combine(hlsDir, "keyinfo.txt");
            await System.IO.File.WriteAllTextAsync(keyInfoPath, keyUrl + "\n" + keyFile.Replace("\\", "/") + "\n", ct);

            var playlistPath = existingPlaylist; // path decided above
            var segPattern = Path.Combine(hlsDir, "seg_%03d.ts");

            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-hide_banner -loglevel error -nostdin -i \"{sourceFile}\" -c:a aac -b:a 192k -hls_time 6 -hls_playlist_type vod -hls_segment_filename \"{segPattern}\" -hls_key_info_file \"{keyInfoPath}\" -y \"{playlistPath}\"",
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var p = Process.Start(psi))
            {
                if (p != null)
                    await p.WaitForExitAsync(ct);
            }

            if (!System.IO.File.Exists(playlistPath))
            {
                return new { hls = false, error = "HLS generation failed" };
            }

            var pEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(playlistPath));
            return new
            {
                hls = true,
                reused = false,
                p = playlistPath,
                e = pEncoded,
                playlist = $"/api/hls/playlist?sid={sessionId}&p={Uri.EscapeDataString(pEncoded)}",
                session = sessionId
            };
        }

        private async Task ApplyFiltersAsync(string input, string output, string filterChain, string ffmpegPath, CancellationToken token)
        {
            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-hide_banner -loglevel error -nostdin -i \"{input}\" -vn -af \"{filterChain}\" -c:a libmp3lame -b:a 192k -threads 1 -y \"{output}\"",
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            if (p != null)
                await p.WaitForExitAsync(token);
        }

        private static void TryDeleteDirectory(string dir)
        {
            try { if (Directory.Exists(dir)) Directory.Delete(dir, true); } catch { }
        }
        // Xóa file an toàn nếu tồn tại
        private static void TryDeleteFile(string? path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }
            catch { }
        }
        private string BuildAudioFilterChain(AudioExportRequest request)
        {
            var parts = new List<string>();

            // 1) EQ band: merge Manual EQ với EQ delta từ Mood để chỉ có 1 firequalizer
            var baseBands = ToEqBands(request);
            var mood = BuildMoodComponents(request);
            var merged = AddBands(baseBands, mood.eqDelta);
            if (HasAnyEq(merged))
            {
                parts.Add(BuildEqFilter(merged));
            }

            // 2) Thêm các filter bổ sung (không phải EQ) từ Mood sau EQ
            if (!string.IsNullOrWhiteSpace(mood.extraFilters))
            {
                parts.Add(mood.extraFilters);
            }

            return string.Join(",", parts.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        private static double DbToLinear(double db) => Math.Pow(10.0, db / 20.0);

        private bool HasAnyEq(AudioExportRequest request)
        {
            return !(request.SubBass == 0 && request.Bass == 0 && request.Low == 0 &&
                     request.LowMid == 0 && request.Mid == 0 && request.Presence == 0 &&
                     request.HighMid == 0 && request.Treble == 0 && request.Air == 0);
        }

        private string BuildEqFilter(AudioExportRequest request)
        {
            // 10-band style mapping
            // 32, 64, 125, 250, 1000, 2000, 4000, 8000, 16000 Hz
            return $"firequalizer=gain_entry='" +
                $"entry(32,{request.SubBass});" +
                $"entry(64,{request.Bass});" +
                $"entry(125,{request.Low});" +
                $"entry(250,{request.LowMid});" +
                $"entry(1000,{request.Mid});" +
                $"entry(2000,{request.Presence});" +
                $"entry(4000,{request.HighMid});" +
                $"entry(8000,{request.Treble});" +
                $"entry(16000,{request.Air})'";
        }

        private string BuildEqFilter(EqBands b)
        {
            return $"firequalizer=gain_entry='" +
                   $"entry(32,{b.SubBass});" +
                   $"entry(64,{b.Bass});" +
                   $"entry(125,{b.Low});" +
                   $"entry(250,{b.LowMid});" +
                   $"entry(1000,{b.Mid});" +
                   $"entry(2000,{b.Presence});" +
                   $"entry(4000,{b.HighMid});" +
                   $"entry(8000,{b.Treble});" +
                   $"entry(16000,{b.Air})'";
        }

        private async Task<string?> SeparateAndMixAsync(string inputPath, AudioExportRequest request, string ffmpegPath, CancellationToken token, string? stemsSaveDir = null)
        {
            // URL của FastAPI tách stems
            string separationUrl = Environment.GetEnvironmentVariable("SEPARATION_API_URL") ?? "http://localhost:8000/separate";

            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            var workDir = Path.Combine(uploads, "stems_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(workDir);

            string zipPath = Path.Combine(workDir, "stems.zip");
            try
            {
                using var http = new HttpClient();
                using var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(System.IO.File.OpenRead(inputPath));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/mpeg");
                content.Add(fileContent, "file", Path.GetFileName(inputPath));

                using var resp = await http.PostAsync(separationUrl, content, token);
                if (!resp.IsSuccessStatusCode)
                    return null;

                await using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await resp.Content.CopyToAsync(fs, token);
                }

                // Giải nén
                string extractDir = Path.Combine(workDir, "extracted");
                Directory.CreateDirectory(extractDir);
                ZipFile.ExtractToDirectory(zipPath, extractDir);

                // Tìm 2 file: Voice và BackgroundSound
                var allFiles = Directory.GetFiles(extractDir, "*", SearchOption.AllDirectories)
                                         .Where(p => new[] { ".mp3", ".wav", ".flac", ".m4a", ".ogg" }
                                             .Contains(Path.GetExtension(p).ToLowerInvariant()))
                                         .ToList();
                if (allFiles.Count < 2)
                    return null;

                string? voice = allFiles.FirstOrDefault(p => Path.GetFileName(p).ToLowerInvariant().Contains("voice"));
                if (voice == null)
                    voice = allFiles.FirstOrDefault(p => Path.GetFileName(p).ToLowerInvariant().Contains("vocals"));
                string? bg = allFiles.FirstOrDefault(p => Path.GetFileName(p).ToLowerInvariant().Contains("backgroundsound"));
                if (bg == null)
                    bg = allFiles.FirstOrDefault(p => Path.GetFileName(p).ToLowerInvariant().Contains("instrumental"));

                if (voice == null || bg == null)
                {
                    // fallback: lấy 2 file đầu
                    voice = allFiles[0];
                    bg = allFiles[1];
                }

                // Optionally persist stems for session reuse
                if (!string.IsNullOrWhiteSpace(stemsSaveDir))
                {
                    Directory.CreateDirectory(stemsSaveDir);
                    var stemVoice = Path.Combine(stemsSaveDir, "stems_voice.wav");
                    var stemBg = Path.Combine(stemsSaveDir, "stems_bg.wav");
                    try
                    {
                        System.IO.File.Copy(voice, stemVoice, true);
                        System.IO.File.Copy(bg, stemBg, true);
                    }
                    catch { }
                }

                var outputPath = Path.Combine(stemsSaveDir ?? uploads, $"mixed_{Guid.NewGuid()}.mp3");

                var vDb = request.VoiceGainDb.ToString(CultureInfo.InvariantCulture) + "dB";
                var bDb = request.BackgroundSoundGainDb.ToString(CultureInfo.InvariantCulture) + "dB";

                // [0:a]volume=vDb[a0];[1:a]volume=bDb[a1];[a0][a1]amix=2:duration=longest:dropout_transition=0[m]
                var fc = new System.Text.StringBuilder();
                fc.Append($"[0:a]volume={vDb}[a0];[1:a]volume={bDb}[a1];[a0][a1]amix=inputs=2:duration=longest:dropout_transition=0[m]");

                var psi = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = $"-hide_banner -loglevel error -nostdin -i \"{voice}\" -i \"{bg}\" -vn -filter_complex \"{fc}\" -map \"[m]\" -c:a libmp3lame -b:a 192k -y \"{outputPath}\"",
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var p = Process.Start(psi))
                {
                    if (p != null)
                        await p.WaitForExitAsync(token);
                }

                // Dọn thư mục tạm
                try { Directory.Delete(workDir, true); } catch { }
                return outputPath;
            }
            catch
            {
                try { Directory.Delete(workDir, true); } catch { }
                return null;
            }
        }

        private async Task MixExistingStemsAsync(string voice, string bg, AudioExportRequest request, string ffmpegPath, string outputPath, CancellationToken token)
        {
            var vDb = request.VoiceGainDb.ToString(CultureInfo.InvariantCulture) + "dB";
            var bDb = request.BackgroundSoundGainDb.ToString(CultureInfo.InvariantCulture) + "dB";
            var fc = new System.Text.StringBuilder();
            fc.Append($"[0:a]volume={vDb}[a0];[1:a]volume={bDb}[a1];[a0][a1]amix=inputs=2:duration=longest:dropout_transition=0[m]");
            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-hide_banner -loglevel error -nostdin -i \"{voice}\" -i \"{bg}\" -vn -filter_complex \"{fc}\" -map \"[m]\" -c:a libmp3lame -b:a 192k -y \"{outputPath}\"",
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            if (p != null)
                await p.WaitForExitAsync(token);
        }

        private static string ComputeSha1(string filePath)
        {
            using var sha1 = SHA1.Create();
            using var fs = System.IO.File.OpenRead(filePath);
            var hash = sha1.ComputeHash(fs);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static bool ConfigEquals(PreviousConfig cfg, AudioExportRequest req)
        {
            if (cfg == null) return false;
            return Math.Abs(cfg.audioSeparation.VoiceGainDb - req.VoiceGainDb) < 0.0001 &&
                   Math.Abs(cfg.audioSeparation.BackgroundSoundGainDb - req.BackgroundSoundGainDb) < 0.0001 &&
                   (cfg.moodMode.Mood ?? string.Empty) == (req.Mood ?? string.Empty) &&
                   Math.Abs(cfg.moodMode.MoodIntensity - req.MoodIntensity) < 0.0001 &&
                   (cfg.soundEffect?.Type ?? string.Empty) == (req.SoundEffectType ?? string.Empty) &&
                   Math.Abs((cfg.soundEffect?.VolumeDb ?? 0) - req.SoundEffectVolumeDb) < 0.0001 &&
                   cfg.eqBands.SubBass == req.SubBass && cfg.eqBands.Bass == req.Bass && cfg.eqBands.Low == req.Low &&
                   cfg.eqBands.LowMid == req.LowMid && cfg.eqBands.Mid == req.Mid && cfg.eqBands.Presence == req.Presence &&
                   cfg.eqBands.HighMid == req.HighMid && cfg.eqBands.Treble == req.Treble && cfg.eqBands.Air == req.Air;
        }

        private static bool SoundEffectConfigChanged(PreviousConfig cfg, AudioExportRequest req)
        {
            if (cfg == null) return true;
            var prevType = cfg.soundEffect?.Type ?? string.Empty;
            var curType = req.SoundEffectType ?? string.Empty;
            if (!string.Equals(prevType, curType, StringComparison.OrdinalIgnoreCase)) return true;
            var prevVol = cfg.soundEffect?.VolumeDb ?? 0;
            return Math.Abs(prevVol - req.SoundEffectVolumeDb) >= 0.0001;
        }

        private static PreviousConfig BuildPreviousConfigFromRequest(AudioExportRequest req) => new PreviousConfig
        {
            audioSeparation = new AudioSeparationConfig
            {
                VoiceGainDb = req.VoiceGainDb,
                BackgroundSoundGainDb = req.BackgroundSoundGainDb
            },
            eqBands = new EqBandsConfig
            {
                SubBass = req.SubBass,
                Bass = req.Bass,
                Low = req.Low,
                LowMid = req.LowMid,
                Mid = req.Mid,
                Presence = req.Presence,
                HighMid = req.HighMid,
                Treble = req.Treble,
                Air = req.Air
            },
            moodMode = new MoodModeConfig
            {
                Mood = req.Mood,
                MoodIntensity = req.MoodIntensity
            },
            soundEffect = string.IsNullOrWhiteSpace(req.SoundEffectType) ? null : new SoundEffectConfig
            {
                Type = req.SoundEffectType,
                VolumeDb = req.SoundEffectVolumeDb
            }
        };

        private static async Task<PreviousRoot?> LoadPreviousAsync(string previousPath)
        {
            try
            {
                if (!System.IO.File.Exists(previousPath)) return null;
                using var fs = System.IO.File.OpenRead(previousPath);
                return await JsonSerializer.DeserializeAsync<PreviousRoot>(fs);
            }
            catch { return null; }
        }

        private static async Task SavePreviousAsync(string previousPath, PreviousRoot data)
        {
            try
            {
                var dir = Path.GetDirectoryName(previousPath);
                if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
                var opts = new JsonSerializerOptions { WriteIndented = true };
                await using var fs = new FileStream(previousPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await JsonSerializer.SerializeAsync(fs, data, opts);
            }
            catch { }
        }

        private async Task<bool> TryApplySoundEffectAsync(string baseInput, string outputPath, string type, double volumeDb, double baseDurationSeconds, string ffmpegPath, CancellationToken token)
        {
            try
            {
                var effectPath = ResolveSoundEffectPath(type);
                if (effectPath == null) return false;
                if (baseDurationSeconds <= 0)
                {
                    // attempt to probe
                    baseDurationSeconds = await GetAudioDurationSecondsAsync(baseInput, Path.Combine(Path.GetDirectoryName(ffmpegPath) ?? "", "ffprobe.exe"), token);
                    if (baseDurationSeconds <= 0) baseDurationSeconds = 300; // fallback 5 min max
                }
                // Build filter: loop effect infinitely, trim to base duration, set volume, amix
                var durStr = baseDurationSeconds.ToString(CultureInfo.InvariantCulture);
                var volStr = volumeDb.ToString(CultureInfo.InvariantCulture) + "dB";
                var filter = $"[1:a]aloop=loop=-1:size=2e9,atrim=0:{durStr},asetpts=N/SR/TB,volume={volStr}[fx];[0:a][fx]amix=inputs=2:duration=first:dropout_transition=0[m]";
                var psi = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = $"-hide_banner -loglevel error -nostdin -i \"{baseInput}\" -i \"{effectPath}\" -filter_complex \"{filter}\" -map \"[m]\" -c:a libmp3lame -b:a 192k -y \"{outputPath}\"",
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                if (p != null)
                    await p.WaitForExitAsync(token);
                return System.IO.File.Exists(outputPath);
            }
            catch { return false; }
        }

        private string? ResolveSoundEffectPath(string type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(type)) return null;
                var soundDir = Path.Combine(Directory.GetCurrentDirectory(), "sound");
                if (!Directory.Exists(soundDir)) return null;
                var lowered = type.Trim().ToLowerInvariant();
                var files = Directory.GetFiles(soundDir, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => new[] { ".mp3", ".wav", ".ogg", ".m4a", ".flac" }.Contains(Path.GetExtension(f).ToLowerInvariant()));
                // Match by filename without extension
                var match = files.FirstOrDefault(f => string.Equals(Path.GetFileNameWithoutExtension(f).ToLowerInvariant(), lowered, StringComparison.OrdinalIgnoreCase));
                return match;
            }
            catch { return null; }
        }

        private (EqBands eqDelta, string extraFilters) BuildMoodComponents(AudioExportRequest request)
        {
            var zero = new EqBands();
            if (string.IsNullOrWhiteSpace(request.Mood)) return (zero, string.Empty);
            double intensity = Math.Clamp(request.MoodIntensity, 0.0, 1.0);
            string mood = request.Mood.Trim().ToLowerInvariant();

            // round helper
            int R(double v) => (int)Math.Round(v);

            switch (mood)
            {
                case "energetic":
                    return (
                        new EqBands { Bass = R(6 * intensity), Presence = R(5 * intensity) },
                        $"acompressor=threshold=-20dB:ratio={(3.0 + 1.5 * intensity).ToString(CultureInfo.InvariantCulture)}:attack=5:release=60, " +
                        $"stereotools=mutel={0}:phase={0}:slev={(1.2 + 0.2 * intensity).ToString(CultureInfo.InvariantCulture)}"
                    );

                case "mellow":
                    return (
                        new EqBands { Treble = R(-6 * intensity), Low = R(3 * intensity) },
                        $"aecho=0.8:0.9:{(int)(1000 * intensity)}:{(0.4 * intensity).ToString(CultureInfo.InvariantCulture)}, " +
                        $"acompressor=threshold=-28dB:ratio=2:attack=10:release=200"
                    );

                case "bright":
                    return (
                        new EqBands { Presence = R(4.0 * intensity), Treble = R(6.0 * intensity), Air = R(5.0 * intensity) },
                        $"highpass=f=120,alimiter=limit=0.9"
                    );

                case "intimate":
                    return (
                        new EqBands { Mid = R(8 * intensity), HighMid = R(6 * intensity) },
                        $"acompressor=threshold=-30dB:ratio={(6.0 + 2.0 * intensity).ToString(CultureInfo.InvariantCulture)}:attack=5:release=120, " +
                        $"stereotools=slev={(0.8 - 0.2 * intensity).ToString(CultureInfo.InvariantCulture)}"
                    );

                case "balance":
                    return (
                        new EqBands { Bass = R(1 * intensity), Mid = R(0), Treble = R(1.5 * intensity) },
                        $"alimiter=limit=0.95"
                    );

                case "warm":
                    return (
                        new EqBands { Low = R(4 * intensity), Mid = R(2 * intensity), Treble = R(-3 * intensity) },
                        $"aresample=async=1:first_pts=0, " +
                        $"acompressor=threshold=-25dB:ratio=2:attack=15:release=150"
                    );

                case "cool":
                    return (
                        new EqBands { Bass = R(-2 * intensity), Presence = R(4 * intensity), Treble = R(5 * intensity) },
                        $"stereotools=slev={(1.3 + 0.2 * intensity).ToString(CultureInfo.InvariantCulture)}, " +
                        $"aecho=0.7:0.8:{(int)(600 * intensity)}:{(0.3 * intensity).ToString(CultureInfo.InvariantCulture)}"
                    );

                default:
                    return (zero, string.Empty);
            }

        }

        private struct EqBands
        {
            public int SubBass;   // 32Hz
            public int Bass;      // 64Hz
            public int Low;       // 125Hz
            public int LowMid;    // 250Hz
            public int Mid;       // 1kHz
            public int Presence;  // 2kHz
            public int HighMid;   // 4kHz
            public int Treble;    // 8kHz
            public int Air;       // 16kHz
        }

        private EqBands ToEqBands(AudioExportRequest r) => new EqBands
        {
            SubBass = r.SubBass,
            Bass = r.Bass,
            Low = r.Low,
            LowMid = r.LowMid,
            Mid = r.Mid,
            Presence = r.Presence,
            HighMid = r.HighMid,
            Treble = r.Treble,
            Air = r.Air
        };

        private static EqBands AddBands(EqBands a, EqBands b) => new EqBands
        {
            SubBass = a.SubBass + b.SubBass,
            Bass = a.Bass + b.Bass,
            Low = a.Low + b.Low,
            LowMid = a.LowMid + b.LowMid,
            Mid = a.Mid + b.Mid,
            Presence = a.Presence + b.Presence,
            HighMid = a.HighMid + b.HighMid,
            Treble = a.Treble + b.Treble,
            Air = a.Air + b.Air
        };

        private static bool HasAnyEq(EqBands b)
        {
            return !(b.SubBass == 0 && b.Bass == 0 && b.Low == 0 && b.LowMid == 0 &&
                     b.Mid == 0 && b.Presence == 0 && b.HighMid == 0 && b.Treble == 0 && b.Air == 0);
        }

        private static async Task<double> GetAudioDurationSecondsAsync(string inputPath, string? ffprobePath, CancellationToken token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ffprobePath) || !System.IO.File.Exists(ffprobePath)) return 0d;
                var psi = new ProcessStartInfo
                {
                    FileName = ffprobePath,
                    Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{inputPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = Process.Start(psi);
                if (p == null) return 0d;
                string? output = await p.StandardOutput.ReadToEndAsync();
                await p.WaitForExitAsync(token);
                if (double.TryParse(output?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
                    return seconds;
                return 0d;
            }
            catch { return 0d; }
        }
    }

    public class AudioExportRequest
    {
        public IFormFile File { get; set; }
        public string? SessionId { get; set; }
        public bool UseHls { get; set; } = false; // yêu cầu trả về HLS + AES-128
        // Sound effect overlay
        public string? SoundEffectType { get; set; }
        public double SoundEffectVolumeDb { get; set; } = -6.0; // default gentle level

        // --- EQ 10-band ---
        public int SubBass { get; set; }   // ~32Hz
        public int Bass { get; set; }      // ~64Hz
        public int Low { get; set; }       // ~125Hz
        public int LowMid { get; set; }    // ~250Hz
        public int Mid { get; set; }       // ~1kHz
        public int Presence { get; set; }  // ~2kHz
        public int HighMid { get; set; }   // ~4kHz
        public int Treble { get; set; }    // ~8kHz
        public int Air { get; set; }       // ~16kHz

        public double VoiceGainDb { get; set; } = 0.0;
        public double BackgroundSoundGainDb { get; set; } = 0.0;

        // Mood processing
        public string? Mood { get; set; }
        public double MoodIntensity { get; set; } = 1.0; // 0..1
    }

    // Previous JSON models
    public class PreviousRoot
    {
        public string fileHash { get; set; }
        public DateTime createdAt { get; set; }
        public PreviousPaths paths { get; set; }
        public PreviousConfig config { get; set; }
    }

    public class PreviousPaths
    {
        public string original { get; set; }
        public string voiceStem { get; set; }
        public string bgStem { get; set; }
        public string mix { get; set; }
        public string output { get; set; }
    }

    public class PreviousConfig
    {
        public AudioSeparationConfig audioSeparation { get; set; }
        public EqBandsConfig eqBands { get; set; }
        public MoodModeConfig moodMode { get; set; }
        public SoundEffectConfig? soundEffect { get; set; }
    }

    public class AudioSeparationConfig
    {
        public double VoiceGainDb { get; set; }
        public double BackgroundSoundGainDb { get; set; }
    }

    public class EqBandsConfig
    {
        public int SubBass { get; set; }
        public int Bass { get; set; }
        public int Low { get; set; }
        public int LowMid { get; set; }
        public int Mid { get; set; }
        public int Presence { get; set; }
        public int HighMid { get; set; }
        public int Treble { get; set; }
        public int Air { get; set; }
    }

    public class MoodModeConfig
    {
        public string? Mood { get; set; }
        public double MoodIntensity { get; set; }
    }

    public class SoundEffectConfig
    {
        public string? Type { get; set; }
        public double VolumeDb { get; set; }
    }
}
