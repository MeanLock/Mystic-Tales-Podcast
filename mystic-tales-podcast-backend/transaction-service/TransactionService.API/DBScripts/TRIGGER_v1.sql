-- =====================================================
-- TRANSACTION SERVICE TRIGGERS [Port: 8076]
-- =====================================================

-- Trigger for AccountBalanceTransaction table
CREATE TRIGGER TR_AccountBalanceTransaction_UpdatedAt
ON AccountBalanceTransaction
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE AccountBalanceTransaction
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM AccountBalanceTransaction a
    INNER JOIN inserted i ON a.id = i.id;
END;
GO

-- Trigger for PodcastSubscriptionTransaction table
CREATE TRIGGER TR_PodcastSubscriptionTransaction_UpdatedAt
ON PodcastSubscriptionTransaction
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastSubscriptionTransaction
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastSubscriptionTransaction p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

-- Trigger for MemberSubscriptionTransaction table
CREATE TRIGGER TR_MemberSubscriptionTransaction_UpdatedAt
ON MemberSubscriptionTransaction
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE MemberSubscriptionTransaction
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM MemberSubscriptionTransaction m
    INNER JOIN inserted i ON m.id = i.id;
END;
GO

-- Trigger for BookingTransaction table
CREATE TRIGGER TR_BookingTransaction_UpdatedAt
ON BookingTransaction
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE BookingTransaction
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM BookingTransaction b
    INNER JOIN inserted i ON b.id = i.id;
END;
GO

-- Trigger for BookingStorageTransaction table
CREATE TRIGGER TR_BookingStorageTransaction_UpdatedAt
ON BookingStorageTransaction
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE BookingStorageTransaction
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM BookingStorageTransaction b
    INNER JOIN inserted i ON b.id = i.id;
END;
GO