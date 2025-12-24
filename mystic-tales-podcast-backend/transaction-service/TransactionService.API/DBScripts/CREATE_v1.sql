-- =====================================================
-- TRANSACTION SERVICE DATABASE [Port: 8076]
-- =====================================================

-- TransactionType table
CREATE TABLE TransactionType (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- TransactionStatus table
CREATE TABLE TransactionStatus (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- AccountBalanceTransaction table
CREATE TABLE AccountBalanceTransaction (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    accountId INT NOT NULL,
    orderCode NVARCHAR(MAX) NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    transactionTypeId INT NOT NULL,
    transactionStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (transactionTypeId) REFERENCES TransactionType(id),
    FOREIGN KEY (transactionStatusId) REFERENCES TransactionStatus(id)
);

-- AccountBalanceWithdrawalRequest table
CREATE TABLE AccountBalanceWithdrawalRequest
(
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    accountId INT NOT NULL, -- Không có FK vì Account nằm trong UserService
    amount DECIMAL(18,2) NOT NULL,
    transferReceiptImageFileKey NVARCHAR(MAX) NULL,
    rejectReason NVARCHAR(MAX) NULL,
    isRejected BIT NULL, -- NULL: đang chờ, 1: rejected, 0: approved
    completedAt DATETIME NULL,
    createdAt DATETIME NOT NULL DEFAULT GETDATE(),
    updatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

-- PodcastSubscriptionTransaction table
CREATE TABLE PodcastSubscriptionTransaction (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    podcastSubscriptionRegistrationId UNIQUEIDENTIFIER NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    profit DECIMAL(18,2) NULL,
    transactionTypeId INT NOT NULL,
    transactionStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (transactionTypeId) REFERENCES TransactionType(id),
    FOREIGN KEY (transactionStatusId) REFERENCES TransactionStatus(id)
);

-- MemberSubscriptionTransaction table
CREATE TABLE MemberSubscriptionTransaction (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    memberSubscriptionRegistrationId UNIQUEIDENTIFIER NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    transactionTypeId INT NOT NULL,
    transactionStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (transactionTypeId) REFERENCES TransactionType(id),
    FOREIGN KEY (transactionStatusId) REFERENCES TransactionStatus(id)
);

-- BookingTransaction table
CREATE TABLE BookingTransaction (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    bookingId INT NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    profit DECIMAL(18,2) NULL,
    transactionTypeId INT NOT NULL,
    transactionStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (transactionTypeId) REFERENCES TransactionType(id),
    FOREIGN KEY (transactionStatusId) REFERENCES TransactionStatus(id)
);

-- BookingStorageTransaction table
CREATE TABLE BookingStorageTransaction (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    accountId INT NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    transactionTypeId INT NOT NULL,
    transactionStatusId INT NOT NULL,
    storageSize FLOAT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (transactionTypeId) REFERENCES TransactionType(id),
    FOREIGN KEY (transactionStatusId) REFERENCES TransactionStatus(id)
);