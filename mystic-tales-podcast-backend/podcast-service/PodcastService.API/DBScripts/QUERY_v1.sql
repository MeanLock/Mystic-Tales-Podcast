select * from PodcastChannel
select * from PodcastChannelStatusTracking
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