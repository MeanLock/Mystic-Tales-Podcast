-- =====================================================
-- DEMO DATA INSERT SCRIPT
-- =====================================================
-- Note: 
-- - All file keys that allow NULL will be NULL
-- - All file keys that are NOT NULL will have dummy paths (files don't exist)
-- - All foreign keys across services must exist and be synchronized
-- - All accounts are verified, no violations, not deactivated
-- - Episodes are all in Draft status (no audio files yet)
-- - Bookings don't have tracks yet (only producing requests)
-- =====================================================

-- =====================================================
-- USER SERVICE DATABASE [Port: 8046]
-- =====================================================

-- Account table
-- Password for all: $2a$10$get4qcrEk0Cny7NLKiXXsu6JIw84lZiF3NpK7KxNKc6JyxV6w0Oo.
-- 1 Admin + 2 Staff + 2 Podcaster + 6 Customer = 11 accounts
SET IDENTITY_INSERT Account ON;

INSERT INTO Account (id, email, password, roleId, fullName, dob, gender, address, phone, balance, mainImageFileKey, isVerified, googleId, verifyCode, podcastListenSlot, violationPoint, violationLevel, lastViolationPointChanged, lastViolationLevelChanged, lastPodcastListenSlotChanged, deactivatedAt, createdAt, updatedAt) VALUES
-- Admin
(1, 'admin@podcastplatform.com', '$2a$10$get4qcrEk0Cny7NLKiXXsu6JIw84lZiF3NpK7KxNKc6JyxV6w0Oo.', 3, N'Nguyễn Văn Admin', '1985-01-15', N'Male', N'123 Admin Street, District 1, HCMC', '0901234567', 0, NULL, 1, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, '2024-01-01 10:00:00', '2024-01-01 10:00:00'),

-- Staff
(2, 'staff1@podcastplatform.com', '$2a$10$get4qcrEk0Cny7NLKiXXsu6JIw84lZiF3NpK7KxNKc6JyxV6w0Oo.', 2, N'Trần Thị Lan', '1990-03-20', N'Female', N'456 Staff Road, District 3, HCMC', '0902345678', 0, NULL, 1, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, '2024-01-15 09:00:00', '2024-01-15 09:00:00'),
(3, 'staff2@podcastplatform.com', '$2a$10$get4qcrEk0Cny7NLKiXXsu6JIw84lZiF3NpK7KxNKc6JyxV6w0Oo.', 2, N'Lê Văn Minh', '1992-07-10', N'Male', N'789 Staff Avenue, District 5, HCMC', '0903456789', 0, NULL, 1, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, '2024-01-20 11:00:00', '2024-01-20 11:00:00'),

-- Podcasters (will have PodcasterProfile)
(4, 'podcaster1@example.com', '$2a$10$get4qcrEk0Cny7NLKiXXsu6JIw84lZiF3NpK7KxNKc6JyxV6w0Oo.', 1, N'Phạm Minh Tuấn', '1988-05-12', N'Male', N'111 Podcaster Lane, District 2, HCMC', '0904567890', 5000000, NULL, 1, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, '2024-02-01 08:00:00', '2024-02-01 08:00:00'),
(5, 'podcaster2@example.com', '$2a$10$get4qcrEk0Cny7NLKiXXsu6JIw84lZiF3NpK7KxNKc6JyxV6w0Oo.', 1, N'Hoàng Thị Mai', '1991-09-25', N'Female', N'222 Creator Street, District 7, HCMC', '0905678901', 3500000, NULL, 1, NULL, NULL, NULL, 0, 0, NULL, NULL, NULL, NULL, '2024-02-05 10:00:00', '2024-02-05 10:00:00'),

-- Customers
(6, 'customer1@example.com', '$2a$10$get4qcrEk0Cny7NLKiXXsu6JIw84lZiF3NpK7KxNKc6JyxV6w0Oo.', 1, N'Nguyễn Thị Hương', '1995-02-14', N'Female', N'333 Customer Road, District 1, HCMC', '0906789012', 500000, NULL, 1, NULL, NULL, 9, 0, 0, NULL, NULL, NULL, NULL, '2024-03-01 09:00:00', '2024-03-01 09:00:00'),
(7, 'customer2@example.com', '$2a$10$get4qcrEk0Cny7NLKiXXsu6JIw84lZiF3NpK7KxNKc6JyxV6w0Oo.', 1, N'Trần Văn Đức', '1993-11-30', N'Male', N'444 Listener Avenue, District 3, HCMC', '0907890123', 1200000, NULL, 1, NULL, NULL, 9, 0, 0, NULL, NULL, NULL, NULL, '2024-03-05 14:00:00', '2024-03-05 14:00:00'),
(8, 'customer3@example.com', '$2a$10$get4qcrEk0Cny7NLKiXXsu6JIw84lZiF3NpK7KxNKc6JyxV6w0Oo.', 1, N'Lê Thị Phương', '1996-06-18', N'Female', N'555 Fan Street, District 5, HCMC', '0908901234', 800000, NULL, 1, NULL, NULL, 9, 0, 0, NULL, NULL, NULL, NULL, '2024-03-10 11:00:00', '2024-03-10 11:00:00'),
(9, 'customer4@example.com', '$2a$10$get4qcrEk0Cny7NLKiXXsu6JIw84lZiF3NpK7KxNKc6JyxV6w0Oo.', 1, N'Vũ Minh Anh', '1994-04-22', N'Male', N'666 User Lane, District 10, HCMC', '0909012345', 300000, NULL, 1, NULL, NULL, 9, 0, 0, NULL, NULL, NULL, NULL, '2024-03-15 16:00:00', '2024-03-15 16:00:00'),
(10, 'customer5@example.com', '$2a$10$get4qcrEk0Cny7NLKiXXsu6JIw84lZiF3NpK7KxNKc6JyxV6w0Oo.', 1, N'Đỗ Thị Thanh', '1997-08-05', N'Female', N'777 Member Road, Binh Thanh, HCMC', '0900123456', 1500000, NULL, 1, NULL, NULL, 9, 0, 0, NULL, NULL, NULL, NULL, '2024-03-20 13:00:00', '2024-03-20 13:00:00'),
(11, 'customer6@example.com', '$2a$10$get4qcrEk0Cny7NLKiXXsu6JIw84lZiF3NpK7KxNKc6JyxV6w0Oo.', 1, N'Bùi Văn Hải', '1992-12-01', N'Male', N'888 Audience Street, Thu Duc, HCMC', '0901234568', 2000000, NULL, 1, NULL, NULL, 9, 0, 0, NULL, NULL, NULL, NULL, '2024-03-25 10:00:00', '2024-03-25 10:00:00');

SET IDENTITY_INSERT Account OFF;

-- PodcasterProfile table
-- Podcaster 4 (id=4): Verified Podcast Buddy
-- Podcaster 5 (id=5): Verified Podcaster (not buddy)
INSERT INTO PodcasterProfile (accountId, name, description, averageRating, ratingCount, totalFollow, listenCount, commitmentDocumentFileKey, buddyAudioFileKey, ownedBookingStorageSize, usedBookingStorageSize, isVerified, pricePerBookingWord, verifiedAt, isBuddy, createdAt, updatedAt) VALUES
(4, N'True Crime Stories VN', N'Chuyên về các vụ án hình sự có thật tại Việt Nam. Phân tích sâu, kể chuyện hấp dẫn.', 4.5, 12, 150, 2500, 'commitment_docs/podcaster_4.pdf', NULL, 5.0, 2.3, 1, 150.00, '2024-02-15 10:00:00', 1, '2024-02-01 08:00:00', '2024-12-01 15:00:00'),
(5, N'Mysterious Tales', N'Những câu chuyện bí ẩn, siêu nhiên và kinh dị từ khắp nơi trên thế giới.', 4.2, 8, 95, 1800, 'commitment_docs/podcaster_5.pdf', NULL, 3.0, 1.5, 1, 120.00, '2024-02-20 14:00:00', 0, '2024-02-05 10:00:00', '2024-11-28 09:00:00');


-- =====================================================
-- PODCAST SERVICE DATABASE [Port: 8056]
-- =====================================================

-- PodcastChannel table
-- Channel 1: From Podcaster 4 (Published)
-- Channel 2: From Podcaster 5 (Published)
-- Channel 3: From Podcaster 4 (Unpublished)
INSERT INTO PodcastChannel (id, accountId, name, description, mainImageFileKey, coverImageFileKey, createdAt, updatedAt) VALUES
(NEWID(), 4, N'True Crime Vietnam', N'Kênh podcast chuyên về các vụ án hình sự có thật tại Việt Nam', NULL, NULL, '2024-02-05 10:00:00', '2024-02-05 10:00:00'),
(NEWID(), 5, N'Mystery World', N'Khám phá những bí ẩn chưa được giải đáp', NULL, NULL, '2024-02-10 11:00:00', '2024-02-10 11:00:00'),
(NEWID(), 4, N'Dark History Channel', N'Lịch sử đen tối và những sự kiện bi thảm', NULL, NULL, '2024-03-01 09:00:00', '2024-03-01 09:00:00');

-- Get Channel IDs for reference (stored as comments)
-- Channel 1 (True Crime Vietnam): Let's assume ID = '11111111-1111-1111-1111-111111111111'
-- Channel 2 (Mystery World): Let's assume ID = '22222222-2222-2222-2222-222222222222'
-- Channel 3 (Dark History): Let's assume ID = '33333333-3333-3333-3333-333333333333'

-- PodcastChannelStatusTracking table
-- Note: Need to use actual NEWID() generated above. For demo, using placeholder GUIDs
DECLARE @Channel1Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastChannel WHERE name = N'True Crime Vietnam');
DECLARE @Channel2Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastChannel WHERE name = N'Mystery World');
DECLARE @Channel3Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastChannel WHERE name = N'Dark History Channel');

