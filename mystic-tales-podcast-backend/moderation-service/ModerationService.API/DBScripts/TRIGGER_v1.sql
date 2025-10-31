-- =====================================================
-- MODERATION SERVICE TRIGGERS [Port: 8071]
-- =====================================================

-- Trigger for PodcastBuddyReportReviewSession table
CREATE TRIGGER TR_PodcastBuddyReportReviewSession_UpdatedAt
ON PodcastBuddyReportReviewSession
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastBuddyReportReviewSession
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastBuddyReportReviewSession p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

-- Trigger for PodcastShowReportReviewSession table
CREATE TRIGGER TR_PodcastShowReportReviewSession_UpdatedAt
ON PodcastShowReportReviewSession
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastShowReportReviewSession
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastShowReportReviewSession p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

-- Trigger for PodcastEpisodeReportReviewSession table
CREATE TRIGGER TR_PodcastEpisodeReportReviewSession_UpdatedAt
ON PodcastEpisodeReportReviewSession
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastEpisodeReportReviewSession
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastEpisodeReportReviewSession p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

-- Trigger for DMCAAccusation table
CREATE TRIGGER TR_DMCAAccusation_UpdatedAt
ON DMCAAccusation
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE DMCAAccusation
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM DMCAAccusation d
    INNER JOIN inserted i ON d.id = i.id;
END;
GO

-- Trigger for CounterNotice table
CREATE TRIGGER TR_CounterNotice_UpdatedAt
ON CounterNotice
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE CounterNotice
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM CounterNotice c
    INNER JOIN inserted i ON c.id = i.id;
END;
GO

-- Trigger for DMCANotice table
CREATE TRIGGER TR_DMCANotice_UpdatedAt
ON DMCANotice
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE DMCANotice
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM DMCANotice d
    INNER JOIN inserted i ON d.id = i.id;
END;
GO

-- Trigger for LawsuitProof table
CREATE TRIGGER TR_LawsuitProof_UpdatedAt
ON LawsuitProof
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE LawsuitProof
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM LawsuitProof l
    INNER JOIN inserted i ON l.id = i.id;
END;
GO

-- Trigger for DMCAAccusationConclusionReport table
CREATE TRIGGER TR_DMCAAccusationConclusionReport_UpdatedAt
ON DMCAAccusationConclusionReport
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE DMCAAccusationConclusionReport
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM DMCAAccusationConclusionReport d
    INNER JOIN inserted i ON d.id = i.id;
END;
GO