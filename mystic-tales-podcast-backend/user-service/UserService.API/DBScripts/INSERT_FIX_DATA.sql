-- =====================================================
-- FIX DATA INSERT SCRIPT
-- =====================================================

-- =====================================================
-- USER SERVICE DATABASE [Port: 8046]
-- =====================================================

-- Role table
INSERT INTO Role (id, name) VALUES
(1, N'Customer'),
(2, N'Staff'),
(3, N'Admin');

-- NotificationType table
-- (To be defined based on business requirements)


-- =====================================================
-- SYSTEM CONFIGURATION SERVICE DATABASE [Port: 8051]
-- =====================================================

-- SystemConfigProfile table
SET IDENTITY_INSERT SystemConfigProfile ON;
INSERT INTO SystemConfigProfile (id, name, isActive, deletedAt, createdAt, updatedAt) VALUES
(1, N'Default Configuration', 1, NULL, '2025-10-05 23:36:35.663', '2025-10-05 23:36:35.663');
SET IDENTITY_INSERT SystemConfigProfile OFF;

-- PodcastSubscriptionConfig table
INSERT INTO PodcastSubscriptionConfig (configProfileId, subscriptionCycleTypeId, profitRate, incomeTakenDelayDays, createdAt, updatedAt) VALUES
(1, 1, 0.2, 7, '2025-10-05 23:37:03.267', '2025-10-05 23:37:03.267'),
(1, 2, 0.2, 7, '2025-10-05 23:37:03.267', '2025-10-05 23:37:03.267');

-- PodcastSuggestionConfig table
INSERT INTO PodcastSuggestionConfig (configProfileId, minShortRangeUserBehaviorLookbackDayCount, minMediumRangeUserBehaviorLookbackDayCount, minLongRangeUserBehaviorLookbackDayCount, minShortRangeContentBehaviorLookbackDayCount, minMediumRangeContentBehaviorLookbackDayCount, minLongRangeContentBehaviorLookbackDayCount, minExtraLongRangeContentBehaviorLookbackDayCount, minChannelQuery, minShowQuery, createdAt, updatedAt) VALUES
(1, 2, 7, 30, 2, 7, 30, 90, 10, 10, '2025-10-05 23:37:03.270', '2025-11-08 13:40:08.153');

-- BookingConfig table
INSERT INTO BookingConfig (configProfileId, profitRate, depositRate, podcastTrackPreviewListenSlot, previewResponseAllowedDays, producingRequestResponseAllowedDays, chatRoomExpiredHours, chatRoomFileMessageExpiredHours, freeInitialBookingStorageSize, singleStorageUnitPurchasePrice, createdAt, updatedAt) VALUES
(1, 0.3, 0.5, 3, 30, 1, 5, 5, 1, 5000.00, '2025-10-05 23:37:03.273', '2025-10-05 23:37:03.273');

-- AccountConfig table
INSERT INTO AccountConfig (configProfileId, violationPointDecayHours, podcastListenSlotThreshold, podcastListenSlotRecoverySeconds, createdAt, updatedAt) VALUES
(1, 24, 9, 18000, '2025-10-05 23:37:03.273', '2025-10-05 23:37:03.277');

-- AccountViolationLevelConfig table
INSERT INTO AccountViolationLevelConfig (configProfileId, violationLevel, violationPointThreshold, punishmentDays, createdAt, updatedAt) VALUES
(1, 0, 10, 0, '2025-10-05 23:37:03.277', '2025-10-14 12:31:13.550'),
(1, 1, 20, 7, '2025-10-05 23:37:03.277', '2025-10-05 23:37:03.277'),
(1, 2, 50, 14, '2025-10-05 23:37:03.277', '2025-10-05 23:37:03.277'),
(1, 3, 200, 30, '2025-10-05 23:37:03.277', '2025-10-05 23:37:03.277'),
(1, 4, 500, 365, '2025-10-05 23:37:03.277', '2025-10-05 23:37:03.277');

-- ReviewSessionConfig table
INSERT INTO ReviewSessionConfig (configProfileId, podcastBuddyUnResolvedReportStreak, podcastShowUnResolvedReportStreak, podcastEpisodeUnResolvedReportStreak, podcastEpisodePublishEditRequirementExpiredHours, createdAt, updatedAt) VALUES
(1, 5, 5, 5, 24, '2025-10-05 23:37:03.283', '2025-10-05 23:37:03.283');

