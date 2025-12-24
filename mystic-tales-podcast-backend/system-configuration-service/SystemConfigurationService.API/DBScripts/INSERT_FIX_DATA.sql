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
INSERT INTO BookingConfig (configProfileId, profitRate, depositRate, podcastTrackPreviewListenSlot, previewResponseAllowedDays, producingRequestResponseAllowedDays, chatRoomExpiredHours, chatRoomFileMessageExpiredHours, freeInitialBookingStorageSize, singleStorageUnitPurchasePrice) VALUES
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