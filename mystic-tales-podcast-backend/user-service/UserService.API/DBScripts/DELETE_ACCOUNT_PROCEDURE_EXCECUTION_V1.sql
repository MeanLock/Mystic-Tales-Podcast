-- =====================================================
-- MASTER ACCOUNT DELETION ORCHESTRATION SCRIPT
-- =====================================================
-- This script provides the proper execution order for deleting
-- an account across all microservices
-- =====================================================

/*
IMPORTANT NOTES:
1. Execute these procedures in the ORDER shown below
2. Each procedure should be executed on its respective database
3. Wrap all executions in a distributed transaction if possible
4. Always backup your databases before running deletion scripts
5. Test on a development environment first
6. Consider soft-delete (deactivation) instead of hard-delete for audit purposes

EXECUTION ORDER EXPLANATION:
- Transaction Service: Delete financial records first (no dependencies on other services)
- Subscription Service: Delete subscription registrations
- Moderation Service: Delete reports and review sessions
- Podcast Service: Delete podcast content and listen sessions
- Booking Service: Delete bookings and related data
- User Service: Delete the account itself (final step)
*/

-- =====================================================
-- STEP 1: TRANSACTION SERVICE [Port: 8076]
-- Execute on Transaction Service Database
-- =====================================================
PRINT '========================================';
PRINT 'STEP 1: Deleting from Transaction Service';
PRINT '========================================';

-- First, get all booking IDs associated with the account from Booking Service
-- You'll need to query this from the Booking Service database
DECLARE @accountIdToDelete INT = 123; -- REPLACE WITH ACTUAL ACCOUNT ID
DECLARE @bookingIdsForDeletion NVARCHAR(MAX);

-- Option A: If you have cross-database access
/*
SELECT @bookingIdsForDeletion = STRING_AGG(CAST(id AS NVARCHAR(10)), ',')
FROM [BookingServiceDB].dbo.Booking
WHERE accountId = @accountIdToDelete OR podcastBuddyId = @accountIdToDelete;
*/

-- Option B: Manual specification (get these IDs from Booking Service first)
-- SET @bookingIdsForDeletion = '1,2,3,4,5';

-- Execute deletion
DECLARE @accountIdToDelete INT = 123;
EXEC DeleteAccountFromTransactionService @accountId = @accountIdToDelete;
-- OR if using booking IDs:
-- EXEC DeleteAccountFromTransactionServiceWithBookingIds 
--      @accountId = @accountIdToDelete, 
--      @bookingIds = @bookingIdsForDeletion;

-- =====================================================
-- STEP 2: SUBSCRIPTION SERVICE [Port: 8066]
-- Execute on Subscription Service Database
-- =====================================================
PRINT '========================================';
PRINT 'STEP 2: Deleting from Subscription Service';
PRINT '========================================';
DECLARE @accountIdToDelete INT = 123;
EXEC DeleteAccountFromSubscriptionService @accountId = @accountIdToDelete;

-- =====================================================
-- STEP 3: MODERATION SERVICE [Port: 8071]
-- Execute on Moderation Service Database
-- =====================================================
PRINT '========================================';
PRINT 'STEP 3: Deleting from Moderation Service';
PRINT '========================================';
DECLARE @accountIdToDelete INT = 123;
EXEC DeleteAccountFromModerationService @accountId = @accountIdToDelete;

-- =====================================================
-- STEP 4: PODCAST SERVICE [Port: 8061]
-- Execute on Podcast Service Database
-- =====================================================
PRINT '========================================';
PRINT 'STEP 4: Deleting from Podcast Service';
PRINT '========================================';
DECLARE @accountIdToDelete INT = 123;
EXEC DeleteAccountFromPodcastService @accountId = @accountIdToDelete;

-- =====================================================
-- STEP 5: BOOKING SERVICE [Port: 8056]
-- Execute on Booking Management Service Database
-- =====================================================
PRINT '========================================';
PRINT 'STEP 5: Deleting from Booking Service';
PRINT '========================================';
DECLARE @accountIdToDelete INT = 123;
EXEC DeleteAccountFromBookingService @accountId = @accountIdToDelete;

-- =====================================================
-- STEP 6: USER SERVICE [Port: 8046]
-- Execute on User Service Database
-- THIS IS THE FINAL STEP - DELETE THE ACCOUNT ITSELF
-- =====================================================
PRINT '========================================';
PRINT 'STEP 6: Deleting from User Service (FINAL)';
PRINT '========================================';
DECLARE @accountIdToDelete INT = 123;
EXEC DeleteAccountFromUserService @accountId = @accountIdToDelete;

-- =====================================================
-- COMPLETION
-- =====================================================
PRINT '========================================';
PRINT 'Account deletion process completed!';
PRINT 'Account ID: ' + CAST(@accountIdToDelete AS NVARCHAR(10));
PRINT '========================================';

-- =====================================================
-- VERIFICATION QUERIES (Optional)
-- Run these to verify the account has been completely removed
-- =====================================================

/*
-- User Service
SELECT COUNT(*) AS UserServiceRecords FROM Account WHERE id = @accountIdToDelete;

-- Booking Service
SELECT COUNT(*) AS BookingServiceRecords FROM Booking WHERE accountId = @accountIdToDelete OR podcastBuddyId = @accountIdToDelete;

-- Podcast Service
SELECT COUNT(*) AS PodcastServiceRecords FROM PodcastChannel WHERE podcasterId = @accountIdToDelete;

-- Subscription Service
SELECT COUNT(*) AS SubscriptionServiceRecords FROM PodcastSubscriptionRegistration WHERE accountId = @accountIdToDelete;

-- Moderation Service
SELECT COUNT(*) AS ModerationServiceRecords FROM PodcastBuddyReport WHERE accountId = @accountIdToDelete;

-- Transaction Service
SELECT COUNT(*) AS TransactionServiceRecords FROM AccountBalanceTransaction WHERE accountId = @accountIdToDelete;

-- All counts should return 0
*/