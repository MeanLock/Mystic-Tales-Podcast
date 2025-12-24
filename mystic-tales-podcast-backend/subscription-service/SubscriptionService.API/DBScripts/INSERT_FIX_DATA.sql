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