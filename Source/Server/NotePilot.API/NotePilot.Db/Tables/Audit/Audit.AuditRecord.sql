CREATE TABLE Audit.AuditRecord
(
    [Id]            UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [UserId]        UNIQUEIDENTIFIER NULL,
    [AuditType]     INT NOT NULL,
    [Table]         NVARCHAR(100) NULL,
    [Field]         NVARCHAR(100) NULL,
    [PrimaryKey]    NVARCHAR(100) NULL,
    [OldValue]      NVARCHAR(1000) NULL,
    [NewValue]      NVARCHAR(1000) NULL,
    
    [DeletedDate]   DATETIME2(7) NULL,
    [CreatedBy]     NVARCHAR(255) NULL,
    [CreatedDate]   DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedBy]     NVARCHAR(255) NULL,
    [UpdatedDate]   DATETIME NULL
);
GO

CREATE NONCLUSTERED INDEX [IX_AuditRecord_UserId]
    ON Audit.AuditRecord(UserId);
GO