INSERT INTO PodcastChannelStatusTracking (id, podcastChannelId, podcastChannelStatusId, createdAt) VALUES
(NEWID(), @Channel1Id, 2, '2024-02-06 10:00:00'), -- Published
(NEWID(), @Channel2Id, 2, '2024-02-11 11:00:00'), -- Published
(NEWID(), @Channel3Id, 1, '2024-03-01 09:00:00'); -- Unpublished

-- PodcastShow table
-- Show 1: Channel 1, Published
-- Show 2: Channel 1, Published
-- Show 3: Channel 2, Published
-- Show 4: Channel 2, Draft
-- Show 5: Channel 3, Draft
INSERT INTO PodcastShow (id, accountId, podcastChannelId, name, description, mainImageFileKey, coverImageFileKey, podcastShowSubscriptionTypeId, categoryId, totalFollow, createdAt, updatedAt) VALUES
(NEWID(), 4, @Channel1Id, N'Vụ Án Bí Ẩn', N'Series phân tích các vụ án hình sự bí ẩn chưa được giải quyết', NULL, NULL, 1, 1, 45, '2024-02-07 10:00:00', '2024-02-07 10:00:00'),
(NEWID(), 4, @Channel1Id, N'Serial Killers Vietnam', N'Những tên sát nhân hàng loạt tại Việt Nam', NULL, NULL, 2, 1, 32, '2024-02-15 14:00:00', '2024-02-15 14:00:00'),
(NEWID(), 5, @Channel2Id, N'Haunted Places', N'Những địa điểm ma ám nổi tiếng', NULL, NULL, 1, 2, 28, '2024-02-12 11:00:00', '2024-02-12 11:00:00'),
(NEWID(), 5, @Channel2Id, N'Urban Legends Asia', N'Truyền thuyết đô thị châu Á', NULL, NULL, 1, 2, 0, '2024-03-05 15:00:00', '2024-03-05 15:00:00'),
(NEWID(), 4, @Channel3Id, N'War Crimes History', N'Tội ác chiến tranh trong lịch sử', NULL, NULL, 1, 6, 0, '2024-03-02 10:00:00', '2024-03-02 10:00:00');

-- Get Show IDs for reference
DECLARE @Show1Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastShow WHERE name = N'Vụ Án Bí Ẩn');
DECLARE @Show2Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastShow WHERE name = N'Serial Killers Vietnam');
DECLARE @Show3Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastShow WHERE name = N'Haunted Places');
DECLARE @Show4Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastShow WHERE name = N'Urban Legends Asia');
DECLARE @Show5Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastShow WHERE name = N'War Crimes History');

-- PodcastShowStatusTracking table
INSERT INTO PodcastShowStatusTracking (id, podcastShowId, podcastShowStatusId, createdAt) VALUES
(NEWID(), @Show1Id, 3, '2024-02-08 10:00:00'), -- Published
(NEWID(), @Show2Id, 3, '2024-02-16 14:00:00'), -- Published
(NEWID(), @Show3Id, 3, '2024-02-13 11:00:00'), -- Published
(NEWID(), @Show4Id, 1, '2024-03-05 15:00:00'), -- Draft
(NEWID(), @Show5Id, 1, '2024-03-02 10:00:00'); -- Draft