-- PodcastRestrictedTerm table
INSERT INTO PodcastRestrictedTerm (id, term) VALUES
(1, N'cmm'),
(2, N'đmm'),
(3, N'địt mẹ'),
(4, N'địt bố'),
(5, N'đụ mẹ'),
(6, N'đụ má'),
(7, N'đụ'),
(8, N'dm'),
(9, N'đm'),
(10, N'dcm'),
(11, N'đcm'),
(12, N'dmm'),
(13, N'vcl'),
(14, N'vkl'),
(15, N'clgt'),
(16, N'vl'),
(17, N'cc'),
(18, N'ccc'),
(19, N'c*c'),
(20, N'cứt'),
(21, N'ỉa'),
(22, N'đái'),
(23, N'rắm'),
(24, N'thối tha'),
(25, N'óc chó'),
(26, N'đầu bò'),
(27, N'đầu đất'),
(28, N'não chó'),
(29, N'não cá vàng'),
(30, N'não phẳng'),
(31, N'ngu dốt'),
(32, N'đần độn'),
(33, N'khờ dại'),
(34, N'khùng'),
(35, N'điên khùng'),
(36, N'thần kinh'),
(37, N'tâm thần'),
(38, N'chậm phát triển'),
(39, N'tật nguyền (xúc phạm)'),
(40, N'què'),
(41, N'đui'),
(42, N'mù'),
(43, N'câm'),
(44, N'điếc'),
(45, N'bại não'),
(46, N'bẩn thỉu'),
(47, N'hôi hám'),
(48, N'dơ dáy'),
(49, N'súc vật'),
(50, N'thú vật'),
(51, N'đồ chó má'),
(52, N'đồ heo'),
(53, N'đồ lợn'),
(54, N'đồ bò'),
(55, N'đồ trâu'),
(56, N'đồ khỉ'),
(57, N'đồ rác'),
(58, N'cặn bã'),
(59, N'tiện nhân'),
(60, N'đê tiện'),
(61, N'bỉ ổi'),
(62, N'vô học'),
(63, N'mất dạy'),
(64, N'mất nết'),
(65, N'hỗn láo'),
(66, N'bố láo'),
(67, N'láo toét'),
(68, N'láo lếu'),
(69, N'láo chó'),
(70, N'khốn nạn'),
(71, N'khốn kiếp'),
(72, N'chết tiệt'),
(73, N'đồ hèn'),
(74, N'nhục'),
(75, N'nhục mặt'),
(76, N'hạ đẳng'),
(77, N'mạt hạng'),
(78, N'rẻ rách'),
(79, N'phèn'),
(80, N'nhà quê (xúc phạm)'),
(81, N'con hoang'),
(82, N'đồ mồ côi (xúc phạm)'),
(83, N'đồ phản phúc'),
(84, N'đồ ăn hại'),
(85, N'đồ phế vật'),
(86, N'ăn bám'),
(87, N'đào mỏ'),
(88, N'lẳng lơ'),
(89, N'lăng loàn'),
(90, N'dâm đãng'),
(91, N'dâm phụ'),
(92, N'con phò'),
(93, N'con cave'),
(94, N'điếm'),
(95, N'con điếm'),
(96, N'đĩ thõa'),
(97, N'đĩ mẹ'),
(98, N'đĩ chó'),
(99, N'bú c*'),
(100, N'liếm đít'),
(101, N'địt m mày'),
(102, N'cút m mày'),
(103, N'đ!t mẹ'),
(104, N'đt mẹ'),
(105, N'djt mẹ'),
(106, N'đjt mẹ'),
(107, N'dit me'),
(108, N'd1t me'),
(109, N'd.i.t mẹ'),
(110, N'd i t mẹ'),
(111, N'djt m'),
(112, N'đ!t m*'),
(113, N'djt mạ*'),
(114, N'đjt mạ*'),
(115, N'dit m* may'),
(116, N'd1t m* my'),
(117, N'đ!t m my'),
(118, N'djt m**y'),
(119, N'djt m m*'),
(120, N'đt mạy'),
(121, N'dit m a y'),
(122, N'djt m a y'),
(123, N'đụ m*'),
(124, N'đụ mạ*'),
(125, N'đụ mịa'),
(126, N'du me'),
(127, N'du ma'),
(128, N'd u m e'),
(129, N'đu m*'),
(130, N'd.u.m.e'),
(131, N'du m*'),
(132, N'đụ m`.'),
(133, N'đo'),
(134, N'do'),
(135, N'deo'),
(136, N'đéo m*'),
(137, N'đo mẹ'),
(138, N'deo me'),
(139, N'd3o'),
(140, N'đ3o'),
(141, N'd. e . o'),
(142, N'đ**o'),
(143, N'lon'),
(144, N'ln'),
(145, N'l0n'),
(146, N'l—n'),
(147, N'l•n'),
(148, N'l0n'),
(149, N'ln'),
(150, N'l. o . n'),
(151, N'l_on'),
(152, N'l-0-n'),
(153, N'cac'),
(154, N'cc'),
(155, N'c@c'),
(156, N'cạk'),
(157, N'cak'),
(158, N'c4c'),
(159, N'c/\c'),
(160, N'c.a.c'),
(161, N'c a c'),
(162, N'cặk'),
(163, N'oc cho'),
(164, N'0c ch0'),
(165, N'o c c h o'),
(166, N'oc-cho'),
(167, N'o.c.c.h.o'),
(168, N'o''c cho'),
(169, N'ọc chó'),
(170, N'óc ch0'),
(171, N'oc ch0'),
(172, N'0c chó'),
(173, N'mat day'),
(174, N'matday'),
(175, N'm@t d@y'),
(176, N'md dy'),
(177, N'm a t d a y'),
(178, N'mất-dạy'),
(179, N'mats day'),
(180, N'mât day'),
(181, N'm a t-d @ y'),
(182, N'm4t d@y'),
(183, N'khon nan'),
(184, N'kh0n n@n'),
(185, N'khn nn'),
(186, N'khon-nan'),
(187, N'khốn-nạn'),
(188, N'khon kiep'),
(189, N'kh0n ki3p'),
(190, N'khn kip'),
(191, N'khon-kiep'),
(192, N'khốn-kiếp'),
(193, N'ngu dot'),
(194, N'ngu d0t'),
(195, N'n g u d o t'),
(196, N'ngu-dốt'),
(197, N'ngu**dốt'),
(198, N'dan don'),
(199, N'd@n d0n'),
(200, N'đn đn'),
(201, N'đần-độn'),
(202, N'd a n d o n'),
(203, N'suc vat'),
(204, N'sc vt'),
(205, N's-uc v-at'),
(206, N's u c v a t'),
(207, N'phe vat'),
(208, N'ph* vt'),
(209, N'phe-vat'),
(210, N'can ba'),
(211, N'cn b'),
(212, N'can-ba'),
(213, N'con pho'),
(214, N'c0n ph0'),
(215, N'cn ph'),
(216, N'con-pho'),
(217, N'c0n cave'),
(218, N'con c@ve'),
(219, N'con cv3'),
(220, N'd!em'),
(221, N'di3m'),
(222, N'd*i');


