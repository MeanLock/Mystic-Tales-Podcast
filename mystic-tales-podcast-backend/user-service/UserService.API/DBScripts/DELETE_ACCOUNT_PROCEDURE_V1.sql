-- =====================================================
-- DELETE ACCOUNT DATA - USER SERVICE DATABASE [Port: 8046]
-- =====================================================
-- Usage: EXEC DeleteAccountFromUserService @accountId = <account_id>
-- =====================================================

CREATE OR ALTER PROCEDURE DeleteAccountFromUserService
    @accountId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Delete AccountNotification
        DELETE FROM AccountNotification 
        WHERE accountId = @accountId;
        
        -- Delete PodcastBuddyReview (as reviewer)
        DELETE FROM PodcastBuddyReview 
        WHERE accountId = @accountId;
        
        -- Delete PodcastBuddyReview (as buddy being reviewed)
        DELETE FROM PodcastBuddyReview 
        WHERE podcastBuddyId = @accountId;
        
        -- Delete AccountSavedPodcastEpisode
        DELETE FROM AccountSavedPodcastEpisode 
        WHERE accountId = @accountId;
        
        -- Delete AccountFollowedPodcastShow
        DELETE FROM AccountFollowedPodcastShow 
        WHERE accountId = @accountId;
        
        -- Delete AccountFavoritedPodcastChannel
        DELETE FROM AccountFavoritedPodcastChannel 
        WHERE accountId = @accountId;
        
        -- Delete AccountFollowedPodcaster (as follower)
        DELETE FROM AccountFollowedPodcaster 
        WHERE accountId = @accountId;
        
        -- Delete AccountFollowedPodcaster (as podcaster being followed)
        DELETE FROM AccountFollowedPodcaster 
        WHERE podcasterId = @accountId;
        
        -- Delete PodcasterProfile
        DELETE FROM PodcasterProfile 
        WHERE accountId = @accountId;
        
        -- Delete PasswordResetToken
        DELETE FROM PasswordResetToken 
        WHERE accountId = @accountId;
        
        -- Delete Account (main table)
        DELETE FROM Account 
        WHERE id = @accountId;
        
        COMMIT TRANSACTION;
        
        PRINT 'Account ' + CAST(@accountId AS NVARCHAR(10)) + ' successfully deleted from User Service.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- Example usage:
-- EXEC DeleteAccountFromUserService @accountId = 123;


-- =====================================================
-- DELETE ACCOUNT DATA - BOOKING MANAGEMENT SERVICE DATABASE [Port: 8056]
-- =====================================================
-- Usage: EXEC DeleteAccountFromBookingService @accountId = <account_id>
-- =====================================================

