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


-- Bảng PodcastSubscriptionTransaction
ALTER TABLE PodcastSubscriptionTransaction
DROP COLUMN podcastSubscriptionRegistrationId;

ALTER TABLE PodcastSubscriptionTransaction
ADD podcastSubscriptionRegistrationId UNIQUEIDENTIFIER NOT NULL;

-- Bảng MemberSubscriptionTransaction
ALTER TABLE MemberSubscriptionTransaction
DROP COLUMN memberSubscriptionRegistrationId;

ALTER TABLE MemberSubscriptionTransaction
ADD memberSubscriptionRegistrationId UNIQUEIDENTIFIER NOT NULL;