-- =====================================================
-- BOOKING MANAGEMENT SERVICE DATABASE [Port: 8061]
-- =====================================================

-- BookingStatus table
INSERT INTO BookingStatus (id, name) VALUES
(1, N'Quotation Request'),
(2, N'Quotation Dealing'),
(3, N'Quotation Rejected'),
(4, N'Quotation Cancelled'),
(5, N'Producing'),
(6, N'Track Previewing'),
(7, N'Producing Requested'),
(8, N'Completed'),
(9, N'Customer Cancel Request'),
(10, N'Podcast Buddy Cancel Request'),
(11, N'Cancelled Automatically'),
(12, N'Cancelled Manually');

-- BookingOptionalManualCancelReason table
INSERT INTO BookingOptionalManualCancelReason (id, reason) VALUES
(1, N'Podcast buddy did not respond in time'),
(2, N'Delay in delivery timeline'),
(3, N'I found another podcaster'),
(4, N'Too expensive / Price not acceptable'),
(5, N'Quality concerns (voice, style, demo mismatch)'),
(6, N'Technical issues with audio or platform'),
(7, N'Personal reasons (no longer need the podcast)'),
(8, N'Misunderstanding about booking terms');

-- PodcastBookingToneCategory table
INSERT INTO PodcastBookingToneCategory (id, name) VALUES
(1, N'Giọng Nam'),
(2, N'Giọng Nữ'),
(3, N'Giọng Đặc biệt / Trung tính');

