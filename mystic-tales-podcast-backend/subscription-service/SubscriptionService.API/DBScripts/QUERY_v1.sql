select * from  podcastSubscription
select * from  PodcastSubscriptionBenefitMapping
select * from  PodcastSubscriptionCycleTypePrice
select * from  PodcastSubscriptionRegistration

Drop table PodcastSubscriptionRegistrationBenefit
Drop table PodcastSubscriptionRegistration
Drop table MemberSubscriptionRegistration

SELECT
    *
FROM
    INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE
    TABLE_NAME = 'PodcastSubscriptionCycleTypePrice' or TABLE_NAME = 'PodcastSubscriptionBenefitMapping' or TABLE_NAME = 'PodcastSubscriptionRegistration';

FK__PodcastSu__podca__1CBC4616 = PodcastSubscriptionRegistration
FK__PodcastSu__podca__5812160E = PodcastSubscriptionCycleTypePrice
FK__PodcastSu__podca__5DCAEF64 = PodcastSubscriptionBenefitMapping



SELECT 
    fk.name AS constraint_name,   
	o2.name AS to_table,
    o1.name AS from_table,
    s2.name AS to_schema
FROM sys.foreign_keys fk
INNER JOIN sys.objects o1 ON fk.parent_object_id = o1.object_id
INNER JOIN sys.schemas s1 ON o1.schema_id = s1.schema_id
INNER JOIN sys.objects o2 ON fk.referenced_object_id = o2.object_id
INNER JOIN sys.schemas s2 ON o2.schema_id = s2.schema_id
WHERE o1.name IN (
    'PodcastSubscriptionRegistration',
    'PodcastSubscriptionCycleTypePrice',
    'PodcastSubscriptionBenefitMapping'
)
ORDER BY o1.name;





-- Bý?c 1: T?o b?ng m?i v?i IDENTITY
CREATE TABLE PodcastSubscription_New (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(250) NOT NULL,
    description NVARCHAR(MAX) NOT NULL DEFAULT '',
    podcastChannelId UNIQUEIDENTIFIER NULL,
    podcastShowId UNIQUEIDENTIFIER NULL,
    isActive BIT NOT NULL DEFAULT 0,
    currentVersion INT NOT NULL,
    deletedAt DATETIME NULL DEFAULT NULL,
    createdAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME)),
    updatedAt DATETIME NOT NULL DEFAULT (CAST(SYSDATETIMEOFFSET() AT TIME ZONE 'N. Central Asia Standard Time' AS DATETIME))
);

-- Bý?c 2: Chuy?n d? li?u t? b?ng c? sang b?ng m?i (n?u có d? li?u)
IF EXISTS (SELECT 1 FROM PodcastSubscription)
BEGIN
    SET IDENTITY_INSERT PodcastSubscription_New ON;
    INSERT INTO PodcastSubscription_New (id, name, description, podcastChannelId, podcastShowId, isActive, currentVersion, deletedAt, createdAt, updatedAt)
    SELECT id, name, description, podcastChannelId, podcastShowId, isActive, currentVersion, deletedAt, createdAt, updatedAt
    FROM PodcastSubscription;
    SET IDENTITY_INSERT PodcastSubscription_New OFF;
END

-- Bý?c 3: Xóa các ràng bu?c Foreign Key t? các b?ng liên quan
ALTER TABLE PodcastSubscriptionRegistration 
DROP CONSTRAINT FK__PodcastSu__podca__1CBC4616;

ALTER TABLE PodcastSubscriptionCycleTypePrice 
DROP CONSTRAINT FK__PodcastSu__podca__5812160E;

ALTER TABLE PodcastSubscriptionBenefitMapping 
DROP CONSTRAINT FK__PodcastSu__podca__5DCAEF64;

-- Bý?c 4: Xóa b?ng c?
DROP TABLE PodcastSubscription;

-- Bý?c 5: Ð?i tên b?ng m?i thành tên b?ng c?
EXEC sp_rename 'PodcastSubscription_New', 'PodcastSubscription';

-- Bý?c 6: Set IDENTITY seed v? 50
DBCC CHECKIDENT ('PodcastSubscription', RESEED, 50);

-- Bý?c 7: T?o l?i các Foreign Key constraints v?i tên m?i
ALTER TABLE PodcastSubscriptionRegistration
ADD CONSTRAINT FK__PodcastSu__podca__1CBC4616
FOREIGN KEY (podcastSubscriptionId) REFERENCES PodcastSubscription(id);

ALTER TABLE PodcastSubscriptionCycleTypePrice
ADD CONSTRAINT FK__PodcastSu__podca__5812160E
FOREIGN KEY (podcastSubscriptionId) REFERENCES PodcastSubscription(id);

ALTER TABLE PodcastSubscriptionBenefitMapping
ADD CONSTRAINT FK__PodcastSu__podca__5DCAEF64
FOREIGN KEY (podcastSubscriptionId) REFERENCES PodcastSubscription(id);

-- Ki?m tra k?t qu?
SELECT IDENT_CURRENT('PodcastSubscription') AS CurrentIdentity,
       IDENT_SEED('PodcastSubscription') AS IdentitySeed,
       IDENT_INCR('PodcastSubscription') AS IdentityIncrement;