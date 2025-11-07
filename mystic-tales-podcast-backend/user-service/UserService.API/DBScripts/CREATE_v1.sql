-- =====================================================
-- USER SERVICE DATABASE [Port: 8046]
-- =====================================================

-- Role table
CREATE TABLE Role (
    id INT PRIMARY KEY,
    name NVARCHAR(20) NOT NULL
);

-- Account table
CREATE TABLE Account (
    id INT IDENTITY(1,1) PRIMARY KEY,
    email VARCHAR(250) NOT NULL,
    password VARCHAR(250) NOT NULL,
    roleId INT NOT NULL,
    fullName NVARCHAR(250) NOT NULL,
    dob DATE NULL,
    gender NVARCHAR(250) NULL,
    address NVARCHAR(250) NULL,
    phone VARCHAR(20) NULL,
    balance DECIMAL(18,2) NOT NULL DEFAULT 0,
    mainImageFileKey NVARCHAR(MAX) NULL,
    isVerified BIT NOT NULL DEFAULT 0,
    googleId VARCHAR(250) NULL DEFAULT NULL,
    verifyCode NVARCHAR(250) NULL DEFAULT NULL,
    podcastListenSlot INT NULL,
    violationPoint INT NOT NULL DEFAULT 0,
    violationLevel INT NOT NULL DEFAULT 0,
    lastViolationPointChanged DATETIME NULL DEFAULT NULL,
    lastViolationLevelChanged DATETIME NULL DEFAULT NULL,
    lastPodcastListenSlotChanged DATETIME NULL DEFAULT NULL,
    deactivatedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (roleId) REFERENCES Role(id)
);


-- PasswordResetToken table
CREATE TABLE PasswordResetToken (
    id INT IDENTITY(1,1) PRIMARY KEY,
    accountId INT NOT NULL,
    token NVARCHAR(250) NOT NULL,
    expiredAt DATETIME NOT NULL,
    isUsed BIT NOT NULL DEFAULT 0,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (accountId) REFERENCES Account(id)
);

-- PodcasterProfile table
CREATE TABLE PodcasterProfile (
    accountId INT PRIMARY KEY,
    name NVARCHAR(250) NOT NULL,
    description NVARCHAR(MAX) NOT NULL DEFAULT '',
    averageRating FLOAT NOT NULL DEFAULT 0,
    ratingCount INT NOT NULL DEFAULT 0,
    totalFollow INT NOT NULL DEFAULT 0,
    listenCount INT NOT NULL DEFAULT 0,
    commitmentDocumentFileKey NVARCHAR(MAX) NOT NULL,
    buddyAudioFileKey NVARCHAR(MAX) NULL,
    ownedBookingStorageSize FLOAT NOT NULL,
    usedBookingStorageSize FLOAT NOT NULL,
    isVerified BIT NULL DEFAULT NULL,
    pricePerBookingWord DECIMAL(18,2) NULL DEFAULT NULL,
    verifiedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (accountId) REFERENCES Account(id)
);

-- AccountFollowedPodcaster table
CREATE TABLE AccountFollowedPodcaster (
    accountId INT NOT NULL,
    podcasterId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    PRIMARY KEY (accountId, podcasterId),
    FOREIGN KEY (accountId) REFERENCES Account(id),
    FOREIGN KEY (podcasterId) REFERENCES Account(id)
);

-- AccountFavoritedPodcastChannel table
CREATE TABLE AccountFavoritedPodcastChannel (
    accountId INT NOT NULL,
    podcastChannelId UNIQUEIDENTIFIER NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    PRIMARY KEY (accountId, podcastChannelId),
    FOREIGN KEY (accountId) REFERENCES Account(id)
);

-- AccountFollowedPodcastShow table
CREATE TABLE AccountFollowedPodcastShow (
    accountId INT NOT NULL,
    podcastShowId UNIQUEIDENTIFIER NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    PRIMARY KEY (accountId, podcastShowId),
    FOREIGN KEY (accountId) REFERENCES Account(id)
);

-- AccountSavedPodcastEpisode table
CREATE TABLE AccountSavedPodcastEpisode (
    accountId INT NOT NULL,
    podcastEpisodeId UNIQUEIDENTIFIER NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    PRIMARY KEY (accountId, podcastEpisodeId),
    FOREIGN KEY (accountId) REFERENCES Account(id)
);

-- PodcastBuddyReview table
CREATE TABLE PodcastBuddyReview (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    title NVARCHAR(250) NULL,
    content NVARCHAR(MAX) NULL,
    rating FLOAT NOT NULL,
    accountId INT NOT NULL,
    podcastBuddyId INT NOT NULL,
    deletedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (accountId) REFERENCES Account(id),
    FOREIGN KEY (podcastBuddyId) REFERENCES Account(id)
);

-- AccountNotification table
CREATE TABLE AccountNotification (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    accountId INT NOT NULL,
    content NVARCHAR(MAX) NOT NULL,
    notificationTypeId INT NOT NULL,
    isSeen BIT NOT NULL DEFAULT 0,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (accountId) REFERENCES Account(id)
    FOREIGN KEY (notificationTypeId) REFERENCES NotificationType(id)
);

-- NotificationType table
CREATE TABLE NotificationType (
    id INT PRIMARY KEY,
    name NVARCHAR(250) NOT NULL
);

