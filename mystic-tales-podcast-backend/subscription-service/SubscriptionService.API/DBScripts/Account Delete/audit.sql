-- =====================================================
-- SUBSCRIPTION SERVICE DATA
-- =====================================================
PRINT '--- SUBSCRIPTION SERVICE ---';
DECLARE @accountId INT = 4010; -- REPLACE WITH ACTUAL ACCOUNT ID
-- Active Subscriptions
SELECT 
    'Podcast Subscriptions' AS Type,
    COUNT(*) AS Total,
    COUNT(CASE WHEN cancelledAt IS NULL THEN 1 END) AS Active,
    COUNT(CASE WHEN cancelledAt IS NOT NULL THEN 1 END) AS Cancelled
FROM PodcastSubscriptionRegistration
WHERE accountId = @accountId
UNION ALL
SELECT 
    'Member Subscriptions' AS Type,
    COUNT(*) AS Total,
    COUNT(CASE WHEN cancelledAt IS NULL THEN 1 END) AS Active,
    COUNT(CASE WHEN cancelledAt IS NOT NULL THEN 1 END) AS Cancelled
FROM MemberSubscriptionRegistration
WHERE accountId = @accountId;

-- Subscription Details
SELECT 
    ps.name AS SubscriptionName,
    sct.name AS CycleType,
    psr.lastPaidAt,
    psr.cancelledAt,
    psr.createdAt
FROM PodcastSubscriptionRegistration psr
INNER JOIN PodcastSubscription ps ON psr.podcastSubscriptionId = ps.id
INNER JOIN SubscriptionCycleType sct ON psr.subscriptionCycleTypeId = sct.id
WHERE psr.accountId = @accountId;

PRINT '';