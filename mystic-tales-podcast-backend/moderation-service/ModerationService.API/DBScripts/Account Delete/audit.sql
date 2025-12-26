PRINT '--- MODERATION SERVICE ---';
DECLARE @accountId INT = 6010; -- REPLACE WITH ACTUAL ACCOUNT ID
-- Reports Filed
SELECT 
    'Buddy Reports Filed' AS Type,
    COUNT(*) AS Count,
    COUNT(CASE WHEN resolvedAt IS NOT NULL THEN 1 END) AS Resolved
FROM PodcastBuddyReport
WHERE accountId = @accountId
UNION ALL
SELECT 
    'Show Reports Filed' AS Type,
    COUNT(*) AS Count,
    COUNT(CASE WHEN resolvedAt IS NOT NULL THEN 1 END) AS Resolved
FROM PodcastShowReport
WHERE accountId = @accountId
UNION ALL
SELECT 
    'Episode Reports Filed' AS Type,
    COUNT(*) AS Count,
    COUNT(CASE WHEN resolvedAt IS NOT NULL THEN 1 END) AS Resolved
FROM PodcastEpisodeReport
WHERE accountId = @accountId
UNION ALL
SELECT 
    'Reports Against User' AS Type,
    COUNT(*) AS Count,
    COUNT(CASE WHEN resolvedAt IS NOT NULL THEN 1 END) AS Resolved
FROM PodcastBuddyReport
WHERE podcastBuddyId = @accountId;

-- Staff Review Sessions
SELECT 
    'Buddy Report Reviews' AS Type,
    COUNT(*) AS Total,
    COUNT(CASE WHEN isResolved = 1 THEN 1 END) AS Resolved,
    COUNT(CASE WHEN isResolved = 0 THEN 1 END) AS Rejected,
    COUNT(CASE WHEN isResolved IS NULL THEN 1 END) AS Pending
FROM PodcastBuddyReportReviewSession
WHERE assignedStaff = @accountId
UNION ALL
SELECT 
    'Show Report Reviews' AS Type,
    COUNT(*) AS Total,
    COUNT(CASE WHEN isResolved = 1 THEN 1 END) AS Resolved,
    COUNT(CASE WHEN isResolved = 0 THEN 1 END) AS Rejected,
    COUNT(CASE WHEN isResolved IS NULL THEN 1 END) AS Pending
FROM PodcastShowReportReviewSession
WHERE assignedStaff = @accountId
UNION ALL
SELECT 
    'Episode Report Reviews' AS Type,
    COUNT(*) AS Total,
    COUNT(CASE WHEN isResolved = 1 THEN 1 END) AS Resolved,
    COUNT(CASE WHEN isResolved = 0 THEN 1 END) AS Rejected,
    COUNT(CASE WHEN isResolved IS NULL THEN 1 END) AS Pending
FROM PodcastEpisodeReportReviewSession
WHERE assignedStaff = @accountId;

PRINT '';