-- =====================================================
-- SYSTEM CONFIGURATION SERVICE DATABASE [Port: 8051]
-- =====================================================

-- SystemConfigProfile table
CREATE TABLE SystemConfigProfile (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(MAX) NOT NULL,
    isActive BIT NOT NULL DEFAULT 0,
    deletedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME))
);

-- PodcastSubscriptionConfig table
CREATE TABLE PodcastSubscriptionConfig (
    configProfileId INT NOT NULL,
    subscriptionCycleTypeId INT NOT NULL,
    profitRate FLOAT NOT NULL,
    incomeTakenDelayDays INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    PRIMARY KEY (configProfileId, subscriptionCycleTypeId),
    FOREIGN KEY (configProfileId) REFERENCES SystemConfigProfile(id)
);

-- PodcastSuggestionConfig table
CREATE TABLE PodcastSuggestionConfig (
    configProfileId INT PRIMARY KEY,
    behaviorLookbackDayCount INT NOT NULL,
    minChannelQuery INT NOT NULL,
    minShowQuery INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (configProfileId) REFERENCES SystemConfigProfile(id)
);

-- BookingConfig table
CREATE TABLE BookingConfig (
    configProfileId INT PRIMARY KEY,
    profitRate FLOAT NOT NULL,
    depositRate FLOAT NOT NULL,
    podcastTrackPreviewListenSlot INT NOT NULL,
    previewResponseAllowedDays INT NOT NULL,
    producingRequestResponseAllowedDays INT NOT NULL,
    chatRoomExpiredHours INT NOT NULL,
    chatRoomFileMessageExpiredHours INT NOT NULL,
    freeInitialBookingStorageSize FLOAT NOT NULL,
    singleStorageUnitPurchasePrice DECIMAL(18,2) NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (configProfileId) REFERENCES SystemConfigProfile(id)
);

-- AccountConfig table
CREATE TABLE AccountConfig (
    configProfileId INT PRIMARY KEY,
    violationPointDecayHours INT NOT NULL,
    podcastListenSlotThreshold INT NOT NULL,
    podcastListenSlotRecoverySeconds INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (configProfileId) REFERENCES SystemConfigProfile(id)
);

-- AccountViolationLevelConfig table
CREATE TABLE AccountViolationLevelConfig (
    configProfileId INT NOT NULL,
    violationLevel INT NOT NULL,
    violationPointThreshold INT NOT NULL,
    punishmentDays INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    PRIMARY KEY (configProfileId, violationLevel),
    FOREIGN KEY (configProfileId) REFERENCES SystemConfigProfile(id)
);

-- ReviewSessionConfig table
CREATE TABLE ReviewSessionConfig (
    configProfileId INT PRIMARY KEY,
    podcastBuddyUnResolvedReportStreak INT NOT NULL,
    podcastShowUnResolvedReportStreak INT NOT NULL,
    podcastEpisodeUnResolvedReportStreak INT NOT NULL,
    podcastEpisodePublishEditRequirementExpiredHours INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (configProfileId) REFERENCES SystemConfigProfile(id)
);

-- PodcastRestrictedTerm table
CREATE TABLE PodcastRestrictedTerm (
    id INT IDENTITY(1,1) PRIMARY KEY,
    term NVARCHAR(100) NOT NULL
);

-- =====================================================
-- BOOKING MANAGEMENT SERVICE DATABASE [Port: 8056]
-- =====================================================

-- BookingStatus table
CREATE TABLE BookingStatus (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- BookingOptionalManualCancelReason table
CREATE TABLE BookingOptionalManualCancelReason (
    id INT PRIMARY KEY,
    name NVARCHAR(250) NOT NULL
);

-- Booking table
CREATE TABLE Booking (
    id INT IDENTITY(1,1) PRIMARY KEY,
    title NVARCHAR(250) NOT NULL,
    description NVARCHAR(MAX) NOT NULL DEFAULT '',
    accountId INT NOT NULL,
    podcastBuddyId INT NOT NULL,
    price DECIMAL(18,2) NULL,
    deadline DATE NULL,
    demoAudioFileKey NVARCHAR(MAX) NULL,
    bookingManualCancelledReason NVARCHAR(MAX) NULL,
    bookingAutoCancelReason NVARCHAR(MAX) NULL,
    assignedStaffId INT NULL,
    customerBookingCancelDepositRefundRate FLOAT NULL DEFAULT NULL,
    podcastBuddyBookingCancelDepositRefundRate FLOAT NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME))
);

-- PodcastBookingToneCategory table
CREATE TABLE PodcastBookingToneCategory (
    id INT PRIMARY KEY,
    name NVARCHAR(100) NOT NULL
);

-- PodcastBookingTone table
CREATE TABLE PodcastBookingTone (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(500) NULL DEFAULT NULL,
    podcastBookingToneCategoryId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    deletedAt DATETIME NULL DEFAULT NULL,
    FOREIGN KEY (podcastBookingToneCategoryId) REFERENCES PodcastBookingToneCategory(id)
);

-- PodcastBuddyBookingTone table
CREATE TABLE PodcastBuddyBookingTone (
    podcasterId INT NOT NULL,
    podcastBookingToneId UNIQUEIDENTIFIER NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    PRIMARY KEY (podcasterId, podcastBookingToneId),
    FOREIGN KEY (podcastBookingToneId) REFERENCES PodcastBookingTone(id)
);

