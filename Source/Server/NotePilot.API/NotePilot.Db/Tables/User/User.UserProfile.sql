CREATE TABLE [User].[UserProfile]
(
    [Id]              UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

    [UserAuthId]      UNIQUEIDENTIFIER NOT NULL,
    [FullName]        NVARCHAR(150) NOT NULL,
    [PhoneNumber]     NVARCHAR(20) NULL,

    [DeletedDate]     DATETIME NULL,
    [CreatedBy]       NVARCHAR(255) NULL,
    [CreatedDate]     DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy]       NVARCHAR(255) NULL,
    [UpdatedDate]     DATETIME NULL,

    CONSTRAINT [FK_UserProfile_UserAuth]
        FOREIGN KEY ([UserAuthId]) 
        REFERENCES [Auth].[UserAuth]([Id])
);
GO

CREATE NONCLUSTERED INDEX [IX_UserProfile_DeletedDate]
ON [User].[UserProfile] ([DeletedDate]);
GO