-- PodcastBookingTone table
INSERT INTO PodcastBookingTone (name, description, podcastBookingToneCategoryId) VALUES
(N'Nam – Tông cao', N'Trẻ trung, năng động, hợp podcast giải trí, thể thao', 1),
(N'Nam – Tông trầm', N'Truyền cảm, chững chạc, hợp podcast tâm sự, triết lý', 1),
(N'Nam – Ấm áp', N'Dễ nghe, thân thiện, gần gũi', 1),
(N'Nam – Truyền cảm xúc', N'Diễn đạt tốt cảm xúc, hợp kể chuyện, trải nghiệm cá nhân', 1),
(N'Nam – Lạnh lùng', N'Bí ẩn, hợp chủ đề phân tích, crime, thriller', 1),
(N'Nam – Hài hước', N'Linh hoạt, vui tươi, hợp talkshow hoặc podcast giải trí', 1),
(N'Nam – Nghiêm nghị', N'Chuẩn mực, phát thanh viên, hợp chủ đề chính luận', 1),
(N'Nam – Đam mê', N'Mạnh mẽ, nhiều năng lượng, hợp chủ đề truyền cảm hứng', 1),
(N'Nam – Lãnh đạo', N'Quyết đoán, thuyết phục, hợp kinh doanh, quản trị', 1),
(N'Nam – Lãng tử', N'Tự do, nhẹ nhàng, hợp podcast du lịch, khám phá', 1),
(N'Nam – Bình thản', N'Chậm rãi, từ tốn, hợp thiền, mindfulness', 1),
(N'Nam – Dễ thương', N'Giọng cao vừa, hóm hỉnh, gần gũi', 1),
(N'Nam – Trẻ trung sinh viên', N'Năng động, thân thiện, hợp đối tượng Gen Z', 1),
(N'Nam – Kể chuyện đêm khuya', N'Trầm, nhẹ, chậm rãi, du dương, hợp podcast đêm', 1),
(N'Nam – Nhà nghiên cứu', N'Rõ ràng, logic, hợp podcast khoa học, tri thức', 1),
(N'Nam – MC chuyên nghiệp', N'Tông trung, phát âm chuẩn, dẫn dắt tốt', 1),
(N'Nam – Phản biện', N'Dứt khoát, có nhịp lên xuống, hợp chủ đề tranh luận', 1),
(N'Nam – Hào sảng', N'Giọng vang, nhiệt huyết, hợp du lịch, phượt', 1),
(N'Nam – Chất nghệ sĩ', N'Biểu cảm, cảm xúc, hợp thơ, văn, nghệ thuật', 1),
(N'Nam – Hơi khàn', N'Quyến rũ, cuốn hút, hợp podcast tình cảm, tâm sự', 1),
(N'Nữ – Tông cao', N'Tươi sáng, hoạt bát, hợp lifestyle, giáo dục', 2),
(N'Nữ – Tông trầm', N'Quyến rũ, trưởng thành, hợp tâm sự, cảm xúc', 2),
(N'Nữ – Quyến rũ', N'Dẻo, nhấn nhá, hợp chủ đề tình yêu, cảm xúc', 2),
(N'Nữ – Dễ thương', N'Trong trẻo, vui tươi, hợp kể chuyện, giải trí', 2),
(N'Nữ – Truyền cảm', N'Dịu dàng, cảm xúc, hợp đọc truyện, tâm sự', 2),
(N'Nữ – Ấm áp', N'Nhẹ nhàng, thân thiện, hợp giáo dục, chăm sóc tinh thần', 2),
(N'Nữ – Nghiêm túc', N'Chắc giọng, rõ chữ, hợp chủ đề chuyên môn', 2),
(N'Nữ – MC radio', N'Đều, mềm, rõ nhịp, hợp talkshow sáng', 2),
(N'Nữ – Trẻ trung sinh viên', N'Năng động, vui vẻ, hợp podcast học đường', 2),
(N'Nữ – Dịu dàng', N'Nói chậm, mềm mại, hợp chủ đề thư giãn, tâm hồn', 2),
(N'Nữ – Lạc quan', N'Tone sáng, cười trong giọng, hợp chủ đề tích cực', 2),
(N'Nữ – Hài hước', N'Nhanh, linh hoạt, hợp podcast giải trí', 2),
(N'Nữ – Tri thức', N'Chuẩn, trang trọng, hợp podcast học thuật', 2),
(N'Nữ – Cảm động', N'Nhẹ, có cảm xúc sâu, hợp kể chuyện thật', 2),
(N'Nữ – Quyền lực', N'Rõ, tự tin, hợp chủ đề lãnh đạo, truyền cảm hứng', 2),
(N'Nữ – Nghệ sĩ', N'Biểu cảm, hợp đọc thơ, văn, nghệ thuật', 2),
(N'Nữ – Phóng viên', N'Nhanh, rõ, linh hoạt, hợp tin tức, phỏng vấn', 2),
(N'Nữ – Lạnh lùng', N'Giọng đều, ít cảm xúc, hợp chủ đề khoa học, phân tích', 2),
(N'Nữ – Chất giọng miền Nam', N'Dịu, nhẹ, thân mật, hợp podcast đời sống', 2),
(N'Nữ – Chất giọng miền Bắc', N'Rõ chữ, tự nhiên, hợp chính luận, giáo dục', 2),
(N'Giọng kể truyện cổ tích', N'Nhấn nhá nhẹ, êm, hợp podcast cho trẻ em', 3),
(N'Giọng đọc tin tức', N'Chuẩn, dứt khoát, không cảm xúc', 3),
(N'Giọng thầy cô giáo', N'Nhiệt tình, rõ, dễ nghe, hợp chủ đề học tập', 3),
(N'Giọng thiền / mindfulness', N'Rất chậm, nhẹ, trầm, tạo cảm giác thư giãn', 3),
(N'Giọng chuyên gia', N'Tự tin, logic, hợp phân tích chuyên sâu', 3),
(N'Giọng hoạt hình', N'Tăng nhịp, vui nhộn, hợp giải trí, parody', 3),
(N'Giọng kể phim / trailer', N'Trầm, có lực, tạo kịch tính, hợp review phim', 3),
(N'Giọng tự sự', N'Tự nhiên, cảm xúc thật, như đang ghi nhật ký', 3),
(N'Giọng cặp đôi (song thoại)', N'Nam – nữ đối thoại tự nhiên, hợp talkshow', 3),
(N'Giọng trung tính AI / chatbot', N'Rõ, đều, dễ hiểu, hợp hướng dẫn kỹ thuật', 3);


