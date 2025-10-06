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