-- BookingRequirement table
CREATE TABLE BookingRequirement (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    bookingId INT NOT NULL,
    name NVARCHAR(MAX) NOT NULL DEFAULT '',
    description NVARCHAR(MAX) NOT NULL DEFAULT '',
    requirementDocumentFileKey NVARCHAR(MAX) NOT NULL,
    [order] INT NOT NULL,
    wordCount INT NOT NULL,
    podcastBookingToneId UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY (bookingId) REFERENCES Booking(id),
    FOREIGN KEY (podcastBookingToneId) REFERENCES PodcastBookingTone(id)
);

-- BookingStatusTracking table
CREATE TABLE BookingStatusTracking (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    bookingId INT NOT NULL,
    bookingStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (bookingId) REFERENCES Booking(id),
    FOREIGN KEY (bookingStatusId) REFERENCES BookingStatus(id)
);

-- BookingProducingRequest table
CREATE TABLE BookingProducingRequest (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    bookingId INT NOT NULL,
    note NVARCHAR(MAX) NOT NULL DEFAULT '',
    deadline DATE NOT NULL,
    isAccepted BIT NULL,
    finishedAt DATETIME NULL DEFAULT NULL,
    rejectReason NVARCHAR(MAX) NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (bookingId) REFERENCES Booking(id)
);

-- BookingPodcastTrack table
CREATE TABLE BookingPodcastTrack (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    bookingId INT NOT NULL,
    bookingProducingRequestId UNIQUEIDENTIFIER NOT NULL,
    audioFileKey NVARCHAR(MAX) NOT NULL,
    audioFileSize FLOAT NOT NULL,
    audioLength INT NOT NULL,
    audioEncryptionKeyId UNIQUEIDENTIFIER NULL DEFAULT NULL,
    audioEncryptionKeyFileKey NVARCHAR(MAX) NULL DEFAULT NULL,
    remainingPreviewListenSlot INT NOT NULL,
    bookingRequirementId UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY (bookingId) REFERENCES Booking(id),
    FOREIGN KEY (bookingProducingRequestId) REFERENCES BookingProducingRequest(id),
    FOREIGN KEY (bookingRequirementId) REFERENCES BookingRequirement(id)
);

-- BookingProducingRequestPodcastTrackToEdit table
CREATE TABLE BookingProducingRequestPodcastTrackToEdit (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    bookingProducingRequestId UNIQUEIDENTIFIER NOT NULL,
    bookingPodcastTrackId UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY (bookingProducingRequestId) REFERENCES BookingProducingRequest(id),
    FOREIGN KEY (bookingPodcastTrackId) REFERENCES BookingPodcastTrack(id)
);

-- BookingChatRoom table
CREATE TABLE BookingChatRoom (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    bookingId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (bookingId) REFERENCES Booking(id)
);

-- BookingChatMessages table
CREATE TABLE BookingChatMessages (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    text NVARCHAR(MAX) NULL,
    audioFileKey NVARCHAR(MAX) NULL,
    chatRoomId UNIQUEIDENTIFIER NOT NULL,
    senderId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (chatRoomId) REFERENCES BookingChatRoom(id)
);

-- BookingChatMember table
CREATE TABLE BookingChatMember (
    chatRoomId UNIQUEIDENTIFIER NOT NULL,
    accountId INT NOT NULL,
    PRIMARY KEY (chatRoomId, accountId),
    FOREIGN KEY (chatRoomId) REFERENCES BookingChatRoom(id)
);

-- =====================================================
-- PODCAST SERVICE DATABASE [Port: 8061]
-- =====================================================

