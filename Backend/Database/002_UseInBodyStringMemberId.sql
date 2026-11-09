USE [InBodyConsult];

-- Switch to InBody string iD as the member primary key only when related tables are empty.
-- If data exists, map existing integer IDs to InBody IDs first.

IF EXISTS (SELECT 1 FROM [dbo].[ProductMembers])
   OR EXISTS (SELECT 1 FROM [dbo].[InBodyMeasurements])
   OR EXISTS (SELECT 1 FROM [dbo].[Members])
BEGIN
    RAISERROR(N'Migration stopped because member or measurement data exists. Map existing IDs to InBody IDs first.', 16, 1);
    RETURN;
END;

DROP TABLE [dbo].[ProductMembers];
DROP TABLE [dbo].[InBodyMeasurements];
DROP TABLE [dbo].[Members];

IF COL_LENGTH('dbo.Products', 'ProductId') IS NULL
   AND COL_LENGTH('dbo.Products', 'Id') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.Products.Id', N'ProductId', N'COLUMN';
END;

CREATE TABLE [dbo].[Members]
(
    [MemberId] VARCHAR(100) NOT NULL CONSTRAINT [PK_Members] PRIMARY KEY,
    [UserToken] VARCHAR(15) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Gender] VARCHAR(1) NOT NULL,
    CONSTRAINT [CK_Members_Gender] CHECK ([Gender] IN ('M', 'F'))
);
CREATE UNIQUE INDEX [UX_Members_UserToken]
    ON [dbo].[Members] ([UserToken]);

CREATE TABLE [dbo].[InBodyMeasurements]
(
    [MeasurementId] BIGINT IDENTITY(1, 1) NOT NULL CONSTRAINT [PK_InBodyMeasurements] PRIMARY KEY,
    [MemberId] VARCHAR(100) NOT NULL,
    [MeasuredAt] DATETIME2(0) NOT NULL,
    [EquipSerial] VARCHAR(50) NOT NULL,
    [Smm] DECIMAL(6, 2) NULL,
    [Bfm] DECIMAL(6, 2) NULL,
    [Weight] DECIMAL(6, 2) NULL,
    [Pbf] DECIMAL(6, 2) NULL,
    CONSTRAINT [FK_InBodyMeasurements_Members_MemberId]
        FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Members] ([MemberId]) ON DELETE CASCADE
);
CREATE UNIQUE INDEX [UX_InBodyMeasurements_MemberId_MeasuredAt_EquipSerial]
    ON [dbo].[InBodyMeasurements] ([MemberId], [MeasuredAt], [EquipSerial]);

CREATE TABLE [dbo].[ProductMembers]
(
    [ProductMemberId] INT IDENTITY(1, 1) NOT NULL CONSTRAINT [PK_ProductMembers] PRIMARY KEY,
    [ProductId] INT NOT NULL,
    [MemberId] VARCHAR(100) NOT NULL,
    [AddedAt] DATETIME2(0) NOT NULL,
    [BeforeMeasurementId] BIGINT NULL,
    [AfterMeasurementId] BIGINT NULL,
    CONSTRAINT [FK_ProductMembers_Products_ProductId]
        FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ProductId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductMembers_Members_MemberId]
        FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Members] ([MemberId]),
    CONSTRAINT [FK_ProductMembers_InBodyMeasurements_BeforeMeasurementId]
        FOREIGN KEY ([BeforeMeasurementId]) REFERENCES [dbo].[InBodyMeasurements] ([MeasurementId]),
    CONSTRAINT [FK_ProductMembers_InBodyMeasurements_AfterMeasurementId]
        FOREIGN KEY ([AfterMeasurementId]) REFERENCES [dbo].[InBodyMeasurements] ([MeasurementId])
);
CREATE UNIQUE INDEX [UX_ProductMembers_ProductId_MemberId]
    ON [dbo].[ProductMembers] ([ProductId], [MemberId]);