-- PodcastEpisode table
-- ALL EPISODES ARE IN DRAFT STATUS (status = 1)
-- audioFileKey = NULL for all episodes
INSERT INTO PodcastEpisode (id, name, description, explicitContent, releaseDate, isReleased, mainImageFileKey, audioFileKey, audioFileSize, audioLength, audioFingerPrint, audioTranscript, audioEncryptionKeyId, audioEncryptionKeyFileKey, podcastEpisodeSubscriptionTypeId, podcastShowId, seasonNumber, episodeOrder, totalSave, listenCount, isAudioPublishable, takenDownReason, deletedAt, createdAt, updatedAt) VALUES
-- Show 1 episodes (Draft)
(NEWID(), N'Vụ án mất tích bí ẩn năm 1995', N'Phân tích vụ mất tích chưa được giải quyết', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, @Show1Id, 1, 1, 0, 0, NULL, NULL, NULL, '2024-02-20 10:00:00', '2024-02-20 10:00:00'),
(NEWID(), N'Tội ác hoàn hảo?', N'Vụ án được cho là không có manh mối', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, @Show1Id, 1, 2, 0, 0, NULL, NULL, NULL, '2024-03-01 11:00:00', '2024-03-01 11:00:00'),
(NEWID(), N'Kẻ giết người hàng loạt Sài Gòn', N'Những vụ giết người liên tiếp chưa được phá án', 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, @Show1Id, 1, 3, 0, 0, NULL, NULL, NULL, '2024-03-10 14:00:00', '2024-03-10 14:00:00'),

-- Show 2 episodes (Draft)
(NEWID(), N'Sát nhân Zodiac Việt Nam', N'Phân tích tâm lý kẻ sát nhân', 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2, @Show2Id, 1, 1, 0, 0, NULL, NULL, NULL, '2024-02-25 09:00:00', '2024-02-25 09:00:00'),
(NEWID(), N'Dấu vết tội ác', N'Điều tra các bằng chứng tại hiện trường', 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 2, @Show2Id, 1, 2, 0, 0, NULL, NULL, NULL, '2024-03-05 10:00:00', '2024-03-05 10:00:00'),

-- Show 3 episodes (Draft)
(NEWID(), N'Ngôi nhà ma ám Đà Lạt', N'Câu chuyện về căn biệt thự bỏ hoang', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, @Show3Id, 1, 1, 0, 0, NULL, NULL, NULL, '2024-02-18 13:00:00', '2024-02-18 13:00:00'),
(NEWID(), N'Bệnh viện bỏ hoang', N'Những hiện tượng lạ tại bệnh viện cũ', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, @Show3Id, 1, 2, 0, 0, NULL, NULL, NULL, '2024-02-28 15:00:00', '2024-02-28 15:00:00'),
(NEWID(), N'Khách sạn ma Huế', N'Truyền thuyết về khách sạn bị nguyền rủa', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, @Show3Id, 1, 3, 0, 0, NULL, NULL, NULL, '2024-03-08 11:00:00', '2024-03-08 11:00:00'),

-- Show 4 episodes (Draft)
(NEWID(), N'Cô gái váy trắng', N'Truyền thuyết đô thị nổi tiếng nhất Việt Nam', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, @Show4Id, 1, 1, 0, 0, NULL, NULL, NULL, '2024-03-12 16:00:00', '2024-03-12 16:00:00'),
(NEWID(), N'Xe bus ma số 14', N'Chuyến xe buýt bí ẩn trong đêm', 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, @Show4Id, 1, 2, 0, 0, NULL, NULL, NULL, '2024-03-15 10:00:00', '2024-03-15 10:00:00');

-- Get some Episode IDs for reference
DECLARE @Episode1Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastEpisode WHERE name = N'Vụ án mất tích bí ẩn năm 1995');
DECLARE @Episode2Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastEpisode WHERE name = N'Tội ác hoàn hảo?');
DECLARE @Episode3Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastEpisode WHERE name = N'Ngôi nhà ma ám Đà Lạt');
DECLARE @Episode4Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastEpisode WHERE name = N'Sát nhân Zodiac Việt Nam');
DECLARE @Episode5Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastEpisode WHERE name = N'Cô gái váy trắng');

-- PodcastEpisodeStatusTracking table
-- All episodes in Draft status
INSERT INTO PodcastEpisodeStatusTracking (id, podcastEpisodeId, podcastEpisodeStatusId, createdAt)
SELECT NEWID(), id, 1, createdAt
FROM PodcastEpisode;

-- PodcastEpisodeLicense table
-- Each episode needs a license
INSERT INTO PodcastEpisodeLicense (id, podcastEpisodeId, licenseDocumentFileKey, podcastEpisodeLicenseTypeId, createdAt, updatedAt)
SELECT NEWID(), id, 'episode_licenses/episode_' + CAST(id AS NVARCHAR(50)) + '_license.pdf', 5, createdAt, updatedAt
FROM PodcastEpisode;

-- PodcastShowReview table
-- Reviews for published shows (Show 1, 2, 3)
INSERT INTO PodcastShowReview (id, title, content, rating, accountId, podcastShowId, deletedAt, createdAt, updatedAt) VALUES
(NEWID(), N'Rất hay và hấp dẫn!', N'Nội dung phân tích rất chi tiết, giọng đọc dễ nghe', 5.0, 6, @Show1Id, NULL, '2024-02-25 14:00:00', '2024-02-25 14:00:00'),
(NEWID(), N'Chất lượng tốt', N'Podcast rất chuyên nghiệp, đáng nghe', 4.5, 7, @Show1Id, NULL, '2024-03-02 10:00:00', '2024-03-02 10:00:00'),
(NEWID(), N'Hay nhưng hơi dài', N'Nội dung hay nhưng mỗi tập hơi dài', 4.0, 8, @Show1Id, NULL, '2024-03-08 16:00:00', '2024-03-08 16:00:00'),
(NEWID(), N'Rất đáng sợ!', N'Nghe xong không dám ngủ', 4.5, 6, @Show3Id, NULL, '2024-03-01 20:00:00', '2024-03-01 20:00:00'),
(NEWID(), N'Kinh dị thật sự', N'Những câu chuyện ma rất chân thực', 5.0, 9, @Show3Id, NULL, '2024-03-10 22:00:00', '2024-03-10 22:00:00'),
(NEWID(), N'Podcast chất lượng', N'Nội dung hay, kể chuyện hấp dẫn', 4.0, 10, @Show2Id, NULL, '2024-03-05 15:00:00', '2024-03-05 15:00:00');

-- Hashtag table
INSERT INTO Hashtag (id, name, createdAt) VALUES
(NEWID(), N'#truecrime', '2024-02-01 10:00:00'),
(NEWID(), N'#mystery', '2024-02-01 10:00:00'),
(NEWID(), N'#horror', '2024-02-01 10:00:00'),
(NEWID(), N'#haunted', '2024-02-01 10:00:00'),
(NEWID(), N'#serialkiller', '2024-02-01 10:00:00'),
(NEWID(), N'#unsolved', '2024-02-01 10:00:00'),
(NEWID(), N'#darkhistory', '2024-02-01 10:00:00'),
(NEWID(), N'#urbanlegend', '2024-02-01 10:00:00'),
(NEWID(), N'#supernatural', '2024-02-01 10:00:00'),
(NEWID(), N'#investigation', '2024-02-01 10:00:00'),
(NEWID(), N'#crime', '2024-02-01 10:00:00'),
(NEWID(), N'#scary', '2024-02-01 10:00:00'),
(NEWID(), N'#vietnam', '2024-02-01 10:00:00'),
(NEWID(), N'#ghost', '2024-02-01 10:00:00'),
(NEWID(), N'#coldcase', '2024-02-01 10:00:00');

