-- Script to update default constraints for all columns using GETDATE() to use timezone 'N. Central Asia Standard Time'
-- Note: You may need to adjust constraint names if they are not default-named by SQL Server
use SurveyTalkDB_test_1
-- ACCOUNT TABLE
ALTER TABLE Account DROP CONSTRAINT IF EXISTS DF__Account__created__0662F0A3;
ALTER TABLE Account DROP CONSTRAINT IF EXISTS DF__Account__updated__075714DC;
ALTER TABLE Account DROP CONSTRAINT IF EXISTS DF__Account__deactiv__208CD6FA;
ALTER TABLE Account ADD CONSTRAINT DF_Account_createdAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR createdAt;
ALTER TABLE Account ADD CONSTRAINT DF_Account_updatedAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR updatedAt;
SELECT name AS ConstraintName, OBJECT_NAME(parent_object_id) AS TableName, COL_NAME(parent_object_id, parent_column_id) AS ColumnName
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID(N'Account');

-- ACCOUNTONLINETRACKING TABLE
ALTER TABLE AccountOnlineTracking DROP CONSTRAINT IF EXISTS DF__AccountOn__onlin__236943A5;
ALTER TABLE AccountOnlineTracking ADD CONSTRAINT DF_AccountOnlineTracking_onlineDate DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATE)) FOR onlineDate;
SELECT name AS ConstraintName, OBJECT_NAME(parent_object_id) AS TableName, COL_NAME(parent_object_id, parent_column_id) AS ColumnName
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID(N'AccountOnlineTracking');

-- SYSTEMCONFIGPROFILE TABLE
ALTER TABLE SystemConfigProfile DROP CONSTRAINT IF EXISTS DF__SystemCon__creat__15A53433;
ALTER TABLE SystemConfigProfile DROP CONSTRAINT IF EXISTS DF__SystemCon__updat__1699586C;
ALTER TABLE SystemConfigProfile ADD CONSTRAINT DF_SystemConfigProfile_createdAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR createdAt;
ALTER TABLE SystemConfigProfile ADD CONSTRAINT DF_SystemConfigProfile_updatedAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR updatedAt;
SELECT name AS ConstraintName, OBJECT_NAME(parent_object_id) AS TableName, COL_NAME(parent_object_id, parent_column_id) AS ColumnName
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID(N'SystemConfigProfile');

-- SURVEYQUESTIONTYPE TABLE
ALTER TABLE SurveyQuestionType DROP CONSTRAINT IF EXISTS DF__SurveyQue__deact__361203C5;
ALTER TABLE SurveyQuestionType ADD CONSTRAINT DF_SurveyQuestionType_deactivatedAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR deactivatedAt;
SELECT name AS ConstraintName, OBJECT_NAME(parent_object_id) AS TableName, COL_NAME(parent_object_id, parent_column_id) AS ColumnName
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID(N'SurveyQuestionType');

-- SURVEY TABLE
ALTER TABLE Survey DROP CONSTRAINT IF EXISTS DF__Survey__createdA__4183B671;
ALTER TABLE Survey DROP CONSTRAINT IF EXISTS DF__Survey__updatedA__1D114BD1;
ALTER TABLE Survey ADD CONSTRAINT DF_Survey_createdAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR createdAt;
ALTER TABLE Survey ADD CONSTRAINT DF_Survey_updatedAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR updatedAt;
SELECT name AS ConstraintName, OBJECT_NAME(parent_object_id) AS TableName, COL_NAME(parent_object_id, parent_column_id) AS ColumnName
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID(N'Survey');

-- SURVEYSTATUSTRACKING TABLE
ALTER TABLE SurveyStatusTracking DROP CONSTRAINT IF EXISTS DF__SurveySta__creat__46486B8E;
ALTER TABLE SurveyStatusTracking ADD CONSTRAINT DF_SurveyStatusTracking_createdAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR createdAt;
SELECT name AS ConstraintName, OBJECT_NAME(parent_object_id) AS TableName, COL_NAME(parent_object_id, parent_column_id) AS ColumnName
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID(N'SurveyStatusTracking');

