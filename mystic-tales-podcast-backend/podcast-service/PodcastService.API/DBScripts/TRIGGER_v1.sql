-- =====================================================
-- PODCAST SERVICE TRIGGERS [Port: 8061]
-- =====================================================

-- Trigger for PodcastChannel table
CREATE TRIGGER TR_PodcastChannel_UpdatedAt
ON PodcastChannel
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastChannel
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastChannel p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

-- Trigger for PodcastShow table
CREATE TRIGGER TR_PodcastShow_UpdatedAt
ON PodcastShow
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastShow
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastShow p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

-- Trigger for PodcastEpisode table
CREATE TRIGGER TR_PodcastEpisode_UpdatedAt
ON PodcastEpisode
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastEpisode
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastEpisode p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

-- Trigger for PodcastEpisodePublishReviewSession table
CREATE TRIGGER TR_PodcastEpisodePublishReviewSession_UpdatedAt
ON PodcastEpisodePublishReviewSession
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastEpisodePublishReviewSession
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastEpisodePublishReviewSession p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

-- Trigger for PodcastShowReview table
CREATE TRIGGER TR_PodcastShowReview_UpdatedAt
ON PodcastShowReview
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastShowReview
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastShowReview p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO