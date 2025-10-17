select * from PodcastChannel
select * from PodcastChannelStatusTracking
select * from PodcastShow
select * from PodcastShowStatusTracking
select * from Hashtag
select * from PodcastChannelHashtag
select * from PodcastShow
select * from PodcastCategory
select * from PodcastSubCategory
select * from PodcastShowSubscriptionType



delete from PodcastChannelStatusTracking
delete from PodcastChannel


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




ALTER TABLE PodcastShow DROP CONSTRAINT DF__PodcastSh__podca__6C190EBB
-- 2. Đổi tên bảng
EXEC sp_rename 'PodcastShowsSubscriptionType', 'PodcastShowSubscriptionType';

-- 3. Đổi tên cột FK trong bảng PodcastShow
EXEC sp_rename 'PodcastShow.podcastShowsSubscriptionTypeId', 'podcastShowSubscriptionTypeId', 'COLUMN';

-- 4. Tạo lại foreign key constraint với tên mới
ALTER TABLE PodcastShow
ADD CONSTRAINT FK_PodcastShow_PodcastShowSubscriptionType
FOREIGN KEY (podcastShowSubscriptionTypeId) 
REFERENCES PodcastShowSubscriptionType(id);