-- SURVEYMARKET TABLE
ALTER TABLE SurveyMarket DROP CONSTRAINT IF EXISTS DF__SurveyMar__creat__595B4002;
ALTER TABLE SurveyMarket DROP CONSTRAINT IF EXISTS DF__SurveyMar__updat__5A4F643B;
ALTER TABLE SurveyMarket ADD CONSTRAINT DF_SurveyMarket_createdAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR createdAt;
ALTER TABLE SurveyMarket ADD CONSTRAINT DF_SurveyMarket_updatedAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR updatedAt;
SELECT name AS ConstraintName, OBJECT_NAME(parent_object_id) AS TableName, COL_NAME(parent_object_id, parent_column_id) AS ColumnName
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID(N'SurveyMarket');

-- SURVEYMARKETVERSIONSTATUSTRACKING TABLE
ALTER TABLE SurveyMarketVersionStatusTracking DROP CONSTRAINT IF EXISTS DF__SurveyMar__creat__54968AE5;
ALTER TABLE SurveyMarketVersionStatusTracking ADD CONSTRAINT DF_SurveyMarketVersionStatusTracking_createdAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR createdAt;
SELECT name AS ConstraintName, OBJECT_NAME(parent_object_id) AS TableName, COL_NAME(parent_object_id, parent_column_id) AS ColumnName
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID(N'SurveyMarketVersionStatusTracking');

-- SURVEYTAKENRESULT TABLE
ALTER TABLE SurveyTakenResult DROP CONSTRAINT IF EXISTS DF__SurveyTak__compl__63D8CE75;
ALTER TABLE SurveyTakenResult ADD CONSTRAINT DF_SurveyTakenResult_completedAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR completedAt;
SELECT name AS ConstraintName, OBJECT_NAME(parent_object_id) AS TableName, COL_NAME(parent_object_id, parent_column_id) AS ColumnName
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID(N'SurveyTakenResult');

-- DATAPURCHASE TABLE
ALTER TABLE DataPurchase DROP CONSTRAINT IF EXISTS DF__DataPurch__purch__6F4A8121;
ALTER TABLE DataPurchase ADD CONSTRAINT DF_DataPurchase_purchasedAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR purchasedAt;
SELECT name AS ConstraintName, OBJECT_NAME(parent_object_id) AS TableName, COL_NAME(parent_object_id, parent_column_id) AS ColumnName
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID(N'DataPurchase');

-- SURVEYREWARDTRACKING TABLE
ALTER TABLE SurveyRewardTracking DROP CONSTRAINT IF EXISTS DF__SurveyRew__creat__76EBA2E9;
ALTER TABLE SurveyRewardTracking ADD CONSTRAINT DF_SurveyRewardTracking_createdAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR createdAt;
SELECT name AS ConstraintName, OBJECT_NAME(parent_object_id) AS TableName, COL_NAME(parent_object_id, parent_column_id) AS ColumnName
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID(N'SurveyRewardTracking');

-- SURVEYFEEDBACK TABLE
ALTER TABLE SurveyFeedback DROP CONSTRAINT IF EXISTS DF__SurveyFee__creat__119F9925;
ALTER TABLE SurveyFeedback DROP CONSTRAINT IF EXISTS DF__SurveyFee__updat__1293BD5E;
ALTER TABLE SurveyFeedback ADD CONSTRAINT DF_SurveyFeedback_createdAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR createdAt;
ALTER TABLE SurveyFeedback ADD CONSTRAINT DF_SurveyFeedback_updatedAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR updatedAt;
SELECT name AS ConstraintName, OBJECT_NAME(parent_object_id) AS TableName, COL_NAME(parent_object_id, parent_column_id) AS ColumnName
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID(N'SurveyFeedback');

-- PAYMENTHISTORY TABLE
ALTER TABLE PaymentHistory DROP CONSTRAINT IF EXISTS DF__PaymentHi__creat__1C1D2798;
ALTER TABLE PaymentHistory ADD CONSTRAINT DF_PaymentHistory_createdAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR createdAt;
SELECT name AS ConstraintName, OBJECT_NAME(parent_object_id) AS TableName, COL_NAME(parent_object_id, parent_column_id) AS ColumnName
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID(N'PaymentHistory');

-- PASSWORDRESETTOKEN TABLE
ALTER TABLE PasswordResetToken DROP CONSTRAINT IF EXISTS DF__PasswordR__creat__3C89F72A;
ALTER TABLE PasswordResetToken ADD CONSTRAINT DF_PasswordResetToken_createdAt DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)) FOR createdAt;
SELECT name AS ConstraintName, OBJECT_NAME(parent_object_id) AS TableName, COL_NAME(parent_object_id, parent_column_id) AS ColumnName
FROM sys.default_constraints
WHERE parent_object_id = OBJECT_ID(N'PasswordResetToken');
