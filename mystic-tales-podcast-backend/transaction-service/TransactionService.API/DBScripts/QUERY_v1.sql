-- PodcastSubscriptionRegistration table
CREATE TABLE PodcastSubscriptionRegistration (
    accountId INT PRIMARY KEY,
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

-- PodcastSubscriptionRegistrationBenefit table
CREATE TABLE PodcastSubscriptionRegistrationBenefit (
    podcastSubscriptionRegistrationId INT NOT NULL,
    podcastSubscriptionBenefitId INT NOT NULL,
    PRIMARY KEY (podcastSubscriptionRegistrationId, podcastSubscriptionBenefitId),
    FOREIGN KEY (podcastSubscriptionRegistrationId) REFERENCES PodcastSubscriptionRegistration(accountId),
    FOREIGN KEY (podcastSubscriptionBenefitId) REFERENCES PodcastSubscriptionBenefit(id)
);


Drop table PodcastSubscriptionRegistrationBenefit
Drop table PodcastSubscriptionRegistration
Drop table MemberSubscriptionRegistration



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

