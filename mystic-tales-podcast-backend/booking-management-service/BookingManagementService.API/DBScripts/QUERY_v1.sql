select * from BookingProducingRequest


-- Add deadlineDays to Booking table
ALTER TABLE Booking
ADD deadlineDays INT NULL;

-- Add deadlineDays to BookingProducingRequest table
ALTER TABLE BookingProducingRequest
ADD deadlineDays INT NULL;

-- =====================================================
-- ALLOW NULL FOR deadline COLUMN IN BookingProducingRequest
-- =====================================================

-- Modify deadline column to allow NULL
ALTER TABLE BookingProducingRequest
ALTER COLUMN deadline DATETIME NULL;