-- PodcastCategory table
CREATE TABLE PodcastCategory (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- PodcastSubCategory table
CREATE TABLE PodcastSubCategory (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL,
    podcastCategoryId INT NOT NULL,
    FOREIGN KEY (podcastCategoryId) REFERENCES PodcastCategory(id)
);

-- PodcastChannelStatus table
CREATE TABLE PodcastChannelStatus (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- PodcastShowStatus table
CREATE TABLE PodcastShowStatus (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- PodcastEpisodeStatus table
CREATE TABLE PodcastEpisodeStatus (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- PodcastEpisodeSubscriptionType table
CREATE TABLE PodcastEpisodeSubscriptionType (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- PodcastShowSubscriptionType table
CREATE TABLE PodcastShowSubscriptionType (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- PodcastBackgroundSoundTrack table
CREATE TABLE PodcastBackgroundSoundTrack
(
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    name NVARCHAR(50) NOT NULL,
    description NVARCHAR(MAX) NULL,
    mainImageFileKey NVARCHAR(MAX) NULL,
    audioFileKey NVARCHAR(MAX) NULL,
    createdAt DATETIME NOT NULL DEFAULT GETDATE(),
    updatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- PodcastIllegalContentType table
CREATE TABLE PodcastIllegalContentType (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- PodcastEpisodeLicenseType table
CREATE TABLE PodcastEpisodeLicenseType (
    id INT PRIMARY KEY,
    name NVARCHAR(100) NOT NULL
);

-- PodcastEpisodePublishReviewSessionStatus table
CREATE TABLE PodcastEpisodePublishReviewSessionStatus (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- PodcastChannel table
CREATE TABLE PodcastChannel (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    name NVARCHAR(250) NOT NULL,
    description NVARCHAR(MAX) NOT NULL DEFAULT '',
    backgroundImageFileKey NVARCHAR(MAX) NULL,
    mainImageFileKey NVARCHAR(MAX) NULL,
    totalFavorite INT NOT NULL DEFAULT 0,
    listenCount INT NOT NULL DEFAULT 0,
    podcasterId INT NOT NULL,
    podcastCategoryId INT NULL,
    podcastSubCategoryId INT NULL,
    deletedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastCategoryId) REFERENCES PodcastCategory(id),
    FOREIGN KEY (podcastSubCategoryId) REFERENCES PodcastSubCategory(id)
);

-- PodcastShow table
CREATE TABLE PodcastShow (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    name NVARCHAR(250) NOT NULL,
    description NVARCHAR(MAX) NOT NULL DEFAULT '',
    language NVARCHAR(50) NOT NULL,
    releaseDate DATE NULL,
    isReleased BIT NULL,
    copyright NVARCHAR(250) NOT NULL,
    uploadFrequency NVARCHAR(MAX) NULL,
    averageRating FLOAT NOT NULL DEFAULT 0,
    ratingCount INT NOT NULL DEFAULT 0,
    mainImageFileKey NVARCHAR(MAX) NULL,
    trailerAudioFileKey NVARCHAR(MAX) NULL,
    totalFollow INT NOT NULL DEFAULT 0,
    listenCount INT NOT NULL DEFAULT 0,
    podcasterId INT NOT NULL,
    podcastCategoryId INT NULL,
    podcastSubCategoryId INT NULL,
    podcastShowSubscriptionTypeId INT NOT NULL DEFAULT 1,
    podcastChannelId UNIQUEIDENTIFIER NULL,
    takenDownReason NVARCHAR(MAX) NULL,
    deletedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastCategoryId) REFERENCES PodcastCategory(id),
    FOREIGN KEY (podcastSubCategoryId) REFERENCES PodcastSubCategory(id),
    FOREIGN KEY (podcastShowSubscriptionTypeId) REFERENCES PodcastShowSubscriptionType(id),
    FOREIGN KEY (podcastChannelId) REFERENCES PodcastChannel(id)
);

-- PodcastEpisode table
CREATE TABLE PodcastEpisode (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    name NVARCHAR(250) NOT NULL,
    description NVARCHAR(MAX) NOT NULL DEFAULT '',
    explicitContent BIT NOT NULL DEFAULT 0,
    releaseDate DATE NULL,
    isReleased BIT NULL,
    mainImageFileKey NVARCHAR(MAX) NULL,
    audioFileKey NVARCHAR(MAX) NULL DEFAULT NULL,
    audioFileSize FLOAT NULL DEFAULT NULL,
    audioLength INT NULL DEFAULT NULL,
    audioFingerPrint VARBINARY(MAX) NULL DEFAULT NULL,
    audioTranscript NVARCHAR(MAX) NULL DEFAULT NULL,
    audioEncryptionKeyId UNIQUEIDENTIFIER NULL DEFAULT NULL,
    audioEncryptionKeyFileKey NVARCHAR(MAX) NULL DEFAULT NULL,
    podcastEpisodeSubscriptionTypeId INT NOT NULL DEFAULT 1,
    podcastShowId UNIQUEIDENTIFIER NOT NULL,
    seasonNumber INT NOT NULL DEFAULT 0,
    episodeOrder INT NOT NULL DEFAULT 1,
    totalSave INT NOT NULL DEFAULT 0,
    listenCount INT NOT NULL DEFAULT 0,
    isAudioPublishable BIT NULL,
    takenDownReason NVARCHAR(MAX) NULL,
    deletedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastEpisodeSubscriptionTypeId) REFERENCES PodcastEpisodeSubscriptionType(id),
    FOREIGN KEY (podcastShowId) REFERENCES PodcastShow(id)
);

-- PodcastEpisodeListenSession table
CREATE TABLE PodcastEpisodeListenSession (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    accountId INT NOT NULL,
    podcastEpisodeId UNIQUEIDENTIFIER NOT NULL,
    lastListenDurationSeconds INT NOT NULL DEFAULT 0,
    isCompleted BIT NOT NULL DEFAULT 0,
    isContentRemoved BIT NOT NULL DEFAULT 0,
    podcastCategoryId INT NULL,
    podcastSubCategoryId INT NULL;
    expiredAt DATETIME NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastEpisodeId) REFERENCES PodcastEpisode(id)
    FOREIGN KEY (podcastCategoryId) REFERENCES PodcastCategory(id),
    FOREIGN KEY (podcastSubCategoryId) REFERENCES PodcastSubCategory(id)
);

-- PodcastEpisodeListenSessionHlsEnckeyRequestToken table
CREATE TABLE PodcastEpisodeListenSessionHlsEnckeyRequestToken (
    podcastEpisodeListenSessionId UNIQUEIDENTIFIER NOT NULL,
    token NVARCHAR(500) NOT NULL, -- Changed from NVARCHAR(MAX) to allow use as primary key
    isUsed BIT NOT NULL DEFAULT 0,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    -- Composite primary key
    CONSTRAINT PK_PodcastEpisodeListenSessionHlsEnckeyRequestToken 
        PRIMARY KEY (podcastEpisodeListenSessionId, token),
    -- Foreign key to PodcastEpisodeListenSession
    CONSTRAINT FK_PodcastEpisodeListenSessionHlsEnckeyRequestToken_Session
        FOREIGN KEY (podcastEpisodeListenSessionId) 
        REFERENCES PodcastEpisodeListenSession(id)
        ON DELETE CASCADE -- Automatically delete tokens when session is deleted
);

-- PodcastEpisodeLicense table
CREATE TABLE PodcastEpisodeLicense (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    podcastEpisodeId UNIQUEIDENTIFIER NOT NULL,
    licenseDocumentFileKey NVARCHAR(MAX) NOT NULL,
    podcastEpisodeLicenseTypeId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastEpisodeId) REFERENCES PodcastEpisode(id),
    FOREIGN KEY (podcastEpisodeLicenseTypeId) REFERENCES PodcastEpisodeLicenseType(id)
);

-- PodcastEpisodeIllegalContentTypeMarking table
CREATE TABLE PodcastEpisodeIllegalContentTypeMarking (
    podcastEpisodeId UNIQUEIDENTIFIER NOT NULL,
    podcastIllegalContentTypeId INT NOT NULL,
    markerId INT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    PRIMARY KEY (podcastEpisodeId, podcastIllegalContentTypeId),
    FOREIGN KEY (podcastEpisodeId) REFERENCES PodcastEpisode(id),
    FOREIGN KEY (podcastIllegalContentTypeId) REFERENCES PodcastIllegalContentType(id)
);

CREATE TABLE PodcastEpisodePublishDuplicateDetection (
    podcastEpisodePublishReviewSessionId INT NOT NULL,
    duplicatePodcastEpisodeId UNIQUEIDENTIFIER NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
	PRIMARY KEY (podcastEpisodePublishReviewSessionId, duplicatePodcastEpisodeId),
    FOREIGN KEY (podcastEpisodePublishReviewSessionId) REFERENCES PodcastEpisodePublishReviewSession(id),
    FOREIGN KEY (duplicatePodcastEpisodeId) REFERENCES PodcastEpisode(id),
);

-- PodcastEpisodePublishReviewSession table
CREATE TABLE PodcastEpisodePublishReviewSession (
    id INT IDENTITY(1,1) PRIMARY KEY,
    assignedStaff INT NOT NULL,
    podcastEpisodeId UNIQUEIDENTIFIER NOT NULL,
    note NVARCHAR(MAX) NULL,
    reReviewCount INT NOT NULL DEFAULT 0,
    deadline DATETIME NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastEpisodeId) REFERENCES PodcastEpisode(id)
);

-- PodcastEpisodePublishReviewSessionStatusTracking table
CREATE TABLE PodcastEpisodePublishReviewSessionStatusTracking (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    podcastEpisodePublishReviewSessionId INT NOT NULL,
    podcastEpisodePublishReviewSessionStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastEpisodePublishReviewSessionId) REFERENCES PodcastEpisodePublishReviewSession(id),
    FOREIGN KEY (podcastEpisodePublishReviewSessionStatusId) REFERENCES PodcastEpisodePublishReviewSessionStatus(id)
);

-- PodcastChannelStatusTracking table
CREATE TABLE PodcastChannelStatusTracking (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    podcastChannelId UNIQUEIDENTIFIER NOT NULL,
    podcastChannelStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastChannelId) REFERENCES PodcastChannel(id),
    FOREIGN KEY (podcastChannelStatusId) REFERENCES PodcastChannelStatus(id)
);

-- PodcastShowStatusTracking table
CREATE TABLE PodcastShowStatusTracking (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    podcastShowId UNIQUEIDENTIFIER NOT NULL,
    podcastShowStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastShowId) REFERENCES PodcastShow(id),
    FOREIGN KEY (podcastShowStatusId) REFERENCES PodcastShowStatus(id)
);

