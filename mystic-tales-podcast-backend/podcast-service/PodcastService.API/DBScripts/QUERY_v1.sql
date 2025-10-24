select * from PodcastChannel
select * from PodcastChannelStatusTracking
select * from PodcastShow
select * from PodcastShowReview
select * from PodcastShowStatusTracking where podcastShowId = N'89709A6A-11A3-4574-B94A-3ED61A0BE627'
select * from Hashtag
select * from PodcastChannelHashtag
select * from PodcastShow
select * from PodcastCategory
select * from PodcastSubCategory
select * from PodcastShowSubscriptionType
select * from PodcastEpisode
select * from PodcastEpisodeStatusTracking order by createdAt
select * from PodcastEpisodeLicense

select * from PodcastEpisodePublishReviewSession
select * from PodcastEpisodePublishReviewSessionStatusTracking
select * from PodcastEpisodePublishDuplicateDetection
select * from PodcastEpisodeIllegalContentTypeMarking

delete from PodcastEpisodeIllegalContentTypeMarking
delete from PodcastEpisodePublishDuplicateDetection
delete from PodcastEpisodePublishReviewSession


delete from PodcastChannelStatusTracking
delete from PodcastChannel
delete from PodcastShowReview
INSERT INTO PodcastEpisodeStatusTracking (podcastEpisodeId, podcastEpisodeStatusId)
VALUES (N'FDE924E8-B1C1-49BB-BE2E-1F359A7866A7' , 4); -- pending editrequest
INSERT INTO PodcastShowStatusTracking(podcastShowId, podcastShowStatusId)
VALUES (N'b4988aad-58cb-4c17-937e-3ad5e65336ce' , 3); 
INSERT INTO PodcastEpisodePublishReviewSessionStatusTracking(podcastEpisodePublishReviewSessionId, podcastEpisodePublishReviewSessionStatusId)
VALUES (7 , 3); 

ALTER TABLE PodcastEpisode
    ALTER COLUMN audioFileKey NVARCHAR(MAX) NULL ;

ALTER TABLE PodcastEpisode
    ALTER COLUMN audioFileSize FLOAT NULL ;

ALTER TABLE PodcastEpisode
    ALTER COLUMN audioLength INT NULL ;



-- 1. Thêm DEFAULT NULL cho audioFileKey
ALTER TABLE PodcastEpisode
    ADD CONSTRAINT DF_PodcastEpisode_audioFileKey DEFAULT NULL FOR audioFileKey;
PRINT '✓ DEFAULT NULL added for audioFileKey';

-- 2. Thêm DEFAULT NULL cho audioFileSize
ALTER TABLE PodcastEpisode
    ADD CONSTRAINT DF_PodcastEpisode_audioFileSize DEFAULT NULL FOR audioFileSize;
PRINT '✓ DEFAULT NULL added for audioFileSize';

-- 3. Thêm DEFAULT NULL cho audioLength
ALTER TABLE PodcastEpisode
    ADD CONSTRAINT DF_PodcastEpisode_audioLength DEFAULT NULL FOR audioLength;
PRINT '✓ DEFAULT NULL added for audioLength';

ALTER TABLE PodcastEpisode
    ADD audioTranscript NVARCHAR(MAX) NULL DEFAULT NULL;



ALTER TABLE PodcastEpisode
ADD episodeOrder INT NOT NULL DEFAULT 1;


INSERT INTO PodcastShowHashtag (podcastShowId, hashtagId, createdAt)
VALUES 
    ('89709A6A-11A3-4574-B94A-3ED61A0BE627', 1, CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    ('89709A6A-11A3-4574-B94A-3ED61A0BE627', 2, CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    ('89709A6A-11A3-4574-B94A-3ED61A0BE627', 3, CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME));

-- Cultural Deep Dive (Society & Culture Show - ID: 172EB07F-2121-4FF7-8B5C-91EEEC0DEE86)
INSERT INTO PodcastShowHashtag (podcastShowId, hashtagId, createdAt)
VALUES 
    ('172EB07F-2121-4FF7-8B5C-91EEEC0DEE86', 4, CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    ('172EB07F-2121-4FF7-8B5C-91EEEC0DEE86', 5, CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    ('172EB07F-2121-4FF7-8B5C-91EEEC0DEE86', 6, CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME));

-- Mysteries Unveiled (True Crime Show - ID: 7C723A74-FD5E-4747-B7A5-FAD6FA3B6F31)
INSERT INTO PodcastShowHashtag (podcastShowId, hashtagId, createdAt)
VALUES 
    ('7C723A74-FD5E-4747-B7A5-FAD6FA3B6F31', 7, CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    ('7C723A74-FD5E-4747-B7A5-FAD6FA3B6F31', 8, CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    ('7C723A74-FD5E-4747-B7A5-FAD6FA3B6F31', 9, CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME));



EXEC sp_rename 'PodcastEpisode.title', 'name', 'COLUMN'

-- Thêm cột createdAt vào bảng join
ALTER TABLE PodcastChannelHashtag
ADD createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME));

ALTER TABLE PodcastShowHashtag
ADD createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME));