-- Get Hashtag IDs
DECLARE @HashtagTrueCrime UNIQUEIDENTIFIER = (SELECT id FROM Hashtag WHERE name = N'#truecrime');
DECLARE @HashtagMystery UNIQUEIDENTIFIER = (SELECT id FROM Hashtag WHERE name = N'#mystery');
DECLARE @HashtagHorror UNIQUEIDENTIFIER = (SELECT id FROM Hashtag WHERE name = N'#horror');
DECLARE @HashtagHaunted UNIQUEIDENTIFIER = (SELECT id FROM Hashtag WHERE name = N'#haunted');
DECLARE @HashtagSerialKiller UNIQUEIDENTIFIER = (SELECT id FROM Hashtag WHERE name = N'#serialkiller');
DECLARE @HashtagUnsolved UNIQUEIDENTIFIER = (SELECT id FROM Hashtag WHERE name = N'#unsolved');
DECLARE @HashtagDarkHistory UNIQUEIDENTIFIER = (SELECT id FROM Hashtag WHERE name = N'#darkhistory');
DECLARE @HashtagUrbanLegend UNIQUEIDENTIFIER = (SELECT id FROM Hashtag WHERE name = N'#urbanlegend');
DECLARE @HashtagSupernatural UNIQUEIDENTIFIER = (SELECT id FROM Hashtag WHERE name = N'#supernatural');
DECLARE @HashtagVietnam UNIQUEIDENTIFIER = (SELECT id FROM Hashtag WHERE name = N'#vietnam');
DECLARE @HashtagGhost UNIQUEIDENTIFIER = (SELECT id FROM Hashtag WHERE name = N'#ghost');

-- PodcastChannelHashtag table
INSERT INTO PodcastChannelHashtag (podcastChannelId, hashtagId, createdAt) VALUES
(@Channel1Id, @HashtagTrueCrime, '2024-02-05 10:00:00'),
(@Channel1Id, @HashtagMystery, '2024-02-05 10:00:00'),
(@Channel1Id, @HashtagVietnam, '2024-02-05 10:00:00'),
(@Channel2Id, @HashtagHorror, '2024-02-10 11:00:00'),
(@Channel2Id, @HashtagMystery, '2024-02-10 11:00:00'),
(@Channel2Id, @HashtagSupernatural, '2024-02-10 11:00:00'),
(@Channel3Id, @HashtagDarkHistory, '2024-03-01 09:00:00'),
(@Channel3Id, @HashtagTrueCrime, '2024-03-01 09:00:00');

-- PodcastShowHashtag table
INSERT INTO PodcastShowHashtag (podcastShowId, hashtagId, createdAt) VALUES
(@Show1Id, @HashtagTrueCrime, '2024-02-07 10:00:00'),
(@Show1Id, @HashtagMystery, '2024-02-07 10:00:00'),
(@Show1Id, @HashtagUnsolved, '2024-02-07 10:00:00'),
(@Show1Id, @HashtagVietnam, '2024-02-07 10:00:00'),
(@Show2Id, @HashtagSerialKiller, '2024-02-15 14:00:00'),
(@Show2Id, @HashtagTrueCrime, '2024-02-15 14:00:00'),
(@Show2Id, @HashtagVietnam, '2024-02-15 14:00:00'),
(@Show3Id, @HashtagHaunted, '2024-02-12 11:00:00'),
(@Show3Id, @HashtagHorror, '2024-02-12 11:00:00'),
(@Show3Id, @HashtagGhost, '2024-02-12 11:00:00'),
(@Show3Id, @HashtagSupernatural, '2024-02-12 11:00:00'),
(@Show4Id, @HashtagUrbanLegend, '2024-03-05 15:00:00'),
(@Show4Id, @HashtagMystery, '2024-03-05 15:00:00'),
(@Show4Id, @HashtagVietnam, '2024-03-05 15:00:00');

-- PodcastEpisodeHashtag table
INSERT INTO PodcastEpisodeHashtag (podcastEpisodeId, hashtagId, createdAt) VALUES
(@Episode1Id, @HashtagTrueCrime, '2024-02-20 10:00:00'),
(@Episode1Id, @HashtagMystery, '2024-02-20 10:00:00'),
(@Episode1Id, @HashtagUnsolved, '2024-02-20 10:00:00'),
(@Episode3Id, @HashtagHaunted, '2024-02-18 13:00:00'),
(@Episode3Id, @HashtagHorror, '2024-02-18 13:00:00'),
(@Episode3Id, @HashtagGhost, '2024-02-18 13:00:00'),
(@Episode4Id, @HashtagSerialKiller, '2024-02-25 09:00:00'),
(@Episode4Id, @HashtagTrueCrime, '2024-02-25 09:00:00'),
(@Episode5Id, @HashtagUrbanLegend, '2024-03-12 16:00:00'),
(@Episode5Id, @HashtagGhost, '2024-03-12 16:00:00');


-- =====================================================
-- BOOKING MANAGEMENT SERVICE DATABASE [Port: 8061]
-- =====================================================

-- PodcastBuddyBookingTone table
-- Podcaster 4 (Podcast Buddy) supports multiple tones
-- Using tone IDs from fix data (we need to get actual tone IDs from PodcastBookingTone)
-- For demo, assuming tones exist from fix data insert
INSERT INTO PodcastBuddyBookingTone (podcasterId, podcastBookingToneId, createdAt)
SELECT 4, id, GETDATE()
FROM PodcastBookingTone
WHERE podcastBookingToneCategoryId IN (1, 3) -- Male voices and special
  AND name IN (
    N'Nam – Tông trầm',
    N'Nam – Truyền cảm xúc',
    N'Nam – Nghiêm nghị',
    N'Nam – Kể chuyện đêm khuya',
    N'Giọng kể truyện cổ tích',
    N'Giọng tự sự'
  );

-- Booking table
-- Status: 1=Quotation Request, 2=Quotation Dealing, 3=Quotation Rejected, 5=Producing, 7=Producing Requested
-- NO bookings in status 6 (Track Previewing) or 8 (Completed) - because we don't have tracks
SET IDENTITY_INSERT Booking ON;

INSERT INTO Booking (id, customerId, podcasterId, totalPrice, depositedAmount, totalBookingWordCount, currentStatusId, cancelledAt, completedAt, createdAt, updatedAt) VALUES
-- Quotation Request (status 1)
(1, 6, 4, NULL, NULL, 1500, 1, NULL, NULL, '2024-11-01 10:00:00', '2024-11-01 10:00:00'),
(2, 7, 5, NULL, NULL, 2000, 1, NULL, NULL, '2024-11-05 14:00:00', '2024-11-05 14:00:00'),