-- PodcastEpisodeStatusTracking table
CREATE TABLE PodcastEpisodeStatusTracking (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    podcastEpisodeId UNIQUEIDENTIFIER NOT NULL,
    podcastEpisodeStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastEpisodeId) REFERENCES PodcastEpisode(id),
    FOREIGN KEY (podcastEpisodeStatusId) REFERENCES PodcastEpisodeStatus(id)
);

-- PodcastShowReview table
CREATE TABLE PodcastShowReview (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    title NVARCHAR(250) NULL,
    content NVARCHAR(MAX) NULL,
    rating FLOAT NOT NULL,
    accountId INT NOT NULL,
    podcastShowId UNIQUEIDENTIFIER NOT NULL,
    deletedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastShowId) REFERENCES PodcastShow(id)
);

-- Hashtag table
CREATE TABLE Hashtag (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(250) NOT NULL
);

-- PodcastChannelHashtag table
CREATE TABLE PodcastChannelHashtag (
    podcastChannelId UNIQUEIDENTIFIER NOT NULL,
    hashtagId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    PRIMARY KEY (podcastChannelId, hashtagId),
    FOREIGN KEY (podcastChannelId) REFERENCES PodcastChannel(id),
    FOREIGN KEY (hashtagId) REFERENCES Hashtag(id)
);