ALTER TABLE PodcastEpisodeHashtag
ADD createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME));

INSERT INTO Hashtag (name) VALUES 
(N'TrueCrime'),
(N'Horror'),
(N'Mystery'),

INSERT INTO PodcastChannelStatusTracking (podcastChannelId, podcastChannelStatusId) VALUES 
(N'7C50EC4D-DD32-47C8-955A-084C2CC640BF', 1),
(N'90921933-FC03-46A4-BFE2-0ACCEB5520F2', 1),
(N'515054CA-D7BB-4BFD-A07E-392671A2FEBD', 1),
(N'D603081B-8E35-4EEA-A37C-ADD84D7A00E6', 1)
(N'7C50EC4D-DD32-47C8-955A-084C2CC640BF', 2)
(N'90921933-FC03-46A4-BFE2-0ACCEB5520F2', 2)

INSERT INTO PodcastChannelStatusTracking (podcastChannelId, podcastChannelStatusId) VALUES 
(N'7C50EC4D-DD32-47C8-955A-084C2CC640BF', 2)




-- Khai báo biến
DECLARE @ShowId UNIQUEIDENTIFIER = '172eb07f-2121-4ff7-8b5c-91eeec0dee86';
DECLARE @Episode1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Episode2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Episode3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @CurrentDateTime DATETIME = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME);

-- ========================================
-- EPISODE 1
-- ========================================
INSERT INTO PodcastEpisode (
    id,
    name,
    description,
    explicitContent,
    releaseDate,
    isReleased,
    mainImageFileKey,
    audioFileKey,
    audioFileSize,
    audioLength,
    audioFingerPrint,
    audioTranscript,
    podcastEpisodeSubscriptionTypeId,
    podcastShowId,
    seasonNumber,
    episodeOrder,
    totalSave,
    listenCount,
    isAudioPublishable,
    takenDownReason,
    deletedAt,
    createdAt,
    updatedAt
)
VALUES (
    @Episode1Id,
    N'Episode 1 - Giới thiệu',
    N'Episode đầu tiên của chương trình',
    0,
    CAST(DATEADD(DAY, -7, @CurrentDateTime) AS DATE), -- Phát hành 7 ngày trước
    1, -- isReleased = true
    N'episode1-cover.jpg',
    N'episode1-audio.mp3',
    25.5, -- 25.5 MB
    1800, -- 30 phút (1800 giây)
    NULL,
    N'Nội dung transcript của episode 1...',
    1, -- Free
    @ShowId,
    1, -- Season 1
    1, -- Episode order 1
    0,
    0,
    1, -- isAudioPublishable = true (đã được duyệt)
    NULL,
    NULL,
    DATEADD(DAY, -10, @CurrentDateTime),
    @CurrentDateTime
);

-- Status tracking cho Episode 1
INSERT INTO PodcastEpisodeStatusTracking (id, podcastEpisodeId, podcastEpisodeStatusId, createdAt)
VALUES (NEWID(), @Episode1Id, 1, DATEADD(DAY, -10, @CurrentDateTime)); -- Draft

INSERT INTO PodcastEpisodeStatusTracking (id, podcastEpisodeId, podcastEpisodeStatusId, createdAt)
VALUES (NEWID(), @Episode1Id, 4, DATEADD(DAY, -8, @CurrentDateTime)); -- Ready To Release

