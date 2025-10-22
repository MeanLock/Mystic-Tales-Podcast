Select * from Account
Select * from PodcasterProfile
select * from PodcastBuddyReview
select * from AccountFollowedPodcaster
select * from AccountFavoritedPodcastChannel
select * from AccountFollowedPodcastShow
select * from AccountSavedPodcastEpisode




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