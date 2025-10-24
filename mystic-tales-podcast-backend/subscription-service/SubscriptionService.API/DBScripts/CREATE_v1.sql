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
    id INT PRIMARY KEY,
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