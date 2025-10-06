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