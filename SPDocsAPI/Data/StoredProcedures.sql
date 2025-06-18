-- ================================================================
-- Stored Procedures for SPDocsAPI
-- These should be created in your Azure SQL Database
-- ================================================================

-- 1. Create the Documents table (if not exists)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Documents' AND xtype='U')
BEGIN
    CREATE TABLE Documents (
        Id int IDENTITY(1,1) PRIMARY KEY,
        Title nvarchar(255) NOT NULL,
        Description nvarchar(1000) NULL,
        DocumentType nvarchar(50) NOT NULL,
        CreatedDate datetime2 NOT NULL DEFAULT GETUTCDATE(),
        ModifiedDate datetime2 NULL,
        CreatedBy nvarchar(100) NOT NULL,
        ModifiedBy nvarchar(100) NULL,
        IsActive bit NOT NULL DEFAULT 1,
        FilePath nvarchar(500) NULL,
        FileSize bigint NULL
    );
END
GO

-- 2. Stored Procedure: Get Documents by Type
CREATE OR ALTER PROCEDURE GetDocumentsByType
    @DocumentType NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, Title, Description, DocumentType, CreatedDate, ModifiedDate,
        CreatedBy, ModifiedBy, IsActive, FilePath, FileSize
    FROM Documents 
    WHERE DocumentType = @DocumentType 
        AND IsActive = 1
    ORDER BY CreatedDate DESC;
END
GO

-- 3. Stored Procedure: Get Documents by User
CREATE OR ALTER PROCEDURE GetDocumentsByUser
    @UserName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id, Title, Description, DocumentType, CreatedDate, ModifiedDate,
        CreatedBy, ModifiedBy, IsActive, FilePath, FileSize
    FROM Documents 
    WHERE (CreatedBy = @UserName OR ModifiedBy = @UserName)
        AND IsActive = 1
    ORDER BY CreatedDate DESC;
END
GO

-- 4. Stored Procedure: Activate/Deactivate Document
CREATE OR ALTER PROCEDURE ActivateDeactivateDocument
    @DocumentId INT,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Documents 
    SET IsActive = @IsActive,
        ModifiedDate = GETUTCDATE()
    WHERE Id = @DocumentId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- 5. Stored Procedure: Get Next Category Code (for GetLessonID API)
CREATE OR ALTER PROCEDURE GetNextCategoryCode
    @Category NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- This is a sample implementation. 
    -- Replace this logic with your actual business requirements
    DECLARE @NextCode NVARCHAR(100);
    
    -- Example logic: Generate next code based on category
    IF @Category = 'Math'
        SET @NextCode = 'MATH-' + FORMAT(GETDATE(), 'yyyyMMdd') + '-' + RIGHT('000' + CAST(ABS(CHECKSUM(NEWID())) % 1000 AS VARCHAR(3)), 3);
    ELSE IF @Category = 'Science'
        SET @NextCode = 'SCI-' + FORMAT(GETDATE(), 'yyyyMMdd') + '-' + RIGHT('000' + CAST(ABS(CHECKSUM(NEWID())) % 1000 AS VARCHAR(3)), 3);
    ELSE IF @Category = 'English'
        SET @NextCode = 'ENG-' + FORMAT(GETDATE(), 'yyyyMMdd') + '-' + RIGHT('000' + CAST(ABS(CHECKSUM(NEWID())) % 1000 AS VARCHAR(3)), 3);
    ELSE
        SET @NextCode = 'GEN-' + FORMAT(GETDATE(), 'yyyyMMdd') + '-' + RIGHT('000' + CAST(ABS(CHECKSUM(NEWID())) % 1000 AS VARCHAR(3)), 3);
    
    -- Return the generated code
    SELECT @NextCode AS NextCategoryCode;
END
GO

-- 6. Sample data insertion (optional)
-- You can run this to add some test data
/*
INSERT INTO Documents (Title, Description, DocumentType, CreatedBy, FilePath, FileSize)
VALUES 
    ('Sample Contract', 'Test contract document', 'Contract', 'admin@company.com', '/documents/contract1.pdf', 1024000),
    ('User Manual', 'Application user manual', 'Manual', 'admin@company.com', '/documents/manual.pdf', 2048000),
    ('Project Proposal', 'New project proposal document', 'Proposal', 'manager@company.com', '/documents/proposal.pdf', 1536000);
*/ 