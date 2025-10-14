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
    lastLawsuitCheckingAlertAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME))
);

-- CounterNotice table
CREATE TABLE CounterNotice (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    accountId INT NOT NULL,
    accountEmail NVARCHAR(MAX) NOT NULL,
    accountPhone NVARCHAR(MAX) NOT NULL,
    statementPerjury NVARCHAR(MAX) NOT NULL,
    signature NVARCHAR(MAX) NOT NULL,
    dmcaAccusationId INT NOT NULL,
    jurisdiction NVARCHAR(MAX) NOT NULL,
    isValid BIT NULL,
    invalidReason NVARCHAR(MAX) NULL,
    validatedBy INT NULL DEFAULT NULL,
    validatedAt DATETIME NULL DEFAULT NULL,
    filedDate DATE NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (dmcaAccusationId) REFERENCES DMCAAccusation(id)
);

-- DMCANotice table
CREATE TABLE DMCANotice (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    podcastShowId UNIQUEIDENTIFIER NULL,
    podcastEpisodeId UNIQUEIDENTIFIER NULL,
    accountId INT NOT NULL,
    accountEmail NVARCHAR(MAX) NOT NULL,
    accountPhone NVARCHAR(MAX) NOT NULL,
    goodFaithStatement NVARCHAR(MAX) NOT NULL,
    workClaimed NVARCHAR(MAX) NOT NULL,
    signature NVARCHAR(MAX) NOT NULL,
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
    accountId INT NOT NULL,
    goodFaithStatement NVARCHAR(MAX) NOT NULL,
    courtName NVARCHAR(MAX) NOT NULL,
    caseNumber NVARCHAR(100) NOT NULL,
    filingDate DATE NOT NULL,
    signature NVARCHAR(MAX) NOT NULL,
    dmcaAccusationId INT NOT NULL,
    isValid BIT NULL,
    inValidReason NVARCHAR(MAX) NULL,
    validatedBy INT NULL DEFAULT NULL,
    validatedAt DATETIME NULL DEFAULT NULL,
    judgmentDetails NVARCHAR(MAX) NULL,
    dateResolved DATE NULL DEFAULT NULL,
    outcome NVARCHAR(MAX) NULL DEFAULT NULL,
    rulingDocumentFileUrl NVARCHAR(MAX) NULL DEFAULT NULL,
    isDefendantWon BIT NULL DEFAULT NULL,
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