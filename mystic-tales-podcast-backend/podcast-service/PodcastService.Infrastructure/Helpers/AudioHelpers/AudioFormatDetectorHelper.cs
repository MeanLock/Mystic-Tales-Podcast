using Microsoft.Extensions.Logging;
using PodcastService.Infrastructure.Models.Audio;
using System.Diagnostics;
using System.Text;

namespace PodcastService.Infrastructure.Helpers.AudioHelpers
{
    /// <summary>
    /// Detects audio format from stream using magic bytes (file signatures)
    /// </summary>
    public class AudioFormatDetectorHelper
    {
        private readonly ILogger<AudioFormatDetectorHelper>? _logger;

        public AudioFormatDetectorHelper(ILogger<AudioFormatDetectorHelper>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Detect audio format from stream by reading magic bytes
        /// IMPORTANT: This method does NOT reset stream position to allow non-seekable streams
        /// Caller must handle stream position management
        /// </summary>
        /// <param name="stream">Audio stream to analyze (will be read from current position)</param>
        /// <returns>Audio format information</returns>
        public AudioFormatInfo DetectFormatFromStream(Stream stream)
        {
            if (stream == null || !stream.CanRead)
            {
                _logger?.LogWarning("Stream is null or cannot be read");
                Console.WriteLine("Stream is null or cannot be read");
                return CreateUnknownFormat();
            }

            try
            {
                // Read first 12 bytes (magic bytes) from current position
                var buffer = new byte[12];
                var bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead < 4)
                {
                    _logger?.LogWarning("Stream too short to detect format (read {BytesRead} bytes)", bytesRead);
                    return CreateUnknownFormat();
                }

                // Detect format based on magic bytes
                var format = DetectFormatFromBytes(buffer);
                _logger?.LogInformation($"Detected audio format: {format.Format} ({format.Extension})");
                Console.WriteLine($"Detecteddddđ audio format: {format.Format} ({format.Extension})");

                return format;
            }
            catch (NotSupportedException ex)
            {
                _logger?.LogError(ex, "Stream does not support required operations (Position/Seek). This is common with S3 HashStream.");
                return CreateUnknownFormat();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error detecting audio format");
                return CreateUnknownFormat();
            }
        }

        /// <summary>
        /// Detect format from seekable stream (preserves position)
        /// Use this for local FileStream or MemoryStream
        /// </summary>
        /// <param name="stream">Seekable stream</param>
        /// <returns>Audio format information</returns>
        public AudioFormatInfo DetectFormatSeekableFromStream(Stream stream)
        {
            if (stream == null || !stream.CanRead)
            {
                _logger?.LogWarning("Stream is null or cannot be read");
                return CreateUnknownFormat();
            }

            if (!stream.CanSeek)
            {
                _logger?.LogWarning("Stream is not seekable. Use DetectFormatFromStream() instead or copy to MemoryStream.");
                return DetectFormatFromStream(stream); // Fallback to non-seekable version
            }

            try
            {
                // Save original position
                var originalPosition = stream.Position;

                // Read first 12 bytes (magic bytes)
                var buffer = new byte[12];
                stream.Position = 0;
                var bytesRead = stream.Read(buffer, 0, buffer.Length);

                // Restore original position
                stream.Position = originalPosition;

                if (bytesRead < 4)
                {
                    _logger?.LogWarning("Stream too short to detect format");
                    return CreateUnknownFormat();
                }

                // Detect format based on magic bytes
                var format = DetectFormatFromBytes(buffer);
                _logger?.LogInformation($"Detected audio format: {format.Format} ({format.Extension})");

                return format;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error detecting audio format");
                return CreateUnknownFormat();
            }
        }

        /// <summary>
        /// Detect format from filename extension (fallback method)
        /// </summary>
        public AudioFormatInfo DetectFormatFromExtension(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return CreateUnknownFormat();

            var extension = Path.GetExtension(filename).ToLowerInvariant();

            return extension switch
            {
                ".mp3" => CreateMp3Format(),
                ".aac" => CreateAacFormat(),
                ".m4a" => CreateM4aFormat(),
                ".flac" => CreateFlacFormat(),
                ".wav" => CreateWavFormat(),
                _ => CreateUnknownFormat()
            };
        }