CREATE OR ALTER PROCEDURE DeleteAccountFromBookingService
    @accountId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Delete BookingPodcastTrackListenSession
        DELETE FROM BookingPodcastTrackListenSession
        WHERE accountId = @accountId;
        
        -- Delete BookingChatMessages (as sender)
        DELETE FROM BookingChatMessages
        WHERE senderId = @accountId;
        
        -- Delete BookingChatMember
        DELETE FROM BookingChatMember
        WHERE accountId = @accountId;
        
        -- Delete BookingChatRoom for bookings owned by this account
        DELETE bcr
        FROM BookingChatRoom bcr
        INNER JOIN Booking b ON bcr.bookingId = b.id
        WHERE b.accountId = @accountId OR b.podcastBuddyId = @accountId;
        
        -- Delete BookingProducingRequestPodcastTrackToEdit for bookings owned by this account
        DELETE bprptte
        FROM BookingProducingRequestPodcastTrackToEdit bprptte
        INNER JOIN BookingProducingRequest bpr ON bprptte.bookingProducingRequestId = bpr.id
        INNER JOIN Booking b ON bpr.bookingId = b.id
        WHERE b.accountId = @accountId OR b.podcastBuddyId = @accountId;
        
        -- Delete BookingPodcastTrack for bookings owned by this account
        DELETE bpt
        FROM BookingPodcastTrack bpt
        INNER JOIN Booking b ON bpt.bookingId = b.id
        WHERE b.accountId = @accountId OR b.podcastBuddyId = @accountId;
        
        -- Delete BookingProducingRequest for bookings owned by this account
        DELETE bpr
        FROM BookingProducingRequest bpr
        INNER JOIN Booking b ON bpr.bookingId = b.id
        WHERE b.accountId = @accountId OR b.podcastBuddyId = @accountId;
        
        -- Delete BookingStatusTracking for bookings owned by this account
        DELETE bst
        FROM BookingStatusTracking bst
        INNER JOIN Booking b ON bst.bookingId = b.id
        WHERE b.accountId = @accountId OR b.podcastBuddyId = @accountId;
        
        -- Delete BookingRequirement for bookings owned by this account
        DELETE br
        FROM BookingRequirement br
        INNER JOIN Booking b ON br.bookingId = b.id
        WHERE b.accountId = @accountId OR b.podcastBuddyId = @accountId;
        
        -- Delete PodcastBuddyBookingTone (as podcaster)
        DELETE FROM PodcastBuddyBookingTone
        WHERE podcasterId = @accountId;
        
        -- Delete Booking (as customer)
        DELETE FROM Booking
        WHERE accountId = @accountId;
        
        -- Delete Booking (as podcast buddy)
        DELETE FROM Booking
        WHERE podcastBuddyId = @accountId;
        
        -- Delete Booking (as assigned staff)
        DELETE FROM Booking
        WHERE assignedStaffId = @accountId;
        
        COMMIT TRANSACTION;
        
        PRINT 'Account ' + CAST(@accountId AS NVARCHAR(10)) + ' successfully deleted from Booking Management Service.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- Example usage:
-- EXEC DeleteAccountFromBookingService @accountId = 123;

-- =====================================================
-- DELETE ACCOUNT DATA - PODCAST SERVICE DATABASE [Port: 8061]
-- =====================================================
-- Usage: EXEC DeleteAccountFromPodcastService @accountId = <account_id>
-- =====================================================

