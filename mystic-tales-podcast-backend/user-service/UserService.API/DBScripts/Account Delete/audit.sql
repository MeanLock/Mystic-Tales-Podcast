-- =====================================================
-- USER SERVICE DATA
-- =====================================================
PRINT '--- USER SERVICE ---';
DECLARE @accountId INT = 4010;
-- Account Details
SELECT 
    'Account Info' AS Category,
    email,
    fullName,
    roleId,
    balance,
    violationPoint,
    violationLevel,
    createdAt
FROM Account
WHERE id = @accountId;

-- Podcaster Profile
SELECT 
    'Podcaster Profile' AS Category,
    name,
    averageRating,
    ratingCount,
    totalFollow,
    listenCount,
    isVerified,
    isBuddy
FROM PodcasterProfile
WHERE accountId = @accountId;

-- Relationships Count
SELECT 
    'Following' AS Type,
    COUNT(*) AS Count
FROM AccountFollowedPodcaster
WHERE accountId = @accountId
UNION ALL
SELECT 
    'Followers' AS Type,
    COUNT(*) AS Count
FROM AccountFollowedPodcaster
WHERE podcasterId = @accountId
UNION ALL
SELECT 
    'Favorited Channels' AS Type,
    COUNT(*) AS Count
FROM AccountFavoritedPodcastChannel
WHERE accountId = @accountId
UNION ALL
SELECT 
    'Followed Shows' AS Type,
    COUNT(*) AS Count
FROM AccountFollowedPodcastShow
WHERE accountId = @accountId
UNION ALL
SELECT 
    'Saved Episodes' AS Type,
    COUNT(*) AS Count
FROM AccountSavedPodcastEpisode
WHERE accountId = @accountId
UNION ALL
SELECT 
    'Notifications' AS Type,
    COUNT(*) AS Count
FROM AccountNotification
WHERE accountId = @accountId
UNION ALL
SELECT 
    'Reviews Written' AS Type,
    COUNT(*) AS Count
FROM PodcastBuddyReview
WHERE accountId = @accountId
UNION ALL
SELECT 
    'Reviews Received' AS Type,
    COUNT(*) AS Count
FROM PodcastBuddyReview
WHERE podcastBuddyId = @accountId;

PRINT '';