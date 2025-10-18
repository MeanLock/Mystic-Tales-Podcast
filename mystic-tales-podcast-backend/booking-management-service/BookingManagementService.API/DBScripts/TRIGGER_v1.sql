-- =====================================================
-- BOOKING MANAGEMENT SERVICE TRIGGERS [Port: 8056]
-- =====================================================

-- Trigger for Booking table
CREATE TRIGGER TR_Booking_UpdatedAt
ON Booking
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Booking
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM Booking b
    INNER JOIN inserted i ON b.id = i.id;
END;
GO