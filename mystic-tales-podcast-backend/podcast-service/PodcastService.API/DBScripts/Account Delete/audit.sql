-- =====================================================
-- PODCAST SERVICE DATA
-- =====================================================
PRINT '--- PODCAST SERVICE ---';
DECLARE @accountId INT = 6010; -- REPLACE WITH ACTUAL ACCOUNT ID
-- Content Summary
SELECT 
    'Channels Created' AS Type,
    COUNT(*) AS Count,
    SUM(totalFavorite) AS TotalFavorites,
    SUM(listenCount) AS TotalListens
FROM PodcastChannel
WHERE podcasterId = @accountId
UNION ALL
SELECT 
    'Shows Created' AS Type,
    COUNT(*) AS Count,
    SUM(totalFollow) AS TotalFollowers,
    SUM(listenCount) AS TotalListens
FROM PodcastShow
WHERE podcasterId = @accountId;

-- Episodes Summary
SELECT 
    'Episodes Created' AS Type,
    COUNT(*) AS Count,
    SUM(totalSave) AS TotalSaves
FROM PodcastEpisode pe
INNER JOIN PodcastShow ps ON pe.podcastShowId = ps.id
WHERE ps.podcasterId = @accountId;

-- Top Channels
SELECT TOP 5
    id,
    name,
    totalFavorite,
    listenCount,
    createdAt
FROM PodcastChannel
WHERE podcasterId = @accountId
ORDER BY listenCount DESC;

-- Top Shows
SELECT TOP 5
    id,
    name,
    totalFollow,
    listenCount,
    averageRating,
    ratingCount
FROM PodcastShow
WHERE podcasterId = @accountId
ORDER BY listenCount DESC;

-- Listen History
SELECT 
    'Listen Sessions' AS Type,
    COUNT(*) AS Count,
    COUNT(CASE WHEN isCompleted = 1 THEN 1 END) AS Completed
FROM PodcastEpisodeListenSession
WHERE accountId = @accountId;

-- Reviews Written
SELECT 
    'Show Reviews Written' AS Type,
    COUNT(*) AS Count,
    AVG(rating) AS AverageRating
FROM PodcastShowReview
WHERE accountId = @accountId;

PRINT '';