-- =====================================================
-- USER SERVICE DATABASE [Port: 8046]
-- =====================================================

-- Role
INSERT INTO Role (id, name) VALUES
(1, N'Customer'),
(2, N'Staff'),
(3, N'Admin');

-- NotificationType
INSERT INTO NotificationType (id, name) VALUES
(1, N'Content Notification'),
(2, N'Payment Cycle Notification'),
(3, N'Comment Notification'),
(4, N'Withdrawal-Request Status Notification'),
(5, N'Account Balance Change Notification'),
(6, N'Booking Status Notification'),
(7, N'Moderation Status Notification'),
(8, N'Report Notification');

-- =====================================================
-- SYSTEM CONFIGURATION SERVICE DATABASE [Port: 8051]
-- =====================================================

-- SystemConfigProfile
INSERT INTO SystemConfigProfile (id, name, isActive) VALUES
(1, N'Default Configuration', 1);

-- PodcastSubscriptionConfig
INSERT INTO PodcastSubscriptionConfig (configProfileId, subscriptionCycleTypeId, profitRate, incomeTakenDelayDays) VALUES
(1, 1, 0.2, 7),  -- Monthly
(1, 2, 0.2, 7);  -- Annually

-- PodcastSuggestionConfig
INSERT INTO PodcastSuggestionConfig (configProfileId, behaviorLookbackDayCount, minChannelQuery, minShowQuery) VALUES
(1, 30, 10, 10);

-- BookingConfig
INSERT INTO BookingConfig (configProfileId, profitRate, depositRate, podcastTrackPreviewListenSlot, previewResponseAllowedDays, producingRequestResponseAllowedDays, chatRoomExpiredHours, chatRoomFileMessageExpiredHours, FreeInitialBookingStorageSize, singleStorageUnitPurchasePrice) VALUES
(1, 0.3, 0.5, 3, 30, 1, 5, 5, 1, 5000);

-- AccountConfig
INSERT INTO AccountConfig (configProfileId, violationPointDecayHours, podcastListenSlotThreshold, podcastListenSlotRecoverySeconds) VALUES
(1, 24, 9, 18000);

-- AccountViolationLevelConfig
INSERT INTO AccountViolationLevelConfig (configProfileId, violationLevel, violationPointThreshold, punishmentDays) VALUES
(1, 0, 0, 0),
(1, 1, 20, 7),
(1, 2, 50, 14),
(1, 3, 200, 30),
(1, 4, 500, 365);

-- ReviewSessionConfig
INSERT INTO ReviewSessionConfig (configProfileId, podcastBuddyUnResolvedReportStreak, podcastShowUnResolvedReportStreak, podcastEpisodeUnResolvedReportStreak, podcastEpisodePublishEditRequirementExpiredHours) VALUES
(1, 5, 5, 5, 24);

