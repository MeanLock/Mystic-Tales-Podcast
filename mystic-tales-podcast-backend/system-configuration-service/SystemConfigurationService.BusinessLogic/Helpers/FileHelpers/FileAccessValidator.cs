using System.Text.RegularExpressions;
using SystemConfigurationService.BusinessLogic.Enums.App;

namespace SystemConfigurationService.BusinessLogic.Helpers.FileHelpers
{
    public class FileAccessValidator
    {
        // Regex patterns
        private const string INT_PATTERN = @"\d+";
        private const string GUID_PATTERN = @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";

        // File extensions
        private const string IMAGE_EXT = @"\.(jpg|jpeg|png|webp|gif|svg)$";
        private const string AUDIO_EXT = @"\.(mp3|wav|m4a|aac|flac)$";
        private const string DOCUMENT_EXT = @"\.(pdf|doc|docx|txt|xls|xlsx|csv)$";
        private const string ARCHIVE_EXT = @"\.(zip|rar)$";
        private const string M3U8_EXT = @"\.m3u8$";
        private const string TS_EXT = @"\.ts$";
        private const string KEY_EXT = @"\.key$";

        // File patterns with access levels
        private static readonly Dictionary<FileCategoryEnum, (string pattern, FileAccessLevelEnum accessLevel, string description)> FilePatterns = new()
        {
            // ============ PUBLIC (No auth required) ============
            [FileCategoryEnum.AccountMainImage] = (
                $@"^main_files/Accounts/{INT_PATTERN}/main_image{IMAGE_EXT}",
                FileAccessLevelEnum.Public,
                "Account profile image"
            ),
            [FileCategoryEnum.AccountBuddyTrailerAudio] = (
                $@"^main_files/Accounts/{INT_PATTERN}/buddy_trailer_audio{AUDIO_EXT}",
                FileAccessLevelEnum.Public,
                "Podcast buddy trailer audio"
            ),
            [FileCategoryEnum.ChannelMainImage] = (
                $@"^main_files/PodcastChannels/{GUID_PATTERN}/main_image{IMAGE_EXT}",
                FileAccessLevelEnum.Public,
                "Channel main image"
            ),
            [FileCategoryEnum.ChannelBackgroundImage] = (
                $@"^main_files/PodcastChannels/{GUID_PATTERN}/background_image{IMAGE_EXT}",
                FileAccessLevelEnum.Public,
                "Channel background image"
            ),
            [FileCategoryEnum.ShowMainImage] = (
                $@"^main_files/PodcastShows/{GUID_PATTERN}/main_image{IMAGE_EXT}",
                FileAccessLevelEnum.Public,
                "Show main image"
            ),
            [FileCategoryEnum.ShowTrailerAudio] = (
                $@"^main_files/PodcastShows/{GUID_PATTERN}/trailer_audio{AUDIO_EXT}",
                FileAccessLevelEnum.Public,
                "Show trailer audio"
            ),
            [FileCategoryEnum.EpisodeMainImage] = (
                $@"^main_files/PodcastEpisodes/{GUID_PATTERN}/main_image{IMAGE_EXT}",
                FileAccessLevelEnum.Public,
                "Episode main image"
            ),
            [FileCategoryEnum.BackgroundSoundTrackMainImage] = (
                $@"^main_files/PodcastBackgroundSoundTracks/{GUID_PATTERN}/main_image{IMAGE_EXT}",
                FileAccessLevelEnum.Public,
                "Background sound track main image"
            ),
            [FileCategoryEnum.BackgroundSoundTrackAudio] = (
                $@"^main_files/PodcastBackgroundSoundTracks/{GUID_PATTERN}/audio{AUDIO_EXT}",
                FileAccessLevelEnum.Public,
                "Background sound track audio"
            ),

            // ============ REQUIRES AUTH ============
            [FileCategoryEnum.BuddyCommitmentDocument] = (
                $@"^main_files/Accounts/{INT_PATTERN}/buddy_commitment_document{DOCUMENT_EXT}",
                FileAccessLevelEnum.RequiresOwnership,
                "Buddy commitment document"
            ),
            [FileCategoryEnum.EpisodeLicenseDocument] = (
                $@"^main_files/PodcastEpisodes/{GUID_PATTERN}/{GUID_PATTERN}_license_document({DOCUMENT_EXT}|{IMAGE_EXT})",
                FileAccessLevelEnum.RequiresOwnership,
                "Episode license document"
            ),
            [FileCategoryEnum.BookingDemoAudio] = (
                $@"^main_files/Bookings/{INT_PATTERN}/demo_audio{AUDIO_EXT}",
                FileAccessLevelEnum.RequiresAuth,
                "Booking demo audio"
            ),
            [FileCategoryEnum.BookingNegotiationAudio] = (
                $@"^main_files/Bookings/{INT_PATTERN}/{GUID_PATTERN}_negotiation_demo_audio{AUDIO_EXT}",
                FileAccessLevelEnum.RequiresAuth,
                "Booking negotiation audio"
            ),
            [FileCategoryEnum.BookingMessageAudio] = (
                $@"^main_files/Bookings/{INT_PATTERN}/{GUID_PATTERN}_message_audio{AUDIO_EXT}",
                FileAccessLevelEnum.RequiresAuth,
                "Booking chat message audio"
            ),
            [FileCategoryEnum.BookingRequirement] = (
                $@"^main_files/Bookings/{INT_PATTERN}/{GUID_PATTERN}_requirement_document({DOCUMENT_EXT}|{IMAGE_EXT}|{ARCHIVE_EXT})",
                FileAccessLevelEnum.RequiresAuth,
                "Booking requirement attachment"
            ),
            [FileCategoryEnum.DMCANotice] = (
                $@"^main_files/DMCAAccusations/{INT_PATTERN}/{GUID_PATTERN}_dmca_notice{DOCUMENT_EXT}",
                FileAccessLevelEnum.RequiresOwnership,
                "DMCA notice document"
            ),
            [FileCategoryEnum.CounterNotice] = (
                $@"^main_files/DMCAAccusations/{INT_PATTERN}/{GUID_PATTERN}_counter_notice{DOCUMENT_EXT}",
                FileAccessLevelEnum.RequiresOwnership,
                "DMCA counter notice"
            ),
            [FileCategoryEnum.LawsuitDocument] = (
                $@"^main_files/DMCAAccusations/{INT_PATTERN}/{GUID_PATTERN}_lawsuit_document{DOCUMENT_EXT}",
                FileAccessLevelEnum.RequiresOwnership,
                "Lawsuit proof document"
            ),

            // ============ THÊM MỚI - REQUIRES OWNERSHIP ============
            [FileCategoryEnum.WithdrawalRequestTransferReceiptImage] = (
                $@"^main_files/AccountBalanceWithdrawalRequests/{GUID_PATTERN}/transfer_receipt_image{IMAGE_EXT}",
                FileAccessLevelEnum.RequiresOwnership,
                "Withdrawal request transfer receipt image"
            ),

            // ============ REQUIRES TOKEN ============
            [FileCategoryEnum.EpisodeRawAudio] = (
                $@"^main_files/PodcastEpisodes/{GUID_PATTERN}/audio{AUDIO_EXT}",
                FileAccessLevelEnum.RequiresToken,
                "Episode raw audio (use HLS streaming)"
            ),
            [FileCategoryEnum.BookingTrackAudio] = (
                $@"^main_files/Bookings/{INT_PATTERN}/{GUID_PATTERN}_track_audio{AUDIO_EXT}",
                FileAccessLevelEnum.RequiresToken,
                "Booking track audio (use HLS streaming)"
            ),

            // ============ STRICT PRIVATE (Never direct download) ============
            [FileCategoryEnum.HlsPlaylist] = (
                $@"^main_files/PodcastEpisodes/{GUID_PATTERN}/playlist/playlist{M3U8_EXT}",
                FileAccessLevelEnum.StrictPrivate,
                "HLS playlist (use playback session)"
            ),
            [FileCategoryEnum.HlsSegment] = (
                // $@"^main_files/PodcastEpisodes/{GUID_PATTERN}/playlist/seg_\d+{TS_EXT}",
                $@"^main_files/PodcastEpisodes/{GUID_PATTERN}/playlist/seg_[0-9a-fA-F]{{8}}_\d+{TS_EXT}",
                FileAccessLevelEnum.StrictPrivate,
                "HLS segment (use playback session)"
            ),
            [FileCategoryEnum.EncryptionKey] = (
                $@"^main_files/PodcastEpisodes/{GUID_PATTERN}/playlist/enc{KEY_EXT}",
                FileAccessLevelEnum.StrictPrivate,
                "Encryption key (use key endpoint)"
            )
        };

