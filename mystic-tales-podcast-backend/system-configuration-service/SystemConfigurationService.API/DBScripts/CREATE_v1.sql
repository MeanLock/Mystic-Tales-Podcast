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