-- PodcastRestrictedTerm
INSERT INTO PodcastRestrictedTerm (term) VALUES
(N'f@ck'), (N'f u c k'), (N'fuq'), (N'fvk'), (N'phuck'),
(N'motherf***er'), (N'mofo'), (N'mf''er'), (N'a**hole'), (N'@sshole'),
(N'arsehole'), (N'arse'), (N'a55hole'), (N'dickhead'), (N'd!ck'),
(N'd1ck'), (N'prick'), (N'wanker'), (N'tosser'), (N'douchebag'),
(N'douche'), (N'scumbag'), (N'scum'), (N'jackass'), (N'dumbass'),
(N'smartass'), (N'dipshit'), (N'bullsh*t'), (N'bullshit'), (N'sh!t'),
(N'sh1t'), (N'piss'), (N'piss off'), (N'twat'), (N'bollocks'),
(N'bugger'), (N'bloody hell'), (N'goddamn'), (N'damn'), (N'hoe'),
(N'ho'), (N'skank'), (N'tramp'), (N'tart'), (N'wh0re'),
(N'wh0r3'), (N'biatch'), (N'b!tch'), (N'btch'), (N'son of a bitch'),
(N'SOB'), (N'piece of sh*t'), (N'POS'), (N'retard'), (N'r*tard'),
(N'retarded'), (N'r3tard'), (N'spastic'), (N'spazz'), (N'moron'),
(N'imbecile'), (N'idiot'), (N'dumb'), (N'stupid'), (N'dyke'),
(N'd*ke'), (N'homo'), (N'queer'), (N'sissy'), (N'pansy'),
(N'shemale'), (N'he-she'), (N'ladyboy'), (N'fudgepacker'), (N'fairy'),
(N'poof'), (N'poofter'), (N'transvestite'), (N'spic'), (N'wetback'),
(N'beaner'), (N'greaser'), (N'redskin'), (N'injun'), (N'savage'),
(N'jungle bunny'), (N'porch monkey'), (N'raghead'), (N'sand nigger'), (N'towelhead'),
(N'curry muncher'), (N'zipperhead'), (N'slope'), (N'yid'), (N'hebe'),
(N'sheeny'), (N'gypsy'), (N'kraut'), (N'frog'), (N'limey'),
(N'paddy'), (N'mick'), (N'dago'), (N'wop'), (N'polack'),
(N'honky'), (N'cracker'), (N'white trash'), (N'trailer trash'), (N'infidel'),
(N'christ-killer'), (N'jap'), (N'nip'), (N'chinaman'), (N'eskimo'),
(N'abo'), (N'boong'), (N'coolie'), (N'cmm'), (N'đmm'),
(N'địt mẹ'), (N'địt bố'), (N'đụ mẹ'), (N'đụ má'), (N'đụ'),
(N'dm'), (N'đm'), (N'dcm'), (N'đcm'), (N'dmm'),
(N'vcl'), (N'vkl'), (N'clgt'), (N'vl'), (N'cc'),
(N'ccc'), (N'c*c'), (N'cứt'), (N'ỉa'), (N'đái'),
(N'rắm'), (N'thối tha'), (N'óc chó'), (N'đầu bò'), (N'đầu đất'),
(N'não chó'), (N'não cá vàng'), (N'não phẳng'), (N'ngu dốt'), (N'đần độn'),
(N'khờ dại'), (N'khùng'), (N'điên khùng'), (N'thần kinh'), (N'tâm thần'),
(N'chậm phát triển'), (N'tật nguyền'), (N'què'), (N'đui'), (N'mù'),
(N'câm'), (N'điếc'), (N'bại não'), (N'bẩn thỉu'), (N'hôi hám'),
(N'dơ dáy'), (N'súc vật'), (N'thú vật'), (N'đồ chó má'), (N'đồ heo'),
(N'đồ lợn'), (N'đồ bò'), (N'đồ trâu'), (N'đồ khỉ'), (N'đồ rác'),
(N'cặn bã'), (N'tiện nhân'), (N'đê tiện'), (N'bỉ ổi'), (N'vô học'),
(N'mất dạy'), (N'mất nết'), (N'hỗn láo'), (N'bố láo'), (N'láo toét'),
(N'láo lếu'), (N'láo chó'), (N'khốn nạn'), (N'khốn kiếp'), (N'chết tiệt'),
(N'đồ hèn'), (N'nhục'), (N'nhục mặt'), (N'hạ đẳng'), (N'mạt hạng'),
(N'rẻ rách'), (N'phèn'), (N'nhà quê'), (N'con hoang'), (N'đồ mồ côi'),
(N'đồ phản phúc'), (N'đồ ăn hại'), (N'đồ phế vật'), (N'ăn bám'), (N'đào mỏ'),
(N'lẳng lơ'), (N'lăng loàn'), (N'dâm đãng'), (N'dâm phụ'), (N'con phò'),
(N'con cave'), (N'điếm'), (N'con điếm'), (N'đĩ thõa'), (N'đĩ mẹ'),
(N'đĩ chó'), (N'bú c*'), (N'liếm đít'), (N'địt m mày'), (N'cút m mày'),
(N'đ!t mẹ'), (N'đ*t mẹ'), (N'djt mẹ'), (N'đjt mẹ'), (N'dit me'),
(N'd1t me'), (N'd.i.t mẹ'), (N'd i t mẹ'), (N'djt m*'), (N'đ!t m*'),
(N'djt mạ*'), (N'đjt mạ*'), (N'dit m* may'), (N'd1t m* m*y'), (N'đ!t m* m*y'),
(N'djt m**y'), (N'djt m* m*'), (N'đt mạy'), (N'dit m a y'), (N'djt m a y'),
(N'đụ m*'), (N'đụ mạ*'), (N'đụ mịa'), (N'du me'), (N'du ma'),
(N'd u m e'), (N'đu m*'), (N'd.u.m.e'), (N'du m*'), (N'đụ m`.'),
(N'đ*o'), (N'd*o'), (N'deo'), (N'đéo m*'), (N'đ*o mẹ'),
(N'deo me'), (N'd3o'), (N'đ3o'), (N'd. e . o'), (N'đ**o'),
(N'lon'), (N'l*n'), (N'l0n'), (N'l—n'), (N'l•n'),
(N'l0*n'), (N'ln'), (N'l. o . n'), (N'l_on'), (N'l-0-n'),
(N'cac'), (N'c*c'), (N'c@c'), (N'cạk'), (N'cak'),
(N'c4c'), (N'c/\c'), (N'c.a.c'), (N'c a c'), (N'cặk'),
(N'oc cho'), (N'0c ch0'), (N'o c c h o'), (N'oc-cho'), (N'o.c.c.h.o'),
(N'o''c cho'), (N'ọc chó'), (N'óc ch0'), (N'oc ch0'), (N'0c chó'),
(N'mat day'), (N'matday'), (N'm@t d@y'), (N'md dy'), (N'm a t d a y'),
(N'mất-dạy'), (N'mats day'), (N'mât day'), (N'm a t-d @ y'), (N'm4t d@y'),
(N'khon nan'), (N'kh0n n@n'), (N'khn nn'), (N'khon-nan'), (N'khốn-nạn'),
(N'khon kiep'), (N'kh0n ki3p'), (N'khn kip'), (N'khon-kiep'), (N'khốn-kiếp'),
(N'ngu dot'), (N'ng*u d0t'), (N'n g u d o t'), (N'ngu-dốt'), (N'ngu***dốt'),
(N'dan don'), (N'd@n d0n'), (N'đn đn'), (N'đần-độn'), (N'd a n d o n'),
(N'suc vat'), (N'sc vt'), (N's-uc v-at'), (N's u c v a t'), (N'phe vat'),
(N'ph* v*t'), (N'phe-vat'), (N'can ba'), (N'cn b'), (N'can-ba'),
(N'con pho'), (N'c0n ph0'), (N'cn ph'), (N'con-pho'), (N'c0n cave'),
(N'con c@ve'), (N'con c*v3'), (N'd!em'), (N'di3m'), (N'd*i');