-- =====================================================
-- PODCAST SERVICE DATABASE [Port: 8056]
-- =====================================================

-- PodcastEpisodeLicenseType table
INSERT INTO PodcastEpisodeLicenseType (id, name) VALUES
(1, N'Public Domain'),
(2, N'Creative Commons'),
(3, N'Copyrighted – With Permission'),
(4, N'Copyrighted – Paid License'),
(5, N'Original Content'),
(6, N'Adaptation License'),
(7, N'User-Submitted Content License'),
(8, N'Royalty-Free Content License'),
(9, N'Podsafe License');

-- PodcastEpisodePublishReviewSessionStatus table
INSERT INTO PodcastEpisodePublishReviewSessionStatus (id, name) VALUES
(1, N'Pending Review'),
(2, N'Discard'),
(3, N'Accepted'),
(4, N'Rejected');

-- PodcastIllegalContentType table
INSERT INTO PodcastIllegalContentType (id, name) VALUES
(1, N'Near-exact Duplicate Content (trùng 90%)'),
(2, N'Excessive Restricted Terms (nhiều restrict terms quét được)'),
(3, N'Hate Speech / Hate Content'),
(4, N'Harassment / Abusive Language'),
(5, N'Misleading or False Information'),
(6, N'Inappropriate or Explicit Sexual Content'),
(7, N'Violence / Graphical / Offensive Violence'),
(8, N'Self-Harm or Suicidal Content'),
(9, N'Privacy Violation (personal data exposure)'),
(10, N'Impersonation / False Identity');

-- PodcastChannelStatus table
INSERT INTO PodcastChannelStatus (id, name) VALUES
(1, N'Unpublished'),
(2, N'Published');

-- PodcastShowStatus table
INSERT INTO PodcastShowStatus (id, name) VALUES
(1, N'Draft'),
(2, N'Ready To Release'),
(3, N'Published'),
(4, N'Taken Down'),
(5, N'Removed');

-- PodcastEpisodeStatus table
INSERT INTO PodcastEpisodeStatus (id, name) VALUES
(1, N'Draft'),
(2, N'Pending Review'),
(3, N'Pending Edit Required'),
(4, N'Ready To Release'),
(5, N'Published'),
(6, N'Taken Down'),
(7, N'Removed'),
(8, N'Audio Processing');

