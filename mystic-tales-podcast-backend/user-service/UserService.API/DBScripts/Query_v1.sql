Select * from Account
Select * from PodcasterProfile
select * from PodcastBuddyReview
select * from AccountFollowedPodcaster

Delete from PodcasterProfile

EXEC sp_rename 'PodcastEpisode.title', 'name', 'COLUMN';

ALTER TABLE PodcasterProfile 
DROP CONSTRAINT DF__Podcaster__isVer__787EE5A0;

ALTER TABLE PodcasterProfile 
ADD CONSTRAINT DF_PodcasterProfile_isVerified DEFAULT NULL FOR isVerified;

ALTER TABLE PodcasterProfile
ADD totalFollow INT NOT NULL DEFAULT 0;