-- =====================================================
-- BOOKING MANAGEMENT SERVICE DATABASE [Port: 8056]
-- =====================================================

-- BookingStatus
INSERT INTO BookingStatus (id, name) VALUES
(1, N'Quotation Under Negotiation'),
(2, N'Quotation Dealing'),
(3, N'Quotation Rejected'),
(4, N'Quotation Cancelled'),
(5, N'Producing'),
(6, N'Track Previewing'),
(7, N'Producing Requested'),
(8, N'Completed'),
(9, N'Cancelled Automatically'),
(10, N'Cancelled Manually');

-- BookingOptionalManualCancelReason
INSERT INTO BookingOptionalManualCancelReason (id, name) VALUES
(1, N'Podcast buddy did not respond in time'),
(2, N'Delay in delivery timeline'),
(3, N'I found another podcaster'),
(4, N'Too expensive / Price not acceptable'),
(5, N'Quality concerns (voice, style, demo mismatch)'),
(6, N'Technical issues with audio or platform'),
(7, N'Personal reasons (no longer need the podcast)'),
(8, N'Misunderstanding about booking terms');

-- =====================================================
-- PODCAST SERVICE DATABASE [Port: 8061]
-- =====================================================

-- PodcastEpisodeLicenseType
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

-- PodcastIllegalContentType
INSERT INTO PodcastIllegalContentType (id, name) VALUES
(1, N'Near-exact Duplicate Content'),
(2, N'Excessive Restricted Terms'),
(3, N'Hate Speech / Hate Content'),
(4, N'Harassment / Abusive Language'),
(5, N'Misleading or False Information'),
(6, N'Inappropriate or Explicit Sexual Content'),
(7, N'Violence / Graphical / Offensive Violence'),
(8, N'Self-Harm or Suicidal Content'),
(9, N'Privacy Violation (personal data exposure)'),
(10, N'Impersonation / False Identity');

-- PodcastChannelStatus
INSERT INTO PodcastChannelStatus (id, name) VALUES
(1, N'Unpublished'),
(2, N'Published');

