-- =====================================================
-- TRANSACTION SERVICE DATA
-- =====================================================
PRINT '--- TRANSACTION SERVICE ---';
DECLARE @accountId INT = 4010; -- REPLACE WITH ACTUAL ACCOUNT ID
-- Balance Transactions
SELECT 
    tt.name AS TransactionType,
    COUNT(*) AS Count,
    SUM(abt.amount) AS TotalAmount
FROM AccountBalanceTransaction abt
INNER JOIN TransactionType tt ON abt.transactionTypeId = tt.id
WHERE abt.accountId = @accountId
GROUP BY tt.name;

-- Withdrawal Requests
SELECT 
    'Withdrawal Requests' AS Type,
    COUNT(*) AS Total,
    COUNT(CASE WHEN isRejected = 0 THEN 1 END) AS Approved,
    COUNT(CASE WHEN isRejected = 1 THEN 1 END) AS Rejected,
    COUNT(CASE WHEN isRejected IS NULL THEN 1 END) AS Pending,
    SUM(amount) AS TotalAmount
FROM AccountBalanceWithdrawalRequest
WHERE accountId = @accountId;

-- Subscription Transactions
SELECT 
    'Subscription Transactions' AS Type,
    COUNT(*) AS Count,
    SUM(amount) AS TotalAmount,
    SUM(ISNULL(profit, 0)) AS TotalProfit
FROM PodcastSubscriptionTransaction pst
INNER JOIN PodcastSubscriptionRegistration psr ON pst.podcastSubscriptionRegistrationId = psr.id
WHERE psr.accountId = @accountId;

-- Booking Transactions (requires cross-database query or parameter)
-- Note: This might need adjustment based on your architecture
SELECT 
    'Booking Transactions' AS Type,
    COUNT(*) AS Count,
    SUM(amount) AS TotalAmount,
    SUM(ISNULL(profit, 0)) AS TotalProfit
FROM BookingTransaction
WHERE bookingId IN (
    SELECT id FROM Booking 
    WHERE accountId = @accountId OR podcastBuddyId = @accountId
);

-- Storage Transactions
SELECT 
    'Storage Purchases' AS Type,
    COUNT(*) AS Count,
    SUM(amount) AS TotalAmount,
    SUM(storageSize) AS TotalStorageGB
FROM BookingStorageTransaction
WHERE accountId = @accountId;

PRINT '';