-- PodcastShowHashtag table
CREATE TABLE PodcastShowHashtag (
    podcastShowId UNIQUEIDENTIFIER NOT NULL,
    hashtagId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    PRIMARY KEY (podcastShowId, hashtagId),
    FOREIGN KEY (podcastShowId) REFERENCES PodcastShow(id),
    FOREIGN KEY (hashtagId) REFERENCES Hashtag(id)
);

-- PodcastEpisodeHashtag table
CREATE TABLE PodcastEpisodeHashtag (
    podcastEpisodeId UNIQUEIDENTIFIER NOT NULL,
    hashtagId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    PRIMARY KEY (podcastEpisodeId, hashtagId),
    FOREIGN KEY (podcastEpisodeId) REFERENCES PodcastEpisode(id),
    FOREIGN KEY (hashtagId) REFERENCES Hashtag(id)
);

-- =====================================================
-- SUBSCRIPTION SERVICE DATABASE [Port: 8066]
-- =====================================================

-- SubscriptionCycleType table
CREATE TABLE SubscriptionCycleType (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- PodcastSubscriptionBenefit table
CREATE TABLE PodcastSubscriptionBenefit (
    id INT PRIMARY KEY,
    name NVARCHAR(250) NOT NULL
);

-- MemberSubscriptionBenefit table
CREATE TABLE MemberSubscriptionBenefit (
    id INT PRIMARY KEY,
    name NVARCHAR(250) NOT NULL
);

-- PodcastSubscription table
CREATE TABLE PodcastSubscription (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(250) NOT NULL,
    description NVARCHAR(MAX) NOT NULL DEFAULT '',
    podcastChannelId UNIQUEIDENTIFIER NULL,
    podcastShowId UNIQUEIDENTIFIER NULL,
    isActive BIT NOT NULL DEFAULT 0,
    currentVersion INT NOT NULL,
    deletedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME))
);

-- PodcastSubscriptionCycleTypePrice table
CREATE TABLE PodcastSubscriptionCycleTypePrice (
    podcastSubscriptionId INT NOT NULL,
    subscriptionCycleTypeId INT NOT NULL,
    version INT NOT NULL,
    price DECIMAL(18,2) NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    PRIMARY KEY (podcastSubscriptionId, subscriptionCycleTypeId, version),
    FOREIGN KEY (podcastSubscriptionId) REFERENCES PodcastSubscription(id),
    FOREIGN KEY (subscriptionCycleTypeId) REFERENCES SubscriptionCycleType(id)
);

-- PodcastSubscriptionBenefitMapping table
CREATE TABLE PodcastSubscriptionBenefitMapping (
    podcastSubscriptionId INT NOT NULL,
    podcastSubscriptionBenefitId INT NOT NULL,
    version INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    PRIMARY KEY (podcastSubscriptionId, podcastSubscriptionBenefitId, version),
    FOREIGN KEY (podcastSubscriptionId) REFERENCES PodcastSubscription(id),
    FOREIGN KEY (podcastSubscriptionBenefitId) REFERENCES PodcastSubscriptionBenefit(id)
);

-- PodcastSubscriptionRegistration table
CREATE TABLE PodcastSubscriptionRegistration (
	id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    accountId INT,
    podcastSubscriptionId INT NOT NULL,
    subscriptionCycleTypeId INT NOT NULL,
    currentVersion INT NOT NULL,
    isAcceptNewestVersionSwitch BIT NULL DEFAULT NULL,
    lastPaidAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    isIncomeTaken BIT NOT NULL DEFAULT 0,
    cancelledAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastSubscriptionId) REFERENCES PodcastSubscription(id),
    FOREIGN KEY (subscriptionCycleTypeId) REFERENCES SubscriptionCycleType(id)
);


-- MemberSubscription table
CREATE TABLE MemberSubscription (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(250) NOT NULL,
    description NVARCHAR(MAX) NOT NULL DEFAULT '',
    isActive BIT NOT NULL DEFAULT 0,
    isSubscribable BIT NOT NULL DEFAULT 0,
    currentVersion INT NOT NULL,
    deletedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME))
);

-- MemberSubscriptionCycleTypePrice table
CREATE TABLE MemberSubscriptionCycleTypePrice (
    memberSubscriptionId INT NOT NULL,
    subscriptionCycleTypeId INT NOT NULL,
    version INT NOT NULL,
    price DECIMAL(18,2) NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    PRIMARY KEY (memberSubscriptionId, subscriptionCycleTypeId, version),
    FOREIGN KEY (memberSubscriptionId) REFERENCES MemberSubscription(id),
    FOREIGN KEY (subscriptionCycleTypeId) REFERENCES SubscriptionCycleType(id)
);

-- MemberSubscriptionBenefitMapping table
CREATE TABLE MemberSubscriptionBenefitMapping (
    memberSubscriptionId INT NOT NULL,
    memberSubscriptionBenefitId INT NOT NULL,
    version INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    PRIMARY KEY (memberSubscriptionId, memberSubscriptionBenefitId, version),
    FOREIGN KEY (memberSubscriptionId) REFERENCES MemberSubscription(id),
    FOREIGN KEY (memberSubscriptionBenefitId) REFERENCES MemberSubscriptionBenefit(id)
);

