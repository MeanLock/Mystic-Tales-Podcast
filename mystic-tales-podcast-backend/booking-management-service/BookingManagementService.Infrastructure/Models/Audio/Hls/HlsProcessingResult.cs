using BookingManagementService.Infrastructure.Services.Audio.Hls;

namespace BookingManagementService.Infrastructure.Models.Audio.Hls
{
    /// <summary>
    /// Result object for HLS processing operations
    /// </summary>
    public class HlsProcessingResult
    {
        public bool Success { get; set; }
        public string? PlaylistPath { get; set; }
        public string? EncodedPath { get; set; }
        public bool IsReused { get; set; }
        public string? ErrorMessage { get; set; }
        public List<HlsFile> GeneratedFiles { get; set; } = new List<HlsFile>();
        public HlsFile EncryptionKeyFile { get; set; }
        public Guid EncryptionKeyId { get; set; }

    }


}