INSERT INTO PodcastEpisodeStatusTracking (id, podcastEpisodeId, podcastEpisodeStatusId, createdAt)
VALUES (NEWID(), @Episode1Id, 5, DATEADD(DAY, -7, @CurrentDateTime)); -- Published

-- ========================================
-- EPISODE 2
-- ========================================
INSERT INTO PodcastEpisode (
    id,
    name,
    description,
    explicitContent,
    releaseDate,
    isReleased,
    mainImageFileKey,
    audioFileKey,
    audioFileSize,
    audioLength,
    audioFingerPrint,
    audioTranscript,
    podcastEpisodeSubscriptionTypeId,
    podcastShowId,
    seasonNumber,
    episodeOrder,
    totalSave,
    listenCount,
    isAudioPublishable,
    takenDownReason,
    deletedAt,
    createdAt,
    updatedAt
)
VALUES (
    @Episode2Id,
    N'Episode 2 - Phát triển',
    N'Episode thứ hai với nội dung chuyên sâu hơn',
    0,
    CAST(DATEADD(DAY, -5, @CurrentDateTime) AS DATE), -- Phát hành 5 ngày trước
    1,
    N'episode2-cover.jpg',
    N'episode2-audio.mp3',
    30.2, -- 30.2 MB
    2100, -- 35 phút
    NULL,
    N'Nội dung transcript của episode 2...',
    1, -- Free
    @ShowId,
    1, -- Season 1
    2, -- Episode order 2
    0,
    0,
    1,
    NULL,
    NULL,
    DATEADD(DAY, -8, @CurrentDateTime),
    @CurrentDateTime
);

-- Status tracking cho Episode 2
INSERT INTO PodcastEpisodeStatusTracking (id, podcastEpisodeId, podcastEpisodeStatusId, createdAt)
VALUES (NEWID(), @Episode2Id, 1, DATEADD(DAY, -8, @CurrentDateTime)); -- Draft

INSERT INTO PodcastEpisodeStatusTracking (id, podcastEpisodeId, podcastEpisodeStatusId, createdAt)
VALUES (NEWID(), @Episode2Id, 4, DATEADD(DAY, -6, @CurrentDateTime)); -- Ready To Release

INSERT INTO PodcastEpisodeStatusTracking (id, podcastEpisodeId, podcastEpisodeStatusId, createdAt)
VALUES (NEWID(), @Episode2Id, 5, DATEADD(DAY, -5, @CurrentDateTime)); -- Published

-- ========================================
-- EPISODE 3
-- ========================================
INSERT INTO PodcastEpisode (
    id,
    name,
    description,
    explicitContent,
    releaseDate,
    isReleased,
    mainImageFileKey,
    audioFileKey,
    audioFileSize,
    audioLength,
    audioFingerPrint,
    audioTranscript,
    podcastEpisodeSubscriptionTypeId,
    podcastShowId,
    seasonNumber,
    episodeOrder,
    totalSave,
    listenCount,
    isAudioPublishable,
    takenDownReason,
    deletedAt,
    createdAt,
    updatedAt
)
VALUES (
    @Episode3Id,
    N'Episode 3 - Kết nối',
    N'Episode thứ ba với các câu chuyện thú vị',
    0,
    CAST(DATEADD(DAY, -3, @CurrentDateTime) AS DATE), -- Phát hành 3 ngày trước
    1,
    N'episode3-cover.jpg',
    N'episode3-audio.mp3',
    28.7, -- 28.7 MB
    1950, -- 32.5 phút
    NULL,
    N'Nội dung transcript của episode 3...',
    1, -- Free
    @ShowId,
    1, -- Season 1
    3, -- Episode order 3
    0,
    0,
    1,
    NULL,
    NULL,
    DATEADD(DAY, -6, @CurrentDateTime),
    @CurrentDateTime
);

-- Status tracking cho Episode 3
INSERT INTO PodcastEpisodeStatusTracking (id, podcastEpisodeId, podcastEpisodeStatusId, createdAt)
VALUES (NEWID(), @Episode3Id, 1, DATEADD(DAY, -6, @CurrentDateTime)); -- Draft

INSERT INTO PodcastEpisodeStatusTracking (id, podcastEpisodeId, podcastEpisodeStatusId, createdAt)
VALUES (NEWID(), @Episode3Id, 4, DATEADD(DAY, -4, @CurrentDateTime)); -- Ready To Release