-- PodcastEpisodeSubscriptionType table
INSERT INTO PodcastEpisodeSubscriptionType (id, name) VALUES
(1, N'Free'),
(2, N'Subscriber-Only'),
(3, N'Bonus'),
(4, N'Archive');

-- PodcastShowSubscriptionType table
INSERT INTO PodcastShowSubscriptionType (id, name) VALUES
(1, N'Free'),
(2, N'Subscriber only');

-- PodcastCategory table
INSERT INTO PodcastCategory (id, name, mainImageFileKey) VALUES
(1, N'True Crime', N'main_files/PodcastCategories/1/main_image.png'),
(2, N'Horror', N'main_files/PodcastCategories/2/main_image.png'),
(3, N'Society & Culture', N'main_files/PodcastCategories/3/main_image.png'),
(4, N'Psychology', N'main_files/PodcastCategories/4/main_image.png'),
(5, N'Philosophy', N'main_files/PodcastCategories/5/main_image.png'),
(6, N'History', N'main_files/PodcastCategories/6/main_image.png'),
(7, N'Comics', N'main_files/PodcastCategories/7/main_image.png');

-- PodcastSubCategory table
INSERT INTO PodcastSubCategory (id, name, categoryId) VALUES
(1, N'Serial Killers', 1),
(2, N'Unsolved Mysteries', 1),
(3, N'White Collar Crime', 1),
(4, N'Cybercrime', 1),
(5, N'International Crimes', 1),
(6, N'Organized Crime', 1),
(7, N'Miscarriage of Justice', 1),
(8, N'Prison Escapes', 1),
(9, N'Criminal Psychology', 1),
(10, N'Cold Case Files', 1),
(11, N'Police Corruption', 1),
(12, N'Asian Folklore Horror', 2),
(13, N'European Folklore Horror', 2),
(14, N'Creepypasta', 2),
(15, N'Urban Legends', 2),
(16, N'Supernatural', 2),
(17, N'Haunted Locations', 2),
(18, N'Possession & Exorcism', 2),
(19, N'Body Horror', 2),
(20, N'Psychological Horror', 2),
(21, N'Cosmic Horror', 2),
(22, N'Occult Rituals', 2),
(23, N'Heterodox Faith', 3),
(24, N'Horrific Cultures', 3),
(25, N'Dark Prejudices', 3),
(26, N'Mysterious Tribes', 3),
(27, N'Lost Civilizations', 3),
(28, N'Cultural Taboos', 3),
(29, N'Rituals & Ceremonies', 3),
(30, N'Urban Subcultures', 3),
(31, N'Media Influence', 3),
(32, N'Social Experiments', 3),
(33, N'Criminal Profiling', 4),
(34, N'Abnormal Psychology', 4),
(35, N'Cognitive Biases', 4),
(36, N'Fear & Phobia Studies', 4),
(37, N'Psychopathy & Sociopathy', 4),
(38, N'Mass Hysteria', 4),
(39, N'Dream & Subconscious', 4),
(40, N'Trauma & Recovery', 4),
(41, N'Mind Control & Suggestion', 4),
(42, N'Collective Behavior', 4),
(43, N'Existentialism', 5),
(44, N'Nihilism', 5),
(45, N'Moral Dilemmas', 5),
(46, N'Philosophy of Evil', 5),
(47, N'Death & Meaning', 5),
(48, N'Metaphysics & Reality', 5),
(49, N'Human Nature', 5),
(50, N'Knowledge & Perception', 5),
(51, N'Ethics & Responsibility', 5),
(52, N'Dualism & Consciousness', 5),
(53, N'Ancient Civilizations', 6),
(54, N'Medieval Inquisitions', 6),
(55, N'War Crimes', 6),
(56, N'Revolutions & Rebellions', 6),
(57, N'Historical Mysteries', 6),
(58, N'Lost Empires', 6),
(59, N'Colonial Atrocities', 6),
(60, N'Dark Ages Legends', 6),
(61, N'Prophecies & Predictions', 6),
(62, N'Archaeological Discoveries', 6),
(63, N'Dark Graphic Novels', 7),
(64, N'Psychological Manga', 7),
(65, N'Historical Comics', 7),
(66, N'True Crime Adaptations', 7),
(67, N'Supernatural Series', 7),
(68, N'Horror Anthologies', 7),
(69, N'Detective Stories', 7),
(70, N'Philosophical Comics', 7),
(71, N'Gothic Visual Tales', 7),
(72, N'Parody & Satirical Comics', 7);