CREATE OR ALTER PROCEDURE DeleteAccountFromPodcastService
    @accountId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Delete PodcastEpisodeListenSessionHlsEnckeyRequestToken
        DELETE pelsert
        FROM PodcastEpisodeListenSessionHlsEnckeyRequestToken pelsert
        INNER JOIN PodcastEpisodeListenSession pels ON pelsert.podcastEpisodeListenSessionId = pels.id
        WHERE pels.accountId = @accountId;
        
        -- Delete PodcastEpisodeListenSession
        DELETE FROM PodcastEpisodeListenSession
        WHERE accountId = @accountId;
        
        -- Delete PodcastShowReview
        DELETE FROM PodcastShowReview
        WHERE accountId = @accountId;
        
        -- Delete PodcastEpisodePublishDuplicateDetection for episodes published by this account
        DELETE pepdd
        FROM PodcastEpisodePublishDuplicateDetection pepdd
        INNER JOIN PodcastEpisodePublishReviewSession peprs ON pepdd.podcastEpisodePublishReviewSessionId = peprs.id
        INNER JOIN PodcastEpisode pe ON peprs.podcastEpisodeId = pe.id
        INNER JOIN PodcastShow ps ON pe.podcastShowId = ps.id
        WHERE ps.podcasterId = @accountId;
        
        -- Delete PodcastEpisodePublishReviewSessionStatusTracking for episodes published by this account
        DELETE peprsst
        FROM PodcastEpisodePublishReviewSessionStatusTracking peprsst
        INNER JOIN PodcastEpisodePublishReviewSession peprs ON peprsst.podcastEpisodePublishReviewSessionId = peprs.id
        INNER JOIN PodcastEpisode pe ON peprs.podcastEpisodeId = pe.id
        INNER JOIN PodcastShow ps ON pe.podcastShowId = ps.id
        WHERE ps.podcasterId = @accountId;
        
        -- Delete PodcastEpisodePublishReviewSession for episodes published by this account
        DELETE peprs
        FROM PodcastEpisodePublishReviewSession peprs
        INNER JOIN PodcastEpisode pe ON peprs.podcastEpisodeId = pe.id
        INNER JOIN PodcastShow ps ON pe.podcastShowId = ps.id
        WHERE ps.podcasterId = @accountId;
        
        -- Delete PodcastEpisodePublishReviewSession as assigned staff
        DELETE FROM PodcastEpisodePublishReviewSession
        WHERE assignedStaff = @accountId;
        
        -- Delete PodcastEpisodeIllegalContentTypeMarking for episodes owned by this account
        DELETE peictm
        FROM PodcastEpisodeIllegalContentTypeMarking peictm
        INNER JOIN PodcastEpisode pe ON peictm.podcastEpisodeId = pe.id
        INNER JOIN PodcastShow ps ON pe.podcastShowId = ps.id
        WHERE ps.podcasterId = @accountId OR peictm.markerId = @accountId;
        
        -- Delete PodcastEpisodeLicense for episodes owned by this account
        DELETE pel
        FROM PodcastEpisodeLicense pel
        INNER JOIN PodcastEpisode pe ON pel.podcastEpisodeId = pe.id
        INNER JOIN PodcastShow ps ON pe.podcastShowId = ps.id
        WHERE ps.podcasterId = @accountId;
        
        -- Delete PodcastEpisodeHashtag for episodes owned by this account
        DELETE peh
        FROM PodcastEpisodeHashtag peh
        INNER JOIN PodcastEpisode pe ON peh.podcastEpisodeId = pe.id
        INNER JOIN PodcastShow ps ON pe.podcastShowId = ps.id
        WHERE ps.podcasterId = @accountId;
        
        -- Delete PodcastEpisodeStatusTracking for episodes owned by this account
        DELETE pest
        FROM PodcastEpisodeStatusTracking pest
        INNER JOIN PodcastEpisode pe ON pest.podcastEpisodeId = pe.id
        INNER JOIN PodcastShow ps ON pe.podcastShowId = ps.id
        WHERE ps.podcasterId = @accountId;
        
        -- Delete PodcastEpisode for shows owned by this account
        DELETE pe
        FROM PodcastEpisode pe
        INNER JOIN PodcastShow ps ON pe.podcastShowId = ps.id
        WHERE ps.podcasterId = @accountId;
        
        -- Delete PodcastShowHashtag for shows owned by this account
        DELETE psh
        FROM PodcastShowHashtag psh
        INNER JOIN PodcastShow ps ON psh.podcastShowId = ps.id
        WHERE ps.podcasterId = @accountId;
        
        -- Delete PodcastShowStatusTracking for shows owned by this account
        DELETE psst
        FROM PodcastShowStatusTracking psst
        INNER JOIN PodcastShow ps ON psst.podcastShowId = ps.id
        WHERE ps.podcasterId = @accountId;
        
        -- Delete PodcastShow
        DELETE FROM PodcastShow
        WHERE podcasterId = @accountId;
        
        -- Delete PodcastChannelHashtag for channels owned by this account
        DELETE pch
        FROM PodcastChannelHashtag pch
        INNER JOIN PodcastChannel pc ON pch.podcastChannelId = pc.id
        WHERE pc.podcasterId = @accountId;
        
        -- Delete PodcastChannelStatusTracking for channels owned by this account
        DELETE pcst
        FROM PodcastChannelStatusTracking pcst
        INNER JOIN PodcastChannel pc ON pcst.podcastChannelId = pc.id
        WHERE pc.podcasterId = @accountId;
        
        -- Delete PodcastChannel
        DELETE FROM PodcastChannel
        WHERE podcasterId = @accountId;
        
        COMMIT TRANSACTION;
        
        PRINT 'Account ' + CAST(@accountId AS NVARCHAR(10)) + ' successfully deleted from Podcast Service.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- Example usage:
-- EXEC DeleteAccountFromPodcastService @accountId = 123;

-- =====================================================
-- DELETE ACCOUNT DATA - SUBSCRIPTION SERVICE DATABASE [Port: 8066]
-- =====================================================
-- Usage: EXEC DeleteAccountFromSubscriptionService @accountId = <account_id>
-- =====================================================