        /// <summary>
        /// Validate file access với access level cụ thể
        /// </summary>
        public static FileAccessValidationResult ValidateFileAccess(string fileKey, FileAccessLevelEnum requiredAccessLevel)
        {
            if (string.IsNullOrWhiteSpace(fileKey))
            {
                return FileAccessValidationResult.Fail("File key cannot be empty", FileCategoryEnum.Unknown);
            }

            // Normalize
            fileKey = fileKey.Replace("\\", "/");

            // Find matching pattern
            foreach (var (category, (pattern, accessLevel, description)) in FilePatterns)
            {
                if (Regex.IsMatch(fileKey, pattern, RegexOptions.IgnoreCase))
                {
                    // Check if file's access level is compatible with required level
                    if (accessLevel <= requiredAccessLevel)
                    {
                        return FileAccessValidationResult.Success(category, accessLevel, description);
                    }
                    else
                    {
                        return FileAccessValidationResult.Fail(
                            $"Access denied: {description} requires {accessLevel} but only {requiredAccessLevel} provided",
                            category,
                            accessLevel
                        );
                    }
                }
            }

            // No pattern matched
            return FileAccessValidationResult.Fail("Invalid file key format", FileCategoryEnum.Unknown);
        }

        /// <summary>
        /// Quick validation cho public API (no auth)
        /// </summary>
        public static FileAccessValidationResult ValidatePublicAccess(string fileKey)
        {
            return ValidateFileAccess(fileKey, FileAccessLevelEnum.Public);
        }