-- PodcastShowStatus
INSERT INTO PodcastShowStatus (id, name) VALUES
(1, N'Draft'),
(2, N'Ready to Release'),
(3, N'Published'),
(4, N'Taken Down'),
(5, N'Removed');

-- PodcastEpisodeStatus
INSERT INTO PodcastEpisodeStatus (id, name) VALUES
(1, N'Draft'),
(2, N'Pending Review'),
(3, N'Pending Edit Required'),
(4, N'Ready to Release'),
(5, N'Published'),
(6, N'Taken Down'),
(7, N'Removed');

-- PodcastEpisodePublishReviewSessionStatus
INSERT INTO PodcastEpisodePublishReviewSessionStatus (id, name) VALUES
(1, N'Pending Review'),
(2, N'Discard'),
(3, N'Accepted'),
(4, N'Rejected');

-- PodcastEpisodeSubscriptionType
INSERT INTO PodcastEpisodeSubscriptionType (id, name) VALUES
(1, N'Free'),
(2, N'Subscriber-Only'),
(3, N'Bonus'),
(4, N'Archive');

-- PodcastShowsSubscriptionType
INSERT INTO PodcastShowsSubscriptionType (id, name) VALUES
(1, N'Free'),
(2, N'Subscriber only');

-- PodcastCategory
INSERT INTO PodcastCategory (id, name) VALUES
(1, N'True Crime'),
(2, N'Horror'),
(3, N'Society & Culture'),
(4, N'Psychology'),
(5, N'Philosophy'),
(6, N'History'),
(7, N'Comics');

-- PodcastSubCategory
INSERT INTO PodcastSubCategory (id, name, podcastCategoryId) VALUES
-- True Crime (1)
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

-- Horror (2)
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

-- Society & Culture (3)
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

-- Psychology (4)
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

-- Philosophy (5)
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

-- History (6)
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

-- Comics (7)
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

-- SubscriptionCycleType
INSERT INTO SubscriptionCycleType (id, name) VALUES
(1, N'Monthly'),
(2, N'Annually');

-- PodcastSubscriptionBenefit
INSERT INTO PodcastSubscriptionBenefit (id, name) VALUES
(1, N'Non-Quota Listening'),
(2, N'Subscriber-Only Shows'),
(3, N'Subscriber-Only Episodes'),
(4, N'Bonus Episodes'),
(5, N'Shows/Episodes Early Access'),
(6, N'Archive Episodes Access');

-- MemberSubscriptionBenefit
INSERT INTO MemberSubscriptionBenefit (id, name) VALUES
(1, N'Non-Quota Listening');

-- =====================================================
-- MODERATION SERVICE DATABASE [Port: 8071]
-- =====================================================

-- PodcastBuddyReportType
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

-- PodcastShowReportType
INSERT INTO PodcastShowReportType (id, name) VALUES
(1, N'Spam'),
(2, N'Offensive or Obscene Content'),
(3, N'Hate Speech'),
(4, N'Misleading or False Information'),
(5, N'Harassment / Abusive Content'),
(6, N'Impersonation'),
(7, N'Privacy Violation'),
(8, N'Other (please specify)');

-- PodcastEpisodeReportType
INSERT INTO PodcastEpisodeReportType (id, name) VALUES
(1, N'Spam'),
(2, N'Inappropriate or Explicit Language or Content'),
(3, N'Hate Speech'),
(4, N'False or Misleading Information'),
(5, N'Harassment / Abusive Content'),
(6, N'Impersonation'),
(7, N'Privacy Violation'),
(8, N'Other (please specify)');

-- DMCAAccusationStatus
INSERT INTO DMCAAccusationStatus (id, name) VALUES
(1, N'Pending'),
(2, N'Reviewing'),
(3, N'Rejected'),
(4, N'Take Down Permanent'),
(5, N'Close Withdrawn'),
(6, N'Counter Reviewing'),
(7, N'Lawsuit Pending'),
(8, N'Lawsuit Filed'),
(9, N'Lawsuit Verified'),
(10, N'DMCA Wins'),
(11, N'Counter Wins');

-- =====================================================
-- TRANSACTION SERVICE DATABASE [Port: 8076]
-- =====================================================

-- TransactionType
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
(11, N'Podcaster Subscription Income');

-- TransactionStatus
INSERT INTO TransactionStatus (id, name) VALUES
(1, N'Pending'),
(2, N'Success'),
(3, N'Cancelled'),
(4, N'Error');