-- Quotation Dealing (status 2)
(3, 8, 4, 300000, NULL, 2500, 2, NULL, NULL, '2024-10-20 09:00:00', '2024-11-10 11:00:00'),
(4, 9, 5, 250000, NULL, 1800, 2, NULL, NULL, '2024-10-25 15:00:00', '2024-11-12 16:00:00'),

-- Quotation Rejected (status 3)
(5, 10, 4, 400000, NULL, 3000, 3, NULL, NULL, '2024-10-15 10:00:00', '2024-10-22 14:00:00'),

-- Producing (status 5)
(6, 6, 5, 350000, 175000, 2200, 5, NULL, NULL, '2024-10-10 08:00:00', '2024-11-15 10:00:00'),
(7, 11, 4, 450000, 225000, 2800, 5, NULL, NULL, '2024-10-05 11:00:00', '2024-11-18 09:00:00'),

-- Producing Requested (status 7)
(8, 7, 4, 380000, 190000, 2400, 7, NULL, NULL, '2024-09-20 10:00:00', '2024-11-20 15:00:00');

SET IDENTITY_INSERT Booking OFF;

-- BookingStatusTracking table
-- Each booking must have at least one status tracking
INSERT INTO BookingStatusTracking (id, bookingId, bookingStatusId, createdAt) VALUES
-- Booking 1: Quotation Request
(NEWID(), 1, 1, '2024-11-01 10:00:00'),

-- Booking 2: Quotation Request
(NEWID(), 2, 1, '2024-11-05 14:00:00'),

-- Booking 3: Quotation Request -> Quotation Dealing
(NEWID(), 3, 1, '2024-10-20 09:00:00'),
(NEWID(), 3, 2, '2024-11-10 11:00:00'),

-- Booking 4: Quotation Request -> Quotation Dealing
(NEWID(), 4, 1, '2024-10-25 15:00:00'),
(NEWID(), 4, 2, '2024-11-12 16:00:00'),

-- Booking 5: Quotation Request -> Quotation Dealing -> Quotation Rejected
(NEWID(), 5, 1, '2024-10-15 10:00:00'),
(NEWID(), 5, 2, '2024-10-18 11:00:00'),
(NEWID(), 5, 3, '2024-10-22 14:00:00'),

-- Booking 6: Quotation Request -> Quotation Dealing -> Producing
(NEWID(), 6, 1, '2024-10-10 08:00:00'),
(NEWID(), 6, 2, '2024-10-15 09:00:00'),
(NEWID(), 6, 5, '2024-11-15 10:00:00'),

-- Booking 7: Quotation Request -> Quotation Dealing -> Producing
(NEWID(), 7, 1, '2024-10-05 11:00:00'),
(NEWID(), 7, 2, '2024-10-10 10:00:00'),
(NEWID(), 7, 5, '2024-11-18 09:00:00'),

-- Booking 8: Quotation Request -> Quotation Dealing -> Producing -> Producing Requested
(NEWID(), 8, 1, '2024-09-20 10:00:00'),
(NEWID(), 8, 2, '2024-09-25 11:00:00'),
(NEWID(), 8, 5, '2024-11-01 09:00:00'),
(NEWID(), 8, 7, '2024-11-20 15:00:00');

-- BookingRequirement table
-- Each booking has 1-2 requirements
INSERT INTO BookingRequirement (id, bookingId, name, description, requirementDocumentFileKey, createdAt, updatedAt) VALUES
(NEWID(), 1, N'Script gốc', N'File script podcast cần đọc', 'booking_requirements/booking_1_req_1.pdf', '2024-11-01 10:00:00', '2024-11-01 10:00:00'),
(NEWID(), 2, N'Nội dung podcast', N'Tài liệu nội dung và yêu cầu giọng đọc', 'booking_requirements/booking_2_req_1.pdf', '2024-11-05 14:00:00', '2024-11-05 14:00:00'),
(NEWID(), 3, N'Script và background music', N'Script đọc và nhạc nền mong muốn', 'booking_requirements/booking_3_req_1.pdf', '2024-10-20 09:00:00', '2024-10-20 09:00:00'),
(NEWID(), 3, N'Tham khảo giọng đọc', N'File tham khảo giọng đọc mong muốn', 'booking_requirements/booking_3_req_2.pdf', '2024-10-20 09:00:00', '2024-10-20 09:00:00'),
(NEWID(), 4, N'Kịch bản podcast', N'Nội dung chi tiết các tập podcast', 'booking_requirements/booking_4_req_1.pdf', '2024-10-25 15:00:00', '2024-10-25 15:00:00'),
(NEWID(), 5, N'Yêu cầu chi tiết', N'Yêu cầu về giọng đọc và phong cách', 'booking_requirements/booking_5_req_1.pdf', '2024-10-15 10:00:00', '2024-10-15 10:00:00'),
(NEWID(), 6, N'Script hoàn chỉnh', N'Script podcast đã được duyệt', 'booking_requirements/booking_6_req_1.pdf', '2024-10-10 08:00:00', '2024-10-10 08:00:00'),
(NEWID(), 7, N'Nội dung và timeline', N'Script và timeline dự kiến', 'booking_requirements/booking_7_req_1.pdf', '2024-10-05 11:00:00', '2024-10-05 11:00:00'),
(NEWID(), 8, N'Script podcast series', N'Kịch bản cho series podcast', 'booking_requirements/booking_8_req_1.pdf', '2024-09-20 10:00:00', '2024-09-20 10:00:00'),
(NEWID(), 8, N'Nhạc nền và hiệu ứng', N'Tài liệu về nhạc nền và sound effects', 'booking_requirements/booking_8_req_2.pdf', '2024-09-20 10:00:00', '2024-09-20 10:00:00');

-- BookingProducingRequest table
-- Only for booking 8 (status 7 - Producing Requested)
INSERT INTO BookingProducingRequest (id, bookingId, requestContent, isApproved, rejectReason, createdAt) VALUES
(NEWID(), 8, N'Đề nghị điều chỉnh tốc độ đọc chậm hơn một chút, và tăng cường hiệu ứng âm thanh ở phần hồi hộp', NULL, NULL, '2024-11-20 15:00:00');

-- NO BookingPodcastTrack inserted (no tracks completed yet)
-- NO BookingPodcastTrackListenSession inserted (no tracks to listen)


-- =====================================================
-- SUBSCRIPTION SERVICE DATABASE [Port: 8066]
-- =====================================================

-- PodcastSubscription table
-- Subscriptions from podcasters
SET IDENTITY_INSERT PodcastSubscription ON;