        /// <summary>
        /// Validation cho authenticated API
        /// </summary>
        public static FileAccessValidationResult ValidateAuthenticatedAccess(string fileKey)
        {
            return ValidateFileAccess(fileKey, FileAccessLevelEnum.RequiresAuth);
        }

        /// <summary>
        /// Get file category và access level
        /// </summary>
        public static (FileCategoryEnum category, FileAccessLevelEnum accessLevel) GetFileCategoryAndLevel(string fileKey)
        {
            if (string.IsNullOrWhiteSpace(fileKey))
                return (FileCategoryEnum.Unknown, FileAccessLevelEnum.StrictPrivate);

            fileKey = fileKey.Replace("\\", "/");

            foreach (var (category, (pattern, accessLevel, _)) in FilePatterns)
            {
                if (Regex.IsMatch(fileKey, pattern, RegexOptions.IgnoreCase))
                {
                    return (category, accessLevel);
                }
            }

            return (FileCategoryEnum.Unknown, FileAccessLevelEnum.StrictPrivate);
        }

        /// <summary>
        /// Check if file requires ownership validation
        /// </summary>
        public static bool RequiresOwnershipCheck(string fileKey)
        {
            var (_, accessLevel) = GetFileCategoryAndLevel(fileKey);
            return accessLevel == FileAccessLevelEnum.RequiresOwnership;
        }

        #region ID Extractors

        public static int? ExtractAccountId(string fileKey)
        {
            var match = Regex.Match(fileKey, $@"^main_files/Accounts/({INT_PATTERN})/");
            return match.Success && int.TryParse(match.Groups[1].Value, out var id) ? id : null;
        }

        public static Guid? ExtractChannelId(string fileKey)
        {
            var match = Regex.Match(fileKey, $@"^main_files/PodcastChannels/({GUID_PATTERN})/");
            return match.Success && Guid.TryParse(match.Groups[1].Value, out var id) ? id : null;
        }

        public static Guid? ExtractShowId(string fileKey)
        {
            var match = Regex.Match(fileKey, $@"^main_files/PodcastShows/({GUID_PATTERN})/");
            return match.Success && Guid.TryParse(match.Groups[1].Value, out var id) ? id : null;
        }

        public static Guid? ExtractEpisodeId(string fileKey)
        {
            var match = Regex.Match(fileKey, $@"^main_files/PodcastEpisodes/({GUID_PATTERN})/");
            return match.Success && Guid.TryParse(match.Groups[1].Value, out var id) ? id : null;
        }

        public static int? ExtractBookingId(string fileKey)
        {
            var match = Regex.Match(fileKey, $@"^main_files/Bookings/({INT_PATTERN})/");
            return match.Success && int.TryParse(match.Groups[1].Value, out var id) ? id : null;
        }

        public static int? ExtractDMCAAccusationId(string fileKey)
        {
            var match = Regex.Match(fileKey, $@"^main_files/DMCAAccusations/({INT_PATTERN})/");
            return match.Success && int.TryParse(match.Groups[1].Value, out var id) ? id : null;
        }
        public static Guid? ExtractBackgroundSoundTrackId(string fileKey)
        {
            var match = Regex.Match(fileKey, $@"^main_files/PodcastBackgroundSoundTracks/({GUID_PATTERN})/");
            return match.Success && Guid.TryParse(match.Groups[1].Value, out var id) ? id : null;
        }

        public static Guid? ExtractWithdrawalRequestId(string fileKey)
        {
            var match = Regex.Match(fileKey, $@"^main_files/AccountBalanceWithdrawalRequests/({GUID_PATTERN})/");
            return match.Success && Guid.TryParse(match.Groups[1].Value, out var id) ? id : null;
        }

        #endregion
    }

    /// <summary>
    /// Validation result
    /// </summary>
    public class FileAccessValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public FileCategoryEnum Category { get; set; }
        public FileAccessLevelEnum RequiredAccessLevel { get; set; }
        public string? Description { get; set; }

        public static FileAccessValidationResult Success(FileCategoryEnum category, FileAccessLevelEnum accessLevel, string description)
        {
            return new FileAccessValidationResult
            {
                IsValid = true,
                Category = category,
                RequiredAccessLevel = accessLevel,
                Description = description
            };
        }

        public static FileAccessValidationResult Fail(string errorMessage, FileCategoryEnum category, FileAccessLevelEnum? accessLevel = null)
        {
            return new FileAccessValidationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage,
                Category = category,
                RequiredAccessLevel = accessLevel ?? FileAccessLevelEnum.StrictPrivate
            };
        }
    }
}