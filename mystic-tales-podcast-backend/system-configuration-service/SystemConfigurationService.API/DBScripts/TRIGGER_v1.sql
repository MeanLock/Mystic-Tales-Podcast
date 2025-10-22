-- =====================================================
-- SYSTEM CONFIGURATION SERVICE TRIGGERS [Port: 8051]
-- =====================================================

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