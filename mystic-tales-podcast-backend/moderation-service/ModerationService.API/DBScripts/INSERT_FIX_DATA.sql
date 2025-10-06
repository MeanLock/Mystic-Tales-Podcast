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