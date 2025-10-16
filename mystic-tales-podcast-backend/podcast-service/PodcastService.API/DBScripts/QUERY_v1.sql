select * from PodcastChannel
select * from PodcastChannelStatusTracking
select * from Hashtag
select * from PodcastChannelHashtag

delete from PodcastChannelStatusTracking
delete from PodcastChannel


ALTER TABLE PodcastEpisode
ADD episodeOrder INT NOT NULL DEFAULT 1;



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