CREATE OR ALTER PROCEDURE DeleteAccountFromSubscriptionService
    @accountId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Delete PodcastSubscriptionRegistration
        DELETE FROM PodcastSubscriptionRegistration
        WHERE accountId = @accountId;
        
        -- Delete MemberSubscriptionRegistration
        DELETE FROM MemberSubscriptionRegistration
        WHERE accountId = @accountId;
        
        COMMIT TRANSACTION;
        
        PRINT 'Account ' + CAST(@accountId AS NVARCHAR(10)) + ' successfully deleted from Subscription Service.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- Example usage:
-- EXEC DeleteAccountFromSubscriptionService @accountId = 123;


-- =====================================================
-- DELETE ACCOUNT DATA - MODERATION SERVICE DATABASE [Port: 8071]
-- =====================================================
-- Usage: EXEC DeleteAccountFromModerationService @accountId = <account_id>
-- =====================================================

CREATE OR ALTER PROCEDURE DeleteAccountFromModerationService
    @accountId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Delete PodcastBuddyReport (as reporter)
        DELETE FROM PodcastBuddyReport
        WHERE accountId = @accountId;
        
        -- Delete PodcastBuddyReport (as reported buddy)
        DELETE FROM PodcastBuddyReport
        WHERE podcastBuddyId = @accountId;
        
        -- Delete PodcastShowReport
        DELETE FROM PodcastShowReport
        WHERE accountId = @accountId;
        
        -- Delete PodcastEpisodeReport
        DELETE FROM PodcastEpisodeReport
        WHERE accountId = @accountId;
        
        -- Delete PodcastBuddyReportReviewSession (as reviewed buddy)
        DELETE FROM PodcastBuddyReportReviewSession
        WHERE podcastBuddyId = @accountId;
        
        -- Delete PodcastBuddyReportReviewSession (as assigned staff)
        DELETE FROM PodcastBuddyReportReviewSession
        WHERE assignedStaff = @accountId;
        
        -- Delete PodcastShowReportReviewSession (as assigned staff)
        DELETE FROM PodcastShowReportReviewSession
        WHERE assignedStaff = @accountId;
        
        -- Delete PodcastEpisodeReportReviewSession (as assigned staff)
        DELETE FROM PodcastEpisodeReportReviewSession
        WHERE assignedStaff = @accountId;
        
        -- Delete CounterNotice (as validator)
        UPDATE CounterNotice
        SET validatedBy = NULL
        WHERE validatedBy = @accountId;
        
        -- Delete DMCANotice (as validator)
        UPDATE DMCANotice
        SET validatedBy = NULL
        WHERE validatedBy = @accountId;
        
        -- Delete LawsuitProof (as validator)
        UPDATE LawsuitProof
        SET validatedBy = NULL
        WHERE validatedBy = @accountId;
        
        -- Delete DMCAAccusation (as assigned staff)
        UPDATE DMCAAccusation
        SET assignedStaff = NULL
        WHERE assignedStaff = @accountId;
        
        COMMIT TRANSACTION;
        
        PRINT 'Account ' + CAST(@accountId AS NVARCHAR(10)) + ' successfully deleted from Moderation Service.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- Example usage:
-- EXEC DeleteAccountFromModerationService @accountId = 123;

-- =====================================================
-- DELETE ACCOUNT DATA - TRANSACTION SERVICE DATABASE [Port: 8076]
-- =====================================================
-- Usage: EXEC DeleteAccountFromTransactionService @accountId = <account_id>
-- =====================================================

