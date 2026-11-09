IF DB_ID(N'InBodyConsult') IS NULL
    CREATE DATABASE [InBodyConsult];
GO

USE [InBodyConsult];
GO

CREATE TABLE [dbo].[Products]
(
    [ProductId] INT IDENTITY(1, 1) NOT NULL CONSTRAINT [PK_Products] PRIMARY KEY,
    [Name] NVARCHAR(200) NOT NULL,
    [RegisteredAt] DATE NOT NULL
);
GO

CREATE TABLE [dbo].[Members]
(
    [MemberId] VARCHAR(100) NOT NULL CONSTRAINT [PK_Members] PRIMARY KEY,
    [UserToken] VARCHAR(15) NOT NULL,
    [Name] NVARCHAR(100) NOT NULL,
    [Gender] VARCHAR(1) NOT NULL,
    CONSTRAINT [CK_Members_Gender] CHECK ([Gender] IN ('M', 'F'))
);
GO

CREATE UNIQUE INDEX [UX_Members_UserToken] ON [dbo].[Members] ([UserToken]);
GO

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
GO

CREATE UNIQUE INDEX [UX_InBodyMeasurements_MemberId_MeasuredAt_EquipSerial]
    ON [dbo].[InBodyMeasurements] ([MemberId], [MeasuredAt], [EquipSerial]);
GO

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
GO

CREATE UNIQUE INDEX [UX_ProductMembers_ProductId_MemberId]
    ON [dbo].[ProductMembers] ([ProductId], [MemberId]);
GO

CREATE TABLE [dbo].[InBodyApiUsage]
(
    [AccountKey] CHAR(64) NOT NULL,
    [QuotaDate] DATE NOT NULL,
    [CallsUsed] INT NOT NULL,
    CONSTRAINT [PK_InBodyApiUsage] PRIMARY KEY ([AccountKey], [QuotaDate]),
    CONSTRAINT [CK_InBodyApiUsage_CallsUsed] CHECK ([CallsUsed] >= 0)
);
GO
