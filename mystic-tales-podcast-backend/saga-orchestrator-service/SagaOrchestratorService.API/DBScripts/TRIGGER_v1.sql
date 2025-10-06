-- =====================================================
-- SAGA ORCHESTRATOR SERVICE TRIGGERS
-- =====================================================

-- Trigger for SagaInstance table
CREATE TRIGGER TR_SagaInstance_UpdatedAt
ON SagaInstance
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE SagaInstance
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM SagaInstance s
    INNER JOIN inserted i ON s.id = i.id;
END;
GO