INSERT INTO PodcastSubscription (id, podcasterId, name, description, priceMonthly, priceAnnually, isActive, cancelledAt, createdAt, updatedAt) VALUES
(1, 4, N'True Crime VN Premium', N'Truy cập không giới hạn tất cả nội dung True Crime Vietnam', 99000, 990000, 1, NULL, '2024-02-10 10:00:00', '2024-02-10 10:00:00'),
(2, 5, N'Mystery World Exclusive', N'Nội dung độc quyền và early access', 79000, 790000, 1, NULL, '2024-02-15 11:00:00', '2024-02-15 11:00:00'),
(3, 4, N'Dark History Access', N'Truy cập kênh Dark History Channel', 89000, 890000, 0, '2024-03-20 10:00:00', '2024-03-05 09:00:00', '2024-03-20 10:00:00');

SET IDENTITY_INSERT PodcastSubscription OFF;

-- PodcastSubscriptionCycleTypePrice table
INSERT INTO PodcastSubscriptionCycleTypePrice (podcastSubscriptionId, subscriptionCycleTypeId, price, createdAt, updatedAt) VALUES
(1, 1, 99000, '2024-02-10 10:00:00', '2024-02-10 10:00:00'),  -- Monthly
(1, 2, 990000, '2024-02-10 10:00:00', '2024-02-10 10:00:00'), -- Annually
(2, 1, 79000, '2024-02-15 11:00:00', '2024-02-15 11:00:00'),
(2, 2, 790000, '2024-02-15 11:00:00', '2024-02-15 11:00:00'),
(3, 1, 89000, '2024-03-05 09:00:00', '2024-03-05 09:00:00'),
(3, 2, 890000, '2024-03-05 09:00:00', '2024-03-05 09:00:00');

-- PodcastSubscriptionBenefitMapping table
-- Map benefits to subscriptions
INSERT INTO PodcastSubscriptionBenefitMapping (podcastSubscriptionId, podcastSubscriptionBenefitId, version, createdAt, updatedAt) VALUES
-- Subscription 1 benefits
(1, 1, 1, '2024-02-10 10:00:00', '2024-02-10 10:00:00'), -- Non-Quota Listening
(1, 2, 1, '2024-02-10 10:00:00', '2024-02-10 10:00:00'), -- Subscriber-Only Shows
(1, 3, 1, '2024-02-10 10:00:00', '2024-02-10 10:00:00'), -- Subscriber-Only Episodes
(1, 4, 1, '2024-02-10 10:00:00', '2024-02-10 10:00:00'), -- Bonus Episodes
-- Subscription 2 benefits
(2, 1, 1, '2024-02-15 11:00:00', '2024-02-15 11:00:00'),
(2, 3, 1, '2024-02-15 11:00:00', '2024-02-15 11:00:00'),
(2, 5, 1, '2024-02-15 11:00:00', '2024-02-15 11:00:00'), -- Early Access
-- Subscription 3 benefits
(3, 1, 1, '2024-03-05 09:00:00', '2024-03-05 09:00:00'),
(3, 6, 1, '2024-03-05 09:00:00', '2024-03-05 09:00:00'); -- Archive Access

-- PodcastSubscriptionRegistration table
INSERT INTO PodcastSubscriptionRegistration (id, accountId, podcastSubscriptionId, currentVersion, isAcceptNewestVersionSwitch, lastPaidAt, cancelledAt, createdAt, updatedAt) VALUES
-- Active subscriptions
(NEWID(), 6, 1, 1, 1, '2024-11-01 10:00:00', NULL, '2024-03-01 10:00:00', '2024-11-01 10:00:00'),
(NEWID(), 7, 1, 1, 1, '2024-11-05 11:00:00', NULL, '2024-03-05 11:00:00', '2024-11-05 11:00:00'),
(NEWID(), 8, 2, 1, 1, '2024-11-10 09:00:00', NULL, '2024-03-10 09:00:00', '2024-11-10 09:00:00'),
(NEWID(), 9, 2, 1, NULL, '2024-11-15 14:00:00', NULL, '2024-03-15 14:00:00', '2024-11-15 14:00:00'),
(NEWID(), 10, 1, 1, 1, '2024-11-20 16:00:00', NULL, '2024-03-20 16:00:00', '2024-11-20 16:00:00'),
-- Cancelled subscription
(NEWID(), 11, 3, 1, NULL, '2024-03-25 10:00:00', '2024-04-25 10:00:00', '2024-03-25 10:00:00', '2024-04-25 10:00:00');


-- =====================================================
-- USER SERVICE RELATIONSHIPS [Port: 8046]
-- =====================================================
-- Insert after PodcastService data is ready

-- AccountFollowedPodcaster table
INSERT INTO AccountFollowedPodcaster (accountId, podcasterId, createdAt) VALUES
(6, 4, '2024-03-02 10:00:00'),
(6, 5, '2024-03-08 14:00:00'),
(7, 4, '2024-03-06 11:00:00'),
(8, 5, '2024-03-11 15:00:00'),
(9, 4, '2024-03-16 09:00:00'),
(10, 5, '2024-03-21 13:00:00'),
(11, 4, '2024-03-26 16:00:00'),
(11, 5, '2024-03-27 10:00:00');

-- AccountFavoritedPodcastChannel table
INSERT INTO AccountFavoritedPodcastChannel (accountId, podcastChannelId, createdAt) VALUES
(6, @Channel1Id, '2024-03-03 10:00:00'),
(6, @Channel2Id, '2024-03-09 11:00:00'),
(7, @Channel1Id, '2024-03-07 14:00:00'),
(8, @Channel2Id, '2024-03-12 16:00:00'),
(9, @Channel1Id, '2024-03-17 10:00:00'),
(10, @Channel2Id, '2024-03-22 11:00:00'),
(11, @Channel1Id, '2024-03-28 09:00:00');

-- AccountFollowedPodcastShow table
INSERT INTO AccountFollowedPodcastShow (accountId, podcastShowId, createdAt) VALUES
(6, @Show1Id, '2024-03-04 11:00:00'),
(6, @Show3Id, '2024-03-10 14:00:00'),
(7, @Show1Id, '2024-03-08 10:00:00'),
(7, @Show2Id, '2024-03-12 16:00:00'),
(8, @Show3Id, '2024-03-13 11:00:00'),
(9, @Show1Id, '2024-03-18 09:00:00'),
(9, @Show2Id, '2024-03-19 15:00:00'),
(10, @Show3Id, '2024-03-23 13:00:00'),
(11, @Show1Id, '2024-03-29 10:00:00'),
(11, @Show3Id, '2024-03-30 11:00:00');

-- AccountSavedPodcastEpisode table
INSERT INTO AccountSavedPodcastEpisode (accountId, podcastEpisodeId, createdAt) VALUES
(6, @Episode1Id, '2024-03-05 12:00:00'),
(6, @Episode3Id, '2024-03-11 15:00:00'),
(7, @Episode1Id, '2024-03-09 11:00:00'),
(7, @Episode4Id, '2024-03-14 16:00:00'),
(8, @Episode3Id, '2024-03-14 13:00:00'),
(8, @Episode5Id, '2024-03-18 10:00:00'),
(9, @Episode1Id, '2024-03-19 10:00:00'),
(9, @Episode2Id, '2024-03-20 14:00:00'),
(10, @Episode3Id, '2024-03-24 12:00:00'),
(11, @Episode1Id, '2024-03-31 09:00:00'),
(11, @Episode4Id, '2024-04-01 11:00:00');

