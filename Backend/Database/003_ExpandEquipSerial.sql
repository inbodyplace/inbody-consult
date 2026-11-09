USE [InBodyConsult];
GO

DROP INDEX [UX_InBodyMeasurements_MemberId_MeasuredAt_EquipSerial]
    ON [dbo].[InBodyMeasurements];
GO

ALTER TABLE [dbo].[InBodyMeasurements]
    ALTER COLUMN [EquipSerial] VARCHAR(50) NOT NULL;
GO

CREATE UNIQUE INDEX [UX_InBodyMeasurements_MemberId_MeasuredAt_EquipSerial]
    ON [dbo].[InBodyMeasurements] ([MemberId], [MeasuredAt], [EquipSerial]);
GO
