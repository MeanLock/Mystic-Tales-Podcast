-- =====================================================
-- BOOKING SERVICE DATA
-- =====================================================

PRINT '--- BOOKING SERVICE ---';
DECLARE @accountId INT = 4010; -- REPLACE WITH ACTUAL ACCOUNT ID
-- Bookings Summary
SELECT 
    'Bookings as Customer' AS Type,
    COUNT(*) AS Count,
    ISNULL(SUM(price), 0) AS TotalValue
FROM Booking
WHERE accountId = @accountId
UNION ALL
SELECT 
    'Bookings as Buddy' AS Type,
    COUNT(*) AS Count,
    ISNULL(SUM(price), 0) AS TotalValue
FROM Booking
WHERE podcastBuddyId = @accountId
UNION ALL
SELECT 
    'Bookings as Staff' AS Type,
    COUNT(*) AS Count,
    ISNULL(SUM(price), 0) AS TotalValue
FROM Booking
WHERE assignedStaffId = @accountId;

-- Booking Details
SELECT TOP 10
    b.id AS BookingId,
    b.title,
    b.price,
    b.createdAt,
    bs.name AS CurrentStatus,
    CASE 
        WHEN b.accountId = @accountId THEN 'Customer'
        WHEN b.podcastBuddyId = @accountId THEN 'Buddy'
        WHEN b.assignedStaffId = @accountId THEN 'Staff'
    END AS Role
FROM Booking b
CROSS APPLY (
    SELECT TOP 1 bs.name
    FROM BookingStatusTracking bst
    INNER JOIN BookingStatus bs ON bst.bookingStatusId = bs.id
    WHERE bst.bookingId = b.id
    ORDER BY bst.createdAt DESC
) bs
WHERE b.accountId = @accountId 
   OR b.podcastBuddyId = @accountId 
   OR b.assignedStaffId = @accountId
ORDER BY b.createdAt DESC;

-- Chat Data
SELECT 
    'Chat Rooms' AS Type,
    COUNT(DISTINCT bcm.chatRoomId) AS Count
FROM BookingChatMember bcm
WHERE bcm.accountId = @accountId
UNION ALL
SELECT 
    'Chat Messages Sent' AS Type,
    COUNT(*) AS Count
FROM BookingChatMessages
WHERE senderId = @accountId;

PRINT '';