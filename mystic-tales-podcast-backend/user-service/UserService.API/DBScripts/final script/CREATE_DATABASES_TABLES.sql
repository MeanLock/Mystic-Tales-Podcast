-- =====================================================
-- USER SERVICE DATABASE [Port: 8046]
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'UserServiceDB')
BEGIN
    CREATE DATABASE UserServiceDB;
END
GO

USE UserServiceDB;
GO

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

-- Trigger for Account table
CREATE TRIGGER TR_Account_UpdatedAt
ON Account
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Account
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM Account a
    INNER JOIN inserted i ON a.id = i.id;
END;
GO


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
    isBuddy BIT NOT NULL DEFAULT 0,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (accountId) REFERENCES Account(id)
);

-- Trigger for PodcasterProfile table
CREATE TRIGGER TR_PodcasterProfile_UpdatedAt
ON PodcasterProfile
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcasterProfile
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcasterProfile p
    INNER JOIN inserted i ON p.accountId = i.accountId;
END;
GO

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

-- Trigger for PodcastBuddyReview table
CREATE TRIGGER TR_PodcastBuddyReview_UpdatedAt
ON PodcastBuddyReview
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastBuddyReview
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastBuddyReview p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO


-- =====================================================
-- SYSTEM CONFIGURATION SERVICE DATABASE [Port: 8051]
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SystemConfigurationServiceDB')
BEGIN
    CREATE DATABASE SystemConfigurationServiceDB;
END
GO

USE SystemConfigurationServiceDB;
GO

-- SystemConfigProfile table
CREATE TABLE SystemConfigProfile (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(MAX) NOT NULL,
    isActive BIT NOT NULL DEFAULT 0,
    deletedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME))
);

-- Trigger for SystemConfigProfile table
CREATE TRIGGER TR_SystemConfigProfile_UpdatedAt
ON SystemConfigProfile
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE SystemConfigProfile
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM SystemConfigProfile s
    INNER JOIN inserted i ON s.id = i.id;
END;
GO

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

-- Trigger for PodcastSubscriptionConfig table
CREATE TRIGGER TR_PodcastSubscriptionConfig_UpdatedAt
ON PodcastSubscriptionConfig
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastSubscriptionConfig
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastSubscriptionConfig p
    INNER JOIN inserted i ON p.configProfileId = i.configProfileId 
        AND p.subscriptionCycleTypeId = i.subscriptionCycleTypeId;
END;
GO

-- PodcastSuggestionConfig table
CREATE TABLE PodcastSuggestionConfig (
    configProfileId INT PRIMARY KEY,
    minShortRangeUserBehaviorLookbackDayCount INT NOT NULL,
    minMediumRangeUserBehaviorLookbackDayCount INT NOT NULL,
    minLongRangeUserBehaviorLookbackDayCount INT NOT NULL,
    minShortRangeContentBehaviorLookbackDayCount INT NOT NULL,
    minMediumRangeContentBehaviorLookbackDayCount INT NOT NULL,
    minLongRangeContentBehaviorLookbackDayCount INT NOT NULL,
    minExtraLongRangeContentBehaviorLookbackDayCount INT NOT NULL,
    minChannelQuery INT NOT NULL,
    minShowQuery INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (configProfileId) REFERENCES SystemConfigProfile(id)
);

-- Trigger for PodcastSuggestionConfig table
CREATE TRIGGER TR_PodcastSuggestionConfig_UpdatedAt
ON PodcastSuggestionConfig
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastSuggestionConfig
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastSuggestionConfig p
    INNER JOIN inserted i ON p.configProfileId = i.configProfileId;
END;
GO

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

-- Trigger for BookingConfig table
CREATE TRIGGER TR_BookingConfig_UpdatedAt
ON BookingConfig
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE BookingConfig
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM BookingConfig b
    INNER JOIN inserted i ON b.configProfileId = i.configProfileId;
END;
GO

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

-- Trigger for AccountConfig table
CREATE TRIGGER TR_AccountConfig_UpdatedAt
ON AccountConfig
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE AccountConfig
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM AccountConfig a
    INNER JOIN inserted i ON a.configProfileId = i.configProfileId;
END;
GO

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

-- Trigger for AccountViolationLevelConfig table
CREATE TRIGGER TR_AccountViolationLevelConfig_UpdatedAt
ON AccountViolationLevelConfig
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE AccountViolationLevelConfig
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM AccountViolationLevelConfig a
    INNER JOIN inserted i ON a.configProfileId = i.configProfileId 
        AND a.violationLevel = i.violationLevel;
END;
GO

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

-- Trigger for ReviewSessionConfig table
CREATE TRIGGER TR_ReviewSessionConfig_UpdatedAt
ON ReviewSessionConfig
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ReviewSessionConfig
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM ReviewSessionConfig r
    INNER JOIN inserted i ON r.configProfileId = i.configProfileId;
END;
GO

