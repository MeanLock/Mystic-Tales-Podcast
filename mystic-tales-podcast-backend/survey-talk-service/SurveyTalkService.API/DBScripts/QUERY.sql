
use SurveyTalkDB_test_1

SELECT * from Account
SELECT * from PlatformFeedback
SELECT * from AccountOnlineTracking
SELECT * from FilterTag
SELECT * from TakerTagFilter 
SELECT * from SurveyTopicFavorite where accountid=6


Delete from SurveyTakenResultTagFilter where surveyTakenResultId = 14
Delete from SurveyResponse where surveyTakenResultId = 14
Delete from SurveyTakenResult where id = 206
Delete from Account where id = 30

INSERT INTO SurveyRewardTracking(surveyId, rewardPrice ,rewardXp) VALUES (6, 10000, 1331);

SELECT * from AccountProfile where accountid = 3
SELECT * from SurveyTakerSegment where surveyId = 8
SELECT * from Survey where surveyTypeId = 1 and id = 1
SELECT * from SurveyRewardTracking where surveyId = 10
SELECT * from SurveyStatusTracking where surveyId = 326

SELECT * from SurveyGeneralConfig 
SELECT * from SurveyQuestion
SELECT * from  SurveySecurityMode 

SELECT * from SurveyTagFilter where surveyId =6

SELECT * from SurveyStatusTracking where surveyId = 7
SELECT * from SurveyMarketVersionStatusTracking where surveyId = 12 and version =1 
SELECT * from SurveyMarket where surveyId = 12
SELECT * from SurveyQuestion where surveyId =7
SELECT * from SurveyOption order by id desc
SELECT * from SurveyRewardTracking where surveyId = 290
SELECT * from SurveyTakenResult where surveyId = 283
SELECT * from SurveyTakenResultTagFilter where surveyTakenResultId =147
SELECT * from SurveyResponse where SurveyTakenResultId = 1
SELECT * from AccountBalanceTransaction
SELECT * from SurveyCommunityTransaction

SELECT * from TransactionType
DBCC CHECKIDENT ('AccountBalanceTransaction', RESEED, 42);


SELECT * from AccountGeneralConfig
SELECT * from AccountLevelSettingConfig
SELECT * from SurveySecurityModeConfig

SELECT * from SurveyTimeRateConfig 

INSERT INTO SurveyTopicFavorite (AccountId, SurveyTopicId, favoriteScore) VALUES (3, 1, FLOOR(RAND()*5)+1);
INSERT INTO SurveyTopicFavorite (AccountId, SurveyTopicId, favoriteScore) VALUES (3, 2, FLOOR(RAND()*5)+1);
INSERT INTO SurveyTopicFavorite (AccountId, SurveyTopicId, favoriteScore) VALUES (3, 3, FLOOR(RAND()*5)+1);
INSERT INTO SurveyTopicFavorite (AccountId, SurveyTopicId, favoriteScore) VALUES (3, 4, FLOOR(RAND()*5)+1);
INSERT INTO SurveyTopicFavorite (AccountId, SurveyTopicId, favoriteScore) VALUES (3, 5, FLOOR(RAND()*5)+1);
INSERT INTO SurveyTopicFavorite (AccountId, SurveyTopicId, favoriteScore) VALUES (3, 6, FLOOR(RAND()*5)+1);
INSERT INTO SurveyTopicFavorite (AccountId, SurveyTopicId, favoriteScore) VALUES (3, 7, FLOOR(RAND()*5)+1);
INSERT INTO SurveyTopicFavorite (AccountId, SurveyTopicId, favoriteScore) VALUES (3, 8, FLOOR(RAND()*5)+1);
INSERT INTO SurveyTopicFavorite (AccountId, SurveyTopicId, favoriteScore) VALUES (3, 9, FLOOR(RAND()*5)+1);
INSERT INTO SurveyTopicFavorite (AccountId, SurveyTopicId, favoriteScore) VALUES (3, 10, FLOOR(RAND()*5)+1);
DBCC CHECKIDENT ('PaymentHistory', RESEED, 38)
DBCC CHECKIDENT ('PaymentHistory', RESEED, 37)

SELECT *
FROM SurveyTakenResult
WHERE takerId IN (1, 2, 3, 4, 9, 10);


UPDATE SurveyMarketVersionStatusTracking
SET createdAt = GETDATE()
WHERE surveyId = 12 and version = 1; 

INSERT INTO SurveyMarketVersionStatusTracking (surveyId, [version], surveyStatusId)
VALUES (12, 2, 2)

INSERT INTO SurveyStatusTracking (surveyId, surveyStatusId)
VALUES (7, 1); 

UPDATE Survey
SET SurveyTopicId = 1
WHERE id = 7;

