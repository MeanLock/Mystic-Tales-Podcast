namespace PodcastService.BusinessLogic.Enums.App
{
    public enum FileAccessLevelEnum
    {
        Public = 0,              // Không cần auth - cho public API
        RequiresAuth = 1,        // Cần login - cho authenticated API
        RequiresOwnership = 2,   // Cần là owner/participant
        RequiresToken = 3,       // Cần special token (HLS playback)
        StrictPrivate = 4        // Không bao giờ cho direct download
    }

    public enum FileCategoryEnum
    {
        Unknown,

        // Public files
        AccountMainImage,
        AccountBuddyTrailerAudio,
        ChannelMainImage,
        ChannelBackgroundImage,
        ShowMainImage,
        ShowTrailerAudio,
        EpisodeMainImage,
        BackgroundSoundTrackMainImage,
        BackgroundSoundTrackAudio,
        WithdrawalRequestTransferReceiptImage,

        // Authenticated files
        BuddyCommitmentDocument,
        EpisodeLicenseDocument,
        BookingDemoAudio,
        BookingNegotiationAudio,
        BookingMessageAudio,
        BookingRequirement,
        DMCANotice,
        CounterNotice,
        LawsuitDocument,

        // Token-required files
        EpisodeRawAudio,
        BookingTrackAudio,

        // Strict private (never direct download)
        PodcastEpisodeHlsPlaylist,
        PodcastEpisodeHlsSegment,
        PodcastEpisodeEncryptionKey
    }
}