-- PodcastRestrictedTerm table
CREATE TABLE PodcastRestrictedTerm (
    id INT IDENTITY(1,1) PRIMARY KEY,
    term NVARCHAR(100) NOT NULL
);

-- =====================================================
-- BOOKING MANAGEMENT SERVICE DATABASE [Port: 8056]
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BookingManagementServiceDB')
BEGIN
    CREATE DATABASE BookingManagementServiceDB;
END
GO

USE BookingManagementServiceDB;
GO

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
    deadlineDays INT NULL,
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

-- Trigger for Booking table
CREATE TRIGGER TR_Booking_UpdatedAt
ON Booking
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Booking
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM Booking b
    INNER JOIN inserted i ON b.id = i.id;
END;
GO

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
    deadlineDays INT NULL,
    deadline DATE NULL,
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

CREATE TABLE BookingPodcastTrackListenSession (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    accountId INT NOT NULL,
    bookingPodcastTrackId UNIQUEIDENTIFIER NOT NULL,
    lastListenDurationSeconds INT NOT NULL DEFAULT 0,
    isCompleted BIT NOT NULL DEFAULT 0,
    expiredAt DATETIME NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (bookingPodcastTrackId) REFERENCES BookingPodcastTrack(id)
);

-- BookingProducingRequestPodcastTrackToEdit table
CREATE TABLE BookingProducingRequestPodcastTrackToEdit (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    bookingProducingRequestId UNIQUEIDENTIFIER NOT NULL,
    bookingPodcastTrackId UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY (bookingProducingRequestId) REFERENCES BookingProducingRequest(id),
    FOREIGN KEY (bookingPodcastTrackId) REFERENCES BookingPodcastTrack(id)
);



-- =====================================================
-- PODCAST SERVICE DATABASE [Port: 8061]
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PodcastServiceDB')
BEGIN
    CREATE DATABASE PodcastServiceDB;
END
GO

USE PodcastServiceDB;
GO

-- PodcastCategory table
CREATE TABLE PodcastCategory (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL,
    mainImageFileKey NVARCHAR(MAX) NULL
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

-- Trigger for PodcastChannel table
CREATE TRIGGER TR_PodcastChannel_UpdatedAt
ON PodcastChannel
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastChannel
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastChannel p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

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

-- Trigger for PodcastShow table
CREATE TRIGGER TR_PodcastShow_UpdatedAt
ON PodcastShow
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastShow
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastShow p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

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

-- Trigger for PodcastEpisode table
CREATE TRIGGER TR_PodcastEpisode_UpdatedAt
ON PodcastEpisode
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastEpisode
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastEpisode p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

-- PodcastEpisodeListenSession table
CREATE TABLE PodcastEpisodeListenSession (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    accountId INT NOT NULL,
    podcastEpisodeId UNIQUEIDENTIFIER NOT NULL,
    lastListenDurationSeconds INT NOT NULL DEFAULT 0,
    isCompleted BIT NOT NULL DEFAULT 0,
    isContentRemoved BIT NOT NULL DEFAULT 0,
    podcastCategoryId INT NULL,
    podcastSubCategoryId INT NULL,
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

-- Trigger for PodcastEpisodePublishReviewSession table
CREATE TRIGGER TR_PodcastEpisodePublishReviewSession_UpdatedAt
ON PodcastEpisodePublishReviewSession
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastEpisodePublishReviewSession
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastEpisodePublishReviewSession p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

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

-- Trigger for PodcastShowReview table
CREATE TRIGGER TR_PodcastShowReview_UpdatedAt
ON PodcastShowReview
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastShowReview
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastShowReview p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

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

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SubscriptionServiceDB')
BEGIN
    CREATE DATABASE SubscriptionServiceDB;
END
GO

USE SubscriptionServiceDB;
GO

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

-- Trigger for PodcastSubscription table
CREATE TRIGGER TR_PodcastSubscription_UpdatedAt
ON PodcastSubscription
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastSubscription
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastSubscription p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

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

-- Trigger for PodcastSubscriptionCycleTypePrice table
CREATE TRIGGER TR_PodcastSubscriptionCycleTypePrice_UpdatedAt
ON PodcastSubscriptionCycleTypePrice
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastSubscriptionCycleTypePrice
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastSubscriptionCycleTypePrice p
    INNER JOIN inserted i ON p.podcastSubscriptionId = i.podcastSubscriptionId 
        AND p.subscriptionCycleTypeId = i.subscriptionCycleTypeId 
        AND p.version = i.version;
END;
GO

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

-- Trigger for PodcastSubscriptionBenefitMapping table
CREATE TRIGGER TR_PodcastSubscriptionBenefitMapping_UpdatedAt
ON PodcastSubscriptionBenefitMapping
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastSubscriptionBenefitMapping
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastSubscriptionBenefitMapping p
    INNER JOIN inserted i ON p.podcastSubscriptionId = i.podcastSubscriptionId 
        AND p.podcastSubscriptionBenefitId = i.podcastSubscriptionBenefitId 
        AND p.version = i.version;
END;
GO

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

-- Trigger for PodcastSubscriptionRegistration table
CREATE TRIGGER TR_PodcastSubscriptionRegistration_UpdatedAt
ON PodcastSubscriptionRegistration
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastSubscriptionRegistration
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastSubscriptionRegistration p
    INNER JOIN inserted i ON p.accountId = i.accountId;
END;
GO


-- =====================================================
-- MODERATION SERVICE DATABASE [Port: 8071]
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ModerationServiceDB')
BEGIN
    CREATE DATABASE ModerationServiceDB;
END
GO

USE ModerationServiceDB;
GO

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

-- Trigger for PodcastBuddyReportReviewSession table
CREATE TRIGGER TR_PodcastBuddyReportReviewSession_UpdatedAt
ON PodcastBuddyReportReviewSession
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastBuddyReportReviewSession
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastBuddyReportReviewSession p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

-- PodcastShowReportReviewSession table
CREATE TABLE PodcastShowReportReviewSession (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    podcastShowId UNIQUEIDENTIFIER NOT NULL,
    assignedStaff INT NOT NULL,
    isResolved BIT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME))
);