-- MemberSubscriptionRegistration table
CREATE TABLE MemberSubscriptionRegistration (
	id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    accountId INT,
    memberSubscriptionId INT NOT NULL,
    currentVersion INT NOT NULL,
    isAcceptNewestVersionSwitch BIT NULL,
    lastPaidAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    cancelledAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (memberSubscriptionId) REFERENCES MemberSubscription(id)
);

-- =====================================================
-- MODERATION SERVICE DATABASE [Port: 8071]
-- =====================================================

-- PodcastBuddyReportType table
CREATE TABLE PodcastBuddyReportType (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- PodcastShowReportType table
CREATE TABLE PodcastShowReportType (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- PodcastEpisodeReportType table
CREATE TABLE PodcastEpisodeReportType (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- DMCAAccusationStatus table
CREATE TABLE DMCAAccusationStatus (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- PodcastBuddyReport table
CREATE TABLE PodcastBuddyReport (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    content NVARCHAR(MAX) NULL,
    accountId INT NOT NULL,
    podcastBuddyId INT NOT NULL,
    podcastBuddyReportTypeId INT NOT NULL,
    resolvedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastBuddyReportTypeId) REFERENCES PodcastBuddyReportType(id)
);

-- PodcastShowReport table
CREATE TABLE PodcastShowReport (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    content NVARCHAR(MAX) NULL,
    accountId INT NOT NULL,
    podcastShowId UNIQUEIDENTIFIER NOT NULL,
    podcastShowReportTypeId INT NOT NULL,
    resolvedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastShowReportTypeId) REFERENCES PodcastShowReportType(id)
);

-- PodcastEpisodeReport table
CREATE TABLE PodcastEpisodeReport (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    content NVARCHAR(MAX) NULL,
    accountId INT NOT NULL,
    podcastEpisodeId UNIQUEIDENTIFIER NOT NULL,
    podcastEpisodeReportTypeId INT NOT NULL,
    resolvedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastEpisodeReportTypeId) REFERENCES PodcastEpisodeReportType(id)
);

-- PodcastBuddyReportReviewSession table
CREATE TABLE PodcastBuddyReportReviewSession (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    podcastBuddyId INT NOT NULL,
    assignedStaff INT NOT NULL,
    resolvedViolationPoint INT NOT NULL DEFAULT 1,
    isResolved BIT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME))
);

-- PodcastShowReportReviewSession table
CREATE TABLE PodcastShowReportReviewSession (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    podcastShowId UNIQUEIDENTIFIER NOT NULL,
    assignedStaff INT NOT NULL,
    isResolved BIT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME))
);

-- PodcastEpisodeReportReviewSession table
CREATE TABLE PodcastEpisodeReportReviewSession (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    podcastEpisodeId UNIQUEIDENTIFIER NOT NULL,
    assignedStaff INT NOT NULL,
    isResolved BIT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME))
);

-- DMCAAccusation table
CREATE TABLE DMCAAccusation (
    id INT IDENTITY(1,1) PRIMARY KEY,
    podcastShowId UNIQUEIDENTIFIER NULL,
    podcastEpisodeId UNIQUEIDENTIFIER NULL,
    assignedStaff INT NULL,
    accuserEmail NVARCHAR(254) NOT NULL,
    accuserPhone NVARCHAR(20) NOT NULL,
    accuserFullName NVARCHAR(500) NOT NULL,
    dismissReason NVARCHAR(MAX) NULL,
    resolvedAt DATETIME NULL DEFAULT NULL,
    cancelledAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME))
);

