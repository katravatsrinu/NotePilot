CREATE TABLE [Auth].[UserAuth]
(
    [Id]              UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

    [UserName]        NVARCHAR(100) NOT NULL,
    [Email]           NVARCHAR(200) NOT NULL,
    [Password]        NVARCHAR(500) NOT NULL,  

    [CreatedBy]       NVARCHAR(255) NOT NULL,
    [CreatedDate]     DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy]       NVARCHAR(255) NULL,
    [UpdatedDate]     DATETIME NULL,
    [DeletedDate]     DATETIME NULL
);
GO

CREATE NONCLUSTERED INDEX [IX_UserAuth_DeletedDate]
ON [Auth].[UserAuth] ([DeletedDate]);
GO