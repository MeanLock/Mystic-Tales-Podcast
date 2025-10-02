-- =====================================================
-- SUBSCRIPTION SERVICE TRIGGERS [Port: 8066]
-- =====================================================

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

-- Trigger for MemberSubscription table
CREATE TRIGGER TR_MemberSubscription_UpdatedAt
ON MemberSubscription
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE MemberSubscription
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM MemberSubscription m
    INNER JOIN inserted i ON m.id = i.id;
END;
GO

-- Trigger for MemberSubscriptionCycleTypePrice table
CREATE TRIGGER TR_MemberSubscriptionCycleTypePrice_UpdatedAt
ON MemberSubscriptionCycleTypePrice
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE MemberSubscriptionCycleTypePrice
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM MemberSubscriptionCycleTypePrice m
    INNER JOIN inserted i ON m.memberSubscriptionId = i.memberSubscriptionId 
        AND m.subscriptionCycleTypeId = i.subscriptionCycleTypeId 
        AND m.version = i.version;
END;
GO

-- Trigger for MemberSubscriptionBenefitMapping table
CREATE TRIGGER TR_MemberSubscriptionBenefitMapping_UpdatedAt
ON MemberSubscriptionBenefitMapping
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE MemberSubscriptionBenefitMapping
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM MemberSubscriptionBenefitMapping m
    INNER JOIN inserted i ON m.memberSubscriptionId = i.memberSubscriptionId 
        AND m.memberSubscriptionBenefitId = i.memberSubscriptionBenefitId 
        AND m.version = i.version;
END;
GO

-- Trigger for MemberSubscriptionRegistration table
CREATE TRIGGER TR_MemberSubscriptionRegistration_UpdatedAt
ON MemberSubscriptionRegistration
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE MemberSubscriptionRegistration
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM MemberSubscriptionRegistration m
    INNER JOIN inserted i ON m.accountId = i.accountId;
END;
GO