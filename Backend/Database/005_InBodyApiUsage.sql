USE [InBodyConsult];
GO

-- Run once on existing databases before deploying quota-aware API clients.
IF OBJECT_ID(N'dbo.InBodyApiUsage', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[InBodyApiUsage]
    (
        [AccountKey] CHAR(64) NOT NULL,
        [QuotaDate] DATE NOT NULL,
        [CallsUsed] INT NOT NULL,
        CONSTRAINT [PK_InBodyApiUsage] PRIMARY KEY ([AccountKey], [QuotaDate]),
        CONSTRAINT [CK_InBodyApiUsage_CallsUsed] CHECK ([CallsUsed] >= 0)
    );
END;
GO