CREATE OR ALTER PROCEDURE DeleteAccountFromTransactionService
    @accountId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Delete AccountBalanceTransaction
        DELETE FROM AccountBalanceTransaction
        WHERE accountId = @accountId;
        
        -- Delete AccountBalanceWithdrawalRequest
        DELETE FROM AccountBalanceWithdrawalRequest
        WHERE accountId = @accountId;
        
        -- Delete PodcastSubscriptionTransaction for subscriptions owned by this account
        DELETE pst
        FROM PodcastSubscriptionTransaction pst
        INNER JOIN PodcastSubscriptionRegistration psr ON pst.podcastSubscriptionRegistrationId = psr.id
        WHERE psr.accountId = @accountId;
        
        -- Delete MemberSubscriptionTransaction for subscriptions owned by this account
        DELETE mst
        FROM MemberSubscriptionTransaction mst
        INNER JOIN MemberSubscriptionRegistration msr ON mst.memberSubscriptionRegistrationId = msr.id
        WHERE msr.accountId = @accountId;
        
        -- Delete BookingTransaction for bookings owned by this account
        -- Note: This assumes bookings are identified by accountId in a related service
        -- You may need to pass booking IDs if they're not directly linked to accountId
        DELETE FROM BookingTransaction
        WHERE bookingId IN (
            -- This subquery would need to be adjusted based on how you identify 
            -- bookings belonging to this account in the Transaction Service
            -- If bookingId is stored directly, you might need to handle this differently
            SELECT id FROM Booking WHERE accountId = @accountId OR podcastBuddyId = @accountId
        );
        
        -- Delete BookingStorageTransaction
        DELETE FROM BookingStorageTransaction
        WHERE accountId = @accountId;
        
        COMMIT TRANSACTION;
        
        PRINT 'Account ' + CAST(@accountId AS NVARCHAR(10)) + ' successfully deleted from Transaction Service.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- Alternative version if Booking table is not accessible from Transaction Service:
-- In this case, you would need to pass the booking IDs as a parameter

CREATE OR ALTER PROCEDURE DeleteAccountFromTransactionServiceWithBookingIds
    @accountId INT,
    @bookingIds NVARCHAR(MAX) -- Comma-separated list of booking IDs
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Create temp table for booking IDs
        CREATE TABLE #BookingIds (BookingId INT);
        
        -- Parse comma-separated booking IDs if provided
        IF @bookingIds IS NOT NULL AND @bookingIds != ''
        BEGIN
            INSERT INTO #BookingIds (BookingId)
            SELECT CAST(value AS INT)
            FROM STRING_SPLIT(@bookingIds, ',');
        END
        
        -- Delete AccountBalanceTransaction
        DELETE FROM AccountBalanceTransaction
        WHERE accountId = @accountId;
        
        -- Delete AccountBalanceWithdrawalRequest
        DELETE FROM AccountBalanceWithdrawalRequest
        WHERE accountId = @accountId;
        
        -- Delete PodcastSubscriptionTransaction for subscriptions owned by this account
        DELETE pst
        FROM PodcastSubscriptionTransaction pst
        INNER JOIN PodcastSubscriptionRegistration psr ON pst.podcastSubscriptionRegistrationId = psr.id
        WHERE psr.accountId = @accountId;
        
        -- Delete MemberSubscriptionTransaction for subscriptions owned by this account
        DELETE mst
        FROM MemberSubscriptionTransaction mst
        INNER JOIN MemberSubscriptionRegistration msr ON mst.memberSubscriptionRegistrationId = msr.id
        WHERE msr.accountId = @accountId;
        
        -- Delete BookingTransaction using provided booking IDs
        DELETE FROM BookingTransaction
        WHERE bookingId IN (SELECT BookingId FROM #BookingIds);
        
        -- Delete BookingStorageTransaction
        DELETE FROM BookingStorageTransaction
        WHERE accountId = @accountId;
        
        DROP TABLE #BookingIds;
        
        COMMIT TRANSACTION;
        
        PRINT 'Account ' + CAST(@accountId AS NVARCHAR(10)) + ' successfully deleted from Transaction Service.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        IF OBJECT_ID('tempdb..#BookingIds') IS NOT NULL
            DROP TABLE #BookingIds;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- Example usage:
-- EXEC DeleteAccountFromTransactionService @accountId = 123;
-- OR
-- EXEC DeleteAccountFromTransactionServiceWithBookingIds @accountId = 123, @bookingIds = '1,2,3,4,5';