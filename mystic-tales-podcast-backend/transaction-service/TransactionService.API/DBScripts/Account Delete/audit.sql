-- =====================================================
-- TRANSACTION SERVICE DATA
-- =====================================================
PRINT '--- TRANSACTION SERVICE ---';
DECLARE @accountId INT = 6010; -- REPLACE WITH ACTUAL ACCOUNT ID
PRINT '--- TRANSACTION SERVICE ---';

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