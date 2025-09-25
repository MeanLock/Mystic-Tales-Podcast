-- Xoá các trigger cũ nếu tồn tại
DROP TRIGGER IF EXISTS trg_Account_Update;
DROP TRIGGER IF EXISTS trg_Survey_Update;
DROP TRIGGER IF EXISTS trg_SurveyMarket_Update;
DROP TRIGGER IF EXISTS trg_SurveyFeedback_Update;
DROP TRIGGER IF EXISTS trg_SystemConfigProfile_Update;
GO

-- Trigger cho Account
CREATE TRIGGER trg_Account_Update
ON Account
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Account
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM Inserted
    WHERE Account.id = Inserted.id;
END
GO

-- Trigger cho Survey
CREATE TRIGGER trg_Survey_Update
ON Survey
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Survey
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM Inserted
    WHERE Survey.id = Inserted.id;
END
GO

-- Trigger cho PlatformFeedback
CREATE TRIGGER trg_PlatformFeedback_Update
ON PlatformFeedback
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PlatformFeedback
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM Inserted
    WHERE PlatformFeedback.accountId = Inserted.accountId;
END

-- Trigger cho SurveyMarket
CREATE TRIGGER trg_SurveyMarket_Update
ON SurveyMarket
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE SurveyMarket
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM Inserted
    WHERE SurveyMarket.surveyId = Inserted.surveyId AND SurveyMarket.version = Inserted.version;
END
GO

-- Trigger cho SurveyFeedback
CREATE TRIGGER trg_SurveyFeedback_Update
ON SurveyFeedback
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE SurveyFeedback
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM Inserted
    WHERE SurveyFeedback.surveyId = Inserted.surveyId AND SurveyFeedback.takerId = Inserted.takerId;
END
GO

-- Trigger cho SystemConfigProfile
CREATE TRIGGER trg_SystemConfigProfile_Update
ON SystemConfigProfile
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE SystemConfigProfile
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM Inserted
    WHERE SystemConfigProfile.id = Inserted.id;
END
GO