        /// <summary>
        /// Detect format from magic bytes
        /// </summary>
        private AudioFormatInfo DetectFormatFromBytes(byte[] buffer)
        {
            // MP3: FF FB or FF F3 or FF F2 (MPEG Layer 3)
            // MP3 with ID3: 49 44 33 (ID3)
            if ((buffer[0] == 0xFF && (buffer[1] & 0xE0) == 0xE0) ||
                (buffer[0] == 0x49 && buffer[1] == 0x44 && buffer[2] == 0x33))
            {
                return CreateMp3Format();
            }

            // FLAC: 66 4C 61 43 ("fLaC")
            if (buffer[0] == 0x66 && buffer[1] == 0x4C &&
                buffer[2] == 0x61 && buffer[3] == 0x43)
            {
                return CreateFlacFormat();
            }

            // WAV: 52 49 46 46 ... 57 41 56 45 ("RIFF....WAVE")
            if (buffer[0] == 0x52 && buffer[1] == 0x49 &&
                buffer[2] == 0x46 && buffer[3] == 0x46)
            {
                // Check for "WAVE" at offset 8
                if (buffer.Length >= 12 &&
                    buffer[8] == 0x57 && buffer[9] == 0x41 &&
                    buffer[10] == 0x56 && buffer[11] == 0x45)
                {
                    return CreateWavFormat();
                }
            }

            // M4A/AAC: Starts with ftyp atom (00 00 00 xx 66 74 79 70)
            // Check for "ftyp" at offset 4
            if (buffer.Length >= 8 &&
                buffer[4] == 0x66 && buffer[5] == 0x74 &&
                buffer[6] == 0x79 && buffer[7] == 0x70)
            {
                // Further check for M4A specific brands
                if (buffer.Length >= 12)
                {
                    var brand = Encoding.ASCII.GetString(buffer, 8, 4);
                    if (brand.StartsWith("M4A") || brand.StartsWith("mp4"))
                    {
                        return CreateM4aFormat();
                    }
                }
                return CreateM4aFormat(); // Default to M4A for ftyp
            }

            // Raw AAC (ADTS): FF F1 or FF F9
            if (buffer[0] == 0xFF && (buffer[1] == 0xF1 || buffer[1] == 0xF9))
            {
                return CreateAacFormat();
            }

            _logger?.LogWarning($"Unknown format. Magic bytes: {BitConverter.ToString(buffer)}");
            return CreateUnknownFormat();
        }

        private AudioFormatInfo DetectFormatUsingFFprobe(string filePath)
        {
            // Chạy: ffprobe -v error -show_entries format=format_name -of default=noprint_wrappers=1:nokey=1 "file"
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffprobe",
                    Arguments = $"-v error -show_entries format=format_name -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd().Trim().ToLower();
            process.WaitForExit();

            // Parse output
            if (output.Contains("mp3")) return CreateMp3Format();
            if (output.Contains("aac") || output.Contains("m4a")) return CreateAacFormat();
            if (output.Contains("flac")) return CreateFlacFormat();
            if (output.Contains("wav")) return CreateWavFormat();

            _logger?.LogError($"FFprobe also failed to detect format: {output}");
            return CreateUnknownFormat();
        }

        #region Format Creators

        private static AudioFormatInfo CreateMp3Format() => new AudioFormatInfo
        {
            Format = AudioFormat.MP3,
            Extension = ".mp3",
            FfmpegCodec = "libmp3lame",
            IsLossless = false,
            RecommendedBitrate = 192,
            MimeType = "audio/mpeg"
        };

        private static AudioFormatInfo CreateAacFormat() => new AudioFormatInfo
        {
            Format = AudioFormat.AAC,
            Extension = ".aac",
            FfmpegCodec = "aac",
            IsLossless = false,
            RecommendedBitrate = 192,
            MimeType = "audio/aac"
        };

        private static AudioFormatInfo CreateM4aFormat() => new AudioFormatInfo
        {
            Format = AudioFormat.M4A,
            Extension = ".m4a",
            FfmpegCodec = "aac",
            IsLossless = false,
            RecommendedBitrate = 256,
            MimeType = "audio/mp4"
        };

        private static AudioFormatInfo CreateFlacFormat() => new AudioFormatInfo
        {
            Format = AudioFormat.FLAC,
            Extension = ".flac",
            FfmpegCodec = "flac",
            IsLossless = true,
            RecommendedBitrate = null, // Lossless doesn't use bitrate
            MimeType = "audio/flac"
        };

        private static AudioFormatInfo CreateWavFormat() => new AudioFormatInfo
        {
            Format = AudioFormat.WAV,
            Extension = ".wav",
            FfmpegCodec = "pcm_s16le",
            IsLossless = true,
            RecommendedBitrate = null, // Lossless doesn't use bitrate
            MimeType = "audio/wav"
        };

        private static AudioFormatInfo CreateUnknownFormat() => new AudioFormatInfo
        {
            Format = AudioFormat.Unknown,
            Extension = ".mp3", // Fallback to MP3
            FfmpegCodec = "libmp3lame",
            IsLossless = false,
            RecommendedBitrate = 192,
            MimeType = "audio/mpeg"
        };

        #endregion
    }
}