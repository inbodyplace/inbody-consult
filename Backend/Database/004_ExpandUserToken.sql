USE [InBodyConsult];
GO

-- Run once on existing databases before accepting international phone numbers.
DROP INDEX [UX_Members_UserToken] ON [dbo].[Members];
GO

ALTER TABLE [dbo].[Members]
    ALTER COLUMN [UserToken] VARCHAR(15) NOT NULL;
GO

CREATE UNIQUE INDEX [UX_Members_UserToken] ON [dbo].[Members] ([UserToken]);
GO
