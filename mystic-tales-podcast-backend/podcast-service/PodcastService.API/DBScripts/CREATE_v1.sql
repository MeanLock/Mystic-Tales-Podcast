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

-- PodcastShowsSubscriptionType table
CREATE TABLE PodcastShowsSubscriptionType (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
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
    id UNIQUEIDENTIFIER PRIMARY KEY,
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
    id UNIQUEIDENTIFIER PRIMARY KEY,
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
    podcastShowsSubscriptionTypeId INT NOT NULL DEFAULT 1,
    podcastChannelId UNIQUEIDENTIFIER NULL,
    takenDownReason NVARCHAR(MAX) NULL,
    deletedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastCategoryId) REFERENCES PodcastCategory(id),
    FOREIGN KEY (podcastSubCategoryId) REFERENCES PodcastSubCategory(id),
    FOREIGN KEY (podcastShowsSubscriptionTypeId) REFERENCES PodcastShowsSubscriptionType(id),
    FOREIGN KEY (podcastChannelId) REFERENCES PodcastChannel(id)
);

-- PodcastEpisode table
CREATE TABLE PodcastEpisode (
    id UNIQUEIDENTIFIER PRIMARY KEY,
    title NVARCHAR(250) NOT NULL,
    description NVARCHAR(MAX) NOT NULL DEFAULT '',
    explicitContent BIT NOT NULL DEFAULT 0,
    releaseDate DATE NULL,
    isReleased BIT NULL,
    mainImageFileKey NVARCHAR(MAX) NULL,
    audioFileKey NVARCHAR(MAX) NOT NULL,
    audioFileSize FLOAT NOT NULL,
    audioLength INT NOT NULL,
    audioFingerPrint VARBINARY(MAX) NULL DEFAULT NULL,
    podcastEpisodeSubscriptionTypeId INT NOT NULL DEFAULT 1,
    podcastShowId UNIQUEIDENTIFIER NOT NULL,
    seasonNumber INT NOT NULL DEFAULT 0,
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

-- PodcastEpisodeLicense table
CREATE TABLE PodcastEpisodeLicense (
    id UNIQUEIDENTIFIER PRIMARY KEY,
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
    id UNIQUEIDENTIFIER PRIMARY KEY,
    podcastEpisodePublishReviewSessionId INT NOT NULL,
    podcastEpisodePublishReviewSessionStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastEpisodePublishReviewSessionId) REFERENCES PodcastEpisodePublishReviewSession(id),
    FOREIGN KEY (podcastEpisodePublishReviewSessionStatusId) REFERENCES PodcastEpisodePublishReviewSessionStatus(id)
);

-- PodcastChannelStatusTracking table
CREATE TABLE PodcastChannelStatusTracking (
    id UNIQUEIDENTIFIER PRIMARY KEY,
    podcastChannelId UNIQUEIDENTIFIER NOT NULL,
    podcastChannelStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastChannelId) REFERENCES PodcastChannel(id),
    FOREIGN KEY (podcastChannelStatusId) REFERENCES PodcastChannelStatus(id)
);

-- PodcastShowStatusTracking table
CREATE TABLE PodcastShowStatusTracking (
    id UNIQUEIDENTIFIER PRIMARY KEY,
    podcastShowId UNIQUEIDENTIFIER NOT NULL,
    podcastShowStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastShowId) REFERENCES PodcastShow(id),
    FOREIGN KEY (podcastShowStatusId) REFERENCES PodcastShowStatus(id)
);

-- PodcastEpisodeStatusTracking table
CREATE TABLE PodcastEpisodeStatusTracking (
    id UNIQUEIDENTIFIER PRIMARY KEY,
    podcastEpisodeId UNIQUEIDENTIFIER NOT NULL,
    podcastEpisodeStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (podcastEpisodeId) REFERENCES PodcastEpisode(id),
    FOREIGN KEY (podcastEpisodeStatusId) REFERENCES PodcastEpisodeStatus(id)
);

-- PodcastShowReview table
CREATE TABLE PodcastShowReview (
    id UNIQUEIDENTIFIER PRIMARY KEY,
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
    PRIMARY KEY (podcastChannelId, hashtagId),
    FOREIGN KEY (podcastChannelId) REFERENCES PodcastChannel(id),
    FOREIGN KEY (hashtagId) REFERENCES Hashtag(id)
);

-- PodcastShowHashtag table
CREATE TABLE PodcastShowHashtag (
    podcastShowId UNIQUEIDENTIFIER NOT NULL,
    hashtagId INT NOT NULL,
    PRIMARY KEY (podcastShowId, hashtagId),
    FOREIGN KEY (podcastShowId) REFERENCES PodcastShow(id),
    FOREIGN KEY (hashtagId) REFERENCES Hashtag(id)
);

-- PodcastEpisodeHashtag table
CREATE TABLE PodcastEpisodeHashtag (
    podcastEpisodeId UNIQUEIDENTIFIER NOT NULL,
    hashtagId INT NOT NULL,
    PRIMARY KEY (podcastEpisodeId, hashtagId),
    FOREIGN KEY (podcastEpisodeId) REFERENCES PodcastEpisode(id),
    FOREIGN KEY (hashtagId) REFERENCES Hashtag(id)
);