-- DMCAAccusationConclusionReportType table
CREATE TABLE DMCAAccusationConclusionReportType (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- DMCAAccusationConclusionReport table
CREATE TABLE DMCAAccusationConclusionReport (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    dmcaAccusationId INT NOT NULL,
    dmcaAccusationConclusionReportTypeId INT NOT NULL,
    description NVARCHAR(MAX) NULL,
    invalidReason NVARCHAR(MAX) NULL,
    isRejected BIT NULL DEFAULT NULL,
    completedAt DATETIME NULL DEFAULT NULL,
    cancelledAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (dmcaAccusationId) REFERENCES DMCAAccusation(id),
    FOREIGN KEY (dmcaAccusationConclusionReportTypeId) REFERENCES DMCAAccusationConclusionReportType(id)
);

-- CounterNotice table
CREATE TABLE CounterNotice (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    dmcaAccusationId INT NOT NULL,
    isValid BIT NULL,
    invalidReason NVARCHAR(MAX) NULL,
    validatedBy INT NULL DEFAULT NULL,
    validatedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (dmcaAccusationId) REFERENCES DMCAAccusation(id)
);

-- DMCANotice table
CREATE TABLE DMCANotice (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    isValid BIT NULL,
    invalidReason NVARCHAR(MAX) NULL,
    validatedBy INT NULL DEFAULT NULL,
    validatedAt DATETIME NULL DEFAULT NULL,
    dmcaAccusationId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (dmcaAccusationId) REFERENCES DMCAAccusation(id)
);

-- LawsuitProof table
CREATE TABLE LawsuitProof (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    dmcaAccusationId INT NOT NULL,
    isValid BIT NULL,
    inValidReason NVARCHAR(MAX) NULL,
    validatedBy INT NULL DEFAULT NULL,
    validatedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (dmcaAccusationId) REFERENCES DMCAAccusation(id)
);

-- DMCAAccusationStatusTracking table
CREATE TABLE DMCAAccusationStatusTracking (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    dmcaAccusationId INT NOT NULL,
    dmcaAccusationStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (dmcaAccusationId) REFERENCES DMCAAccusation(id),
    FOREIGN KEY (dmcaAccusationStatusId) REFERENCES DMCAAccusationStatus(id)
);

-- CounterNoticeAttachFile table
CREATE TABLE CounterNoticeAttachFile (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    counterNoticeId UNIQUEIDENTIFIER NOT NULL,
    attachFileKey NVARCHAR(MAX) NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (counterNoticeId) REFERENCES CounterNotice(id)
);

-- DMCANoticeAttachFile table
CREATE TABLE DMCANoticeAttachFile (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    dmcaNoticeId UNIQUEIDENTIFIER NOT NULL,
    attachFileKey NVARCHAR(MAX) NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (dmcaNoticeId) REFERENCES DMCANotice(id)
);

-- LawsuitProofAttachFile table
CREATE TABLE LawsuitProofAttachFile (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    lawsuitProofId UNIQUEIDENTIFIER NOT NULL,
    attachFileKey NVARCHAR(MAX) NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (lawsuitProofId) REFERENCES LawsuitProof(id)
);

-- =====================================================
-- TRANSACTION SERVICE DATABASE [Port: 8076]
-- =====================================================

-- TransactionType table
CREATE TABLE TransactionType (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- TransactionStatus table
CREATE TABLE TransactionStatus (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- AccountBalanceTransaction table
CREATE TABLE AccountBalanceTransaction (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    accountId INT NOT NULL,
    orderCode NVARCHAR(MAX) NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    transactionTypeId INT NOT NULL,
    transactionStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (transactionTypeId) REFERENCES TransactionType(id),
    FOREIGN KEY (transactionStatusId) REFERENCES TransactionStatus(id)
);

-- AccountBalanceWithdrawalRequest table
CREATE TABLE AccountBalanceWithdrawalRequest
(
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    accountId INT NOT NULL, -- Không có FK vì Account nằm trong UserService
    amount DECIMAL(18,2) NOT NULL,
    transferReceiptImageFileKey NVARCHAR(MAX) NULL,
    rejectReason NVARCHAR(MAX) NULL,
    isRejected BIT NULL, -- NULL: đang chờ, 1: rejected, 0: approved
    completedAt DATETIME NULL,
    createdAt DATETIME NOT NULL DEFAULT GETDATE(),
    updatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- PodcastSubscriptionTransaction table
CREATE TABLE PodcastSubscriptionTransaction (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    podcastSubscriptionRegistrationId UNIQUEIDENTIFIER NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    profit DECIMAL(18,2) NULL,
    transactionTypeId INT NOT NULL,
    transactionStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (transactionTypeId) REFERENCES TransactionType(id),
    FOREIGN KEY (transactionStatusId) REFERENCES TransactionStatus(id)
);

-- MemberSubscriptionTransaction table
CREATE TABLE MemberSubscriptionTransaction (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    memberSubscriptionRegistrationId UNIQUEIDENTIFIER NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    transactionTypeId INT NOT NULL,
    transactionStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (transactionTypeId) REFERENCES TransactionType(id),
    FOREIGN KEY (transactionStatusId) REFERENCES TransactionStatus(id)
);

-- BookingTransaction table
CREATE TABLE BookingTransaction (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    bookingId INT NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    profit DECIMAL(18,2) NULL,
    transactionTypeId INT NOT NULL,
    transactionStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (transactionTypeId) REFERENCES TransactionType(id),
    FOREIGN KEY (transactionStatusId) REFERENCES TransactionStatus(id)
);

-- BookingStorageTransaction table
CREATE TABLE BookingStorageTransaction (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    accountId INT NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    transactionTypeId INT NOT NULL,
    transactionStatusId INT NOT NULL,
    storageSize FLOAT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (transactionTypeId) REFERENCES TransactionType(id),
    FOREIGN KEY (transactionStatusId) REFERENCES TransactionStatus(id)
);

-- =====================================================
-- SAGA ORCHESTRATOR SERVICE DATABASE
-- =====================================================

-- SagaInstance table
CREATE TABLE SagaInstance (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    flowName NVARCHAR(250) NOT NULL,
    currentStepName NVARCHAR(250) NULL,
    initialData NVARCHAR(MAX) NULL,
    resultData NVARCHAR(MAX) NULL,
    flowStatus NVARCHAR(50) NOT NULL,
    errorStepName NVARCHAR(250) NULL,
    errorMessage NVARCHAR(MAX) NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    completedAt DATETIME NULL
);

-- SagaStepExecution table
CREATE TABLE SagaStepExecution (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    sagaInstanceId UNIQUEIDENTIFIER NOT NULL,
    stepName NVARCHAR(250) NOT NULL,
    topicName NVARCHAR(250) NOT NULL,
    stepStatus NVARCHAR(50) NOT NULL,
    requestData NVARCHAR(MAX) NULL,
    responseData NVARCHAR(MAX) NULL,
    errorMessage NVARCHAR(MAX) NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (sagaInstanceId) REFERENCES SagaInstance(id)
);