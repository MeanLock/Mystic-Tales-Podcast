using BookingManagementService.Infrastructure.Models.Audio;

namespace BookingManagementService.Infrastructure.Services.Audio.Hls
{
    /// <summary>
    /// Strategy for selecting HLS codec based on input audio format
    /// </summary>
    public static class HlsCodecStrategy
    {
        /// <summary>
        /// Get FFmpeg codec arguments for HLS conversion based on input format
        /// </summary>
        /// <param name="inputFormat">Input audio format information</param>
        /// <returns>Tuple of (codecArg, bitrateArg, shouldCopyCodec)</returns>
        public static (string codecArg, string? bitrateArg, bool shouldCopyCodec) GetCodecStrategy(AudioFormatInfo inputFormat)
        {
            return inputFormat.Format switch
            {
                // AAC/M4A: Copy codec to avoid re-encoding (best performance + quality)
                AudioFormat.AAC or AudioFormat.M4A => 
                    ("copy", null, true),

                // FLAC/WAV: High quality AAC encoding (preserve lossless quality)
                AudioFormat.FLAC or AudioFormat.WAV => 
                    ("aac", "256k", false),

                // MP3: Standard quality AAC encoding
                AudioFormat.MP3 => 
                    ("aac", "192k", false),

                // Unknown: Fallback to standard AAC
                _ => 
                    ("aac", "192k", false)
            };
        }

        /// <summary>
        /// Get human-readable description of codec strategy
        /// </summary>
        public static string GetStrategyDescription(AudioFormatInfo inputFormat)
        {
            var (codec, bitrate, shouldCopy) = GetCodecStrategy(inputFormat);

            if (shouldCopy)
            {
                return $"Copy codec (no re-encode) - Input: {inputFormat.Format}";
            }

            return bitrate != null 
                ? $"Encode to {codec.ToUpper()} {bitrate} - Input: {inputFormat.Format} ({(inputFormat.IsLossless ? "Lossless" : "Lossy")})"
                : $"Encode to {codec.ToUpper()} - Input: {inputFormat.Format}";
        }

        /// <summary>
        /// Check if input format is optimal for HLS (already AAC)
        /// </summary>
        public static bool IsOptimalForHls(AudioFormatInfo inputFormat)
        {
            return inputFormat.Format is AudioFormat.AAC or AudioFormat.M4A;
        }

        /// <summary>
        /// Estimate quality loss for conversion
        /// </summary>
        public static string GetQualityImpactDescription(AudioFormatInfo inputFormat)
        {
            return inputFormat.Format switch
            {
                AudioFormat.AAC or AudioFormat.M4A => 
                    "✅ No quality loss (codec copy)",

                AudioFormat.FLAC or AudioFormat.WAV => 
                    "⚠️ Lossless → Lossy conversion (high quality AAC 256k)",

                AudioFormat.MP3 => 
                    "⚠️ Re-encoding lossy format (some quality loss)",

                _ => 
                    "❓ Unknown format, using fallback strategy"
            };
        }
    }
}