Delete from AccountProfile
INSERT INTO AccountProfile (AccountId, CountryRegion, MaritalStatus, AverageIncome, EducationLevel, JobField)
VALUES
(1, N'Việt Nam', N'Độc thân', N'Từ 3 đến 7 triệu/tháng', N'Đại học', N'Công nghệ thông tin'),
(2, N'Việt Nam', N'Kết hôn', N'Trên 30 triệu/tháng', N'Sau Đại học', N'Y tế'),
(3, N'Việt Nam', N'Ly hôn', N'Từ 7 đến 15 triệu/tháng', N'Cao học (Cao đẳng, Học nghề)', N'Giáo dục'),
(4, N'Việt Nam', N'Độc thân', N'Dưới 3 triệu/tháng', N'Trung học phổ thông', N'Kinh doanh & Marketing'),
(5, N'Việt Nam', N'Kết hôn', N'Từ 15 đến 30 triệu/tháng', N'Đại học', N'Công nghệ thông tin'),
(6, N'Việt Nam', N'Ly hôn', N'Từ 7 đến 15 triệu/tháng', N'Trung học cơ sở', N'Giáo dục'),
(7, N'Việt Nam', N'Kết hôn', N'Từ 3 đến 7 triệu/tháng', N'Cao học (Cao đẳng, Học nghề)', N'Y tế'),
(8, N'Việt Nam', N'Độc thân', N'Từ 15 đến 30 triệu/tháng', N'Sau Đại học', N'Kinh doanh & Marketing'),
(9, N'Việt Nam', N'Kết hôn', N'Dưới 3 triệu/tháng', N'Tiểu học', N'Giáo dục'),
(10, N'Việt Nam', N'Ly hôn', N'Từ 3 đến 7 triệu/tháng', N'Trung học phổ thông', N'Y tế');


update AccountProfile
set MaritalStatus = N'Dưới 3 triệu/tháng'
where accountId = 6
update Account
set balance = 1000000000000
where id = 6

update SurveyTakerSegment
set tagFilterAccuracyRate = 0.5
where surveyId = 7


Delete from SurveyTakerSegment
INSERT INTO SurveyTakerSegment (SurveyId, CountryRegion, MaritalStatus, AverageIncome, EducationLevel, JobField)
VALUES
(6,  N'Việt Nam', N'Độc thân | Kết hôn', N'Từ 3 đến 7 triệu/tháng | Từ 15 đến 30 triệu/tháng', N'Đại học | Trung học cơ sở', N'Công nghệ thông tin | Giáo dục'),
(7,  N'Việt Nam | Miền Nam', N'Độc thân | Kết hôn | Ly hôn', N'Trên 30 triệu/tháng | Từ 7 đến 15 triệu/tháng | Dưới 3 triệu/tháng', N'Trung học cơ sở | Cao học (Cao đẳng, Học nghề)', N'Y tế | Công nghệ thông tin'),
(8,  N'Việt Nam | Miền Nam', N'Độc thân | Kết hôn | Ly hôn', N'Từ 15 đến 30 triệu/tháng | Dưới 3 triệu/tháng', N'Sau Đại học | Tiểu học | Trung học cơ sở', N'Kinh doanh & Marketing | Giáo dục | Công nghệ thông tin'),
(9,  N'Việt Nam | Miền Nam', N'Độc thân', N'Dưới 3 triệu/tháng', N'Tiểu học | Trung học cơ sở', N'Giáo dục | Công nghệ thông tin'),
(10, N'Việt Nam', N'Ly hôn | Độc thân', N'Từ 3 đến 7 triệu/tháng | Dưới 3 triệu/tháng', N'Trung học phổ thông', N'Y tế | Kinh doanh & Marketing'),
(13, N'Việt Nam', N'Kết hôn | Độc thân', N'Từ 3 đến 7 triệu/tháng | Từ 15 đến 30 triệu/tháng', N'Cao học (Cao đẳng, Học nghề) | Đại học', N'Y tế | Công nghệ thông tin'),
(14, N'Việt Nam', N'Ly hôn', N'Từ 7 đến 15 triệu/tháng', N'Cao học (Cao đẳng, Học nghề) | Trung học cơ sở', N'Giáo dục');


EXEC sp_rename 'AccountVerification', 'AccountNationalVerification';
ALTER TABLE Account
ADD googleId VARCHAR(250) NULL,
    verifyCode NVARCHAR(250) NULL;

	UPDATE Account SET isVerified = 1;

SELECT 
    name AS TriggerName,
    object_id AS TriggerObjectId,
    parent_class_desc,
    create_date,
    modify_date,
    is_disabled
FROM sys.triggers;


