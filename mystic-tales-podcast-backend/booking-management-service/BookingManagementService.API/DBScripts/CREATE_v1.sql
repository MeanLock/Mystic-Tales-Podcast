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
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME))
);

-- BookingRequirementAttachFile table
CREATE TABLE BookingRequirementAttachFile (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    bookingId INT NOT NULL,
    attachFileKey NVARCHAR(MAX) NOT NULL,
    description NVARCHAR(MAX) NOT NULL DEFAULT '',
    FOREIGN KEY (bookingId) REFERENCES Booking(id)
);

-- BookingNegotiation table
CREATE TABLE BookingNegotiation (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    bookingId INT NOT NULL,
    note NVARCHAR(MAX) NOT NULL DEFAULT '',
    deadline DATE NULL,
    price DECIMAL(18,2) NULL,
    demoAudioRequired BIT NOT NULL DEFAULT 0,
    demoAudioFileKey NVARCHAR(MAX) NULL,
    isCompleted BIT NOT NULL DEFAULT 0,
    isFromCustomer BIT NOT NULL DEFAULT 1,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    FOREIGN KEY (bookingId) REFERENCES Booking(id)
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
    FOREIGN KEY (bookingId) REFERENCES Booking(id),
    FOREIGN KEY (bookingProducingRequestId) REFERENCES BookingProducingRequest(id)
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