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
(11, N'Podcaster Subscription Income'),
(12, N'System Booking Income'),
(13, N'Podcaster Booking Income');

-- TransactionStatus
INSERT INTO TransactionStatus (id, name) VALUES
(1, N'Pending'),
(2, N'Success'),
(3, N'Cancelled'),
(4, N'Error');