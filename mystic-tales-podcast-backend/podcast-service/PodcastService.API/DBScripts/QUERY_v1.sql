select * from PodcastChannel
select * from PodcastChannelStatusTracking
select * from PodcastShow
select * from PodcastShowReview
select * from PodcastShowStatusTracking where podcastShowId = N'b06353ee-668a-4d71-86dc-dc7374a0fd9c'
select * from Hashtag
select * from PodcastChannelHashtag
select * from PodcastShow
select * from PodcastCategory
select * from PodcastSubCategory
select * from PodcastShowSubscriptionType
select * from PodcastEpisode where id = N'b06353ee-668a-4d71-86dc-dc7374a0fd9c'
select * from PodcastEpisodeStatusTracking order by createdAt
select * from PodcastEpisodeLicense

select * from PodcastEpisodePublishReviewSession
select * from PodcastEpisodePublishReviewSessionStatusTracking
select * from PodcastEpisodePublishDuplicateDetection
select * from PodcastEpisodeIllegalContentTypeMarking
select * from PodcastBackgroundSoundTrack

select * from PodcastEpisodeListenSession order by createdAt DESC
where podcastEpisodeId = N'b06353ee-668a-4d71-86dc-dc7374a0fd9c' order by createdAt DESC
select * from PodcastEpisodeListenSession order by createdAt DESC

delete from PodcastEpisodeIllegalContentTypeMarking
delete from PodcastEpisodePublishDuplicateDetection
delete from PodcastEpisodePublishReviewSession



delete from PodcastChannelStatusTracking
delete from PodcastChannel
delete from PodcastShowReview
INSERT INTO PodcastEpisodeStatusTracking (podcastEpisodeId, podcastEpisodeStatusId)
VALUES (N'9735b0a8-f38e-4f93-a86d-1fda7558872a' , 4); -- pending editrequest
INSERT INTO PodcastShowStatusTracking(podcastShowId, podcastShowStatusId)
VALUES (N'b4988aad-58cb-4c17-937e-3ad5e65336ce' , 3); 
INSERT INTO PodcastEpisodePublishReviewSessionStatusTracking(podcastEpisodePublishReviewSessionId, podcastEpisodePublishReviewSessionStatusId)
VALUES (7 , 3); 

ALTER TABLE PodcastCategory
ADD mainImageFileKey NVARCHAR(MAX) NULL;

-- Query cơ bản: Lấy tất cả episode của podcaster 17
SELECT 
    pe.id AS EpisodeId,
    pe.name AS EpisodeName,
    pe.description AS EpisodeDescription,
    pe.releaseDate AS ReleaseDate,
    pe.isReleased AS IsReleased,
    pe.seasonNumber AS Season,
    pe.episodeOrder AS EpisodeNumber,
    pe.listenCount AS ListenCount,
    pe.totalSave AS TotalSave,
    pe.audioLength AS AudioLengthSeconds,
    pe.deletedAt AS DeletedAt,
    ps.id AS ShowId,
    ps.name AS ShowName,
    ps.podcastChannelId AS ChannelId
FROM PodcastEpisode pe
INNER JOIN PodcastShow ps ON pe.podcastShowId = ps.id
WHERE ps.podcasterId = 17
    AND pe.deletedAt IS NULL  -- Chỉ lấy episode chưa bị xóa
ORDER BY ps.name, pe.seasonNumber, pe.episodeOrder;
-- chạy trong podcast db
CREATE TABLE PodcastBackgroundSoundTrack
(
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    name NVARCHAR(50) NOT NULL,
    description NVARCHAR(MAX) NULL,
    mainImageFileKey NVARCHAR(MAX) NULL,
    audioFileKey NVARCHAR(MAX) NULL,
    createdAt DATETIME NOT NULL DEFAULT GETDATE(),
    updatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- chạy trong transaction db
CREATE TABLE AccountBalanceWithdrawalRequest
(
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    accountId INT NOT NULL, -- Không có FK vì Account nằm trong UserService
    amount DECIMAL(18,2) NOT NULL,
    transferReceiptImageFileKey NVARCHAR(MAX) NULL,
    rejectReason NVARCHAR(MAX) NULL,
    isRejected BIT NULL, -- NULL: đang chờ, 1: rejected, 0: approved
    completedAt DATETIME NULL,
    createdAt DATETIME NOT NULL DEFAULT GETDATE(),
    updatedAt DATETIME NOT NULL DEFAULT GETDATE()
);







