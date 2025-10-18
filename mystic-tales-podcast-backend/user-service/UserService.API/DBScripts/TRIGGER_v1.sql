-- =====================================================
-- USER SERVICE TRIGGERS [Port: 8046]
-- =====================================================

-- Trigger for Account table
CREATE TRIGGER TR_Account_UpdatedAt
ON Account
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Account
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM Account a
    INNER JOIN inserted i ON a.id = i.id;
END;
GO

-- Trigger for PodcasterProfile table
CREATE TRIGGER TR_PodcasterProfile_UpdatedAt
ON PodcasterProfile
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcasterProfile
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcasterProfile p
    INNER JOIN inserted i ON p.accountId = i.accountId;
END;
GO

-- Trigger for PodcastBuddyReview table
CREATE TRIGGER TR_PodcastBuddyReview_UpdatedAt
ON PodcastBuddyReview
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastBuddyReview
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastBuddyReview p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

-- =====================================================
-- SYSTEM CONFIGURATION SERVICE TRIGGERS [Port: 8051]
-- =====================================================

-- Trigger for SystemConfigProfile table
CREATE TRIGGER TR_SystemConfigProfile_UpdatedAt
ON SystemConfigProfile
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE SystemConfigProfile
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM SystemConfigProfile s
    INNER JOIN inserted i ON s.id = i.id;
END;
GO

-- Trigger for PodcastSubscriptionConfig table
CREATE TRIGGER TR_PodcastSubscriptionConfig_UpdatedAt
ON PodcastSubscriptionConfig
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastSubscriptionConfig
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastSubscriptionConfig p
    INNER JOIN inserted i ON p.configProfileId = i.configProfileId 
        AND p.subscriptionCycleTypeId = i.subscriptionCycleTypeId;
END;
GO

-- Trigger for PodcastSuggestionConfig table
CREATE TRIGGER TR_PodcastSuggestionConfig_UpdatedAt
ON PodcastSuggestionConfig
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastSuggestionConfig
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastSuggestionConfig p
    INNER JOIN inserted i ON p.configProfileId = i.configProfileId;
END;
GO

-- Trigger for BookingConfig table
CREATE TRIGGER TR_BookingConfig_UpdatedAt
ON BookingConfig
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE BookingConfig
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM BookingConfig b
    INNER JOIN inserted i ON b.configProfileId = i.configProfileId;
END;
GO

-- Trigger for AccountConfig table
CREATE TRIGGER TR_AccountConfig_UpdatedAt
ON AccountConfig
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE AccountConfig
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM AccountConfig a
    INNER JOIN inserted i ON a.configProfileId = i.configProfileId;
END;
GO

-- Trigger for AccountViolationLevelConfig table
CREATE TRIGGER TR_AccountViolationLevelConfig_UpdatedAt
ON AccountViolationLevelConfig
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE AccountViolationLevelConfig
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM AccountViolationLevelConfig a
    INNER JOIN inserted i ON a.configProfileId = i.configProfileId 
        AND a.violationLevel = i.violationLevel;
END;
GO

-- Trigger for ReviewSessionConfig table
CREATE TRIGGER TR_ReviewSessionConfig_UpdatedAt
ON ReviewSessionConfig
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ReviewSessionConfig
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM ReviewSessionConfig r
    INNER JOIN inserted i ON r.configProfileId = i.configProfileId;
END;
GO

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

-- =====================================================
-- SUBSCRIPTION SERVICE TRIGGERS [Port: 8066]
-- =====================================================

-- Trigger for PodcastSubscription table
CREATE TRIGGER TR_PodcastSubscription_UpdatedAt
ON PodcastSubscription
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastSubscription
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastSubscription p
    INNER JOIN inserted i ON p.id = i.id;
END;
GO

-- Trigger for PodcastSubscriptionCycleTypePrice table
CREATE TRIGGER TR_PodcastSubscriptionCycleTypePrice_UpdatedAt
ON PodcastSubscriptionCycleTypePrice
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastSubscriptionCycleTypePrice
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastSubscriptionCycleTypePrice p
    INNER JOIN inserted i ON p.podcastSubscriptionId = i.podcastSubscriptionId 
        AND p.subscriptionCycleTypeId = i.subscriptionCycleTypeId 
        AND p.version = i.version;
END;
GO

-- Trigger for PodcastSubscriptionBenefitMapping table
CREATE TRIGGER TR_PodcastSubscriptionBenefitMapping_UpdatedAt
ON PodcastSubscriptionBenefitMapping
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastSubscriptionBenefitMapping
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastSubscriptionBenefitMapping p
    INNER JOIN inserted i ON p.podcastSubscriptionId = i.podcastSubscriptionId 
        AND p.podcastSubscriptionBenefitId = i.podcastSubscriptionBenefitId 
        AND p.version = i.version;
END;
GO

-- Trigger for PodcastSubscriptionRegistration table
CREATE TRIGGER TR_PodcastSubscriptionRegistration_UpdatedAt
ON PodcastSubscriptionRegistration
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PodcastSubscriptionRegistration
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM PodcastSubscriptionRegistration p
    INNER JOIN inserted i ON p.accountId = i.accountId;
END;
GO

-- Trigger for MemberSubscription table
CREATE TRIGGER TR_MemberSubscription_UpdatedAt
ON MemberSubscription
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE MemberSubscription
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM MemberSubscription m
    INNER JOIN inserted i ON m.id = i.id;
END;
GO

-- Trigger for MemberSubscriptionCycleTypePrice table
CREATE TRIGGER TR_MemberSubscriptionCycleTypePrice_UpdatedAt
ON MemberSubscriptionCycleTypePrice
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE MemberSubscriptionCycleTypePrice
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM MemberSubscriptionCycleTypePrice m
    INNER JOIN inserted i ON m.memberSubscriptionId = i.memberSubscriptionId 
        AND m.subscriptionCycleTypeId = i.subscriptionCycleTypeId 
        AND m.version = i.version;
END;
GO

-- Trigger for MemberSubscriptionBenefitMapping table
CREATE TRIGGER TR_MemberSubscriptionBenefitMapping_UpdatedAt
ON MemberSubscriptionBenefitMapping
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE MemberSubscriptionBenefitMapping
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM MemberSubscriptionBenefitMapping m
    INNER JOIN inserted i ON m.memberSubscriptionId = i.memberSubscriptionId 
        AND m.memberSubscriptionBenefitId = i.memberSubscriptionBenefitId 
        AND m.version = i.version;
END;
GO

-- Trigger for MemberSubscriptionRegistration table
CREATE TRIGGER TR_MemberSubscriptionRegistration_UpdatedAt
ON MemberSubscriptionRegistration
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE MemberSubscriptionRegistration
    SET updatedAt = CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)
    FROM MemberSubscriptionRegistration m
    INNER JOIN inserted i ON m.accountId = i.accountId;
END;
GO

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
    INNER JOIN inserted i ON s.sagaId = i.sagaId;
END;
GO