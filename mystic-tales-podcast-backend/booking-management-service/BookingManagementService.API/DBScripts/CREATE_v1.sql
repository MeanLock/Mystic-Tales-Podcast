-- =====================================================
-- BOOKING MANAGEMENT SERVICE DATABASE [Port: 8056]
-- =====================================================

-- BookingStatus table
CREATE TABLE BookingStatus (
    id INT PRIMARY KEY,
    name NVARCHAR(50) NOT NULL
);

-- BookingOptionalManualCancelReason table
CREATE TABLE BookingOptionalManualCancelReason (
    id INT PRIMARY KEY,
    name NVARCHAR(250) NOT NULL
);

-- Booking table
CREATE TABLE Booking (
    id INT IDENTITY(1,1) PRIMARY KEY,
    title NVARCHAR(250) NOT NULL,
    description NVARCHAR(MAX) NOT NULL DEFAULT '',
    accountId INT NOT NULL,
    podcastBuddyId INT NOT NULL,
    price DECIMAL(18,2) NULL,
    deadline DATE NULL,
    demoAudioFileKey NVARCHAR(MAX) NULL,
    bookingManualCancelledReason NVARCHAR(MAX) NULL,
    bookingAutoCancelReason NVARCHAR(MAX) NULL,
    assignedStaffId INT NULL,
    customerBookingCancelDepositRefundRate FLOAT NULL DEFAULT NULL,
    podcastBuddyBookingCancelDepositRefundRate FLOAT NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME))
);

-- PodcastBookingToneCategory table
CREATE TABLE PodcastBookingToneCategory (
    id INT PRIMARY KEY,
    name NVARCHAR(100) NOT NULL
);

-- PodcastBookingTone table
CREATE TABLE PodcastBookingTone (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(500) NULL DEFAULT NULL,
    podcastBookingToneCategoryId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    deletedAt DATETIME NULL DEFAULT NULL,
    FOREIGN KEY (podcastBookingToneCategoryId) REFERENCES PodcastBookingToneCategory(id)
);

-- PodcastBuddyBookingTone table
CREATE TABLE PodcastBuddyBookingTone (
    podcasterId INT NOT NULL,
    podcastBookingToneId UNIQUEIDENTIFIER NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    PRIMARY KEY (podcasterId, podcastBookingToneId),
    FOREIGN KEY (podcastBookingToneId) REFERENCES PodcastBookingTone(id)
);

-- BookingRequirement table
CREATE TABLE BookingRequirement (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    bookingId INT NOT NULL,
    name NVARCHAR(MAX) NOT NULL DEFAULT '',
    description NVARCHAR(MAX) NOT NULL DEFAULT '',
    requirementDocumentFileKey NVARCHAR(MAX) NOT NULL,
    [order] INT NOT NULL,
    wordCount INT NOT NULL,
    podcastBookingToneId UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY (bookingId) REFERENCES Booking(id),
    FOREIGN KEY (podcastBookingToneId) REFERENCES PodcastBookingTone(id)
);

-- BookingStatusTracking table
CREATE TABLE BookingStatusTracking (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    bookingId INT NOT NULL,
    bookingStatusId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (bookingId) REFERENCES Booking(id),
    FOREIGN KEY (bookingStatusId) REFERENCES BookingStatus(id)
);

-- BookingProducingRequest table
CREATE TABLE BookingProducingRequest (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    bookingId INT NOT NULL,
    note NVARCHAR(MAX) NOT NULL DEFAULT '',
    deadline DATE NOT NULL,
    isAccepted BIT NULL,
    finishedAt DATETIME NULL DEFAULT NULL,
    rejectReason NVARCHAR(MAX) NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (bookingId) REFERENCES Booking(id)
);

-- BookingPodcastTrack table
CREATE TABLE BookingPodcastTrack (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    bookingId INT NOT NULL,
    bookingProducingRequestId UNIQUEIDENTIFIER NOT NULL,
    audioFileKey NVARCHAR(MAX) NOT NULL,
    audioFileSize FLOAT NOT NULL,
    audioLength INT NOT NULL,
    remainingPreviewListenSlot INT NOT NULL,
    bookingRequirementId UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY (bookingId) REFERENCES Booking(id),
    FOREIGN KEY (bookingProducingRequestId) REFERENCES BookingProducingRequest(id),
    FOREIGN KEY (bookingRequirementId) REFERENCES BookingRequirement(id)
);

-- BookingProducingRequestPodcastTrackToEdit table
CREATE TABLE BookingProducingRequestPodcastTrackToEdit (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    bookingProducingRequestId UNIQUEIDENTIFIER NOT NULL,
    bookingPodcastTrackId UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY (bookingProducingRequestId) REFERENCES BookingProducingRequest(id),
    FOREIGN KEY (bookingPodcastTrackId) REFERENCES BookingPodcastTrack(id)
);

-- BookingChatRoom table
CREATE TABLE BookingChatRoom (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    bookingId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (bookingId) REFERENCES Booking(id)
);

-- BookingChatMessages table
CREATE TABLE BookingChatMessages (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    text NVARCHAR(MAX) NULL,
    audioFileKey NVARCHAR(MAX) NULL,
    chatRoomId UNIQUEIDENTIFIER NOT NULL,
    senderId INT NOT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (chatRoomId) REFERENCES BookingChatRoom(id)
);

-- BookingChatMember table
CREATE TABLE BookingChatMember (
    chatRoomId UNIQUEIDENTIFIER NOT NULL,
    accountId INT NOT NULL,
    PRIMARY KEY (chatRoomId, accountId),
    FOREIGN KEY (chatRoomId) REFERENCES BookingChatRoom(id)
);