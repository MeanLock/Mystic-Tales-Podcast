Select * from Account
Select * from PodcasterProfile
select * from PodcastBuddyReview
select * from AccountFollowedPodcaster
select * from AccountFavoritedPodcastChannel
select * from AccountFollowedPodcastShow
select * from AccountSavedPodcastEpisode
select * from PasswordResetToken

Delete from Account where id = 8011
Delete from PodcasterProfile where accountId = 8008

Delete from PodcastBuddyReview where accountId = 1012

ALTER TABLE PodcasterProfile
ADD listenCount INT NOT NULL DEFAULT 0;

DROP TABLE AccountPodcastListenHistory

ALTER TABLE PodcasterProfile
drop column ListenCount ;

ALTER TABLE PodcasterProfile
DROP CONSTRAINT DF__Podcaster__Liste__73852659

ALTER TABLE AccountNotification
ADD CONSTRAINT FK_AccountNotification_NotificationType 
FOREIGN KEY (notificationTypeId) REFERENCES NotificationType(id);

Delete from AccountFavoritedPodcastChannel
Delete from PodcasterProfile

EXEC sp_rename 'PodcastEpisode.title', 'name', 'COLUMN';

ALTER TABLE PodcasterProfile 
DROP CONSTRAINT DF__Podcaster__isVer__787EE5A0;

ALTER TABLE PodcasterProfile 
ADD CONSTRAINT DF_PodcasterProfile_isVerified DEFAULT NULL FOR isVerified;

ALTER TABLE PodcasterProfile
ADD totalFollow INT NOT NULL DEFAULT 0;

ALTER TABLE AccountPodcastListenHistory
ADD 
    lastListenDurationSeconds INT NOT NULL DEFAULT 0,
    isCompleted BIT NOT NULL DEFAULT 0;


ALTER TABLE PodcasterProfile
ADD verifiedAt DATETIME NULL DEFAULT NULL;