-- =====================================================
-- SUBSCRIPTION SERVICE DATABASE [Port: 8066]
-- =====================================================

-- SubscriptionCycleType table
INSERT INTO SubscriptionCycleType (id, name) VALUES
(1, N'Monthly'),
(2, N'Annually');

-- PodcastSubscriptionBenefit table
INSERT INTO PodcastSubscriptionBenefit (id, name) VALUES
(1, N'Non-Quota Listening'),
(2, N'Subscriber-Only Shows'),
(3, N'Subscriber-Only Episodes'),
(4, N'Bonus Episodes'),
(5, N'Shows/Episodes Early Access'),
(6, N'Archive Episodes Access');


-- =====================================================
-- MODERATION SERVICE DATABASE [Port: 8071]
-- =====================================================

-- PodcastBuddyReportType table
INSERT INTO PodcastBuddyReportType (id, name) VALUES
(1, N'Scam / Fraud'),
(2, N'Spam'),
(3, N'Harassment / Abusive Behavior'),
(4, N'Hate Speech'),
(5, N'Misinformation / False Claims'),
(6, N'Copyright Violation'),
(7, N'Impersonation'),
(8, N'Privacy Violation'),
(9, N'Inappropriate Content'),
(10, N'Other (please specify)');

-- PodcastShowReportType table
INSERT INTO PodcastShowReportType (id, name) VALUES
(1, N'Spam'),
(2, N'Offensive or Obscene Content'),
(3, N'Hate Speech'),
(4, N'Misleading or False Information'),
(5, N'Harassment / Abusive Content'),
(6, N'Impersonation'),
(7, N'Privacy Violation'),
(8, N'Other (please specify)');

-- PodcastEpisodeReportType table
INSERT INTO PodcastEpisodeReportType (id, name) VALUES
(1, N'Spam'),
(2, N'Inappropriate or Explicit Language or Content'),
(3, N'Hate Speech'),
(4, N'False or Misleading Information'),
(5, N'Harassment / Abusive Content'),
(6, N'Impersonation'),
(7, N'Privacy Violation'),
(8, N'Other (please specify)');

-- DMCAAccusationStatus table
INSERT INTO DMCAAccusationStatus (id, name) VALUES
(1, N'Pending DMCA Notice Review'),
(2, N'Invalid DMCA Notice'),
(3, N'Valid DMCA Notice'),
(4, N'Invalid Counter Notice'),
(5, N'Valid Counter Notice'),
(6, N'Invalid Lawsuit Proof'),
(7, N'Valid Lawsuit Proof'),
(8, N'Podcaster Lawsuit Win'),
(9, N'Accuser Lawsuit Win'),
(10, N'Unresolved Dismissed'),
(11, N'Direct Resolve Dismissed');

-- DMCAAccusationConclusionReportType table
INSERT INTO DMCAAccusationConclusionReportType (id, name) VALUES
(1, N'Invalid DMCA Notice'),
(2, N'Invalid Counter Notice'),
(3, N'Invalid Lawsuit Proof'),
(4, N'Podcaster Lawsuit Win'),
(5, N'Accuser Lawsuit Win');


-- =====================================================
-- TRANSACTION SERVICE DATABASE [Port: 8076]
-- =====================================================

-- TransactionType table
INSERT INTO TransactionType (id, name) VALUES
(1, N'Account Balance Deposits'),
(2, N'Account Balance Withdrawal'),
(3, N'Booking Deposit'),
(4, N'Booking Deposit Refund'),
(5, N'Booking Deposit Compensation'),
(6, N'Booking Pay The Rest'),
(7, N'Booking Additional Storage Purchase'),
(8, N'Customer Subscription Cycle Payment'),
(9, N'Customer Subscription Cycle Payment Refund'),
(10, N'System Subscription Income'),
(11, N'Podcaster Subscription Income'),
(12, N'System Booking Income'),
(13, N'Podcaster Booking Income');

-- TransactionStatus table
INSERT INTO TransactionStatus (id, name) VALUES
(1, N'Pending'),
(2, N'Success'),
(3, N'Cancelled'),
(4, N'Error');