-- PodcastBuddyReview table
-- Reviews for Podcast Buddy (Podcaster 4)
INSERT INTO PodcastBuddyReview (id, title, content, rating, accountId, podcastBuddyId, deletedAt, createdAt, updatedAt) VALUES
(NEWID(), N'Rất chuyên nghiệp!', N'Giọng đọc hay, giao hàng đúng hạn, rất hài lòng', 5.0, 6, 4, NULL, '2024-10-15 14:00:00', '2024-10-15 14:00:00'),
(NEWID(), N'Chất lượng tốt', N'Podcast buddy làm việc nhiệt tình và chỉnh sửa cẩn thận', 4.5, 7, 4, NULL, '2024-10-20 10:00:00', '2024-10-20 10:00:00'),
(NEWID(), N'Đáng giá tiền', N'Giá hợp lý, chất lượng cao', 4.5, 8, 4, NULL, '2024-10-25 16:00:00', '2024-10-25 16:00:00'),
(NEWID(), N'Sẽ quay lại lần sau', N'Rất hài lòng với dịch vụ', 5.0, 11, 4, NULL, '2024-11-01 11:00:00', '2024-11-01 11:00:00'),
(NEWID(), N'Tốt nhưng hơi chậm', N'Chất lượng tốt nhưng giao hàng chậm hơn dự kiến', 3.5, 9, 4, NULL, '2024-11-05 15:00:00', '2024-11-05 15:00:00');


-- =====================================================
-- PODCAST SERVICE - LISTEN SESSIONS [Port: 8056]
-- =====================================================
-- Note: Episodes are in Draft so technically shouldn't have listen sessions
-- But for demo purposes, we can show some test sessions from podcasters themselves

-- PodcastEpisodeListenSession table
-- Only test sessions, not from regular customers
INSERT INTO PodcastEpisodeListenSession (id, accountId, podcastEpisodeId, lastListenDurationSeconds, isCompleted, isContentRemoved, podcastCategoryId, podcastSubCategoryId, expiredAt, createdAt) VALUES
-- Podcaster testing their own episodes
(NEWID(), 4, @Episode1Id, 180, 0, 0, 1, 2, '2024-12-20 10:00:00', '2024-02-21 10:00:00'),
(NEWID(), 4, @Episode2Id, 240, 0, 0, 1, 2, '2024-12-30 11:00:00', '2024-03-02 11:00:00'),
(NEWID(), 5, @Episode3Id, 300, 1, 0, 2, 16, '2025-01-18 13:00:00', '2024-02-19 13:00:00');


-- =====================================================
-- TRANSACTION SERVICE DATABASE [Port: 8076]
-- =====================================================

-- AccountBalanceTransaction table
INSERT INTO AccountBalanceTransaction (id, accountId, orderCode, amount, transactionTypeId, transactionStatusId, createdAt, updatedAt) VALUES
-- Account deposits
(NEWID(), 6, 'DEP-2024-001', 500000, 1, 2, '2024-02-28 10:00:00', '2024-02-28 10:00:00'),
(NEWID(), 7, 'DEP-2024-002', 1200000, 1, 2, '2024-03-04 14:00:00', '2024-03-04 14:00:00'),
(NEWID(), 8, 'DEP-2024-003', 800000, 1, 2, '2024-03-09 11:00:00', '2024-03-09 11:00:00'),
(NEWID(), 9, 'DEP-2024-004', 300000, 1, 2, '2024-03-14 16:00:00', '2024-03-14 16:00:00'),
(NEWID(), 10, 'DEP-2024-005', 1500000, 1, 2, '2024-03-19 13:00:00', '2024-03-19 13:00:00'),
(NEWID(), 11, 'DEP-2024-006', 2000000, 1, 2, '2024-03-24 10:00:00', '2024-03-24 10:00:00'),
-- Account withdrawals
(NEWID(), 4, 'WD-2024-001', 2000000, 2, 2, '2024-11-01 15:00:00', '2024-11-01 15:00:00'),
(NEWID(), 5, 'WD-2024-002', 1500000, 2, 2, '2024-11-10 14:00:00', '2024-11-10 14:00:00');

-- BookingTransaction table
-- Only for bookings that have been deposited (status >= 5 - Producing)
-- Bookings 6, 7, 8 have deposits
INSERT INTO BookingTransaction (id, bookingId, amount, profit, transactionTypeId, transactionStatusId, createdAt, updatedAt) VALUES
-- Booking 6 deposit
(NEWID(), 6, 175000, NULL, 3, 2, '2024-11-15 10:00:00', '2024-11-15 10:00:00'),
-- Booking 7 deposit
(NEWID(), 7, 225000, NULL, 3, 2, '2024-11-18 09:00:00', '2024-11-18 09:00:00'),
-- Booking 8 deposit
(NEWID(), 8, 190000, NULL, 3, 2, '2024-11-01 09:00:00', '2024-11-01 09:00:00');

-- PodcastSubscriptionTransaction table
-- For subscription registrations
DECLARE @SubReg1Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastSubscriptionRegistration WHERE accountId = 6 AND podcastSubscriptionId = 1);
DECLARE @SubReg2Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastSubscriptionRegistration WHERE accountId = 7 AND podcastSubscriptionId = 1);
DECLARE @SubReg3Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastSubscriptionRegistration WHERE accountId = 8 AND podcastSubscriptionId = 2);
DECLARE @SubReg4Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastSubscriptionRegistration WHERE accountId = 9 AND podcastSubscriptionId = 2);
DECLARE @SubReg5Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM PodcastSubscriptionRegistration WHERE accountId = 10 AND podcastSubscriptionId = 1);