-- Trigger for PodcastShowReportReviewSession table
CREATE TRIGGER TR_PodcastShowReportReviewSession_UpdatedAt
ON PodcastShowReportReviewSession
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastShowReportReviewSession
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastShowReportReviewSession p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

-- PodcastEpisodeReportReviewSession table
CREATE TABLE PodcastEpisodeReportReviewSession (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    podcastEpisodeId UNIQUEIDENTIFIER NOT NULL,
    assignedStaff INT NOT NULL,
    isResolved BIT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME))
);

-- Trigger for PodcastEpisodeReportReviewSession table
CREATE TRIGGER TR_PodcastEpisodeReportReviewSession_UpdatedAt
ON PodcastEpisodeReportReviewSession
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastEpisodeReportReviewSession
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastEpisodeReportReviewSession p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

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

-- Trigger for DMCAAccusation table
CREATE TRIGGER TR_DMCAAccusation_UpdatedAt
ON DMCAAccusation
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE DMCAAccusation
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM DMCAAccusation d
    INNER JOIN inserted i ON d.id = i.id;
END;
GO

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

-- Trigger for DMCAAccusationConclusionReport table
CREATE TRIGGER TR_DMCAAccusationConclusionReport_UpdatedAt
ON DMCAAccusationConclusionReport
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE DMCAAccusationConclusionReport
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM DMCAAccusationConclusionReport d
    INNER JOIN inserted i ON d.id = i.id;
END;
GO

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

-- Trigger for CounterNotice table
CREATE TRIGGER TR_CounterNotice_UpdatedAt
ON CounterNotice
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE CounterNotice
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM CounterNotice c
    INNER JOIN inserted i ON c.id = i.id;
END;
GO

-- DMCANotice table
CREATE TABLE DMCANotice (
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

-- Trigger for DMCANotice table
CREATE TRIGGER TR_DMCANotice_UpdatedAt
ON DMCANotice
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE DMCANotice
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM DMCANotice d
    INNER JOIN inserted i ON d.id = i.id;
END;
GO

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

-- Trigger for LawsuitProof table
CREATE TRIGGER TR_LawsuitProof_UpdatedAt
ON LawsuitProof
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE LawsuitProof
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM LawsuitProof l
    INNER JOIN inserted i ON l.id = i.id;
END;
GO

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

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TransactionServiceDB')
BEGIN
    CREATE DATABASE TransactionServiceDB;
END
GO

USE TransactionServiceDB;
GO

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

-- Trigger for AccountBalanceTransaction table
CREATE TRIGGER TR_AccountBalanceTransaction_UpdatedAt
ON AccountBalanceTransaction
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE AccountBalanceTransaction
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM AccountBalanceTransaction a
    INNER JOIN inserted i ON a.id = i.id;
END;
GO

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

-- Trigger for PodcastSubscriptionTransaction table
CREATE TRIGGER TR_PodcastSubscriptionTransaction_UpdatedAt
ON PodcastSubscriptionTransaction
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastSubscriptionTransaction
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastSubscriptionTransaction p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO


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

-- Trigger for BookingTransaction table
CREATE TRIGGER TR_BookingTransaction_UpdatedAt
ON BookingTransaction
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE BookingTransaction
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM BookingTransaction b
    INNER JOIN inserted i ON b.id = i.id;
END;
GO


-- =====================================================
-- SAGA ORCHESTRATOR SERVICE DATABASE
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SagaOrchestratorServiceDB')
BEGIN
    CREATE DATABASE SagaOrchestratorServiceDB;
END
GO

USE SagaOrchestratorServiceDB;
GO

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

-- Trigger for SagaInstance table
CREATE TRIGGER TR_SagaInstance_UpdatedAt
ON SagaInstance
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE SagaInstance
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM SagaInstance s
    INNER JOIN inserted i ON s.id = i.id;
END;
GO

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