INSERT INTO PodcastSubscriptionTransaction (id, podcastSubscriptionRegistrationId, amount, profit, transactionTypeId, transactionStatusId, createdAt, updatedAt) VALUES
-- Customer payments (type 8)
(NEWID(), @SubReg1Id, 99000, NULL, 8, 2, '2024-11-01 10:00:00', '2024-11-01 10:00:00'),
(NEWID(), @SubReg2Id, 99000, NULL, 8, 2, '2024-11-05 11:00:00', '2024-11-05 11:00:00'),
(NEWID(), @SubReg3Id, 79000, NULL, 8, 2, '2024-11-10 09:00:00', '2024-11-10 09:00:00'),
(NEWID(), @SubReg4Id, 79000, NULL, 8, 2, '2024-11-15 14:00:00', '2024-11-15 14:00:00'),
(NEWID(), @SubReg5Id, 99000, NULL, 8, 2, '2024-11-20 16:00:00', '2024-11-20 16:00:00'),
-- System income (type 10) - 20% profit from subscriptions
(NEWID(), @SubReg1Id, 19800, 19800, 10, 2, '2024-11-08 10:00:00', '2024-11-08 10:00:00'),
(NEWID(), @SubReg2Id, 19800, 19800, 10, 2, '2024-11-12 11:00:00', '2024-11-12 11:00:00'),
(NEWID(), @SubReg3Id, 15800, 15800, 10, 2, '2024-11-17 09:00:00', '2024-11-17 09:00:00'),
-- Podcaster income (type 11) - 80% to podcaster
(NEWID(), @SubReg1Id, 79200, 79200, 11, 2, '2024-11-08 10:00:00', '2024-11-08 10:00:00'),
(NEWID(), @SubReg2Id, 79200, 79200, 11, 2, '2024-11-12 11:00:00', '2024-11-12 11:00:00'),
(NEWID(), @SubReg3Id, 63200, 63200, 11, 2, '2024-11-17 09:00:00', '2024-11-17 09:00:00');


-- =====================================================
-- MODERATION SERVICE DATABASE [Port: 8071]
-- =====================================================

-- PodcastBuddyReport table
INSERT INTO PodcastBuddyReport (id, content, accountId, podcastBuddyId, podcastBuddyReportTypeId, resolvedAt, createdAt) VALUES
(NEWID(), N'Spam tin nhắn liên tục yêu cầu booking', 9, 4, 2, '2024-11-10 15:00:00', '2024-11-08 10:00:00'),
(NEWID(), N'Chất lượng không như cam kết', 10, 4, 9, NULL, '2024-11-15 14:00:00');

-- PodcastShowReport table
INSERT INTO PodcastShowReport (id, content, accountId, podcastShowId, podcastShowReportTypeId, resolvedAt, createdAt) VALUES
(NEWID(), N'Nội dung có yếu tố bạo lực quá mức', 8, @Show2Id, 2, NULL, '2024-11-05 16:00:00'),
(NEWID(), N'Thông tin sai sự thật về vụ án', 11, @Show1Id, 4, '2024-11-12 10:00:00', '2024-11-01 11:00:00');

-- PodcastEpisodeReport table
INSERT INTO PodcastEpisodeReport (id, content, accountId, podcastEpisodeId, podcastEpisodeReportTypeId, resolvedAt, createdAt) VALUES
(NEWID(), N'Nội dung quá kinh dị, không phù hợp', 6, @Episode3Id, 2, NULL, '2024-11-20 20:00:00'),
(NEWID(), N'Thông tin gây hiểu lầm về sự kiện lịch sử', 7, @Episode1Id, 4, NULL, '2024-11-22 15:00:00');

-- PodcastBuddyReportReviewSession table
INSERT INTO PodcastBuddyReportReviewSession (id, podcastBuddyId, assignedStaff, resolvedViolationPoint, isResolved, createdAt, updatedAt) VALUES
(NEWID(), 4, 2, 1, 1, '2024-11-09 09:00:00', '2024-11-10 15:00:00'),
(NEWID(), 4, 3, 1, NULL, '2024-11-16 10:00:00', '2024-11-16 10:00:00');

-- PodcastShowReportReviewSession table
INSERT INTO PodcastShowReportReviewSession (id, podcastShowId, assignedStaff, isResolved, createdAt, updatedAt) VALUES
(NEWID(), @Show2Id, 2, NULL, '2024-11-06 09:00:00', '2024-11-06 09:00:00'),
(NEWID(), @Show1Id, 3, 1, '2024-11-02 10:00:00', '2024-11-12 10:00:00');

-- PodcastEpisodeReportReviewSession table
INSERT INTO PodcastEpisodeReportReviewSession (id, podcastEpisodeId, assignedStaff, isResolved, createdAt, updatedAt) VALUES
(NEWID(), @Episode3Id, 2, NULL, '2024-11-21 09:00:00', '2024-11-21 09:00:00'),
(NEWID(), @Episode1Id, 3, NULL, '2024-11-23 10:00:00', '2024-11-23 10:00:00');

-- DMCAAccusation table
SET IDENTITY_INSERT DMCAAccusation ON;

INSERT INTO DMCAAccusation (id, podcastShowId, podcastEpisodeId, assignedStaff, accuserEmail, accuserPhone, accuserFullName, dismissReason, resolvedAt, cancelledAt, createdAt, updatedAt) VALUES
(1, NULL, @Episode1Id, 2, 'accuser1@example.com', '0981234567', N'Nguyễn Văn A', NULL, NULL, NULL, '2024-11-18 14:00:00', '2024-11-18 14:00:00'),
(2, @Show2Id, NULL, 3, 'accuser2@example.com', '0982345678', N'Trần Thị B', NULL, NULL, NULL, '2024-11-25 10:00:00', '2024-11-25 10:00:00');

SET IDENTITY_INSERT DMCAAccusation OFF;

-- DMCANotice table
INSERT INTO DMCANotice (id, isValid, invalidReason, validatedBy, validatedAt, dmcaAccusationId, createdAt, updatedAt) VALUES
(NEWID(), NULL, NULL, NULL, NULL, 1, '2024-11-18 14:00:00', '2024-11-18 14:00:00'),
(NEWID(), NULL, NULL, NULL, NULL, 2, '2024-11-25 10:00:00', '2024-11-25 10:00:00');

-- Get DMCA Notice IDs
DECLARE @DMCANotice1Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM DMCANotice WHERE dmcaAccusationId = 1);
DECLARE @DMCANotice2Id UNIQUEIDENTIFIER = (SELECT TOP 1 id FROM DMCANotice WHERE dmcaAccusationId = 2);

-- DMCANoticeAttachFile table
INSERT INTO DMCANoticeAttachFile (id, dmcaNoticeId, attachFileKey, createdAt) VALUES
(NEWID(), @DMCANotice1Id, 'dmca_notices/dmca_notice_1_file_1.pdf', '2024-11-18 14:00:00'),
(NEWID(), @DMCANotice1Id, 'dmca_notices/dmca_notice_1_file_2.pdf', '2024-11-18 14:00:00'),
(NEWID(), @DMCANotice2Id, 'dmca_notices/dmca_notice_2_file_1.pdf', '2024-11-25 10:00:00');

-- DMCAAccusationStatusTracking table
INSERT INTO DMCAAccusationStatusTracking (id, dmcaAccusationId, dmcaAccusationStatusId, createdAt) VALUES
(NEWID(), 1, 1, '2024-11-18 14:00:00'), -- Pending DMCA Notice Review
(NEWID(), 2, 1, '2024-11-25 10:00:00'); -- Pending DMCA Notice Review


-- =====================================================
-- END OF DEMO DATA INSERT SCRIPT
-- =====================================================
